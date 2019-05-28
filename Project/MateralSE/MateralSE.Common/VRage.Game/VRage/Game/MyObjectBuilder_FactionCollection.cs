namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, "Factions"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FactionCollection : MyObjectBuilder_Base
    {
        [ProtoMember(0x34)]
        public List<MyObjectBuilder_Faction> Factions;
        [ProtoMember(0x37)]
        public SerializableDictionary<long, long> Players;
        [ProtoMember(0x3a)]
        public List<MyObjectBuilder_FactionRelation> Relations;
        [ProtoMember(0x3d)]
        public List<MyObjectBuilder_FactionRequests> Requests;
    }
}

