namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyImpostorProperties
    {
        public bool Enabled;
        public MyImpostorType ImpostorType;
        public MyTransparentMaterial Material;
        public int ImpostorsCount;
        public float MinDistance;
        public float MaxDistance;
        public float MinRadius;
        public float MaxRadius;
        public Vector4 AnimationSpeed;
        public Vector3 Color;
        public float Intensity;
        public float Contrast;
        public float Radius;
        public float Anim1;
        public float Anim2;
        public float Anim3;
        public float Anim4;
    }
}

