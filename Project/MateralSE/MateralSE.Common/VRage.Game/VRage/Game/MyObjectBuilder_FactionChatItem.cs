namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_FactionChatItem : MyObjectBuilder_Base
    {
        [ProtoMember(0x4f), XmlAttribute("t")]
        public string Text;
        [ProtoMember(0x52), XmlElement(ElementName="I")]
        public long IdentityIdUniqueNumber;
        [ProtoMember(0x55), XmlElement(ElementName="T")]
        public long TimestampMs;
        [ProtoMember(0x58), DefaultValue((string) null), XmlElement(ElementName="PTST")]
        public List<long> PlayersToSendToUniqueNumber;
        [ProtoMember(0x5b), DefaultValue((string) null), XmlElement(ElementName="IAST")]
        public List<bool> IsAlreadySentTo;
    }
}

