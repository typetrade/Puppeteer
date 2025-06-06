// * MIT License
//  *
//  * Copyright (c) Darío Kondratiuk
//  *
//  * Permission is hereby granted, free of charge, to any person obtaining a copy
//  * of this software and associated documentation files (the "Software"), to deal
//  * in the Software without restriction, including without limitation the rights
//  * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  * copies of the Software, and to permit persons to whom the Software is
//  * furnished to do so, subject to the following conditions:
//  *
//  * The above copyright notice and this permission notice shall be included in all
//  * copies or substantial portions of the Software.
//  *
//  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  * SOFTWARE.

using System;
using System.Threading.Tasks;
using Puppeteer.Cdp.Messaging;
using Puppeteer.Helpers;
using Puppeteer.Helpers.Json;

namespace Puppeteer;

/// <summary>
/// Prompt manager.
/// </summary>
public class DeviceRequestPromptManager
{
    private readonly TimeoutSettings _timeoutSettings;
    private ICDPSession? client;
    private TaskCompletionSource<DeviceRequestPrompt> _deviceRequestPromptTcs;

    public DeviceRequestPromptManager(ICDPSession client, TimeoutSettings timeoutSettings)
    {
        this.client = client;
        _timeoutSettings = timeoutSettings;
        this.client.MessageReceived += OnMessageReceived;
    }

    public async Task<DeviceRequestPrompt> WaitForDevicePromptAsync(WaitForOptions options = default)
    {
        if (this.client == null)
        {
            throw new PuppeteerException("Cannot wait for device prompt through detached session!");
        }

        var needsEnable = _deviceRequestPromptTcs == null || _deviceRequestPromptTcs.Task.IsCompleted;
        var enableTask = Task.CompletedTask;

        if (needsEnable)
        {
            _deviceRequestPromptTcs = new TaskCompletionSource<DeviceRequestPrompt>();
            enableTask = this.client.SendAsync("DeviceAccess.enable");
        }

        var timeout = options?.Timeout ?? _timeoutSettings.Timeout;
        var task = _deviceRequestPromptTcs.Task.WithTimeout(timeout);

        await Task.WhenAll(enableTask, task).ConfigureAwait(false);

        return await _deviceRequestPromptTcs.Task.ConfigureAwait(false);
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            switch (e.MessageID)
            {
                case "DeviceAccess.deviceRequestPrompted":
                    OnDeviceRequestPrompted(e.MessageData.ToObject<DeviceAccessDeviceRequestPromptedResponse>(true));
                    break;
                case "Target.detachedFromTarget":
                    this.client = null;
                    break;
            }
        }
        catch (Exception ex)
        {
            var message = $"Connection failed to process {e.MessageID}. {ex.Message}. {ex.StackTrace}";
            (this.client as CDPSession)?.Close(message);
        }
    }

    private void OnDeviceRequestPrompted(DeviceAccessDeviceRequestPromptedResponse e)
    {
        if (_deviceRequestPromptTcs == null)
        {
            return;
        }

        if (this.client == null)
        {
            _deviceRequestPromptTcs.TrySetException(new PuppeteerException("Session closed. Most likely the target has been closed."));
            return;
        }

        var devicePrompt = new DeviceRequestPrompt(this.client, _timeoutSettings, e);
        _deviceRequestPromptTcs.TrySetResult(devicePrompt);
    }
}
