namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_StatControls : MyObjectBuilder_Base
    {
        public bool ApplyHudScale = true;
        public Vector2 Position;
        public MyGuiDrawAlignEnum OriginAlign;
        [XmlElement(typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase VisibleCondition;
        [XmlArrayItem("StatStyle", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_StatVisualStyle>))]
        public MyObjectBuilder_StatVisualStyle[] StatStyles;
    }
}

