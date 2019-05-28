namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;

    [ProtoContract(SkipConstructor=true), MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionSettings : MyObjectBuilder_Base
    {
        [XmlIgnore]
        public const uint DEFAULT_AUTOSAVE_IN_MINUTES = 5;
        [ProtoMember(0x17), Display(Name="Game Mode", Description="The type of the game mode."), Category("Others"), GameRelation(VRage.Game.Game.Shared)]
        public MyGameModeEnum GameMode;
        [ProtoMember(0x1d), Display(Name="Characters Inventory Size", Description="The multiplier for inventory size for the characters."), Category("Multipliers"), GameRelation(VRage.Game.Game.Shared), Range(1, 100)]
        public float InventorySizeMultiplier = 3f;
        [ProtoMember(0x24), Display(Name="Blocks Inventory Size", Description="The multiplier for inventory size for the blocks."), Category("Multipliers"), GameRelation(VRage.Game.Game.Shared), Range(1, 100)]
        public float BlocksInventorySizeMultiplier = 1f;
        [ProtoMember(0x2b), Display(Name="Assembler Speed", Description="The multiplier for assembler speed."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(1, 100)]
        public float AssemblerSpeedMultiplier = 3f;
        [ProtoMember(50), Display(Name="Assembler Efficiency", Description="The multiplier for assembler efficiency."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(1, 100)]
        public float AssemblerEfficiencyMultiplier = 3f;
        [ProtoMember(0x39), Display(Name="Refinery Speed", Description="The multiplier for refinery speed."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(1, 100)]
        public float RefinerySpeedMultiplier = 3f;
        [ProtoMember(0x40)]
        public MyOnlineModeEnum OnlineMode;
        [ProtoMember(0x43), Display(Name="Max Players", Description="The maximum number of connected players."), GameRelation(VRage.Game.Game.Shared), Category("Players"), Range(2, 0x40)]
        public short MaxPlayers = 4;
        [ProtoMember(0x4a), Display(Name="Max Floating Objects", Description="The maximum number of existing floating objects."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(2, 0x400), Category("Environment")]
        public short MaxFloatingObjects = 100;
        [ProtoMember(0x51), Display(Name="Max Backup Saves", Description="The maximum number of backup saves."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x3e8), Category("Others")]
        public short MaxBackupSaves = 5;
        [ProtoMember(0x58), Display(Name="Max Grid Blocks", Description="The maximum number of blocks in one grid."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x7fffffff), Category("Block Limits")]
        public int MaxGridSize = 0xc350;
        [ProtoMember(0x5f), Display(Name="Max Blocks per Player", Description="The maximum number of blocks per player."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x7fffffff), Category("Block Limits")]
        public int MaxBlocksPerPlayer = 0x186a0;
        [ProtoMember(0x66), Display(Name="World PCU", Description="The total number of Performance Cost Units in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x7fffffff), Category("Block Limits")]
        public int TotalPCU = 0x927c0;
        [ProtoMember(0x6d), Display(Name="Pirate PCU", Description="Number of Performance Cost Units allocated for pirate faction."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x7fffffff), Category("Block Limits")]
        public int PiratePCU = 0xc350;
        [ProtoMember(0x74), Display(Name="Max Factions Count", Description="The maximum number of existing factions in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 0x7fffffff), Category("Block Limits")]
        public int MaxFactionsCount;
        [ProtoMember(0x7b), Display(Name="Block Limits Mode", Description="Defines block limits mode."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Block Limits")]
        public MyBlockLimitsEnabledEnum BlockLimitsEnabled;
        [ProtoMember(0x81), Display(Name="Enable Remote Grid Removal", Description="Enables possibility to remove grid remotely from the world by an author."), Category("Others"), GameRelation(VRage.Game.Game.SpaceEngineers)]
        public bool EnableRemoteBlockRemoval = true;
        [ProtoMember(0x87), Display(Name="Environment Hostility", Description="Defines hostility of the environment."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public MyEnvironmentHostilityEnum EnvironmentHostility = MyEnvironmentHostilityEnum.NORMAL;
        [ProtoMember(0x8e), Display(Name="Auto Healing", Description="Auto-healing heals players only in oxygen environments and during periods of not taking damage."), Category("Players"), GameRelation(VRage.Game.Game.SpaceEngineers)]
        public bool AutoHealing = true;
        [ProtoMember(0x94), Display(Name="Enable Copy & Paste", Description="Enables copy and paste feature."), GameRelation(VRage.Game.Game.Shared), Category("Players")]
        public bool EnableCopyPaste = true;
        [ProtoMember(0xa2), Display(Name="Enable Weapons", Description="Enables weapons."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool WeaponsEnabled = true;
        [ProtoMember(0xa8), Display(Name="Show Player Names on HUD", Description=""), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool ShowPlayerNamesOnHud = true;
        [ProtoMember(0xae), Display(Name="Enable Thruster Damage", Description="Enables thruster damage."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool ThrusterDamage = true;
        [ProtoMember(180), Display(Name="Enable Cargo Ships", Description="Enables spawning of cargo ships."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("NPCs")]
        public bool CargoShipsEnabled = true;
        [ProtoMember(0xba), Display(Name="Enable Spectator Camera", Description="Enables spectator camera."), GameRelation(VRage.Game.Game.Shared), Category("Others")]
        public bool EnableSpectator;
        [ProtoMember(0xc5), Display(Name="World Size [km]", Description="Defines the size of the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(0, 0x7fffffff)]
        public int WorldSizeKm;
        [ProtoMember(0xcc), Display(Name="Remove Respawn Ships on Logoff", Description="When enabled respawn ship is removed after player logout."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool RespawnShipDelete;
        [ProtoMember(210), Display(Name="Reset Ownership", Description=""), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool ResetOwnership;
        [ProtoMember(0xd8), Display(Name="Welder Speed", Description="The multiplier for welder speed."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 100)]
        public float WelderSpeedMultiplier = 2f;
        [ProtoMember(0xdf), Display(Name="Grinder Speed", Description="The multiplier for grinder speed."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 100)]
        public float GrinderSpeedMultiplier = 2f;
        [ProtoMember(230), Display(Name="Enable Realistic Sound", Description="Enables realistic sounds."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool RealisticSound;
        [ProtoMember(0xf3), Display(Name="Hacking Speed", Description="The multiplier for hacking speed."), Category("Multipliers"), GameRelation(VRage.Game.Game.SpaceEngineers), Range(0, 100)]
        public float HackSpeedMultiplier = 0.33f;
        [ProtoMember(250), Display(Name="Permanent Death", Description="Enables permanent death."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool? PermanentDeath = false;
        [ProtoMember(0x100), Display(Name="Autosave Interval [mins]", Description="Defines autosave interval."), GameRelation(VRage.Game.Game.Shared), Category("Others"), Range((double) 0.0, (double) 4294967295)]
        public uint AutoSaveInMinutes = 5;
        [ProtoMember(0x107), Display(Name="Enable Saving from Menu", Description="Enables saving from the menu."), GameRelation(VRage.Game.Game.Shared), Category("Others")]
        public bool EnableSaving = true;
        [ProtoMember(0x10d), Display(Name="Enable Infinite Ammunition in Survival", Description="Enables infinite ammunition in survival game mode."), GameRelation(VRage.Game.Game.Shared), Category("Others")]
        public bool InfiniteAmmo;
        [ProtoMember(0x113), Display(Name="Enable Drop Containers", Description="Enables drop containers (unknown signals)."), GameRelation(VRage.Game.Game.Shared), Category("Others")]
        public bool EnableContainerDrops = true;
        [ProtoMember(0x119), Display(Name="Respawn Ship Time Multiplier", Description="The multiplier for respawn ship timer."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players"), Range(0, 100)]
        public float SpawnShipTimeMultiplier;
        [ProtoMember(0x120), Display(Name="Procedural Density", Description="Defines density of the procedurally generated content."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(0, 1)]
        public float ProceduralDensity;
        [ProtoMember(0x128), Display(Name="Procedural Seed", Description="Defines unique starting seed for the procedurally generated content."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(-2147483648, 0x7fffffff)]
        public int ProceduralSeed;
        [ProtoMember(0x130), Display(Name="Enable Destructible Blocks", Description="Enables destruction feature for the blocks."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool DestructibleBlocks = true;
        [ProtoMember(310), Display(Name="Enable Ingame Scripts", Description="Enables in game scripts."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool EnableIngameScripts = true;
        [ProtoMember(0x13c), Display(Name="View Distance"), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(5, 0xc350), Browsable(false)]
        public int ViewDistance = 0x3a98;
        [ProtoMember(0x145), DefaultValue(false), Display(Name="Enable Tool Shake", Description="Enables tool shake feature."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool EnableToolShake;
        [ProtoMember(0x14d), Display(Name="Voxel Generator Version", Description=""), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(0, 100)]
        public int VoxelGeneratorVersion = 4;
        [ProtoMember(340), Display(Name="Enable Oxygen", Description="Enables oxygen in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableOxygen;
        [ProtoMember(0x15a), Display(Name="Enable Airtightness", Description="Enables airtightness in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableOxygenPressurization;
        [ProtoMember(0x160), Display(Name="Enable 3rd Person Camera", Description="Enables 3rd person camera."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool Enable3rdPersonView = true;
        [ProtoMember(0x166), Display(Name="Enable Encounters", Description="Enables random encounters in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("NPCs")]
        public bool EnableEncounters = true;
        [ProtoMember(0x16c), Display(Name="Enable Convert to Station", Description="Enables possibility of converting grid to station."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool EnableConvertToStation = true;
        [ProtoMember(370), Display(Name="Enable Station Grid with Voxel", Description="Enables possibility of station grid inside voxel."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool StationVoxelSupport;
        [ProtoMember(0x178), Display(Name="Enable Sun Rotation", Description="Enables sun rotation."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableSunRotation = true;
        [ProtoMember(0x17e), Display(Name="Enable Respawn Ships", Description="Enables respawn ships."), GameRelation(VRage.Game.Game.Shared), Category("Others")]
        public bool EnableRespawnShips = true;
        [ProtoMember(0x184), Display(Name=""), GameRelation(VRage.Game.Game.SpaceEngineers), Browsable(false)]
        public bool ScenarioEditMode;
        [ProtoMember(0x18a), Display(Name=""), GameRelation(VRage.Game.Game.SpaceEngineers), Browsable(false)]
        public bool Scenario;
        [ProtoMember(400), Display(Name=""), GameRelation(VRage.Game.Game.SpaceEngineers), Browsable(false)]
        public bool CanJoinRunning;
        [ProtoMember(0x196), Display(Name="Physics Iterations", Description=""), Category("Environment"), Range(2, 0x20)]
        public int PhysicsIterations = 8;
        [ProtoMember(0x19c), Display(Name="Sun Rotation Interval", Description="Defines interval of one rotation of the sun."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(0, 0x5a0)]
        public float SunRotationIntervalMinutes = 120f;
        [ProtoMember(0x1a3), Display(Name="Enable Jetpack", Description="Enables jetpack."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool EnableJetpack = true;
        [ProtoMember(0x1a9), Display(Name="Spawn with Tools", Description="Enables spawning with tools in the inventory."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Players")]
        public bool SpawnWithTools = true;
        [ProtoMember(0x1af), Display(Name=""), GameRelation(VRage.Game.Game.SpaceEngineers), Browsable(false)]
        public bool StartInRespawnScreen;
        [ProtoMember(0x1b5), Display(Name="Enable Voxel Destruction", Description="Enables voxel destructions."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableVoxelDestruction = true;
        [ProtoMember(0x1bb), Display(Name=""), GameRelation(VRage.Game.Game.SpaceEngineers), Browsable(false)]
        public int MaxDrones = 5;
        [ProtoMember(0x1c1), Display(Name="Enable Drones", Description="Enables spawning of drones in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("NPCs")]
        public bool EnableDrones = true;
        [ProtoMember(0x1c7), Display(Name="Enable Wolves", Description="Enables spawning of wolves in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("NPCs")]
        public bool EnableWolfs = true;
        [ProtoMember(0x1cd), Display(Name="Enable Spiders", Description="Enables spawning of spiders in the world."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("NPCs")]
        public bool EnableSpiders;
        [ProtoMember(0x1d3), Display(Name="Flora Density Multiplier", Description=""), GameRelation(VRage.Game.Game.Shared), Category("Environment"), Range(0, 100), Browsable(false)]
        public float FloraDensityMultiplier = 1f;
        [ProtoMember(0x1db), Display(Name="Enable Structural Simulation"), GameRelation(VRage.Game.Game.MedievalEngineers)]
        public bool EnableStructuralSimulation;
        [ProtoMember(480), Display(Name="Max Active Fracture Pieces"), GameRelation(VRage.Game.Game.MedievalEngineers), Range(0, 0x7fffffff)]
        public int MaxActiveFracturePieces = 50;
        [ProtoMember(0x1e6), Display(Name="Block Type World Limits"), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Block Limits")]
        public SerializableDictionary<string, short> BlockTypeLimits;
        [ProtoMember(0x1fb), Display(Name="Enable Scripter Role", Description="Enables scripter role for administration."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool EnableScripterRole;
        [ProtoMember(0x201, IsRequired=false), Display(Name="Min Drop Container Respawn Time", Description="Defines minimum respawn time for drop containers."), GameRelation(VRage.Game.Game.Shared), Category("Others"), Range(0, 100)]
        public int MinDropContainerRespawnTime;
        [ProtoMember(520, IsRequired=false), Display(Name="Max Drop Container Respawn Time", Description="Defines maximum respawn time for drop containers."), GameRelation(VRage.Game.Game.Shared), Category("Others"), Range(0, 100)]
        public int MaxDropContainerRespawnTime;
        [ProtoMember(0x20f, IsRequired=false), Display(Name="Enable Turrets Friendly Fire", Description="Enables friendly fire for turrets."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableTurretsFriendlyFire;
        [ProtoMember(0x215, IsRequired=false), Display(Name="Enable Sub-Grid Damage", Description="Enables sub-grid damage."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment")]
        public bool EnableSubgridDamage;
        [ProtoMember(0x21b, IsRequired=false), Display(Name="Sync Distance", Description="Defines synchronization distance in multiplayer. High distance can slow down server drastically. Use with caution."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Environment"), Range(0x3e8, 0x4e20)]
        public int SyncDistance;
        [ProtoMember(0x222, IsRequired=false), Display(Name="Experimental Mode", Description="Enables experimental mode."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool ExperimentalMode;
        [ProtoMember(0x228, IsRequired=false), Display(Name="Adaptive Simulation Quality", Description="Enables adaptive simulation quality system. This system is useful if you have a lot of voxel deformations in the world and low simulation speed."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool AdaptiveSimulationQuality;
        [ProtoMember(0x22e, IsRequired=false), Display(Name="Enable voxel hand", Description="Enables voxel hand."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool EnableVoxelHand;
        [ProtoMember(0x234, IsRequired=false), Display(Name="Trash Removal Enabled", Description="Enables trash removal system."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public bool TrashRemovalEnabled;
        [ProtoMember(570, IsRequired=false), Display(Name="Trash Removal Flags", Description="Defines flags for trash removal system."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal"), MyFlagEnum(typeof(MyTrashRemovalFlags))]
        public int TrashFlagsValue;
        [ProtoMember(0x253, IsRequired=false), Display(Name="Block Count Threshold", Description="Defines block count threshold for trash removal system."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public int BlockCountThreshold;
        [ProtoMember(0x259, IsRequired=false), Display(Name="Player Distance Threshold [m]", Description="Defines player distance threshold for trash removal system."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public float PlayerDistanceThreshold;
        [ProtoMember(0x25f, IsRequired=false), Display(Name="Optimal Grid Count", Description="By setting this, server will keep number of grids around this value. \n !WARNING! It ignores Powered and Fixed flags, Block Count and lowers Distance from player.\n Set to 0 to disable."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public int OptimalGridCount;
        [ProtoMember(0x265, IsRequired=false), Display(Name="Player Inactivity Threshold [hours]", Description="Defines player inactivity (time from logout) threshold for trash removal system. \n !WARNING! This will remove all grids of the player.\n Set to 0 to disable."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public float PlayerInactivityThreshold;
        [ProtoMember(0x26b, IsRequired=false), Display(Name="Character Removal Threshold [mins]", Description="Defines character removal threshold for trash removal system. If player disconnects it will remove his character after this time.\n Set to 0 to disable."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public int PlayerCharacterRemovalThreshold;
        [ProtoMember(0x271, IsRequired=false), Display(Name="Voxel Reverting Enabled", Description="Enables system for voxel reverting."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public bool VoxelTrashRemovalEnabled;
        [ProtoMember(0x277, IsRequired=false), Display(Name="Distance voxel from player (m)", Description="Only voxel chunks that are further from player will be reverted."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public float VoxelPlayerDistanceThreshold;
        [ProtoMember(0x27d, IsRequired=false), Display(Name="Distance voxel from grid (m)", Description="Only voxel chunks that are further from any grid will be reverted."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public float VoxelGridDistanceThreshold;
        [ProtoMember(0x283, IsRequired=false), Display(Name="Voxel age (min)", Description="Only voxel chunks that have been modified longer time age may be reverted."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Trash Removal")]
        public int VoxelAgeThreshold;
        [ProtoMember(0x289, IsRequired=false), Display(Name="Enable Progression", Description="Enables research progression."), GameRelation(VRage.Game.Game.SpaceEngineers), Category("Others")]
        public bool EnableResearch;
        [ProtoMember(0x28f, IsRequired=false), GameRelation(VRage.Game.Game.SpaceEngineers), Display(Name="Enable Good.bot Hints", Description="Enables Good.bot hints in the world. If user has disabled hints, this will not override that."), Category("Others")]
        public bool EnableGoodBotHints;
        [ProtoMember(0x295, IsRequired=false), GameRelation(VRage.Game.Game.SpaceEngineers), Display(Name="Optimal respawn distance", Description="Sets optimal distance in meters the game should take into consideration when spawning new player near others."), Category("Players"), Range(0x3e8, 0x61a8)]
        public float OptimalSpawnDistance;
        [ProtoMember(0x29c, IsRequired=false), GameRelation(VRage.Game.Game.SpaceEngineers), Display(Name="Enable Autorespawn", Description="Enables automatic respawn at nearest available respawn point"), Category("Players")]
        public bool EnableAutorespawn;
        [ProtoMember(0x2a2, IsRequired=false), GameRelation(VRage.Game.Game.SpaceEngineers), Display(Name="Enable Supergridding", Description="Allows super gridding exploit to be used"), Category("Others")]
        public bool EnableSupergridding;
        private bool m_experimentalReasonInited;
        private ExperimentalReason m_experimentalReason;

        public MyObjectBuilder_SessionSettings()
        {
            Dictionary<string, short> dict = new Dictionary<string, short>();
            dict.Add("Assembler", 0x18);
            dict.Add("Refinery", 0x18);
            dict.Add("Blast Furnace", 0x18);
            dict.Add("Antenna", 30);
            dict.Add("Drill", 30);
            dict.Add("InteriorTurret", 50);
            dict.Add("GatlingTurret", 50);
            dict.Add("MissileTurret", 50);
            dict.Add("ExtendedPistonBase", 50);
            dict.Add("MotorStator", 50);
            dict.Add("MotorAdvancedStator", 50);
            dict.Add("ShipWelder", 100);
            dict.Add("ShipGrinder", 150);
            this.BlockTypeLimits = new SerializableDictionary<string, short>(dict);
            this.MinDropContainerRespawnTime = 5;
            this.MaxDropContainerRespawnTime = 20;
            this.SyncDistance = 0xbb8;
            this.AdaptiveSimulationQuality = true;
            this.TrashRemovalEnabled = true;
            this.TrashFlagsValue = 0x1e1a;
            this.BlockCountThreshold = 20;
            this.PlayerDistanceThreshold = 500f;
            this.PlayerCharacterRemovalThreshold = 15;
            this.VoxelPlayerDistanceThreshold = 5000f;
            this.VoxelGridDistanceThreshold = 5000f;
            this.VoxelAgeThreshold = 0x18;
            this.EnableGoodBotHints = true;
            this.OptimalSpawnDistance = 4000f;
            this.EnableAutorespawn = true;
        }

        public ExperimentalReason GetExperimentalReason(bool update = true)
        {
            if (update || !this.m_experimentalReasonInited)
            {
                this.UpdateExperimentalReason();
            }
            return this.m_experimentalReason;
        }

        public static int GetInitialPCU(MyObjectBuilder_SessionSettings settings)
        {
            switch (settings.BlockLimitsEnabled)
            {
                case MyBlockLimitsEnabledEnum.NONE:
                    return 0x7fffffff;

                case MyBlockLimitsEnabledEnum.PER_FACTION:
                    return ((settings.MaxFactionsCount != 0) ? (settings.TotalPCU / settings.MaxFactionsCount) : settings.TotalPCU);

                case MyBlockLimitsEnabledEnum.PER_PLAYER:
                    return (settings.TotalPCU / settings.MaxPlayers);
            }
            return settings.TotalPCU;
        }

        public bool IsSettingsExperimental()
        {
            if (!this.m_experimentalReasonInited)
            {
                this.UpdateExperimentalReason();
            }
            return (this.m_experimentalReason != ~(ExperimentalReason.AdaptiveSimulationQuality | ExperimentalReason.BlockLimitsEnabled | ExperimentalReason.CargoShipsEnabled | ExperimentalReason.DestructibleBlocks | ExperimentalReason.Enable3rdPersonView | ExperimentalReason.EnableConvertToStation | ExperimentalReason.EnableCopyPaste | ExperimentalReason.EnableDrones | ExperimentalReason.EnableEncounters | ExperimentalReason.EnableIngameScripts | ExperimentalReason.EnableJetpack | ExperimentalReason.EnableOxygen | ExperimentalReason.EnableOxygenPressurization | ExperimentalReason.EnableRemoteBlockRemoval | ExperimentalReason.EnableRespawnShips | ExperimentalReason.EnableSpectator | ExperimentalReason.EnableSpiders | ExperimentalReason.EnableSubgridDamage | ExperimentalReason.EnableSunRotation | ExperimentalReason.EnableToolShake | ExperimentalReason.EnableTurretsFriendlyFire | ExperimentalReason.EnableVoxelDestruction | ExperimentalReason.EnableWolfs | ExperimentalReason.ExperimentalTurnedOnInConfiguration | ExperimentalReason.FloraDensity | ExperimentalReason.MaxPlayers | ExperimentalReason.PermanentDeath | ExperimentalReason.Plugins | ExperimentalReason.ProceduralDensity | ExperimentalReason.ResetOwnership | ExperimentalReason.ThrusterDamage | ExperimentalReason.WeaponsEnabled));
        }

        public void LogMembers(MyLog log, LoggingOptions options)
        {
            log.WriteLine("Settings:");
            using (log.IndentUsing(options))
            {
                log.WriteLine("GameMode = " + this.GameMode);
                log.WriteLine("MaxPlayers = " + this.MaxPlayers);
                log.WriteLine("OnlineMode = " + this.OnlineMode);
                log.WriteLine("AutoHealing = " + this.AutoHealing.ToString());
                log.WriteLine("WeaponsEnabled = " + this.WeaponsEnabled.ToString());
                log.WriteLine("ThrusterDamage = " + this.ThrusterDamage.ToString());
                log.WriteLine("EnableSpectator = " + this.EnableSpectator.ToString());
                log.WriteLine("EnableCopyPaste = " + this.EnableCopyPaste.ToString());
                log.WriteLine("MaxFloatingObjects = " + this.MaxFloatingObjects);
                log.WriteLine("MaxGridSize = " + this.MaxGridSize);
                log.WriteLine("MaxBlocksPerPlayer = " + this.MaxBlocksPerPlayer);
                log.WriteLine("CargoShipsEnabled = " + this.CargoShipsEnabled.ToString());
                log.WriteLine("EnvironmentHostility = " + this.EnvironmentHostility);
                log.WriteLine("ShowPlayerNamesOnHud = " + this.ShowPlayerNamesOnHud.ToString());
                log.WriteLine("InventorySizeMultiplier = " + this.InventorySizeMultiplier);
                log.WriteLine("RefinerySpeedMultiplier = " + this.RefinerySpeedMultiplier);
                log.WriteLine("AssemblerSpeedMultiplier = " + this.AssemblerSpeedMultiplier);
                log.WriteLine("AssemblerEfficiencyMultiplier = " + this.AssemblerEfficiencyMultiplier);
                log.WriteLine("WelderSpeedMultiplier = " + this.WelderSpeedMultiplier);
                log.WriteLine("GrinderSpeedMultiplier = " + this.GrinderSpeedMultiplier);
                log.WriteLine("ClientCanSave = " + this.ClientCanSave.ToString());
                log.WriteLine("HackSpeedMultiplier = " + this.HackSpeedMultiplier);
                log.WriteLine("PermanentDeath = " + this.PermanentDeath);
                log.WriteLine("DestructibleBlocks =  " + this.DestructibleBlocks.ToString());
                log.WriteLine("EnableScripts =  " + this.EnableIngameScripts.ToString());
                log.WriteLine("AutoSaveInMinutes = " + this.AutoSaveInMinutes);
                log.WriteLine("SpawnShipTimeMultiplier = " + this.SpawnShipTimeMultiplier);
                log.WriteLine("ProceduralDensity = " + this.ProceduralDensity);
                log.WriteLine("ProceduralSeed = " + this.ProceduralSeed);
                log.WriteLine("DestructibleBlocks = " + this.DestructibleBlocks.ToString());
                log.WriteLine("EnableIngameScripts = " + this.EnableIngameScripts.ToString());
                log.WriteLine("ViewDistance = " + this.ViewDistance);
                log.WriteLine("Voxel destruction = " + this.EnableVoxelDestruction.ToString());
                log.WriteLine("EnableStructuralSimulation = " + this.EnableStructuralSimulation.ToString());
                log.WriteLine("MaxActiveFracturePieces = " + this.MaxActiveFracturePieces);
                log.WriteLine("EnableContainerDrops = " + this.EnableContainerDrops.ToString());
                log.WriteLine("MinDropContainerRespawnTime = " + this.MinDropContainerRespawnTime);
                log.WriteLine("MaxDropContainerRespawnTime = " + this.MaxDropContainerRespawnTime);
                log.WriteLine("EnableTurretsFriendlyFire = " + this.EnableTurretsFriendlyFire.ToString());
                log.WriteLine("EnableSubgridDamage = " + this.EnableSubgridDamage.ToString());
                log.WriteLine("SyncDistance = " + this.SyncDistance);
                log.WriteLine("BlockLimitsEnabled = " + this.BlockLimitsEnabled);
                log.WriteLine("ExperimentalMode = " + this.ExperimentalMode.ToString());
                log.WriteLine("ExperimentalModeReason = " + this.GetExperimentalReason(true));
            }
        }

        public bool ShouldSerializeAutoSave() => 
            false;

        public bool ShouldSerializeProceduralDensity() => 
            (this.ProceduralDensity > 0f);

        public bool ShouldSerializeProceduralSeed() => 
            (this.ProceduralDensity > 0f);

        public bool ShouldSerializeTrashFlags() => 
            false;

        public void UpdateExperimentalReason()
        {
            this.m_experimentalReasonInited = true;
            this.m_experimentalReason = ~(ExperimentalReason.AdaptiveSimulationQuality | ExperimentalReason.BlockLimitsEnabled | ExperimentalReason.CargoShipsEnabled | ExperimentalReason.DestructibleBlocks | ExperimentalReason.Enable3rdPersonView | ExperimentalReason.EnableConvertToStation | ExperimentalReason.EnableCopyPaste | ExperimentalReason.EnableDrones | ExperimentalReason.EnableEncounters | ExperimentalReason.EnableIngameScripts | ExperimentalReason.EnableJetpack | ExperimentalReason.EnableOxygen | ExperimentalReason.EnableOxygenPressurization | ExperimentalReason.EnableRemoteBlockRemoval | ExperimentalReason.EnableRespawnShips | ExperimentalReason.EnableSpectator | ExperimentalReason.EnableSpiders | ExperimentalReason.EnableSubgridDamage | ExperimentalReason.EnableSunRotation | ExperimentalReason.EnableToolShake | ExperimentalReason.EnableTurretsFriendlyFire | ExperimentalReason.EnableVoxelDestruction | ExperimentalReason.EnableWolfs | ExperimentalReason.ExperimentalTurnedOnInConfiguration | ExperimentalReason.FloraDensity | ExperimentalReason.MaxPlayers | ExperimentalReason.PermanentDeath | ExperimentalReason.Plugins | ExperimentalReason.ProceduralDensity | ExperimentalReason.ResetOwnership | ExperimentalReason.ThrusterDamage | ExperimentalReason.WeaponsEnabled);
            if (this.ExperimentalMode)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableConvertToStation;
            }
            if ((this.MaxPlayers > 0x10) || (this.MaxPlayers == 0))
            {
                this.m_experimentalReason |= ExperimentalReason.MaxPlayers;
            }
            if ((this.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM) || (this.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM_UNREAL))
            {
                this.m_experimentalReason |= ExperimentalReason.EnableJetpack;
            }
            if (this.ProceduralDensity > 0.35f)
            {
                this.m_experimentalReason |= ExperimentalReason.ProceduralDensity;
            }
            if (this.SunRotationIntervalMinutes <= 30f)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableWolfs;
            }
            if (this.MaxFloatingObjects > 100)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableSpiders;
            }
            if (this.PhysicsIterations != 8)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableRemoteBlockRemoval;
            }
            if (this.SyncDistance != 0xbb8)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableSubgridDamage;
            }
            if (this.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
            {
                this.m_experimentalReason |= ExperimentalReason.BlockLimitsEnabled;
            }
            if (this.TotalPCU > 0x927c0)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableRespawnShips;
            }
            if (this.EnableSpectator)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableSpectator;
            }
            if (this.ResetOwnership)
            {
                this.m_experimentalReason |= ExperimentalReason.ResetOwnership;
            }
            bool? permanentDeath = this.PermanentDeath;
            bool flag = true;
            if ((permanentDeath.GetValueOrDefault() == flag) & (permanentDeath != null))
            {
                this.m_experimentalReason |= ExperimentalReason.PermanentDeath;
            }
            if (this.EnableIngameScripts)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableIngameScripts;
            }
            if (this.StationVoxelSupport)
            {
                this.m_experimentalReason |= ExperimentalReason.MaxPlayers;
            }
            if (this.EnableWolfs)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableWolfs;
            }
            if (this.EnableSpiders)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableSpiders;
            }
            if (this.EnableSubgridDamage)
            {
                this.m_experimentalReason |= ExperimentalReason.EnableSubgridDamage;
            }
            if (!this.AdaptiveSimulationQuality)
            {
                this.m_experimentalReason |= ExperimentalReason.AdaptiveSimulationQuality;
            }
            if (this.EnableSupergridding)
            {
                this.m_experimentalReason |= ExperimentalReason.ResetOwnership;
            }
        }

        public bool AutoSave
        {
            get => 
                (this.AutoSaveInMinutes != 0);
            set => 
                (this.AutoSaveInMinutes = value ? 5 : 0);
        }

        [Display(Name="Client can save"), GameRelation(VRage.Game.Game.Shared), XmlIgnore, NoSerialize, Browsable(false)]
        public bool ClientCanSave
        {
            get => 
                false;
            set
            {
            }
        }

        [XmlIgnore, ProtoIgnore, Browsable(false)]
        public MyTrashRemovalFlags TrashFlags
        {
            get => 
                ((MyTrashRemovalFlags) this.TrashFlagsValue);
            set => 
                (this.TrashFlagsValue = (int) value);
        }

        [Flags]
        public enum ExperimentalReason : long
        {
            ExperimentalMode = 2L,
            MaxPlayers = 4L,
            EnvironmentHostility = 8L,
            ProceduralDensity = 0x10L,
            FloraDensity = 0x20L,
            WorldSizeKm = 0x40L,
            SpawnShipTimeMultiplier = 0x80L,
            SunRotationIntervalMinutes = 0x100L,
            MaxFloatingObjects = 0x200L,
            PhysicsIterations = 0x400L,
            SyncDistance = 0x800L,
            ViewDistance = 0x1000L,
            BlockLimitsEnabled = 0x2000L,
            TotalPCU = 0x4000L,
            AutoHealing = 0x8000L,
            RespawnShipDelete = 0x10000L,
            EnableSpectator = 0x20000L,
            EnableCopyPaste = 0x40000L,
            ShowPlayerNamesOnHud = 0x80000L,
            ResetOwnership = 0x100000L,
            ThrusterDamage = 0x200000L,
            PermanentDeath = 0x400000L,
            WeaponsEnabled = 0x800000L,
            CargoShipsEnabled = 0x1000000L,
            DestructibleBlocks = 0x2000000L,
            EnableIngameScripts = 0x4000000L,
            EnableToolShake = 0x8000000L,
            EnableEncounters = 0x10000000L,
            Enable3rdPersonView = 0x20000000L,
            EnableOxygen = 0x40000000L,
            EnableOxygenPressurization = -2147483648L,
            EnableSunRotation = 1L,
            EnableConvertToStation = 2L,
            StationVoxelSupport = 4L,
            EnableJetpack = 8L,
            SpawnWithTools = 0x10L,
            StartInRespawnScreen = 0x20L,
            EnableVoxelDestruction = 0x40L,
            EnableDrones = 0x80L,
            EnableWolfs = 0x100L,
            EnableSpiders = 0x200L,
            EnableRemoteBlockRemoval = 0x400L,
            EnableSubgridDamage = 0x800L,
            EnableTurretsFriendlyFire = 0x1000L,
            EnableContainerDrops = 0x2000L,
            EnableRespawnShips = 0x4000L,
            AdaptiveSimulationQuality = 0x8000L,
            ExperimentalTurnedOnInConfiguration = 0x10000L,
            InsufficientHardware = 0x20000L,
            Mods = 0x40000L,
            Plugins = 0x80000L,
            SupergriddingEnabled = 0x100000L,
            ReasonMax = -2147483648L
        }
    }
}

