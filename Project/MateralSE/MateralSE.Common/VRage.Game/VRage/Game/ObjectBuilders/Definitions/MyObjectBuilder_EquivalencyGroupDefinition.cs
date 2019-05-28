namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_EquivalencyGroupDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x12), XmlElement("Main")]
        public SerializableDefinitionId MainId;
        [ProtoMember(0x16), XmlElement("ForceMain"), DefaultValue(false)]
        public bool ForceMainId;
        [ProtoMember(0x1a), XmlElement("Equivalent")]
        public SerializableDefinitionId[] EquivalentId;
    }
}

