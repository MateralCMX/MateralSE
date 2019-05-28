namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PoweredCargoContainerDefinition : MyObjectBuilder_CargoContainerDefinition
    {
        [ProtoMember(11)]
        public string ResourceSinkGroup;
        [ProtoMember(14)]
        public float RequiredPowerInput;
    }
}

