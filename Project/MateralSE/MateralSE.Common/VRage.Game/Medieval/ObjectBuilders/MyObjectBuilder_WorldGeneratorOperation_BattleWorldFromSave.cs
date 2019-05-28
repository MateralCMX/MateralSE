namespace Medieval.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("BattleWorldFromSave"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WorldGeneratorOperation_BattleWorldFromSave : MyObjectBuilder_WorldGeneratorOperation_WorldFromSave
    {
    }
}

