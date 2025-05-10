// <copyright file="InvalidTargetException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Puppeteer
{
    [Serializable]
    public class InvalidTargetException : PuppeteerException
    {
        public InvalidTargetException()
        {
        }

        public InvalidTargetException(string message) : base(message)
        {
        }

        public InvalidTargetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTargetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
