namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderFogSettings
    {
        public float FogMultiplier;
        public Color FogColor;
        public float FogDensity;
    }
}

