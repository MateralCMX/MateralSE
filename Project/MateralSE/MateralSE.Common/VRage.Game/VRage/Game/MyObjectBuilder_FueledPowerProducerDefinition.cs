namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FueledPowerProducerDefinition : MyObjectBuilder_PowerProducerDefinition
    {
        [ProtoMember(0x10)]
        public float FuelProductionToCapacityMultiplier = 3600f;
        [ProtoMember(0x13)]
        public List<FuelInfo> FuelInfos;
        [ProtoMember(0x16)]
        public string ResourceSinkGroup;

        [ProtoContract]
        public class FuelInfo
        {
            [ProtoMember(0x1c)]
            public float Ratio = 1f;
            [ProtoMember(0x1f)]
            public SerializableDefinitionId Id;
        }
    }
}

