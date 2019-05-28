namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using VRage.ObjectBuilders;

    [ProtoContract]
    public class MyFuelConverterInfo
    {
        [ProtoMember(10)]
        public SerializableDefinitionId FuelId;
        [ProtoMember(13)]
        public float Efficiency = 1f;
    }
}

