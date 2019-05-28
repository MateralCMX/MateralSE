namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_LastSession : MyObjectBuilder_Base
    {
        [ProtoMember(11)]
        public string Path;
        [ProtoMember(14)]
        public bool IsContentWorlds;
        [ProtoMember(0x11)]
        public bool IsOnline;
        [ProtoMember(20)]
        public bool IsLobby;
        [ProtoMember(0x17)]
        public string GameName;
        [ProtoMember(0x1a)]
        public string ServerIP;
        [ProtoMember(0x1d)]
        public int ServerPort;
    }
}

