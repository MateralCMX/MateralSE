namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRageMath;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MyPostprocessSettings
    {
        [StructDefault]
        public static readonly MyPostprocessSettings Default;
        public bool EnableTonemapping;
        public bool EnableEyeAdaptation;
        public bool HighQualityBloom;
        public bool BloomAntiFlickerFilter;
        public int BloomSize;
        public float Temperature;
        public bool BloomEnabled;
        public float HistogramLogMin;
        public float HistogramLogMax;
        public float HistogramFilterMin;
        public float HistogramFilterMax;
        public float MinEyeAdaptationLogBrightness;
        public float MaxEyeAdaptationLogBrightness;
        public float HistogramLuminanceThreshold;
        public float HistogramSkyboxFactor;
        public bool EyeAdaptationPrioritizeScreenCenter;
        public int ChromaticIterations;
        public string DirtTexture;
        [XmlElement(Type=typeof(MyStructXmlSerializer<Layout>))]
        public Layout Data;
        static MyPostprocessSettings()
        {
            MyPostprocessSettings settings = new MyPostprocessSettings {
                EnableTonemapping = true,
                EnableEyeAdaptation = false,
                BloomSize = 6,
                Temperature = 6500f,
                HighQualityBloom = true,
                BloomAntiFlickerFilter = true,
                BloomEnabled = true,
                HistogramLogMin = -4f,
                HistogramLogMax = 4f,
                HistogramFilterMin = 70f,
                HistogramFilterMax = 95f,
                MinEyeAdaptationLogBrightness = -1f,
                MaxEyeAdaptationLogBrightness = 2f,
                HistogramLuminanceThreshold = 0f,
                HistogramSkyboxFactor = 0.5f,
                EyeAdaptationPrioritizeScreenCenter = false,
                ChromaticIterations = 4,
                DirtTexture = "",
                Data = Layout.Default
            };
            Default = settings;
        }

        public static MyPostprocessSettings LerpExposure(ref MyPostprocessSettings A, ref MyPostprocessSettings B, float t)
        {
            MyPostprocessSettings settings = A;
            settings.Data.LuminanceExposure = MathHelper.Lerp(A.Data.LuminanceExposure, B.Data.LuminanceExposure, t);
            return settings;
        }

        public Layout GetProcessedData()
        {
            Layout data = this.Data;
            if (this.EnableEyeAdaptation)
            {
                data.ConstantLuminance = -1f;
            }
            else
            {
                data.EyeAdaptationTau = 0f;
            }
            data.TemperatureColor = ColorExtensions.TemperatureToRGB(this.Temperature);
            return data;
        }
        [StructLayout(LayoutKind.Sequential, Pack=1), XmlType("MyPostprocessSettings.Layout")]
        public struct Layout
        {
            public float Contrast;
            public float Brightness;
            public float ConstantLuminance;
            public float LuminanceExposure;
            public float Saturation;
            public float BrightnessFactorR;
            public float BrightnessFactorG;
            public float BrightnessFactorB;
            public Vector3 TemperatureColor;
            public float TemperatureStrength;
            public float Vibrance;
            public float EyeAdaptationTau;
            public float BloomExposure;
            public float BloomLumaThreshold;
            public float BloomMult;
            public float BloomEmissiveness;
            public float BloomDepthStrength;
            public float BloomDepthSlope;
            public Vector3 LightColor;
            public float Res2;
            public Vector3 DarkColor;
            public float SepiaStrength;
            public float EyeAdaptationSpeedUp;
            public float EyeAdaptationSpeedDown;
            public float WhitePoint;
            public float BloomDirtRatio;
            public int GrainSize;
            public float GrainAmount;
            public float GrainStrength;
            public float ChromaticFactor;
            public float VignetteStart;
            public float VignetteLength;
            public float Res0;
            public float Res1;
            [StructDefault]
            public static readonly MyPostprocessSettings.Layout Default;
            static Layout()
            {
                MyPostprocessSettings.Layout layout = new MyPostprocessSettings.Layout {
                    Contrast = 1f,
                    Brightness = 1f,
                    ConstantLuminance = 0.1f,
                    LuminanceExposure = 1f,
                    Saturation = 1f,
                    BrightnessFactorR = 1f,
                    BrightnessFactorG = 1f,
                    BrightnessFactorB = 1f,
                    EyeAdaptationTau = 0.3f,
                    Vibrance = 0f,
                    TemperatureStrength = 0f,
                    BloomEmissiveness = 1f,
                    BloomExposure = 5.8f,
                    BloomLumaThreshold = 0.16f,
                    BloomMult = 0.28f,
                    BloomDepthStrength = 2f,
                    BloomDepthSlope = 0.3f,
                    LightColor = new Vector3(1f, 0.9f, 0.5f),
                    DarkColor = new Vector3(0.2f, 0.05f, 0f),
                    SepiaStrength = 0f,
                    EyeAdaptationSpeedUp = 2f,
                    EyeAdaptationSpeedDown = 1f,
                    WhitePoint = 6f,
                    BloomDirtRatio = 0.5f,
                    GrainSize = 1,
                    GrainAmount = 0.1f,
                    GrainStrength = 0f,
                    ChromaticFactor = 0.1f,
                    VignetteStart = 2f,
                    VignetteLength = 2f
                };
                Default = layout;
            }
        }
    }
}

