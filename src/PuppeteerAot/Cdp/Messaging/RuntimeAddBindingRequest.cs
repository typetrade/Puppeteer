namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Request to add a binding to the runtime.
    /// </summary>
    public class RuntimeAddBindingRequest
    {
        /// <summary>
        /// The name of the function to be bound. The name must not collide with any of the JavaScript native properties (e.g. window, document, etc.).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the function to be bound. The function must be serializable and should not have any side effects.
        /// </summary>
        public string? ExecutionContextName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the execution context to bind the function to. If not specified, the function will be bound to all contexts.
        /// </summary>
        public int? ExecutionContextId { get; set; }
    }
}
