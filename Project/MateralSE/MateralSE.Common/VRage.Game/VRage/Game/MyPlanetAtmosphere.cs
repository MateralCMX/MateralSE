namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyPlanetAtmosphere
    {
        [ProtoMember(0x211), XmlElement]
        public bool Breathable;
        [ProtoMember(0x215), XmlElement]
        public float OxygenDensity = 1f;
        [ProtoMember(0x219), XmlElement]
        public float Density = 1f;
        [ProtoMember(0x21d), XmlElement]
        public float LimitAltitude = 2f;
        [XmlElement, ProtoMember(0x222)]
        public float MaxWindSpeed;
    }
}

