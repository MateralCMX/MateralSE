namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class CameraControllerSettings
    {
        [ProtoMember(14)]
        public bool IsFirstPerson;
        [ProtoMember(0x11)]
        public double Distance;
        [ProtoMember(20)]
        public SerializableVector2? HeadAngle;
        [XmlAttribute]
        public long EntityId;
    }
}

