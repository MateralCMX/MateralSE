namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionComponentResearchDefinition : MyObjectBuilder_SessionComponentDefinition
    {
        public bool WhitelistMode;
        [XmlElement("Research")]
        public List<SerializableDefinitionId> Researches;
    }
}

