// <copyright file="VisionDeficiency.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Puppeteer.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Puppeteer
{
    /// <summary>
    /// Types of vision deficiency to emulate using <see cref="IPage.EmulateVisionDeficiencyAsync(VisionDeficiency)"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<VisionDeficiency>))]
    public enum VisionDeficiency
    {
        /// <summary>
        /// None.
        /// </summary>
        [EnumMember(Value = "none")]
        None,

        /// <summary>
        /// Achromatopsia.
        /// </summary>
        [EnumMember(Value = "achromatopsia")]
        Achromatopsia,

        /// <summary>
        /// BlurredVision.
        /// </summary>
        [EnumMember(Value = "blurredVision")]
        BlurredVision,

        /// <summary>
        /// Deuteranopia.
        /// </summary>
        [EnumMember(Value = "deuteranopia")]
        Deuteranopia,

        /// <summary>
        /// Protanopia.
        /// </summary>
        [EnumMember(Value = "protanopia")]
        Protanopia,

        /// <summary>
        /// Tritanopia.
        /// </summary>
        [EnumMember(Value = "tritanopia")]
        Tritanopia,
    }
}
