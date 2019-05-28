namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPassLoddingSetting
    {
        public int LodShiftVisible;
        public int LodShift;
        public int MinLod;
        [StructDefault]
        public static readonly MyPassLoddingSetting Default;
        static MyPassLoddingSetting()
        {
        }
    }
}

