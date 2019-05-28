namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Serialization;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Faction
    {
        [ProtoMember(0x18)]
        public long FactionId;
        [ProtoMember(0x1b), Serialize(MyObjectFlags.DefaultZero)]
        public string Tag;
        [ProtoMember(0x1f), Serialize(MyObjectFlags.DefaultZero)]
        public string Name;
        [ProtoMember(0x23), Serialize(MyObjectFlags.DefaultZero)]
        public string Description;
        [ProtoMember(0x27), Serialize(MyObjectFlags.DefaultZero)]
        public string PrivateInfo;
        [ProtoMember(0x2b)]
        public List<MyObjectBuilder_FactionMember> Members;
        [ProtoMember(0x2e)]
        public List<MyObjectBuilder_FactionMember> JoinRequests;
        [ProtoMember(0x31)]
        public bool AutoAcceptMember;
        [ProtoMember(0x34)]
        public bool AutoAcceptPeace;
        [ProtoMember(0x37)]
        public bool AcceptHumans = true;
        [ProtoMember(0x3a)]
        public bool EnableFriendlyFire = true;
    }
}

