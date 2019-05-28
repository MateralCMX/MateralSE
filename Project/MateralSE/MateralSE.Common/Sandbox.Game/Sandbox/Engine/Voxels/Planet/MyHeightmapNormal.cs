namespace Sandbox.Engine.Voxels.Planet
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyHeightmapNormal
    {
        public ushort Dx;
        public ushort Dy;
    }
}

