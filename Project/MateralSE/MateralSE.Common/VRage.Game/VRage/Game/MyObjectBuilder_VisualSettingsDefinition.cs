namespace VRage.Game
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageRender;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("VisualSettingsDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VisualSettingsDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlElement(Type=typeof(MyStructXmlSerializer<MyFogProperties>))]
        public MyFogProperties FogProperties = MyFogProperties.Default;
        [XmlElement(Type=typeof(MyStructXmlSerializer<MySunProperties>))]
        public MySunProperties SunProperties = MySunProperties.Default;
        [XmlElement(Type=typeof(MyStructXmlSerializer<MyPostprocessSettings>))]
        public MyPostprocessSettings PostProcessSettings = MyPostprocessSettings.Default;
        public MyShadowsSettings ShadowSettings = new MyShadowsSettings();
    }
}

