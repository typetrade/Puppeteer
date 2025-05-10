// <copyright file="IsolatedWorld.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers;
using Puppeteer.Helpers.Json;

namespace Puppeteer
{
    /// <summary>
    /// A LazyArg is an evaluation argument that will be resolved when the CDP call is built.
    /// </summary>
    /// <param name="context">Execution context.</param>
    /// <returns>Resolved argument.</returns>
    public delegate Task<object> LazyArg(ExecutionContext context);

    public class IsolatedWorld : Realm, IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly List<string> _contextBindings = new();
        private readonly TaskQueue _bindingQueue = new();
        private bool _detached;
        private TaskCompletionSource<ExecutionContext> _contextResolveTaskWrapper = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private ExecutionContext? context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsolatedWorld"/> class.
        /// </summary>
        /// <param name="frame">The frame associated with this isolated world.</param>
        /// <param name="worker"></param>
        /// <param name="timeoutSettings"></param>
        /// <param name="isMainWorld"></param>
        public IsolatedWorld(
            Frame frame,
            WebWorker worker,
            TimeoutSettings timeoutSettings,
            bool isMainWorld) : base(timeoutSettings)
        {
            this.Frame = frame;
            this.Worker = worker;
            this.IsMainWorld = isMainWorld;
            this._logger = Client.Connection.LoggerFactory.CreateLogger<IsolatedWorld>();

            this._detached = false;
            this.FrameUpdated();
        }

        /// <summary>
        /// Gets a value indicating whether this property is not upstream. It's helpful for debugging.
        /// </summary>
        public bool IsMainWorld { get; }

        /// <summary>
        /// Gets the frame associated with this isolated world.
        /// </summary>
        public Frame Frame { get; }

        /// <summary>
        /// Gets the client associated with this isolated world.
        /// </summary>
        public CDPSession Client => this.Frame?.Client ?? Worker?.Client;

        /// <summary>
        /// Gets a value indicating whether gets the context associated with this isolated world.
        /// </summary>
        public bool HasContext => this._contextResolveTaskWrapper?.Task.IsCompleted == true;

        /// <summary>
        /// Gets the context associated with this isolated world.
        /// </summary>
        public ConcurrentDictionary<string, Binding> Bindings { get; } = new();

        /// <summary>
        /// Gets the context associated with this isolated world.
        /// </summary>
        public override IEnvironment Environment => (IEnvironment)this.Frame ?? this.Worker;

        /// <summary>
        /// Gets the worker associated with this isolated world.
        /// </summary>
        private WebWorker Worker { get; }

        /// <summary>
        /// Disposes the isolated world.
        /// </summary>
        public void Dispose()
        {
            this._bindingQueue.Dispose();
            this.context?.Dispose();
        }

        /// <summary>
        /// Asynchronously disposes the isolated world.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await this._bindingQueue.DisposeAsync().ConfigureAwait(false);
            if (this.context != null)
            {
                await this.context.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets the frame updated event handler.
        /// </summary>
        public void FrameUpdated() => this.Client.MessageReceived += this.Client_MessageReceived;

        /// <summary>
        /// Adds a binding to the context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task AddBindingToContextAsync(ExecutionContext context, string name)
        {
            // Previous operation added the binding so we are done.
            if (this._contextBindings.Contains(name))
            {
                return;
            }

            await this._bindingQueue.Enqueue(async () =>
            {
                var expression = BindingUtils.PageBindingInitString("internal", name);
                try
                {
                    // TODO: In theory, it would be enough to call this just once
                    await context.Client.SendAsync(
                        "Runtime.addBinding",
                        new RuntimeAddBindingRequest
                        {
                            Name = name,
                            ExecutionContextName = !string.IsNullOrEmpty(this.context.ContextName) ? this.context.ContextName : null,
                            ExecutionContextId = string.IsNullOrEmpty(context.ContextName) ? context.ContextId : null,
                        }).ConfigureAwait(false);

                    await context.EvaluateExpressionAsync(expression).ConfigureAwait(false);
                    _contextBindings.Add(name);
                }
                catch (Exception ex)
                {
                    var ctxDestroyed = ex.Message.Contains("Execution context was destroyed");
                    var ctxNotFound = ex.Message.Contains("Cannot find context with specified id");
                    if (ctxDestroyed || ctxNotFound)
                    {
                        return;
                    }

                    _logger.LogError(ex.ToString());
                }
            }).ConfigureAwait(false);
        }

        public override async Task<IElementHandle> AdoptBackendNodeAsync(object backendNodeId)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            var obj = await Client.SendAsync<DomResolveNodeResponse>("DOM.resolveNode", new DomResolveNodeRequest
            {
                BackendNodeId = backendNodeId,
                ExecutionContextId = context.ContextId,
            }).ConfigureAwait(false);

            return context.CreateJSHandle(obj.Object) as IElementHandle;
        }

        public override async Task<IJSHandle> TransferHandleAsync(IJSHandle handle)
        {
            if ((handle as JSHandle)?.Realm == this)
            {
                return handle;
            }

            if (handle.RemoteObject.ObjectId == null)
            {
                return handle;
            }

            var info = await Client.SendAsync<DomDescribeNodeResponse>(
                "DOM.describeNode",
                new DomDescribeNodeRequest
                {
                    ObjectId = handle.RemoteObject.ObjectId,
                }).ConfigureAwait(false);

            var newHandle = await AdoptBackendNodeAsync(info.Node.BackendNodeId).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
            return newHandle;
        }

        public override async Task<IJSHandle> AdoptHandleAsync(IJSHandle handle)
        {
            if ((handle as JSHandle)?.Realm == this)
            {
                return handle;
            }

            var nodeInfo = await Client.SendAsync<DomDescribeNodeResponse>("DOM.describeNode", new DomDescribeNodeRequest
            {
                ObjectId = ((JSHandle)handle).RemoteObject.ObjectId,
            }).ConfigureAwait(false);
            return await AdoptBackendNodeAsync(nodeInfo.Node.BackendNodeId).ConfigureAwait(false);
        }

        public void Detach()
        {
            _detached = true;
            Client.MessageReceived -= Client_MessageReceived;
            TaskManager.TerminateAll(new Exception("waitForFunction failed: frame got detached."));
        }

        public Task<ExecutionContext> GetExecutionContextAsync()
        {
            if (_detached)
            {
                throw new PuppeteerException($"Execution Context is not available in detached frame \"{Frame.Url}\" (are you trying to evaluate?)");
            }

            return _contextResolveTaskWrapper.Task;
        }

        public override async Task<IJSHandle> EvaluateExpressionHandleAsync(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionHandleAsync(script).ConfigureAwait(false);
        }

        public override async Task<IJSHandle> EvaluateFunctionHandleAsync(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionHandleAsync(script, args).ConfigureAwait(false);
        }

        public override async Task<T> EvaluateExpressionAsync<T>(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionAsync<T>(script).ConfigureAwait(false);
        }

        public override async Task<JsonElement> EvaluateExpressionAsync(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateExpressionAsync(script).ConfigureAwait(false);
        }

        public override async Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionAsync<T>(script, args).ConfigureAwait(false);
        }

        public override async Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(false);
            return await context.EvaluateFunctionAsync(script, args).ConfigureAwait(false);
        }

        public void ClearContext()
        {
            _contextResolveTaskWrapper.TrySetException(new PuppeteerException("Execution Context was destroyed"));
            _contextResolveTaskWrapper = new TaskCompletionSource<ExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.context?.Dispose();
            this.context = null;
            Frame?.ClearDocumentHandle();
        }

        public void SetNewContext(
            CDPSession client,
            ContextPayload contextPayload,
            IsolatedWorld world)
        {
            this.context = new ExecutionContext(
                client,
                contextPayload,
                world);

            this.SetContext(this.context);
        }

        /// <summary>
        /// Sets the context for the isolated world.
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetContext(ExecutionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _contextBindings.Clear();
            _contextResolveTaskWrapper.TrySetResult(context);
            TaskManager.RerunAll();
        }

        private async void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.MessageID)
                {
                    case "Runtime.bindingCalled":
                        await OnBindingCalledAsync(e.MessageData.ToObject<BindingCalledResponse>(true)).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"IsolatedWorld failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
                _logger.LogError(ex, message);
                Client.Close(message);
            }
        }

        private async Task OnBindingCalledAsync(BindingCalledResponse e)
        {
            var payload = e.BindingPayload;

            if (payload.Type != "internal")
            {
                return;
            }

            if (!_contextBindings.Contains(payload.Name))
            {
                return;
            }

            try
            {
                var context = await GetExecutionContextAsync().ConfigureAwait(false);

                if (e.ExecutionContextId != context.ContextId)
                {
                    return;
                }

                if (Bindings.TryGetValue(payload.Name, out var binding))
                {
                    await binding.RunAsync(context, payload.Seq, payload.Args.Cast<object>().ToArray(), payload.IsTrivial).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Protocol error"))
                {
                    return;
                }

                _logger.LogError(ex.ToString());
            }
        }
    }
}
