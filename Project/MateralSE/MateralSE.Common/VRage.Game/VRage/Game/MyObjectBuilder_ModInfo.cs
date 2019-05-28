namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ModInfo : MyObjectBuilder_Base
    {
        [ProtoMember(11)]
        public ulong SteamIDOwner;
        [ProtoMember(14)]
        public ulong WorkshopId;
    }
}

