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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Puppeteer;
using Puppeteer.Cdp.Messaging;

namespace Puppeteer.Cdp;

/// <inheritdoc />
public class CdpBrowserContext : BrowserContext
{
    private readonly Connection _connection;
    private readonly CdpBrowser _browser;

    public CdpBrowserContext(Connection connection, CdpBrowser browser, string contextId)
    {
        _connection = connection;
        Browser = browser;
        _browser = browser;
        Id = contextId;
    }

    /// <inheritdoc/>
    public override ITarget[] Targets() => Array.FindAll(Browser.Targets(), target => target.BrowserContext == this);

    /// <inheritdoc/>
    public override async Task<IPage[]> PagesAsync()
        => (await Task.WhenAll(
                Targets()
                    .Where(t =>
                        t.Type == TargetType.Page ||
                        (t.Type == TargetType.Other && Browser.IsPageTargetFunc(t as Target)))
                    .Select(t => t.PageAsync())).ConfigureAwait(false))
            .Where(p => p != null).ToArray();

    /// <inheritdoc/>
    public override Task<IPage> NewPageAsync() => this._browser.CreatePageInContextAsync(this.Id);

    /// <inheritdoc/>
    public override Task CloseAsync()
    {
        if (this.Id == null)
        {
            throw new PuppeteerException("Non-incognito profiles cannot be closed!");
        }

        return this._browser.DisposeContextAsync(this.Id);
    }

    /// <inheritdoc/>
    public override Task OverridePermissionsAsync(string origin, IEnumerable<OverridePermission> permissions)
        => _connection.SendAsync("Browser.grantPermissions", new BrowserGrantPermissionsRequest
        {
            Origin = origin,
            BrowserContextId = Id,
            Permissions = permissions.ToArray(),
        });

    /// <inheritdoc/>
    public override Task ClearPermissionOverridesAsync()
        => _connection.SendAsync("Browser.resetPermissions", new BrowserResetPermissionsRequest
        {
            BrowserContextId = Id,
        });
}
