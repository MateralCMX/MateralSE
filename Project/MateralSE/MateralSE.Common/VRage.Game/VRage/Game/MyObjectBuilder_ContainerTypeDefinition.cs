namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ContainerTypeDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlAttribute, ProtoMember(0x24)]
        public int CountMin;
        [XmlAttribute, ProtoMember(40)]
        public int CountMax;
        [XmlArrayItem("Item"), ProtoMember(0x2c)]
        public ContainerTypeItem[] Items;

        [ProtoContract]
        public class ContainerTypeItem
        {
            [XmlAttribute, ProtoMember(0x11)]
            public string AmountMin;
            [XmlAttribute, ProtoMember(0x15)]
            public string AmountMax;
            [ProtoMember(0x18), DefaultValue((float) 1f)]
            public float Frequency = 1f;
            [ProtoMember(0x1b)]
            public SerializableDefinitionId Id;
        }
    }
}

