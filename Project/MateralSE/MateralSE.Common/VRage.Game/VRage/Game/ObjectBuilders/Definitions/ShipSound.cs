namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class ShipSound
    {
        [ProtoMember(0x61), XmlAttribute("Type")]
        public ShipSystemSoundsEnum SoundType = ShipSystemSoundsEnum.MainLoopMedium;
        [ProtoMember(100), XmlAttribute("SoundName")]
        public string SoundName = "";
    }
}

