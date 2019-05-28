namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TreeObject : MyObjectBuilder_PhysicalObject
    {
        public override bool CanStack(MyObjectBuilder_PhysicalObject a) => 
            false;
    }
}

