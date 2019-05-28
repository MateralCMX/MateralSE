namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("AddObjectsPrefab")]
    public class MyObjectBuilder_WorldGeneratorOperation_AddObjectsPrefab : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0xdb), XmlAttribute]
        public string PrefabFile;
    }
}

