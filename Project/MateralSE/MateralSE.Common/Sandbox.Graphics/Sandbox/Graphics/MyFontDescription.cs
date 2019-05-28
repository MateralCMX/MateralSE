namespace Sandbox.Graphics
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyFontDescription
    {
        public string Id;
        public string Path;
        public bool IsDebug;
    }
}

