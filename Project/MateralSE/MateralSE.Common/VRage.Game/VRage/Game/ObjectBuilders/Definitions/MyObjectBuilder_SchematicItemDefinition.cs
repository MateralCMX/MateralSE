namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SchematicItemDefinition : MyObjectBuilder_UsableItemDefinition
    {
        [ProtoMember(0x11)]
        public SerializableDefinitionId? Research;
    }
}

