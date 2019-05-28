namespace Medieval.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("TestTrees"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_TestTrees : MyObjectBuilder_WorldGeneratorOperation
    {
    }
}

