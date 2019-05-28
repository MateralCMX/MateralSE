namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyComponentBlockEntry
    {
        [ProtoMember(10), XmlAttribute]
        public string Type;
        [ProtoMember(14), XmlAttribute]
        public string Subtype;
        [ProtoMember(0x15), XmlAttribute]
        public bool Main = true;
        [ProtoMember(0x19), DefaultValue(true), XmlAttribute]
        public bool Enabled = true;
    }
}

