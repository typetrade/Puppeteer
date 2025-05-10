using System;

namespace Puppeteer
{
    public class PageBinding
    {
        public string Name { get; set; }

        public Delegate Function { get; set; }
    }
}
