namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_HudDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("StatControl", typeof(MyAbstractXmlSerializer<MyObjectBuilder_StatControls>))]
        public MyObjectBuilder_StatControls[] StatControls;
        public MyObjectBuilder_ToolbarControlVisualStyle Toolbar;
        public MyObjectBuilder_GravityIndicatorVisualStyle GravityIndicator;
        public MyObjectBuilder_CrosshairStyle Crosshair;
        public Vector2I? OptimalScreenRatio;
        public float? CustomUIScale;
        public MyStringHash? VisorOverlayTexture;
    }
}

