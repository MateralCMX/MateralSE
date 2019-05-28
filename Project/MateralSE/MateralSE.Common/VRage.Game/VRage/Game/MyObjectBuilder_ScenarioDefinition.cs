namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("ScenarioDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ScenarioDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(15)]
        public SerializableDefinitionId GameDefinition = ((SerializableDefinitionId) MyGameDefinition.Default);
        [ProtoMember(0x12)]
        public SerializableDefinitionId EnvironmentDefinition = new SerializableDefinitionId(typeof(MyObjectBuilder_EnvironmentDefinition), "Default");
        [ProtoMember(0x15)]
        public AsteroidClustersSettings AsteroidClusters;
        [ProtoMember(0x18)]
        public MyEnvironmentHostilityEnum DefaultEnvironment = MyEnvironmentHostilityEnum.NORMAL;
        [ProtoMember(0x1b), XmlArrayItem("StartingState", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_WorldGeneratorPlayerStartingState>))]
        public MyObjectBuilder_WorldGeneratorPlayerStartingState[] PossibleStartingStates;
        [ProtoMember(0x1f), XmlArrayItem("Operation", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_WorldGeneratorOperation>))]
        public MyObjectBuilder_WorldGeneratorOperation[] WorldGeneratorOperations;
        [ProtoMember(0x23), XmlArrayItem("Weapon")]
        public string[] CreativeModeWeapons;
        [ProtoMember(0x27), XmlArrayItem("Component")]
        public StartingItem[] CreativeModeComponents;
        [ProtoMember(0x2b), XmlArrayItem("PhysicalItem")]
        public StartingPhysicalItem[] CreativeModePhysicalItems;
        [ProtoMember(0x2f), XmlArrayItem("AmmoItem")]
        public StartingItem[] CreativeModeAmmoItems;
        [ProtoMember(0x33), XmlArrayItem("Weapon")]
        public string[] SurvivalModeWeapons;
        [ProtoMember(0x37), XmlArrayItem("Component")]
        public StartingItem[] SurvivalModeComponents;
        [ProtoMember(0x3b), XmlArrayItem("PhysicalItem")]
        public StartingPhysicalItem[] SurvivalModePhysicalItems;
        [ProtoMember(0x3f), XmlArrayItem("AmmoItem")]
        public StartingItem[] SurvivalModeAmmoItems;
        [ProtoMember(0x43)]
        public MyObjectBuilder_InventoryItem[] CreativeInventoryItems;
        [ProtoMember(70)]
        public MyObjectBuilder_InventoryItem[] SurvivalInventoryItems;
        [ProtoMember(0x49)]
        public SerializableBoundingBoxD? WorldBoundaries;
        private MyObjectBuilder_Toolbar m_creativeDefaultToolbar;
        [ProtoMember(0x5c)]
        public MyObjectBuilder_Toolbar SurvivalDefaultToolbar;
        [ProtoMember(0x5f)]
        public MyOBBattleSettings Battle;
        [ProtoMember(0x62)]
        public string MainCharacterModel;
        [ProtoMember(0x65)]
        public long GameDate = 0x91bf304a5d29800L;
        [ProtoMember(0x68)]
        public SerializableVector3 SunDirection = Vector3.Invalid;

        public bool ShouldSerializeDefaultToolbar() => 
            false;

        [ProtoMember(0x4c)]
        public MyObjectBuilder_Toolbar DefaultToolbar
        {
            get => 
                null;
            set => 
                (this.CreativeDefaultToolbar = this.SurvivalDefaultToolbar = value);
        }

        [ProtoMember(0x54)]
        public MyObjectBuilder_Toolbar CreativeDefaultToolbar
        {
            get => 
                this.m_creativeDefaultToolbar;
            set => 
                (this.m_creativeDefaultToolbar = value);
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct AsteroidClustersSettings
        {
            [ProtoMember(110), XmlAttribute]
            public bool Enabled;
            [ProtoMember(0x71), XmlAttribute]
            public float Offset;
            [ProtoMember(0x75), XmlAttribute]
            public bool CentralCluster;
            public bool ShouldSerializeOffset() => 
                this.Enabled;

            public bool ShouldSerializeCentralCluster() => 
                this.Enabled;
        }

        [ProtoContract]
        public class MyOBBattleSettings
        {
            [ProtoMember(0x94), XmlArrayItem("Slot")]
            public SerializableBoundingBoxD[] AttackerSlots;
            [ProtoMember(0x98)]
            public SerializableBoundingBoxD DefenderSlot;
            [ProtoMember(0x9b)]
            public long DefenderEntityId;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct StartingItem
        {
            [ProtoMember(0x7d), XmlAttribute]
            public float amount;
            [ProtoMember(0x80), XmlText]
            public string itemName;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct StartingPhysicalItem
        {
            [ProtoMember(0x87), XmlAttribute]
            public float amount;
            [ProtoMember(0x8a), XmlText]
            public string itemName;
            [ProtoMember(0x8d), XmlAttribute]
            public string itemType;
        }
    }
}

