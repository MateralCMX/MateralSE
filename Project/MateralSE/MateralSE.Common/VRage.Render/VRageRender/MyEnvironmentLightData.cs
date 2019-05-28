namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MyEnvironmentLightData
    {
        [StructDefault]
        public static readonly MyEnvironmentLightData Default;
        [XmlIgnore]
        public Vector3 SunColorRaw;
        public float SunDiffuseFactor;
        public Vector3 SunLightDirection;
        public float AODirLight;
        public float SunGlossFactor;
        public float AmbientDiffuseFactor;
        public float AmbientSpecularFactor;
        public float AmbientForwardPass;
        public Vector3 SunDiscColor;
        public float SunDiscInnerDot;
        public Vector3 SunDiscColor2;
        public float AmbientLightsGatherRadius;
        public float SunDiscOuterDot;
        public float AOIndirectLight;
        public float AOPointLight;
        public float AOSpotLight;
        public float SkyboxBrightness;
        public float EnvSkyboxBrightness;
        public float ShadowFadeoutMultiplier;
        public float EnvShadowFadeoutMultiplier;
        public float EnvAtmosphereBrightness;
        public float AmbientRadius;
        public float GlassAmbient;
        public float ForwardDimDistance;
        public float SunDiscIntensity;
        public int SkipIBLevels;
        public float SunSpecularFactor;
        public float Pad0;
        [XmlIgnore]
        public Vector3 SunSpecularColorRaw;
        public float Pad2;
        static MyEnvironmentLightData()
        {
            MyEnvironmentLightData data = new MyEnvironmentLightData {
                SunColor = Defaults.SunColor,
                SunSpecularColor = Defaults.SunSpecularColor,
                SunDiffuseFactor = 2.9f,
                SunGlossFactor = 1f,
                SunSpecularFactor = 1f,
                AmbientDiffuseFactor = 1f,
                AmbientSpecularFactor = 1f,
                AmbientForwardPass = 0.01f,
                SunDiscColor = Defaults.SunDiscColor,
                SunDiscColor2 = Defaults.SunDiscColor2,
                SunDiscInnerDot = 0.999f,
                SunDiscOuterDot = 0.996f,
                AODirLight = 1f,
                AOIndirectLight = 1.5f,
                AOPointLight = 0.5f,
                AOSpotLight = 0.5f,
                SkyboxBrightness = 1f,
                EnvSkyboxBrightness = 3f,
                ShadowFadeoutMultiplier = 0.02f,
                EnvShadowFadeoutMultiplier = 0f,
                EnvAtmosphereBrightness = 0.2f,
                AmbientLightsGatherRadius = 10f,
                AmbientRadius = 10f,
                GlassAmbient = 0.45f,
                ForwardDimDistance = 3f,
                SunDiscIntensity = Defaults.SunDiscIntensity
            };
            Default = data;
        }

        public Vector3 SunColor
        {
            get => 
                this.SunColorRaw.ToSRGB();
            set => 
                (this.SunColorRaw = value.ToLinearRGB());
        }
        public Vector3 SunSpecularColor
        {
            get => 
                this.SunSpecularColorRaw.ToSRGB();
            set => 
                (this.SunSpecularColorRaw = value.ToLinearRGB());
        }
        private static class Defaults
        {
            public static readonly Vector3 SunColor = new Vector3(1f, 1f, 1f);
            public static readonly Vector3 SunSpecularColor = new Vector3(1f, 1f, 1f);
            public const float SunDiffuseFactor = 2.9f;
            public const float SunGlossFactor = 1f;
            public const float SunSpecularFactor = 1f;
            public const float AmbientDiffuseFactor = 1f;
            public const float AmbientSpecularFactor = 1f;
            public const float AmbientForwardPass = 0.01f;
            public static readonly Vector3 SunDiscColor = new Vector3(1.5f, 1.35f, 1f);
            public static readonly Vector3 SunDiscColor2 = new Vector3(1f, 1f, 1f);
            public static float SunDiscIntensity;
            public const float GlassAmbient = 0.45f;
            public const float SunDiscInnerDot = 0.999f;
            public const float SunDiscOuterDot = 0.996f;
            public const float ForwardDimDistance = 3f;
            public const float AODirLight = 1f;
            public const float AOIndirectLight = 1.5f;
            public const float AOPointLight = 0.5f;
            public const float AOSpotLight = 0.5f;
            public const float SkyboxBrightness = 1f;
            public const float EnvSkyboxBrightness = 3f;
            public const float ShadowFadeoutMultiplier = 0.02f;
            public const float EnvShadowFadeoutMultiplier = 0f;
            public const float EnvAtmosphereBrightness = 0.2f;
            public const float AmbientRadius = 10f;
            public const float AmbientLightsGatherRadius = 10f;
        }
    }
}

