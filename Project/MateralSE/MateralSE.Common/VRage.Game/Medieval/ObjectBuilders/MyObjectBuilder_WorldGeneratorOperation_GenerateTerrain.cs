namespace Medieval.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("GenerateTerrain"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_GenerateTerrain : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x1a), XmlAttribute]
        public string Name;
        [ProtoMember(0x1d)]
        public SerializableVector3 Size;
    }
}

