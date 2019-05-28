namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Client : MyObjectBuilder_Base
    {
        [ProtoMember(12)]
        public ulong SteamId;
        [ProtoMember(15), Serialize(MyObjectFlags.DefaultZero)]
        public string Name;
        [ProtoMember(0x13)]
        public bool IsAdmin;
    }
}

