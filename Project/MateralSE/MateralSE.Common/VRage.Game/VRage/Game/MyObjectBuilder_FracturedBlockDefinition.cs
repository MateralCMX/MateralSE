namespace VRage.Game
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FracturedBlockDefinition : MyObjectBuilder_CubeBlockDefinition
    {
    }
}

