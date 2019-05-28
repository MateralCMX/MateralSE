namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_PlayerChatItem : MyObjectBuilder_Base
    {
        [ProtoMember(0x3d), XmlAttribute("t")]
        public string Text;
        [ProtoMember(0x40), XmlElement(ElementName="I")]
        public long IdentityIdUniqueNumber;
        [ProtoMember(0x43), XmlElement(ElementName="T")]
        public long TimestampMs;
        [ProtoMember(70), DefaultValue(true), XmlElement(ElementName="S")]
        public bool Sent = true;
    }
}

