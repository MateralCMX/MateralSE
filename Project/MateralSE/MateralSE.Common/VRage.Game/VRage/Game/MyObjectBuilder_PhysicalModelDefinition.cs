namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("VR.PhysicalModelDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PhysicalModelDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(14), ModdableContentFile("mwm")]
        public string Model;
        [ProtoMember(0x12)]
        public string PhysicalMaterial;
        [ProtoMember(0x15)]
        public float Mass;
    }
}

