namespace Puppeteer.Cdp.Messaging
{
    public class EvaluationExceptionResponseCallFrame
    {
        public int ColumnNumber { get; set; }

        public int LineNumber { get; set; }

        public string Url { get; set; }

        public string FunctionName { get; set; }
    }
}
