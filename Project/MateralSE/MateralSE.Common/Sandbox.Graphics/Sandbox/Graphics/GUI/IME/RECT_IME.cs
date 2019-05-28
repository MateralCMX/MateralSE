namespace Sandbox.Graphics.GUI.IME
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT_IME
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}

