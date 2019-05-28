namespace Sandbox.Game
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Utils;
    using VRageMath;

    public static class MyPerGameSettings
    {
        public static MyBasicGameInfo BasicGameInfo = new MyBasicGameInfo();
        public static GameEnum Game = GameEnum.UNKNOWN_GAME;
        public static string GameWebUrl = "www.SpaceEngineersGame.com";
        public static string DeluxeEditionUrl = "http://news.spaceengineersgame.com/space_engineers_deluxe";
        public static string SkinSaleUrl = "http://store.steampowered.com/itemstore/244850/browse/?filter=all";
        public static uint DeluxeEditionDlcId = 0x8bee8;
        public static string LocalizationWebUrl = "http://www.spaceengineersgame.com/localization.html";
        public static string ChangeLogUrl = "http://mirror.keenswh.com/news/SpaceEngineersChangelog.xml";
        public static string EventsUrl = "http://mirror.keenswh.com/news/SpaceEngineersActiveEvents.txt";
        public static string ChangeLogUrlDevelop = "http://mirror.keenswh.com/news/SpaceEngineersChangelogDevelop.xml";
        public static string RankedServersUrl = "http://mirror.keenswh.com/xml/rankedServers.xml";
        public static string EShopUrl = "https://shop.keenswh.com/";
        public static bool RequiresDX11 = false;
        public static string GameIcon;
        public static bool EnableGlobalGravity;
        public static bool ZoomRequiresLookAroundPressed = true;
        public static bool EnablePregeneratedAsteroidHack = false;
        public static bool SendLogToKeen = false;
        public static string GA_Public_GameKey = string.Empty;
        public static string GA_Public_SecretKey = string.Empty;
        public static string GA_Dev_GameKey = string.Empty;
        public static string GA_Dev_SecretKey = string.Empty;
        public static string GA_Pirate_GameKey = string.Empty;
        public static string GA_Pirate_SecretKey = string.Empty;
        public static string GA_Other_GameKey = string.Empty;
        public static string GA_Other_SecretKey = string.Empty;
        public static string GameModAssembly;
        public static string GameModObjBuildersAssembly;
        public static string GameModBaseObjBuildersAssembly;
        public static string SandboxAssembly = "Sandbox.Common.dll";
        public static string SandboxGameAssembly = "Sandbox.Game.dll";
        public static bool OffsetVoxelMapByHalfVoxel = false;
        public static bool UseVolumeLimiter = false;
        public static bool UseMusicController = false;
        public static bool UseReverbEffect = false;
        public static bool UseSameSoundLimiter = false;
        public static bool UseNewDamageEffects = false;
        public static bool RestrictSpectatorFlyMode = false;
        public static float MaxFrameRate = 120f;
        private static Type m_isoMesherType = typeof(MyDualContouringMesher);
        public static double MinimumLargeShipCollidableMass = 1000.0;
        public static float? ConstantVoxelAmbient;
        private const float DefaultMaxWalkSpeed = 6f;
        private const float DefaultMaxCrouchWalkSpeed = 4f;
        public static MyCharacterMovementSettings CharacterMovement;
        public static MyCollisionParticleSettings CollisionParticle;
        public static MyDestructionParticleSettings DestructionParticle;
        public static bool UseGridSegmenter;
        public static bool Destruction;
        public static float PhysicsConvexRadius;
        public static bool PhysicsNoCollisionLayerWithDefault;
        public static float DefaultLinearDamping;
        public static float DefaultAngularDamping;
        public static bool BallFriendlyPhysics;
        public static bool DoubleKinematicForLargeGrids;
        public static bool CharacterStartsOnVoxel;
        public static bool LimitedWorld;
        public static bool EnableCollisionSparksEffect;
        private static bool m_useAnimationInsteadOfIK;
        public static bool WorkshopUseUGCEnumerate;
        public static string SteamGameServerGameDir;
        public static string SteamGameServerProductName;
        public static string SteamGameServerDescription;
        public static bool TerminalEnabled;
        public static MyGUISettings GUI;
        public static Type PathfindingType;
        public static Type BotFactoryType;
        public static bool EnableAi;
        public static bool EnablePathfinding;
        public static bool NavmeshPresumesDownwardGravity;
        public static Type ControlMenuInitializerType;
        public static Type CompatHelperType;
        public static MyCredits Credits;
        public static MyMusicTrack? MainMenuTrack;
        public static bool EnableObjectExport;
        public static bool TryConvertGridToDynamicAfterSplit;
        public static bool AnimateOnlyVisibleCharacters;
        public static float CharacterDamageMinVelocity;
        public static float CharacterDamageDeadlyDamageVelocity;
        public static float CharacterDamageMediumDamageVelocity;
        public static float CharacterDamageHitObjectMinMass;
        public static float CharacterDamageHitObjectMinVelocity;
        public static float CharacterDamageHitObjectMediumEnergy;
        public static float CharacterDamageHitObjectSmallEnergy;
        public static float CharacterDamageHitObjectCriticalEnergy;
        public static float CharacterDamageHitObjectDeadlyEnergy;
        public static float CharacterSmallDamage;
        public static float CharacterMediumDamage;
        public static float CharacterCriticalDamage;
        public static float CharacterDeadlyDamage;
        public static float CharacterSqueezeDamageDelay;
        public static float CharacterSqueezeMinMass;
        public static float CharacterSqueezeMediumDamageMass;
        public static float CharacterSqueezeCriticalDamageMass;
        public static float CharacterSqueezeDeadlyDamageMass;
        public static bool CharacterSuicideEnabled;
        public static Func<bool> ConstrainInventory;
        public static bool SwitchToSpectatorCameraAfterDeath;
        public static bool SimplePlayerNames;
        public static Type CharacterDetectionComponent;
        public static string BugReportUrl;
        public static bool EnableScenarios;
        public static bool EnableRagdollModels;
        public static bool ShowObfuscationStatus;
        public static bool EnableRagdollInJetpack;
        public static bool InventoryMass;
        public static bool EnableCharacterCollisionDamage;
        public static MyStringId DefaultGraphicsRenderer;
        public static bool EnableWelderAutoswitch;
        public static Type VoiceChatLogic;
        public static bool VoiceChatEnabled;
        public static bool EnableMutePlayer;
        public static bool EnableJumpDrive;
        public static bool EnableShipSoundSystem;
        public static bool EnableFloatingObjectsActiveSync;
        public static bool DisableAnimationsOnDS;
        public static float CharacterGravityMultiplier;
        public static bool BlockForVoxels;
        public static bool AlwaysShowAvailableBlocksOnHud;
        public static float MaxAntennaDrawDistance;
        public static bool EnableResearch;
        public static MyRenderDeviceSettings? DefaultRenderDeviceSettings;
        public static MyRelationsBetweenFactions DefaultFactionRelationship;
        public static bool MultiplayerEnabled;
        public static Type ClientStateType;
        public static RigidBodyFlag LargeGridRBFlag;
        public static readonly string MotDServerNameVariable;
        public static readonly string MotDWorldNameVariable;
        public static readonly string MotDServerDescriptionVariable;
        public static readonly string MotDPlayerCountVariable;
        public static readonly string MotDCurrentPlayerVariable;

        static MyPerGameSettings()
        {
            MyCharacterMovementSettings settings = new MyCharacterMovementSettings {
                WalkAcceleration = 50f,
                WalkDecceleration = 10f,
                SprintAcceleration = 100f,
                SprintDecceleration = 20f
            };
            CharacterMovement = settings;
            MyCollisionParticleSettings settings2 = new MyCollisionParticleSettings {
                LargeGridClose = "Collision_Sparks_LargeGrid_Close",
                LargeGridDistant = "Collision_Sparks_LargeGrid_Distant",
                SmallGridClose = "Collision_Sparks",
                SmallGridDistant = "Collision_Sparks",
                CloseDistanceSq = 400f,
                Scale = 1f
            };
            CollisionParticle = settings2;
            MyDestructionParticleSettings settings3 = new MyDestructionParticleSettings {
                DestructionSmokeLarge = "Dummy",
                DestructionHit = "Dummy",
                CloseDistanceSq = 400f,
                Scale = 1f
            };
            DestructionParticle = settings3;
            UseGridSegmenter = true;
            Destruction = false;
            PhysicsConvexRadius = 0.05f;
            PhysicsNoCollisionLayerWithDefault = false;
            DefaultLinearDamping = 0f;
            DefaultAngularDamping = 0.1f;
            BallFriendlyPhysics = false;
            DoubleKinematicForLargeGrids = true;
            CharacterStartsOnVoxel = false;
            LimitedWorld = false;
            EnableCollisionSparksEffect = true;
            m_useAnimationInsteadOfIK = false;
            WorkshopUseUGCEnumerate = true;
            SteamGameServerGameDir = "Space Engineers";
            SteamGameServerProductName = "Space Engineers";
            SteamGameServerDescription = "Space Engineers";
            TerminalEnabled = true;
            MyGUISettings settings4 = new MyGUISettings {
                EnableTerminalScreen = true,
                EnableToolbarConfigScreen = true,
                MultipleSpinningWheels = true,
                LoadingScreenIndexRange = new Vector2I(1, 0x10),
                HUDScreen = typeof(MyGuiScreenHudSpace),
                ToolbarConfigScreen = typeof(MyGuiScreenCubeBuilder),
                ToolbarControl = typeof(MyGuiControlToolbar),
                CustomWorldScreen = typeof(MyGuiScreenWorldSettings),
                ScenarioScreen = typeof(MyGuiScreenScenario),
                EditWorldSettingsScreen = typeof(MyGuiScreenWorldSettings),
                HelpScreen = typeof(MyGuiScreenHelpSpace),
                VoxelMapEditingScreen = typeof(MyGuiScreenDebugSpawnMenu),
                ScenarioLobbyClientScreen = typeof(MyGuiScreenScenarioMpClient),
                AdminMenuScreen = typeof(MyGuiScreenAdminMenu),
                CreateFactionScreen = typeof(MyGuiScreenCreateOrEditFaction),
                PlayersScreen = typeof(MyGuiScreenPlayers)
            };
            GUI = settings4;
            PathfindingType = null;
            BotFactoryType = null;
            EnableAi = false;
            EnablePathfinding = false;
            NavmeshPresumesDownwardGravity = false;
            ControlMenuInitializerType = null;
            CompatHelperType = typeof(MySessionCompatHelper);
            Credits = new MyCredits();
            MainMenuTrack = null;
            EnableObjectExport = true;
            TryConvertGridToDynamicAfterSplit = false;
            AnimateOnlyVisibleCharacters = false;
            CharacterDamageMinVelocity = 12f;
            CharacterDamageDeadlyDamageVelocity = 16f;
            CharacterDamageMediumDamageVelocity = 13f;
            CharacterDamageHitObjectMinMass = 10f;
            CharacterDamageHitObjectMinVelocity = 8.5f;
            CharacterDamageHitObjectMediumEnergy = 100f;
            CharacterDamageHitObjectSmallEnergy = 80f;
            CharacterDamageHitObjectCriticalEnergy = 200f;
            CharacterDamageHitObjectDeadlyEnergy = 500f;
            CharacterSmallDamage = 10f;
            CharacterMediumDamage = 30f;
            CharacterCriticalDamage = 70f;
            CharacterDeadlyDamage = 100f;
            CharacterSqueezeDamageDelay = 1f;
            CharacterSqueezeMinMass = 200f;
            CharacterSqueezeMediumDamageMass = 1000f;
            CharacterSqueezeCriticalDamageMass = 3000f;
            CharacterSqueezeDeadlyDamageMass = 5000f;
            CharacterSuicideEnabled = true;
            ConstrainInventory = () => MySession.Static.SurvivalMode;
            SwitchToSpectatorCameraAfterDeath = false;
            SimplePlayerNames = false;
            BugReportUrl = "http://forum.keenswh.com/forums/bug-reports.326950";
            EnableScenarios = false;
            EnableRagdollModels = true;
            ShowObfuscationStatus = true;
            EnableRagdollInJetpack = false;
            InventoryMass = false;
            EnableCharacterCollisionDamage = false;
            EnableWelderAutoswitch = false;
            VoiceChatLogic = null;
            VoiceChatEnabled = false;
            EnableMutePlayer = false;
            EnableJumpDrive = false;
            EnableShipSoundSystem = false;
            EnableFloatingObjectsActiveSync = false;
            DisableAnimationsOnDS = true;
            CharacterGravityMultiplier = 1f;
            BlockForVoxels = false;
            AlwaysShowAvailableBlocksOnHud = false;
            MaxAntennaDrawDistance = 500000f;
            EnableResearch = true;
            DefaultFactionRelationship = MyRelationsBetweenFactions.Enemies;
            MultiplayerEnabled = true;
            ClientStateType = typeof(MyClientState);
            LargeGridRBFlag = MyFakes.ENABLE_DOUBLED_KINEMATIC ? RigidBodyFlag.RBF_DOUBLED_KINEMATIC : RigidBodyFlag.RBF_DEFAULT;
            MotDServerNameVariable = "{server_name}";
            MotDWorldNameVariable = "{world_name}";
            MotDServerDescriptionVariable = "{server_description}";
            MotDPlayerCountVariable = "{player_count}";
            MotDCurrentPlayerVariable = "{current_player}";
        }

        public static string GameName =>
            BasicGameInfo.GameName;

        public static string GameNameSafe =>
            BasicGameInfo.GameNameSafe;

        public static string MinimumRequirementsPage =>
            BasicGameInfo.MinimumRequirementsWeb;

        public static Type IsoMesherType
        {
            get => 
                m_isoMesherType;
            set => 
                (m_isoMesherType = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPerGameSettings.<>c <>9 = new MyPerGameSettings.<>c();

            internal bool <.cctor>b__140_0() => 
                MySession.Static.SurvivalMode;
        }
    }
}

