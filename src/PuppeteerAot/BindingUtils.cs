using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers.Json;

namespace Puppeteer
{
    public static class BindingUtils
    {
        public static string PageBindingInitString(string type, string name)
            => EvaluationString(
                @"function addPageBinding(type, name) {
                    // This is the CDP binding.
                    const callCdp = globalThis[name];

                    // Depending on the frame loading state either Runtime.evaluate or
                    // Page.addScriptToEvaluateOnNewDocument might succeed. Let's check that we
                    // don't re-wrap Puppeteer's binding.
                    if (callCdp[Symbol.toStringTag] === 'PuppeteerBinding') {
                      return;
                    }

                    // We replace the CDP binding with a Puppeteer binding.
                    Object.assign(globalThis, {
                    [name](...args) {
                      // This is the Puppeteer binding.
                      const callPuppeteer = globalThis[name];
                      callPuppeteer.args ??= new Map();
                      callPuppeteer.callbacks ??= new Map();

                      const seq = (callPuppeteer.lastSeq ?? 0) + 1;
                      callPuppeteer.lastSeq = seq;
                      callPuppeteer.args.set(seq, args);

                      callCdp(
                        JSON.stringify({
                            type,
                            name,
                            seq,
                            args,
                            isTrivial: !args.some(value => {
                            return value instanceof Node;
                            }),
                        })
                      );

                      return new Promise((resolve, reject) => {
                        callPuppeteer.callbacks.set(seq, {
                          resolve(value) {
                            callPuppeteer.args.delete(seq);
                            resolve(value);
                          },
                          reject(value) {
                            callPuppeteer.args.delete(seq);
                            reject(value);
                          },
                        });
                      });
                    },
                  });
                  globalThis[name][Symbol.toStringTag] = 'PuppeteerBinding';
                }",
                type,
                name);

        public static string EvaluationString(string fun, params object[] args)
        {
            return $"({fun})({string.Join(",", args.Select(SerializeArgument))})";

            static string SerializeArgument(object? arg)
            {
                if (arg == null)
                {
                    return "undefined";
                }

                var json = JsonSerializer.Serialize(arg, JsonHelper.DefaultJsonSerializerSettings);
                return json;
            }
        }

        /// <summary>
        /// Executes a binding asynchronously within the specified execution context.
        /// </summary>
        /// <param name="context">The execution context in which the binding is executed.</param>
        /// <param name="e">The binding call response containing the binding payload.</param>
        /// <param name="pageBindings">A dictionary of page bindings available for execution.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task ExecuteBindingAsync(ExecutionContext context, BindingCalledResponse e, ConcurrentDictionary<string, Binding> pageBindings)
        {
            var binding = pageBindings[e.BindingPayload.Name];
            var methodParams = binding.Function.Method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

            var args = e.BindingPayload.Args.Select((token, i) => token.ToObject(methodParams[i])).Cast<object>().ToArray();

            await binding.RunAsync(context, e.BindingPayload.Seq, args, e.BindingPayload.IsTrivial).ConfigureAwait(false);
        }

        public static async Task<object> ExecuteBindingAsync(Delegate binding, object[] rawArgs)
        {
            const string taskResultPropertyName = "Result";
            object result;
            var methodParams = binding.Method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

            var args = rawArgs.Select((arg, i) => arg is JsonElement token ? token.ToObject(methodParams[i]) : arg).ToArray();

            result = binding.DynamicInvoke(args);
            if (result is Task taskResult)
            {
                await taskResult.ConfigureAwait(false);

                if (taskResult.GetType().IsGenericType)
                {
                    // the task is already awaited and therefore the call to property Result will not deadlock
                    result = taskResult.GetType().GetProperty(taskResultPropertyName).GetValue(taskResult);
                }
            }

            return result;
        }
    }
}
