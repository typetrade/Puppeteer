using Puppeteer.Helpers.Json;
using System.Text.Json.Serialization;

namespace Puppeteer.Cdp.Messaging;

/// <summary>
/// Navigation types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter<NavigationType>))]
public enum NavigationType
{
    /// <summary>
    /// Normal navigation.
    /// </summary>
    Navigation,

    /// <summary>
    /// Back forward cache restore.
    /// </summary>
    BackForwardCacheRestore,
}
