namespace VRage.Game.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyItemInfo
    {
        public float Mass;
        public Vector3 Size;
        public float Volume;
        public MyFixedPoint MaxStackAmount;
        public bool UsesFractions;
        public bool IsOre;
        public bool IsIngot;
    }
}

