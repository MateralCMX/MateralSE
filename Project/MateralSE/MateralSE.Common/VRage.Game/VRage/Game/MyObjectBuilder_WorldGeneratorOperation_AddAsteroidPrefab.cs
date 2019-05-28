namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("AddAsteroidPrefab")]
    public class MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0xcd), XmlAttribute]
        public string PrefabFile;
        [ProtoMember(0xd0), XmlAttribute]
        public string Name;
        [ProtoMember(0xd3)]
        public SerializableVector3 Position;
    }
}

