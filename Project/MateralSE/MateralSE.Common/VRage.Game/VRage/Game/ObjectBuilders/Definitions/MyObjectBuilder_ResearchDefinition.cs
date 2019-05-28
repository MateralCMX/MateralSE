namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("ResearchDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ResearchDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x11), XmlElement("Entry")]
        public List<SerializableDefinitionId> Entries;
    }
}

