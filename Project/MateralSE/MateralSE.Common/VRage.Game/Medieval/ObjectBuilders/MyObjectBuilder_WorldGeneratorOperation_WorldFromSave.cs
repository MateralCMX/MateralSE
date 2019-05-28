namespace Medieval.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("WorldFromSave"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_WorldFromSave : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0x26), XmlAttribute]
        public string PrefabDirectory;
    }
}

