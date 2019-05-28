namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract]
    public class CutsceneSequenceNodeWaypoint
    {
        [ProtoMember(0xb8), XmlAttribute]
        public string Name = "";
        [ProtoMember(0xbc), XmlAttribute]
        public float Time;
    }
}

