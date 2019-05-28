namespace VRage.Game.Graphics
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEmissiveColorStateResult
    {
        public Color EmissiveColor;
        public Color DisplayColor;
        public float Emissivity;
    }
}

