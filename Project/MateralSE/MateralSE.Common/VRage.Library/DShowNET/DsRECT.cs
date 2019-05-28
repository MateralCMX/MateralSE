namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DsRECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

