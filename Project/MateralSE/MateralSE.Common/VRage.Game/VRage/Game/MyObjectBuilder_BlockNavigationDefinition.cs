namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BlockNavigationDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x13), XmlArrayItem("Triangle")]
        public Triangle[] Triangles;
        [ProtoMember(0x16)]
        public bool NoEntry;
        [ProtoMember(0x19)]
        public SerializableVector3I Size = new SerializableVector3I(1, 1, 1);
        [ProtoMember(0x1c)]
        public SerializableVector3I Center = new SerializableVector3I(0, 0, 0);

        [ProtoContract]
        public class Triangle
        {
            [ProtoMember(15), XmlArrayItem("Point")]
            public SerializableVector3[] Points;
        }
    }
}

