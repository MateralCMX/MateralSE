namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_GlobalChatItem : MyObjectBuilder_Base
    {
        [ProtoMember(100), XmlAttribute("t")]
        public string Text;
        [ProtoMember(0x67), XmlElement(ElementName="I")]
        public long IdentityIdUniqueNumber;
        [ProtoMember(0x6a), XmlAttribute("a"), DefaultValue("")]
        public string Author;
        [ProtoMember(0x6d), XmlAttribute("f"), DefaultValue("Blue")]
        public string Font;
    }
}

