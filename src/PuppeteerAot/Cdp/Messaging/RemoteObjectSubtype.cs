using Puppeteer.Helpers.Json;
using System.Text.Json.Serialization;

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Remote object subtype.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<RemoteObjectSubtype>))]
    public enum RemoteObjectSubtype
    {
        /// <summary>
        /// Other.
        /// </summary>
        Other,

        /// <summary>
        /// Array.
        /// </summary>
        Array,

        /// <summary>
        /// Null.
        /// </summary>
        Null,

        /// <summary>
        /// Node.
        /// </summary>
        Node,

        /// <summary>
        /// Regexp.
        /// </summary>
        Regexp,

        /// <summary>
        /// Date.
        /// </summary>
        Date,

        /// <summary>
        /// Map.
        /// </summary>
        Map,

        /// <summary>
        /// Set.
        /// </summary>
        Set,

        /// <summary>
        /// Weakmap.
        /// </summary>
        Weakmap,

        /// <summary>
        /// Weakset.
        /// </summary>
        Weakset,

        /// <summary>
        /// Iterator.
        /// </summary>
        Iterator,

        /// <summary>
        /// Generator.
        /// </summary>
        Generator,

        /// <summary>
        /// Error.
        /// </summary>
        Error,

        /// <summary>
        /// Proxy.
        /// </summary>
        Proxy,

        /// <summary>
        /// Promise.
        /// </summary>
        Promise,

        /// <summary>
        /// Typedarray.
        /// </summary>
        Typedarray,

        /// <summary>
        /// Arraybuffer.
        /// </summary>
        Arraybuffer,

        /// <summary>
        /// Dataview.
        /// </summary>
        Dataview,
    }
}
