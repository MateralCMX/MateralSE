namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("DistantSound")]
    public sealed class DistantSound
    {
        [ProtoMember(13), XmlAttribute]
        public float Distance = 50f;
        [ProtoMember(0x10), XmlAttribute]
        public float DistanceCrossfade = -1f;
        [ProtoMember(0x13), XmlAttribute]
        public string Sound = "";
    }
}

