namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CuttingDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x1c)]
        public SerializableDefinitionId EntityId;
        [ProtoMember(0x1f)]
        public SerializableDefinitionId ScrapWoodBranchesId;
        [ProtoMember(0x22)]
        public SerializableDefinitionId ScrapWoodId;
        [ProtoMember(0x25)]
        public int ScrapWoodAmountMin = 5;
        [ProtoMember(40)]
        public int ScrapWoodAmountMax = 7;
        [ProtoMember(0x2b)]
        public int CraftingScrapWoodAmountMin = 1;
        [ProtoMember(0x2e)]
        public int CraftingScrapWoodAmountMax = 3;
        [XmlArrayItem("CuttingPrefab"), ProtoMember(50), DefaultValue((string) null)]
        public MyCuttingPrefab[] CuttingPrefabs;
        [ProtoMember(0x35)]
        public bool DestroySourceAfterCrafting = true;
        [ProtoMember(0x38)]
        public float CraftingTimeInSeconds = 0.5f;

        [ProtoContract]
        public class MyCuttingPrefab
        {
            [ProtoMember(0x12), DefaultValue((string) null)]
            public string Prefab;
            [ProtoMember(0x15), DefaultValue(1)]
            public int SpawnCount = 1;
            [ProtoMember(0x18), DefaultValue((string) null)]
            public SerializableDefinitionId? PhysicalItemId;
        }
    }
}

