namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionComponentContainerDropSystem : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(0x58)]
        public List<PlayerContainerData> PlayerData = new List<PlayerContainerData>();
        [ProtoMember(0x5b)]
        public List<MyContainerGPS> GPSForRemoval = new List<MyContainerGPS>();
        [ProtoMember(0x5e)]
        public List<MyEntityForRemoval> EntitiesForRemoval = new List<MyEntityForRemoval>();
        [ProtoMember(0x61)]
        public uint ContainerIdSmall = 1;
        [ProtoMember(100)]
        public uint ContainerIdLarge = 1;
    }
}

