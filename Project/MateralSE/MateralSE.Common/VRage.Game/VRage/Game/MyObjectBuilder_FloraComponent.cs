namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FloraComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x1d)]
        public List<HarvestedData> HarvestedItems = new List<HarvestedData>();
        [XmlArrayItem("Item"), ProtoMember(0x21)]
        public HarvestedData[] DecayItems = new HarvestedData[0];

        [ProtoContract]
        public class HarvestedData
        {
            [ProtoMember(0x10), XmlAttribute]
            public string GroupName;
            [ProtoMember(20), XmlAttribute]
            public int LocalId;
            [ProtoMember(0x18), XmlAttribute]
            public double Timer;
        }
    }
}

