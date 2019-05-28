namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.FileSystem;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Checkpoint : MyObjectBuilder_Base
    {
        private static SerializableDefinitionId DEFAULT_SCENARIO = new SerializableDefinitionId(typeof(MyObjectBuilder_ScenarioDefinition), "EmptyWorld");
        public static DateTime DEFAULT_DATE = new DateTime(0x4bf, 7, 1, 12, 0, 0);
        [ProtoMember(0x3a)]
        public SerializableVector3I CurrentSector;
        [ProtoMember(0x40)]
        public long ElapsedGameTime;
        [ProtoMember(0x43)]
        public string SessionName;
        [ProtoMember(70)]
        public MyPositionAndOrientation SpectatorPosition = new MyPositionAndOrientation(Matrix.Identity);
        [ProtoMember(0x49)]
        public bool SpectatorIsLightOn;
        [ProtoMember(0x4f), DefaultValue(0)]
        public MyCameraControllerEnum CameraController;
        [ProtoMember(0x52)]
        public long CameraEntity;
        [ProtoMember(0x55), DefaultValue(-1)]
        public long ControlledObject = -1L;
        [ProtoMember(0x58)]
        public string Password;
        [ProtoMember(0x5b)]
        public string Description;
        [ProtoMember(0x5e)]
        public DateTime LastSaveTime;
        [ProtoMember(0x61)]
        public float SpectatorDistance;
        [ProtoMember(100), DefaultValue((string) null)]
        public ulong? WorkshopId;
        [ProtoMember(0x68)]
        public MyObjectBuilder_Toolbar CharacterToolbar;
        [ProtoMember(0x6b)]
        public SerializableDictionary<long, PlayerId> ControlledEntities;
        [ProtoMember(110), XmlElement("Settings", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_SessionSettings>))]
        public MyObjectBuilder_SessionSettings Settings = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SessionSettings>();
        public MyObjectBuilder_ScriptManager ScriptManagerData;
        [ProtoMember(0x74)]
        public int AppVersion;
        [ProtoMember(0x77), DefaultValue((string) null)]
        public MyObjectBuilder_FactionCollection Factions;
        [ProtoMember(0xe3)]
        public List<ModItem> Mods;
        [ProtoMember(230)]
        public SerializableDictionary<ulong, MyPromoteLevel> PromotedUsers;
        public HashSet<ulong> CreativeTools;
        [ProtoMember(0xeb)]
        public SerializableDefinitionId Scenario = DEFAULT_SCENARIO;
        [ProtoMember(0xfe)]
        public List<RespawnCooldownItem> RespawnCooldowns;
        [ProtoMember(0x101)]
        public List<MyObjectBuilder_Identity> Identities;
        [ProtoMember(260)]
        public List<MyObjectBuilder_Client> Clients;
        [ProtoMember(0x108)]
        public MyEnvironmentHostilityEnum? PreviousEnvironmentHostility;
        [ProtoMember(0x10b)]
        public SerializableDictionary<PlayerId, MyObjectBuilder_Player> AllPlayersData;
        [ProtoMember(270)]
        public SerializableDictionary<PlayerId, List<Vector3>> AllPlayersColors;
        [ProtoMember(0x112)]
        public List<MyObjectBuilder_ChatHistory> ChatHistory;
        [ProtoMember(0x115)]
        public List<MyObjectBuilder_FactionChatHistory> FactionChatHistory;
        [ProtoMember(280)]
        public List<long> NonPlayerIdentities;
        [ProtoMember(0x11b)]
        public SerializableDictionary<long, MyObjectBuilder_Gps> Gps;
        [ProtoMember(0x11e)]
        public SerializableBoundingBoxD? WorldBoundaries;
        [ProtoMember(0x125), XmlArrayItem("MyObjectBuilder_SessionComponent", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_SessionComponent>))]
        public List<MyObjectBuilder_SessionComponent> SessionComponents;
        [ProtoMember(0x129)]
        public SerializableDefinitionId GameDefinition = ((SerializableDefinitionId) MyGameDefinition.Default);
        [ProtoMember(0x12e)]
        public HashSet<string> SessionComponentEnabled = new HashSet<string>();
        [ProtoMember(0x131)]
        public HashSet<string> SessionComponentDisabled = new HashSet<string>();
        [ProtoMember(0x135)]
        public DateTime InGameTime = DEFAULT_DATE;
        [ProtoMember(0x13c)]
        public MyObjectBuilder_SessionComponentMission MissionTriggers;
        [ProtoMember(0x13f)]
        public string Briefing;
        [ProtoMember(0x142)]
        public string BriefingVideo;
        public string CustomLoadingScreenImage;
        public string CustomLoadingScreenText;
        [ProtoMember(0x148)]
        public string CustomSkybox = "";
        [ProtoMember(0x14b), DefaultValue(9)]
        public int RequiresDX = 9;
        [ProtoMember(0x14e)]
        public List<string> VicinityModelsCache;
        [ProtoMember(0x151)]
        public List<string> VicinityArmorModelsCache;
        [ProtoMember(340)]
        public List<string> VicinityVoxelCache;
        public SerializableDictionary<ulong, MyObjectBuilder_Player> Players;
        [ProtoMember(0x17f)]
        public SerializableDictionary<PlayerId, MyObjectBuilder_Player> ConnectedPlayers;
        [ProtoMember(0x184)]
        public SerializableDictionary<PlayerId, long> DisconnectedPlayers;
        public List<PlayerItem> AllPlayers;
        [ProtoMember(0x1fc)]
        public SerializableDictionary<ulong, int> RemoteAdminSettings = new SerializableDictionary<ulong, int>();

        public bool ShouldSerializeAllPlayers() => 
            false;

        public bool ShouldSerializeAllPlayersColors() => 
            ((this.AllPlayersColors != null) && (this.AllPlayersColors.Dictionary.Count > 0));

        public bool ShouldSerializeAssemblerEfficiencyMultiplier() => 
            false;

        public bool ShouldSerializeAssemblerSpeedMultiplier() => 
            false;

        public bool ShouldSerializeAutoHealing() => 
            false;

        public bool ShouldSerializeAutoSave() => 
            false;

        public bool ShouldSerializeCargoShipsEnabled() => 
            false;

        public bool ShouldSerializeClients() => 
            ((this.Clients != null) && (this.Clients.Count != 0));

        public bool ShouldSerializeConnectedPlayers() => 
            false;

        public bool ShouldSerializeDisconnectedPlayers() => 
            false;

        public bool ShouldSerializeEnableCopyPaste() => 
            false;

        public bool ShouldSerializeGameMode() => 
            false;

        public bool ShouldSerializeGameTime() => 
            false;

        public bool ShouldSerializeInGameTime() => 
            (this.InGameTime != DEFAULT_DATE);

        public bool ShouldSerializeInventorySizeMultiplier() => 
            false;

        public bool ShouldSerializeMaxFloatingObjects() => 
            false;

        public bool ShouldSerializeMaxPlayers() => 
            false;

        public bool ShouldSerializeOnlineMode() => 
            false;

        public bool ShouldSerializeRefinerySpeedMultiplier() => 
            false;

        public bool ShouldSerializeShowPlayerNamesOnHud() => 
            false;

        public bool ShouldSerializeThrusterDamage() => 
            false;

        public bool ShouldSerializeWeaponsEnabled() => 
            false;

        public bool ShouldSerializeWorkshopId() => 
            (this.WorkshopId != null);

        public bool ShouldSerializeWorldBoundaries() => 
            (this.WorldBoundaries != null);

        public DateTime GameTime
        {
            get => 
                (new DateTime(0x821, 1, 1, 0, 0, 0, DateTimeKind.Utc) + new TimeSpan(this.ElapsedGameTime));
            set => 
                (this.ElapsedGameTime = (value - new DateTime(0x821, 1, 1)).Ticks);
        }

        public MyOnlineModeEnum OnlineMode
        {
            get => 
                this.Settings.OnlineMode;
            set => 
                (this.Settings.OnlineMode = value);
        }

        public bool AutoHealing
        {
            get => 
                this.Settings.AutoHealing;
            set => 
                (this.Settings.AutoHealing = value);
        }

        public bool EnableCopyPaste
        {
            get => 
                this.Settings.EnableCopyPaste;
            set => 
                (this.Settings.EnableCopyPaste = value);
        }

        public short MaxPlayers
        {
            get => 
                this.Settings.MaxPlayers;
            set => 
                (this.Settings.MaxPlayers = value);
        }

        public bool WeaponsEnabled
        {
            get => 
                this.Settings.WeaponsEnabled;
            set => 
                (this.Settings.WeaponsEnabled = value);
        }

        public bool ShowPlayerNamesOnHud
        {
            get => 
                this.Settings.ShowPlayerNamesOnHud;
            set => 
                (this.Settings.ShowPlayerNamesOnHud = value);
        }

        public short MaxFloatingObjects
        {
            get => 
                this.Settings.MaxFloatingObjects;
            set => 
                (this.Settings.MaxFloatingObjects = value);
        }

        public MyGameModeEnum GameMode
        {
            get => 
                this.Settings.GameMode;
            set => 
                (this.Settings.GameMode = value);
        }

        public float InventorySizeMultiplier
        {
            get => 
                this.Settings.InventorySizeMultiplier;
            set => 
                (this.Settings.InventorySizeMultiplier = value);
        }

        public float AssemblerSpeedMultiplier
        {
            get => 
                this.Settings.AssemblerSpeedMultiplier;
            set => 
                (this.Settings.AssemblerSpeedMultiplier = value);
        }

        public float AssemblerEfficiencyMultiplier
        {
            get => 
                this.Settings.AssemblerEfficiencyMultiplier;
            set => 
                (this.Settings.AssemblerEfficiencyMultiplier = value);
        }

        public float RefinerySpeedMultiplier
        {
            get => 
                this.Settings.RefinerySpeedMultiplier;
            set => 
                (this.Settings.RefinerySpeedMultiplier = value);
        }

        public bool ThrusterDamage
        {
            get => 
                this.Settings.ThrusterDamage;
            set => 
                (this.Settings.ThrusterDamage = value);
        }

        public bool CargoShipsEnabled
        {
            get => 
                this.Settings.CargoShipsEnabled;
            set => 
                (this.Settings.CargoShipsEnabled = value);
        }

        public bool AutoSave
        {
            get => 
                (this.Settings.AutoSaveInMinutes != 0);
            set => 
                (this.Settings.AutoSaveInMinutes = value ? 5 : 0);
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct ModItem
        {
            private MyWorkshopItem m_workshopItem;
            [ProtoMember(0x97)]
            public string Name;
            [ProtoMember(0x9b), DefaultValue(0)]
            public ulong PublishedFileId;
            [ProtoMember(0x9f), DefaultValue(false)]
            public bool IsDependency;
            [ProtoMember(0xa3), XmlAttribute]
            public string FriendlyName;
            public bool ShouldSerializeName() => 
                (this.Name != null);

            public bool ShouldSerializePublishedFileId() => 
                (this.PublishedFileId != 0L);

            public bool ShouldSerializeIsDependency() => 
                true;

            public bool ShouldSerializeFriendlyName() => 
                !string.IsNullOrEmpty(this.FriendlyName);

            public ModItem(ulong publishedFileId)
            {
                this.Name = publishedFileId.ToString() + ".sbm";
                this.PublishedFileId = publishedFileId;
                this.FriendlyName = string.Empty;
                this.IsDependency = false;
                this.m_workshopItem = null;
            }

            public ModItem(ulong publishedFileId, bool isDependency)
            {
                this.Name = publishedFileId.ToString() + ".sbm";
                this.PublishedFileId = publishedFileId;
                this.FriendlyName = string.Empty;
                this.IsDependency = isDependency;
                this.m_workshopItem = null;
            }

            public ModItem(string name, ulong publishedFileId)
            {
                this.Name = name ?? (publishedFileId.ToString() + ".sbm");
                this.PublishedFileId = publishedFileId;
                this.FriendlyName = string.Empty;
                this.IsDependency = false;
                this.m_workshopItem = null;
            }

            public ModItem(string name, ulong publishedFileId, string friendlyName)
            {
                this.Name = name ?? (publishedFileId.ToString() + ".sbm");
                this.PublishedFileId = publishedFileId;
                this.FriendlyName = friendlyName;
                this.IsDependency = false;
                this.m_workshopItem = null;
            }

            public override string ToString() => 
                $"{this.FriendlyName} ({this.PublishedFileId})";

            public void SetModData(MyWorkshopItem workshopItem)
            {
                this.m_workshopItem = workshopItem;
            }

            public string GetPath() => 
                ((this.m_workshopItem == null) ? Path.Combine(MyFileSystem.ModsPath, this.Name) : this.m_workshopItem.Folder);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PlayerId
        {
            public ulong ClientId;
            public int SerialId;
            public PlayerId(ulong steamId)
            {
                this.ClientId = steamId;
                this.SerialId = 0;
            }

            public void AssignFrom(ulong steamId)
            {
                this.ClientId = steamId;
                this.SerialId = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct PlayerItem
        {
            [ProtoMember(0x7d)]
            public long PlayerId;
            [ProtoMember(0x7f)]
            public bool IsDead;
            [ProtoMember(0x81)]
            public string Name;
            [ProtoMember(0x83)]
            public ulong SteamId;
            [ProtoMember(0x85)]
            public string Model;
            public PlayerItem(long id, string name, bool isDead, ulong steamId, string model)
            {
                this.PlayerId = id;
                this.IsDead = isDead;
                this.Name = name;
                this.SteamId = steamId;
                this.Model = model;
            }
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct RespawnCooldownItem
        {
            [ProtoMember(0xf1)]
            public ulong PlayerSteamId;
            [ProtoMember(0xf4)]
            public int PlayerSerialId;
            [ProtoMember(0xf7)]
            public string RespawnShipId;
            [ProtoMember(250)]
            public int Cooldown;
        }
    }
}

