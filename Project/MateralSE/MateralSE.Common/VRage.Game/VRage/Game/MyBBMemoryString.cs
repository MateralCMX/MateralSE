namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("MyBBMemoryString"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyBBMemoryString : MyBBMemoryValue
    {
        [ProtoMember(10)]
        public string StringValue;
    }
}

