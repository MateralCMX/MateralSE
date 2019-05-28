namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ComponentSubstitutionDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x16)]
        public SerializableDefinitionId RequiredComponentId;
        [XmlArrayItem("ProvidingComponent"), ProtoMember(0x1a)]
        public ProvidingComponent[] ProvidingComponents;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct ProvidingComponent
        {
            [ProtoMember(15)]
            public SerializableDefinitionId Id;
            [ProtoMember(0x12)]
            public int Amount;
        }
    }
}

