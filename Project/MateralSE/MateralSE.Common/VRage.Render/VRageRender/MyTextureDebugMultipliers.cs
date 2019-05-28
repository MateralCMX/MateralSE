namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MyTextureDebugMultipliers
    {
        public float AlbedoMultiplier;
        public float MetalnessMultiplier;
        public float GlossMultiplier;
        public float AoMultiplier;
        public float EmissiveMultiplier;
        public float ColorMaskMultiplier;
        public float AlbedoShift;
        public float MetalnessShift;
        public float GlossShift;
        public float AoShift;
        public float EmissiveShift;
        public float ColorMaskShift;
        public Vector4 ColorizeHSV;
        [StructDefault]
        public static readonly MyTextureDebugMultipliers Defaults;
        static MyTextureDebugMultipliers()
        {
            MyTextureDebugMultipliers multipliers = new MyTextureDebugMultipliers {
                AlbedoMultiplier = 1f,
                MetalnessMultiplier = 1f,
                GlossMultiplier = 1f,
                AoMultiplier = 1f,
                EmissiveMultiplier = 1f,
                ColorMaskMultiplier = 1f,
                ColorizeHSV = new Vector4(0f, 0.8f, -0.1f, 0f)
            };
            Defaults = multipliers;
        }
    }
}

