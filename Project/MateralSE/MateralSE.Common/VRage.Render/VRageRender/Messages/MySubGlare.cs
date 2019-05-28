namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySubGlare
    {
        [XmlIgnore]
        public MyStringId Material;
        public SubGlareType Type;
        public Vector4 Color;
        public bool FixedSize;
        public Vector2 Size;
        public float ScreenIntensityMultiplierCenter;
        public float ScreenIntensityMultiplierEdge;
        public Vector2 ScreenCenterDistance;
        public KeyPoint[] OcclusionToIntensityCurve;
        [XmlElement(ElementName="Material")]
        public string MaterialName
        {
            get => 
                this.Material.String;
            set => 
                (this.Material = MyStringId.GetOrCompute(value));
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyPoint
        {
            public float Occlusion;
            public float Intensity;
        }
    }
}

