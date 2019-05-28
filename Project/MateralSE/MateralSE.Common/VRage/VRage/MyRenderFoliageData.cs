namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderFoliageData
    {
        public float Density;
        public MyFoliageType Type;
        public FoliageEntry[] Entries;
        [StructLayout(LayoutKind.Sequential)]
        public struct FoliageEntry
        {
            public Vector2 Size;
            public float SizeVariation;
            public string ColorAlphaTexture;
            public string NormalGlossTexture;
            public float Probability;
        }
    }
}

