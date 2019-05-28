namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BattleSystemComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(12)]
        public bool IsCastleSiegeMap;
        [ProtoMember(15)]
        public ulong Points;
        [ProtoMember(0x12)]
        public ulong BaseMapVoxelHandVolumeChanged;
        [ProtoMember(0x15)]
        public ulong BaseMapSmallGridsPoints;
        [ProtoMember(0x18)]
        public ulong BaseMapLargeGridsPoints;
        [ProtoMember(0x1b)]
        public SerializableBoundingSphereD[] AttackerSlots;
        [ProtoMember(30)]
        public SerializableBoundingSphereD DefenderSlot;
        [ProtoMember(0x21)]
        public long Faction1EntityId;
    }
}

