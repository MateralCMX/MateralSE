namespace Medieval.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers"), XmlType("Range")]
    public class MyObjectBuilder_WorldGeneratorPlayerStartingState_Range : MyObjectBuilder_WorldGeneratorPlayerStartingState
    {
        [ProtoMember(14)]
        public SerializableVector3 MinPosition;
        [ProtoMember(0x11)]
        public SerializableVector3 MaxPosition;
    }
}

