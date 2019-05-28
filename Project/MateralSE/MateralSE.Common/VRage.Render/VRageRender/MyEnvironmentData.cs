namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEnvironmentData
    {
        public MyEnvironmentLightData EnvironmentLight;
        public string Skybox;
        public Quaternion SkyboxOrientation;
        public MyEnvironmentProbeData EnvironmentProbe;
        public int EnvMapResolution;
        public int EnvMapFilteredResolution;
        public MyTextureDebugMultipliers TextureMultipliers;
    }
}

