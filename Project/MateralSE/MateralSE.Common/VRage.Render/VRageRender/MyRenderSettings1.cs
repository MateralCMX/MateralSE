namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderSettings1 : IEquatable<MyRenderSettings1>
    {
        public bool HqTarget;
        public MyAntialiasingMode AntialiasingMode;
        public bool AmbientOcclusionEnabled;
        public MyShadowsQuality ShadowQuality;
        public MyTextureQuality TextureQuality;
        public MyTextureAnisoFiltering AnisotropicFiltering;
        public MyRenderQualityEnum ModelQuality;
        public MyRenderQualityEnum VoxelQuality;
        public bool HqDepth;
        public MyRenderQualityEnum VoxelShaderQuality;
        public MyRenderQualityEnum AlphaMaskedShaderQuality;
        public MyRenderQualityEnum AtmosphereShaderQuality;
        public float GrassDrawDistance;
        public float GrassDensityFactor;
        public float DistanceFade;
        bool IEquatable<MyRenderSettings1>.Equals(MyRenderSettings1 other) => 
            this.Equals(ref other);

        public override bool Equals(object other)
        {
            MyRenderSettings1 settings = (MyRenderSettings1) other;
            return this.Equals(ref settings);
        }

        public bool Equals(ref MyRenderSettings1 other) => 
            (this.GrassDensityFactor.IsEqual(other.GrassDensityFactor, 0.1f) && (this.GrassDrawDistance.IsEqual(other.GrassDrawDistance, 2f) && ((this.ModelQuality == other.ModelQuality) && ((this.VoxelQuality == other.VoxelQuality) && ((this.AntialiasingMode == other.AntialiasingMode) && ((this.ShadowQuality == other.ShadowQuality) && ((this.AmbientOcclusionEnabled == other.AmbientOcclusionEnabled) && ((this.TextureQuality == other.TextureQuality) && ((this.AnisotropicFiltering == other.AnisotropicFiltering) && ((this.HqDepth == other.HqDepth) && ((this.VoxelShaderQuality == other.VoxelShaderQuality) && ((this.AlphaMaskedShaderQuality == other.AlphaMaskedShaderQuality) && ((this.AtmosphereShaderQuality == other.AtmosphereShaderQuality) && this.DistanceFade.IsEqual(other.DistanceFade, 4f))))))))))))));
    }
}

