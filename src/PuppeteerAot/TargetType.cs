// <copyright file="TargetType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Puppeteer.Helpers.Json;

namespace Puppeteer
{
    /// <summary>
    /// Target type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<TargetType>))]
    public enum TargetType
    {
        /// <summary>
        /// The other.
        /// </summary>
        Other,

        /// <summary>
        /// Target type page.
        /// </summary>
        [EnumMember(Value = "page")]
        Page,

        /// <summary>
        /// Target type service worker.
        /// </summary>
        [EnumMember(Value = "service_worker")]
        ServiceWorker,

        /// <summary>
        /// Target type browser.
        /// </summary>
        [EnumMember(Value = "browser")]
        Browser,

        /// <summary>
        /// Target type background page.
        /// </summary>
        [EnumMember(Value = "background_page")]
        BackgroundPage,

        /// <summary>
        /// Target type worker.
        /// </summary>
        [EnumMember(Value = "worker")]
        Worker,

        /// <summary>
        /// Target type javascript.
        /// </summary>
        [EnumMember(Value = "javascript")]
        Javascript,

        /// <summary>
        /// Target type network.
        /// </summary>
        [EnumMember(Value = "network")]
        Network,

        /// <summary>
        /// Target type deprecation.
        /// </summary>
        [EnumMember(Value = "deprecation")]
        Deprecation,

        /// <summary>
        /// Target type security.
        /// </summary>
        [EnumMember(Value = "security")]
        Security,

        /// <summary>
        /// Target type recommendation.
        /// </summary>
        [EnumMember(Value = "recommendation")]
        Recommendation,

        /// <summary>
        /// Target type shared worker.
        /// </summary>
        [EnumMember(Value = "shared_worker")]
        SharedWorker,

        /// <summary>
        /// Target type iFrame.
        /// </summary>
        [EnumMember(Value = "iframe")]
        IFrame,

        /// <summary>
        /// Target type rendering.
        /// </summary>
        [EnumMember(Value = "rendering")]
        Rendering,

        /// <summary>
        /// Webview.
        /// </summary>
        Webview,

        /// <summary>
        /// Target type tab.
        /// </summary>
        [EnumMember(Value = "tab")]
        Tab,
    }
}
