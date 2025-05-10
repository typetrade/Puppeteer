using System.Collections.Generic;

namespace Puppeteer.Cdp.Messaging
{
    public class PerformanceGetMetricsResponse
    {
        public List<Metric> Metrics { get; set; }
    }
}
