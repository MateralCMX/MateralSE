namespace VRageRender.Messages
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyAtmosphereSettings
    {
        [ProtoMember(10)]
        public Vector3 RayleighScattering;
        [ProtoMember(12)]
        public float MieScattering;
        [ProtoMember(14)]
        public Vector3 MieColorScattering;
        [ProtoMember(0x11)]
        public float RayleighHeight;
        [ProtoMember(0x13)]
        public float RayleighHeightSpace;
        [ProtoMember(0x15)]
        public float RayleighTransitionModifier;
        [ProtoMember(0x17)]
        public float MieHeight;
        [ProtoMember(0x19)]
        public float MieG;
        [ProtoMember(0x1b)]
        public float Intensity;
        [ProtoMember(0x1d)]
        public float FogIntensity;
        [ProtoMember(0x1f)]
        public float SeaLevelModifier;
        [ProtoMember(0x21)]
        public float AtmosphereTopModifier;
        [ProtoMember(0x24)]
        public float Scale;
        [XmlIgnore]
        public Vector3? SunColorLinear;
        [XmlIgnore]
        public Vector3? SunSpecularColorLinear;
        public Vector3 SunColor
        {
            get => 
                ((this.SunColorLinear != null) ? this.SunColorLinear.Value.ToSRGB() : Vector3.One);
            set => 
                (this.SunColorLinear = new Vector3?(value.ToLinearRGB()));
        }
        public Vector3 SunSpecularColor
        {
            get => 
                ((this.SunSpecularColorLinear != null) ? this.SunSpecularColorLinear.Value.ToSRGB() : Vector3.One);
            set => 
                (this.SunSpecularColorLinear = new Vector3?(value.ToLinearRGB()));
        }
        public static MyAtmosphereSettings Defaults()
        {
            MyAtmosphereSettings settings;
            settings.RayleighScattering = new Vector3(20f, 7.5f, 10f);
            settings.MieScattering = 50f;
            settings.MieColorScattering = new Vector3(50f, 50f, 50f);
            settings.RayleighHeight = 10f;
            settings.RayleighHeightSpace = 10f;
            settings.RayleighTransitionModifier = 1f;
            settings.MieHeight = 50f;
            settings.MieG = 0.9998f;
            settings.Intensity = 1f;
            settings.FogIntensity = 0f;
            settings.SeaLevelModifier = 1f;
            settings.AtmosphereTopModifier = 1f;
            settings.Scale = 0.5f;
            settings.SunColorLinear = null;
            settings.SunSpecularColorLinear = null;
            return settings;
        }
    }
}

