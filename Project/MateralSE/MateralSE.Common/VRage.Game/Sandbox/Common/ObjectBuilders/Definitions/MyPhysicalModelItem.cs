namespace Sandbox.Common.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyPhysicalModelItem
    {
        [ProtoMember(13), XmlAttribute(AttributeName="TypeId")]
        public string TypeId;
        [ProtoMember(0x11), XmlAttribute(AttributeName="SubtypeId")]
        public string SubtypeId;
        [ProtoMember(0x15), XmlAttribute(AttributeName="Weight")]
        public float Weight = 1f;
    }
}

