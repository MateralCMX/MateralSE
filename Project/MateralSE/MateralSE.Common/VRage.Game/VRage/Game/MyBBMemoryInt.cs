namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("MyBBMemoryInt"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyBBMemoryInt : MyBBMemoryValue
    {
        [ProtoMember(10)]
        public int IntValue;
    }
}

