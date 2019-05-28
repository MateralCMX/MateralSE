namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderPlanetSettings
    {
        public float AtmosphereIntensityMultiplier;
        public float AtmosphereIntensityAmbientMultiplier;
        public float AtmosphereDesaturationFactorForward;
        public float CloudsIntensityMultiplier;
    }
}

