using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Puppeteer
{
    /// <summary>
    /// Resource type.
    /// </summary>
    /// <seealso cref="IRequest.ResourceType"/>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<ResourceType>))]
    public enum ResourceType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Beacon.
        /// </summary>
        Beacon,

        /// <summary>
        /// Document.
        /// </summary>
        Document,

        /// <summary>
        /// Stylesheet.
        /// </summary>
        [EnumMember(Value = "stylesheet")]
        StyleSheet,

        /// <summary>
        /// Image.
        /// </summary>
        Image,

        /// <summary>
        /// Imageset.
        /// </summary>
        [EnumMember(Value = "imageset")]
        ImageSet,

        /// <summary>
        /// Media.
        /// </summary>
        Media,

        /// <summary>
        /// Font.
        /// </summary>
        Font,

        /// <summary>
        /// Script.
        /// </summary>
        Script,

        /// <summary>
        /// Texttrack.
        /// </summary>
        [EnumMember(Value = "texttrack")]
        TextTrack,

        /// <summary>
        /// XHR.
        /// </summary>
        Xhr,

        /// <summary>
        /// Fetch.
        /// </summary>
        Fetch,

        /// <summary>
        /// Event source.
        /// </summary>
        [EnumMember(Value = "eventsource")]
        EventSource,

        /// <summary>
        /// Web Socket.
        /// </summary>
        [EnumMember(Value = "websocket")]
        WebSocket,

        /// <summary>
        /// Manifest.
        /// </summary>
        Manifest,

        /// <summary>
        /// Ping.
        /// </summary>
        Ping,

        /// <summary>
        /// Image.
        /// </summary>
        Img,

        /// <summary>
        /// Other.
        /// </summary>
        Other,
    }
}
