namespace VRage.Game.ObjectBuilders.Definitions.SessionComponents
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ClipboardDefinition : MyObjectBuilder_SessionComponentDefinition
    {
        public MyPlacementSettings PastingSettings;
    }
}

