namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderFlagsChange
    {
        public RenderFlags Add;
        public RenderFlags Remove;
    }
}

