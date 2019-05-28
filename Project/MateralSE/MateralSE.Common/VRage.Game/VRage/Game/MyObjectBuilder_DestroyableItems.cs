namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), MyEnvironmentItems(typeof(MyObjectBuilder_DestroyableItem)), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DestroyableItems : MyObjectBuilder_EnvironmentItems
    {
    }
}

