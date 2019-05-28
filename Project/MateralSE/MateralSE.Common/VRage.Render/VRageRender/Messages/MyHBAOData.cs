namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyHBAOData
    {
        public bool Enabled;
        public float Radius;
        public float Bias;
        public float SmallScaleAO;
        public float LargeScaleAO;
        public float PowerExponent;
        public bool UseGBufferNormals;
        public bool ForegroundAOEnable;
        public float ForegroundViewDepth;
        public bool BackgroundAOEnable;
        public bool AdaptToFOV;
        public float BackgroundViewDepth;
        public bool DepthClampToEdge;
        public bool DepthThresholdEnable;
        public float DepthThreshold;
        public float DepthThresholdSharpness;
        public bool BlurEnable;
        public bool BlurRadius4;
        public float BlurSharpness;
        public bool BlurSharpnessFunctionEnable;
        public float BlurSharpnessFunctionForegroundScale;
        public float BlurSharpnessFunctionForegroundViewDepth;
        public float BlurSharpnessFunctionBackgroundViewDepth;
        [StructDefault]
        public static readonly MyHBAOData Default;
        static MyHBAOData()
        {
            MyHBAOData data = new MyHBAOData {
                Enabled = true,
                Radius = 2f,
                Bias = 0.2f,
                SmallScaleAO = 1f,
                LargeScaleAO = 1f,
                PowerExponent = 5f,
                DepthClampToEdge = false,
                UseGBufferNormals = false,
                DepthThresholdEnable = false,
                ForegroundAOEnable = true,
                ForegroundViewDepth = 7f,
                BackgroundAOEnable = true,
                AdaptToFOV = true,
                BackgroundViewDepth = 200f,
                DepthThreshold = 0f,
                DepthThresholdSharpness = 100f,
                BlurEnable = true,
                BlurRadius4 = true,
                BlurSharpness = 1f,
                BlurSharpnessFunctionEnable = false,
                BlurSharpnessFunctionForegroundScale = 4f,
                BlurSharpnessFunctionForegroundViewDepth = 0f,
                BlurSharpnessFunctionBackgroundViewDepth = 1f
            };
            Default = data;
        }
    }
}

