// <copyright file="DeviceAccessDeviceRequestPromptedResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Cdp.Messaging
{
    /// <summary>
    /// Response for the DeviceAccessDeviceRequestPrompted command.
    /// </summary>
    public class DeviceAccessDeviceRequestPromptedResponse
    {
        public string Id { get; set; }

        public DeviceAccessDevice[] Devices { get; set; } = [];

        public class DeviceAccessDevice
        {
            public string Name { get; set; }

            public string Id { get; set; }
        }
    }
}
