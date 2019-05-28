namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CircularProgressBarStatVisualStyle : MyObjectBuilder_StatVisualStyle
    {
        public Vector2 SegmentSizePx;
        public MyStringHash SegmentTexture;
        public MyStringHash? BackgroudTexture;
        public Vector2? SegmentOrigin;
        public float? SpacingAngle;
        public float? AngleOffset;
        public bool? Animate;
        public int? NumberOfSegments;
        public bool? ShowEmptySegments;
        public Vector4? EmptySegmentColorMask;
        public Vector4? FullSegmentColorMask;
        public Vector4? AnimatedSegmentColorMask;
        public double? AnimationDelayMs;
        public double? AnimationSegmentDelayMs;
    }
}

