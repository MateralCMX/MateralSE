namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.GUI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_StatVisualStyle : MyObjectBuilder_Base
    {
        public MyStringHash StatId;
        [XmlElement(typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase VisibleCondition;
        [XmlElement(typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase BlinkCondition;
        public Vector2 SizePx;
        public Vector2 OffsetPx;
        public uint? FadeInTimeMs;
        public uint? FadeOutTimeMs;
        public uint? MaxOnScreenTimeMs;
        public MyAlphaBlinkBehavior Blink;
        public VisualStyleCategory? Category;
    }
}

