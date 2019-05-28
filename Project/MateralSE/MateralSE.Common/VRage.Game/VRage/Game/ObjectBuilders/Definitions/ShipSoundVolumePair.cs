namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class ShipSoundVolumePair
    {
        [ProtoMember(0x6b), XmlAttribute("Speed")]
        public float Speed;
        [ProtoMember(110), XmlAttribute("Volume")]
        public float Volume;
    }
}

