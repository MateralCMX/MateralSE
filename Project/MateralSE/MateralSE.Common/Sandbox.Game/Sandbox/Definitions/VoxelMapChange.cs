namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct VoxelMapChange
    {
        public Dictionary<byte, byte> Changes;
    }
}

