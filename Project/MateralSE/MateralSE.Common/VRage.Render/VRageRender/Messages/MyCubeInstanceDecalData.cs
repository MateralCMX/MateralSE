namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCubeInstanceDecalData
    {
        public uint DecalId;
        public int InstanceIndex;
    }
}

