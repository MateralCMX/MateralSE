namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderVoxelMaterialData
    {
        public byte Index;
        public TextureSet[] TextureSets;
        public Vector4 Far3Color;
        public TilingSetup StandardTilingSetup;
        public TilingSetup SimpleTilingSetup;
        public MyRenderFoliageData? Foliage;
        [StructLayout(LayoutKind.Sequential)]
        public struct TextureSet
        {
            public string ColorMetalXZnY;
            public string ColorMetalY;
            public string NormalGlossXZnY;
            public string NormalGlossY;
            public string ExtXZnY;
            public string ExtY;
            public void Check()
            {
                if (this.ColorMetalY == null)
                {
                    this.ColorMetalY = this.ColorMetalXZnY;
                }
                if (this.NormalGlossY == null)
                {
                    this.NormalGlossY = this.NormalGlossXZnY;
                }
                if (this.ExtY == null)
                {
                    this.ExtY = this.ExtXZnY;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TilingSetup
        {
            public Vector4 DistanceAndScale;
            public Vector3 DistanceAndScaleFar;
            public float TilingScale;
            public Vector3 DistanceAndScaleFar2;
            public float _padding1;
            public Vector3 DistanceAndScaleFar3;
            public float ExtensionDetailScale;
            public float InitialScale
            {
                get => 
                    this.DistanceAndScale.X;
                set => 
                    (this.DistanceAndScale.X = value);
            }
            public float InitialDistance
            {
                get => 
                    this.DistanceAndScale.Y;
                set => 
                    (this.DistanceAndScale.Y = value);
            }
            public float ScaleMultiplier
            {
                get => 
                    this.DistanceAndScale.Z;
                set => 
                    (this.DistanceAndScale.Z = value);
            }
            public float DistanceMultiplier
            {
                get => 
                    this.DistanceAndScale.W;
                set => 
                    (this.DistanceAndScale.W = value);
            }
            public float Far1Scale
            {
                get => 
                    this.DistanceAndScaleFar.X;
                set
                {
                    this.DistanceAndScaleFar.X = value;
                    this.DistanceAndScaleFar.Z = 1f;
                }
            }
            public float Far1Distance
            {
                get => 
                    this.DistanceAndScaleFar.Y;
                set => 
                    (this.DistanceAndScaleFar.Y = value);
            }
            public float Far2Scale
            {
                get => 
                    this.DistanceAndScaleFar2.X;
                set
                {
                    this.DistanceAndScaleFar2.X = value;
                    this.DistanceAndScaleFar2.Z = 2f;
                }
            }
            public float Far2Distance
            {
                get => 
                    this.DistanceAndScaleFar2.Y;
                set => 
                    (this.DistanceAndScaleFar2.Y = value);
            }
            public float Far3Scale
            {
                get => 
                    this.DistanceAndScaleFar3.X;
                set
                {
                    this.DistanceAndScaleFar3.X = value;
                    this.DistanceAndScaleFar3.Z = 3f;
                }
            }
            public float Far3Distance
            {
                get => 
                    this.DistanceAndScaleFar3.Y;
                set => 
                    (this.DistanceAndScaleFar3.Y = value);
            }
        }
    }
}

