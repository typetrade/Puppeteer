using Puppeteer.Input;

namespace Puppeteer.Cdp.Messaging
{
    public class InputDispatchTouchEventRequest
    {
        public string Type { get; set; }

        public TouchPoint[] TouchPoints { get; set; }

        public int Modifiers { get; set; }
    }
}
