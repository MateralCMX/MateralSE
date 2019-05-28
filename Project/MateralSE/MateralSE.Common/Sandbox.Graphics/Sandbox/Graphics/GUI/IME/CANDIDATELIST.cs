namespace Sandbox.Graphics.GUI.IME
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class CANDIDATELIST
    {
        public int dwSize;
        public int dwStyle;
        public int dwCount;
        public int dwSelection;
        public int dwPageStart;
        public int dwPageSize;
        public int dwOffset;
    }
}

