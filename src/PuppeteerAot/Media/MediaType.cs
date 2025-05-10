using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Puppeteer.Media
{
    /// <summary>
    /// Media type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<MediaType>))]
    public enum MediaType
    {
        /// <summary>
        /// Media Print.
        /// </summary>
        Print,

        /// <summary>
        /// Media Screen.
        /// </summary>
        Screen,

        /// <summary>
        /// No media set.
        /// </summary>
        [EnumMember(Value = "")]
        None,
    }
}
