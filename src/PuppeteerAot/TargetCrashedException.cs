// <copyright file="TargetCrashedException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception that is thrown when a target crashes.
    /// </summary>
    [Serializable]
    public class TargetCrashedException : PuppeteerException
    {
        public TargetCrashedException()
        {
        }

        public TargetCrashedException(string message) : base(message)
        {
        }

        public TargetCrashedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TargetCrashedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
