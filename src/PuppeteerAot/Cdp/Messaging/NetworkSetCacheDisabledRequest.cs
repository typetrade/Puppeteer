namespace Puppeteer.Cdp.Messaging
{
    public class NetworkSetCacheDisabledRequest(bool cacheDisabled)
    {
        public bool CacheDisabled { get; set; } = cacheDisabled;
    }
}
