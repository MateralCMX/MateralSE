namespace Medieval.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("GenerateStatues"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_GenerateStatues : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x4f), XmlAttribute]
        public int Count;
    }
}

