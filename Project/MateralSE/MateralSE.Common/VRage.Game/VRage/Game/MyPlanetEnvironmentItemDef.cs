namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRageMath;

    [ProtoContract]
    public class MyPlanetEnvironmentItemDef
    {
        [ProtoMember(0x1b5), XmlAttribute(AttributeName="TypeId")]
        public string TypeId;
        [ProtoMember(0x1b9), XmlAttribute(AttributeName="SubtypeId")]
        public string SubtypeId;
        [ProtoMember(0x1bd), XmlAttribute(AttributeName="GroupId")]
        public string GroupId;
        [ProtoMember(0x1c1), XmlAttribute(AttributeName="ModifierId")]
        public string ModifierId;
        [ProtoMember(0x1c5)]
        public int GroupIndex = -1;
        [ProtoMember(0x1c8)]
        public int ModifierIndex = -1;
        [ProtoMember(0x1cb), XmlAttribute(AttributeName="Density")]
        public float Density;
        [ProtoMember(0x1cf), XmlAttribute(AttributeName="IsDetail")]
        public bool IsDetail;
        [ProtoMember(0x1d3)]
        public Vector3 BaseColor = Vector3.Zero;
        [ProtoMember(470)]
        public Vector2 ColorSpread = Vector2.Zero;
        [ProtoMember(0x1d9), XmlAttribute(AttributeName="Offset")]
        public float Offset;
        [ProtoMember(0x1dd), XmlAttribute(AttributeName="MaxRoll")]
        public float MaxRoll;
    }
}

