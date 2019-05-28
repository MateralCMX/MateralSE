namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ConsumableItemDefinition : MyObjectBuilder_UsableItemDefinition
    {
        [XmlArrayItem("Stat"), ProtoMember(30)]
        public StatValue[] Stats;

        [ProtoContract]
        public class StatValue
        {
            [ProtoMember(0x10), XmlAttribute]
            public string Name;
            [ProtoMember(20), XmlAttribute]
            public float Value;
            [ProtoMember(0x18), XmlAttribute]
            public float Time;
        }
    }
}

