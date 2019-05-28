namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;

    public class MyShadowsSettings
    {
        private float[] m_shadowCascadeSmallSkipThresholds = new float[] { 1000f, 5000f, 200f, 1000f, 1000f, 1000f };
        private bool[] m_shadowCascadeFrozen = new bool[6];
        [XmlElement(Type=typeof(MyStructXmlSerializer<Struct>))]
        public Struct Data = Struct.Default;
        private Cascade[] m_cascades = new Cascade[8];

        public MyShadowsSettings()
        {
            float num = 5f;
            float num2 = 5f;
            float num3 = 2f;
            float num4 = 758f;
            float num5 = 0f;
            for (int i = 0; i < 8; i++)
            {
                float num7 = num * ((float) Math.Pow((double) num2, (double) i));
                this.m_cascades[i].FullCoverageDepth = num7;
                this.m_cascades[i].ExtendedCoverageDepth = num7 * num3;
                this.m_cascades[i].ShadowNormalOffset = (num7 + num3) / num4;
                this.m_cascades[i].SkippingSmallObjectThreshold = num5;
            }
        }

        public void CopyFrom(MyShadowsSettings settings)
        {
            this.ShadowCascadeSmallSkipThresholds = settings.ShadowCascadeSmallSkipThresholds.Clone() as float[];
            this.ShadowCascadeFrozen = settings.ShadowCascadeFrozen.Clone() as bool[];
            this.Data = settings.Data;
            this.Cascades = settings.Cascades.Clone() as Cascade[];
        }

        [XmlArrayItem("Value")]
        public float[] ShadowCascadeSmallSkipThresholds
        {
            get => 
                this.m_shadowCascadeSmallSkipThresholds;
            set
            {
                if (this.ShadowCascadeSmallSkipThresholds.Length != value.Length)
                {
                    this.ShadowCascadeSmallSkipThresholds = new float[value.Length];
                }
                value.CopyTo(this.ShadowCascadeSmallSkipThresholds, 0);
            }
        }

        [XmlIgnore]
        public bool[] ShadowCascadeFrozen
        {
            get => 
                this.m_shadowCascadeFrozen;
            set
            {
                if (this.ShadowCascadeFrozen.Length != value.Length)
                {
                    this.ShadowCascadeFrozen = new bool[value.Length];
                }
                value.CopyTo(this.ShadowCascadeFrozen, 0);
            }
        }

        [XmlArrayItem("Cascade")]
        public Cascade[] Cascades
        {
            get => 
                this.m_cascades;
            set
            {
                if (this.m_cascades.Length != value.Length)
                {
                    this.m_cascades = new Cascade[value.Length];
                }
                value.CopyTo(this.m_cascades, 0);
            }
        }

        [StructLayout(LayoutKind.Sequential), XmlType("MyShadowSettings.Cascade")]
        public struct Cascade
        {
            public float FullCoverageDepth;
            public float ExtendedCoverageDepth;
            public float ShadowNormalOffset;
            public float SkippingSmallObjectThreshold;
        }

        [StructLayout(LayoutKind.Sequential), XmlType("MyShadowSettings.Struct")]
        public struct Struct
        {
            [StructDefault]
            public static readonly MyShadowsSettings.Struct Default;
            public bool UpdateCascadesEveryFrame;
            public bool EnableShadowBlur;
            public float ShadowCascadeMaxDistance;
            public float ShadowCascadeMaxDistanceMultiplierMedium;
            public float ShadowCascadeMaxDistanceMultiplierHigh;
            public float ShadowCascadeMaxDistanceMultiplierExtreme;
            public float ShadowCascadeSpreadFactor;
            public float ShadowCascadeZOffset;
            public float ReflectorShadowDistanceLow;
            public float ReflectorShadowDistanceMedium;
            public float ReflectorShadowDistanceHigh;
            public float ReflectorShadowDistanceExtreme;
            public float LightDirectionDifferenceThreshold;
            public float LightDirectionChangeDelayMultiplier;
            public float ZBias;
            public int CascadesCount;
            static Struct()
            {
                MyShadowsSettings.Struct struct2 = new MyShadowsSettings.Struct {
                    UpdateCascadesEveryFrame = false,
                    EnableShadowBlur = true,
                    ShadowCascadeMaxDistance = 300f,
                    ShadowCascadeMaxDistanceMultiplierMedium = 2f,
                    ShadowCascadeMaxDistanceMultiplierHigh = 3.5f,
                    ShadowCascadeSpreadFactor = 0.5f,
                    ShadowCascadeZOffset = 400f,
                    ReflectorShadowDistanceLow = 0.2f,
                    ReflectorShadowDistanceMedium = 0.4f,
                    ReflectorShadowDistanceHigh = 0.8f,
                    LightDirectionDifferenceThreshold = 0.0175f,
                    LightDirectionChangeDelayMultiplier = 18f,
                    ZBias = 0.01f,
                    CascadesCount = 6
                };
                Default = struct2;
            }
        }
    }
}

