namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MySpotLightLayout
    {
        public MyLightLayout Light;
        public Vector3 Up;
        public float ApertureCos;
        public Vector3 Direction;
        public float _pad0;
    }
}

