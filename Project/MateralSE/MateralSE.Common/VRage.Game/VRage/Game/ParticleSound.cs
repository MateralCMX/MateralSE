namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("ParticleSound")]
    public class ParticleSound
    {
        [ProtoMember(0x62), XmlAttribute("Name")]
        public string Name = "";
        [ProtoMember(0x65), XmlAttribute("Version")]
        public int Version;
        [ProtoMember(0x68)]
        public List<GenerationProperty> Properties;
    }
}

