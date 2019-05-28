namespace Sandbox.Game.World
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.ContextHandling;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World.Generator;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions;
    using VRage.Game.Entity;
    using VRage.Game.GUI;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.Voxels;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Profiler;
    using VRage.Scripting;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner]
    public sealed class MySession : IMyNetObject, IMyEventOwner, IMySession
    {
        private static readonly ComponentComparer SessionComparer = new ComponentComparer();
        private readonly CachingDictionary<System.Type, MySessionComponentBase> m_sessionComponents;
        private readonly Dictionary<int, SortedSet<MySessionComponentBase>> m_sessionComponentsForUpdate;
        private HashSet<string> m_componentsToLoad;
        public HashSet<string> SessionComponentEnabled;
        public HashSet<string> SessionComponentDisabled;
        private MyOxygenProviderSystemHelper m_oxygenHelper;
        private const string SAVING_FOLDER = ".new";
        public const int MIN_NAME_LENGTH = 5;
        public const int MAX_NAME_LENGTH = 0x80;
        public const int MAX_DESCRIPTION_LENGTH = 0x1f3f;
        private static string m_platform = "Steam";
        private static string m_platformLinkAgreement_Steam = "http://steamcommunity.com/sharedfiles/workshoplegalagreement";
        private static string m_platformLinkAgreement_WeGame = "---";
        internal MySpectatorCameraController Spectator;
        internal MyTimeSpan m_timeOfSave;
        internal DateTime m_lastTimeMemoryLogged;
        private Dictionary<string, short> EmptyBlockTypeLimitDictionary;
        public int RequiresDX;
        public MyObjectBuilder_SessionSettings Settings;
        private bool? m_saveOnUnloadOverride;
        private MyObjectBuilder_SessionSettings.ExperimentalReason m_experimentalReason;
        private bool m_experimentalReasonInited;
        public MyScriptManager ScriptManager;
        public List<Tuple<string, MyBlueprintItemInfo>> BattleBlueprints;
        public Dictionary<ulong, MyPromoteLevel> PromotedUsers;
        public MyScenarioDefinition Scenario;
        public BoundingBoxD? WorldBoundaries;
        public readonly MyVoxelMaps VoxelMaps;
        public readonly MyFactionCollection Factions;
        public MyPlayerCollection Players;
        public MyPerPlayerData PerPlayerData;
        public readonly MyToolBarCollection Toolbars;
        internal MyVirtualClients VirtualClients;
        internal MyCameraCollection Cameras;
        public MyGpsCollection Gpss;
        public MyBlockLimits GlobalBlockLimits;
        public MyBlockLimits PirateBlockLimits;
        public MyChatSystem ChatSystem;
        public MyChatBot ChatBot;
        public bool ServerSaving;
        private AdminSettingsEnum m_adminSettings;
        private Dictionary<ulong, AdminSettingsEnum> m_remoteAdminSettings;
        private bool m_largeStreamingInProgress;
        private bool m_smallStreamingInProgress;
        [CompilerGenerated]
        private Action<ulong, MyPromoteLevel> OnUserPromoteLevelChanged;
        private static bool m_showMotD = false;
        public Dictionary<string, MyFixedPoint> AmountMined;
        [CompilerGenerated]
        private Action<IMyCameraController, IMyCameraController> CameraAttachedToChanged;
        private bool m_cameraAwaitingEntity;
        private IMyCameraController m_cameraController;
        public ulong WorldSizeInBytes;
        private int m_gameplayFrameCounter;
        private const int FRAMES_TO_CONSIDER_READY = 10;
        private int m_framesToReady;
        [CompilerGenerated]
        private static Action OnLoading;
        [CompilerGenerated]
        private static Action OnUnloading;
        [CompilerGenerated]
        private static Action AfterLoading;
        [CompilerGenerated]
        private static Action BeforeLoading;
        [CompilerGenerated]
        private static Action OnUnloaded;
        [CompilerGenerated]
        private Action OnReady;
        [CompilerGenerated]
        private Action<MyObjectBuilder_Checkpoint> OnSavingCheckpoint;
        private HashSet<ulong> m_creativeTools;
        private bool m_updateAllowed;
        private MyHudNotification m_aliveNotification;
        private List<MySessionComponentBase> m_loadOrder;
        private static int m_profilerDumpDelay;
        private int m_currentDumpNumber;
        private MyObjectBuilder_SessionSettings _settings;
        public const float ADAPTIVE_LOAD_THRESHOLD = 90f;

        public static  event Action AfterLoading
        {
            [CompilerGenerated] add
            {
                Action afterLoading = AfterLoading;
                while (true)
                {
                    Action a = afterLoading;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    afterLoading = Interlocked.CompareExchange<Action>(ref AfterLoading, action3, a);
                    if (ReferenceEquals(afterLoading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action afterLoading = AfterLoading;
                while (true)
                {
                    Action source = afterLoading;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    afterLoading = Interlocked.CompareExchange<Action>(ref AfterLoading, action3, source);
                    if (ReferenceEquals(afterLoading, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action BeforeLoading
        {
            [CompilerGenerated] add
            {
                Action beforeLoading = BeforeLoading;
                while (true)
                {
                    Action a = beforeLoading;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    beforeLoading = Interlocked.CompareExchange<Action>(ref BeforeLoading, action3, a);
                    if (ReferenceEquals(beforeLoading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action beforeLoading = BeforeLoading;
                while (true)
                {
                    Action source = beforeLoading;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    beforeLoading = Interlocked.CompareExchange<Action>(ref BeforeLoading, action3, source);
                    if (ReferenceEquals(beforeLoading, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<IMyCameraController, IMyCameraController> CameraAttachedToChanged
        {
            [CompilerGenerated] add
            {
                Action<IMyCameraController, IMyCameraController> cameraAttachedToChanged = this.CameraAttachedToChanged;
                while (true)
                {
                    Action<IMyCameraController, IMyCameraController> a = cameraAttachedToChanged;
                    Action<IMyCameraController, IMyCameraController> action3 = (Action<IMyCameraController, IMyCameraController>) Delegate.Combine(a, value);
                    cameraAttachedToChanged = Interlocked.CompareExchange<Action<IMyCameraController, IMyCameraController>>(ref this.CameraAttachedToChanged, action3, a);
                    if (ReferenceEquals(cameraAttachedToChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyCameraController, IMyCameraController> cameraAttachedToChanged = this.CameraAttachedToChanged;
                while (true)
                {
                    Action<IMyCameraController, IMyCameraController> source = cameraAttachedToChanged;
                    Action<IMyCameraController, IMyCameraController> action3 = (Action<IMyCameraController, IMyCameraController>) Delegate.Remove(source, value);
                    cameraAttachedToChanged = Interlocked.CompareExchange<Action<IMyCameraController, IMyCameraController>>(ref this.CameraAttachedToChanged, action3, source);
                    if (ReferenceEquals(cameraAttachedToChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action OnLoading
        {
            [CompilerGenerated] add
            {
                Action onLoading = OnLoading;
                while (true)
                {
                    Action a = onLoading;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onLoading = Interlocked.CompareExchange<Action>(ref OnLoading, action3, a);
                    if (ReferenceEquals(onLoading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onLoading = OnLoading;
                while (true)
                {
                    Action source = onLoading;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onLoading = Interlocked.CompareExchange<Action>(ref OnLoading, action3, source);
                    if (ReferenceEquals(onLoading, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action OnReady
        {
            [CompilerGenerated] add
            {
                Action onReady = this.OnReady;
                while (true)
                {
                    Action a = onReady;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onReady = Interlocked.CompareExchange<Action>(ref this.OnReady, action3, a);
                    if (ReferenceEquals(onReady, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onReady = this.OnReady;
                while (true)
                {
                    Action source = onReady;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onReady = Interlocked.CompareExchange<Action>(ref this.OnReady, action3, source);
                    if (ReferenceEquals(onReady, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyObjectBuilder_Checkpoint> OnSavingCheckpoint
        {
            [CompilerGenerated] add
            {
                Action<MyObjectBuilder_Checkpoint> onSavingCheckpoint = this.OnSavingCheckpoint;
                while (true)
                {
                    Action<MyObjectBuilder_Checkpoint> a = onSavingCheckpoint;
                    Action<MyObjectBuilder_Checkpoint> action3 = (Action<MyObjectBuilder_Checkpoint>) Delegate.Combine(a, value);
                    onSavingCheckpoint = Interlocked.CompareExchange<Action<MyObjectBuilder_Checkpoint>>(ref this.OnSavingCheckpoint, action3, a);
                    if (ReferenceEquals(onSavingCheckpoint, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyObjectBuilder_Checkpoint> onSavingCheckpoint = this.OnSavingCheckpoint;
                while (true)
                {
                    Action<MyObjectBuilder_Checkpoint> source = onSavingCheckpoint;
                    Action<MyObjectBuilder_Checkpoint> action3 = (Action<MyObjectBuilder_Checkpoint>) Delegate.Remove(source, value);
                    onSavingCheckpoint = Interlocked.CompareExchange<Action<MyObjectBuilder_Checkpoint>>(ref this.OnSavingCheckpoint, action3, source);
                    if (ReferenceEquals(onSavingCheckpoint, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action OnUnloaded
        {
            [CompilerGenerated] add
            {
                Action onUnloaded = OnUnloaded;
                while (true)
                {
                    Action a = onUnloaded;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onUnloaded = Interlocked.CompareExchange<Action>(ref OnUnloaded, action3, a);
                    if (ReferenceEquals(onUnloaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onUnloaded = OnUnloaded;
                while (true)
                {
                    Action source = onUnloaded;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onUnloaded = Interlocked.CompareExchange<Action>(ref OnUnloaded, action3, source);
                    if (ReferenceEquals(onUnloaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action OnUnloading
        {
            [CompilerGenerated] add
            {
                Action onUnloading = OnUnloading;
                while (true)
                {
                    Action a = onUnloading;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onUnloading = Interlocked.CompareExchange<Action>(ref OnUnloading, action3, a);
                    if (ReferenceEquals(onUnloading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onUnloading = OnUnloading;
                while (true)
                {
                    Action source = onUnloading;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onUnloading = Interlocked.CompareExchange<Action>(ref OnUnloading, action3, source);
                    if (ReferenceEquals(onUnloading, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<ulong, MyPromoteLevel> OnUserPromoteLevelChanged
        {
            [CompilerGenerated] add
            {
                Action<ulong, MyPromoteLevel> onUserPromoteLevelChanged = this.OnUserPromoteLevelChanged;
                while (true)
                {
                    Action<ulong, MyPromoteLevel> a = onUserPromoteLevelChanged;
                    Action<ulong, MyPromoteLevel> action3 = (Action<ulong, MyPromoteLevel>) Delegate.Combine(a, value);
                    onUserPromoteLevelChanged = Interlocked.CompareExchange<Action<ulong, MyPromoteLevel>>(ref this.OnUserPromoteLevelChanged, action3, a);
                    if (ReferenceEquals(onUserPromoteLevelChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong, MyPromoteLevel> onUserPromoteLevelChanged = this.OnUserPromoteLevelChanged;
                while (true)
                {
                    Action<ulong, MyPromoteLevel> source = onUserPromoteLevelChanged;
                    Action<ulong, MyPromoteLevel> action3 = (Action<ulong, MyPromoteLevel>) Delegate.Remove(source, value);
                    onUserPromoteLevelChanged = Interlocked.CompareExchange<Action<ulong, MyPromoteLevel>>(ref this.OnUserPromoteLevelChanged, action3, source);
                    if (ReferenceEquals(onUserPromoteLevelChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action IMySession.OnSessionLoading
        {
            add
            {
                OnLoading += value;
            }
            remove
            {
                OnLoading -= value;
            }
        }

        event Action IMySession.OnSessionReady
        {
            add
            {
                Static.OnReady += value;
            }
            remove
            {
                Static.OnReady -= value;
            }
        }

        static MySession()
        {
            if (MyAPIGatewayShortcuts.GetMainCamera == null)
            {
                MyAPIGatewayShortcuts.GetMainCamera = new MyAPIGatewayShortcuts.GetMainCameraCallback(MySession.GetMainCamera);
            }
            if (MyAPIGatewayShortcuts.GetWorldBoundaries == null)
            {
                MyAPIGatewayShortcuts.GetWorldBoundaries = new MyAPIGatewayShortcuts.GetWorldBoundariesCallback(MySession.GetWorldBoundaries);
            }
            if (MyAPIGatewayShortcuts.GetLocalPlayerPosition == null)
            {
                MyAPIGatewayShortcuts.GetLocalPlayerPosition = new MyAPIGatewayShortcuts.GetLocalPlayerPositionCallback(MySession.GetLocalPlayerPosition);
            }
        }

        private MySession() : this(Sandbox.Engine.Platform.Game.IsDedicated ? Sandbox.Engine.Multiplayer.MyMultiplayer.Static.SyncLayer : new MySyncLayer(new MyTransportLayer(2)), true)
        {
        }

        private MySession(MySyncLayer syncLayer, bool registerComponents = true)
        {
            this.m_sessionComponents = new CachingDictionary<System.Type, MySessionComponentBase>();
            this.m_sessionComponentsForUpdate = new Dictionary<int, SortedSet<MySessionComponentBase>>();
            this.SessionComponentEnabled = new HashSet<string>();
            this.SessionComponentDisabled = new HashSet<string>();
            this.m_oxygenHelper = new MyOxygenProviderSystemHelper();
            this.Spectator = new MySpectatorCameraController();
            this.EmptyBlockTypeLimitDictionary = new Dictionary<string, short>();
            this.RequiresDX = 9;
            this.PromotedUsers = new Dictionary<ulong, MyPromoteLevel>();
            this.VoxelMaps = new MyVoxelMaps();
            this.Factions = new MyFactionCollection();
            this.Players = new MyPlayerCollection();
            this.PerPlayerData = new MyPerPlayerData();
            this.Toolbars = new MyToolBarCollection();
            this.VirtualClients = new MyVirtualClients();
            this.Cameras = new MyCameraCollection();
            this.Gpss = new MyGpsCollection();
            this.ChatSystem = new MyChatSystem();
            this.ChatBot = new MyChatBot();
            this.m_remoteAdminSettings = new Dictionary<ulong, AdminSettingsEnum>();
            this.AmountMined = new Dictionary<string, MyFixedPoint>();
            this.m_cameraController = MySpectatorCameraController.Static;
            this.m_creativeTools = new HashSet<ulong>();
            this.m_loadOrder = new List<MySessionComponentBase>();
            if (syncLayer == null)
            {
                MyLog.Default.WriteLine("MySession.Static.MySession() - sync layer is null");
            }
            this.SyncLayer = syncLayer;
            TimeSpan span = new TimeSpan();
            this.ElapsedGameTime = span;
            this.Spectator.Reset();
            MyCubeGrid.ResetInfoGizmos();
            this.m_timeOfSave = MyTimeSpan.Zero;
            span = new TimeSpan();
            this.ElapsedGameTime = span;
            this.Ready = false;
            this.MultiplayerLastMsg = 0.0;
            this.MultiplayerAlive = true;
            this.MultiplayerDirect = true;
            this.AppVersionFromSave = (int) MyFinalBuildConstants.APP_VERSION;
            this.Factions.FactionStateChanged += new Action<MyFactionStateChange, long, long, long, long>(this.OnFactionsStateChanged);
            this.ScriptManager = new MyScriptManager();
            GC.Collect(2, GCCollectionMode.Forced);
            MySandboxGame.Log.WriteLine($"GC Memory: {GC.GetTotalMemory(false).ToString("##,#")} B");
            MySandboxGame.Log.WriteLine($"Process Memory: {Process.GetCurrentProcess().PrivateMemorySize64.ToString("##,#")} B");
            this.GameFocusManager = new MyGameFocusManager();
        }

        private void AddAllModels(MyModel model, HashSet<string> models)
        {
            if (!string.IsNullOrEmpty(model.AssetName))
            {
                models.Add(model.AssetName);
            }
        }

        public void AddComponentForUpdate(MyUpdateOrder updateOrder, MySessionComponentBase component)
        {
            for (int i = 0; i <= 2; i++)
            {
                if ((updateOrder & (1 << (i & 0x1f))) != MyUpdateOrder.NoUpdate)
                {
                    SortedSet<MySessionComponentBase> set = null;
                    if (!this.m_sessionComponentsForUpdate.TryGetValue(1 << (i & 0x1f), out set))
                    {
                        this.m_sessionComponentsForUpdate.Add(1 << (i & 0x1f), set = new SortedSet<MySessionComponentBase>(SessionComparer));
                    }
                    set.Add(component);
                }
            }
        }

        public void BeforeStartComponents()
        {
            this.TotalDamageDealt = 0;
            this.TotalBlocksCreated = 0;
            this.ToolbarPageSwitches = 0;
            TimeSpan span = new TimeSpan();
            this.ElapsedPlayTime = span;
            this.m_timeOfSave = MySandboxGame.Static.TotalTime;
            MyFpsManager.Reset();
            using (Dictionary<System.Type, MySessionComponentBase>.ValueCollection.Enumerator enumerator = this.m_sessionComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.BeforeStart();
                }
            }
            if (MySpaceAnalytics.Instance != null)
            {
                if (Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MySpaceAnalytics.Instance.StartSessionAndIdentifyPlayer(true);
                }
                MySpaceAnalytics.Instance.ReportGameplayStart(this.Settings);
            }
        }

        public bool CanDemoteUser(ulong requester, ulong target)
        {
            MyPromoteLevel userPromoteLevel = this.GetUserPromoteLevel(requester);
            MyPromoteLevel level2 = this.GetUserPromoteLevel(target);
            return ((level2 > MyPromoteLevel.None) && ((level2 < MyPromoteLevel.Owner) && ((userPromoteLevel >= level2) && (userPromoteLevel >= MyPromoteLevel.Admin))));
        }

        public bool CanPromoteUser(ulong requester, ulong target)
        {
            MyPromoteLevel userPromoteLevel = this.GetUserPromoteLevel(requester);
            MyPromoteLevel level2 = this.GetUserPromoteLevel(target);
            return ((level2 < MyPromoteLevel.Admin) && ((userPromoteLevel >= level2) && (userPromoteLevel >= MyPromoteLevel.Admin)));
        }

        public bool CheckDLCAndNotify(MyDefinitionBase definition)
        {
            MyHudNotificationBase notification = MyHud.Notifications.Get(MyNotificationSingletons.MissingDLC);
            MyDLCs.MyDLC firstMissingDefinitionDLC = this.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId);
            if (firstMissingDefinitionDLC == null)
            {
                return true;
            }
            object[] arguments = new object[] { MyTexts.Get(firstMissingDefinitionDLC.DisplayName) };
            notification.SetTextFormatArguments(arguments);
            MyHud.Notifications.Add(notification);
            return false;
        }

        public bool CheckLimitsAndNotify(long ownerID, string blockName, int pcuToBuild, int blocksToBuild = 0, int blocksCount = 0, Dictionary<string, int> blocksPerType = null)
        {
            string str;
            LimitResult result = this.IsWithinWorldLimits(out str, ownerID, blockName, pcuToBuild, blocksToBuild, blocksCount, blocksPerType);
            if (result == LimitResult.Passed)
            {
                return true;
            }
            MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            MyHud.Notifications.Add(GetNotificationForLimitResult(result));
            return false;
        }

        private void CheckMultiplayerStatus()
        {
            this.MultiplayerAlive = Sandbox.Engine.Multiplayer.MyMultiplayer.Static.IsConnectionAlive;
            this.MultiplayerDirect = Sandbox.Engine.Multiplayer.MyMultiplayer.Static.IsConnectionDirect;
            if (Sync.IsServer)
            {
                this.MultiplayerLastMsg = 0.0;
            }
            else
            {
                this.MultiplayerLastMsg = (DateTime.UtcNow - Sandbox.Engine.Multiplayer.MyMultiplayer.Static.LastMessageReceived).TotalSeconds;
                MyReplicationClient replicationLayer = Sandbox.Engine.Multiplayer.MyMultiplayer.ReplicationLayer as MyReplicationClient;
                if (replicationLayer != null)
                {
                    this.MultiplayerPing = replicationLayer.Ping;
                    this.LargeStreamingInProgress = replicationLayer.PendingStreamingRelicablesCount > 1;
                    this.SmallStreamingInProgress = replicationLayer.PendingStreamingRelicablesCount > 0;
                }
            }
        }

        private static void CheckProfilerDump()
        {
            m_profilerDumpDelay--;
            if (m_profilerDumpDelay == 0)
            {
                MyRenderProxy.GetRenderProfiler().Dump();
                VRage.Profiler.MyRenderProfiler.SetLevel(0);
            }
            else if (m_profilerDumpDelay < 0)
            {
                m_profilerDumpDelay = -1;
            }
        }

        public bool CheckResearchAndNotify(long identityId, MyDefinitionId id)
        {
            if ((!Static.Settings.EnableResearch || (MySessionComponentResearch.Static.CanUse(identityId, id) || Static.CreativeMode)) || Static.CreativeToolsEnabled(Static.Players.TryGetSteamId(identityId)))
            {
                return true;
            }
            if ((Static.LocalCharacter != null) && (identityId == Static.LocalCharacter.GetPlayerIdentityId()))
            {
                MyHud.Notifications.Add(MyNotificationSingletons.BlockNotResearched);
            }
            return false;
        }

        private void CheckUpdate()
        {
            bool flag = true;
            if (this.IsPausable())
            {
                flag = !MySandboxGame.IsPaused && MySandboxGame.Static.IsActive;
            }
            if (this.m_updateAllowed != flag)
            {
                this.m_updateAllowed = flag;
                if (!this.m_updateAllowed)
                {
                    MyLog.Default.WriteLine("Updating stopped.");
                    SortedSet<MySessionComponentBase> set = null;
                    if (!this.m_sessionComponentsForUpdate.TryGetValue(4, out set))
                    {
                        return;
                    }
                    else
                    {
                        using (SortedSet<MySessionComponentBase>.Enumerator enumerator = set.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.UpdatingStopped();
                            }
                            return;
                        }
                    }
                }
                MyLog.Default.WriteLine("Updating continues.");
            }
        }

        internal static void CreateWithEmptyWorld(MyMultiplayerBase multiplayerSession)
        {
            Static = new MySession(multiplayerSession.SyncLayer, false);
            Static.InGameTime = MyObjectBuilder_Checkpoint.DEFAULT_DATE;
            Static.Gpss.RegisterChat(multiplayerSession);
            Static.CameraController = MySpectatorCameraController.Static;
            Static.Settings = new MyObjectBuilder_SessionSettings();
            Static.Settings.AutoSaveInMinutes = 0;
            Static.IsCameraAwaitingEntity = true;
            Static.PrepareBaseSession(new List<MyObjectBuilder_Checkpoint.ModItem>(), null);
            multiplayerSession.StartProcessingClientMessagesWithEmptyWorld();
            if (Sync.IsServer)
            {
                Static.InitializeFactions();
            }
            MyLocalCache.ClearLastSessionInfo();
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (Static.LocalHumanPlayer == null))
            {
                Sync.Players.RequestNewPlayer(0, MyGameService.UserName, null, true, true);
            }
            MyGeneralStats.Clear();
        }

        public bool CreativeToolsEnabled(ulong user) => 
            (this.m_creativeTools.Contains(user) && this.HasPlayerCreativeRights(user));

        private void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                bool flag1 = MyDebugDrawSettings.DEBUG_DRAW_CONTROLLED_ENTITIES;
                if (MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_COMPOSITION)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_DataProvider);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_ACCESS)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_Access);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_FULLCELLS)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.FullCells);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MICRONODES)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_MicroNodes);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MICRONODES_SCALED)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_MicroNodesScaled);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACRONODES)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_MacroNodes);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACROLEAVES)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_MacroLeaves);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACRO_SCALED)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Content_MacroScaled);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MATERIALS_MACRONODES)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Materials_MacroNodes);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MATERIALS_MACROLEAVES)
                {
                    this.VoxelMaps.DebugDraw(MyVoxelDebugDrawMode.Materials_MacroLeaves);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_ENCOUNTERS)
                {
                    MyEncounterGenerator.Static.DebugDraw();
                }
            }
        }

        private void DisconnectMultiplayer()
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.Disconnect();
            }
        }

        public void Draw()
        {
            using (Dictionary<System.Type, MySessionComponentBase>.ValueCollection.Enumerator enumerator = this.m_sessionComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Draw();
                }
            }
        }

        public void EnableCreativeTools(ulong user, bool value)
        {
            if (!value || !this.HasCreativeRights)
            {
                this.m_creativeTools.Remove(user);
            }
            else
            {
                this.m_creativeTools.Add(user);
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool>(s => new Action<bool>(MySession.OnCreativeToolsEnabled), value, targetEndpoint, position);
        }

        internal static void FixIncorrectSettings(MyObjectBuilder_SessionSettings settings)
        {
            MyObjectBuilder_SessionSettings settings2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SessionSettings>();
            bool enableWolfs = settings.EnableWolfs;
            bool enableSpiders = settings.EnableSpiders;
            if (settings.RefinerySpeedMultiplier <= 0f)
            {
                settings.RefinerySpeedMultiplier = settings2.RefinerySpeedMultiplier;
            }
            if (settings.AssemblerSpeedMultiplier <= 0f)
            {
                settings.AssemblerSpeedMultiplier = settings2.AssemblerSpeedMultiplier;
            }
            if (settings.AssemblerEfficiencyMultiplier <= 0f)
            {
                settings.AssemblerEfficiencyMultiplier = settings2.AssemblerEfficiencyMultiplier;
            }
            if (settings.InventorySizeMultiplier <= 0f)
            {
                settings.InventorySizeMultiplier = settings2.InventorySizeMultiplier;
            }
            if (settings.WelderSpeedMultiplier <= 0f)
            {
                settings.WelderSpeedMultiplier = settings2.WelderSpeedMultiplier;
            }
            if (settings.GrinderSpeedMultiplier <= 0f)
            {
                settings.GrinderSpeedMultiplier = settings2.GrinderSpeedMultiplier;
            }
            if (settings.HackSpeedMultiplier <= 0f)
            {
                settings.HackSpeedMultiplier = settings2.HackSpeedMultiplier;
            }
            if (settings.PermanentDeath == null)
            {
                settings.PermanentDeath = true;
            }
            settings.ViewDistance = MathHelper.Clamp(settings.ViewDistance, 0x3e8, 0xc350);
            settings.SyncDistance = MathHelper.Clamp(settings.SyncDistance, 0x3e8, 0x4e20);
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                settings.Scenario = false;
                settings.ScenarioEditMode = false;
            }
            if ((Static != null) && (Static.Scenario != null))
            {
                settings.WorldSizeKm = Static.Scenario.HasPlanets ? 0 : settings.WorldSizeKm;
            }
            if (((Static != null) && (Static.WorldBoundaries == null)) && (settings.WorldSizeKm > 0))
            {
                double x = settings.WorldSizeKm * 500;
                if (x > 0.0)
                {
                    Static.WorldBoundaries = new BoundingBoxD(new Vector3D(-x, -x, -x), new Vector3D(x, x, x));
                }
            }
        }

        internal void FixMissingCharacter()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                int num1;
                int num2;
                bool flag = (this.ControlledEntity != null) && (this.ControlledEntity is MyCockpit);
                bool flag2 = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCharacter>().Any<MyCharacter>();
                if ((this.ControlledEntity == null) || !(this.ControlledEntity is MyRemoteControl))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (this.ControlledEntity as MyRemoteControl).WasControllingCockpitWhenSaved();
                }
                bool flag3 = (bool) num1;
                if ((this.ControlledEntity == null) || !(this.ControlledEntity is MyLargeTurretBase))
                {
                    num2 = 0;
                }
                else
                {
                    num2 = (int) (this.ControlledEntity as MyLargeTurretBase).WasControllingCockpitWhenSaved();
                }
                bool flag4 = (bool) num2;
                if ((!MyInput.Static.ENABLE_DEVELOPER_KEYS && (!flag && (!flag2 && !flag3))) && !flag4)
                {
                    MyPlayerCollection.RequestLocalRespawn();
                }
            }
        }

        public void GameOver()
        {
            this.GameOver(new MyStringId?(MyCommonTexts.MP_YouHaveBeenKilled));
        }

        public void GameOver(MyStringId? customMessage)
        {
        }

        private void GatherVicinityInformation(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && MyFakes.PRIORITIZED_VICINITY_ASSETS_LOADING)
            {
                if (checkpoint.VicinityArmorModelsCache == null)
                {
                    checkpoint.VicinityArmorModelsCache = new List<string>();
                }
                else
                {
                    checkpoint.VicinityArmorModelsCache.Clear();
                }
                if (checkpoint.VicinityModelsCache == null)
                {
                    checkpoint.VicinityModelsCache = new List<string>();
                }
                else
                {
                    checkpoint.VicinityModelsCache.Clear();
                }
                if (checkpoint.VicinityVoxelCache == null)
                {
                    checkpoint.VicinityVoxelCache = new List<string>();
                }
                else
                {
                    checkpoint.VicinityVoxelCache.Clear();
                }
                if (this.LocalCharacter != null)
                {
                    BoundingSphereD bs = new BoundingSphereD(this.LocalCharacter.WorldMatrix.Translation, MyFakes.PRIORITIZED_CUBE_VICINITY_RADIUS);
                    HashSet<string> voxelMaterials = new HashSet<string>();
                    HashSet<string> models = new HashSet<string>();
                    HashSet<string> armorModels = new HashSet<string>();
                    this.GatherVicinityInformation(ref bs, voxelMaterials, models, armorModels);
                    checkpoint.VicinityArmorModelsCache.AddRange(armorModels);
                    checkpoint.VicinityModelsCache.AddRange(models);
                    checkpoint.VicinityVoxelCache.AddRange(voxelMaterials);
                }
            }
        }

        public void GatherVicinityInformation(ref BoundingSphereD bs, HashSet<string> voxelMaterials, HashSet<string> models, HashSet<string> armorModels)
        {
            List<VRage.Game.Entity.MyEntity> result = new List<VRage.Game.Entity.MyEntity>();
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref bs, result, MyEntityQueryType.Both);
            foreach (VRage.Game.Entity.MyEntity entity in result)
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid != null)
                {
                    if ((grid.RenderData != null) && (grid.RenderData.Cells != null))
                    {
                        foreach (KeyValuePair<Vector3I, MyCubeGridRenderCell> pair in grid.Render.RenderData.Cells)
                        {
                            if (pair.Value.CubeParts == null)
                            {
                                continue;
                            }
                            IEnumerator<KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>>> enumerator = pair.Value.CubeParts.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>> current = enumerator.Current;
                                    this.AddAllModels(current.Key.Model, armorModels);
                                }
                            }
                            finally
                            {
                                if (enumerator == null)
                                {
                                    continue;
                                }
                                enumerator.Dispose();
                            }
                        }
                    }
                    foreach (MySlimBlock block in grid.CubeBlocks)
                    {
                        if (block.FatBlock == null)
                        {
                            continue;
                        }
                        if (block.FatBlock.Model != null)
                        {
                            this.AddAllModels(block.FatBlock.Model, models);
                        }
                    }
                    continue;
                }
                MyVoxelBase voxel = entity as MyVoxelBase;
                if ((voxel != null) && !(voxel is MyVoxelPhysics))
                {
                    this.GetVoxelMaterials(voxelMaterials, voxel, 7, bs.Center, (float) MyFakes.PRIORITIZED_VOXEL_VICINITY_RADIUS_FAR);
                    this.GetVoxelMaterials(voxelMaterials, voxel, 1, bs.Center, (float) MyFakes.PRIORITIZED_VOXEL_VICINITY_RADIUS_CLOSE);
                }
            }
        }

        public short GetBlockTypeLimit(string blockType)
        {
            short num2;
            int num = 1;
            switch (this.BlockLimitsEnabled)
            {
                case MyBlockLimitsEnabledEnum.NONE:
                    return 0;

                case MyBlockLimitsEnabledEnum.GLOBALLY:
                    num = 1;
                    break;

                case MyBlockLimitsEnabledEnum.PER_FACTION:
                    num = (this.MaxFactionsCount != 0) ? 1 : 1;
                    break;

                case MyBlockLimitsEnabledEnum.PER_PLAYER:
                    num = 1;
                    break;

                default:
                    break;
            }
            if (!this.BlockTypeLimits.TryGetValue(blockType, out num2))
            {
                return 0;
            }
            if ((num2 <= 0) || ((num2 / num) != 0))
            {
                return (short) (num2 / num);
            }
            return 1;
        }

        public MyCameraControllerEnum GetCameraControllerEnum()
        {
            if (!ReferenceEquals(this.CameraController, MySpectatorCameraController.Static))
            {
                if (ReferenceEquals(this.CameraController, MyThirdPersonSpectator.Static))
                {
                    return MyCameraControllerEnum.ThirdPersonSpectator;
                }
                if ((this.CameraController is VRage.Game.Entity.MyEntity) || (this.CameraController is MyEntityRespawnComponentBase))
                {
                    if ((!this.CameraController.IsInFirstPersonView && !this.CameraController.ForceFirstPersonCamera) || !this.CameraController.EnableFirstPersonView)
                    {
                        return MyCameraControllerEnum.ThirdPersonSpectator;
                    }
                    return MyCameraControllerEnum.Entity;
                }
            }
            else
            {
                switch (MySpectatorCameraController.Static.SpectatorCameraMovement)
                {
                    case MySpectatorCameraMovementEnum.UserControlled:
                        return MyCameraControllerEnum.Spectator;

                    case MySpectatorCameraMovementEnum.ConstantDelta:
                        return MyCameraControllerEnum.SpectatorDelta;

                    case MySpectatorCameraMovementEnum.None:
                        return MyCameraControllerEnum.SpectatorFixed;

                    case MySpectatorCameraMovementEnum.Orbit:
                        return MyCameraControllerEnum.SpectatorOrbit;

                    default:
                        break;
                }
            }
            return MyCameraControllerEnum.Spectator;
        }

        public float GetCameraTargetDistance() => 
            ((float) MyThirdPersonSpectator.Static.GetViewerDistance());

        public MyObjectBuilder_Checkpoint GetCheckpoint(string saveName)
        {
            SerializableBoundingBoxD? nullable1;
            MatrixD matrix = MatrixD.Invert(MySpectatorCameraController.Static.GetViewMatrix());
            MyCameraControllerEnum cameraControllerEnum = this.GetCameraControllerEnum();
            MyObjectBuilder_Checkpoint checkpoint = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Checkpoint>();
            MyObjectBuilder_SessionSettings settings = MyObjectBuilderSerializer.Clone(this.Settings) as MyObjectBuilder_SessionSettings;
            settings.ScenarioEditMode = settings.ScenarioEditMode || this.PersistentEditMode;
            checkpoint.SessionName = saveName;
            checkpoint.Description = this.Description;
            checkpoint.PromotedUsers = new SerializableDictionary<ulong, MyPromoteLevel>(this.PromotedUsers);
            checkpoint.RemoteAdminSettings.Dictionary.Clear();
            foreach (KeyValuePair<ulong, AdminSettingsEnum> pair in this.m_remoteAdminSettings)
            {
                checkpoint.RemoteAdminSettings[pair.Key] = pair.Value;
            }
            checkpoint.CreativeTools = this.m_creativeTools;
            checkpoint.Briefing = this.Briefing;
            checkpoint.BriefingVideo = this.BriefingVideo;
            checkpoint.Password = this.Password;
            checkpoint.LastSaveTime = DateTime.Now;
            checkpoint.WorkshopId = this.WorkshopId;
            checkpoint.ElapsedGameTime = this.ElapsedGameTime.Ticks;
            checkpoint.InGameTime = this.InGameTime;
            checkpoint.Settings = settings;
            checkpoint.Mods = this.Mods;
            checkpoint.CharacterToolbar = MyToolbarComponent.CharacterToolbar.GetObjectBuilder();
            checkpoint.Scenario = (SerializableDefinitionId) this.Scenario.Id;
            BoundingBoxD? worldBoundaries = this.WorldBoundaries;
            if (worldBoundaries != null)
            {
                nullable1 = new SerializableBoundingBoxD?(worldBoundaries.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            checkpoint.WorldBoundaries = nullable1;
            checkpoint.PreviousEnvironmentHostility = this.PreviousEnvironmentHostility;
            checkpoint.RequiresDX = this.RequiresDX;
            checkpoint.CustomLoadingScreenImage = this.CustomLoadingScreenImage;
            checkpoint.CustomLoadingScreenText = this.CustomLoadingScreenText;
            checkpoint.CustomSkybox = this.CustomSkybox;
            checkpoint.GameDefinition = (SerializableDefinitionId) this.GameDefinition.Id;
            checkpoint.SessionComponentDisabled = this.SessionComponentDisabled;
            checkpoint.SessionComponentEnabled = this.SessionComponentEnabled;
            Sync.Players.SavePlayers(checkpoint);
            this.Toolbars.SaveToolbars(checkpoint);
            this.Cameras.SaveCameraCollection(checkpoint);
            this.Gpss.SaveGpss(checkpoint);
            if (MyFakes.ENABLE_MISSION_TRIGGERS)
            {
                checkpoint.MissionTriggers = MySessionComponentMissionTriggers.Static.GetObjectBuilder();
            }
            checkpoint.Factions = !MyFakes.SHOW_FACTIONS_GUI ? null : this.Factions.GetObjectBuilder();
            checkpoint.Identities = Sync.Players.SaveIdentities();
            checkpoint.RespawnCooldowns = new List<MyObjectBuilder_Checkpoint.RespawnCooldownItem>();
            Sync.Players.RespawnComponent.SaveToCheckpoint(checkpoint);
            checkpoint.ControlledEntities = Sync.Players.SerializeControlledEntities();
            checkpoint.SpectatorPosition = new MyPositionAndOrientation(ref matrix);
            checkpoint.SpectatorIsLightOn = MySpectatorCameraController.Static.IsLightOn;
            checkpoint.SpectatorDistance = (float) MyThirdPersonSpectator.Static.GetViewerDistance();
            checkpoint.CameraController = cameraControllerEnum;
            if (cameraControllerEnum == MyCameraControllerEnum.Entity)
            {
                checkpoint.CameraEntity = ((VRage.Game.Entity.MyEntity) this.CameraController).EntityId;
            }
            if (this.ControlledEntity == null)
            {
                checkpoint.ControlledObject = -1L;
            }
            else
            {
                checkpoint.ControlledObject = this.ControlledEntity.Entity.EntityId;
                if (this.ControlledEntity is MyCharacter)
                {
                }
            }
            checkpoint.AppVersion = (int) MyFinalBuildConstants.APP_VERSION;
            checkpoint.Clients = this.SaveMembers(false);
            checkpoint.NonPlayerIdentities = Sync.Players.SaveNpcIdentities();
            this.SaveSessionComponentObjectBuilders(checkpoint);
            checkpoint.ScriptManagerData = this.ScriptManager.GetObjectBuilder();
            this.GatherVicinityInformation(checkpoint);
            if (this.OnSavingCheckpoint != null)
            {
                this.OnSavingCheckpoint(checkpoint);
            }
            return checkpoint;
        }

        public T GetComponent<T>() where T: MySessionComponentBase
        {
            MySessionComponentBase base2;
            this.m_sessionComponents.TryGetValue(typeof(T), out base2);
            return (base2 as T);
        }

        private bool GetComponentInfo(System.Type type, out MyDefinitionId? definition)
        {
            string key = null;
            if (this.m_componentsToLoad.Contains(type.Name))
            {
                key = type.Name;
            }
            else if (this.m_componentsToLoad.Contains(type.FullName))
            {
                key = type.FullName;
            }
            if (key != null)
            {
                this.GameDefinition.SessionComponents.TryGetValue(key, out definition);
                return true;
            }
            definition = 0;
            return false;
        }

        public static float GetIdentityLoginTimeSeconds(long identityId)
        {
            MyIdentity identity = Static.Players.TryGetIdentity(identityId);
            if (identity == null)
            {
                return 0f;
            }
            return (float) ((int) (DateTime.Now - identity.LastLoginTime).TotalSeconds);
        }

        public static float GetIdentityLogoutTimeSeconds(long identityId)
        {
            MyPlayer.PlayerId id;
            if (Static.Players.TryGetPlayerId(identityId, out id) && (Static.Players.GetPlayerById(id) != null))
            {
                return 0f;
            }
            MyIdentity identity = Static.Players.TryGetIdentity(identityId);
            if (identity == null)
            {
                return 0f;
            }
            return (float) ((int) (DateTime.Now - identity.LastLogoutTime).TotalSeconds);
        }

        private static MyLobbyType GetLobbyType(MyOnlineModeEnum onlineMode)
        {
            switch (onlineMode)
            {
                case MyOnlineModeEnum.PUBLIC:
                    return MyLobbyType.Public;

                case MyOnlineModeEnum.FRIENDS:
                    return MyLobbyType.FriendsOnly;

                case MyOnlineModeEnum.PRIVATE:
                    return MyLobbyType.Private;
            }
            return MyLobbyType.Private;
        }

        private static Vector3D GetLocalPlayerPosition()
        {
            if ((Static != null) && (Static.LocalHumanPlayer != null))
            {
                return Static.LocalHumanPlayer.GetPosition();
            }
            return new Vector3D();
        }

        private static IMyCamera GetMainCamera() => 
            MySector.MainCamera;

        public static MyNotificationSingletons GetNotificationForLimitResult(LimitResult result)
        {
            switch (result)
            {
                case LimitResult.MaxGridSize:
                    return MyNotificationSingletons.LimitsGridSize;

                case LimitResult.NoFaction:
                    return MyNotificationSingletons.LimitsNoFaction;

                case LimitResult.BlockTypeLimit:
                    return MyNotificationSingletons.LimitsPerBlockType;

                case LimitResult.MaxBlocksPerPlayer:
                    return MyNotificationSingletons.LimitsPlayer;

                case LimitResult.PCU:
                    return MyNotificationSingletons.LimitsPCU;
            }
            return MyNotificationSingletons.LimitsPCU;
        }

        public static float GetOwnerLoginTimeSeconds(MyCubeGrid grid) => 
            ((grid != null) ? ((grid.BigOwners.Count != 0) ? GetIdentityLoginTimeSeconds(grid.BigOwners[0]) : 0f) : 0f);

        public static float GetOwnerLogoutTimeSeconds(MyCubeGrid grid) => 
            ((grid != null) ? ((grid.BigOwners.Count != 0) ? GetIdentityLogoutTimeSeconds(grid.BigOwners[0]) : 0f) : 0f);

        public List<MyObjectBuilder_Planet> GetPlanetObjectBuilders()
        {
            List<MyObjectBuilder_Planet> list = new List<MyObjectBuilder_Planet>();
            foreach (MyPlanet planet in this.VoxelMaps.Instances)
            {
                if (planet != null)
                {
                    list.Add(planet.GetObjectBuilder(false) as MyObjectBuilder_Planet);
                }
            }
            return list;
        }

        public static float GetPlayerDistance(VRage.Game.Entity.MyEntity entity, ICollection<MyPlayer> players)
        {
            MatrixD worldMatrix = entity.WorldMatrix;
            Vector3D translation = worldMatrix.Translation;
            float maxValue = float.MaxValue;
            using (IEnumerator<MyPlayer> enumerator = players.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Sandbox.Game.Entities.IMyControllableEntity controlledEntity = enumerator.Current.Controller.ControlledEntity;
                    if (controlledEntity != null)
                    {
                        float num2 = Vector3.DistanceSquared((Vector3) controlledEntity.Entity.WorldMatrix.Translation, (Vector3) translation);
                        if (num2 < maxValue)
                        {
                            maxValue = num2;
                        }
                    }
                }
            }
            return (float) Math.Sqrt((double) maxValue);
        }

        public MyObjectBuilder_Sector GetSector(bool includeEntities = true)
        {
            MyObjectBuilder_Sector sector = null;
            sector = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Sector>();
            if (includeEntities)
            {
                sector.SectorObjects = Sandbox.Game.Entities.MyEntities.Save();
            }
            sector.SectorEvents = MyGlobalEvents.GetObjectBuilder();
            sector.Environment = MySector.GetEnvironmentSettings();
            sector.AppVersion = (int) MyFinalBuildConstants.APP_VERSION;
            return sector;
        }

        public MyObjectBuilder_SessionSettings.ExperimentalReason GetSettingsExperimentalReason()
        {
            if (!this.m_experimentalReasonInited)
            {
                this.m_experimentalReasonInited = true;
                this.m_experimentalReason = this.Settings.GetExperimentalReason(false);
                if ((Sync.IsServer && (!Sync.IsDedicated && (this.OnlineMode != MyOnlineModeEnum.OFFLINE))) && (this.TotalPCU > 0xc350))
                {
                    this.m_experimentalReason |= MyObjectBuilder_SessionSettings.ExperimentalReason.EnableRespawnShips;
                }
                if (MySandboxGame.Config.ExperimentalMode && ((Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null) || Sandbox.Engine.Multiplayer.MyMultiplayer.Static.IsServerExperimental))
                {
                    this.m_experimentalReason |= MyObjectBuilder_SessionSettings.ExperimentalReason.ExperimentalTurnedOnInConfiguration;
                }
                if (MySandboxGame.InsufficientHardware)
                {
                    this.m_experimentalReason |= MyObjectBuilder_SessionSettings.ExperimentalReason.EnableSpectator;
                }
                if (this.Mods.Count > 0)
                {
                    this.m_experimentalReason |= MyObjectBuilder_SessionSettings.ExperimentalReason.EnableCopyPaste;
                }
                if (((MySandboxGame.ConfigDedicated != null) && (MySandboxGame.ConfigDedicated.Plugins != null)) && (MySandboxGame.ConfigDedicated.Plugins.Count != 0))
                {
                    this.m_experimentalReason |= MyObjectBuilder_SessionSettings.ExperimentalReason.Plugins;
                }
            }
            return this.m_experimentalReason;
        }

        private string GetTempSavingFolder() => 
            Path.Combine(this.CurrentPath, ".new");

        public MyPromoteLevel GetUserPromoteLevel(ulong steamId)
        {
            MyPromoteLevel level;
            if (Static.OnlineMode == MyOnlineModeEnum.OFFLINE)
            {
                return MyPromoteLevel.Owner;
            }
            if ((Static.OnlineMode != MyOnlineModeEnum.OFFLINE) && (steamId == Sync.ServerId))
            {
                return MyPromoteLevel.Owner;
            }
            Static.PromotedUsers.TryGetValue(steamId, out level);
            return level;
        }

        public bool GetVoxelHandAvailable(MyCharacter character)
        {
            MyPlayer playerFromCharacter = MyPlayer.GetPlayerFromCharacter(character);
            return ((playerFromCharacter != null) ? this.GetVoxelHandAvailable(playerFromCharacter.Client.SteamUserId) : false);
        }

        public bool GetVoxelHandAvailable(ulong user) => 
            (this.Settings.EnableVoxelHand && (!this.SurvivalMode || this.CreativeToolsEnabled(user)));

        public Dictionary<string, byte[]> GetVoxelMapsArray(bool includeChanged) => 
            this.VoxelMaps.GetVoxelMapsArray(includeChanged);

        private void GetVoxelMaterials(HashSet<string> voxelMaterials, MyVoxelBase voxel, int lod, Vector3D center, float radius)
        {
            MyShapeSphere sphere1 = new MyShapeSphere();
            sphere1.Center = center;
            sphere1.Radius = radius;
            MyShapeSphere shape = sphere1;
            foreach (byte num in voxel.GetMaterialsInShape(shape, lod))
            {
                MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(num);
                if (voxelMaterialDefinition != null)
                {
                    voxelMaterials.Add(voxelMaterialDefinition.Id.SubtypeName);
                }
            }
        }

        private static ulong GetVoxelsSizeInBytes(string sessionPath)
        {
            ulong num = 0UL;
            string[] strArray = Directory.GetFiles(sessionPath, "*.vx2", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < strArray.Length; i++)
            {
                using (Stream stream = MyFileSystem.OpenRead(strArray[i]))
                {
                    num += (ulong) stream.Length;
                }
            }
            return num;
        }

        public MyObjectBuilder_World GetWorld(bool includeEntities = true)
        {
            MyObjectBuilder_World world1 = new MyObjectBuilder_World();
            world1.Checkpoint = this.GetCheckpoint(this.Name);
            world1.Sector = this.GetSector(includeEntities);
            world1.VoxelMaps = includeEntities ? new SerializableDictionary<string, byte[]>(Static.GetVoxelMapsArray(false)) : new SerializableDictionary<string, byte[]>();
            return world1;
        }

        private static BoundingBoxD GetWorldBoundaries()
        {
            if ((Static != null) && (Static.WorldBoundaries != null))
            {
                return Static.WorldBoundaries.Value;
            }
            return new BoundingBoxD();
        }

        public void HandleInput()
        {
            using (Dictionary<System.Type, MySessionComponentBase>.ValueCollection.Enumerator enumerator = this.m_sessionComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.HandleInput();
                }
            }
        }

        public bool HasPlayerCreativeRights(ulong steamId) => 
            ((Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null) || (this.IsUserSpaceMaster(steamId) || this.CreativeMode));

        public bool HasPlayerSpectatorRights(ulong steamId) => 
            (this.CreativeMode || (this.Settings.EnableSpectator || (this.IsUserAdmin(steamId) || (this.IsUserModerator(steamId) && this.CreativeToolsEnabled(steamId)))));

        private void InitDataComponents()
        {
            foreach (MySessionComponentBase base2 in this.m_sessionComponents.Values)
            {
                if (!base2.Initialized)
                {
                    MyObjectBuilder_SessionComponent sessionComponent = null;
                    if (base2.ObjectBuilderType != MyObjectBuilderType.Invalid)
                    {
                        sessionComponent = (MyObjectBuilder_SessionComponent) Activator.CreateInstance((System.Type) base2.ObjectBuilderType);
                    }
                    base2.Init(sessionComponent);
                }
            }
        }

        private void InitializeFactions()
        {
            this.Factions.CreateDefaultFactions();
        }

        public static void InitiateDump()
        {
            VRage.Profiler.MyRenderProfiler.SetLevel(-1);
            m_profilerDumpDelay = 60;
        }

        public bool IsCameraControlledObject() => 
            ReferenceEquals(this.ControlledEntity, Static.CameraController);

        public bool IsCameraUserAnySpectator() => 
            ((MySpectatorCameraController.Static == null) || (ReferenceEquals(Static.CameraController, MySpectatorCameraController.Static) && (MySpectatorCameraController.Static.SpectatorCameraMovement != MySpectatorCameraMovementEnum.None)));

        public bool IsCameraUserControlledSpectator() => 
            ((MySpectatorCameraController.Static == null) || (ReferenceEquals(Static.CameraController, MySpectatorCameraController.Static) && ((MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.UserControlled) || ((MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.Orbit) || (MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.FreeMouse)))));

        public static bool IsCompatibleVersion(MyObjectBuilder_Checkpoint checkpoint) => 
            ((checkpoint != null) ? (checkpoint.AppVersion <= MyFinalBuildConstants.APP_VERSION) : false);

        public bool IsCopyPastingEnabledForUser(ulong user)
        {
            if (!this.CreativeToolsEnabled(user) || !this.HasPlayerCreativeRights(user))
            {
                return (this.CreativeMode && this.Settings.EnableCopyPaste);
            }
            return true;
        }

        public bool IsPausable() => 
            !Sync.MultiplayerActive;

        public bool IsSettingsExperimental() => 
            ((this.GetSettingsExperimentalReason() != ~(MyObjectBuilder_SessionSettings.ExperimentalReason.AdaptiveSimulationQuality | MyObjectBuilder_SessionSettings.ExperimentalReason.BlockLimitsEnabled | MyObjectBuilder_SessionSettings.ExperimentalReason.CargoShipsEnabled | MyObjectBuilder_SessionSettings.ExperimentalReason.DestructibleBlocks | MyObjectBuilder_SessionSettings.ExperimentalReason.Enable3rdPersonView | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableConvertToStation | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableCopyPaste | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableDrones | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableEncounters | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableIngameScripts | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableJetpack | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableOxygen | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableOxygenPressurization | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableRemoteBlockRemoval | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableRespawnShips | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableSpectator | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableSpiders | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableSubgridDamage | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableSunRotation | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableToolShake | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableTurretsFriendlyFire | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableVoxelDestruction | MyObjectBuilder_SessionSettings.ExperimentalReason.EnableWolfs | MyObjectBuilder_SessionSettings.ExperimentalReason.ExperimentalTurnedOnInConfiguration | MyObjectBuilder_SessionSettings.ExperimentalReason.FloraDensity | MyObjectBuilder_SessionSettings.ExperimentalReason.MaxPlayers | MyObjectBuilder_SessionSettings.ExperimentalReason.PermanentDeath | MyObjectBuilder_SessionSettings.ExperimentalReason.Plugins | MyObjectBuilder_SessionSettings.ExperimentalReason.ProceduralDensity | MyObjectBuilder_SessionSettings.ExperimentalReason.ResetOwnership | MyObjectBuilder_SessionSettings.ExperimentalReason.ThrusterDamage | MyObjectBuilder_SessionSettings.ExperimentalReason.WeaponsEnabled)) && !MyCampaignManager.Static.IsCampaignRunning);

        public bool IsUserAdmin(ulong steamId) => 
            (this.GetUserPromoteLevel(steamId) >= MyPromoteLevel.Admin);

        public bool IsUserModerator(ulong steamId) => 
            (this.GetUserPromoteLevel(steamId) >= MyPromoteLevel.Moderator);

        public bool IsUserOwner(ulong steamId) => 
            (this.GetUserPromoteLevel(steamId) >= MyPromoteLevel.Owner);

        public bool IsUserScripter(ulong steamId) => 
            (this.EnableScripterRole ? (this.GetUserPromoteLevel(steamId) >= MyPromoteLevel.Scripter) : true);

        public bool IsUserSpaceMaster(ulong steamId) => 
            (this.GetUserPromoteLevel(steamId) >= MyPromoteLevel.SpaceMaster);

        public LimitResult IsWithinWorldLimits(out string failedBlockType, long ownerID, string blockName, int pcuToBuild, int blocksToBuild = 0, int blocksCount = 0, Dictionary<string, int> blocksPerType = null)
        {
            failedBlockType = null;
            if (this.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
            {
                return LimitResult.Passed;
            }
            MyIdentity identity = this.Players.TryGetIdentity(ownerID);
            if ((this.MaxGridSize != 0) && ((blocksCount + blocksToBuild) > this.MaxGridSize))
            {
                return LimitResult.MaxGridSize;
            }
            if (identity != null)
            {
                MyBlockLimits blockLimits = identity.BlockLimits;
                if ((this.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION) && (this.Factions.GetPlayerFaction(identity.IdentityId) == null))
                {
                    return LimitResult.NoFaction;
                }
                if (blockLimits != null)
                {
                    if ((this.MaxBlocksPerPlayer != 0) && ((blockLimits.BlocksBuilt + blocksToBuild) > blockLimits.MaxBlocks))
                    {
                        return LimitResult.MaxBlocksPerPlayer;
                    }
                    if ((this.TotalPCU != 0) && (pcuToBuild > blockLimits.PCU))
                    {
                        return LimitResult.PCU;
                    }
                    if (blocksPerType != null)
                    {
                        using (Dictionary<string, short>.Enumerator enumerator = this.BlockTypeLimits.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                KeyValuePair<string, short> current = enumerator.Current;
                                if (blocksPerType.ContainsKey(current.Key))
                                {
                                    MyBlockLimits.MyTypeLimitData data;
                                    int num = blocksPerType[current.Key];
                                    if (blockLimits.BlockTypeBuilt.TryGetValue(current.Key, out data))
                                    {
                                        num += data.BlocksBuilt;
                                    }
                                    if (num > this.GetBlockTypeLimit(current.Key))
                                    {
                                        return LimitResult.BlockTypeLimit;
                                    }
                                }
                            }
                            goto TR_0006;
                        }
                    }
                    short blockTypeLimit = this.GetBlockTypeLimit(blockName);
                    if (blockTypeLimit > 0)
                    {
                        MyBlockLimits.MyTypeLimitData data2;
                        if (blockLimits.BlockTypeBuilt.TryGetValue(blockName, out data2))
                        {
                            blocksToBuild += data2.BlocksBuilt;
                        }
                        if (blocksToBuild > blockTypeLimit)
                        {
                            return LimitResult.BlockTypeLimit;
                        }
                    }
                }
            }
        TR_0006:
            return LimitResult.Passed;
        }

        public static void Load(string sessionPath, MyObjectBuilder_Checkpoint checkpoint, ulong checkpointSizeInBytes, bool saveLastStates = true, bool allowXml = true)
        {
            ulong num;
            MyLog.Default.WriteLineAndConsole("Loading session: " + sessionPath);
            MyEntityIdentifier.Reset();
            MyEntityIdentifier.SetSingleThreadClearWarnings(MySandboxGame.Config.SyncRendering);
            bool needsXml = false;
            MyObjectBuilder_Sector sector = MyLocalCache.LoadSector(sessionPath, (Vector3I) checkpoint.CurrentSector, allowXml, out num, out needsXml);
            if (sector == null)
            {
                if (!allowXml & needsXml)
                {
                    throw new MyLoadingNeedXMLException();
                }
                throw new ApplicationException("Sector could not be loaded");
            }
            ulong voxelsSizeInBytes = GetVoxelsSizeInBytes(sessionPath);
            MyCubeGrid.Preload();
            Static = new MySession();
            Static.Name = MyStatControlText.SubstituteTexts(checkpoint.SessionName, null);
            Static.Description = checkpoint.Description;
            Static.Mods = checkpoint.Mods;
            Static.Settings = checkpoint.Settings;
            Static.CurrentPath = sessionPath;
            Static.WorldSizeInBytes = (checkpointSizeInBytes + num) + voxelsSizeInBytes;
            MyLog.Default.WriteLineAndConsole("Experimental mode: " + (Static.IsSettingsExperimental() ? "Yes" : "No"));
            MyLog.Default.WriteLineAndConsole("Experimental mode reason: " + Static.GetSettingsExperimentalReason());
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (Static.OnlineMode != MyOnlineModeEnum.OFFLINE))
            {
                StartServerRequest();
            }
            if (BeforeLoading != null)
            {
                BeforeLoading();
            }
            MySandboxGame.Static.SessionCompatHelper.FixSessionComponentObjectBuilders(checkpoint, sector);
            Static.PrepareBaseSession(checkpoint, sector);
            MyVisualScriptLogicProvider.Init();
            Static.LoadWorld(checkpoint, sector);
            if (Sync.IsServer)
            {
                Static.InitializeFactions();
            }
            if (saveLastStates)
            {
                MyLocalCache.SaveLastSessionInfo(sessionPath, false, false, Static.Name, null, 0);
            }
            Static.LogSettings(null, 0);
            object[] arguments = new object[] { Static.Name };
            MyHud.Notifications.Get(MyNotificationSingletons.WorldLoaded).SetTextFormatArguments(arguments);
            MyHud.Notifications.Add(MyNotificationSingletons.WorldLoaded);
            if (!MyFakes.LOAD_UNCONTROLLED_CHARACTERS && !MySessionComponentReplay.Static.HasAnyData)
            {
                Static.RemoveUncontrolledCharacters();
            }
            MyGeneralStats.Clear();
            MyHudChat.ResetChatSettings();
            Static.BeforeStartComponents();
            RaiseAfterLoading();
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (Static.LocalCharacter != null))
            {
                MyLocalCache.LoadInventoryConfig(Static.LocalCharacter, true);
            }
            MyLog.Default.WriteLineAndConsole("Session loaded");
        }

        private void LoadCamera(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (checkpoint.SpectatorDistance > 0f)
            {
                MyThirdPersonSpectator.Static.UpdateAfterSimulation();
                MyThirdPersonSpectator.Static.ResetViewerDistance(new double?((double) checkpoint.SpectatorDistance));
            }
            MySandboxGame.Log.WriteLine("Checkpoint.CameraAttachedTo: " + checkpoint.CameraEntity);
            IMyEntity objB = null;
            MyCameraControllerEnum cameraController = checkpoint.CameraController;
            if (!Static.Enable3RdPersonView && (cameraController == MyCameraControllerEnum.ThirdPersonSpectator))
            {
                cameraController = checkpoint.CameraController = MyCameraControllerEnum.Entity;
            }
            if ((checkpoint.CameraEntity == 0) && (this.ControlledEntity != null))
            {
                objB = this.ControlledEntity as VRage.Game.Entity.MyEntity;
                if (objB != null)
                {
                    MyRemoteControl controlledEntity = this.ControlledEntity as MyRemoteControl;
                    if (controlledEntity != null)
                    {
                        objB = controlledEntity.Pilot;
                    }
                    else if (!(this.ControlledEntity is IMyCameraController))
                    {
                        objB = null;
                        cameraController = MyCameraControllerEnum.Spectator;
                    }
                }
            }
            else if (Sandbox.Game.Entities.MyEntities.EntityExists(checkpoint.CameraEntity))
            {
                objB = Sandbox.Game.Entities.MyEntities.GetEntityById(checkpoint.CameraEntity, false);
            }
            else
            {
                objB = this.ControlledEntity as VRage.Game.Entity.MyEntity;
                if (objB == null)
                {
                    MyLog.Default.WriteLine("ERROR: Camera entity from checkpoint does not exists!");
                    cameraController = MyCameraControllerEnum.Spectator;
                }
                else
                {
                    cameraController = MyCameraControllerEnum.Entity;
                    if (!(this.ControlledEntity is IMyCameraController))
                    {
                        objB = null;
                        cameraController = MyCameraControllerEnum.Spectator;
                    }
                }
            }
            if ((cameraController == MyCameraControllerEnum.Spectator) && (objB != null))
            {
                cameraController = MyCameraControllerEnum.Entity;
            }
            MyEntityCameraSettings cameraSettings = null;
            bool flag = false;
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && ((cameraController == MyCameraControllerEnum.Entity) || (cameraController == MyCameraControllerEnum.ThirdPersonSpectator))) && (objB != null))
            {
                MyPlayer.PlayerId pid = (this.LocalHumanPlayer == null) ? new MyPlayer.PlayerId(Sync.MyId, 0) : this.LocalHumanPlayer.Id;
                if (Static.Cameras.TryGetCameraSettings(pid, objB.EntityId, (objB is MyCharacter) && ReferenceEquals(this.LocalCharacter, objB), out cameraSettings) && !cameraSettings.IsFirstPerson)
                {
                    cameraController = MyCameraControllerEnum.ThirdPersonSpectator;
                    flag = true;
                }
            }
            Static.IsCameraAwaitingEntity = false;
            Vector3D? position = null;
            this.SetCameraController(cameraController, objB, position);
            if (flag)
            {
                MyThirdPersonSpectator.Static.ResetViewerAngle(cameraSettings.HeadAngle);
                MyThirdPersonSpectator.Static.ResetViewerDistance(new double?(cameraSettings.Distance));
            }
        }

        private void LoadCameraControllerSettings(MyObjectBuilder_Checkpoint checkpoint)
        {
            this.Cameras.LoadCameraCollection(checkpoint);
        }

        private void LoadComponent(MySessionComponentBase component)
        {
            if (!component.Loaded)
            {
                foreach (System.Type type in component.Dependencies)
                {
                    MySessionComponentBase base2;
                    this.m_sessionComponents.TryGetValue(type, out base2);
                    if (base2 != null)
                    {
                        this.LoadComponent(base2);
                    }
                }
                if (this.m_loadOrder.Contains(component))
                {
                    string msg = $"Circular dependency: {component.DebugName}";
                    MySandboxGame.Log.WriteLine(msg);
                    throw new Exception(msg);
                }
                this.m_loadOrder.Add(component);
                component.LoadData();
                component.AfterLoadData();
            }
        }

        public void LoadDataComponents()
        {
            MyTimeOfDayHelper.Reset();
            this.RaiseOnLoading();
            Sync.Clients.SetLocalSteamId(Sync.MyId, !(Sandbox.Engine.Multiplayer.MyMultiplayer.Static is MyMultiplayerClient));
            Sync.Players.RegisterEvents();
            this.SetAsNotReady();
            HashSet<MySessionComponentBase> set = new HashSet<MySessionComponentBase>();
            while (true)
            {
                this.m_sessionComponents.ApplyChanges();
                foreach (MySessionComponentBase base2 in this.m_sessionComponents.Values)
                {
                    if (!set.Contains(base2))
                    {
                        this.LoadComponent(base2);
                        set.Add(base2);
                    }
                }
                if (!this.m_sessionComponents.HasChanges())
                {
                    return;
                }
            }
        }

        private void LoadGameDefinition(MyDefinitionId? gameDef = new MyDefinitionId?())
        {
            if (gameDef == null)
            {
                gameDef = new MyDefinitionId?(MyGameDefinition.Default);
            }
            Static.GameDefinition = MyDefinitionManager.Static.GetDefinition<MyGameDefinition>(gameDef.Value);
            if (Static.GameDefinition == null)
            {
                Static.GameDefinition = MyGameDefinition.DefaultDefinition;
            }
            this.RegisterComponentsFromAssemblies();
        }

        private void LoadGameDefinition(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (checkpoint.GameDefinition.IsNull())
            {
                MyDefinitionId? gameDef = null;
                this.LoadGameDefinition(gameDef);
            }
            else
            {
                Static.GameDefinition = MyDefinitionManager.Static.GetDefinition<MyGameDefinition>(checkpoint.GameDefinition);
                this.SessionComponentDisabled = checkpoint.SessionComponentDisabled;
                this.SessionComponentEnabled = checkpoint.SessionComponentEnabled;
                this.RegisterComponentsFromAssemblies();
                ShowMotD = true;
            }
        }

        private void LoadMembersFromWorld(MyObjectBuilder_World world, MyMultiplayerBase multiplayerSession)
        {
            if (multiplayerSession is MyMultiplayerClient)
            {
                (multiplayerSession as MyMultiplayerClient).LoadMembersFromWorld(world.Checkpoint.Clients);
            }
        }

        public static void LoadMission(string sessionPath, MyObjectBuilder_Checkpoint checkpoint, ulong checkpointSizeInBytes, bool persistentEditMode)
        {
            LoadMission(sessionPath, checkpoint, checkpointSizeInBytes, checkpoint.SessionName, checkpoint.Description);
            Static.PersistentEditMode = persistentEditMode;
            Static.LoadedAsMission = true;
        }

        public static void LoadMission(string sessionPath, MyObjectBuilder_Checkpoint checkpoint, ulong checkpointSizeInBytes, string name, string description)
        {
            MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Load);
            Load(sessionPath, checkpoint, checkpointSizeInBytes, true, true);
            Static.Name = name;
            Static.Description = description;
            string sessionUniqueName = MyUtils.StripInvalidChars(checkpoint.SessionName);
            Static.CurrentPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName, false, false);
            while (Directory.Exists(Static.CurrentPath))
            {
                Static.CurrentPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName + MyUtils.GetRandomInt(0x7fffffff).ToString("########"), false, false);
            }
        }

        internal static void LoadMultiplayer(MyObjectBuilder_World world, MyMultiplayerBase multiplayerSession)
        {
            BoundingBoxD? nullable1;
            if (MyFakes.ENABLE_PRELOAD_CHARACTER_ANIMATIONS)
            {
                PreloadAnimations(@"Models\Characters\Animations");
            }
            Static = new MySession(multiplayerSession.SyncLayer, true);
            Static.Mods = world.Checkpoint.Mods;
            Static.Settings = world.Checkpoint.Settings;
            Static.CurrentPath = MyLocalCache.GetSessionSavesPath(MyUtils.StripInvalidChars(world.Checkpoint.SessionName), false, false);
            if (!MyDefinitionManager.Static.TryGetDefinition<MyScenarioDefinition>(world.Checkpoint.Scenario, out Static.Scenario))
            {
                Static.Scenario = MyDefinitionManager.Static.GetScenarioDefinitions().FirstOrDefault<MyScenarioDefinition>();
            }
            FixIncorrectSettings(Static.Settings);
            SerializableBoundingBoxD? worldBoundaries = world.Checkpoint.WorldBoundaries;
            if (worldBoundaries != null)
            {
                nullable1 = new BoundingBoxD?(worldBoundaries.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            Static.WorldBoundaries = nullable1;
            Static.InGameTime = MyObjectBuilder_Checkpoint.DEFAULT_DATE;
            Static.LoadMembersFromWorld(world, multiplayerSession);
            MySandboxGame.Static.SessionCompatHelper.FixSessionComponentObjectBuilders(world.Checkpoint, world.Sector);
            Static.PrepareBaseSession(world.Checkpoint, world.Sector);
            if (MyFakes.MP_SYNC_CLUSTERTREE)
            {
                MyPhysics.DeserializeClusters(world.Clusters);
            }
            foreach (MyObjectBuilder_Planet planet in world.Planets)
            {
                MyPlanetStorageProvider dataProvider = new MyPlanetStorageProvider();
                MyPlanetGeneratorDefinition generator = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(planet.PlanetGenerator));
                dataProvider.Init((long) planet.Seed, generator, (double) planet.Radius);
                VRage.Game.Voxels.IMyStorage storage = new MyOctreeStorage(dataProvider, dataProvider.StorageSize);
                MyPlanet entity = new MyPlanet();
                entity.Init(planet, storage);
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
            }
            long controlledObject = world.Checkpoint.ControlledObject;
            world.Checkpoint.ControlledObject = -1L;
            if (multiplayerSession != null)
            {
                Static.Gpss.RegisterChat(multiplayerSession);
            }
            Static.CameraController = MySpectatorCameraController.Static;
            Static.LoadWorld(world.Checkpoint, world.Sector);
            if (Sync.IsServer)
            {
                Static.InitializeFactions();
            }
            Static.Settings.AutoSaveInMinutes = 0;
            Static.IsCameraAwaitingEntity = true;
            MyGeneralStats.Clear();
            Static.BeforeStartComponents();
        }

        internal void LoadMultiplayerWorld(MyObjectBuilder_World world, MyMultiplayerBase multiplayerSession)
        {
            Static.UnloadDataComponents(true);
            MyDefinitionManager.Static.UnloadData();
            Static.Mods = world.Checkpoint.Mods;
            Static.Settings = world.Checkpoint.Settings;
            Static.CurrentPath = MyLocalCache.GetSessionSavesPath(MyUtils.StripInvalidChars(world.Checkpoint.SessionName), false, false);
            if (!MyDefinitionManager.Static.TryGetDefinition<MyScenarioDefinition>(world.Checkpoint.Scenario, out Static.Scenario))
            {
                Static.Scenario = MyDefinitionManager.Static.GetScenarioDefinitions().FirstOrDefault<MyScenarioDefinition>();
            }
            FixIncorrectSettings(Static.Settings);
            Static.InGameTime = MyObjectBuilder_Checkpoint.DEFAULT_DATE;
            MySandboxGame.Static.SessionCompatHelper.FixSessionComponentObjectBuilders(world.Checkpoint, world.Sector);
            Static.PrepareBaseSession(world.Checkpoint, world.Sector);
            long controlledObject = world.Checkpoint.ControlledObject;
            world.Checkpoint.ControlledObject = -1L;
            Static.Gpss.RegisterChat(multiplayerSession);
            Static.CameraController = MySpectatorCameraController.Static;
            Static.LoadWorld(world.Checkpoint, world.Sector);
            if (Sync.IsServer)
            {
                Static.InitializeFactions();
            }
            Static.Settings.AutoSaveInMinutes = 0;
            Static.IsCameraAwaitingEntity = true;
            MyLocalCache.ClearLastSessionInfo();
            Static.BeforeStartComponents();
        }

        public void LoadObjectBuildersComponents(List<MyObjectBuilder_SessionComponent> objectBuilderData)
        {
            foreach (MyObjectBuilder_SessionComponent component in objectBuilderData)
            {
                MySessionComponentBase base2;
                System.Type key = MySessionComponentMapping.TryGetMappedSessionComponentType(component.GetType());
                if (key == null)
                {
                    continue;
                }
                if (this.m_sessionComponents.TryGetValue(key, out base2))
                {
                    base2.Init(component);
                }
            }
            this.InitDataComponents();
        }

        private void LoadWorld(MyObjectBuilder_Checkpoint checkpoint, MyObjectBuilder_Sector sector)
        {
            MySandboxGame.Static.SessionCompatHelper.FixSessionObjectBuilders(checkpoint, sector);
            Sandbox.Game.Entities.MyEntities.MemoryLimitAddFailureReset();
            this.ElapsedGameTime = new TimeSpan(checkpoint.ElapsedGameTime);
            this.InGameTime = checkpoint.InGameTime;
            this.Name = MyStatControlText.SubstituteTexts(checkpoint.SessionName, null);
            this.Description = checkpoint.Description;
            this.PromotedUsers = (checkpoint.PromotedUsers == null) ? new Dictionary<ulong, MyPromoteLevel>() : checkpoint.PromotedUsers.Dictionary;
            this.m_remoteAdminSettings.Clear();
            foreach (KeyValuePair<ulong, int> pair in checkpoint.RemoteAdminSettings.Dictionary)
            {
                this.m_remoteAdminSettings[pair.Key] = pair.Value;
                if (!Sync.IsDedicated && (pair.Key == Sync.MyId))
                {
                    this.m_adminSettings = pair.Value;
                }
            }
            this.m_creativeTools = (checkpoint.CreativeTools == null) ? new HashSet<ulong>() : checkpoint.CreativeTools;
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                foreach (KeyValuePair<ulong, MyPromoteLevel> pair2 in (from e in this.PromotedUsers
                    where ((MyPromoteLevel) e.Value) == MyPromoteLevel.Owner
                    select e).ToList<KeyValuePair<ulong, MyPromoteLevel>>())
                {
                    this.PromotedUsers.Remove(pair2.Key);
                }
                using (List<string>.Enumerator enumerator3 = MySandboxGame.ConfigDedicated.Administrators.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        ulong num;
                        if (!ulong.TryParse(enumerator3.Current, out num))
                        {
                            continue;
                        }
                        this.PromotedUsers[num] = MyPromoteLevel.Owner;
                    }
                }
            }
            this.Briefing = checkpoint.Briefing;
            this.BriefingVideo = checkpoint.BriefingVideo;
            this.WorkshopId = checkpoint.WorkshopId;
            this.Password = checkpoint.Password;
            this.PreviousEnvironmentHostility = checkpoint.PreviousEnvironmentHostility;
            this.RequiresDX = checkpoint.RequiresDX;
            this.CustomLoadingScreenImage = checkpoint.CustomLoadingScreenImage;
            this.CustomLoadingScreenText = checkpoint.CustomLoadingScreenText;
            this.CustomSkybox = checkpoint.CustomSkybox;
            FixIncorrectSettings(this.Settings);
            this.AppVersionFromSave = checkpoint.AppVersion;
            MyToolbarComponent.InitCharacterToolbar(checkpoint.CharacterToolbar);
            this.LoadCameraControllerSettings(checkpoint);
            Sync.Players.RespawnComponent.InitFromCheckpoint(checkpoint);
            MyPlayer.PlayerId playerId = new MyPlayer.PlayerId();
            MyPlayer.PlayerId? savingPlayerId = null;
            if (this.TryFindSavingPlayerId(checkpoint.ControlledEntities, checkpoint.ControlledObject, out playerId) && (!this.IsScenario || (Static.OnlineMode == MyOnlineModeEnum.OFFLINE)))
            {
                savingPlayerId = new MyPlayer.PlayerId?(playerId);
            }
            if ((Sync.IsServer || ((MyPerGameSettings.Game == GameEnum.ME_GAME) || (!this.IsScenario && (MyPerGameSettings.Game == GameEnum.SE_GAME)))) || (!this.IsScenario && (MyPerGameSettings.Game == GameEnum.VRS_GAME)))
            {
                Sync.Players.LoadIdentities(checkpoint, savingPlayerId);
            }
            this.GlobalBlockLimits = new MyBlockLimits(Static.TotalPCU, 0);
            this.PirateBlockLimits = new MyBlockLimits(Static.PiratePCU, 0);
            this.Toolbars.LoadToolbars(checkpoint);
            if ((checkpoint.Factions != null) && ((Sync.IsServer || (MyPerGameSettings.Game == GameEnum.ME_GAME)) || (!this.IsScenario && (MyPerGameSettings.Game == GameEnum.SE_GAME))))
            {
                Static.Factions.Init(checkpoint.Factions);
            }
            if (!Sandbox.Game.Entities.MyEntities.Load(sector.SectorObjects))
            {
                this.ShowLoadingError();
            }
            Parallel.RunCallbacks();
            MySandboxGame.Static.SessionCompatHelper.AfterEntitiesLoad(sector.AppVersion);
            MyGlobalEvents.LoadEvents(sector.SectorEvents);
            MySpectatorCameraController.Static.InitLight(checkpoint.SpectatorIsLightOn);
            if (Sync.IsServer)
            {
                MySpectatorCameraController.Static.SetViewMatrix(MatrixD.Invert(checkpoint.SpectatorPosition.GetMatrix()));
            }
            if (this.IsScenario && Static.Settings.StartInRespawnScreen)
            {
                Static.Settings.StartInRespawnScreen = false;
            }
            else
            {
                Sync.Players.LoadConnectedPlayers(checkpoint, savingPlayerId);
                Sync.Players.LoadControlledEntities(checkpoint.ControlledEntities, checkpoint.ControlledObject, savingPlayerId);
            }
            this.LoadCamera(checkpoint);
            if ((this.CreativeMode && (!Sandbox.Engine.Platform.Game.IsDedicated && ((this.LocalHumanPlayer != null) && (this.LocalHumanPlayer.Character != null)))) && this.LocalHumanPlayer.Character.IsDead)
            {
                MyPlayerCollection.RequestLocalRespawn();
            }
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnSessionReady();
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (this.LocalHumanPlayer == null))
            {
                Sync.Players.RequestNewPlayer(0, MyGameService.UserName, null, true, true);
            }
            else if (((this.ControlledEntity == null) && Sync.IsServer) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyLog.Default.WriteLine("ControlledObject was null, respawning character");
                this.m_cameraAwaitingEntity = true;
                MyPlayerCollection.RequestLocalRespawn();
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyPlayer.PlayerId pid = new MyPlayer.PlayerId(Sync.MyId, 0);
                MyToolbar toolbar = this.Toolbars.TryGetPlayerToolbar(pid);
                if (toolbar != null)
                {
                    MyToolbarComponent.InitCharacterToolbar(toolbar.GetObjectBuilder());
                }
                else
                {
                    MyToolBarCollection.RequestCreateToolbar(pid);
                    MyToolbarComponent.InitCharacterToolbar(this.Scenario.DefaultToolbar);
                }
            }
            this.Gpss.LoadGpss(checkpoint);
            if (MyFakes.ENABLE_MISSION_TRIGGERS)
            {
                MySessionComponentMissionTriggers.Static.Load(checkpoint.MissionTriggers);
            }
            MyRenderProxy.RebuildCullingStructure();
            this.Settings.ResetOwnership = false;
            if (!this.CreativeMode)
            {
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS = false;
            }
            MyRenderProxy.CollectGarbage();
        }

        private void LogMemoryUsage(string msg)
        {
            MySandboxGame.Log.WriteMemoryUsage(msg);
        }

        private void LogSettings(string scenario = null, int asteroidAmount = 0)
        {
            MyLog log = MySandboxGame.Log;
            log.WriteLine("MySession.Static.LogSettings - START", LoggingOptions.SESSION_SETTINGS);
            using (log.IndentUsing(LoggingOptions.SESSION_SETTINGS))
            {
                log.WriteLine("Name = " + this.Name, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("Description = " + this.Description, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("GameDateTime = " + this.GameDateTime, LoggingOptions.SESSION_SETTINGS);
                if (scenario != null)
                {
                    log.WriteLine("Scenario = " + scenario, LoggingOptions.SESSION_SETTINGS);
                    log.WriteLine("AsteroidAmount = " + asteroidAmount, LoggingOptions.SESSION_SETTINGS);
                }
                log.WriteLine("Password = " + this.Password, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("CurrentPath = " + this.CurrentPath, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("WorkshopId = " + this.WorkshopId, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("CameraController = " + this.CameraController, LoggingOptions.SESSION_SETTINGS);
                log.WriteLine("ThumbPath = " + this.ThumbPath, LoggingOptions.SESSION_SETTINGS);
                this.Settings.LogMembers(log, LoggingOptions.SESSION_SETTINGS);
            }
            log.WriteLine("MySession.Static.LogSettings - END", LoggingOptions.SESSION_SETTINGS);
        }

        private void OnCameraEntityClosing(VRage.Game.Entity.MyEntity entity)
        {
            Vector3D? position = null;
            this.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
        }

        [Event(null, 700), Reliable, Server]
        private static void OnCreativeToolsEnabled(bool value)
        {
            ulong steamId = MyEventContext.Current.Sender.Value;
            if (!value || !Static.HasPlayerCreativeRights(steamId))
            {
                Static.m_creativeTools.Remove(steamId);
            }
            else
            {
                Static.m_creativeTools.Add(steamId);
            }
        }

        private void OnFactionsStateChanged(MyFactionStateChange change, long fromFactionId, long toFactionId, long playerId, long sender)
        {
            string messageText = null;
            if (((change == MyFactionStateChange.FactionMemberKick) && (sender != playerId)) && (this.LocalPlayerId == playerId))
            {
                messageText = MyTexts.GetString(MyCommonTexts.MessageBoxTextYouHaveBeenKickedFromFaction);
            }
            else if (((change == MyFactionStateChange.FactionMemberAcceptJoin) && (sender != playerId)) && (this.LocalPlayerId == playerId))
            {
                messageText = MyTexts.GetString(MyCommonTexts.MessageBoxTextYouHaveBeenAcceptedToFaction);
            }
            else if (((change == MyFactionStateChange.FactionMemberNotPossibleJoin) && (sender != playerId)) && (this.LocalPlayerId == playerId))
            {
                messageText = MyTexts.GetString(MyCommonTexts.MessageBoxTextYouCannotJoinToFaction);
            }
            else if ((change == MyFactionStateChange.FactionMemberNotPossibleJoin) && (this.LocalPlayerId == sender))
            {
                messageText = MyTexts.GetString(MyCommonTexts.MessageBoxTextApplicantCannotJoinToFaction);
            }
            else if (((change == MyFactionStateChange.FactionMemberAcceptJoin) && (Static.Factions[toFactionId].IsFounder(this.LocalPlayerId) || Static.Factions[toFactionId].IsLeader(this.LocalPlayerId))) && (playerId != 0))
            {
                MyIdentity identity = Sync.Players.TryGetIdentity(playerId);
                if (identity != null)
                {
                    string displayName = identity.DisplayName;
                    messageText = string.Format(MyTexts.GetString(MyCommonTexts.Faction_PlayerJoined), displayName);
                }
            }
            else if (((change == MyFactionStateChange.FactionMemberLeave) && (Static.Factions[toFactionId].IsFounder(this.LocalPlayerId) || Static.Factions[toFactionId].IsLeader(this.LocalPlayerId))) && (playerId != 0))
            {
                MyIdentity identity2 = Sync.Players.TryGetIdentity(playerId);
                if (identity2 != null)
                {
                    string displayName = identity2.DisplayName;
                    messageText = string.Format(MyTexts.GetString(MyCommonTexts.Faction_PlayerLeft), displayName);
                }
            }
            else if (((change == MyFactionStateChange.FactionMemberSendJoin) && (Static.Factions[toFactionId].IsFounder(this.LocalPlayerId) || Static.Factions[toFactionId].IsLeader(this.LocalPlayerId))) && (playerId != 0))
            {
                MyIdentity identity3 = Sync.Players.TryGetIdentity(playerId);
                if (identity3 != null)
                {
                    string displayName = identity3.DisplayName;
                    messageText = string.Format(MyTexts.GetString(MyCommonTexts.Faction_PlayerApplied), displayName);
                }
            }
            if (messageText != null)
            {
                MyHud.Chat.ShowMessage(MyTexts.GetString(MySpaceTexts.ChatBotName), messageText, "Blue");
            }
        }

        private static void OnMultiplayerHost(bool success, string msg, MyMultiplayerBase multiplayer)
        {
            if (success)
            {
                Static.StartServer(multiplayer);
            }
            else
            {
                MyHudNotification notification = new MyHudNotification(MyCommonTexts.MultiplayerErrorStartingServer, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                object[] arguments = new object[] { msg };
                notification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(notification);
            }
        }

        [Event(null, 0x319), Reliable, ServerInvoked, Broadcast]
        private static void OnPromoteLevelSet(ulong steamId, MyPromoteLevel level)
        {
            AdminSettingsEnum enum2;
            if (level == MyPromoteLevel.None)
            {
                Static.PromotedUsers.Remove(steamId);
            }
            else
            {
                Static.PromotedUsers[steamId] = level;
            }
            if (Static.RemoteAdminSettings.TryGetValue(steamId, out enum2))
            {
                if (!Static.IsUserAdmin(steamId))
                {
                    Static.RemoteAdminSettings[steamId] = enum2 & ~AdminSettingsEnum.AdminOnly;
                    if (steamId == Sync.MyId)
                    {
                        Static.AdminSettings = Static.RemoteAdminSettings[steamId];
                    }
                }
                else if (!Static.IsUserModerator(steamId))
                {
                    Static.RemoteAdminSettings[steamId] = AdminSettingsEnum.None;
                    if (steamId == Sync.MyId)
                    {
                        Static.AdminSettings = Static.RemoteAdminSettings[steamId];
                    }
                }
            }
            if (Static.OnUserPromoteLevelChanged != null)
            {
                Static.OnUserPromoteLevelChanged(steamId, level);
            }
        }

        [Event(null, 0xaf6), Reliable, Server]
        private static void OnRequestVicinityInformation(long entityId)
        {
            SendVicinityInformation(entityId, MyEventContext.Current.Sender);
        }

        [Event(null, 0x36d), Reliable, Broadcast]
        private static void OnServerPerformanceWarning(string key, MySimpleProfiler.ProfilingBlockType type)
        {
            MySimpleProfiler.ShowServerPerformanceWarning(key, type);
        }

        [Event(null, 860), Reliable, Broadcast]
        private static void OnServerSaving(bool saveStarted)
        {
            Static.ServerSaving = saveStarted;
            if (Static.ServerSaving)
            {
                MySandboxGame.PausePush();
            }
            else
            {
                MySandboxGame.PausePop();
            }
        }

        [Event(null, 0xb0b), Reliable, Client]
        private static void OnVicinityInformation(List<string> voxels, List<string> models, List<string> armorModels)
        {
            PreloadVicinityCache(voxels, models, armorModels);
        }

        private void PerformanceWarning(MySimpleProfiler.MySimpleProfilingBlock block)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, MySimpleProfiler.ProfilingBlockType>(s => new Action<string, MySimpleProfiler.ProfilingBlockType>(MySession.OnServerPerformanceWarning), block.Name, block.Type, targetEndpoint, position);
        }

        private static void PreloadAnimations(string relativeDirectory)
        {
            IEnumerable<string> source = MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, relativeDirectory), "*.mwm", MySearchOption.AllDirectories);
            if ((source != null) && source.Any<string>())
            {
                using (IEnumerator<string> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        char[] trimChars = new char[] { Path.DirectorySeparatorChar };
                        MyModels.GetModelOnlyAnimationData(enumerator.Current.Replace(MyFileSystem.ContentPath, string.Empty).TrimStart(trimChars), false);
                    }
                }
            }
        }

        private static void PreloadVicinityCache(List<string> voxels, List<string> models, List<string> armorModels)
        {
            if ((voxels != null) && (voxels.Count > 0))
            {
                byte[] materials = new byte[voxels.Count];
                int index = 0;
                foreach (string str in voxels)
                {
                    index++;
                    materials[index] = MyDefinitionManager.Static.GetVoxelMaterialDefinition(str).Index;
                }
                MyRenderProxy.PreloadVoxelMaterials(materials);
            }
            if ((models != null) && (models.Count > 0))
            {
                MyRenderProxy.PreloadModels(models, true);
            }
            if ((armorModels != null) && (armorModels.Count > 0))
            {
                MyRenderProxy.PreloadModels(armorModels, false);
            }
        }

        private void PrepareBaseSession(List<MyObjectBuilder_Checkpoint.ModItem> mods, MyScenarioDefinition definition = null)
        {
            MyGeneralStats.Static.LoadData();
            this.ScriptManager.Init(null);
            MyDefinitionManager.Static.LoadData(mods);
            this.LoadGameDefinition(new MyDefinitionId?((definition != null) ? definition.GameDefinition : MyGameDefinition.Default));
            this.Scenario = definition;
            if (definition != null)
            {
                this.WorldBoundaries = definition.WorldBoundaries;
                MySector.EnvironmentDefinition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(definition.Environment);
            }
            MySector.InitEnvironmentSettings(null);
            MyModAPIHelper.Initialize();
            this.LoadDataComponents();
            this.InitDataComponents();
            MyModAPIHelper.Initialize();
        }

        private void PrepareBaseSession(MyObjectBuilder_Checkpoint checkpoint, MyObjectBuilder_Sector sector)
        {
            BoundingBoxD? nullable1;
            MyGeneralStats.Static.LoadData();
            MyScriptCompiler.Static.Compile(MyApiTarget.Ingame, "Dummy", MyScriptCompiler.Static.GetIngameScript("", "Program", typeof(MyGridProgram).Name, "sealed"), new List<MyScriptCompiler.Message>(), "", false);
            MyGuiTextures.Static.Reload();
            this.ScriptManager.Init(checkpoint.ScriptManagerData);
            MyDefinitionManager.Static.LoadData(checkpoint.Mods);
            if (MyFakes.PRIORITIZED_VICINITY_ASSETS_LOADING && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                PreloadVicinityCache(checkpoint.VicinityVoxelCache, checkpoint.VicinityModelsCache, checkpoint.VicinityArmorModelsCache);
                foreach (MyGuiScreenLoading loading in MyScreenManager.Screens)
                {
                    if (loading != null)
                    {
                        loading.DrawLoading();
                        break;
                    }
                }
            }
            this.VirtualClients.Init();
            this.LoadGameDefinition(checkpoint);
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyGuiManager.InitFonts();
            }
            MyDefinitionManager.Static.TryGetDefinition<MyScenarioDefinition>(checkpoint.Scenario, out this.Scenario);
            SerializableBoundingBoxD? worldBoundaries = checkpoint.WorldBoundaries;
            if (worldBoundaries != null)
            {
                nullable1 = new BoundingBoxD?(worldBoundaries.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.WorldBoundaries = nullable1;
            FixIncorrectSettings(this.Settings);
            if ((this.WorldBoundaries == null) && (this.Scenario != null))
            {
                this.WorldBoundaries = this.Scenario.WorldBoundaries;
            }
            MySector.InitEnvironmentSettings(sector.Environment);
            MyModAPIHelper.Initialize();
            this.LoadDataComponents();
            this.LoadObjectBuildersComponents(checkpoint.SessionComponents);
            MyModAPIHelper.Initialize();
        }

        private static void RaiseAfterLoading()
        {
            Action afterLoading = AfterLoading;
            if (afterLoading != null)
            {
                afterLoading();
            }
        }

        private void RaiseOnLoading()
        {
            Action onLoading = OnLoading;
            if (onLoading != null)
            {
                onLoading();
            }
        }

        public void RegisterComponent(MySessionComponentBase component, MyUpdateOrder updateOrder, int priority)
        {
            this.m_sessionComponents[component.ComponentType] = component;
            component.Session = this;
            this.AddComponentForUpdate(updateOrder, component);
            this.m_sessionComponents.ApplyChanges();
        }

        private void RegisterComponentsFromAssemblies()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            this.m_componentsToLoad = new HashSet<string>();
            this.m_componentsToLoad.UnionWith(this.GameDefinition.SessionComponents.Keys);
            this.m_componentsToLoad.RemoveWhere(x => this.SessionComponentDisabled.Contains(x));
            this.m_componentsToLoad.UnionWith(this.SessionComponentEnabled);
            AssemblyName[] referencedAssemblies = executingAssembly.GetReferencedAssemblies();
            int index = 0;
            while (true)
            {
                if (index >= referencedAssemblies.Length)
                {
                    try
                    {
                        foreach (KeyValuePair<MyModContext, HashSet<MyStringId>> pair in this.ScriptManager.ScriptsPerMod)
                        {
                            MyStringId id = pair.Value.First<MyStringId>();
                            this.RegisterComponentsFromAssembly(this.ScriptManager.Scripts[id], true, pair.Key);
                        }
                    }
                    catch (Exception exception2)
                    {
                        MyLog.Default.WriteLine("Error while loading modded session components");
                        MyLog.Default.WriteLine(exception2.ToString());
                    }
                    break;
                }
                AssemblyName assemblyRef = referencedAssemblies[index];
                try
                {
                    if (assemblyRef.Name.Contains("Sandbox") || assemblyRef.Name.Equals("VRage.Game"))
                    {
                        Assembly assembly = Assembly.Load(assemblyRef);
                        object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                        if (customAttributes.Length != 0)
                        {
                            AssemblyProductAttribute attribute = customAttributes[0] as AssemblyProductAttribute;
                            if ((attribute.Product == "Sandbox") || (attribute.Product == "VRage.Game"))
                            {
                                this.RegisterComponentsFromAssembly(assembly, false, null);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine("Error while resolving session components assemblies");
                    MyLog.Default.WriteLine(exception.ToString());
                }
                index++;
            }
            try
            {
                foreach (IPlugin plugin in MyPlugins.Plugins)
                {
                    this.RegisterComponentsFromAssembly(plugin.GetType().Assembly, true, null);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                this.RegisterComponentsFromAssembly(MyPlugins.GameAssembly, false, null);
            }
            catch (Exception exception3)
            {
                MyLog.Default.WriteLine("Error while resolving session components MOD assemblies");
                MyLog.Default.WriteLine(exception3.ToString());
            }
            try
            {
                this.RegisterComponentsFromAssembly(MyPlugins.UserAssemblies, false, null);
            }
            catch (Exception exception4)
            {
                MyLog.Default.WriteLine("Error while resolving session components MOD assemblies");
                MyLog.Default.WriteLine(exception4.ToString());
            }
            this.RegisterComponentsFromAssembly(executingAssembly, false, null);
        }

        public void RegisterComponentsFromAssembly(Assembly[] assemblies, bool modAssembly = false, MyModContext context = null)
        {
            if (assemblies != null)
            {
                foreach (Assembly assembly in assemblies)
                {
                    this.RegisterComponentsFromAssembly(assembly, modAssembly, context);
                }
            }
        }

        public void RegisterComponentsFromAssembly(Assembly assembly, bool modAssembly = false, MyModContext context = null)
        {
            if (assembly != null)
            {
                MySandboxGame.Log.WriteLine("Registered modules from: " + assembly.FullName);
                foreach (System.Type type in assembly.GetTypes())
                {
                    if (Attribute.IsDefined(type, typeof(MySessionComponentDescriptor)))
                    {
                        this.TryRegisterSessionComponent(type, modAssembly, context);
                    }
                }
            }
        }

        private void RemoveUncontrolledCharacters()
        {
            if (Sync.IsServer)
            {
                foreach (MyCharacter character in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCharacter>())
                {
                    if (character.ControllerInfo.Controller != null)
                    {
                        if (!character.ControllerInfo.IsRemotelyControlled())
                        {
                            continue;
                        }
                        if (character.GetCurrentMovementState() == MyCharacterMovementEnum.Died)
                        {
                            continue;
                        }
                    }
                    MyLargeTurretBase controlledEntity = this.ControlledEntity as MyLargeTurretBase;
                    if ((controlledEntity == null) || !ReferenceEquals(controlledEntity.Pilot, character))
                    {
                        MyRemoteControl control = this.ControlledEntity as MyRemoteControl;
                        if ((control == null) || !ReferenceEquals(control.Pilot, character))
                        {
                            character.Close();
                        }
                    }
                }
                using (IEnumerator<MyCubeGrid> enumerator2 = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        using (HashSet<MySlimBlock>.Enumerator enumerator3 = enumerator2.Current.GetBlocks().GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                MyCockpit fatBlock = enumerator3.Current.FatBlock as MyCockpit;
                                if ((fatBlock != null) && (!(fatBlock is MyCryoChamber) && ((fatBlock.Pilot != null) && !ReferenceEquals(fatBlock.Pilot, this.LocalCharacter))))
                                {
                                    fatBlock.Pilot.Close();
                                    fatBlock.ClearSavedpilot();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RequestVicinityCache(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySession.OnRequestVicinityInformation), entityId, targetEndpoint, position);
        }

        public bool Save(string customSaveName = null)
        {
            MySessionSnapshot snapshot;
            if (!this.Save(out snapshot, customSaveName))
            {
                return false;
            }
            bool flag1 = snapshot.Save();
            if (flag1)
            {
                this.WorldSizeInBytes = snapshot.SavedSizeInBytes;
            }
            return flag1;
        }

        public bool Save(out MySessionSnapshot snapshot, string customSaveName = null)
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool>(x => new Action<bool>(MySession.OnServerSaving), true, targetEndpoint, position);
            }
            snapshot = new MySessionSnapshot();
            MySandboxGame.Log.WriteLine("Saving world - START");
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                string saveName = customSaveName ?? this.Name;
                if (customSaveName != null)
                {
                    if (!Path.IsPathRooted(customSaveName))
                    {
                        string directoryName = Path.GetDirectoryName(this.CurrentPath);
                        this.CurrentPath = !Directory.Exists(directoryName) ? MyLocalCache.GetSessionSavesPath(customSaveName, false, true) : Path.Combine(directoryName, customSaveName);
                    }
                    else
                    {
                        this.CurrentPath = customSaveName;
                        saveName = Path.GetFileName(customSaveName);
                    }
                }
                snapshot.TargetDir = this.CurrentPath;
                snapshot.SavingDir = this.GetTempSavingFolder();
                try
                {
                    MySandboxGame.Log.WriteLine("Making world state snapshot.");
                    this.LogMemoryUsage("Before snapshot.");
                    snapshot.CheckpointSnapshot = this.GetCheckpoint(saveName);
                    snapshot.SectorSnapshot = this.GetSector(true);
                    snapshot.CompressedVoxelSnapshots = this.VoxelMaps.GetVoxelMapsData(true, true, null);
                    Dictionary<string, VRage.Game.Voxels.IMyStorage> voxelStorageNameCache = new Dictionary<string, VRage.Game.Voxels.IMyStorage>();
                    snapshot.VoxelSnapshots = this.VoxelMaps.GetVoxelMapsData(true, false, voxelStorageNameCache);
                    snapshot.VoxelStorageNameCache = voxelStorageNameCache;
                    this.LogMemoryUsage("After snapshot.");
                    this.SaveDataComponents();
                }
                catch (Exception exception)
                {
                    MySandboxGame.Log.WriteLine(exception);
                    return false;
                }
                finally
                {
                    this.SaveEnded();
                }
                this.LogMemoryUsage("Directory cleanup");
            }
            MySandboxGame.Log.WriteLine("Saving world - END");
            return true;
        }

        private void SaveComponent(MySessionComponentBase component)
        {
            component.SaveData();
        }

        public void SaveControlledEntityCameraSettings(bool isFirstPerson)
        {
            if ((this.ControlledEntity != null) && (this.LocalHumanPlayer != null))
            {
                MyCharacter controlledEntity = this.ControlledEntity as MyCharacter;
                if ((controlledEntity == null) || !controlledEntity.IsDead)
                {
                    this.Cameras.SaveEntityCameraSettings(this.LocalHumanPlayer.Id, this.ControlledEntity.Entity.EntityId, isFirstPerson, MyThirdPersonSpectator.Static.GetViewerDistance(), (controlledEntity != null) && ReferenceEquals(this.LocalCharacter, this.ControlledEntity), this.ControlledEntity.HeadLocalXAngle, this.ControlledEntity.HeadLocalYAngle, true);
                }
            }
        }

        public void SaveDataComponents()
        {
            foreach (MySessionComponentBase base2 in this.m_sessionComponents.Values)
            {
                this.SaveComponent(base2);
            }
        }

        public void SaveEnded()
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool>(x => new Action<bool>(MySession.OnServerSaving), false, targetEndpoint, position);
            }
        }

        internal List<MyObjectBuilder_Client> SaveMembers(bool forceSave = false)
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null)
            {
                return null;
            }
            if (!forceSave && (Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Members.Count<ulong>() == 1))
            {
                using (IEnumerator<ulong> enumerator = Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Members.GetEnumerator())
                {
                    enumerator.MoveNext();
                    if (enumerator.Current == Sync.MyId)
                    {
                        return null;
                    }
                }
            }
            List<MyObjectBuilder_Client> list = new List<MyObjectBuilder_Client>();
            foreach (ulong num in Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Members)
            {
                MyObjectBuilder_Client item = new MyObjectBuilder_Client {
                    SteamId = num,
                    Name = Sandbox.Engine.Multiplayer.MyMultiplayer.Static.GetMemberName(num),
                    IsAdmin = Static.IsUserAdmin(num)
                };
                list.Add(item);
            }
            return list;
        }

        private void SaveSessionComponentObjectBuilders(MyObjectBuilder_Checkpoint checkpoint)
        {
            checkpoint.SessionComponents = new List<MyObjectBuilder_SessionComponent>();
            using (Dictionary<System.Type, MySessionComponentBase>.ValueCollection.Enumerator enumerator = this.m_sessionComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyObjectBuilder_SessionComponent objectBuilder = enumerator.Current.GetObjectBuilder();
                    if (objectBuilder != null)
                    {
                        checkpoint.SessionComponents.Add(objectBuilder);
                    }
                }
            }
        }

        public static void SendVicinityInformation(long entityId, EndpointId client)
        {
            VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(entityId, false);
            if (entityById != null)
            {
                BoundingSphereD bs = new BoundingSphereD(entityById.PositionComp.WorldMatrix.Translation, MyFakes.PRIORITIZED_CUBE_VICINITY_RADIUS);
                HashSet<string> voxelMaterials = new HashSet<string>();
                HashSet<string> models = new HashSet<string>();
                HashSet<string> armorModels = new HashSet<string>();
                Static.GatherVicinityInformation(ref bs, voxelMaterials, models, armorModels);
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<List<string>, List<string>, List<string>>(s => new Action<List<string>, List<string>, List<string>>(MySession.OnVicinityInformation), voxelMaterials.ToList<string>(), models.ToList<string>(), armorModels.ToList<string>(), client, position);
            }
        }

        public void SetAsNotReady()
        {
            this.m_framesToReady = 10;
            this.Ready = false;
        }

        public void SetCameraController(MyCameraControllerEnum cameraControllerEnum, IMyEntity cameraEntity = null, Vector3D? position = new Vector3D?())
        {
            if ((cameraEntity != null) && (this.Spectator.Position == Vector3.Zero))
            {
                this.Spectator.Position = (cameraEntity.GetPosition() + (cameraEntity.WorldMatrix.Forward * 4.0)) + (cameraEntity.WorldMatrix.Up * 2.0);
                this.Spectator.SetTarget(cameraEntity.GetPosition(), new Vector3D?(cameraEntity.PositionComp.WorldMatrix.Up));
                this.Spectator.Initialized = true;
            }
            this.CameraOnCharacter = cameraEntity is MyCharacter;
            switch (cameraControllerEnum)
            {
                case MyCameraControllerEnum.Spectator:
                    Static.CameraController = MySpectatorCameraController.Static;
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.UserControlled;
                    if (position == null)
                    {
                        break;
                    }
                    MySpectatorCameraController.Static.Position = position.Value;
                    return;

                case MyCameraControllerEnum.Entity:
                    MyEntityRespawnComponentBase base2;
                    if (cameraEntity is IMyCameraController)
                    {
                        Static.CameraController = (IMyCameraController) cameraEntity;
                        return;
                    }
                    if (cameraEntity.Components.TryGet<MyEntityRespawnComponentBase>(out base2))
                    {
                        Static.CameraController = base2;
                        return;
                    }
                    Static.CameraController = this.LocalCharacter;
                    return;

                case MyCameraControllerEnum.ThirdPersonSpectator:
                    if (cameraEntity != null)
                    {
                        Static.CameraController = (IMyCameraController) cameraEntity;
                    }
                    Static.CameraController.IsInFirstPersonView = false;
                    return;

                case MyCameraControllerEnum.SpectatorDelta:
                    Static.CameraController = MySpectatorCameraController.Static;
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.ConstantDelta;
                    if (position == null)
                    {
                        break;
                    }
                    MySpectatorCameraController.Static.Position = position.Value;
                    return;

                case MyCameraControllerEnum.SpectatorFixed:
                    Static.CameraController = MySpectatorCameraController.Static;
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.None;
                    if (position == null)
                    {
                        break;
                    }
                    MySpectatorCameraController.Static.Position = position.Value;
                    return;

                case MyCameraControllerEnum.SpectatorOrbit:
                    Static.CameraController = MySpectatorCameraController.Static;
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.Orbit;
                    if (position != null)
                    {
                        MySpectatorCameraController.Static.Position = position.Value;
                    }
                    break;

                case MyCameraControllerEnum.SpectatorFreeMouse:
                    Static.CameraController = MySpectatorCameraController.Static;
                    MySpectatorCameraController.Static.SpectatorCameraMovement = MySpectatorCameraMovementEnum.FreeMouse;
                    if (position == null)
                    {
                        break;
                    }
                    MySpectatorCameraController.Static.Position = position.Value;
                    return;

                default:
                    return;
            }
        }

        public void SetCameraTargetDistance(double distance)
        {
            double? nullable1;
            if (distance != 0.0)
            {
                nullable1 = new double?(distance);
            }
            else
            {
                nullable1 = null;
            }
            MyThirdPersonSpectator.Static.ResetViewerDistance(nullable1);
        }

        public void SetComponentUpdateOrder(MySessionComponentBase component, MyUpdateOrder order)
        {
            for (int i = 0; i <= 2; i++)
            {
                SortedSet<MySessionComponentBase> set = null;
                if ((order & (1 << (i & 0x1f))) == MyUpdateOrder.NoUpdate)
                {
                    if (this.m_sessionComponentsForUpdate.TryGetValue(1 << (i & 0x1f), out set))
                    {
                        set.Remove(component);
                    }
                }
                else
                {
                    if (!this.m_sessionComponentsForUpdate.TryGetValue(1 << (i & 0x1f), out set))
                    {
                        set = new SortedSet<MySessionComponentBase>();
                        this.m_sessionComponentsForUpdate.Add(i, set);
                    }
                    set.Add(component);
                }
            }
        }

        public void SetEntityCameraPosition(MyPlayer.PlayerId pid, IMyEntity cameraEntity)
        {
            if ((this.LocalHumanPlayer != null) && (this.LocalHumanPlayer.Id == pid))
            {
                MyEntityCameraSettings settings;
                if (!this.Cameras.TryGetCameraSettings(pid, cameraEntity.EntityId, (cameraEntity is MyCharacter) && ReferenceEquals(this.LocalCharacter, cameraEntity), out settings))
                {
                    if (this.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator)
                    {
                        MyThirdPersonSpectator.Static.RecalibrateCameraPosition(cameraEntity is MyCharacter);
                        MyThirdPersonSpectator.Static.ResetSpring();
                        MyThirdPersonSpectator.Static.UpdateZoom();
                    }
                }
                else if (!settings.IsFirstPerson)
                {
                    Vector3D? position = null;
                    this.SetCameraController(MyCameraControllerEnum.ThirdPersonSpectator, cameraEntity, position);
                    MyThirdPersonSpectator.Static.ResetViewerAngle(settings.HeadAngle);
                    MyThirdPersonSpectator.Static.ResetViewerDistance(new double?(settings.Distance));
                }
            }
        }

        public bool SetSaveOnUnloadOverride_Dedicated(bool? save)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                return false;
            }
            this.m_saveOnUnloadOverride = save;
            return true;
        }

        [Event(null, 0x92e), Client, Reliable]
        public static void SetSpectatorPositionFromServer(Vector3D position)
        {
            Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(position));
        }

        public bool SetUserPromoteLevel(ulong steamId, MyPromoteLevel level)
        {
            if ((level < MyPromoteLevel.None) || (level > MyPromoteLevel.Admin))
            {
                throw new ArgumentOutOfRangeException("level", level, null);
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ulong, MyPromoteLevel>(x => new Action<ulong, MyPromoteLevel>(MySession.OnPromoteLevelSet), steamId, level, targetEndpoint, position);
            return true;
        }

        private void ShowLoadingError()
        {
            MyStringId messageBoxTextMemoryLimitReachedDuringLoad;
            MyStringId messageBoxCaptionWarning;
            if (Sandbox.Game.Entities.MyEntities.MemoryLimitAddFailure)
            {
                messageBoxCaptionWarning = MyCommonTexts.MessageBoxCaptionWarning;
                messageBoxTextMemoryLimitReachedDuringLoad = MyCommonTexts.MessageBoxTextMemoryLimitReachedDuringLoad;
            }
            else
            {
                messageBoxCaptionWarning = MyCommonTexts.MessageBoxCaptionError;
                messageBoxTextMemoryLimitReachedDuringLoad = MyCommonTexts.MessageBoxTextErrorLoadingEntities;
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MySandboxGame.Log.WriteLineAndConsole(MyTexts.Get(messageBoxTextMemoryLimitReachedDuringLoad).ToString());
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(messageBoxCaptionWarning);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(messageBoxTextMemoryLimitReachedDuringLoad), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public static void Start(string name, string description, string password, MyObjectBuilder_SessionSettings settings, List<MyObjectBuilder_Checkpoint.ModItem> mods, MyWorldGenerator.Args generationArgs)
        {
            MyLog.Default.WriteLineAndConsole("Starting world " + name);
            MyEntityContainerEventExtensions.InitEntityEvents();
            Static = new MySession();
            Static.Name = name;
            Static.Mods = mods;
            Static.Description = description;
            Static.Password = password;
            Static.Settings = settings;
            Static.Scenario = generationArgs.Scenario;
            FixIncorrectSettings(Static.Settings);
            double x = settings.WorldSizeKm * 500;
            if (x > 0.0)
            {
                Static.WorldBoundaries = new BoundingBoxD(new Vector3D(-x, -x, -x), new Vector3D(x, x, x));
            }
            MyVisualScriptLogicProvider.Init();
            Static.InGameTime = generationArgs.Scenario.GameDate;
            Static.RequiresDX = generationArgs.Scenario.HasPlanets ? 11 : 9;
            if (Static.OnlineMode != MyOnlineModeEnum.OFFLINE)
            {
                StartServerRequest();
            }
            Static.IsCameraAwaitingEntity = true;
            string sessionUniqueName = MyUtils.StripInvalidChars(name);
            Static.CurrentPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName, false, false);
            while (Directory.Exists(Static.CurrentPath))
            {
                Static.CurrentPath = MyLocalCache.GetSessionSavesPath(sessionUniqueName + MyUtils.GetRandomInt(0x7fffffff).ToString("########"), false, false);
            }
            Static.PrepareBaseSession(mods, generationArgs.Scenario);
            MySector.EnvironmentDefinition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(generationArgs.Scenario.Environment);
            MyWorldGenerator.GenerateWorld(generationArgs);
            if (Sync.IsServer)
            {
                Static.InitializeFactions();
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyToolBarCollection.RequestCreateToolbar(new MyPlayer.PlayerId(Sync.MyId, 0));
            }
            string scenario = generationArgs.Scenario.DisplayNameText.ToString();
            Static.LogSettings(scenario, generationArgs.AsteroidAmount);
            if (generationArgs.Scenario.SunDirection.IsValid())
            {
                MySector.SunProperties.SunDirectionNormalized = Vector3.Normalize(generationArgs.Scenario.SunDirection);
                MySector.SunProperties.BaseSunDirectionNormalized = Vector3.Normalize(generationArgs.Scenario.SunDirection);
            }
            MyPrefabManager.FinishedProcessingGrids.Reset();
            if (MyPrefabManager.PendingGrids > 0)
            {
                MyPrefabManager.FinishedProcessingGrids.WaitOne();
            }
            Parallel.RunCallbacks();
            Sandbox.Game.Entities.MyEntities.UpdateOnceBeforeFrame();
            Static.BeforeStartComponents();
            Static.Save(null);
            Static.SessionSimSpeedPlayer = 0f;
            Static.SessionSimSpeedServer = 0f;
            MySpectatorCameraController.Static.InitLight(false);
        }

        internal void StartServer(MyMultiplayerBase multiplayer)
        {
            multiplayer.WorldName = this.Name;
            multiplayer.GameMode = this.Settings.GameMode;
            multiplayer.WorldSize = this.WorldSizeInBytes;
            multiplayer.AppVersion = (int) MyFinalBuildConstants.APP_VERSION;
            multiplayer.DataHash = MyDataIntegrityChecker.GetHashBase64();
            multiplayer.InventoryMultiplier = this.Settings.InventorySizeMultiplier;
            multiplayer.BlocksInventoryMultiplier = this.Settings.BlocksInventorySizeMultiplier;
            multiplayer.AssemblerMultiplier = this.Settings.AssemblerEfficiencyMultiplier;
            multiplayer.RefineryMultiplier = this.Settings.RefinerySpeedMultiplier;
            multiplayer.WelderMultiplier = this.Settings.WelderSpeedMultiplier;
            multiplayer.GrinderMultiplier = this.Settings.GrinderSpeedMultiplier;
            multiplayer.MemberLimit = this.Settings.MaxPlayers;
            multiplayer.Mods = this.Mods;
            multiplayer.ViewDistance = this.Settings.ViewDistance;
            multiplayer.SyncDistance = this.Settings.SyncDistance;
            multiplayer.Scenario = this.IsScenario;
            multiplayer.ExperimentalMode = this.IsSettingsExperimental();
            MyCachedServerItem.SendSettingsToSteam();
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                (multiplayer as MyDedicatedServerBase).SendGameTagsToSteam();
                MySimpleProfiler.ShowPerformanceWarning += new Action<MySimpleProfiler.MySimpleProfilingBlock>(this.PerformanceWarning);
            }
            if (multiplayer is MyMultiplayerLobby)
            {
                ((MyMultiplayerLobby) multiplayer).HostSteamId = Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ServerId;
            }
            Static.Gpss.RegisterChat(multiplayer);
        }

        private static void StartServerRequest()
        {
            if (MyGameService.IsOnline)
            {
                Static.UnloadMultiplayer();
                MyMultiplayerHostResult result1 = Sandbox.Engine.Multiplayer.MyMultiplayer.HostLobby(GetLobbyType(Static.OnlineMode), Static.MaxPlayers, Static.SyncLayer);
                result1.Done += new Action<bool, string, MyMultiplayerBase>(MySession.OnMultiplayerHost);
                result1.Wait(true);
            }
            else
            {
                MyHudNotification notification = new MyHudNotification(MyCommonTexts.MultiplayerErrorStartingServer, 0x2710, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                object[] arguments = new object[] { "SteamOffline - try restarting Steam" };
                notification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(notification);
            }
        }

        private bool TryFindSavingPlayerId(SerializableDictionary<long, MyObjectBuilder_Checkpoint.PlayerId> controlledEntities, long controlledObject, out MyPlayer.PlayerId playerId)
        {
            playerId = new MyPlayer.PlayerId();
            if (!MyFakes.REUSE_OLD_PLAYER_IDENTITY)
            {
                return false;
            }
            if (!Sync.IsServer || (Sync.Clients.Count != 1))
            {
                return false;
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                return false;
            }
            if (controlledEntities == null)
            {
                return false;
            }
            bool flag = false;
            foreach (KeyValuePair<long, MyObjectBuilder_Checkpoint.PlayerId> pair in controlledEntities.Dictionary)
            {
                if (pair.Key == controlledObject)
                {
                    playerId = new MyPlayer.PlayerId(pair.Value.ClientId, pair.Value.SerialId);
                }
                if ((pair.Value.ClientId == Sync.MyId) && (pair.Value.SerialId == 0))
                {
                    flag = true;
                }
            }
            return !flag;
        }

        private void TryRegisterSessionComponent(System.Type type, bool modAssembly, MyModContext context)
        {
            try
            {
                MyDefinitionId? definition = null;
                MySessionComponentBase component = (MySessionComponentBase) Activator.CreateInstance(type);
                if ((component.IsRequiredByGame | modAssembly) || this.GetComponentInfo(type, out definition))
                {
                    this.RegisterComponent(component, component.UpdateOrder, component.Priority);
                    this.GetComponentInfo(type, out definition);
                    component.Definition = definition;
                    component.ModContext = context;
                }
            }
            catch (Exception)
            {
                MySandboxGame.Log.WriteLine("Exception during loading of type : " + type.Name);
            }
        }

        public void Unload()
        {
            if (OnUnloading != null)
            {
                OnUnloading();
            }
            Parallel.Scheduler.WaitForTasksToFinish(new TimeSpan(-1L));
            Parallel.RunCallbacks();
            MySandboxGame.IsPaused = false;
            if (MyHud.RotatingWheelVisible)
            {
                MyHud.PopRotatingWheelVisible();
            }
            Sandbox.Engine.Platform.Game.EnableSimSpeedLocking = false;
            MySpectatorCameraController.Static.CleanLight();
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.ReportGameplayEnd();
            }
            MySandboxGame.Log.WriteLine("MySession::Unload START");
            MySessionSnapshot.WaitForSaving();
            MySandboxGame.Log.WriteLine("AutoSaveInMinutes: " + this.AutoSaveInMinutes);
            MySandboxGame.Log.WriteLine("MySandboxGame.IsDedicated: " + Sandbox.Engine.Platform.Game.IsDedicated.ToString());
            MySandboxGame.Log.WriteLine("IsServer: " + Sync.IsServer.ToString());
            if (((this.SaveOnUnloadOverride != null) && this.SaveOnUnloadOverride.Value) || (((this.SaveOnUnloadOverride == null) && (this.AutoSaveInMinutes > 0)) && Sandbox.Engine.Platform.Game.IsDedicated))
            {
                MySandboxGame.Log.WriteLineAndConsole("Autosave in unload");
                this.IsUnloadSaveInProgress = true;
                this.Save(null);
                this.IsUnloadSaveInProgress = false;
            }
            MySandboxGame.Static.ClearInvokeQueue();
            MyDroneAIDataStatic.Reset();
            MyAudio.Static.StopUpdatingAll3DCues();
            MyAudio.Static.Mute = true;
            MyAudio.Static.StopMusic();
            MyAudio.Static.ChangeGlobalVolume(1f, 0f);
            MyAudio.ReloadData(MyAudioExtensions.GetSoundDataFromDefinitions(), MyAudioExtensions.GetEffectData());
            MyEntity3DSoundEmitter.LastTimePlaying.Clear();
            MyParticlesLibrary.Close();
            this.Ready = false;
            this.VoxelMaps.Clear();
            MySandboxGame.Config.Save();
            if ((this.LocalHumanPlayer != null) && (this.LocalHumanPlayer.Controller != null))
            {
                this.LocalHumanPlayer.Controller.SaveCamera();
            }
            this.DisconnectMultiplayer();
            this.UnloadDataComponents(false);
            this.UnloadMultiplayer();
            MyTerminalControlFactory.Unload();
            MyDefinitionManager.Static.UnloadData();
            MyDefinitionManager.Static.PreloadDefinitions();
            MyInput.Static.ClearBlacklist();
            MyDefinitionErrors.Clear();
            MyRenderProxy.UnloadData();
            MyHud.Questlog.CleanDetails();
            MyHud.Questlog.Visible = false;
            MyAPIGateway.Clean();
            MyOxygenProviderSystem.ClearOxygenGenerators();
            MyDynamicAABBTree.Dispose();
            MyDynamicAABBTreeD.Dispose();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            MySandboxGame.Log.WriteLine("MySession::Unload END");
            if (MyCubeBuilder.AllPlayersColors != null)
            {
                MyCubeBuilder.AllPlayersColors.Clear();
            }
            if (OnUnloaded != null)
            {
                OnUnloaded();
            }
            Parallel.Scheduler.WaitForTasksToFinish(new TimeSpan(-1L));
            Parallel.Clean();
        }

        public void UnloadDataComponents(bool beforeLoadWorld = false)
        {
            MySessionComponentBase base2 = null;
            try
            {
                for (int i = this.m_loadOrder.Count - 1; i >= 0; i--)
                {
                    base2 = this.m_loadOrder[i];
                    base2.UnloadDataConditional();
                }
                MySessionComponentMapping.Clear();
                this.m_sessionComponents.Clear();
                this.m_loadOrder.Clear();
                using (Dictionary<int, SortedSet<MySessionComponentBase>>.ValueCollection.Enumerator enumerator = this.m_sessionComponentsForUpdate.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Clear();
                    }
                }
                if (!beforeLoadWorld)
                {
                    Sync.Players.UnregisterEvents();
                    Sync.Clients.Clear();
                    MyNetworkReader.Clear();
                }
                this.Ready = false;
            }
            catch (Exception exception)
            {
                IMyModContext modContext = base2.ModContext;
                if ((modContext == null) || modContext.IsBaseGame)
                {
                    throw;
                }
                throw new ModCrashedException(exception, modContext);
            }
        }

        private void UnloadMultiplayer()
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Static.Gpss.UnregisterChat(Sandbox.Engine.Multiplayer.MyMultiplayer.Static);
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Dispose();
                this.SyncLayer = null;
            }
        }

        public void UnregisterComponent(MySessionComponentBase component)
        {
            component.Session = null;
            this.m_sessionComponents.Remove(component.ComponentType, false);
        }

        public void Update(MyTimeSpan updateTime)
        {
            if (this.m_updateAllowed && (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null))
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.UpdateClientStateGroups();
            }
            this.CheckUpdate();
            CheckProfilerDump();
            if (MySandboxGame.Config.SyncRendering)
            {
                Parallel.Scheduler.WaitForTasksToFinish(TimeSpan.FromMilliseconds(-1.0));
            }
            Parallel.RunCallbacks();
            TimeSpan elapsedTimespan = new TimeSpan(0, 0, 0, 0, 0x10);
            if (!this.m_updateAllowed && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                if ((MySandboxGame.IsPaused && Sync.IsServer) && !Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.UpdateComponentsWhilePaused();
                }
            }
            else
            {
                if (MySandboxGame.IsPaused)
                {
                    return;
                }
                if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.UpdateBefore();
                }
                this.UpdateComponents();
                MyParticleEffectsSoundManager.UpdateEffects();
                if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.UpdateAfter();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Tick();
                }
                if (((this.CameraController == null) || !this.CameraController.IsInFirstPersonView) && (MyThirdPersonSpectator.Static != null))
                {
                    MyThirdPersonSpectator.Static.Update();
                }
                if (this.IsServer)
                {
                    this.Players.SendDirtyBlockLimits();
                }
                this.ElapsedGameTime += MyRandom.EnableDeterminism ? TimeSpan.FromMilliseconds(16.0) : elapsedTimespan;
                if ((this.m_lastTimeMemoryLogged + TimeSpan.FromSeconds(30.0)) < DateTime.UtcNow)
                {
                    MySandboxGame.Log.WriteLine($"GC Memory: {GC.GetTotalMemory(false).ToString("##,#")} B");
                    this.m_lastTimeMemoryLogged = DateTime.UtcNow;
                }
                if (((this.AutoSaveInMinutes > 0) && MySandboxGame.IsGameReady) && ((updateTime.TimeSpan - this.m_timeOfSave.TimeSpan) > TimeSpan.FromMinutes((double) this.AutoSaveInMinutes)))
                {
                    int num1;
                    MySandboxGame.Log.WriteLine("Autosave initiated");
                    MyCharacter localCharacter = this.LocalCharacter;
                    if ((localCharacter == null) || localCharacter.IsDead)
                    {
                        num1 = (int) ReferenceEquals(localCharacter, null);
                    }
                    else
                    {
                        num1 = 1;
                    }
                    bool flag = (bool) num1;
                    MySandboxGame.Log.WriteLine("Character state: " + flag.ToString());
                    flag &= Sync.IsServer;
                    MySandboxGame.Log.WriteLine("IsServer: " + Sync.IsServer.ToString());
                    flag &= !MyAsyncSaving.InProgress;
                    MySandboxGame.Log.WriteLine("MyAsyncSaving.InProgress: " + MyAsyncSaving.InProgress.ToString());
                    if (flag)
                    {
                        MySandboxGame.Log.WriteLineAndConsole("Autosave");
                        MyAsyncSaving.Start(() => MySector.ResetEyeAdaptation = true, null, false);
                    }
                    this.m_timeOfSave = updateTime;
                }
                if (MySandboxGame.IsGameReady && (this.m_framesToReady > 0))
                {
                    this.m_framesToReady--;
                    if (this.m_framesToReady == 0)
                    {
                        this.Ready = true;
                        MyMusicTrack track = new MyMusicTrack {
                            TransitionCategory = MyStringId.GetOrCompute("Default")
                        };
                        MyAudio.Static.PlayMusic(new MyMusicTrack?(track), 0);
                        if (this.OnReady != null)
                        {
                            this.OnReady();
                        }
                        MySimpleProfiler.Reset(true);
                        if (this.OnReady != null)
                        {
                            foreach (Delegate delegate2 in this.OnReady.GetInvocationList())
                            {
                                this.OnReady -= ((Action) delegate2);
                            }
                        }
                        if (Sandbox.Engine.Platform.Game.IsDedicated)
                        {
                            if (Console.IsInputRedirected || !MySandboxGame.IsConsoleVisible)
                            {
                                MyLog.Default.WriteLineAndConsole("Game ready... ");
                            }
                            else
                            {
                                MyLog.Default.WriteLineAndConsole("Game ready... Press Ctrl+C to exit");
                            }
                        }
                    }
                }
                if (Sync.MultiplayerActive && !Sync.IsServer)
                {
                    this.CheckMultiplayerStatus();
                }
                this.m_gameplayFrameCounter++;
            }
            this.UpdateStatistics(ref elapsedTimespan);
            this.DebugDraw();
        }

        public void UpdateComponents()
        {
            SortedSet<MySessionComponentBase> set = null;
            if (this.m_sessionComponentsForUpdate.TryGetValue(1, out set))
            {
                foreach (MySessionComponentBase base2 in set)
                {
                    if (base2.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                    {
                        base2.UpdateBeforeSimulation();
                    }
                }
            }
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.Simulate();
            }
            if (this.m_sessionComponentsForUpdate.TryGetValue(2, out set))
            {
                foreach (MySessionComponentBase base3 in set)
                {
                    if (base3.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                    {
                        base3.Simulate();
                    }
                }
            }
            if (this.m_sessionComponentsForUpdate.TryGetValue(4, out set))
            {
                foreach (MySessionComponentBase base4 in set)
                {
                    if (base4.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                    {
                        base4.UpdateAfterSimulation();
                    }
                }
            }
        }

        public void UpdateComponentsWhilePaused()
        {
            SortedSet<MySessionComponentBase> set = null;
            if (this.m_sessionComponentsForUpdate.TryGetValue(1, out set))
            {
                foreach (MySessionComponentBase base2 in set)
                {
                    if (base2.UpdateOnPause)
                    {
                        base2.UpdateBeforeSimulation();
                    }
                }
            }
            if (this.m_sessionComponentsForUpdate.TryGetValue(2, out set))
            {
                foreach (MySessionComponentBase base3 in set)
                {
                    if (base3.UpdateOnPause)
                    {
                        base3.Simulate();
                    }
                }
            }
            if (this.m_sessionComponentsForUpdate.TryGetValue(4, out set))
            {
                foreach (MySessionComponentBase base4 in set)
                {
                    if (base4.UpdateOnPause)
                    {
                        base4.UpdateAfterSimulation();
                    }
                }
            }
        }

        private void UpdateStatistics(ref TimeSpan elapsedTimespan)
        {
            this.ElapsedPlayTime += MyRandom.EnableDeterminism ? TimeSpan.FromMilliseconds(16.0) : elapsedTimespan;
            this.SessionSimSpeedPlayer += (float) (MyPhysics.SimulationRatio * elapsedTimespan.TotalSeconds);
            this.SessionSimSpeedServer += (float) (Sync.ServerSimulationRatio * elapsedTimespan.TotalSeconds);
            if ((this.LocalHumanPlayer != null) && (this.LocalHumanPlayer.Character != null))
            {
                if (!(this.ControlledEntity is MyCharacter))
                {
                    if (this.ControlledEntity is MyCockpit)
                    {
                        if (((MyCockpit) this.ControlledEntity).IsLargeShip())
                        {
                            this.TimePilotingBigShip += elapsedTimespan;
                        }
                        else
                        {
                            this.TimePilotingSmallShip += elapsedTimespan;
                        }
                        if (((MyCockpit) this.ControlledEntity).BuildingMode)
                        {
                            this.TimeInBuilderMode += elapsedTimespan;
                        }
                    }
                }
                else
                {
                    if (((MyCharacter) this.ControlledEntity).GetCurrentMovementState() == MyCharacterMovementEnum.Flying)
                    {
                        this.TimeOnJetpack += elapsedTimespan;
                    }
                    else
                    {
                        this.TimeOnFoot += elapsedTimespan;
                    }
                    MyCharacterSoundComponent soundComp = ((MyCharacter) this.ControlledEntity).SoundComp;
                    if (soundComp != null)
                    {
                        if (soundComp.StandingOnGrid != null)
                        {
                            if (soundComp.StandingOnGrid.IsStatic)
                            {
                                this.TimeOnStation += elapsedTimespan;
                            }
                            else
                            {
                                this.TimeOnShips += elapsedTimespan;
                            }
                        }
                        if (soundComp.StandingOnVoxel != null)
                        {
                            if (!(soundComp.StandingOnVoxel is MyVoxelPhysics) || !(((MyVoxelPhysics) soundComp.StandingOnVoxel).RootVoxel is MyPlanet))
                            {
                                this.TimeOnAsteroids += elapsedTimespan;
                            }
                            else
                            {
                                this.TimeOnPlanets += elapsedTimespan;
                            }
                        }
                    }
                }
            }
        }

        void IMySession.BeforeStartComponents()
        {
            this.BeforeStartComponents();
        }

        void IMySession.Draw()
        {
            this.Draw();
        }

        void IMySession.GameOver()
        {
            this.GameOver();
        }

        void IMySession.GameOver(MyStringId? customMessage)
        {
            this.GameOver(customMessage);
        }

        MyObjectBuilder_Checkpoint IMySession.GetCheckpoint(string saveName) => 
            this.GetCheckpoint(saveName);

        MyObjectBuilder_Sector IMySession.GetSector() => 
            this.GetSector(true);

        MyPromoteLevel IMySession.GetUserPromoteLevel(ulong steamId) => 
            this.GetUserPromoteLevel(steamId);

        Dictionary<string, byte[]> IMySession.GetVoxelMapsArray() => 
            this.GetVoxelMapsArray(true);

        MyObjectBuilder_World IMySession.GetWorld() => 
            this.GetWorld(true);

        bool IMySession.IsPausable() => 
            this.IsPausable();

        bool IMySession.IsUserAdmin(ulong steamId) => 
            Static.IsUserAdmin(steamId);

        [Obsolete("Use GetUserPromoteLevel")]
        bool IMySession.IsUserPromoted(ulong steamId) => 
            Static.IsUserSpaceMaster(steamId);

        void IMySession.RegisterComponent(MySessionComponentBase component, MyUpdateOrder updateOrder, int priority)
        {
            this.RegisterComponent(component, updateOrder, priority);
        }

        bool IMySession.Save(string customSaveName) => 
            this.Save(customSaveName);

        void IMySession.SetAsNotReady()
        {
            this.SetAsNotReady();
        }

        void IMySession.SetCameraController(MyCameraControllerEnum cameraControllerEnum, IMyEntity cameraEntity, Vector3D? position)
        {
            this.SetCameraController(cameraControllerEnum, cameraEntity, position);
        }

        void IMySession.Unload()
        {
            this.Unload();
        }

        void IMySession.UnloadDataComponents()
        {
            this.UnloadDataComponents(false);
        }

        void IMySession.UnloadMultiplayer()
        {
            this.UnloadMultiplayer();
        }

        void IMySession.UnregisterComponent(MySessionComponentBase component)
        {
            this.UnregisterComponent(component);
        }

        void IMySession.Update(MyTimeSpan time)
        {
            this.Update(time);
        }

        void IMySession.UpdateComponents()
        {
            this.UpdateComponents();
        }

        IMyVoxelMaps IMySession.VoxelMaps =>
            this.VoxelMaps;

        IMyCameraController IMySession.CameraController =>
            this.CameraController;

        float IMySession.AssemblerEfficiencyMultiplier =>
            this.AssemblerEfficiencyMultiplier;

        float IMySession.AssemblerSpeedMultiplier =>
            this.AssemblerSpeedMultiplier;

        bool IMySession.AutoHealing =>
            this.AutoHealing;

        uint IMySession.AutoSaveInMinutes =>
            this.AutoSaveInMinutes;

        bool IMySession.CargoShipsEnabled =>
            this.CargoShipsEnabled;

        bool IMySession.ClientCanSave =>
            false;

        bool IMySession.CreativeMode =>
            this.CreativeMode;

        string IMySession.CurrentPath =>
            this.CurrentPath;

        string IMySession.Description
        {
            get => 
                this.Description;
            set => 
                (this.Description = value);
        }

        TimeSpan IMySession.ElapsedPlayTime =>
            this.ElapsedPlayTime;

        bool IMySession.EnableCopyPaste =>
            this.IsCopyPastingEnabled;

        MyEnvironmentHostilityEnum IMySession.EnvironmentHostility =>
            this.EnvironmentHostility;

        DateTime IMySession.GameDateTime
        {
            get => 
                this.GameDateTime;
            set => 
                (this.GameDateTime = value);
        }

        float IMySession.GrinderSpeedMultiplier =>
            this.GrinderSpeedMultiplier;

        float IMySession.HackSpeedMultiplier =>
            this.HackSpeedMultiplier;

        float IMySession.InventoryMultiplier =>
            this.InventoryMultiplier;

        float IMySession.CharactersInventoryMultiplier =>
            this.CharactersInventoryMultiplier;

        float IMySession.BlocksInventorySizeMultiplier =>
            this.BlocksInventorySizeMultiplier;

        bool IMySession.IsCameraAwaitingEntity
        {
            get => 
                this.IsCameraAwaitingEntity;
            set => 
                (this.IsCameraAwaitingEntity = value);
        }

        bool IMySession.IsCameraControlledObject =>
            this.IsCameraControlledObject();

        bool IMySession.IsCameraUserControlledSpectator =>
            this.IsCameraUserControlledSpectator();

        short IMySession.MaxFloatingObjects =>
            this.MaxFloatingObjects;

        short IMySession.MaxBackupSaves =>
            this.MaxBackupSaves;

        short IMySession.MaxPlayers =>
            this.MaxPlayers;

        bool IMySession.MultiplayerAlive
        {
            get => 
                this.MultiplayerAlive;
            set => 
                (this.MultiplayerAlive = value);
        }

        bool IMySession.MultiplayerDirect
        {
            get => 
                this.MultiplayerDirect;
            set => 
                (this.MultiplayerDirect = value);
        }

        double IMySession.MultiplayerLastMsg
        {
            get => 
                this.MultiplayerLastMsg;
            set => 
                (this.MultiplayerLastMsg = value);
        }

        string IMySession.Name
        {
            get => 
                this.Name;
            set => 
                (this.Name = value);
        }

        float IMySession.NegativeIntegrityTotal
        {
            get => 
                this.NegativeIntegrityTotal;
            set => 
                (this.NegativeIntegrityTotal = value);
        }

        MyOnlineModeEnum IMySession.OnlineMode =>
            this.OnlineMode;

        string IMySession.Password
        {
            get => 
                this.Password;
            set => 
                (this.Password = value);
        }

        float IMySession.PositiveIntegrityTotal
        {
            get => 
                this.PositiveIntegrityTotal;
            set => 
                (this.PositiveIntegrityTotal = value);
        }

        float IMySession.RefinerySpeedMultiplier =>
            this.RefinerySpeedMultiplier;

        bool IMySession.ShowPlayerNamesOnHud =>
            this.ShowPlayerNamesOnHud;

        bool IMySession.SurvivalMode =>
            this.SurvivalMode;

        bool IMySession.ThrusterDamage =>
            this.ThrusterDamage;

        string IMySession.ThumbPath =>
            this.ThumbPath;

        TimeSpan IMySession.TimeOnBigShip =>
            this.TimePilotingBigShip;

        TimeSpan IMySession.TimeOnFoot =>
            this.TimeOnFoot;

        TimeSpan IMySession.TimeOnJetpack =>
            this.TimeOnJetpack;

        TimeSpan IMySession.TimeOnSmallShip =>
            this.TimePilotingSmallShip;

        bool IMySession.WeaponsEnabled =>
            this.WeaponsEnabled;

        float IMySession.WelderSpeedMultiplier =>
            this.WelderSpeedMultiplier;

        ulong? IMySession.WorkshopId =>
            this.WorkshopId;

        IMyPlayer IMySession.Player =>
            this.LocalHumanPlayer;

        VRage.Game.ModAPI.Interfaces.IMyControllableEntity IMySession.ControlledObject =>
            this.ControlledEntity;

        MyObjectBuilder_SessionSettings IMySession.SessionSettings =>
            this.Settings;

        IMyFactionCollection IMySession.Factions =>
            this.Factions;

        IMyCamera IMySession.Camera =>
            MySector.MainCamera;

        double IMySession.CameraTargetDistance
        {
            get => 
                ((double) this.GetCameraTargetDistance());
            set => 
                this.SetCameraTargetDistance(value);
        }

        public IMyConfig Config =>
            MySandboxGame.Config;

        IMyDamageSystem IMySession.DamageSystem =>
            MyDamageSystem.Static;

        IMyGpsCollection IMySession.GPS =>
            Static.Gpss;

        [Obsolete("Use HasCreativeRights")]
        bool IMySession.HasAdminPrivileges =>
            this.HasCreativeRights;

        MyPromoteLevel IMySession.PromoteLevel =>
            this.GetUserPromoteLevel(Sync.MyId);

        bool IMySession.HasCreativeRights =>
            this.HasCreativeRights;

        Version IMySession.Version =>
            ((Version) MyFinalBuildConstants.APP_VERSION);

        IMyOxygenProviderSystem IMySession.OxygenProviderSystem =>
            this.m_oxygenHelper;

        public static string Platform
        {
            get => 
                m_platform;
            set => 
                (m_platform = value);
        }

        public static string PlatformLinkAgreement =>
            (!m_platform.Equals("Steam") ? "" : m_platformLinkAgreement_Steam);

        public static MySession Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }

        public DateTime GameDateTime
        {
            get => 
                (new DateTime(0x821, 1, 1, 0, 0, 0, DateTimeKind.Utc) + this.ElapsedGameTime);
            set => 
                (this.ElapsedGameTime = (TimeSpan) (value - new DateTime(0x821, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        public TimeSpan ElapsedGameTime { get; set; }

        public DateTime InGameTime { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public ulong? WorkshopId { get; private set; }

        public string CurrentPath { get; private set; }

        public string Briefing { get; set; }

        public string BriefingVideo { get; set; }

        public float SessionSimSpeedPlayer { get; private set; }

        public float SessionSimSpeedServer { get; private set; }

        public bool CameraOnCharacter { get; set; }

        public uint AutoSaveInMinutes
        {
            get
            {
                if (!MyFakes.ENABLE_AUTOSAVE || (this.Settings == null))
                {
                    return 0;
                }
                return this.Settings.AutoSaveInMinutes;
            }
        }

        public bool? SaveOnUnloadOverride =>
            this.m_saveOnUnloadOverride;

        public bool IsAdminMenuEnabled =>
            this.IsUserModerator(Sync.MyId);

        public bool CreativeMode =>
            (this.Settings.GameMode == MyGameModeEnum.Creative);

        public bool SurvivalMode =>
            (this.Settings.GameMode == MyGameModeEnum.Survival);

        public bool InfiniteAmmo =>
            (this.Settings.InfiniteAmmo || (this.Settings.GameMode == MyGameModeEnum.Creative));

        public bool EnableContainerDrops =>
            (this.Settings.EnableContainerDrops && (this.Settings.GameMode == MyGameModeEnum.Survival));

        public int MinDropContainerRespawnTime =>
            (this.Settings.MinDropContainerRespawnTime * 60);

        public int MaxDropContainerRespawnTime =>
            (this.Settings.MaxDropContainerRespawnTime * 60);

        public bool AutoHealing =>
            this.Settings.AutoHealing;

        public bool ThrusterDamage =>
            this.Settings.ThrusterDamage;

        public bool WeaponsEnabled =>
            this.Settings.WeaponsEnabled;

        public bool CargoShipsEnabled =>
            this.Settings.CargoShipsEnabled;

        public bool DestructibleBlocks =>
            this.Settings.DestructibleBlocks;

        public bool EnableIngameScripts =>
            this.Settings.EnableIngameScripts;

        public bool Enable3RdPersonView =>
            this.Settings.Enable3rdPersonView;

        public bool EnableToolShake =>
            this.Settings.EnableToolShake;

        public bool ShowPlayerNamesOnHud =>
            this.Settings.ShowPlayerNamesOnHud;

        public bool EnableConvertToStation =>
            this.Settings.EnableConvertToStation;

        public short MaxPlayers =>
            this.Settings.MaxPlayers;

        public short MaxFloatingObjects =>
            this.Settings.MaxFloatingObjects;

        public short MaxBackupSaves =>
            this.Settings.MaxBackupSaves;

        public int MaxGridSize =>
            this.Settings.MaxGridSize;

        public int MaxBlocksPerPlayer =>
            this.Settings.MaxBlocksPerPlayer;

        public Dictionary<string, short> BlockTypeLimits =>
            ((this.Settings.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.NONE) ? this.Settings.BlockTypeLimits.Dictionary : this.EmptyBlockTypeLimitDictionary);

        public bool EnableRemoteBlockRemoval =>
            this.Settings.EnableRemoteBlockRemoval;

        public float InventoryMultiplier =>
            this.Settings.InventorySizeMultiplier;

        public float CharactersInventoryMultiplier =>
            this.Settings.InventorySizeMultiplier;

        public float BlocksInventorySizeMultiplier =>
            this.Settings.BlocksInventorySizeMultiplier;

        public float RefinerySpeedMultiplier =>
            this.Settings.RefinerySpeedMultiplier;

        public float AssemblerSpeedMultiplier =>
            this.Settings.AssemblerSpeedMultiplier;

        public float AssemblerEfficiencyMultiplier =>
            this.Settings.AssemblerEfficiencyMultiplier;

        public float WelderSpeedMultiplier =>
            this.Settings.WelderSpeedMultiplier;

        public float GrinderSpeedMultiplier =>
            this.Settings.GrinderSpeedMultiplier;

        public float HackSpeedMultiplier =>
            this.Settings.HackSpeedMultiplier;

        public MyOnlineModeEnum OnlineMode =>
            this.Settings.OnlineMode;

        public MyEnvironmentHostilityEnum EnvironmentHostility =>
            this.Settings.EnvironmentHostility;

        public bool StartInRespawnScreen =>
            this.Settings.StartInRespawnScreen;

        public bool EnableVoxelDestruction =>
            this.Settings.EnableVoxelDestruction;

        public MyBlockLimitsEnabledEnum BlockLimitsEnabled =>
            this.Settings.BlockLimitsEnabled;

        public int TotalPCU =>
            this.Settings.TotalPCU;

        public int PiratePCU =>
            this.Settings.PiratePCU;

        public int MaxFactionsCount =>
            ((this.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION) ? Math.Max(1, this.Settings.MaxFactionsCount) : this.Settings.MaxFactionsCount);

        public bool ResearchEnabled =>
            (this.Settings.EnableResearch && !this.CreativeMode);

        public string CustomLoadingScreenImage { get; set; }

        public string CustomLoadingScreenText { get; set; }

        public string CustomSkybox { get; set; }

        public bool EnableSpiders =>
            this.Settings.EnableSpiders;

        public bool EnableWolfs =>
            this.Settings.EnableWolfs;

        public bool EnableScripterRole =>
            this.Settings.EnableScripterRole;

        public bool IsScenario =>
            this.Settings.Scenario;

        public bool LoadedAsMission { get; private set; }

        public bool PersistentEditMode { get; private set; }

        public List<MyObjectBuilder_Checkpoint.ModItem> Mods { get; set; }

        BoundingBoxD IMySession.WorldBoundaries =>
            ((this.WorldBoundaries != null) ? this.WorldBoundaries.Value : BoundingBoxD.CreateInvalid());

        public MySyncLayer SyncLayer { get; private set; }

        public static bool ShowMotD
        {
            get => 
                m_showMotD;
            set => 
                (m_showMotD = value);
        }

        public TimeSpan ElapsedPlayTime { get; private set; }

        public TimeSpan TimeOnFoot { get; private set; }

        public TimeSpan TimeOnJetpack { get; private set; }

        public TimeSpan TimePilotingSmallShip { get; private set; }

        public TimeSpan TimePilotingBigShip { get; private set; }

        public TimeSpan TimeOnStation { get; private set; }

        public TimeSpan TimeOnShips { get; private set; }

        public TimeSpan TimeOnAsteroids { get; private set; }

        public TimeSpan TimeOnPlanets { get; private set; }

        public TimeSpan TimeInBuilderMode { get; private set; }

        public float PositiveIntegrityTotal { get; set; }

        public float NegativeIntegrityTotal { get; set; }

        public ulong VoxelHandVolumeChanged { get; set; }

        public uint TotalDamageDealt { get; set; }

        public uint TotalBlocksCreated { get; set; }

        public uint TotalBlocksCreatedFromShips { get; set; }

        public uint ToolbarPageSwitches { get; set; }

        public MyPlayer LocalHumanPlayer
        {
            get
            {
                if ((Sync.Clients == null) || (Sync.Clients.LocalClient == null))
                {
                    return null;
                }
                return Sync.Clients.LocalClient.FirstPlayer;
            }
        }

        IMyPlayer IMySession.LocalHumanPlayer =>
            this.LocalHumanPlayer;

        public VRage.Game.Entity.MyEntity TopMostControlledEntity
        {
            get
            {
                VRage.Game.Entity.MyEntity entity = this.ControlledEntity?.Entity;
                VRage.Game.Entity.MyEntity topMostParent = entity?.GetTopMostParent(null);
                if ((topMostParent == null) || !ReferenceEquals(Sync.Players.GetControllingPlayer(entity), Sync.Players.GetControllingPlayer(topMostParent)))
                {
                    return entity;
                }
                return topMostParent;
            }
        }

        public Sandbox.Game.Entities.IMyControllableEntity ControlledEntity =>
            this.LocalHumanPlayer?.Controller.ControlledEntity;

        public MyCharacter LocalCharacter =>
            this.LocalHumanPlayer?.Character;

        public long LocalCharacterEntityId =>
            ((this.LocalCharacter == null) ? 0L : this.LocalCharacter.EntityId);

        public long LocalPlayerId =>
            ((this.LocalHumanPlayer == null) ? 0L : this.LocalHumanPlayer.Identity.IdentityId);

        public bool IsCameraAwaitingEntity
        {
            get => 
                this.m_cameraAwaitingEntity;
            set => 
                (this.m_cameraAwaitingEntity = value);
        }

        public IMyCameraController CameraController
        {
            get => 
                this.m_cameraController;
            private set
            {
                if (!ReferenceEquals(this.m_cameraController, value))
                {
                    IMyCameraController cameraController = this.m_cameraController;
                    this.m_cameraController = value;
                    if (Static != null)
                    {
                        if (this.CameraAttachedToChanged != null)
                        {
                            this.CameraAttachedToChanged(cameraController, this.m_cameraController);
                        }
                        if (cameraController != null)
                        {
                            cameraController.OnReleaseControl(this.m_cameraController);
                            if (cameraController.Entity != null)
                            {
                                cameraController.Entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.OnCameraEntityClosing);
                            }
                        }
                        this.m_cameraController.OnAssumeControl(cameraController);
                        if (this.m_cameraController.Entity != null)
                        {
                            this.m_cameraController.Entity.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.OnCameraEntityClosing);
                        }
                        this.m_cameraController.ForceFirstPersonCamera = false;
                    }
                }
            }
        }

        public bool IsValid =>
            true;

        public int GameplayFrameCounter =>
            this.m_gameplayFrameCounter;

        public bool Ready { get; private set; }

        public MyEnvironmentHostilityEnum? PreviousEnvironmentHostility { get; set; }

        public bool HasCreativeRights =>
            this.HasPlayerCreativeRights(Sync.MyId);

        public bool IsCopyPastingEnabled
        {
            get
            {
                if (!this.CreativeToolsEnabled(Sync.MyId) || !this.HasCreativeRights)
                {
                    return (this.CreativeMode && this.Settings.EnableCopyPaste);
                }
                return true;
            }
        }

        public MyGameFocusManager GameFocusManager { get; private set; }

        public AdminSettingsEnum AdminSettings
        {
            get => 
                this.m_adminSettings;
            set => 
                (this.m_adminSettings = value);
        }

        public Dictionary<ulong, AdminSettingsEnum> RemoteAdminSettings
        {
            get => 
                this.m_remoteAdminSettings;
            set => 
                (this.m_remoteAdminSettings = value);
        }

        public bool LargeStreamingInProgress
        {
            get => 
                this.m_largeStreamingInProgress;
            set
            {
                if (this.m_largeStreamingInProgress != value)
                {
                    this.m_largeStreamingInProgress = value;
                    if (this.m_largeStreamingInProgress)
                    {
                        MyHud.PushRotatingWheelVisible();
                        MyHud.RotatingWheelText = MyTexts.Get(MySpaceTexts.LoadingWheel_Streaming);
                    }
                    else
                    {
                        MyHud.PopRotatingWheelVisible();
                        MyHud.RotatingWheelText = MyHud.Empty;
                    }
                }
            }
        }

        public bool SmallStreamingInProgress
        {
            get => 
                this.m_smallStreamingInProgress;
            set
            {
                if (this.m_smallStreamingInProgress != value)
                {
                    this.m_smallStreamingInProgress = value;
                }
            }
        }

        public bool IsUnloadSaveInProgress { get; set; }

        public bool IsServer =>
            (Sync.IsServer || ReferenceEquals(Sandbox.Engine.Multiplayer.MyMultiplayer.Static, null));

        public MyGameDefinition GameDefinition { get; set; }

        public int AppVersionFromSave { get; private set; }

        public string ThumbPath =>
            Path.Combine(this.CurrentPath, MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);

        public bool MultiplayerAlive { get; set; }

        public bool MultiplayerDirect { get; set; }

        public double MultiplayerLastMsg { get; set; }

        public MyTimeSpan MultiplayerPing { get; set; }

        public bool HighSimulationQuality =>
            (!this.Settings.AdaptiveSimulationQuality || (MySandboxGame.Static.CPULoadSmooth <= 90f));

        public bool HighSimulationQualityNotification =>
            ((!this.Settings.AdaptiveSimulationQuality || (Sync.IsServer && (MySandboxGame.Static.CPULoadSmooth <= 90f))) || (!Sync.IsServer && (Sync.ServerCPULoadSmooth <= 90f)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySession.<>c <>9 = new MySession.<>c();
            public static Func<IMyEventOwner, Action<bool>> <>9__567_0;
            public static Func<IMyEventOwner, Action<ulong, MyPromoteLevel>> <>9__592_0;
            public static Func<IMyEventOwner, Action<string, MySimpleProfiler.ProfilingBlockType>> <>9__602_0;
            public static Action <>9__618_0;
            public static Func<KeyValuePair<ulong, MyPromoteLevel>, bool> <>9__646_0;
            public static Func<IMyEventOwner, Action<bool>> <>9__668_0;
            public static Func<IMyEventOwner, Action<bool>> <>9__669_0;
            public static Func<IMyEventOwner, Action<long>> <>9__677_0;
            public static Func<IMyEventOwner, Action<List<string>, List<string>, List<string>>> <>9__679_0;

            internal Action<bool> <EnableCreativeTools>b__567_0(IMyEventOwner s) => 
                new Action<bool>(MySession.OnCreativeToolsEnabled);

            internal bool <LoadWorld>b__646_0(KeyValuePair<ulong, MyPromoteLevel> e) => 
                (((MyPromoteLevel) e.Value) == MyPromoteLevel.Owner);

            internal Action<string, MySimpleProfiler.ProfilingBlockType> <PerformanceWarning>b__602_0(IMyEventOwner s) => 
                new Action<string, MySimpleProfiler.ProfilingBlockType>(MySession.OnServerPerformanceWarning);

            internal Action<long> <RequestVicinityCache>b__677_0(IMyEventOwner s) => 
                new Action<long>(MySession.OnRequestVicinityInformation);

            internal Action<bool> <Save>b__668_0(IMyEventOwner x) => 
                new Action<bool>(MySession.OnServerSaving);

            internal Action<bool> <SaveEnded>b__669_0(IMyEventOwner x) => 
                new Action<bool>(MySession.OnServerSaving);

            internal Action<List<string>, List<string>, List<string>> <SendVicinityInformation>b__679_0(IMyEventOwner s) => 
                new Action<List<string>, List<string>, List<string>>(MySession.OnVicinityInformation);

            internal Action<ulong, MyPromoteLevel> <SetUserPromoteLevel>b__592_0(IMyEventOwner x) => 
                new Action<ulong, MyPromoteLevel>(MySession.OnPromoteLevelSet);

            internal void <Update>b__618_0()
            {
                MySector.ResetEyeAdaptation = true;
            }
        }

        private class ComponentComparer : IComparer<MySessionComponentBase>
        {
            public int Compare(MySessionComponentBase x, MySessionComponentBase y)
            {
                int num = x.Priority.CompareTo(y.Priority);
                return ((num != 0) ? num : string.Compare(x.GetType().FullName, y.GetType().FullName, StringComparison.Ordinal));
            }
        }

        public enum LimitResult
        {
            Passed,
            MaxGridSize,
            NoFaction,
            BlockTypeLimit,
            MaxBlocksPerPlayer,
            PCU
        }
    }
}

