namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("AddPlanetPrefab")]
    public class MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x109), XmlAttribute]
        public string PrefabName;
        [ProtoMember(0x10c), XmlAttribute]
        public string DefinitionName;
        [ProtoMember(0x10f), XmlAttribute]
        public bool AddGPS;
        [ProtoMember(0x112)]
        public SerializableVector3D Position;
    }
}

