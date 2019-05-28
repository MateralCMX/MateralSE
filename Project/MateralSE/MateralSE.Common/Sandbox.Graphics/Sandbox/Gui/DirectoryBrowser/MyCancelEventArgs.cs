namespace Sandbox.Gui.DirectoryBrowser
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyCancelEventArgs
    {
        public MyCancelEventArgs()
        {
            this.Cancel = false;
        }

        public bool Cancel { get; set; }
    }
}

