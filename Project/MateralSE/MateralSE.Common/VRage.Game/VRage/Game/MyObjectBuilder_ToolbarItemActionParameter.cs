namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ToolbarItemActionParameter : MyObjectBuilder_Base
    {
        [ProtoMember(12)]
        public System.TypeCode TypeCode;
        [ProtoMember(15)]
        public string Value;
    }
}

