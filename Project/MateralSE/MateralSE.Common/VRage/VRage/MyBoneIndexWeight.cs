namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBoneIndexWeight
    {
        public int Index;
        public float Weight;
    }
}

