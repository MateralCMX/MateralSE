namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("AlternativeImpactSound")]
    public sealed class AlternativeImpactSounds
    {
        [ProtoMember(0x34), XmlAttribute]
        public float mass;
        [ProtoMember(0x37), XmlAttribute]
        public string soundCue = "";
        [ProtoMember(0x3a), XmlAttribute]
        public float minVelocity;
        [ProtoMember(0x3d), XmlAttribute]
        public float maxVolumeVelocity;
    }
}

