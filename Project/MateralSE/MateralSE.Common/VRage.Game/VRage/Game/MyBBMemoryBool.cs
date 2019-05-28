namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("MyBBMemoryBool"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyBBMemoryBool : MyBBMemoryValue
    {
        [ProtoMember(10)]
        public bool BoolValue;
    }
}

