namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBoneDecalUpdate
    {
        public int BoneID;
        public uint DecalID;
    }
}

