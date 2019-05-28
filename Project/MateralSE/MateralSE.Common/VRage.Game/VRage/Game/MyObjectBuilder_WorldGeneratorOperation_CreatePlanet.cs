namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("CreatePlanet")]
    public class MyObjectBuilder_WorldGeneratorOperation_CreatePlanet : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x11a), XmlAttribute]
        public string DefinitionName;
        [ProtoMember(0x11d), XmlAttribute]
        public bool AddGPS;
        [ProtoMember(0x120)]
        public SerializableVector3D PositionMinCorner;
        [ProtoMember(0x123)]
        public SerializableVector3D PositionCenter = new SerializableVector3D(Vector3.Invalid);
        [ProtoMember(0x126)]
        public float Diameter;
    }
}

