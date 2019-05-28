namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySurfaceParams
    {
        public Vector3 Position;
        public Vector3 Gravity;
        public Vector3 Normal;
        public byte Material;
        public float HeightRatio;
        public float Latitude;
        public float Longitude;
        public byte Biome;
    }
}

