namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("ResearchBlock"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ResearchBlockDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x12), XmlArrayItem("GroupSubtype")]
        public string[] UnlockedByGroups;
    }
}

