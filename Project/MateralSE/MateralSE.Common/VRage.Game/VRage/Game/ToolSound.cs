namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract, XmlType("ToolSound")]
    public struct ToolSound
    {
        [ProtoMember(180), XmlAttribute]
        public string type;
        [ProtoMember(0xb7), XmlAttribute]
        public string subtype;
        [ProtoMember(0xba), XmlAttribute]
        public string sound;
    }
}

