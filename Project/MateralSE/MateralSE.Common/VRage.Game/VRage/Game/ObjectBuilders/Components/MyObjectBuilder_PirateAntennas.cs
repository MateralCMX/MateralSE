namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PirateAntennas : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x1c)]
        public long PiratesIdentity;
        [ProtoMember(0x1f)]
        public MyPirateDrone[] Drones;

        [ProtoContract]
        public class MyPirateDrone
        {
            [ProtoMember(15), XmlAttribute("EntityId")]
            public long EntityId;
            [ProtoMember(0x13), XmlAttribute("AntennaEntityId")]
            public long AntennaEntityId;
            [ProtoMember(0x17), XmlAttribute("DespawnTimer")]
            public int DespawnTimer;
        }
    }
}

