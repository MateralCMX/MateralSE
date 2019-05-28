namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlType("MyBBMemoryFloat"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyBBMemoryFloat : MyBBMemoryValue
    {
        [ProtoMember(10)]
        public float FloatValue;
    }
}

