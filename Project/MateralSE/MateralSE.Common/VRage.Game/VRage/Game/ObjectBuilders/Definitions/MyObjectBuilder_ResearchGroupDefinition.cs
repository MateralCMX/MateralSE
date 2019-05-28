namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("ResearchGroup"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ResearchGroupDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("BlockId"), ProtoMember(20), DefaultValue((string) null)]
        public SerializableDefinitionId[] Members;
    }
}

