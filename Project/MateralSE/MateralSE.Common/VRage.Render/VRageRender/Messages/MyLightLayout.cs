namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MyLightLayout
    {
        public Vector3 Position;
        public float Range;
        public Vector3 Color;
        public float Falloff;
        public float GlossFactor;
        public float DiffuseFactor;
        public Vector2 _pad;
    }
}

