namespace Sandbox
{
    using System;

    public class MyGameErrorConsumer : IErrorConsumer
    {
        public void OnError(string header, string message, string callstack)
        {
            string[] textArray1 = new string[] { header, ": ", message, "\n\nStack:\n", callstack };
            string.Concat(textArray1);
        }
    }
}

