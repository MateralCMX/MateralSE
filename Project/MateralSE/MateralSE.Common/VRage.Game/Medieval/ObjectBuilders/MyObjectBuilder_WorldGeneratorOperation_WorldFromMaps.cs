namespace Medieval.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("WorldFromMaps"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_WorldFromMaps : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x36), XmlAttribute]
        public string Name;
        [ProtoMember(0x39)]
        public SerializableVector3 Size;
        [ProtoMember(60)]
        public string HeightMapFile;
        [ProtoMember(0x3f)]
        public string BiomeMapFile;
        [ProtoMember(0x42)]
        public string TreeMapFile;
        [ProtoMember(0x45)]
        public string TreeMaskFile;
    }
}

