namespace Sandbox.Gui.DirectoryBrowser
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class MyDirectoryChangeCancelEventArgs : MyCancelEventArgs
    {
        public MyDirectoryChangeCancelEventArgs(string from, string to, MyGuiControlDirectoryBrowser browser)
        {
            this.From = from;
            this.To = to;
            this.Browser = browser;
        }

        public string From { get; set; }

        public string To { get; set; }

        public MyGuiControlDirectoryBrowser Browser { get; private set; }
    }
}

