namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_HumanoidBotDefinition : MyObjectBuilder_AgentDefinition
    {
        [ProtoMember(0x17)]
        public Item StartingItem;
        [XmlArrayItem("Item"), ProtoMember(0x1b)]
        public Item[] InventoryItems;

        [ProtoContract]
        public class Item
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_PhysicalGunObject);
            [XmlAttribute, ProtoMember(0x13)]
            public string Subtype;
        }
    }
}

