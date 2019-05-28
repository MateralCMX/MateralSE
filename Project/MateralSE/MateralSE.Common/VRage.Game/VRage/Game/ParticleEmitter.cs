namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [ProtoContract]
    public class ParticleEmitter
    {
        [ProtoMember(0x4b), XmlAttribute("Version")]
        public int Version;
        [ProtoMember(0x4e)]
        public List<GenerationProperty> Properties;
    }
}

