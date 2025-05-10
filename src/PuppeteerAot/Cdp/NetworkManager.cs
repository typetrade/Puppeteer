using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers;
using Puppeteer.Helpers.Json;

namespace Puppeteer.Cdp
{
    public class NetworkManager
    {
        private readonly bool _ignoreHTTPSErrors;
        private readonly NetworkEventManager _networkEventManager = new();
        private readonly ILogger _logger;
        private readonly ConcurrentSet<string> _attemptedAuthentications = [];
        private readonly ConcurrentDictionary<ICDPSession, DisposableActionsStack> _clients = new();
        private readonly IFrameProvider _frameManager;
        private readonly ILoggerFactory _loggerFactory;

        private InternalNetworkConditions? emulatedNetworkConditions;
        private Dictionary<string, string> _extraHTTPHeaders;
        private Credentials _credentials;
        private bool _userRequestInterceptionEnabled;
        private bool _protocolRequestInterceptionEnabled;
        private bool? _userCacheDisabled;
        private string _userAgent;
        private UserAgentMetadata? userAgentMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkManager"/> class.
        /// </summary>
        /// <param name="ignoreHTTPSErrors">If set to <c>true</c> ignore http errors.</param>
        /// <param name="frameManager">Frame manager.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public NetworkManager(bool ignoreHTTPSErrors, IFrameProvider frameManager, ILoggerFactory loggerFactory)
        {
            _frameManager = frameManager;
            _ignoreHTTPSErrors = ignoreHTTPSErrors;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NetworkManager>();
        }

        public event EventHandler<ResponseCreatedEventArgs> Response;

        public event EventHandler<RequestEventArgs> Request;

        public event EventHandler<RequestEventArgs> RequestFinished;

        public event EventHandler<RequestEventArgs> RequestFailed;

        public event EventHandler<RequestEventArgs> RequestServedFromCache;

        public Dictionary<string, string> ExtraHTTPHeaders => _extraHTTPHeaders?.Clone();

        public int NumRequestsInProgress => _networkEventManager.NumRequestsInProgress;

        public Task AddClientAsync(ICDPSession client)
        {
            if (_clients.ContainsKey(client))
            {
                return Task.CompletedTask;
            }

            var subscriptions = new DisposableActionsStack();
            _clients[client] = subscriptions;
            client.MessageReceived += Client_MessageReceived;
            subscriptions.Defer(() => client.MessageReceived -= Client_MessageReceived);

            return Task.WhenAll(
                _ignoreHTTPSErrors ? client.SendAsync("Security.setIgnoreCertificateErrors", new SecuritySetIgnoreCertificateErrorsRequest { Ignore = true }) : Task.CompletedTask,
                client.SendAsync("Network.enable"),
                ApplyExtraHTTPHeadersAsync(client),
                ApplyNetworkConditionsAsync(client),
                ApplyProtocolCacheDisabledAsync(client),
                ApplyProtocolRequestInterceptionAsync(client),
                ApplyUserAgentAsync(client));
        }

        public void RemoveClient(CDPSession client)
        {
            if (!_clients.TryRemove(client, out var subscriptions))
            {
                return;
            }

            subscriptions.Dispose();
        }

        public async Task AuthenticateAsync(Credentials credentials)
        {
            _credentials = credentials;
            var enabled = _userRequestInterceptionEnabled || _credentials != null;

            if (enabled == _protocolRequestInterceptionEnabled)
            {
                return;
            }

            _protocolRequestInterceptionEnabled = enabled;
            await ApplyToAllClientsAsync(ApplyProtocolRequestInterceptionAsync).ConfigureAwait(false);
        }

        public Task SetExtraHTTPHeadersAsync(Dictionary<string, string> extraHTTPHeaders)
        {
            _extraHTTPHeaders = [];

            foreach (var item in extraHTTPHeaders)
            {
                _extraHTTPHeaders[item.Key.ToLower(CultureInfo.CurrentCulture)] = item.Value;
            }

            return ApplyToAllClientsAsync(ApplyExtraHTTPHeadersAsync);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userAgent"></param>
        /// <param name="userAgentMetadata"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task SetUserAgentAsync(string userAgent, UserAgentMetadata? userAgentMetadata)
        {
            this._userAgent = userAgent;
            this.userAgentMetadata = userAgentMetadata;
            return this.ApplyToAllClientsAsync(this.ApplyUserAgentAsync);
        }

        public Task SetCacheEnabledAsync(bool enabled)
        {
            _userCacheDisabled = !enabled;
            return ApplyToAllClientsAsync(ApplyProtocolCacheDisabledAsync);
        }

        public async Task SetRequestInterceptionAsync(bool value)
        {
            _userRequestInterceptionEnabled = value;
            var enabled = _userRequestInterceptionEnabled || _credentials != null;

            if (enabled == _protocolRequestInterceptionEnabled)
            {
                return;
            }

            _protocolRequestInterceptionEnabled = enabled;
            await ApplyToAllClientsAsync(ApplyProtocolRequestInterceptionAsync).ConfigureAwait(false);
        }

        public Task SetOfflineModeAsync(bool value)
        {
            emulatedNetworkConditions ??= new InternalNetworkConditions();
            emulatedNetworkConditions.Offline = value;
            return ApplyToAllClientsAsync(ApplyNetworkConditionsAsync);
        }

        public Task EmulateNetworkConditionsAsync(NetworkConditions networkConditions)
        {
            emulatedNetworkConditions ??= new InternalNetworkConditions();
            emulatedNetworkConditions.Upload = networkConditions?.Upload ?? -1;
            emulatedNetworkConditions.Download = networkConditions?.Download ?? -1;
            emulatedNetworkConditions.Latency = networkConditions?.Latency ?? 0;
            return ApplyToAllClientsAsync(ApplyNetworkConditionsAsync);
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var client = sender as CDPSession;
                switch (e.MessageID)
                {
                    case "Fetch.requestPaused":
                        await OnRequestPausedAsync(client, e.MessageData.ToObject<FetchRequestPausedResponse>(true)).ConfigureAwait(false);
                        break;
                    case "Fetch.authRequired":
                        await OnAuthRequiredAsync(client, e.MessageData.ToObject<FetchAuthRequiredResponse>(true)).ConfigureAwait(false);
                        break;
                    case "Network.requestWillBeSent":
                        await OnRequestWillBeSentAsync(client, e.MessageData.ToObject<RequestWillBeSentPayload>(true)).ConfigureAwait(false);
                        break;
                    case "Network.requestServedFromCache":
                        OnRequestServedFromCache(e.MessageData.ToObject<RequestServedFromCacheResponse>(true));
                        break;
                    case "Network.responseReceived":
                        OnResponseReceived(client, e.MessageData.ToObject<ResponseReceivedResponse>(true));
                        break;
                    case "Network.loadingFinished":
                        OnLoadingFinished(e.MessageData.ToObject<LoadingFinishedEventResponse>(true));
                        break;
                    case "Network.loadingFailed":
                        OnLoadingFailed(e.MessageData.ToObject<LoadingFailedEventResponse>(true));
                        break;
                    case "Network.responseReceivedExtraInfo":
                        await OnResponseReceivedExtraInfoAsync(sender as CDPSession, e.MessageData.ToObject<ResponseReceivedExtraInfoResponse>(true)).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"NetworkManager failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                _logger.LogError(ex, message);
                _ = ApplyToAllClientsAsync(client =>
                {
                    (client as CDPSession)?.Close(message);
                    return Task.CompletedTask;
                });
            }
        }

        private async Task OnResponseReceivedExtraInfoAsync(CDPSession client, ResponseReceivedExtraInfoResponse e)
        {
            var redirectInfo = _networkEventManager.TakeQueuedRedirectInfo(e.RequestId);

            if (redirectInfo != null)
            {
                this._networkEventManager.ResponseExtraInfo(e.RequestId).Add(e);
                await OnRequestAsync(client, redirectInfo.Event, redirectInfo.FetchRequestId).ConfigureAwait(false);
                return;
            }

            // We may have skipped response and loading events because we didn't have
            // this ExtraInfo event yet. If so, emit those events now.
            var queuedEvents = this._networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                this._networkEventManager.ForgetQueuedEventGroup(e.RequestId);
                this.EmitResponseEvent(client, queuedEvents.ResponseReceivedEvent, e);

                if (queuedEvents.LoadingFinishedEvent != null)
                {
                    this.EmitLoadingFinished(queuedEvents.LoadingFinishedEvent);
                }

                if (queuedEvents.LoadingFailedEvent != null)
                {
                    this.EmitLoadingFailed(queuedEvents.LoadingFailedEvent);
                }

                return;
            }

            // Wait until we get another event that can use this ExtraInfo event.
            this._networkEventManager.ResponseExtraInfo(e.RequestId).Add(e);
        }

        private void OnLoadingFailed(LoadingFailedEventResponse e)
        {
            var queuedEvents = this._networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                queuedEvents.LoadingFailedEvent = e;
            }
            else
            {
                EmitLoadingFailed(e);
            }
        }

        private void EmitLoadingFailed(LoadingFailedEventResponse e)
        {
            var request = this._networkEventManager.GetRequest(e.RequestId);
            if (request == null)
            {
                return;
            }

            request.FailureText = e.ErrorText;
            request.Response?.BodyLoadedTaskWrapper.TrySetResult(true);

            ForgetRequest(request, true);

            RequestFailed?.Invoke(this, new RequestEventArgs(request));
        }

        private void OnLoadingFinished(LoadingFinishedEventResponse e)
        {
            var queuedEvents = this._networkEventManager.GetQueuedEventGroup(e.RequestId);

            if (queuedEvents != null)
            {
                queuedEvents.LoadingFinishedEvent = e;
            }
            else
            {
                EmitLoadingFinished(e);
            }
        }

        private void EmitLoadingFinished(LoadingFinishedEventResponse e)
        {
            var request = this._networkEventManager.GetRequest(e.RequestId);
            if (request == null)
            {
                return;
            }

            request.Response?.BodyLoadedTaskWrapper.TrySetResult(true);

            ForgetRequest(request, true);
            RequestFinished?.Invoke(this, new RequestEventArgs(request));
        }

        private void ForgetRequest(CdpHttpRequest request, bool events)
        {
            this._networkEventManager.ForgetRequest(request.Id);

            if (request.InterceptionId != null)
            {
                _attemptedAuthentications.Remove(request.InterceptionId);
            }

            if (events)
            {
                this._networkEventManager.Forget(request.Id);
            }
        }

        private void OnResponseReceived(CDPSession client, ResponseReceivedResponse e)
        {
            var request = this._networkEventManager.GetRequest(e.RequestId);
            ResponseReceivedExtraInfoResponse? extraInfo = null;

            if (request is { FromMemoryCache: false } && e.HasExtraInfo)
            {
                extraInfo = this._networkEventManager.ShiftResponseExtraInfo(e.RequestId);

                if (extraInfo == null)
                {
                    this._networkEventManager.QueuedEventGroup(e.RequestId, new()
                    {
                        ResponseReceivedEvent = e,
                    });
                    return;
                }
            }

            this.EmitResponseEvent(client, e, extraInfo);
        }

        /// <summary>
        /// Emits the response event.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <param name="extraInfo"></param>
        private void EmitResponseEvent(CDPSession client, ResponseReceivedResponse e, ResponseReceivedExtraInfoResponse? extraInfo)
        {
            var request = this._networkEventManager.GetRequest(e.RequestId);

            // FileUpload sends a response without a matching request.
            if (request == null)
            {
                return;
            }

            if (e.Response.FromDiskCache)
            {
                extraInfo = null;
            }

            var response = new CdpHttpResponse(
                client,
                request,
                e.Response,
                extraInfo);

            request.Response = response;

            this.Response?.Invoke(this, new ResponseCreatedEventArgs(response));
        }

        private async Task OnAuthRequiredAsync(CDPSession client, FetchAuthRequiredResponse e)
        {
            var response = "Default";
            if (this._attemptedAuthentications.Contains(e.RequestId))
            {
                response = "CancelAuth";
            }
            else if (_credentials != null)
            {
                response = "ProvideCredentials";
                _attemptedAuthentications.Add(e.RequestId);
            }

            var credentials = _credentials ?? new Credentials();
            try
            {
                await client.SendAsync("Fetch.continueWithAuth", new ContinueWithAuthRequest
                {
                    RequestId = e.RequestId,
                    AuthChallengeResponse = new ContinueWithAuthRequestChallengeResponse
                    {
                        Response = response,
                        Username = credentials.Username,
                        Password = credentials.Password,
                    },
                }).ConfigureAwait(false);
            }
            catch (PuppeteerException ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private async Task OnRequestPausedAsync(CDPSession client, FetchRequestPausedResponse e)
        {
            if (!_userRequestInterceptionEnabled && _protocolRequestInterceptionEnabled)
            {
                try
                {
                    await client.SendAsync("Fetch.continueRequest", new FetchContinueRequestRequest
                    {
                        RequestId = e.RequestId,
                    }).ConfigureAwait(false);
                }
                catch (PuppeteerException ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            if (string.IsNullOrEmpty(e.NetworkId))
            {
                OnRequestWithoutNetworkInstrumentationAsync(client, e);
                return;
            }

            var requestWillBeSentEvent = this._networkEventManager.GetRequestWillBeSent(e.NetworkId);

            // redirect requests have the same `requestId`,
            if (
                requestWillBeSentEvent != null &&
                (requestWillBeSentEvent.Request.Url != e.Request.Url ||
                requestWillBeSentEvent.Request.Method != e.Request.Method))
            {
                this._networkEventManager.ForgetRequestWillBeSent(e.NetworkId);
                requestWillBeSentEvent = null;
            }

            if (requestWillBeSentEvent != null)
            {
                this.PatchRequestEventHeaders(requestWillBeSentEvent, e);
                await this.OnRequestAsync(client, requestWillBeSentEvent, e.RequestId).ConfigureAwait(false);
            }
            else
            {
                this._networkEventManager.StoreRequestPaused(e.NetworkId, e);
            }
        }

        private async void OnRequestWithoutNetworkInstrumentationAsync(CDPSession client, FetchRequestPausedResponse e)
        {
            // If an event has no networkId it should not have any network events. We
            // still want to dispatch it for the interception by the user.
            var frame = !string.IsNullOrEmpty(e.FrameId)
                ? await _frameManager.GetFrameAsync(e.FrameId).ConfigureAwait(false)
                : null;

            var request = new CdpHttpRequest(
                    client,
                    frame,
                    e.RequestId,
                    _userRequestInterceptionEnabled,
                    e,
                    [],
                    _loggerFactory);

            Request?.Invoke(this, new RequestEventArgs(request));
            _ = request.FinalizeInterceptionsAsync();
        }

        private async Task OnRequestAsync(CDPSession client, RequestWillBeSentPayload e, string fetchRequestId)
        {
            CdpHttpRequest request;
            var redirectChain = new List<IRequest>();
            if (e.RedirectResponse != null)
            {
                ResponseReceivedExtraInfoResponse? redirectResponseExtraInfo = null;
                if (e.RedirectHasExtraInfo)
                {
                    redirectResponseExtraInfo = this._networkEventManager.ShiftResponseExtraInfo(e.RequestId);
                    if (redirectResponseExtraInfo == null)
                    {
                        this._networkEventManager.QueueRedirectInfo(e.RequestId, new()
                        {
                            Event = e,
                            FetchRequestId = fetchRequestId,
                        });
                        return;
                    }
                }

                request = this._networkEventManager.GetRequest(e.RequestId);

                // If we connect late to the target, we could have missed the requestWillBeSent event.
                if (request != null)
                {
                    HandleRequestRedirect(client, request, e.RedirectResponse, redirectResponseExtraInfo);
                    redirectChain = request.RedirectChainList;
                }
            }

            var frame = !string.IsNullOrEmpty(e.FrameId) ? await _frameManager.GetFrameAsync(e.FrameId).ConfigureAwait(false) : null;

            request = new CdpHttpRequest(
                client,
                frame,
                fetchRequestId,
                _userRequestInterceptionEnabled,
                e,
                redirectChain,
                _loggerFactory);

            this._networkEventManager.StoreRequest(e.RequestId, request);

            Request?.Invoke(this, new RequestEventArgs(request));

            try
            {
                await request.FinalizeInterceptionsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to FinalizeInterceptionsAsync");
            }
        }

        private void OnRequestServedFromCache(RequestServedFromCacheResponse response)
        {
            var request = this._networkEventManager.GetRequest(response.RequestId);

            if (request != null)
            {
                request.FromMemoryCache = true;
            }

            RequestServedFromCache?.Invoke(this, new RequestEventArgs(request));
        }

        private void HandleRequestRedirect(CDPSession client, CdpHttpRequest request, ResponsePayload responseMessage, ResponseReceivedExtraInfoResponse extraInfo)
        {
            var response = new CdpHttpResponse(
                client,
                request,
                responseMessage,
                extraInfo);

            request.Response = response;
            request.RedirectChainList.Add(request);
            response.BodyLoadedTaskWrapper.TrySetException(
                new PuppeteerException("Response body is unavailable for redirect responses"));

            ForgetRequest(request, false);

            Response?.Invoke(this, new ResponseCreatedEventArgs(response));
            RequestFinished?.Invoke(this, new RequestEventArgs(request));
        }

        private async Task OnRequestWillBeSentAsync(CDPSession client, RequestWillBeSentPayload e)
        {
            // Request interception doesn't happen for data URLs with Network Service.
            if (_userRequestInterceptionEnabled && !e.Request.Url.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
            {
                this._networkEventManager.StoreRequestWillBeSent(e.RequestId, e);

                var requestPausedEvent = this._networkEventManager.GetRequestPaused(e.RequestId);
                if (requestPausedEvent != null)
                {
                    var fetchRequestId = requestPausedEvent.RequestId;
                    PatchRequestEventHeaders(e, requestPausedEvent);
                    await OnRequestAsync(client, e, fetchRequestId).ConfigureAwait(false);
                    this._networkEventManager.ForgetRequestPaused(e.RequestId);
                }

                return;
            }

            await OnRequestAsync(client, e, null).ConfigureAwait(false);
        }

        private void PatchRequestEventHeaders(RequestWillBeSentPayload requestWillBeSentEvent, FetchRequestPausedResponse requestPausedEvent)
        {
            foreach (var kv in requestPausedEvent.Request.Headers)
            {
                requestWillBeSentEvent.Request.Headers[kv.Key] = kv.Value;
            }
        }

        private async Task ApplyUserAgentAsync(ICDPSession client)
        {
            if (_userAgent == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setUserAgentOverride",
                new NetworkSetUserAgentOverrideRequest
                {
                    UserAgent = this._userAgent,
                    UserAgentMetadata = this.userAgentMetadata,
                }).ConfigureAwait(false);
        }

        private async Task ApplyProtocolRequestInterceptionAsync(ICDPSession client)
        {
            this._userCacheDisabled ??= false;

            if (this._protocolRequestInterceptionEnabled)
            {
                await Task.WhenAll(
                    this.ApplyProtocolCacheDisabledAsync(client),
                    client.SendAsync(
                        "Fetch.enable",
                        new FetchEnableRequest
                        {
                            HandleAuthRequests = true,
                            Patterns = [new FetchEnableRequest.Pattern("*")],
                        })).ConfigureAwait(false);
            }
            else
            {
                await Task.WhenAll(
                    this.ApplyProtocolCacheDisabledAsync(client),
                    client.SendAsync("Fetch.disable")).ConfigureAwait(false);
            }
        }

        private async Task ApplyProtocolCacheDisabledAsync(ICDPSession client)
        {
            if (_userCacheDisabled == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setCacheDisabled",
                new NetworkSetCacheDisabledRequest(this._userCacheDisabled.Value)).ConfigureAwait(false);
        }

        private async Task ApplyNetworkConditionsAsync(ICDPSession client)
        {
            if (this.emulatedNetworkConditions == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.emulateNetworkConditions",
                new NetworkEmulateNetworkConditionsRequest
                {
                    Offline = emulatedNetworkConditions.Offline,
                    Latency = emulatedNetworkConditions.Latency,
                    UploadThroughput = emulatedNetworkConditions.Upload,
                    DownloadThroughput = emulatedNetworkConditions.Download,
                }).ConfigureAwait(false);
        }

        private async Task ApplyExtraHTTPHeadersAsync(ICDPSession client)
        {
            if (_extraHTTPHeaders == null)
            {
                return;
            }

            await client.SendAsync(
                "Network.setExtraHTTPHeaders",
                new NetworkSetExtraHTTPHeadersRequest(_extraHTTPHeaders)).ConfigureAwait(false);
        }

        private Task ApplyToAllClientsAsync(Func<ICDPSession, Task> func)
            => Task.WhenAll(_clients.Keys.Select(func));
    }
}
