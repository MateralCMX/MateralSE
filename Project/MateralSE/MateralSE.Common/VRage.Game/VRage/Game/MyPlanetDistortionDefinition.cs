namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyPlanetDistortionDefinition
    {
        [ProtoMember(0x15f), XmlAttribute(AttributeName="Type")]
        public string Type;
        [ProtoMember(0x163), XmlAttribute(AttributeName="Value")]
        public byte Value;
        [ProtoMember(0x167), XmlAttribute(AttributeName="Frequency")]
        public float Frequency = 1f;
        [ProtoMember(0x16b), XmlAttribute(AttributeName="Height")]
        public float Height = 1f;
        [ProtoMember(0x16f), XmlAttribute(AttributeName="LayerCount")]
        public int LayerCount = 1;
    }
}

