namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GasFueledPowerProducerDefinition : MyObjectBuilder_FueledPowerProducerDefinition
    {
        [ProtoMember(0x10)]
        public float FuelCapacity = 100f;
    }
}

