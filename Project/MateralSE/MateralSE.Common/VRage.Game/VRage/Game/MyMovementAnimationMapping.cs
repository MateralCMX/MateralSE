namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class MyMovementAnimationMapping
    {
        [ProtoMember(0x4d), XmlAttribute]
        public string Name;
        [ProtoMember(80), XmlAttribute]
        public string AnimationSubtypeName;
    }
}

