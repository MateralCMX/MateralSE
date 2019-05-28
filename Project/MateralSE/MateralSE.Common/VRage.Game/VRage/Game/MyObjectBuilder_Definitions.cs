namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [XmlRoot("Definitions"), ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Definitions : MyObjectBuilder_Base
    {
        [XmlElement("Definition", Type=typeof(MyDefinitionXmlSerializer))]
        public MyObjectBuilder_DefinitionBase[] Definitions;
        [XmlArrayItem("GridCreator"), ProtoMember(0x13)]
        public MyObjectBuilder_GridCreateToolDefinition[] GridCreators;
        [XmlArrayItem("AmmoMagazine", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AmmoMagazineDefinition>)), ProtoMember(0x17)]
        public MyObjectBuilder_AmmoMagazineDefinition[] AmmoMagazines;
        [XmlArrayItem("Blueprint", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BlueprintDefinition>)), ProtoMember(0x1b)]
        public MyObjectBuilder_BlueprintDefinition[] Blueprints;
        [XmlArrayItem("Component", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ComponentDefinition>)), ProtoMember(0x1f)]
        public MyObjectBuilder_ComponentDefinition[] Components;
        [XmlArrayItem("ContainerType", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ContainerTypeDefinition>)), ProtoMember(0x23)]
        public MyObjectBuilder_ContainerTypeDefinition[] ContainerTypes;
        [XmlArrayItem("Definition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CubeBlockDefinition>)), ProtoMember(0x27)]
        public MyObjectBuilder_CubeBlockDefinition[] CubeBlocks;
        [XmlArrayItem("BlockPosition"), ProtoMember(0x2b)]
        public MyBlockPosition[] BlockPositions;
        [ProtoMember(0x2e), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_Configuration>))]
        public MyObjectBuilder_Configuration Configuration;
        [ProtoMember(50), XmlElement("Environment", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EnvironmentDefinition>))]
        public MyObjectBuilder_EnvironmentDefinition[] Environments;
        [XmlArrayItem("GlobalEvent", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_GlobalEventDefinition>)), ProtoMember(0x37)]
        public MyObjectBuilder_GlobalEventDefinition[] GlobalEvents;
        [XmlArrayItem("HandItem", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_HandItemDefinition>)), ProtoMember(0x3b)]
        public MyObjectBuilder_HandItemDefinition[] HandItems;
        [XmlArrayItem("PhysicalItem", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PhysicalItemDefinition>)), ProtoMember(0x3f)]
        public MyObjectBuilder_PhysicalItemDefinition[] PhysicalItems;
        [XmlArrayItem("SpawnGroup", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_SpawnGroupDefinition>)), ProtoMember(0x43)]
        public MyObjectBuilder_SpawnGroupDefinition[] SpawnGroups;
        [XmlArrayItem("TransparentMaterial", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_TransparentMaterialDefinition>)), ProtoMember(0x47)]
        public MyObjectBuilder_TransparentMaterialDefinition[] TransparentMaterials;
        [XmlArrayItem("VoxelMaterial", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_VoxelMaterialDefinition>)), ProtoMember(0x4b)]
        public MyObjectBuilder_VoxelMaterialDefinition[] VoxelMaterials;
        [XmlArrayItem("Character", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CharacterDefinition>)), ProtoMember(0x4f)]
        public MyObjectBuilder_CharacterDefinition[] Characters;
        [XmlArrayItem("Animation", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AnimationDefinition>)), ProtoMember(0x53)]
        public MyObjectBuilder_AnimationDefinition[] Animations;
        [XmlArrayItem("Debris", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_DebrisDefinition>)), ProtoMember(0x57)]
        public MyObjectBuilder_DebrisDefinition[] Debris;
        [XmlArrayItem("Edges", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EdgesDefinition>)), ProtoMember(0x5b)]
        public MyObjectBuilder_EdgesDefinition[] Edges;
        [XmlArrayItem("Faction", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_FactionDefinition>)), ProtoMember(0x5f)]
        public MyObjectBuilder_FactionDefinition[] Factions;
        [XmlArrayItem("Prefab", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PrefabDefinition>)), ProtoMember(0x63)]
        public MyObjectBuilder_PrefabDefinition[] Prefabs;
        [XmlArrayItem("Class"), ProtoMember(0x67)]
        public MyObjectBuilder_BlueprintClassDefinition[] BlueprintClasses;
        [XmlArrayItem("Entry"), ProtoMember(0x6b)]
        public BlueprintClassEntry[] BlueprintClassEntries;
        [XmlArrayItem("EnvironmentItem", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EnvironmentItemDefinition>)), ProtoMember(0x6f)]
        public MyObjectBuilder_EnvironmentItemDefinition[] EnvironmentItems;
        [XmlArrayItem("Template", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CompoundBlockTemplateDefinition>)), ProtoMember(0x73)]
        public MyObjectBuilder_CompoundBlockTemplateDefinition[] CompoundBlockTemplates;
        [XmlArrayItem("Ship", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_RespawnShipDefinition>)), ProtoMember(0x77)]
        public MyObjectBuilder_RespawnShipDefinition[] RespawnShips;
        [XmlArrayItem("DropContainer", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_DropContainerDefinition>)), ProtoMember(0x7b)]
        public MyObjectBuilder_DropContainerDefinition[] DropContainers;
        [XmlArrayItem("WheelModel", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_WheelModelsDefinition>)), ProtoMember(0x7f)]
        public MyObjectBuilder_WheelModelsDefinition[] WheelModels;
        [XmlArrayItem("AsteroidGenerator", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AsteroidGeneratorDefinition>)), ProtoMember(0x83)]
        public MyObjectBuilder_AsteroidGeneratorDefinition[] AsteroidGenerators;
        [XmlArrayItem("Category", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_GuiBlockCategoryDefinition>)), ProtoMember(0x87)]
        public MyObjectBuilder_GuiBlockCategoryDefinition[] CategoryClasses;
        [XmlArrayItem("ShipBlueprint", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ShipBlueprintDefinition>)), ProtoMember(0x8b)]
        public MyObjectBuilder_ShipBlueprintDefinition[] ShipBlueprints;
        [XmlArrayItem("Weapon", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_WeaponDefinition>)), ProtoMember(0x8f)]
        public MyObjectBuilder_WeaponDefinition[] Weapons;
        [XmlArrayItem("Ammo", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AmmoDefinition>)), ProtoMember(0x93)]
        public MyObjectBuilder_AmmoDefinition[] Ammos;
        [XmlArrayItem("Sound", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AudioDefinition>)), ProtoMember(0x97)]
        public MyObjectBuilder_AudioDefinition[] Sounds;
        [XmlArrayItem("AssetModifier", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AssetModifierDefinition>)), ProtoMember(0x9b)]
        public MyObjectBuilder_AssetModifierDefinition[] AssetModifiers;
        [XmlArrayItem("MainMenuInventoryScene", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_MainMenuInventorySceneDefinition>)), ProtoMember(0x9f)]
        public MyObjectBuilder_MainMenuInventorySceneDefinition[] MainMenuInventoryScenes;
        [XmlArrayItem("VoxelHand", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_VoxelHandDefinition>)), ProtoMember(0xa3)]
        public MyObjectBuilder_VoxelHandDefinition[] VoxelHands;
        [XmlArrayItem("MultiBlock", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_MultiBlockDefinition>)), ProtoMember(0xa7)]
        public MyObjectBuilder_MultiBlockDefinition[] MultiBlocks;
        [XmlArrayItem("PrefabThrower", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PrefabThrowerDefinition>)), ProtoMember(0xab)]
        public MyObjectBuilder_PrefabThrowerDefinition[] PrefabThrowers;
        [XmlArrayItem("SoundCategory", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_SoundCategoryDefinition>)), ProtoMember(0xaf)]
        public MyObjectBuilder_SoundCategoryDefinition[] SoundCategories;
        [XmlArrayItem("ShipSoundGroup", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ShipSoundsDefinition>)), ProtoMember(0xb3)]
        public MyObjectBuilder_ShipSoundsDefinition[] ShipSoundGroups;
        [ProtoMember(0xb6), XmlArrayItem("DroneBehavior", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_DroneBehaviorDefinition>))]
        public MyObjectBuilder_DroneBehaviorDefinition[] DroneBehaviors;
        [XmlElement("ShipSoundSystem", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ShipSoundSystemDefinition>)), ProtoMember(0xbb)]
        public MyObjectBuilder_ShipSoundSystemDefinition ShipSoundSystem;
        [XmlArrayItem("ParticleEffect", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ParticleEffect>)), ProtoMember(0xbf)]
        public MyObjectBuilder_ParticleEffect[] ParticleEffects;
        [XmlArrayItem("AIBehavior", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BehaviorTreeDefinition>)), ProtoMember(0xc3)]
        public MyObjectBuilder_BehaviorTreeDefinition[] AIBehaviors;
        [XmlArrayItem("VoxelMapStorage", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_VoxelMapStorageDefinition>)), ProtoMember(0xc7)]
        public MyObjectBuilder_VoxelMapStorageDefinition[] VoxelMapStorages;
        [XmlArrayItem("LCDTextureDefinition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_LCDTextureDefinition>)), ProtoMember(0xcb)]
        public MyObjectBuilder_LCDTextureDefinition[] LCDTextures;
        [XmlArrayItem("Bot", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BotDefinition>)), ProtoMember(0xcf)]
        public MyObjectBuilder_BotDefinition[] Bots;
        [XmlArrayItem("Rope", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_RopeDefinition>)), ProtoMember(0xd3)]
        public MyObjectBuilder_RopeDefinition[] RopeTypes;
        [XmlArrayItem("PhysicalMaterial", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PhysicalMaterialDefinition>)), ProtoMember(0xd7)]
        public MyObjectBuilder_PhysicalMaterialDefinition[] PhysicalMaterials;
        [XmlArrayItem("AiCommand", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AiCommandDefinition>)), ProtoMember(0xdb)]
        public MyObjectBuilder_AiCommandDefinition[] AiCommands;
        [XmlArrayItem("NavDef", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BlockNavigationDefinition>)), ProtoMember(0xdf)]
        public MyObjectBuilder_BlockNavigationDefinition[] BlockNavigationDefinitions;
        [XmlArrayItem("Cutting", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CuttingDefinition>)), ProtoMember(0xe3)]
        public MyObjectBuilder_CuttingDefinition[] Cuttings;
        [XmlArrayItem("Properties", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_MaterialPropertiesDefinition>)), ProtoMember(0xe7)]
        public MyObjectBuilder_MaterialPropertiesDefinition[] MaterialProperties;
        [XmlArrayItem("ControllerSchema", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ControllerSchemaDefinition>)), ProtoMember(0xeb)]
        public MyObjectBuilder_ControllerSchemaDefinition[] ControllerSchemas;
        [XmlArrayItem("SoundCurve", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CurveDefinition>)), ProtoMember(0xef)]
        public MyObjectBuilder_CurveDefinition[] CurveDefinitions;
        [XmlArrayItem("Effect", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AudioEffectDefinition>)), ProtoMember(0xf3)]
        public MyObjectBuilder_AudioEffectDefinition[] AudioEffects;
        [XmlArrayItem("Definition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EnvironmentItemsDefinition>)), ProtoMember(0xf7)]
        public MyObjectBuilder_EnvironmentItemsDefinition[] EnvironmentItemsDefinitions;
        [XmlArrayItem("Entry"), ProtoMember(0xfb)]
        public EnvironmentItemsEntry[] EnvironmentItemsEntries;
        [XmlArrayItem("Definition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_AreaMarkerDefinition>)), ProtoMember(0xff)]
        public MyObjectBuilder_AreaMarkerDefinition[] AreaMarkerDefinitions;
        [XmlArrayItem("Entry"), ProtoMember(0x103)]
        public MyCharacterName[] CharacterNames;
        [ProtoMember(0x106), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BattleDefinition>))]
        public MyObjectBuilder_BattleDefinition Battle;
        [ProtoMember(0x10a)]
        public MyObjectBuilder_DecalGlobalsDefinition DecalGlobals;
        [XmlArrayItem("Decal"), ProtoMember(270)]
        public MyObjectBuilder_DecalDefinition[] Decals;
        [XmlArrayItem("EmissiveColor"), ProtoMember(0x112)]
        public MyObjectBuilder_EmissiveColorDefinition[] EmissiveColors;
        [XmlArrayItem("EmissiveColorStatePreset"), ProtoMember(0x116)]
        public MyObjectBuilder_EmissiveColorStatePresetDefinition[] EmissiveColorStatePresets;
        [XmlArrayItem("PlanetGeneratorDefinition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PlanetGeneratorDefinition>)), ProtoMember(0x11a)]
        public MyObjectBuilder_PlanetGeneratorDefinition[] PlanetGeneratorDefinitions;
        [XmlArrayItem("Definition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_FloraElementDefinition>)), ProtoMember(0x11e)]
        public MyObjectBuilder_FloraElementDefinition[] FloraElements;
        [XmlArrayItem("Stat"), ProtoMember(290)]
        public MyObjectBuilder_EntityStatDefinition[] StatDefinitions;
        [XmlArrayItem("Gas"), ProtoMember(0x126)]
        public MyObjectBuilder_GasProperties[] GasProperties;
        [XmlArrayItem("DistributionGroup"), ProtoMember(0x12a)]
        public MyObjectBuilder_ResourceDistributionGroup[] ResourceDistributionGroups;
        [XmlArrayItem("Group", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ComponentGroupDefinition>)), ProtoMember(0x12e)]
        public MyObjectBuilder_ComponentGroupDefinition[] ComponentGroups;
        [XmlArrayItem("Substitution"), ProtoMember(0x132)]
        public MyObjectBuilder_ComponentSubstitutionDefinition[] ComponentSubstitutions;
        [XmlArrayItem("Block"), ProtoMember(310)]
        public MyComponentBlockEntry[] ComponentBlocks;
        [XmlArrayItem("PlanetPrefab", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PlanetPrefabDefinition>)), ProtoMember(0x13a)]
        public MyObjectBuilder_PlanetPrefabDefinition[] PlanetPrefabs;
        [XmlArrayItem("Group"), ProtoMember(0x13e)]
        public MyGroupedIds[] EnvironmentGroups;
        [XmlArrayItem("Group", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ScriptedGroupDefinition>)), ProtoMember(0x142)]
        public MyObjectBuilder_ScriptedGroupDefinition[] ScriptedGroups;
        [XmlArrayItem("Map"), ProtoMember(0x146)]
        public MyMappedId[] ScriptedGroupsMap;
        [XmlArrayItem("Antenna", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PirateAntennaDefinition>)), ProtoMember(330)]
        public MyObjectBuilder_PirateAntennaDefinition[] PirateAntennas;
        [ProtoMember(0x14d)]
        public MyObjectBuilder_DestructionDefinition Destruction;
        [XmlArrayItem("EntityComponent", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ComponentDefinitionBase>)), ProtoMember(0x151)]
        public MyObjectBuilder_ComponentDefinitionBase[] EntityComponents;
        [XmlArrayItem("Container", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ContainerDefinition>)), ProtoMember(0x155)]
        public MyObjectBuilder_ContainerDefinition[] EntityContainers;
        [ProtoMember(0x158), XmlArrayItem("ShadowTextureSet")]
        public MyObjectBuilder_ShadowTextureSetDefinition[] ShadowTextureSets;
        [XmlArrayItem("Font", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_FontDefinition>)), ProtoMember(0x15d)]
        public MyObjectBuilder_FontDefinition[] Fonts;
        [ProtoMember(0x160), XmlArrayItem("Definition", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_FlareDefinition>))]
        public MyObjectBuilder_FlareDefinition[] Flares;
        [XmlArrayItem("ResearchBlock", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ResearchBlockDefinition>)), ProtoMember(0x165)]
        public MyObjectBuilder_ResearchBlockDefinition[] ResearchBlocks;
        [XmlArrayItem("ResearchGroup", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ResearchGroupDefinition>)), ProtoMember(0x169)]
        public MyObjectBuilder_ResearchGroupDefinition[] ResearchGroups;
    }
}

