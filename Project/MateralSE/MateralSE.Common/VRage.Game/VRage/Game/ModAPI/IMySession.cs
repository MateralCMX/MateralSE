namespace VRage.Game.ModAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMySession
    {
        event Action OnSessionLoading;

        event Action OnSessionReady;

        void BeforeStartComponents();
        void Draw();
        void GameOver();
        void GameOver(MyStringId? customMessage);
        MyObjectBuilder_Checkpoint GetCheckpoint(string saveName);
        MyObjectBuilder_Sector GetSector();
        MyPromoteLevel GetUserPromoteLevel(ulong steamId);
        Dictionary<string, byte[]> GetVoxelMapsArray();
        MyObjectBuilder_World GetWorld();
        bool IsPausable();
        bool IsUserAdmin(ulong steamId);
        [Obsolete("Use GetUserPromoteLevel")]
        bool IsUserPromoted(ulong steamId);
        void RegisterComponent(MySessionComponentBase component, MyUpdateOrder updateOrder, int priority);
        bool Save(string customSaveName = null);
        void SetAsNotReady();
        void SetCameraController(MyCameraControllerEnum cameraControllerEnum, IMyEntity cameraEntity = null, Vector3D? position = new Vector3D?());
        void SetComponentUpdateOrder(MySessionComponentBase component, MyUpdateOrder order);
        void Unload();
        void UnloadDataComponents();
        void UnloadMultiplayer();
        void UnregisterComponent(MySessionComponentBase component);
        void Update(MyTimeSpan time);
        void UpdateComponents();

        float AssemblerEfficiencyMultiplier { get; }

        float AssemblerSpeedMultiplier { get; }

        bool AutoHealing { get; }

        uint AutoSaveInMinutes { get; }

        IMyCameraController CameraController { get; }

        bool CargoShipsEnabled { get; }

        [Obsolete("Client saving not supported anymore")]
        bool ClientCanSave { get; }

        bool CreativeMode { get; }

        string CurrentPath { get; }

        string Description { get; set; }

        IMyCamera Camera { get; }

        double CameraTargetDistance { get; set; }

        IMyPlayer LocalHumanPlayer { get; }

        IMyConfig Config { get; }

        TimeSpan ElapsedPlayTime { get; }

        bool EnableCopyPaste { get; }

        MyEnvironmentHostilityEnum EnvironmentHostility { get; }

        DateTime GameDateTime { get; set; }

        float GrinderSpeedMultiplier { get; }

        float HackSpeedMultiplier { get; }

        float InventoryMultiplier { get; }

        float CharactersInventoryMultiplier { get; }

        float BlocksInventorySizeMultiplier { get; }

        bool IsCameraAwaitingEntity { get; set; }

        List<MyObjectBuilder_Checkpoint.ModItem> Mods { get; set; }

        bool IsCameraControlledObject { get; }

        bool IsCameraUserControlledSpectator { get; }

        bool IsServer { get; }

        short MaxFloatingObjects { get; }

        short MaxBackupSaves { get; }

        short MaxPlayers { get; }

        bool MultiplayerAlive { get; set; }

        bool MultiplayerDirect { get; set; }

        double MultiplayerLastMsg { get; set; }

        string Name { get; set; }

        float NegativeIntegrityTotal { get; set; }

        MyOnlineModeEnum OnlineMode { get; }

        string Password { get; set; }

        float PositiveIntegrityTotal { get; set; }

        float RefinerySpeedMultiplier { get; }

        bool ShowPlayerNamesOnHud { get; }

        bool SurvivalMode { get; }

        bool ThrusterDamage { get; }

        string ThumbPath { get; }

        TimeSpan TimeOnBigShip { get; }

        TimeSpan TimeOnFoot { get; }

        TimeSpan TimeOnJetpack { get; }

        TimeSpan TimeOnSmallShip { get; }

        bool WeaponsEnabled { get; }

        float WelderSpeedMultiplier { get; }

        ulong? WorkshopId { get; }

        IMyVoxelMaps VoxelMaps { get; }

        IMyPlayer Player { get; }

        IMyControllableEntity ControlledObject { get; }

        MyObjectBuilder_SessionSettings SessionSettings { get; }

        IMyFactionCollection Factions { get; }

        IMyDamageSystem DamageSystem { get; }

        IMyGpsCollection GPS { get; }

        BoundingBoxD WorldBoundaries { get; }

        MyPromoteLevel PromoteLevel { get; }

        bool HasCreativeRights { get; }

        [Obsolete("Use HasCreativeRights")]
        bool HasAdminPrivileges { get; }

        System.Version Version { get; }

        IMyOxygenProviderSystem OxygenProviderSystem { get; }
    }
}

