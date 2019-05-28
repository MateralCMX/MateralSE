namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SpaceFaunaComponent : MyObjectBuilder_SessionComponent
    {
        [XmlArrayItem("Info"), ProtoMember(0x3a)]
        public List<SpawnInfo> SpawnInfos = new List<SpawnInfo>();
        [XmlArrayItem("Info"), ProtoMember(0x3e)]
        public List<TimeoutInfo> TimeoutInfos = new List<TimeoutInfo>();

        [ProtoContract]
        public class SpawnInfo
        {
            [ProtoMember(0x10), XmlAttribute]
            public double X;
            [ProtoMember(20), XmlAttribute]
            public double Y;
            [ProtoMember(0x18), XmlAttribute]
            public double Z;
            [ProtoMember(0x1c), XmlAttribute("S")]
            public int SpawnTime;
            [ProtoMember(0x20), XmlAttribute("A")]
            public int AbandonTime;
        }

        [ProtoContract]
        public class TimeoutInfo
        {
            [ProtoMember(40), XmlAttribute]
            public double X;
            [ProtoMember(0x2c), XmlAttribute]
            public double Y;
            [ProtoMember(0x30), XmlAttribute]
            public double Z;
            [ProtoMember(0x34), XmlAttribute("T")]
            public int Timeout;
        }
    }
}

