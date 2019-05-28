namespace Sandbox
{
    using System;

    internal class ErrorInfo
    {
        public string Match;
        public string Caption;
        public string Message;

        public ErrorInfo(string match, string caption, string message)
        {
            this.Match = match;
            this.Caption = caption;
            this.Message = message;
        }
    }
}

