using System.Collections.Generic;

namespace Puppeteer.Cdp.Messaging
{
    public class PerformanceMetricsResponse
    {
        public string Title { get; set; }

        public List<Metric> Metrics { get; set; }
    }
}
