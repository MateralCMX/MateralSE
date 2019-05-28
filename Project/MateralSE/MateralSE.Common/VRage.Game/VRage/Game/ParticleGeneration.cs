namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("ParticleGeneration")]
    public class ParticleGeneration
    {
        [ProtoMember(0x38), XmlAttribute("Name")]
        public string Name = "";
        [ProtoMember(0x3b), XmlAttribute("Version")]
        public int Version;
        [ProtoMember(0x3e)]
        public string GenerationType = "CPU";
        [ProtoMember(0x41)]
        public List<GenerationProperty> Properties;
        [ProtoMember(0x44)]
        public ParticleEmitter Emitter;
    }
}

