using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Puppeteer.Input;

namespace Puppeteer
{
    /// <inheritdoc cref="Puppeteer.IFrame" />
    public abstract class Frame : IFrame, IEnvironment
    {
        private Task<ElementHandle>? documentTask;

        /// <inheritdoc />
        public event EventHandler FrameSwappedByActivation;

        public event EventHandler FrameDetached;

        public event EventHandler<FrameNavigatedEventArgs> FrameNavigated;

        public event EventHandler FrameNavigatedWithinDocument;

        public event EventHandler LifecycleEvent;

        public event EventHandler FrameSwapped;

        /// <inheritdoc/>
        public abstract IReadOnlyCollection<IFrame> ChildFrames { get; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Url { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public bool Detached { get; private set; }

        /// <inheritdoc/>
        public abstract IPage Page { get; }

        /// <inheritdoc/>
        IFrame IFrame.ParentFrame => ParentFrame;

        /// <inheritdoc/>
        public abstract bool IsOopFrame { get; }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public abstract CDPSession Client { get; protected set; }

        /// <inheritdoc/>
        Realm IEnvironment.MainRealm => MainRealm;

        public string ParentId { get; init; }

        public string LoaderId { get; private set; } = string.Empty;

        public List<string> LifecycleEvents { get; } = new();

        public Realm MainRealm { get; set; }

        public Realm IsolatedRealm { get; set; }

        public IsolatedWorld MainWorld => MainRealm as IsolatedWorld;

        public IsolatedWorld PuppeteerWorld => IsolatedRealm as IsolatedWorld;

        public bool HasStartedLoading { get; private set; }

        /// <summary>
        /// Gets the parent frame of this frame.
        /// </summary>
        public abstract Frame? ParentFrame { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger Logger { get; init; }

        /// <inheritdoc/>
        public abstract Task<IResponse> GoToAsync(string url, NavigationOptions options);

        /// <inheritdoc/>
        public Task<IResponse> GoToAsync(string url, int? timeout = null, WaitUntilNavigation[]? waitUntil = null)
            => GoToAsync(url, new NavigationOptions { Timeout = timeout, WaitUntil = waitUntil });

        /// <inheritdoc/>
        public abstract Task<IResponse> WaitForNavigationAsync(NavigationOptions? options = null);

        /// <inheritdoc/>
        public Task<JsonElement> EvaluateExpressionAsync(string script) => MainRealm.EvaluateExpressionAsync(script);

        /// <inheritdoc/>
        public Task<T> EvaluateExpressionAsync<T>(string script) => MainRealm.EvaluateExpressionAsync<T>(script);

        /// <inheritdoc/>
        public Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args) => MainRealm.EvaluateFunctionAsync(script, args);

        /// <inheritdoc/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args) => MainRealm.EvaluateFunctionAsync<T>(script, args);

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateExpressionHandleAsync(string script) => MainRealm.EvaluateExpressionHandleAsync(script);

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateFunctionHandleAsync(string function, params object[] args) => MainRealm.EvaluateFunctionHandleAsync(function, args);

        /// <inheritdoc/>
        public async Task<IElementHandle> WaitForSelectorAsync(string selector, WaitForSelectorOptions? options = null)
        {
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var (updatedSelector, queryHandler) = Client.Connection.CustomQuerySelectorRegistry.GetQueryHandlerAndSelector(selector);
            return await queryHandler.WaitForAsync(this, null, updatedSelector, options).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<IJSHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options, params object[] args)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainRealm.WaitForFunctionAsync(script, options, args);
        }

        /// <inheritdoc/>
        public Task<IJSHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainRealm.WaitForExpressionAsync(script, options);
        }

        /// <inheritdoc/>
        public async Task<string[]> SelectAsync(string selector, params string[] values)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            return await handle.SelectAsync(values).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IJSHandle> QuerySelectorAllHandleAsync(string selector)
        {
            var document = await GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAllHandleAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IElementHandle> QuerySelectorAsync(string selector)
        {
            var document = await GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IElementHandle[]> QuerySelectorAllAsync(string selector)
        {
            var document = await GetDocumentAsync().ConfigureAwait(false);
            return await document.QuerySelectorAllAsync(selector).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<DeviceRequestPrompt> WaitForDevicePromptAsync(WaitForOptions options = default)
            => GetDeviceRequestPromptManager().WaitForDevicePromptAsync(options);

        /// <inheritdoc/>
        public abstract Task<IElementHandle> AddStyleTagAsync(AddTagOptions options);

        /// <inheritdoc/>
        public abstract Task<IElementHandle> AddScriptTagAsync(AddTagOptions options);

        /// <inheritdoc/>
        public Task<string> GetContentAsync()
            => EvaluateFunctionAsync<string>(@"() => {
                let content = '';
                for (const node of document.childNodes) {
                    switch (node) {
                    case document.documentElement:
                        content += document.documentElement.outerHTML;
                        break;
                    default:
                        content += new XMLSerializer().serializeToString(node);
                        break;
                    }
                }

                return content;
            }");

        /// <inheritdoc/>
        public abstract Task SetContentAsync(string html, NavigationOptions? options = null);

        /// <inheritdoc/>
        public Task<string> GetTitleAsync() => IsolatedRealm.EvaluateExpressionAsync<string>("document.title");

        /// <inheritdoc/>
        public async Task ClickAsync(string selector, ClickOptions? options = null)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.ClickAsync(options).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task HoverAsync(string selector)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.HoverAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task FocusAsync(string selector)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.FocusAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task TapAsync(string selector)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);
            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.TapAsync().ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task TypeAsync(string selector, string text, TypeOptions? options = null)
        {
            var handle = await QuerySelectorAsync(selector).ConfigureAwait(false);

            if (handle == null)
            {
                throw new SelectorException($"No node found for selector: {selector}", selector);
            }

            await handle.TypeAsync(text, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears the document handle.
        /// </summary>
        public void ClearDocumentHandle() => this.documentTask = null;

        /// <summary>
        /// Sets the loading state.
        /// </summary>
        public void OnLoadingStarted() => this.HasStartedLoading = true;

        /// <summary>
        /// Sets the loading state.
        /// </summary>
        public void OnLoadingStopped()
        {
            this.LifecycleEvents.Add("DOMContentLoaded");
            this.LifecycleEvents.Add("load");
            this.LifecycleEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the loader id and clears the lifecycle events.
        /// </summary>
        /// <param name="loaderId"></param>
        /// <param name="name"></param>
        public void OnLifecycleEvent(string loaderId, string name)
        {
            if (name == "init")
            {
                this.LoaderId = loaderId;
                this.LifecycleEvents.Clear();
            }

            this.LifecycleEvents.Add(name);
            this.LifecycleEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the frame name and URL.
        /// </summary>
        /// <param name="framePayload"></param>
        public void Navigated(FramePayload framePayload)
        {
            this.Name = framePayload.Name ?? string.Empty;
            this.Url = framePayload.Url + framePayload.UrlFragment;
        }

        /// <summary>
        /// Sets the frame name and URL.
        /// </summary>
        /// <param name="e"></param>
        public void OnFrameNavigated(FrameNavigatedEventArgs e) => FrameNavigated?.Invoke(this, e);

        /// <summary>
        /// Sets the frame name and URL.
        /// </summary>
        public void OnSwapped() => FrameSwapped?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Sets the frame name and URL.
        /// </summary>
        /// <param name="url"></param>
        public void NavigatedWithinDocument(string url)
        {
            Url = url;
            FrameNavigatedWithinDocument?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Detaches the frame.
        /// </summary>
        public void Detach()
        {
            Detached = true;
            MainWorld.Detach();
            PuppeteerWorld.Detach();
            FrameDetached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the frame name and URL.
        /// </summary>
        public void OnFrameSwappedByActivation()
            => FrameSwappedByActivation?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Gets the frame element.
        /// </summary>
        /// <returns></returns>
        public async Task<ElementHandle?> FrameElementAsync()
        {
            var parentFrame = this.ParentFrame;
            if (parentFrame == null)
            {
                return null;
            }

            var list = await parentFrame.IsolatedRealm.EvaluateFunctionHandleAsync(@"() => {
                return document.querySelectorAll('iframe, frame');
            }").ConfigureAwait(false);

            await foreach (var iframe in list.TransposeIterableHandleAsync())
            {
                var frame = await iframe.ContentFrameAsync().ConfigureAwait(false);
                if (frame?.Id == Id)
                {
                    return iframe as ElementHandle;
                }

                try
                {
                    await iframe.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    this.Logger.LogWarning("FrameElementAsync: Error disposing iframe");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the prompts manager for the current client.
        /// </summary>
        /// <returns>The <see cref="DeviceRequestPromptManager"/>.</returns>
        protected internal abstract DeviceRequestPromptManager GetDeviceRequestPromptManager();

        private Task<ElementHandle> GetDocumentAsync()
        {
            if (this.documentTask != null)
            {
                return this.documentTask;
            }

            async Task<ElementHandle> EvaluateDocumentInContext()
            {
                var document = await IsolatedRealm.EvaluateFunctionHandleAsync("() => document").ConfigureAwait(false);

                if (document is not ElementHandle)
                {
                    throw new PuppeteerException("Document is null");
                }

                return await MainRealm.TransferHandleAsync(document).ConfigureAwait(false) as ElementHandle;
            }

            documentTask = EvaluateDocumentInContext();

            return documentTask;
        }
    }
}
