using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers.Json;

namespace Puppeteer
{
    /// <inheritdoc/>
    [JsonConverter(typeof(JSHandleMethodConverter))]
    public abstract class JSHandle : IJSHandle
    {
        public JSHandle(IsolatedWorld world, RemoteObject remoteObject)
        {
            Realm = world;
            RemoteObject = remoteObject;
        }

        /// <inheritdoc/>
        public bool Disposed { get; protected set; }

        /// <inheritdoc/>
        public RemoteObject RemoteObject { get; }

        public Func<Task> DisposeAction { get; set; }

        public IsolatedWorld Realm { get; }

        public Frame Frame => Realm.Environment as Frame;

        public string Id => RemoteObject.ObjectId;

        /// <inheritdoc/>
        public virtual Task<IJSHandle> GetPropertyAsync(string propertyName)
            => EvaluateFunctionHandleAsync(
                @"(object, propertyName) => {
                        return object[propertyName];
                    }",
                propertyName);

        /// <inheritdoc/>
        public virtual async Task<Dictionary<string, IJSHandle>> GetPropertiesAsync()
        {
            var propertyNames = await EvaluateFunctionAsync<string[]>(@"object => {
                    const enumerableProperties = [];
                    const descriptors = Object.getOwnPropertyDescriptors(object);
                    for (const propertyName in descriptors) {
                        if (descriptors[propertyName]?.enumerable)
                        {
                            enumerableProperties.push(propertyName);
                        }
                    }
                    return enumerableProperties;
                }").ConfigureAwait(false);

            var dic = new Dictionary<string, IJSHandle>();

            foreach (var key in propertyNames)
            {
                var handleItem = await GetPropertyAsync(key).ConfigureAwait(false);
                if (handleItem is not null)
                {
                    dic.Add(key, handleItem);
                }
            }

            return dic;
        }

        /// <inheritdoc/>
        public async Task<object> JsonValueAsync() => await JsonValueAsync<object>().ConfigureAwait(false);

        /// <inheritdoc/>
        public abstract Task<T> JsonValueAsync<T>();

        /// <inheritdoc/>
        public abstract ValueTask DisposeAsync();

        /// <inheritdoc/>
        public Task<IJSHandle> EvaluateFunctionHandleAsync(string pageFunction, params object[] args)
        {
            var list = new List<object>(args);
            list.Insert(0, this);
            return Realm.EvaluateFunctionHandleAsync(pageFunction, list.ToArray());
        }

        /// <inheritdoc/>
        public async Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args)
        {
            var list = new List<object>(args);
            var adoptedThis = await Realm.AdoptHandleAsync(this).ConfigureAwait(false);
            list.Insert(0, adoptedThis);
            return await Realm.EvaluateFunctionAsync<JsonElement>(script, list.ToArray())
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            var list = new List<object>(args);
            list.Insert(0, this);
            return Realm.EvaluateFunctionAsync<T>(script, list.ToArray());
        }
    }
}
