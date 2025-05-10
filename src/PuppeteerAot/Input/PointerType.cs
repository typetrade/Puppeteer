// <copyright file="PointerType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Puppeteer.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Puppeteer.Input
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<PointerType>))]
    public enum PointerType
    {
        /// <summary>
        /// Mouse.
        /// </summary>
        [EnumMember(Value = "mouse")]
        Mouse,

        /// <summary>
        /// Pen.
        /// </summary>
        [EnumMember(Value = "pen")]
        Pen,
    }
}
