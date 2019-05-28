namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySSAOSettings
    {
        [StructDefault]
        public static readonly MySSAOSettings Default;
        public bool Enabled;
        public bool UseBlur;
        [XmlElement(Type=typeof(MyStructXmlSerializer<Layout>))]
        public Layout Data;
        static MySSAOSettings()
        {
            MySSAOSettings settings = new MySSAOSettings {
                Enabled = false,
                UseBlur = true,
                Data = Layout.Default
            };
            Default = settings;
        }
        [StructLayout(LayoutKind.Sequential, Pack=1), XmlType("MySSAOSettings.Layout")]
        public struct Layout
        {
            public float MinRadius;
            public float MaxRadius;
            public float RadiusGrowZScale;
            public float Falloff;
            public float RadiusBias;
            public float Contrast;
            public float Normalization;
            public float ColorScale;
            [StructDefault]
            public static readonly MySSAOSettings.Layout Default;
            static Layout()
            {
                MySSAOSettings.Layout layout = new MySSAOSettings.Layout {
                    MinRadius = 0.08f,
                    MaxRadius = 93.374f,
                    RadiusGrowZScale = 3.293f,
                    Falloff = 10f,
                    RadiusBias = 0.38f,
                    Normalization = 1.084f,
                    Contrast = 4.347f,
                    ColorScale = 0.6f
                };
                Default = layout;
            }
        }
    }
}

