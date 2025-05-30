using System;
using System.Threading.Tasks;
using Puppeteer.Cdp;
using Puppeteer.Helpers;

namespace Puppeteer.Cdp
{
    /// <summary>
    /// Worker target.
    /// </summary>
    public class CdpWorkerTarget : CdpTarget
    {
        private Task<WebWorker> _workerTask;

        public CdpWorkerTarget(
            TargetInfo targetInfo,
            CDPSession session,
            BrowserContext context,
            ITargetManager targetManager,
            Func<bool, Task<CDPSession>> sessionFactory,
            TaskQueue screenshotTaskQueue)
            : base(targetInfo, (CdpCDPSession)session, (CdpBrowserContext)context, targetManager, sessionFactory, screenshotTaskQueue)
        {
        }

        /// <inheritdoc/>
        public override Task<WebWorker> WorkerAsync()
        {
            _workerTask ??= WorkerInternalAsync();
            return _workerTask;
        }

        private async Task<WebWorker> WorkerInternalAsync()
        {
            var client = Session ?? await SessionFactory(false).ConfigureAwait(false);
            return new CdpWebWorker(
                client,
                TargetInfo.Url,
                TargetInfo.TargetId,
                TargetInfo.Type,
                (_, _, _) => Task.CompletedTask,
                _ => { });
        }
    }
}
