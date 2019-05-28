namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BarbarianWaveEventDefinition : MyObjectBuilder_GlobalEventDefinition
    {
        [XmlArrayItem("Wave"), ProtoMember(0x2a)]
        public WaveDef[] Waves;

        [ProtoContract]
        public class BotDef
        {
            [ProtoMember(20), XmlAttribute]
            public string TypeName;
            [ProtoMember(0x18), XmlAttribute]
            public string SubtypeName;
        }

        [ProtoContract]
        public class WaveDef
        {
            [ProtoMember(0x20), XmlAttribute]
            public int Day;
            [XmlArrayItem("Bot"), ProtoMember(0x25)]
            public MyObjectBuilder_BarbarianWaveEventDefinition.BotDef[] Bots;
        }
    }
}

