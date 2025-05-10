// <copyright file="WaitForFunctionPollingOption.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Puppeteer.Helpers.Json;
using System.Text.Json.Serialization;

namespace Puppeteer
{
    /// <summary>
    /// An interval at which the <c>pageFunction</c> is executed.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter<WaitForFunctionPollingOption>))]
    public enum WaitForFunctionPollingOption
    {
        /// <summary>
        /// To constantly execute <c>pageFunction</c> in <c>requestAnimationFrame</c> callback.
        /// This is the tightest polling mode which is suitable to observe styling changes.
        /// </summary>
        Raf,

        /// <summary>
        /// To execute <c>pageFunction</c> on every DOM mutation.
        /// </summary>
        Mutation,
    }
}
