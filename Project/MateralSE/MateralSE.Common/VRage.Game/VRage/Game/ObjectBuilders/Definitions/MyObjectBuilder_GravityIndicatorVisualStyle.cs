namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GravityIndicatorVisualStyle : MyObjectBuilder_Base
    {
        public Vector2 OffsetPx;
        public Vector2 SizePx;
        public Vector2 VelocitySizePx;
        public MyStringHash FillTexture;
        public MyStringHash OverlayTexture;
        public MyStringHash VelocityTexture;
        public MyGuiDrawAlignEnum OriginAlign;
        [XmlElement(typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase VisibleCondition;
    }
}

