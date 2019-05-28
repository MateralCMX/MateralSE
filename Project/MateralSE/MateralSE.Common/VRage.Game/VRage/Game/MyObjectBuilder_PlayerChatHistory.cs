namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PlayerChatHistory : MyObjectBuilder_Base
    {
        [ProtoMember(0x1a), XmlArrayItem("PCI")]
        public List<MyObjectBuilder_PlayerChatItem> Chat;
        [ProtoMember(0x1d), XmlElement(ElementName="ID")]
        public long IdentityId;
    }
}

