namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AmmoMagazineDefinition : MyObjectBuilder_PhysicalItemDefinition
    {
        [ProtoMember(0x1f)]
        public int Capacity;
        [ProtoMember(0x22)]
        public MyAmmoCategoryEnum Category;
        [ProtoMember(0x25)]
        public AmmoDefinition AmmoDefinitionId;

        [ProtoContract]
        public class AmmoDefinition
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_AmmoDefinition);
            [XmlAttribute, ProtoMember(0x1b)]
            public string Subtype;
        }
    }
}

