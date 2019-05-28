namespace Sandbox
{
    using Havok;
    using Microsoft.Win32;
    using ParallelTasks;
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Cube.CubeBuilder;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems.TextSurfaceScripts;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.Graphics.GUI.IME;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using Sandbox.ModAPI.Weapons;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Compiler;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.GUI;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.Localization;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.News;
    using VRage.Game.ObjectBuilder;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Game.SessionComponents;
    using VRage.Game.VisualScripting;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Library;
    using VRage.Library.Threading;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Profiler;
    using VRage.Scripting;
    using VRage.Serialization;
    using VRage.Service;
    using VRage.Stats;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;
    using VRageRender.ExternalApp;
    using VRageRender.Lights;
    using VRageRender.Messages;
    using VRageRender.Utils;

    public class MySandboxGame : Sandbox.Engine.Platform.Game, IDisposable
    {
        private const int MilisToMin = 0xea60;
        public static readonly MyStringId DirectX11RendererKey = MyStringId.GetOrCompute("DirectX 11");
        public const string CONSOLE_OUTPUT_AUTORESTART = "AUTORESTART";
        public static Version BuildVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static DateTime BuildDateTime = new DateTime(0x7d0, 1, 1).AddDays((double) BuildVersion.Build).AddSeconds((double) (BuildVersion.Revision * 2));
        public static MySandboxGame Static;
        public static Vector2I ScreenSize;
        public static Vector2I ScreenSizeHalf;
        public static MyViewport ScreenViewport;
        private bool m_isPendingLobbyInvite;
        private IMyLobby m_invitingLobby;
        private ulong m_lobbyInviter;
        private static int m_isPreloading;
        private static bool m_reconfirmClipmaps = false;
        private static bool m_areClipmapsReady = true;
        private static bool m_renderTasksFinished = false;
        public static bool IsUpdateReady = true;
        private static EnumAutorestartStage m_autoRestartState = EnumAutorestartStage.NoRestart;
        private int m_lastRestartCheckInMilis;
        private DateTime m_lastUpdateCheckTime = DateTime.UtcNow;
        private int m_autoUpdateRestartTimeInMin = 0x7fffffff;
        private bool m_isGoingToUpdate;
        public static bool IsRenderUpdateSyncEnabled = false;
        public static bool IsVideoRecordingEnabled = false;
        public static bool IsConsoleVisible = false;
        public static bool IsReloading = false;
        public static bool FatalErrorDuringInit = false;
        protected static ManualResetEvent m_windowCreatedEvent = new ManualResetEvent(false);
        public static readonly MyLog Log = new MyLog(true);
        public static Action PerformNotInteractiveReport = null;
        private bool hasFocus = true;
        private static int m_pauseStartTimeInMilliseconds;
        private static int m_totalPauseTimeInMilliseconds = 0;
        private static long m_lastFrameTimeStamp = 0L;
        public static int NumberOfCores;
        public static uint CPUFrequency;
        public static bool InsufficientHardware;
        private bool m_dataLoadedDebug;
        private ulong? m_joinLobbyId;
        public static bool ShowIsBetterGCAvailableNotification = false;
        public static bool ShowGpuUnderMinimumNotification = false;
        protected IMyBufferedInputSource m_bufferedInputSource;
        private MyConcurrentQueue<MyInvokeData> m_invokeQueue = new MyConcurrentQueue<MyInvokeData>(0x20);
        private MyConcurrentQueue<MyInvokeData> m_invokeQueueExecuting = new MyConcurrentQueue<MyInvokeData>(0x20);
        public MyGameRenderComponent GameRenderComponent;
        public MySessionCompatHelper SessionCompatHelper;
        public static MyConfig Config;
        public static IMyConfigDedicated ConfigDedicated;
        public static IntPtr GameWindowHandle;
        private bool m_enableDamageEffects = true;
        [CompilerGenerated]
        private EventHandler OnGameLoaded;
        [CompilerGenerated]
        private EventHandler OnScreenshotTaken;
        private bool m_unpauseInput;
        private DateTime m_inputPauseTime;
        private const int INPUT_UNPAUSE_DELAY = 10;
        private static int m_timerTTHelper = 0;
        private static readonly int m_timerTTHelper_Max = 100;
        protected Action<bool> m_setMouseVisible;
        private static bool ShowWhitelistPopup = false;
        private static bool CanShowWhitelistPopup = false;
        private static bool ShowHotfixPopup = false;
        private static bool CanShowHotfixPopup = false;
        private static bool m_isPaused;
        private static int m_pauseStackCount = 0;
        private MyNews m_changelog;
        private XmlSerializer m_changelogSerializer;
        private int m_queueSize;
        private static IErrorConsumer m_errorConsumer = new MyGameErrorConsumer();
        private MySandboxForm form;

        public event EventHandler OnGameLoaded
        {
            [CompilerGenerated] add
            {
                EventHandler onGameLoaded = this.OnGameLoaded;
                while (true)
                {
                    EventHandler a = onGameLoaded;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onGameLoaded = Interlocked.CompareExchange<EventHandler>(ref this.OnGameLoaded, handler3, a);
                    if (ReferenceEquals(onGameLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onGameLoaded = this.OnGameLoaded;
                while (true)
                {
                    EventHandler source = onGameLoaded;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onGameLoaded = Interlocked.CompareExchange<EventHandler>(ref this.OnGameLoaded, handler3, source);
                    if (ReferenceEquals(onGameLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler OnScreenshotTaken
        {
            [CompilerGenerated] add
            {
                EventHandler onScreenshotTaken = this.OnScreenshotTaken;
                while (true)
                {
                    EventHandler a = onScreenshotTaken;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onScreenshotTaken = Interlocked.CompareExchange<EventHandler>(ref this.OnScreenshotTaken, handler3, a);
                    if (ReferenceEquals(onScreenshotTaken, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onScreenshotTaken = this.OnScreenshotTaken;
                while (true)
                {
                    EventHandler source = onScreenshotTaken;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onScreenshotTaken = Interlocked.CompareExchange<EventHandler>(ref this.OnScreenshotTaken, handler3, source);
                    if (ReferenceEquals(onScreenshotTaken, source))
                    {
                        return;
                    }
                }
            }
        }

        public MySandboxGame(string[] commandlineArgs)
        {
            MyUtils.MainThread = Thread.CurrentThread;
            if (Config.SyncRendering)
            {
                MyRandom.EnableDeterminism = true;
                MyFakes.FORCE_NO_WORKER = true;
                MyFakes.ENABLE_WORKSHOP_MODS = false;
                MyFakes.ENABLE_HAVOK_MULTITHREADING = false;
                MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING = false;
                MyRenderProxy.SetFrameTimeStep(0.01666667f);
                MyRenderProxy.Settings.IgnoreOcclusionQueries = !IsVideoRecordingEnabled;
                MyRenderProxy.SetSettingsDirty();
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                ConfigDedicated.Load(null);
            }
            this.RegisterAssemblies(commandlineArgs);
            if (commandlineArgs.Contains<string>("-skipintro"))
            {
                MyFakes.ENABLE_LOGOS = false;
            }
            Log.WriteLine("MySandboxGame.Constructor() - START");
            Log.IncreaseIndent();
            Configuration.EnableObjectTracking = false;
            base.UpdateThread = Thread.CurrentThread;
            MyScreenManager.UpdateThread = base.UpdateThread;
            MyPrecalcComponent.UpdateThreadManagedId = base.UpdateThread.ManagedThreadId;
            Log.WriteLine("Game dir: " + MyFileSystem.ExePath);
            Log.WriteLine("Content dir: " + MyFileSystem.ContentPath);
            Static = this;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(this.Console_CancelKeyPress);
            InitNumberOfCores();
            MyLanguage.Init();
            MyGlobalTypeMetadata.Static.Init(true);
            System.Threading.Tasks.Task.Factory.StartNew(() => MyDefinitionManager.Static.LoadScenarios());
            Preallocate();
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.GameRenderComponent = new MyGameRenderComponent();
            }
            else
            {
                int? port = MyDedicatedServerOverrides.Port;
                IPEndPoint serverEndpoint = new IPEndPoint(MyDedicatedServerOverrides.IpAddress ?? IPAddressExtensions.ParseOrAny(ConfigDedicated.IP), (port != null) ? ((ushort) port.GetValueOrDefault()) : ((ushort) ConfigDedicated.ServerPort));
                MyLog.Default.WriteLineAndConsole("Bind IP : " + serverEndpoint.ToString());
                MyDedicatedServer server1 = new MyDedicatedServer(serverEndpoint);
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static = server1;
                FatalErrorDuringInit = !server1.ServerStarted;
                if (FatalErrorDuringInit)
                {
                    Exception exception1 = new Exception("Fatal error during dedicated server init see log for more information.");
                    exception1.Data["Silent"] = true;
                    throw exception1;
                }
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated && !FatalErrorDuringInit)
            {
                (Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyDedicatedServerBase).SendGameTagsToSteam();
            }
            this.SessionCompatHelper = Activator.CreateInstance(MyPerGameSettings.CompatHelperType) as MySessionCompatHelper;
            this.InitMultithreading();
            if (!Sandbox.Engine.Platform.Game.IsDedicated && MyGameService.IsActive)
            {
                MyGameService.OnOverlayActivated += new Action<byte>(this.OverlayActivated);
            }
            MyMessageLoop.AddMessageHandler((uint) 0x41e, new ActionRef<System.Windows.Forms.Message>(this.OnToolIsGameRunningMessage));
            MyCampaignManager.Static.Init();
            Log.DecreaseIndent();
            Log.WriteLine("MySandboxGame.Constructor() - END");
        }

        protected static void AddDefaultGameControl(Dictionary<MyStringId, MyControl> self, MyGuiControlTypeEnum controlTypeEnum, MyStringId controlId, MyMouseButtonsEnum? mouse = new MyMouseButtonsEnum?(), MyKeys? key = new MyKeys?(), MyKeys? key2 = new MyKeys?())
        {
            MyGuiDescriptor gameControlHelper = MyGuiGameControlsHelpers.GetGameControlHelper(controlId);
            MyStringId? helpText = null;
            self[controlId] = new MyControl(controlId, gameControlHelper.NameEnum, controlTypeEnum, mouse, key, helpText, key2, gameControlHelper.DescriptionEnum);
        }

        protected override void AfterDraw()
        {
            MyRenderProxy.AfterUpdate(new MyTimeSpan?(base.TotalTime));
        }

        public static void AfterLogos()
        {
            MyGuiSandbox.BackToMainMenu();
            bool? gDPRConsentSent = Config.GDPRConsentSent;
            bool flag = false;
            if (((gDPRConsentSent.GetValueOrDefault() == flag) & (gDPRConsentSent != null)) || (Config.GDPRConsentSent == null))
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenGDPR());
            }
            if (Config.WelcomScreenCurrentStatus == MyConfig.WelcomeScreenStatus.NotSeen)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenWelcomeScreen());
            }
        }

        [Conditional("DEBUG")]
        public static void AssertUpdateThread()
        {
        }

        public static void AutoRestartWarning(int time)
        {
            MyLog.Default.WriteLineAndConsole($"Server will restart in {time} minute{(time == 1) ? "" : "s"}");
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.SendChatMessage(string.Format(MyTexts.GetString(MyCommonTexts.Server_Restart_Warning), time, (time == 1) ? "" : "s"), ChatChannel.Global, 0L);
        }

        public static void AutoUpdateWarning(int time)
        {
            MyLog.Default.WriteLineAndConsole($"New version available. Server will restart in {time} minute{(time == 1) ? "" : "s"}");
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.SendChatMessage(string.Format(MyTexts.GetString(MyCommonTexts.Server_Update_Warning), time, (time == 1) ? "" : "s"), ChatChannel.Global, 0L);
        }

        private void CheckAutoRestartForDedicatedServer()
        {
            if (((ConfigDedicated.AutoRestartEnabled && (ConfigDedicated.AutoRestatTimeInMin > 0)) || this.IsGoingToUpdate) && (TotalTimeInMilliseconds > this.m_lastRestartCheckInMilis))
            {
                this.m_lastRestartCheckInMilis = TotalTimeInMilliseconds + 0xea60;
                int num = 0x7fffffff;
                if (ConfigDedicated.AutoRestartEnabled && (ConfigDedicated.AutoRestatTimeInMin > 0))
                {
                    num = Math.Min(num, ConfigDedicated.AutoRestatTimeInMin);
                }
                if (this.IsGoingToUpdate)
                {
                    num = Math.Min(num, this.m_autoUpdateRestartTimeInMin);
                }
                switch (AutoRestartState)
                {
                    case EnumAutorestartStage.NotWarned:
                        if (TotalTimeInMilliseconds < ((num - 10) * 0xea60))
                        {
                            break;
                        }
                        AutoRestartWarning(10);
                        m_autoRestartState = EnumAutorestartStage.Warned_10Min;
                        return;

                    case EnumAutorestartStage.Warned_10Min:
                        if (TotalTimeInMilliseconds < ((num - 5) * 0xea60))
                        {
                            break;
                        }
                        AutoRestartWarning(5);
                        m_autoRestartState = EnumAutorestartStage.Warned_5Min;
                        return;

                    case EnumAutorestartStage.Warned_5Min:
                        if (TotalTimeInMilliseconds < ((num - 1) * 0xea60))
                        {
                            break;
                        }
                        AutoRestartWarning(1);
                        m_autoRestartState = EnumAutorestartStage.Warned_1Min;
                        return;

                    case EnumAutorestartStage.Warned_1Min:
                        if (TotalTimeInMilliseconds < (num * 0xea60))
                        {
                            break;
                        }
                        m_autoRestartState = EnumAutorestartStage.Restarting;
                        this.m_lastRestartCheckInMilis = TotalTimeInMilliseconds;
                        return;

                    case EnumAutorestartStage.Restarting:
                        if (ConfigDedicated.AutoRestatTimeInMin > 60)
                        {
                            MyLog.Default.WriteLineAndConsole($"Automatic stop after {num / 60} hours and {num % 60} minutes");
                        }
                        else
                        {
                            MyLog.Default.WriteLineAndConsole($"Automatic stop after {num} minutes");
                        }
                        if (MySession.Static != null)
                        {
                            MySession.Static.SetSaveOnUnloadOverride_Dedicated(new bool?(ConfigDedicated.AutoRestartSave));
                        }
                        ExitThreadSafe();
                        return;

                    case EnumAutorestartStage.NoRestart:
                    {
                        int time = num - (TotalTimeInMilliseconds / 0xea60);
                        if (time > 10)
                        {
                            m_autoRestartState = EnumAutorestartStage.NotWarned;
                        }
                        else if (time > 5)
                        {
                            m_autoRestartState = EnumAutorestartStage.Warned_10Min;
                        }
                        else if (time > 1)
                        {
                            m_autoRestartState = EnumAutorestartStage.Warned_5Min;
                        }
                        else
                        {
                            if (time <= 0)
                            {
                                break;
                            }
                            m_autoRestartState = EnumAutorestartStage.Warned_1Min;
                        }
                        if (!this.IsGoingToUpdate)
                        {
                            AutoRestartWarning(time);
                            break;
                        }
                        AutoUpdateWarning(time);
                        return;
                    }
                    default:
                        return;
                }
            }
        }

        private void CheckAutoUpdateForDedicatedServer()
        {
            if ((ConfigDedicated.AutoUpdateEnabled && !this.IsGoingToUpdate) && ((DateTime.UtcNow - this.m_lastUpdateCheckTime).TotalMinutes >= ConfigDedicated.AutoUpdateCheckIntervalInMin))
            {
                this.m_lastUpdateCheckTime = DateTime.UtcNow;
                System.Threading.Tasks.Task.Run(() => this.DownloadChangelog()).ContinueWith(task => this.DownloadChangelogCompleted());
                if (MyGameService.IsUpdateAvailable())
                {
                    this.StartAutoUpdateCountdown();
                }
            }
        }

        protected virtual void CheckGraphicsCard(MyRenderMessageVideoAdaptersResponse msgVideoAdapters)
        {
            MyAdapterInfo adapter = msgVideoAdapters.Adapters[MyVideoSettingsManager.CurrentDeviceSettings.AdapterOrdinal];
            if (MyGpuIds.IsUnsupported(adapter.VendorId, adapter.DeviceId) || MyGpuIds.IsUnderMinimum(adapter.VendorId, adapter.DeviceId))
            {
                Log.WriteLine("Error: It seems that your graphics card is currently unsupported or it does not meet minimum requirements.");
                Log.WriteLine($"Graphics card name: {adapter.Name}, vendor id: 0x{adapter.VendorId:X}, device id: 0x{adapter.DeviceId:X}.");
                MyErrorReporter.ReportNotCompatibleGPU(MyPerGameSettings.GameName, Log.GetFilePath(), MyPerGameSettings.MinimumRequirementsPage);
            }
            if (!Config.DisableUpdateDriverNotification)
            {
                if (!adapter.IsNvidiaNotebookGpu)
                {
                    if (adapter.DriverUpdateNecessary)
                    {
                        this.ShowUpdateDriverDialog(adapter);
                    }
                }
                else
                {
                    for (int i = 0; i < msgVideoAdapters.Adapters.Length; i++)
                    {
                        adapter = msgVideoAdapters.Adapters[i];
                        if (adapter.DriverUpdateNecessary)
                        {
                            this.ShowUpdateDriverDialog(adapter);
                        }
                    }
                }
            }
        }

        public void ClearInvokeQueue()
        {
            this.m_invokeQueue.Clear();
        }

        private void CloseHandlers()
        {
            MyGameService.OnPingServerResponded -= new EventHandler<MyGameServerItem>(this.ServerResponded);
            MyGameService.OnPingServerFailedToRespond -= new EventHandler(this.ServerFailedToRespond);
        }

        private static void ClosePopup(MyGuiScreenMessageBox.ResultEnum result)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ExitThreadSafe();
            Thread.Sleep(0x3e8);
        }

        public void Dispose()
        {
            MyInMemoryWaveDataCache.Static.Dispose();
            if (MySessionComponentExtDebug.Static != null)
            {
                MySessionComponentExtDebug.Static.Dispose();
                MySessionComponentExtDebug.Static = null;
            }
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Dispose();
            }
            if (this.GameRenderComponent != null)
            {
                this.GameRenderComponent.Dispose();
                this.GameRenderComponent = null;
            }
            MyPlugins.Unload();
            ParallelTasks.Parallel.Scheduler.WaitForTasksToFinish(TimeSpan.FromSeconds(10.0));
            m_windowCreatedEvent.Dispose();
            if (MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                MyScriptCompiler.Static.Whitelist.Clear();
            }
            else
            {
                IlChecker.Clear();
            }
            MyObjectBuilderType.UnregisterAssemblies();
            MyObjectBuilderSerializer.UnregisterAssembliesAndSerializers();
        }

        private void DownloadChangelog()
        {
            if (this.m_changelogSerializer == null)
            {
                this.m_changelogSerializer = new XmlSerializer(typeof(MyNews));
            }
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    using (StringReader reader = new StringReader(client.DownloadString(new Uri(MyPerGameSettings.ChangeLogUrl))))
                    {
                        this.m_changelog = (MyNews) this.m_changelogSerializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error while downloading changelog: " + exception.ToString());
            }
        }

        private void DownloadChangelogCompleted()
        {
            int num;
            if (((this.m_changelog != null) && ((this.m_changelog.Entry.Count > 0) && int.TryParse(this.m_changelog.Entry[0].Version, out num))) && (num > MyFinalBuildConstants.APP_VERSION))
            {
                this.StartAutoUpdateCountdown();
            }
        }

        public void EndLoop()
        {
            MyLog.Default.WriteLineAndConsole("Exiting..");
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.EndSession();
            }
            MyAnalyticsManager.Instance.FlushAndDispose();
            this.UnloadData_UpdateThread();
        }

        public static void ExitThreadSafe()
        {
            ParallelTasks.Parallel.Scheduler.WaitForTasksToFinish(TimeSpan.FromSeconds(10.0));
            Static.Invoke(new Action(Static.Exit), "MySandboxGame::Exit");
            if (!Sandbox.Engine.Platform.Game.IsDedicated && !string.IsNullOrEmpty(MyFakes.EXIT_URL))
            {
                MyGuiSandbox.OpenUrl(MyFakes.EXIT_URL, UrlOpenMode.ExternalBrowser, null);
                MyFakes.EXIT_URL = "";
            }
        }

        public static void ForceStaticCtor(System.Type[] types)
        {
            foreach (System.Type type in types)
            {
                Log.WriteLine(type.Name + " - START");
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                Log.WriteLine(type.Name + " - END");
            }
        }

        private BoundingFrustumD GetCameraFrustum() => 
            ((MySector.MainCamera != null) ? MySector.MainCamera.BoundingFrustumFar : new BoundingFrustumD(MatrixD.Identity));

        private void GetListenerVelocity(ref Vector3 velocity)
        {
            if (MySession.Static != null)
            {
                Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                if (controlledEntity != null)
                {
                    controlledEntity.GetLinearVelocity(ref velocity, false);
                }
            }
        }

        protected virtual void Initialize()
        {
            Log.WriteLine("MySandboxGame.Initialize() - START");
            Log.IncreaseIndent();
            Log.WriteLine("Installed DLCs: ");
            Log.IncreaseIndent();
            if (!Sync.IsDedicated)
            {
                foreach (KeyValuePair<uint, MyDLCs.MyDLC> pair in MyDLCs.DLCs)
                {
                    if (MyGameService.IsDlcInstalled(pair.Value.AppId))
                    {
                        Log.WriteLine($"{MyTexts.GetString(pair.Value.DisplayName)} ({pair.Value.AppId})");
                    }
                }
            }
            Log.DecreaseIndent();
            if (this.GameRenderComponent != null)
            {
                MyRenderDeviceSettings? settingsToTry = MyVideoSettingsManager.Initialize();
                this.StartRenderComponent(settingsToTry);
                m_windowCreatedEvent.WaitOne();
            }
            this.InitInput();
            this.InitSteamWorkshop();
            this.LoadData();
            MyVisualScriptingProxy.Init();
            MyVisualScriptingProxy.RegisterLogicProvider(typeof(VRage.Game.VisualScripting.MyVisualScriptLogicProvider));
            MyVisualScriptingProxy.RegisterLogicProvider(typeof(Sandbox.Game.MyVisualScriptLogicProvider));
            this.InitQuickLaunch();
            MyObjectBuilder_ProfilerSnapshot.SetDelegates();
            Log.DecreaseIndent();
            Log.WriteLine("MySandboxGame.Initialize() - END");
        }

        protected virtual IMyRenderWindow InitializeRenderThread()
        {
            base.DrawThread = Thread.CurrentThread;
            this.form = new MySandboxForm();
            this.WindowHandle = this.form.Handle;
            this.m_bufferedInputSource = this.form;
            m_windowCreatedEvent.Set();
            this.form.Text = MyPerGameSettings.GameName;
            try
            {
                this.form.Icon = new Icon(Path.Combine(MyFileSystem.ExePath, MyPerGameSettings.GameIcon));
            }
            catch (FileNotFoundException)
            {
                this.form.Icon = null;
            }
            this.form.FormClosing += delegate (object o, FormClosingEventArgs e) {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.form.Hide();
                }
                if ((MySession.Static != null) && (MySpaceAnalytics.Instance != null))
                {
                    MySpaceAnalytics.Instance.ReportGameplayEnd();
                    MySpaceAnalytics.Instance.ReportGameQuit("Window closed");
                }
                ExitThreadSafe();
            };
            Action showCursor = delegate {
                if (!this.form.IsDisposed)
                {
                    this.form.ShowCursor = true;
                }
            };
            Action hideCursor = delegate {
                if (!this.form.IsDisposed)
                {
                    this.form.ShowCursor = false;
                }
            };
            this.m_setMouseVisible = delegate (bool b) {
                MyGameRenderComponent gameRenderComponent = this.GameRenderComponent;
                if (gameRenderComponent != null)
                {
                    MyRenderThread renderThread = gameRenderComponent.RenderThread;
                    if (renderThread != null)
                    {
                        renderThread.Invoke(b ? showCursor : hideCursor);
                    }
                }
            };
            if (Config.SyncRendering)
            {
                MyViewport viewport = new MyViewport(0f, 0f, (float) Config.ScreenWidth.Value, (float) Config.ScreenHeight.Value);
                this.RenderThread_SizeChanged((int) viewport.Width, (int) viewport.Height, viewport);
            }
            return this.form;
        }

        internal static void InitIlChecker()
        {
            if (GameCustomInitialization != null)
            {
                GameCustomInitialization.InitIlChecker();
            }
            if (MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                using (IMyWhitelistBatch batch = MyScriptCompiler.Static.Whitelist.OpenBatch())
                {
                    MemberInfo[] members = new MemberInfo[] { typeof(MyCubeBuilder).GetField("Static"), typeof(MyCubeBuilder).GetProperty("CubeBuilderState"), typeof(MyCubeBuilderState).GetProperty("CurrentBlockDefinition"), typeof(MyHud).GetField("BlockInfo") };
                    batch.AllowMembers(MyWhitelistTarget.ModApi, members);
                    System.Type[] typeArray1 = new System.Type[] { typeof(MyHudBlockInfo), typeof(MyHudBlockInfo.ComponentInfo), typeof(MyObjectBuilder_CubeBuilderDefinition), typeof(MyPlacementSettings), typeof(MyGridPlacementSettings), typeof(SnapMode), typeof(VoxelPlacementMode), typeof(VoxelPlacementSettings) };
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray1);
                    System.Type[] typeArray2 = new System.Type[] { typeof(ListExtensions), typeof(VRage.Game.ModAPI.Ingame.IMyCubeBlock), typeof(MyIni), typeof(Sandbox.ModAPI.Ingame.IMyTerminalBlock), typeof(Vector3), typeof(MySprite) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, typeArray2);
                    System.Type[] typeArray3 = new System.Type[0x15];
                    typeArray3[0] = typeof(MyAPIUtilities);
                    typeArray3[1] = typeof(Sandbox.ModAPI.Interfaces.ITerminalAction);
                    typeArray3[2] = typeof(IMyTerminalAction);
                    typeArray3[3] = typeof(VRage.Game.ModAPI.IMyCubeBlock);
                    typeArray3[4] = typeof(MyAPIGateway);
                    typeArray3[5] = typeof(IMyCameraController);
                    typeArray3[6] = typeof(VRage.ModAPI.IMyEntity);
                    typeArray3[7] = typeof(VRage.Game.Entity.MyEntity);
                    typeArray3[8] = typeof(MyEntityExtensions);
                    typeArray3[9] = typeof(EnvironmentItemsEntry);
                    typeArray3[10] = typeof(MyObjectBuilder_GasProperties);
                    typeArray3[11] = typeof(MyObjectBuilder_AdvancedDoor);
                    typeArray3[12] = typeof(MyObjectBuilder_AdvancedDoorDefinition);
                    typeArray3[13] = typeof(MyObjectBuilder_ComponentBase);
                    typeArray3[14] = typeof(MyObjectBuilder_Base);
                    typeArray3[15] = typeof(MyIngameScript);
                    typeArray3[0x10] = typeof(MyResourceSourceComponent);
                    typeArray3[0x11] = typeof(MyCharacterOxygenComponent);
                    typeArray3[0x12] = typeof(IMyUseObject);
                    typeArray3[0x13] = typeof(IMyModelDummy);
                    typeArray3[20] = typeof(IMyTextSurfaceScript);
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray3);
                    System.Type[] typeArray4 = new System.Type[] { typeof(MyBillboard.BlendTypeEnum) };
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray4);
                    System.Type[] typeArray5 = new System.Type[] { typeof(MyObjectBuilder_EntityStatRegenEffect) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray5);
                    System.Type[] typeArray6 = new System.Type[12];
                    typeArray6[0] = typeof(MyStatLogic);
                    typeArray6[1] = typeof(MyEntityStatComponent);
                    typeArray6[2] = typeof(MyEnvironmentSector);
                    typeArray6[3] = typeof(SerializableVector3);
                    typeArray6[4] = typeof(MyDefinitionManager);
                    typeArray6[5] = typeof(MyFixedPoint);
                    typeArray6[6] = typeof(ListReader<>);
                    typeArray6[7] = typeof(MyStorageData);
                    typeArray6[8] = typeof(MyEventArgs);
                    typeArray6[9] = typeof(MyGameTimer);
                    typeArray6[10] = typeof(MyLight);
                    typeArray6[11] = typeof(IMyAutomaticRifleGun);
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray6);
                    MemberInfo[] infoArray2 = new MemberInfo[] { typeof(MySpectatorCameraController).GetProperty("IsLightOn") };
                    batch.AllowMembers(MyWhitelistTarget.ModApi, infoArray2);
                    System.Type[] typeArray7 = new System.Type[] { typeof(TerminalActionExtensions), typeof(Sandbox.ModAPI.Interfaces.ITerminalAction), typeof(ITerminalProperty), typeof(ITerminalProperty<>), typeof(TerminalPropertyExtensions), typeof(MySpaceTexts), typeof(StringBuilderExtensions_Format), typeof(MyFixedPoint) };
                    batch.AllowTypes(MyWhitelistTarget.Both, typeArray7);
                    System.Type[] typeArray8 = new System.Type[9];
                    typeArray8[0] = typeof(MyTuple);
                    typeArray8[1] = typeof(MyTuple<>);
                    typeArray8[2] = typeof(MyTuple<,>);
                    typeArray8[3] = typeof(MyTuple<,,>);
                    typeArray8[4] = typeof(MyTuple<,,,>);
                    typeArray8[5] = typeof(MyTuple<,,,,>);
                    typeArray8[6] = typeof(MyTuple<,,,,,>);
                    typeArray8[7] = typeof(MyTupleComparer<,>);
                    typeArray8[8] = typeof(MyTupleComparer<,,>);
                    batch.AllowTypes(MyWhitelistTarget.Both, typeArray8);
                    System.Type[] typeArray9 = new System.Type[] { typeof(string) };
                    MemberInfo[] infoArray3 = new MemberInfo[7];
                    infoArray3[0] = typeof(MyTexts).GetMethod("GetString", typeArray9);
                    System.Type[] typeArray10 = new System.Type[] { typeof(MyStringId) };
                    infoArray3[1] = typeof(MyTexts).GetMethod("GetString", typeArray10);
                    infoArray3[2] = typeof(MyTexts).GetMethod("Exists");
                    infoArray3[3] = typeof(MyTexts).GetMethod("Get");
                    System.Type[] typeArray11 = new System.Type[] { typeof(StringBuilder), typeof(MyStringId), typeof(object) };
                    infoArray3[4] = typeof(MyTexts).GetMethod("AppendFormat", typeArray11);
                    System.Type[] typeArray12 = new System.Type[] { typeof(StringBuilder), typeof(MyStringId), typeof(MyStringId) };
                    infoArray3[5] = typeof(MyTexts).GetMethod("AppendFormat", typeArray12);
                    infoArray3[6] = typeof(MyTexts).GetProperty("Languages");
                    batch.AllowMembers(MyWhitelistTarget.Both, infoArray3);
                    System.Type[] typeArray13 = new System.Type[] { typeof(MyTexts.LanguageDescription), typeof(MyLanguagesEnum) };
                    batch.AllowTypes(MyWhitelistTarget.Both, typeArray13);
                    System.Type[] typeArray14 = new System.Type[] { typeof(VRage.ModAPI.IMyInput) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray14);
                    System.Type[] typeArray15 = new System.Type[] { typeof(MyInputExtensions), typeof(MyKeys), typeof(MyJoystickAxesEnum), typeof(MyJoystickButtonsEnum), typeof(MyMouseButtonsEnum), typeof(MySharedButtonsEnum), typeof(MyGuiControlTypeEnum), typeof(MyGuiInputDeviceEnum) };
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray15);
                    IEnumerable<MethodInfo> enumerable2 = from method in typeof(MyComponentContainer).GetMethods()
                        where (method.Name == "TryGet") && (method.ContainsGenericParameters && (method.GetParameters().Length == 1))
                        select method;
                    MemberInfo[] infoArray4 = new MemberInfo[4];
                    infoArray4[0] = enumerable2.FirstOrDefault<MethodInfo>();
                    infoArray4[1] = typeof(MyComponentContainer).GetMethod("Has");
                    infoArray4[2] = typeof(MyComponentContainer).GetMethod("Get");
                    System.Type[] typeArray16 = new System.Type[] { typeof(System.Type), typeof(MyComponentBase).MakeByRefType() };
                    infoArray4[3] = typeof(MyComponentContainer).GetMethod("TryGet", typeArray16);
                    batch.AllowMembers(MyWhitelistTarget.Both, infoArray4);
                    System.Type[] typeArray17 = new System.Type[] { typeof(ListReader<>), typeof(MyDefinitionId), typeof(MyRelationsBetweenPlayerAndBlock), typeof(MyRelationsBetweenPlayerAndBlockExtensions), typeof(MyResourceSourceComponentBase), typeof(MyObjectBuilder_GasProperties), typeof(SerializableDefinitionId), typeof(MyCubeSize) };
                    batch.AllowTypes(MyWhitelistTarget.Ingame, typeArray17);
                    MemberInfo[] infoArray5 = new MemberInfo[] { typeof(MyComponentBase).GetMethod("GetAs"), typeof(MyComponentBase).GetProperty("ContainerBase") };
                    batch.AllowMembers(MyWhitelistTarget.Ingame, infoArray5);
                    MemberInfo[] infoArray6 = new MemberInfo[] { typeof(MyObjectBuilder_Base).GetProperty("TypeId"), typeof(MyObjectBuilder_Base).GetProperty("SubtypeId") };
                    batch.AllowMembers(MyWhitelistTarget.Ingame, infoArray6);
                    MemberInfo[] infoArray7 = new MemberInfo[0x13];
                    infoArray7[0] = typeof(MyResourceSourceComponent).GetProperty("CurrentOutput");
                    infoArray7[1] = typeof(MyResourceSourceComponent).GetProperty("MaxOutput");
                    infoArray7[2] = typeof(MyResourceSourceComponent).GetProperty("DefinedOutput");
                    infoArray7[3] = typeof(MyResourceSourceComponent).GetProperty("ProductionEnabled");
                    infoArray7[4] = typeof(MyResourceSourceComponent).GetProperty("RemainingCapacity");
                    infoArray7[5] = typeof(MyResourceSourceComponent).GetProperty("HasCapacityRemaining");
                    infoArray7[6] = typeof(MyResourceSourceComponent).GetProperty("ResourceTypes");
                    infoArray7[7] = typeof(MyResourceSinkComponent).GetProperty("AcceptedResources");
                    infoArray7[8] = typeof(MyResourceSinkComponent).GetProperty("RequiredInput");
                    infoArray7[9] = typeof(MyResourceSinkComponent).GetProperty("SuppliedRatio");
                    infoArray7[10] = typeof(MyResourceSinkComponent).GetProperty("CurrentInput");
                    infoArray7[11] = typeof(MyResourceSinkComponent).GetProperty("IsPowered");
                    infoArray7[12] = typeof(MyResourceSinkComponentBase).GetProperty("AcceptedResources");
                    infoArray7[13] = typeof(MyResourceSinkComponentBase).GetMethod("CurrentInputByType");
                    infoArray7[14] = typeof(MyResourceSinkComponentBase).GetMethod("IsPowerAvailable");
                    infoArray7[15] = typeof(MyResourceSinkComponentBase).GetMethod("IsPoweredByType");
                    infoArray7[0x10] = typeof(MyResourceSinkComponentBase).GetMethod("MaxRequiredInputByType");
                    infoArray7[0x11] = typeof(MyResourceSinkComponentBase).GetMethod("RequiredInputByType");
                    infoArray7[0x12] = typeof(MyResourceSinkComponentBase).GetMethod("SuppliedRatioByType");
                    batch.AllowMembers(MyWhitelistTarget.Ingame, infoArray7);
                    System.Type[] typeArray18 = new System.Type[] { typeof(MyPhysicsHelper), typeof(MyPhysics.CollisionLayers) };
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray18);
                    System.Type[] typeArray19 = new System.Type[0x17];
                    typeArray19[0] = typeof(MyLodTypeEnum);
                    typeArray19[1] = typeof(MyMaterialsSettings);
                    typeArray19[2] = typeof(MyShadowsSettings);
                    typeArray19[3] = typeof(MyPostprocessSettings);
                    typeArray19[4] = typeof(MyHBAOData);
                    typeArray19[5] = typeof(MySSAOSettings);
                    typeArray19[6] = typeof(MyEnvironmentLightData);
                    typeArray19[7] = typeof(MyEnvironmentData);
                    typeArray19[8] = typeof(MyPostprocessSettings.Layout);
                    typeArray19[9] = typeof(MySSAOSettings.Layout);
                    typeArray19[10] = typeof(MyShadowsSettings.Struct);
                    typeArray19[11] = typeof(MyShadowsSettings.Cascade);
                    typeArray19[12] = typeof(MyMaterialsSettings.Struct);
                    typeArray19[13] = typeof(MyMaterialsSettings.MyChangeableMaterial);
                    typeArray19[14] = typeof(MyGlareTypeEnum);
                    typeArray19[15] = typeof(SerializableDictionary<,>);
                    typeArray19[0x10] = typeof(MyToolBase);
                    typeArray19[0x11] = typeof(MyGunBase);
                    typeArray19[0x12] = typeof(MyDeviceBase);
                    typeArray19[0x13] = typeof(Stopwatch);
                    typeArray19[20] = typeof(ConditionalAttribute);
                    typeArray19[0x15] = typeof(Version);
                    typeArray19[0x16] = typeof(ObsoleteAttribute);
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray19);
                    System.Type[] typeArray20 = new System.Type[9];
                    typeArray20[0] = typeof(IWork);
                    typeArray20[1] = typeof(ParallelTasks.Task);
                    typeArray20[2] = typeof(WorkOptions);
                    typeArray20[3] = typeof(VRage.Library.Threading.SpinLock);
                    typeArray20[4] = typeof(SpinLockRef);
                    typeArray20[5] = typeof(Monitor);
                    typeArray20[6] = typeof(AutoResetEvent);
                    typeArray20[7] = typeof(ManualResetEvent);
                    typeArray20[8] = typeof(Interlocked);
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray20);
                    System.Type[] typeArray21 = new System.Type[] { typeof(ProtoMemberAttribute), typeof(ProtoContractAttribute), typeof(ProtoIncludeAttribute), typeof(ProtoIgnoreAttribute), typeof(ProtoEnumAttribute), typeof(MemberSerializationOptions), typeof(DataFormat) };
                    batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray21);
                    MemberInfo[] infoArray8 = new MemberInfo[] { typeof(WorkData).GetMethod("FlagAsFailed") };
                    batch.AllowMembers(MyWhitelistTarget.ModApi, infoArray8);
                    MemberInfo[] infoArray9 = new MemberInfo[] { typeof(ArrayExtensions).GetMethod("Contains") };
                    batch.AllowMembers(MyWhitelistTarget.Both, infoArray9);
                    System.Type[] typeArray22 = new System.Type[] { typeof(MyPhysicalInventoryItemExtensions_ModAPI) };
                    batch.AllowTypes(MyWhitelistTarget.Both, typeArray22);
                    System.Type[] typeArray23 = new System.Type[] { typeof(System.Collections.Immutable.ImmutableArray) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, typeArray23);
                    return;
                }
            }
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyEntityStatComponent));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyFactionMember));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyFontEnum));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_SessionSettings));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(TerminalActionExtensions));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyAPIUtilities));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyEnvironmentSector));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(SerializableBlockOrientation));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(VRage.Game.ModAPI.Ingame.IMyCubeBlock));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(Sandbox.ModAPI.Ingame.IMyTerminalBlock));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(VRage.Game.ModAPI.IMyCubeBlock));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyFinalBuildConstants));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyAPIGateway));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMySession));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMyCameraController));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(VRage.ModAPI.IMyEntity));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMyEntities));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(VRage.Game.Entity.MyEntity));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyEntityExtensions));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(EnvironmentItemsEntry));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyIngameScript));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyGameLogicComponent));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMyComponentBase));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MySessionComponentBase));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_AdvancedDoor));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyObjectBuilder_AdvancedDoor));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_AdvancedDoorDefinition));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_BarbarianWaveEventDefinition));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyObjectBuilder_AdvancedDoorDefinition));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyObjectBuilder_BarbarianWaveEventDefinition));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyObjectBuilder_Base));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyDefinitionBase));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(MyObjectBuilder_AirVent));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_VoxelMap));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyStatLogic));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyObjectBuilder_EntityStatRegenEffect));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyEntityStat));
            IlChecker.AllowedOperands.Add(typeof(MyCharacterMovement), null);
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(SerializableDefinitionId));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(SerializableVector3));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyDefinitionId));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyDefinitionManager));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyDefinitionManagerBase));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(Vector3));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyFixedPoint));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(ListReader<>));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyStorageData));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyEventArgs));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyGameTimer));
            IlChecker.AllowNamespaceOfTypeCommon(typeof(VRage.Game.ModAPI.Ingame.IMyInventoryItem));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyLight));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMyTerminalAction));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(IMyAutomaticRifleGun));
            System.Type type = typeof(MyObjectBuilderSerializer);
            HashSet<MemberInfo> set = new HashSet<MemberInfo>();
            System.Type[] types = new System.Type[] { typeof(MyObjectBuilderType) };
            set.Add(type.GetMethod("CreateNewObject", types));
            System.Type[] typeArray25 = new System.Type[] { typeof(SerializableDefinitionId) };
            set.Add(type.GetMethod("CreateNewObject", typeArray25));
            System.Type[] typeArray26 = new System.Type[] { typeof(string) };
            set.Add(type.GetMethod("CreateNewObject", typeArray26));
            System.Type[] typeArray27 = new System.Type[] { typeof(MyObjectBuilderType), typeof(string) };
            set.Add(type.GetMethod("CreateNewObject", typeArray27));
            IlChecker.AllowedOperands[type] = set;
            IlChecker.AllowedOperands.Add(typeof(IWork), null);
            IlChecker.AllowedOperands.Add(typeof(ParallelTasks.Task), null);
            IlChecker.AllowedOperands.Add(typeof(WorkOptions), null);
            IlChecker.AllowedOperands.Add(typeof(Sandbox.ModAPI.Interfaces.ITerminalAction), null);
            IlChecker.AllowedOperands.Add(typeof(IMyInventoryOwner), null);
            IlChecker.AllowedOperands.Add(typeof(VRage.Game.ModAPI.Ingame.IMyInventory), null);
            IlChecker.AllowedOperands.Add(typeof(VRage.Game.ModAPI.Ingame.IMyInventoryItem), null);
            IlChecker.AllowedOperands.Add(typeof(ITerminalProperty), null);
            IlChecker.AllowedOperands.Add(typeof(ITerminalProperty<>), null);
            IlChecker.AllowedOperands.Add(typeof(TerminalPropertyExtensions), null);
            IlChecker.AllowedOperands.Add(typeof(MyFixedPoint), null);
            IlChecker.AllowedOperands.Add(typeof(MyTexts), null);
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(VRage.ModAPI.IMyInput));
            IlChecker.AllowedOperands.Add(typeof(MyInputExtensions), null);
            IlChecker.AllowedOperands.Add(typeof(MyKeys), null);
            IlChecker.AllowedOperands.Add(typeof(MyJoystickAxesEnum), null);
            IlChecker.AllowedOperands.Add(typeof(MyJoystickButtonsEnum), null);
            IlChecker.AllowedOperands.Add(typeof(MyMouseButtonsEnum), null);
            IlChecker.AllowedOperands.Add(typeof(MySharedButtonsEnum), null);
            IlChecker.AllowedOperands.Add(typeof(MyGuiControlTypeEnum), null);
            IlChecker.AllowedOperands.Add(typeof(MyGuiInputDeviceEnum), null);
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyResourceSourceComponent));
            IlChecker.AllowNamespaceOfTypeModAPI(typeof(MyCharacterOxygenComponent));
            IEnumerable<MethodInfo> source = from method in typeof(MyComponentContainer).GetMethods()
                where (method.Name == "TryGet") && (method.ContainsGenericParameters && (method.GetParameters().Length == 1))
                select method;
            set = new HashSet<MemberInfo>();
            System.Type[] typeArguments = new System.Type[] { typeof(MyResourceSourceComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("Has").MakeGenericMethod(typeArguments));
            System.Type[] typeArray29 = new System.Type[] { typeof(MyResourceSourceComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("Get").MakeGenericMethod(typeArray29));
            System.Type[] typeArray30 = new System.Type[] { typeof(System.Type), typeof(MyResourceSourceComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("TryGet", typeArray30));
            System.Type[] typeArray31 = new System.Type[] { typeof(MyResourceSourceComponent) };
            set.Add(source.FirstOrDefault<MethodInfo>().MakeGenericMethod(typeArray31));
            System.Type[] typeArray32 = new System.Type[] { typeof(MyResourceSinkComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("Has").MakeGenericMethod(typeArray32));
            System.Type[] typeArray33 = new System.Type[] { typeof(MyResourceSinkComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("Get").MakeGenericMethod(typeArray33));
            System.Type[] typeArray34 = new System.Type[] { typeof(System.Type), typeof(MyResourceSinkComponent) };
            set.Add(typeof(MyComponentContainer).GetMethod("TryGet", typeArray34));
            System.Type[] typeArray35 = new System.Type[] { typeof(MyResourceSinkComponent) };
            set.Add(source.FirstOrDefault<MethodInfo>().MakeGenericMethod(typeArray35));
            IlChecker.AllowedOperands.Add(typeof(MyComponentContainer), set);
            IlChecker.AllowedOperands.Add(typeof(MyResourceSourceComponentBase), null);
            HashSet<MemberInfo> set1 = new HashSet<MemberInfo>();
            set1.Add(typeof(MyResourceSinkComponentBase).GetProperty("AcceptedResources").GetGetMethod());
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("CurrentInputByType"));
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("IsPowerAvailable"));
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("IsPoweredByType"));
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("MaxRequiredInputByType"));
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("RequiredInputByType"));
            set1.Add(typeof(MyResourceSinkComponentBase).GetMethod("SuppliedRatioByType"));
            IlChecker.AllowedOperands.Add(typeof(MyResourceSinkComponentBase), set1);
            IlChecker.AllowedOperands.Add(typeof(ListReader<MyDefinitionId>), null);
            IlChecker.AllowedOperands.Add(typeof(MyDefinitionId), null);
        }

        private void InitIlCompiler()
        {
            Log.IncreaseIndent();
            if (GameCustomInitialization != null)
            {
                GameCustomInitialization.InitIlCompiler();
            }
            Log.DecreaseIndent();
            if (MyFakes.ENABLE_SCRIPTS_PDB)
            {
                if (MyFakes.ENABLE_ROSLYN_SCRIPTS)
                {
                    MyScriptCompiler.Static.EnableDebugInformation = true;
                }
                else
                {
                    IlCompiler.Options.CompilerOptions = $"/debug {IlCompiler.Options.CompilerOptions}";
                }
            }
        }

        protected virtual void InitInput()
        {
            MyStringId? description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.FORWARD, new MyGuiDescriptor(MyCommonTexts.ControlName_Forward, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.BACKWARD, new MyGuiDescriptor(MyCommonTexts.ControlName_Backward, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.STRAFE_LEFT, new MyGuiDescriptor(MyCommonTexts.ControlName_StrafeLeft, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.STRAFE_RIGHT, new MyGuiDescriptor(MyCommonTexts.ControlName_StrafeRight, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROLL_LEFT, new MyGuiDescriptor(MySpaceTexts.ControlName_RollLeft, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROLL_RIGHT, new MyGuiDescriptor(MySpaceTexts.ControlName_RollRight, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SPRINT, new MyGuiDescriptor(MyCommonTexts.ControlName_HoldToSprint, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.PRIMARY_TOOL_ACTION, new MyGuiDescriptor(MySpaceTexts.ControlName_FirePrimaryWeapon, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SECONDARY_TOOL_ACTION, new MyGuiDescriptor(MySpaceTexts.ControlName_FireSecondaryWeapon, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.JUMP, new MyGuiDescriptor(MyCommonTexts.ControlName_UpOrJump, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CROUCH, new MyGuiDescriptor(MyCommonTexts.ControlName_DownOrCrouch, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SWITCH_WALK, new MyGuiDescriptor(MyCommonTexts.ControlName_SwitchWalk, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.DAMPING, new MyGuiDescriptor(MySpaceTexts.ControlName_InertialDampenersOnOff, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.THRUSTS, new MyGuiDescriptor(MySpaceTexts.ControlName_Jetpack, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.BROADCASTING, new MyGuiDescriptor(MySpaceTexts.ControlName_Broadcasting, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.HELMET, new MyGuiDescriptor(MySpaceTexts.ControlName_Helmet, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.USE, new MyGuiDescriptor(MyCommonTexts.ControlName_UseOrInteract, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOGGLE_REACTORS, new MyGuiDescriptor(MySpaceTexts.ControlName_PowerSwitchOnOff, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TERMINAL, new MyGuiDescriptor(MySpaceTexts.ControlName_TerminalOrInventory, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.INVENTORY, new MyGuiDescriptor(MySpaceTexts.Inventory, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.HELP_SCREEN, new MyGuiDescriptor(MyCommonTexts.ControlName_Help, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SUICIDE, new MyGuiDescriptor(MyCommonTexts.ControlName_Suicide, description));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.PAUSE_GAME, new MyGuiDescriptor(MyCommonTexts.ControlName_PauseGame, new MyStringId?(MyCommonTexts.ControlDescPauseGame)));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROTATION_LEFT, new MyGuiDescriptor(MySpaceTexts.ControlName_RotationLeft, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROTATION_RIGHT, new MyGuiDescriptor(MySpaceTexts.ControlName_RotationRight, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROTATION_UP, new MyGuiDescriptor(MySpaceTexts.ControlName_RotationUp, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.ROTATION_DOWN, new MyGuiDescriptor(MySpaceTexts.ControlName_RotationDown, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CAMERA_MODE, new MyGuiDescriptor(MyCommonTexts.ControlName_FirstOrThirdPerson, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.HEADLIGHTS, new MyGuiDescriptor(MySpaceTexts.ControlName_ToggleHeadlights, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CHAT_SCREEN, new MyGuiDescriptor(MySpaceTexts.Chat_screen, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CONSOLE, new MyGuiDescriptor(MySpaceTexts.ControlName_Console, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SCREENSHOT, new MyGuiDescriptor(MyCommonTexts.ControlName_Screenshot, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.LOOKAROUND, new MyGuiDescriptor(MyCommonTexts.ControlName_HoldToLookAround, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.LANDING_GEAR, new MyGuiDescriptor(MySpaceTexts.ControlName_LandingGear, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SWITCH_LEFT, new MyGuiDescriptor(MyCommonTexts.ControlName_PreviousColor, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SWITCH_RIGHT, new MyGuiDescriptor(MyCommonTexts.ControlName_NextColor, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.BUILD_SCREEN, new MyGuiDescriptor(MyCommonTexts.ControlName_ToolbarConfig, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateVerticalPos, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateVerticalNeg, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateHorizontalPos, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateHorizontalNeg, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateRollPos, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeRotateRollNeg, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_COLOR_CHANGE, new MyGuiDescriptor(MyCommonTexts.ControlName_CubeColorChange, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SYMMETRY_SWITCH, new MyGuiDescriptor(MySpaceTexts.ControlName_SymmetrySwitch, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.USE_SYMMETRY, new MyGuiDescriptor(MySpaceTexts.ControlName_UseSymmetry, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_DEFAULT_MOUNTPOINT, new MyGuiDescriptor(MySpaceTexts.ControlName_CubeDefaultMountpoint, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT1, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot1, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT2, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot2, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT3, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot3, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT4, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot4, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT5, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot5, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT6, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot6, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT7, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot7, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT8, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot8, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT9, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot9, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SLOT0, new MyGuiDescriptor(MyCommonTexts.ControlName_Slot0, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOOLBAR_DOWN, new MyGuiDescriptor(MyCommonTexts.ControlName_ToolbarDown, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOOLBAR_UP, new MyGuiDescriptor(MyCommonTexts.ControlName_ToolbarUp, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOOLBAR_NEXT_ITEM, new MyGuiDescriptor(MyCommonTexts.ControlName_ToolbarNextItem, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOOLBAR_PREV_ITEM, new MyGuiDescriptor(MyCommonTexts.ControlName_ToolbarPreviousItem, description));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SPECTATOR_NONE, new MyGuiDescriptor(MyCommonTexts.SpectatorControls_None, new MyStringId?(MySpaceTexts.SpectatorControls_None_Desc)));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SPECTATOR_DELTA, new MyGuiDescriptor(MyCommonTexts.SpectatorControls_Delta, new MyStringId?(MyCommonTexts.SpectatorControls_Delta_Desc)));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SPECTATOR_FREE, new MyGuiDescriptor(MyCommonTexts.SpectatorControls_Free, new MyStringId?(MySpaceTexts.SpectatorControls_Free_Desc)));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.SPECTATOR_STATIC, new MyGuiDescriptor(MyCommonTexts.SpectatorControls_Static, new MyStringId?(MySpaceTexts.SpectatorControls_Static_Desc)));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOGGLE_HUD, new MyGuiDescriptor(MyCommonTexts.ControlName_HudOnOff, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.VOXEL_HAND_SETTINGS, new MyGuiDescriptor(MyCommonTexts.ControlName_VoxelHandSettings, description));
            description = null;
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CONTROL_MENU, new MyGuiDescriptor(MyCommonTexts.ControlName_ControlMenu, description));
            if (MyFakes.ENABLE_MISSION_TRIGGERS)
            {
                description = null;
                MyGuiGameControlsHelpers.Add(MyControlsSpace.MISSION_SETTINGS, new MyGuiDescriptor(MySpaceTexts.ControlName_MissionSettings, description));
            }
            MyGuiGameControlsHelpers.Add(MyControlsSpace.FREE_ROTATION, new MyGuiDescriptor(MySpaceTexts.StationRotation_Static, new MyStringId?(MySpaceTexts.StationRotation_Static_Desc)));
            if (MyPerGameSettings.VoiceChatEnabled)
            {
                description = null;
                MyGuiGameControlsHelpers.Add(MyControlsSpace.VOICE_CHAT, new MyGuiDescriptor(MyCommonTexts.ControlName_VoiceChat, description));
            }
            Dictionary<MyStringId, MyControl> self = new Dictionary<MyStringId, MyControl>(MyStringId.Comparer);
            MyMouseButtonsEnum? mouse = null;
            MyKeys? nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.FORWARD, mouse, 0x57, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.BACKWARD, mouse, 0x53, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.STRAFE_LEFT, mouse, 0x41, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.STRAFE_RIGHT, mouse, 0x44, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROTATION_LEFT, mouse, 0x25, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROTATION_RIGHT, mouse, 0x27, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROTATION_UP, mouse, 0x26, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROTATION_DOWN, mouse, 40, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROLL_LEFT, mouse, 0x51, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.ROLL_RIGHT, mouse, 0x45, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.SPRINT, mouse, 160, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.SWITCH_WALK, mouse, 20, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.JUMP, mouse, 0x20, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Navigation, MyControlsSpace.CROUCH, mouse, 0x43, nullable3);
            nullable3 = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.PRIMARY_TOOL_ACTION, 1, nullable3, nullable3);
            nullable3 = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.SECONDARY_TOOL_ACTION, 3, nullable3, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.USE, mouse, 70, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.HELMET, mouse, 0x4a, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.THRUSTS, mouse, 0x58, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.DAMPING, mouse, 90, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.BROADCASTING, mouse, 0x4f, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.TOGGLE_REACTORS, mouse, 0x59, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.HEADLIGHTS, mouse, 0x4c, nullable3);
            if (MyPerGameSettings.VoiceChatEnabled)
            {
                mouse = null;
                nullable3 = null;
                AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.VOICE_CHAT, mouse, 0x55, nullable3);
            }
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.CHAT_SCREEN, mouse, 13, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.TERMINAL, mouse, 0x4b, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.INVENTORY, mouse, 0x49, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems1, MyControlsSpace.SUICIDE, mouse, 8, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.BUILD_SCREEN, mouse, 0x47, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE, mouse, 0x22, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE, mouse, 0x2e, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE, mouse, 0x24, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE, mouse, 0x23, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE, mouse, 0x2d, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE, mouse, 0x21, nullable3);
            nullable3 = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_COLOR_CHANGE, 2, nullable3, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.CUBE_DEFAULT_MOUNTPOINT, mouse, 0x54, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.USE_SYMMETRY, mouse, 0x4e, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.SYMMETRY_SWITCH, mouse, 0x4d, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems2, MyControlsSpace.FREE_ROTATION, mouse, 0x42, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.TOOLBAR_UP, mouse, 190, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.TOOLBAR_DOWN, mouse, 0xbc, nullable3);
            mouse = null;
            nullable3 = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.TOOLBAR_NEXT_ITEM, mouse, nullable3, nullable3);
            mouse = null;
            nullable3 = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.TOOLBAR_PREV_ITEM, mouse, nullable3, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT1, mouse, 0x31, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT2, mouse, 50, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT3, mouse, 0x33, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT4, mouse, 0x34, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT5, mouse, 0x35, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT6, mouse, 0x36, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT7, mouse, 0x37, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT8, mouse, 0x38, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT9, mouse, 0x39, nullable3);
            mouse = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Systems3, MyControlsSpace.SLOT0, mouse, 0x30, 0xc0);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.SWITCH_LEFT, mouse, 0xdb, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.SWITCH_RIGHT, mouse, 0xdd, nullable3);
            if (MyFakes.ENABLE_MISSION_TRIGGERS)
            {
                mouse = null;
                nullable3 = null;
                AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.MISSION_SETTINGS, mouse, 0x55, nullable3);
            }
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.LANDING_GEAR, mouse, 80, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.VOXEL_HAND_SETTINGS, mouse, 0x4b, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.CONTROL_MENU, mouse, 0xbd, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.PAUSE_GAME, mouse, 0x13, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.CONSOLE, mouse, 0xc0, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.ToolsOrWeapons, MyControlsSpace.HELP_SCREEN, mouse, 0x70, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.SPECTATOR_NONE, mouse, 0x75, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.SPECTATOR_DELTA, mouse, 0x76, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.SPECTATOR_FREE, mouse, 0x77, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.SPECTATOR_STATIC, mouse, 120, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.CAMERA_MODE, mouse, 0x56, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.LOOKAROUND, mouse, 0xa4, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.SCREENSHOT, mouse, 0x73, nullable3);
            mouse = null;
            nullable3 = null;
            AddDefaultGameControl(self, MyGuiControlTypeEnum.Spectator, MyControlsSpace.TOGGLE_HUD, mouse, 9, nullable3);
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyInput.Initialize(new MyNullInput());
            }
            else
            {
                MyDirectXInput implementation = new MyDirectXInput(this.m_bufferedInputSource, new MyKeysToString(), self, false);
                MyInput.Initialize(implementation);
                MyTexts.RegisterEvaluator("CONTROL", implementation);
                MyTexts.RegisterEvaluator("GAME_CONTROL", implementation);
            }
            MySpaceBindingCreator.CreateBinding();
        }

        private void InitJoystick()
        {
            List<string> list = MyInput.Static.EnumerateJoystickNames();
            if (MyFakes.ENFORCE_CONTROLLER && (list.Count > 0))
            {
                MyInput.Static.JoystickInstanceName = list[0];
            }
        }

        private void InitModAPI()
        {
            try
            {
                this.InitIlCompiler();
                InitIlChecker();
            }
            catch (MyWhitelistException exception)
            {
                object[] args = new object[] { exception.Message };
                Log.Error("Mod API Whitelist Integrity: {0}", args);
                ShowWhitelistPopup = true;
            }
            catch (Exception exception2)
            {
                object[] args = new object[] { exception2.Message };
                Log.Error("Error during ModAPI initialization: {0}", args);
                ShowHotfixPopup = true;
            }
        }

        private void InitMultithreading()
        {
            if (MyFakes.FORCE_SINGLE_WORKER)
            {
                ParallelTasks.Parallel.Scheduler = new FixedPriorityScheduler(1, ThreadPriority.Normal);
            }
            else if (MyFakes.FORCE_NO_WORKER)
            {
                ParallelTasks.Parallel.Scheduler = new FakeTaskScheduler();
            }
            else
            {
                ParallelTasks.Parallel.Scheduler = new PrioritizedScheduler(Math.Max(NumberOfCores / 2, 1));
            }
        }

        private static void InitNumberOfCores()
        {
            NumberOfCores = VRage.Library.MyEnvironment.ProcessorCount;
            Log.WriteLine("Found processor count: " + NumberOfCores);
            NumberOfCores = MyUtils.GetClampInt(NumberOfCores, 1, 0x10);
            Log.WriteLine("Using processor count: " + NumberOfCores);
        }

        private void InitQuickLaunch()
        {
            bool flag;
            MyWorkshop.CancelToken cancelToken = new MyWorkshop.CancelToken();
            MyQuickLaunchType? nullable = null;
            if (this.m_joinLobbyId != null)
            {
                IMyLobby lobby = MyGameService.CreateLobby(this.m_joinLobbyId.Value);
                if (lobby.IsValid)
                {
                    MyJoinGameHelper.JoinGame(lobby, true);
                    return;
                }
            }
            if (((nullable != null) && !Sandbox.Engine.Platform.Game.IsDedicated) && (Sandbox.Engine.Platform.Game.ConnectToServer == null))
            {
                if (((MyQuickLaunchType) nullable.Value) > MyQuickLaunchType.LAST_SANDBOX)
                {
                    throw new InvalidBranchException();
                }
                MyGuiSandbox.AddScreen(new MyGuiScreenStartQuickLaunch(nullable.Value, MyCommonTexts.StartGameInProgressPleaseWait));
            }
            else if (MyFakes.ENABLE_LOGOS)
            {
                MyGuiSandbox.BackToIntroLogos(new Action(MySandboxGame.AfterLogos));
            }
            else
            {
                AfterLogos();
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                goto TR_0006;
            }
            else
            {
                flag = false;
                if (string.IsNullOrWhiteSpace(ConfigDedicated.WorldName))
                {
                    MyTexts.Get(MyCommonTexts.DefaultSaveName) + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                }
                else
                {
                    ConfigDedicated.WorldName.Trim();
                }
                try
                {
                    string lastSessionPath = MyLocalCache.GetLastSessionPath();
                    if ((!Sandbox.Engine.Platform.Game.IgnoreLastSession && !ConfigDedicated.IgnoreLastSession) && (lastSessionPath != null))
                    {
                        ulong num;
                        MyLog.Default.WriteLineAndConsole("Loading last session " + lastSessionPath);
                        MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(lastSessionPath, out num);
                        if (!MySession.IsCompatibleVersion(checkpoint))
                        {
                            MyLog.Default.WriteLineAndConsole(MyTexts.Get(MyCommonTexts.DialogTextIncompatibleWorldVersion).ToString());
                            Static.Exit();
                        }
                        else if (!MyWorkshop.DownloadWorldModsBlocking(checkpoint.Mods, cancelToken).Success)
                        {
                            MyLog.Default.WriteLineAndConsole("Unable to download mods");
                            Static.Exit();
                        }
                        else
                        {
                            MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Load);
                            MySession.Load(lastSessionPath, checkpoint, num, true, true);
                            MySession.Static.StartServer(Sandbox.Engine.Multiplayer.MyMultiplayer.Static);
                            goto TR_0010;
                        }
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ConfigDedicated.LoadWorld))
                        {
                            flag = true;
                            goto TR_0010;
                        }
                        else
                        {
                            string loadWorld = ConfigDedicated.LoadWorld;
                            if (!Path.IsPathRooted(loadWorld))
                            {
                                loadWorld = Path.Combine(MyFileSystem.SavesPath, loadWorld);
                            }
                            if (!Directory.Exists(loadWorld))
                            {
                                MyLog.Default.WriteLineAndConsole("World " + Path.GetFileName(ConfigDedicated.LoadWorld) + " not found.");
                                MyLog.Default.WriteLineAndConsole("Creating new one with same name");
                                flag = true;
                                Path.GetFileName(ConfigDedicated.LoadWorld);
                                goto TR_0010;
                            }
                            else
                            {
                                ulong num2;
                                MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(loadWorld, out num2);
                                if (!MySession.IsCompatibleVersion(checkpoint))
                                {
                                    MyLog.Default.WriteLineAndConsole(MyTexts.Get(MyCommonTexts.DialogTextIncompatibleWorldVersion).ToString());
                                    Static.Exit();
                                }
                                else if (!MyWorkshop.DownloadWorldModsBlocking(checkpoint.Mods, cancelToken).Success)
                                {
                                    MyLog.Default.WriteLineAndConsole("Unable to download mods");
                                    Static.Exit();
                                }
                                else
                                {
                                    MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Load);
                                    MySession.Load(loadWorld, checkpoint, num2, true, true);
                                    MySession.Static.StartServer(Sandbox.Engine.Multiplayer.MyMultiplayer.Static);
                                    goto TR_0010;
                                }
                            }
                        }
                        return;
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLineAndConsole("Exception while loading world: " + exception.Message);
                    MyLog.Default.WriteLine(exception.StackTrace);
                    Static.Exit();
                    return;
                }
            }
            goto TR_0010;
        TR_0006:
            if ((Sandbox.Engine.Platform.Game.ConnectToServer != null) && MyGameService.IsActive)
            {
                MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(this.ServerResponded);
                MyGameService.OnPingServerFailedToRespond += new EventHandler(this.ServerFailedToRespond);
                MyGameService.PingServer(Sandbox.Engine.Platform.Game.ConnectToServer.Address.ToIPv4NetworkOrder(), (ushort) Sandbox.Engine.Platform.Game.ConnectToServer.Port);
                Sandbox.Engine.Platform.Game.ConnectToServer = null;
            }
            return;
        TR_0010:
            if (flag)
            {
                ulong num3;
                MyObjectBuilder_SessionSettings sessionSettings = ConfigDedicated.SessionSettings;
                if (!MyFileSystem.DirectoryExists(ConfigDedicated.PremadeCheckpointPath))
                {
                    MyLog.Default.WriteLineAndConsole("Cannot start new world - Premade world not found " + ConfigDedicated.PremadeCheckpointPath);
                    Static.Exit();
                    return;
                }
                string premadeCheckpointPath = ConfigDedicated.PremadeCheckpointPath;
                MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(premadeCheckpointPath, out num3);
                if (checkpoint == null)
                {
                    MyLog.Default.WriteLineAndConsole("LoadCheckpoint failed.");
                    Static.Exit();
                    return;
                }
                checkpoint.Settings = sessionSettings;
                checkpoint.SessionName = ConfigDedicated.WorldName;
                if (!MyWorkshop.DownloadWorldModsBlocking(checkpoint.Mods, cancelToken).Success)
                {
                    MyLog.Default.WriteLineAndConsole("Unable to download mods");
                    Static.Exit();
                    return;
                }
                MySession.Load(premadeCheckpointPath, checkpoint, num3, true, true);
                MySession.Static.Save(Path.Combine(MyFileSystem.SavesPath, checkpoint.SessionName.Replace(':', '-')));
                MySession.Static.StartServer(Sandbox.Engine.Multiplayer.MyMultiplayer.Static);
            }
            goto TR_0006;
        }

        protected virtual void InitSteamWorkshop()
        {
            MyWorkshop.Category category = new MyWorkshop.Category {
                Id = "block",
                LocalizableName = MyCommonTexts.WorkshopTag_Block,
                IsVisibleForFilter = true
            };
            MyWorkshop.Category[] modCategories = new MyWorkshop.Category[13];
            modCategories[0] = category;
            category = new MyWorkshop.Category {
                Id = "skybox",
                LocalizableName = MyCommonTexts.WorkshopTag_Skybox,
                IsVisibleForFilter = true
            };
            modCategories[1] = category;
            category = new MyWorkshop.Category {
                Id = "character",
                LocalizableName = MyCommonTexts.WorkshopTag_Character,
                IsVisibleForFilter = true
            };
            modCategories[2] = category;
            category = new MyWorkshop.Category {
                Id = "animation",
                LocalizableName = MyCommonTexts.WorkshopTag_Animation,
                IsVisibleForFilter = true
            };
            modCategories[3] = category;
            category = new MyWorkshop.Category {
                Id = "respawn ship",
                LocalizableName = MySpaceTexts.WorkshopTag_RespawnShip,
                IsVisibleForFilter = true
            };
            modCategories[4] = category;
            category = new MyWorkshop.Category {
                Id = "production",
                LocalizableName = MySpaceTexts.WorkshopTag_Production,
                IsVisibleForFilter = true
            };
            modCategories[5] = category;
            category = new MyWorkshop.Category {
                Id = "script",
                LocalizableName = MyCommonTexts.WorkshopTag_Script,
                IsVisibleForFilter = true
            };
            modCategories[6] = category;
            category = new MyWorkshop.Category {
                Id = "modpack",
                LocalizableName = MyCommonTexts.WorkshopTag_ModPack,
                IsVisibleForFilter = true
            };
            modCategories[7] = category;
            category = new MyWorkshop.Category {
                Id = "asteroid",
                LocalizableName = MySpaceTexts.WorkshopTag_Asteroid,
                IsVisibleForFilter = true
            };
            modCategories[8] = category;
            category = new MyWorkshop.Category {
                Id = "planet",
                LocalizableName = MySpaceTexts.WorkshopTag_Planet,
                IsVisibleForFilter = true
            };
            modCategories[9] = category;
            category = new MyWorkshop.Category {
                Id = "hud",
                LocalizableName = MySpaceTexts.WorkshopTag_Hud,
                IsVisibleForFilter = true
            };
            modCategories[10] = category;
            category = new MyWorkshop.Category {
                Id = "other",
                LocalizableName = MyCommonTexts.WorkshopTag_Other,
                IsVisibleForFilter = true
            };
            modCategories[11] = category;
            category = new MyWorkshop.Category {
                Id = "npc",
                LocalizableName = MyCommonTexts.WorkshopTag_Npc,
                IsVisibleForFilter = false
            };
            modCategories[12] = category;
            category = new MyWorkshop.Category {
                Id = "exploration",
                LocalizableName = MySpaceTexts.WorkshopTag_Exploration
            };
            MyWorkshop.Category[] worldCategories = new MyWorkshop.Category[] { category };
            category = new MyWorkshop.Category {
                Id = "exploration",
                LocalizableName = MySpaceTexts.WorkshopTag_Exploration
            };
            MyWorkshop.Category[] blueprintCategories = new MyWorkshop.Category[] { category };
            MyWorkshop.Init(modCategories, worldCategories, blueprintCategories, new MyWorkshop.Category[0]);
        }

        public void Invoke(Action action, string invokerName)
        {
            MyInvokeData instance = new MyInvokeData {
                Action = action,
                Invoker = invokerName
            };
            this.m_invokeQueue.Enqueue(instance);
        }

        public void Invoke(string invokerName, object context, Action<object> action)
        {
            MyInvokeData instance = new MyInvokeData {
                ContextualAction = action,
                Context = context,
                Invoker = invokerName
            };
            this.m_invokeQueue.Enqueue(instance);
        }

        private unsafe void LoadData()
        {
            MyAudioInitParams* paramsPtr1;
            MyNullAudio audio1;
            if (MySession.Static != null)
            {
                MySession.Static.SetAsNotReady();
            }
            else if (MyAudio.Static != null)
            {
                MyAudio.Static.Mute = false;
            }
            if (MyInput.Static != null)
            {
                MyInput.Static.LoadContent(this.WindowHandle);
            }
            HkBaseSystem.Init(0x1000000, new Action<string>(this.LogWriter), false);
            this.WriteHavokCodeToLog();
            ParallelTasks.Parallel.StartOnEachWorker(() => HkBaseSystem.InitThread(Thread.CurrentThread.Name));
            MyPhysicsDebugDraw.DebugGeometry = new HkGeometry();
            Log.WriteLine("MySandboxGame.LoadData() - START");
            Log.IncreaseIndent();
            StartPreload();
            MyDefinitionManager.Static.PreloadDefinitions();
            MyAudioInitParams initParams = new MyAudioInitParams();
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                audio1 = (MyNullAudio) new MyXAudio2();
            }
            else
            {
                audio1 = new MyNullAudio();
            }
            paramsPtr1->Instance = audio1;
            paramsPtr1 = (MyAudioInitParams*) ref initParams;
            initParams.SimulateNoSoundCard = MyFakes.SIMULATE_NO_SOUND_CARD;
            initParams.DisablePooling = MyFakes.DISABLE_SOUND_POOLING;
            initParams.OnSoundError = MyAudioExtensions.OnSoundError;
            MyAudio.LoadData(initParams, MyAudioExtensions.GetSoundDataFromDefinitions(), MyAudioExtensions.GetEffectData());
            if (MyPerGameSettings.UseVolumeLimiter)
            {
                MyAudio.Static.UseVolumeLimiter = true;
            }
            if (MyPerGameSettings.UseSameSoundLimiter)
            {
                MyAudio.Static.UseSameSoundLimiter = true;
                MyAudio.Static.SetSameSoundLimiter();
            }
            if (!MyPerGameSettings.UseReverbEffect || !MyFakes.AUDIO_ENABLE_REVERB)
            {
                Config.EnableReverb = false;
                MyAudio.Static.EnableReverb = false;
            }
            else if (!Config.EnableReverb || (MyAudio.Static.SampleRate <= MyAudio.MAX_SAMPLE_RATE))
            {
                MyAudio.Static.EnableReverb = Config.EnableReverb;
            }
            else
            {
                Config.EnableReverb = false;
            }
            MyAudio.Static.EnableDoppler = Config.EnableDoppler;
            MyAudio.Static.VolumeMusic = Config.MusicVolume;
            MyAudio.Static.VolumeGame = Config.GameVolume;
            MyAudio.Static.VolumeHud = Config.GameVolume;
            MyAudio.Static.VolumeVoiceChat = Config.VoiceChatVolume;
            MyAudio.Static.EnableVoiceChat = Config.EnableVoiceChat;
            MyGuiAudio.HudWarnings = Config.HudWarnings;
            MyGuiSoundManager.Audio = MyGuiAudio.Static;
            MyLocalization.Initialize();
            MyGuiSandbox.LoadData(Sandbox.Engine.Platform.Game.IsDedicated);
            this.LoadGui();
            MyGuiSkinManager.Static.Init();
            this.m_dataLoadedDebug = true;
            if (!Sandbox.Engine.Platform.Game.IsDedicated && MyGameService.IsActive)
            {
                MyGameService.LobbyJoinRequested += new MyLobbyJoinRequested(this.Matchmaking_LobbyJoinRequest);
                MyGameService.ServerChangeRequested += new MyLobbyServerChangeRequested(this.Matchmaking_ServerChangeRequest);
            }
            MyInput.Static.LoadData(Config.ControlsGeneral, Config.ControlsButtons);
            this.InitJoystick();
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyParticlesManager.Enabled = false;
            }
            MyParticlesManager.EnableCPUGenerations = MyFakes.ENABLE_CPU_PARTICLES;
            MyParticlesManager.CalculateGravityInPoint = new Func<Vector3D, Vector3>(MyGravityProviderSystem.CalculateTotalGravityInPoint);
            MyRenderProxy.SetGravityProvider(new Func<Vector3D, Vector3>(MyGravityProviderSystem.CalculateTotalGravityInPoint));
            Log.DecreaseIndent();
            Log.WriteLine("MySandboxGame.LoadData() - END");
            this.InitModAPI();
            MyPositionComponentBase.OnReportInvalidMatrix = (Action<VRage.ModAPI.IMyEntity>) Delegate.Combine(MyPositionComponentBase.OnReportInvalidMatrix, new Action<VRage.ModAPI.IMyEntity>(this.ReportInvalidMatrix));
            if (this.OnGameLoaded != null)
            {
                this.OnGameLoaded(this, null);
            }
        }

        protected override void LoadData_UpdateThread()
        {
        }

        protected virtual void LoadGui()
        {
            MyGuiSandbox.LoadContent();
        }

        private void LogWriter(string text)
        {
            Log.WriteLine("Havok: " + text);
        }

        private void Matchmaking_LobbyJoinRequest(IMyLobby lobby, ulong invitedBy)
        {
            if (!this.m_isPendingLobbyInvite && (lobby.IsValid && (((MySession.Static == null) || (Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null)) || (Sandbox.Engine.Multiplayer.MyMultiplayer.Static.LobbyId != lobby.LobbyId))))
            {
                this.m_isPendingLobbyInvite = true;
                this.m_invitingLobby = lobby;
                this.m_lobbyInviter = invitedBy;
                if (invitedBy == MyGameService.UserId)
                {
                    this.OnAcceptLobbyInvite(MyGuiScreenMessageBox.ResultEnum.YES);
                }
                else
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.InvitedToLobbyCaption);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.InvitedToLobby), MyGameService.GetPersonaName(invitedBy))), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnAcceptLobbyInvite), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        private void Matchmaking_ServerChangeRequest(string server, string password)
        {
            IPEndPoint point;
            if (IPAddressExtensions.TryParseEndpoint(server, out point))
            {
                MySessionLoader.UnloadAndExitToMenu();
                MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(this.ServerResponded);
                MyGameService.OnPingServerFailedToRespond += new EventHandler(this.ServerFailedToRespond);
                MyGameService.PingServer(point.Address.ToIPv4NetworkOrder(), (ushort) point.Port);
            }
        }

        public void OnAcceptLobbyInvite(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (this.m_isPendingLobbyInvite)
            {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                    MyJoinGameHelper.JoinGame(this.m_invitingLobby, true);
                }
                this.m_isPendingLobbyInvite = false;
            }
        }

        private static void OnDotNetHotfixPopupClosed(MyGuiScreenMessageBox.ResultEnum result)
        {
            Process.Start("https://support.microsoft.com/kb/3120241");
            ClosePopup(result);
        }

        private unsafe void OnToolIsGameRunningMessage(ref System.Windows.Forms.Message msg)
        {
            IntPtr ptr;
            IntPtr* ptrPtr1 = (IntPtr*) new IntPtr((MySession.Static == null) ? 0 : 1);
            ptrPtr1 = (IntPtr*) ref ptr;
            WinApi.PostMessage(msg.WParam, 0x41f, ptr, IntPtr.Zero);
        }

        private static void OnWhitelistIntegrityPopupClosed(MyGuiScreenMessageBox.ResultEnum result)
        {
            ClosePopup(result);
        }

        private void OverlayActivated(byte result)
        {
            if (result == 1)
            {
                if (!Sync.MultiplayerActive)
                {
                    PausePush();
                }
                Static.PauseInput = true;
                Static.m_unpauseInput = false;
            }
            else
            {
                if (!Sync.MultiplayerActive)
                {
                    PausePop();
                }
                Static.m_unpauseInput = true;
                Static.m_inputPauseTime = DateTime.Now;
            }
        }

        public static void PausePop()
        {
            m_pauseStackCount--;
            if (m_pauseStackCount < 0)
            {
                m_pauseStackCount = 0;
            }
            UpdatePauseState(m_pauseStackCount);
        }

        public static void PausePush()
        {
            UpdatePauseState(++m_pauseStackCount);
        }

        public static void PauseToggle()
        {
            if (!Sync.MultiplayerActive)
            {
                if (IsPaused)
                {
                    PausePop();
                }
                else
                {
                    PausePush();
                }
            }
        }

        private static unsafe void PerformPreloading()
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.InitOfflineReplicationLayer();
            MyMath.InitializeFastSin();
            List<Tuple<MyObjectBuilder_Definitions, string>> list = null;
            try
            {
                list = MyDefinitionManager.Static.PrepareBaseDefinitions();
            }
            catch (MyLoadingException exception1)
            {
                string message = exception1.Message;
                Log.WriteLineAndConsole(message);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(message), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(MySandboxGame.ClosePopup), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
                Vector2 vector = screen.Size.Value;
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] *= 1.5f;
                screen.Size = new Vector2?(vector);
                screen.RecreateControls(false);
                MyGuiSandbox.AddScreen(screen);
            }
            if (list != null)
            {
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MyGuiTextures.Static.Reload();
                }
                using (List<Tuple<MyObjectBuilder_Definitions, string>>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyObjectBuilder_Definitions definitions = enumerator.Current.Item1;
                        if (!Sandbox.Engine.Platform.Game.IsDedicated && (definitions.Sounds != null))
                        {
                            MyObjectBuilder_AudioDefinition[] sounds = definitions.Sounds;
                            for (int i = 0; i < sounds.Length; i++)
                            {
                                MyObjectBuilder_AudioDefinition definition1 = sounds[i];
                                MyStringHash.GetOrCompute(definition1.Id.SubtypeName);
                                foreach (MyAudioWave wave in definition1.Waves)
                                {
                                    if (!string.IsNullOrEmpty(wave.Start))
                                    {
                                        MyInMemoryWaveDataCache.Static.Preload(wave.Start);
                                    }
                                    if (!string.IsNullOrEmpty(wave.Loop))
                                    {
                                        MyInMemoryWaveDataCache.Static.Preload(wave.Loop);
                                    }
                                    if (!string.IsNullOrEmpty(wave.End))
                                    {
                                        MyInMemoryWaveDataCache.Static.Preload(wave.End);
                                    }
                                }
                            }
                        }
                        if (definitions.VoxelMapStorages != null)
                        {
                            MyDefinitionManager.Static.ReloadVoxelMaterials();
                            foreach (MyObjectBuilder_VoxelMapStorageDefinition definition in definitions.VoxelMapStorages)
                            {
                                if (!string.IsNullOrEmpty(definition.StorageFile) && ((definition.UseAsPrimaryProceduralAdditionShape || definition.UseForProceduralAdditions) || definition.UseForProceduralRemovals))
                                {
                                    MyStorageBase.LoadFromFile(Path.Combine(MyFileSystem.ContentPath, definition.StorageFile), null, true);
                                }
                            }
                        }
                    }
                }
            }
            IsPreloading = false;
        }

        private static void Preallocate()
        {
            Log.WriteLine("Preallocate - START");
            Log.IncreaseIndent();
            System.Type[] types = new System.Type[] { typeof(Sandbox.Game.Entities.MyEntities), typeof(MyObjectBuilder_Base), typeof(MyTransparentGeometry), typeof(MyCubeGridDeformationTables), typeof(MyMath), typeof(MySimpleObjectDraw) };
            try
            {
                PreloadTypesFrom(MyPlugins.GameAssembly);
                PreloadTypesFrom(MyPlugins.SandboxAssembly);
                PreloadTypesFrom(MyPlugins.UserAssemblies);
                ForceStaticCtor(types);
                PreloadTypesFrom(typeof(MySandboxGame).Assembly);
            }
            catch (ReflectionTypeLoadException exception1)
            {
                StringBuilder builder = new StringBuilder();
                Exception[] loaderExceptions = exception1.LoaderExceptions;
                int index = 0;
                while (true)
                {
                    if (index >= loaderExceptions.Length)
                    {
                        builder.ToString();
                        break;
                    }
                    Exception exception = loaderExceptions[index];
                    builder.AppendLine(exception.Message);
                    if (exception is FileNotFoundException)
                    {
                        FileNotFoundException exception2 = exception as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exception2.FusionLog))
                        {
                            builder.AppendLine("Fusion Log:");
                            builder.AppendLine(exception2.FusionLog);
                        }
                    }
                    builder.AppendLine();
                    index++;
                }
            }
            Log.DecreaseIndent();
            Log.WriteLine("Preallocate - END");
        }

        private static void PreloadTypesFrom(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    PreloadTypesFrom(assemblyArray[i]);
                }
            }
        }

        private static void PreloadTypesFrom(Assembly assembly)
        {
            if (assembly != null)
            {
                ForceStaticCtor((from type in assembly.GetTypes()
                    where Attribute.IsDefined(type, typeof(PreloadRequiredAttribute))
                    select type).ToArray<System.Type>());
            }
        }

        protected override void PrepareForDraw()
        {
            MyStatToken token;
            using (token = Stats.Generic.Measure("GuiPrepareDraw"))
            {
                MyGuiSandbox.Draw();
            }
            using (token = Stats.Generic.Measure("DebugDraw"))
            {
                Sandbox.Game.Entities.MyEntities.DebugDraw();
            }
            using (token = Stats.Generic.Measure("Hierarchy"))
            {
                if (MyGridPhysicalHierarchy.Static != null)
                {
                    MyGridPhysicalHierarchy.Static.Draw();
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_PARTICLES)
            {
                MyParticlesLibrary.DebugDraw();
            }
        }

        public void ProcessInvoke()
        {
            MyInvokeData data;
            MyUtils.Swap<MyConcurrentQueue<MyInvokeData>>(ref this.m_invokeQueue, ref this.m_invokeQueueExecuting);
            while (this.m_invokeQueueExecuting.TryDequeue(out data))
            {
                if (data.Action != null)
                {
                    data.Action();
                    continue;
                }
                data.ContextualAction(data.Context);
            }
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.ProcessInvoke();
            }
        }

        public static void ProcessRenderOutput()
        {
            MyRenderMessageBase base2;
            while (MyRenderProxy.OutputQueue.TryDequeue(out base2))
            {
                if (base2 == null)
                {
                    continue;
                }
                MyRenderMessageEnum messageType = base2.MessageType;
                if (messageType > MyRenderMessageEnum.VideoAdaptersResponse)
                {
                    if (messageType == MyRenderMessageEnum.CreatedDeviceSettings)
                    {
                        MyVideoSettingsManager.OnCreatedDeviceSettings((MyRenderMessageCreatedDeviceSettings) base2);
                    }
                    else if (messageType != MyRenderMessageEnum.MainThreadCallback)
                    {
                        if (messageType == MyRenderMessageEnum.TasksFinished)
                        {
                            RenderTasksFinished = true;
                        }
                    }
                    else
                    {
                        MyRenderMessageMainThreadCallback callback = (MyRenderMessageMainThreadCallback) base2;
                        if (callback.Callback != null)
                        {
                            callback.Callback();
                        }
                        callback.Callback = null;
                    }
                }
                else if (messageType == MyRenderMessageEnum.ClipmapsReady)
                {
                    AreClipmapsReady = true;
                }
                else
                {
                    switch (messageType)
                    {
                        case MyRenderMessageEnum.ScreenshotTaken:
                            if (MySession.Static != null)
                            {
                                MyRenderMessageScreenshotTaken taken = (MyRenderMessageScreenshotTaken) base2;
                                if (taken.ShowNotification)
                                {
                                    MyHudNotification notification = new MyHudNotification(taken.Success ? MyCommonTexts.ScreenshotSaved : MyCommonTexts.ScreenshotFailed, 0x7d0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                                    if (taken.Success)
                                    {
                                        object[] arguments = new object[] { Path.GetFileName(taken.Filename) };
                                        notification.SetTextFormatArguments(arguments);
                                    }
                                    MyHud.Notifications.Add(notification);
                                }
                                if ((Static != null) && (Static.OnScreenshotTaken != null))
                                {
                                    Static.OnScreenshotTaken(Static, null);
                                }
                                MyGuiBlueprintScreen_Reworked.ScreenshotTaken(taken.Success, taken.Filename);
                            }
                            break;

                        case MyRenderMessageEnum.ExportToObjComplete:
                        {
                            MyRenderMessageExportToObjComplete complete1 = (MyRenderMessageExportToObjComplete) base2;
                            break;
                        }
                        case MyRenderMessageEnum.Error:
                        {
                            MyRenderMessageError error = (MyRenderMessageError) base2;
                            ErrorConsumer.OnError("Renderer error", error.Message, error.Callstack);
                            if (error.ShouldTerminate)
                            {
                                ExitThreadSafe();
                            }
                            break;
                        }
                        default:
                            if (messageType == MyRenderMessageEnum.VideoAdaptersResponse)
                            {
                                MyRenderMessageVideoAdaptersResponse message = (MyRenderMessageVideoAdaptersResponse) base2;
                                MyVideoSettingsManager.OnVideoAdaptersResponse(message);
                                Static.CheckGraphicsCard(message);
                                bool firstTimeRun = Config.FirstTimeRun;
                                if (firstTimeRun)
                                {
                                    Config.FirstTimeRun = false;
                                    Config.ExperimentalMode = false;
                                    MyVideoSettingsManager.WriteCurrentSettingsToConfig();
                                    Config.Save();
                                }
                                if (firstTimeRun)
                                {
                                    Config.FirstVTTimeRun = false;
                                }
                                if ((Config.FirstVTTimeRun && !firstTimeRun) && ((MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality == MyRenderQualityEnum.HIGH) || (MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality == MyRenderQualityEnum.EXTREME)))
                                {
                                    Config.SetToMediumQuality();
                                    Config.FirstVTTimeRun = false;
                                    Config.Save();
                                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo);
                                    MyStringId? okButtonText = null;
                                    okButtonText = null;
                                    okButtonText = null;
                                    okButtonText = null;
                                    Vector2? size = null;
                                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.SwitchToNormalVT), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                                }
                                if (MySpaceAnalytics.Instance != null)
                                {
                                    MySpaceAnalytics.Instance.StartSessionAndIdentifyPlayer(firstTimeRun);
                                }
                            }
                            break;
                    }
                }
                base2.Dispose();
            }
        }

        private void RegisterAssemblies(string[] args)
        {
            MyPlugins.RegisterGameAssemblyFile(MyPerGameSettings.GameModAssembly);
            if (MyPerGameSettings.GameModBaseObjBuildersAssembly != null)
            {
                MyPlugins.RegisterBaseGameObjectBuildersAssemblyFile(MyPerGameSettings.GameModBaseObjBuildersAssembly);
            }
            MyPlugins.RegisterGameObjectBuildersAssemblyFile(MyPerGameSettings.GameModObjBuildersAssembly);
            MyPlugins.RegisterSandboxAssemblyFile(MyPerGameSettings.SandboxAssembly);
            MyPlugins.RegisterSandboxGameAssemblyFile(MyPerGameSettings.SandboxGameAssembly);
            MyPlugins.RegisterFromArgs(args);
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyPlugins.RegisterUserAssemblyFiles(ConfigDedicated.Plugins);
            }
            MyPlugins.Load();
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if ((args[i] == "+connect_lobby") && (args.Length > (i + 1)))
                    {
                        ulong num2;
                        i++;
                        if (ulong.TryParse(args[i], out num2))
                        {
                            this.m_joinLobbyId = new ulong?(num2);
                        }
                    }
                }
            }
        }

        public static void ReloadDedicatedServerSession()
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyLog.Default.WriteLineAndConsole("Reloading dedicated server");
                IsReloading = true;
                Static.Exit();
            }
        }

        protected void RenderThread_BeforeDraw()
        {
            MyFpsManager.Update();
        }

        protected void RenderThread_SizeChanged(int width, int height, MyViewport viewport)
        {
            this.Invoke(() => UpdateScreenSize(width, height, viewport), "MySandboxGame::UpdateScreenSize");
        }

        private void ReportInvalidMatrix(VRage.Game.ModAPI.Ingame.IMyEntity entity)
        {
            if (((entity is VRage.Game.Entity.MyEntity) && Sandbox.Engine.Platform.Game.IsDedicated) && MyPerGameSettings.SendLogToKeen)
            {
                if (MySession.Static.Players.GetEntityController((VRage.Game.Entity.MyEntity) entity) == null)
                {
                    string name = entity.Name;
                    string message = name;
                    if (name == null)
                    {
                        string local1 = name;
                        object[] objArray1 = new object[] { entity.ToString(), " with ID:", entity.EntityId, " has invalid world matrix! Deleted." };
                        message = string.Concat(objArray1);
                    }
                    Log.Error(message, Array.Empty<object>());
                    ((VRage.Game.Entity.MyEntity) entity).Close();
                }
                MyReportException exception = new MyReportException();
                MyLog.Default.WriteLineAndConsole("Exception with invalid matrix");
                MyLog.Default.WriteLine(exception.ToString());
                MyLog.Default.WriteLine(System.Environment.StackTrace);
                if (PerformNotInteractiveReport != null)
                {
                    PerformNotInteractiveReport();
                }
            }
        }

        internal static void ResetColdStartRegister()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam\Apps\244850\Coldstart", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    key.SetValue("SpaceEngineers_ColdStart", 0);
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Coldstart reset failed: " + exception);
            }
        }

        public void Run(bool customRenderLoop = false, Action disposeSplashScreen = null)
        {
            if (!FatalErrorDuringInit)
            {
                if (this.GameRenderComponent != null)
                {
                    MyVideoSettingsManager.LogApplicationInformation();
                    MyVideoSettingsManager.LogEnvironmentInformation();
                }
                this.Initialize();
                if (disposeSplashScreen != null)
                {
                    disposeSplashScreen();
                }
                this.LoadData_UpdateThread();
                foreach (IPlugin plugin in MyPlugins.Plugins)
                {
                    Log.WriteLineAndConsole("Plugin Init: " + plugin.GetType());
                    plugin.Init(this);
                }
                if (MyPerGameSettings.Destruction && !HkBaseSystem.DestructionEnabled)
                {
                    MyLog.Default.WriteLine("Havok Destruction is not availiable in this build. Exiting game.");
                    ExitThreadSafe();
                }
                else
                {
                    if (this.form != null)
                    {
                        this.form.Invoke(delegate {
                            this.form.Show();
                            this.form.Focus();
                        });
                    }
                    if (!customRenderLoop)
                    {
                        base.RunLoop();
                        this.EndLoop();
                    }
                }
            }
        }

        public void ServerFailedToRespond(object sender, object e)
        {
            MyLog.Default.WriteLineAndConsole("Server failed to respond");
            this.CloseHandlers();
        }

        public void ServerResponded(object sender, MyGameServerItem serverItem)
        {
            MyLog.Default.WriteLineAndConsole("Server responded");
            this.CloseHandlers();
            MyJoinGameHelper.JoinGame(serverItem, true);
        }

        public void SetMouseVisible(bool visible)
        {
            this.IsCursorVisible = visible;
            if (this.m_setMouseVisible != null)
            {
                this.m_setMouseVisible(visible);
            }
        }

        private void ShowUpdateDriverDialog(MyAdapterInfo adapter)
        {
            MessageBoxResult result = MyErrorReporter.ReportOldDrivers(MyPerGameSettings.GameName, adapter.DeviceName, adapter.DriverUpdateLink);
            if (result == MessageBoxResult.Yes)
            {
                ExitThreadSafe();
                MyBrowserHelper.OpenInternetBrowser(adapter.DriverUpdateLink);
            }
            else if (result == MessageBoxResult.No)
            {
                Config.DisableUpdateDriverNotification = true;
                Config.Save();
            }
            else
            {
                MessageBoxResult result1 = result;
            }
        }

        private void StartAutoUpdateCountdown()
        {
            this.m_isGoingToUpdate = true;
            this.m_autoUpdateRestartTimeInMin = (TotalTimeInMilliseconds / 0xea60) + ConfigDedicated.AutoUpdateRestartDelayInMin;
        }

        public static void StartPreload()
        {
            IsPreloading = true;
            ParallelTasks.Parallel.Start(new Action(MySandboxGame.PerformPreloading));
        }

        protected virtual void StartRenderComponent(MyRenderDeviceSettings? settingsToTry)
        {
            if (settingsToTry != null)
            {
                MyRenderDeviceSettings settings = settingsToTry.Value;
                settings.DisableWindowedModeForOldDriver = Config.DisableUpdateDriverNotification;
                settingsToTry = new MyRenderDeviceSettings?(settings);
            }
            if (Config.SyncRendering)
            {
                this.GameRenderComponent.StartSync(base.m_gameTimer, this.InitializeRenderThread(), settingsToTry, MyRenderQualityEnum.NORMAL, MyPerGameSettings.MaxFrameRate);
            }
            else
            {
                this.GameRenderComponent.Start(base.m_gameTimer, new InitHandler(this.InitializeRenderThread), settingsToTry, MyRenderQualityEnum.NORMAL, MyPerGameSettings.MaxFrameRate);
            }
            this.GameRenderComponent.RenderThread.SizeChanged += new SizeChangedHandler(this.RenderThread_SizeChanged);
            this.GameRenderComponent.RenderThread.BeforeDraw += new Action(this.RenderThread_BeforeDraw);
        }

        public virtual void SwitchSettings(MyRenderDeviceSettings settings)
        {
            MyRenderProxy.SwitchDeviceSettings(settings);
        }

        private void UnloadAudio()
        {
            if (MyAudio.Static != null)
            {
                MyAudio.Static.Mute = true;
                MyXAudio2 @static = MyAudio.Static as MyXAudio2;
                if (@static != null)
                {
                    MyEntity3DSoundEmitter.ClearEntityEmitters();
                    @static.ClearSounds();
                    MyHud.ScreenEffects.FadeScreen(1f, 0f);
                }
            }
        }

        private void UnloadData()
        {
            Log.WriteLine("MySandboxGame.UnloadData() - START");
            Log.IncreaseIndent();
            this.UnloadAudio();
            this.UnloadInput();
            MyAudio.UnloadData();
            Log.DecreaseIndent();
            Log.WriteLine("MySandboxGame.UnloadData() - END");
            MyModels.UnloadData();
            MyGuiSandbox.UnloadContent();
        }

        protected override void UnloadData_UpdateThread()
        {
            if (MySession.Static != null)
            {
                MySession.Static.Unload();
            }
            this.UnloadData();
            if (this.GameRenderComponent != null)
            {
                this.GameRenderComponent.Stop();
                this.GameRenderComponent.Dispose();
            }
            StringBuilder output = new StringBuilder();
            output.AppendLine("Havok memory statistics:");
            HkBaseSystem.GetMemoryStatistics(output);
            MyLog.Default.WriteLine(output.ToString());
            MyPhysicsDebugDraw.DebugGeometry.Dispose();
            ParallelTasks.Parallel.StartOnEachWorker(new Action(HkBaseSystem.QuitThread));
            if (MyFakes.ENABLE_HAVOK_MULTITHREADING)
            {
                HkBaseSystem.Quit();
            }
            MySimpleProfiler.LogPerformanceTestResults();
        }

        private void UnloadInput()
        {
            MyInput.UnloadData();
            MyGuiGameControlsHelpers.Reset();
        }

        protected override void Update()
        {
            StringBuilder builder;
            MyStringId? nullable;
            Vector2? nullable2;
            MyStatToken token;
            if ((IsRenderUpdateSyncEnabled && (this.GameRenderComponent != null)) && (this.GameRenderComponent.RenderThread != null))
            {
                if (this.GameRenderComponent.RenderThread.RenderUpdateSyncEvent == null)
                {
                    this.GameRenderComponent.RenderThread.RenderUpdateSyncEvent = new ManualResetEvent(false);
                }
                else
                {
                    this.GameRenderComponent.RenderThread.RenderUpdateSyncEvent.Set();
                    this.GameRenderComponent.RenderThread.RenderUpdateSyncEvent.Reset();
                }
            }
            if (ShowHotfixPopup && CanShowHotfixPopup)
            {
                ShowHotfixPopup = false;
                builder = MyTexts.Get(MyCommonTexts.ErrorPopup_Hotfix_Caption);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.ErrorPopup_Hotfix_Text), builder, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(MySandboxGame.OnDotNetHotfixPopupClosed), 0, MyGuiScreenMessageBox.ResultEnum.NO, true, nullable2));
            }
            if (ShowWhitelistPopup && CanShowWhitelistPopup)
            {
                ShowHotfixPopup = false;
                builder = MyTexts.Get(MyCommonTexts.ErrorPopup_Whitelist_Caption);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.ErrorPopup_Whitelist_Text), builder, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(MySandboxGame.OnWhitelistIntegrityPopupClosed), 0, MyGuiScreenMessageBox.ResultEnum.NO, true, nullable2));
            }
            long timestamp = Stopwatch.GetTimestamp();
            long num = timestamp - m_lastFrameTimeStamp;
            m_lastFrameTimeStamp = timestamp;
            SecondsSinceLastFrame = MyRandom.EnableDeterminism ? 0.01666666753590107 : (((double) num) / ((double) Stopwatch.Frequency));
            if (ShowIsBetterGCAvailableNotification)
            {
                ShowIsBetterGCAvailableNotification = false;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.BetterGCIsAvailable), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            if (ShowGpuUnderMinimumNotification)
            {
                ShowGpuUnderMinimumNotification = false;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.GpuUnderMinimumNotification), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                ExitListenerSTA.Listen();
            }
            MyMessageLoop.Process();
            using (token = Stats.Generic.Measure("InvokeQueue"))
            {
                this.ProcessInvoke();
            }
            MyGeneralStats.Static.Update();
            if (Config.SyncRendering)
            {
                if ((IsVideoRecordingEnabled && (MySession.Static != null)) && IsGameReady)
                {
                    string pathToSave = Path.Combine(MyFileSystem.UserDataPath, "Recording", "img_" + base.SimulationFrameCounter.ToString("D8") + ".png");
                    MyRenderProxy.TakeScreenshot(Vector2.One, pathToSave, false, true, false);
                }
                this.GameRenderComponent.RenderThread.TickSync();
            }
            using (token = Stats.Generic.Measure("RenderRequests"))
            {
                ProcessRenderOutput();
            }
            using (token = Stats.Generic.Measure("Network"))
            {
                if (Sync.Layer != null)
                {
                    Sync.Layer.TransportLayer.Tick();
                }
                MyGameService.Update();
                try
                {
                    MyNetworkReader.Process();
                }
                catch (MyIncompatibleDataException)
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Dispose();
                    MySessionLoader.UnloadAndExitToMenu();
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.IncompatibleDataNotification), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReportReplicatedObjects();
            }
            using (token = Stats.Generic.Measure("GuiUpdate"))
            {
                MyGuiSandbox.Update(0x10);
            }
            using (token = Stats.Generic.Measure("Input"))
            {
                MyGuiSandbox.HandleInput();
                if (!Sandbox.Engine.Platform.Game.IsDedicated && (MySession.Static != null))
                {
                    MySession.Static.HandleInput();
                }
            }
            using (token = Stats.Generic.Measure("GameLogic"))
            {
                if (MySession.Static != null)
                {
                    bool flag = true;
                    if (Sandbox.Engine.Platform.Game.IsDedicated && ConfigDedicated.PauseGameWhenEmpty)
                    {
                        flag = (Sync.Clients.Count > 1) || !MySession.Static.Ready;
                    }
                    if (flag)
                    {
                        MySession.Static.Update(base.TotalTime);
                    }
                }
            }
            using (token = Stats.Generic.Measure("InputAfter"))
            {
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MyGuiSandbox.HandleInputAfterSimulation();
                }
            }
            if (MyFakes.SIMULATE_SLOW_UPDATE)
            {
                Thread.Sleep(40);
            }
            using (token = Stats.Generic.Measure("Audio"))
            {
                Vector3 up = Vector3.Up;
                Vector3 forward = Vector3.Forward;
                if (MySector.MainCamera != null)
                {
                    up = MySector.MainCamera.UpVector;
                    forward = -MySector.MainCamera.ForwardVector;
                }
                Vector3 zero = Vector3.Zero;
                this.GetListenerVelocity(ref zero);
                MyAudio.Static.Update(0x10, Vector3.Zero, up, forward, zero);
                if ((MyMusicController.Static != null) && MyMusicController.Static.Active)
                {
                    MyMusicController.Static.Update();
                }
                if (Config.EnableMuteWhenNotInFocus)
                {
                    if (Form.ActiveForm == null)
                    {
                        if (this.hasFocus)
                        {
                            MyAudio.Static.VolumeMusic = 0f;
                            MyAudio.Static.VolumeGame = 0f;
                            MyAudio.Static.VolumeHud = 0f;
                            MyAudio.Static.VolumeVoiceChat = 0f;
                            this.hasFocus = false;
                        }
                    }
                    else if (!this.hasFocus)
                    {
                        MyAudio.Static.VolumeMusic = Config.MusicVolume;
                        MyAudio.Static.VolumeGame = Config.GameVolume;
                        MyAudio.Static.VolumeHud = Config.GameVolume;
                        MyAudio.Static.VolumeVoiceChat = Config.VoiceChatVolume;
                        this.hasFocus = true;
                    }
                }
            }
            using (List<IPlugin>.Enumerator enumerator = MyPlugins.Plugins.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
            base.Update();
            MyGameStats.Static.Update();
            if (((MySpaceAnalytics.Instance != null) && (MySession.Static != null)) && MySession.Static.Ready)
            {
                MySpaceAnalytics.Instance.Update(base.TotalTime);
            }
            if (this.m_unpauseInput && ((DateTime.Now - Static.m_inputPauseTime).TotalMilliseconds >= 10.0))
            {
                Static.m_unpauseInput = false;
                Static.PauseInput = false;
            }
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.ReplicationLayer != null)
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.ReplicationLayer.AdvanceSyncTime();
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated && (MySession.Static != null))
            {
                this.CheckAutoUpdateForDedicatedServer();
                this.CheckAutoRestartForDedicatedServer();
            }
            if (MyFakes.ENABLE_TESTING_TOOL_HEPER)
            {
                MyTestingToolHelper.Instance.Update();
            }
            MyStatsGraph.Commit();
        }

        private void UpdateDamageEffectsInScene()
        {
            foreach (MyCubeGrid grid in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
                if (grid != null)
                {
                    foreach (MySlimBlock block in grid.GetBlocks())
                    {
                        if (this.m_enableDamageEffects)
                        {
                            block.ResumeDamageEffect();
                            continue;
                        }
                        if (block.FatBlock != null)
                        {
                            block.FatBlock.StopDamageEffect(false);
                        }
                    }
                }
            }
        }

        internal void UpdateMouseCapture()
        {
            MyRenderProxy.UpdateMouseCapture(Config.CaptureMouse && (Config.WindowMode != MyWindowModeEnum.Fullscreen));
        }

        private static void UpdatePauseState(int pauseStackCount)
        {
            if (pauseStackCount > 0)
            {
                IsPaused = true;
            }
            else
            {
                IsPaused = false;
            }
        }

        public static void UpdateScreenSize(int width, int height, MyViewport viewport)
        {
            ScreenSize = new Vector2I(width, height);
            ScreenSizeHalf = new Vector2I(ScreenSize.X / 2, ScreenSize.Y / 2);
            ScreenViewport = viewport;
            if (MyGuiManager.UpdateScreenSize(ScreenSize, ScreenSizeHalf, MyVideoSettingsManager.IsTripleHead(ScreenSize)))
            {
                MyScreenManager.RecreateControls();
            }
            if (MySector.MainCamera != null)
            {
                MySector.MainCamera.UpdateScreenSize(ScreenViewport);
            }
            CanShowHotfixPopup = true;
            CanShowWhitelistPopup = true;
        }

        public static void WriteConsoleOutputs()
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated && IsAutoRestarting)
            {
                MyLog.Default.WriteLineAndConsole("AUTORESTART");
            }
        }

        private void WriteHavokCodeToLog()
        {
            Log.WriteLine("HkGameName: " + HkBaseSystem.GameName);
            foreach (string str in HkBaseSystem.GetKeyCodes())
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    Log.WriteLine("HkCode: " + str);
                }
            }
        }

        public static bool IsDirectX11 =>
            (MyVideoSettingsManager.RunningGraphicsRenderer == DirectX11RendererKey);

        public static bool IsGameReady
        {
            get
            {
                if (!IsUpdateReady)
                {
                    return false;
                }
                if (!AreClipmapsReady || !RenderTasksFinished)
                {
                    return (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null);
                }
                return true;
            }
        }

        public static bool IsPreloading
        {
            get => 
                (Interlocked.CompareExchange(ref m_isPreloading, 0, 0) != 0);
            private set => 
                Interlocked.Exchange(ref m_isPreloading, value ? 1 : 0);
        }

        public static bool AreClipmapsReady
        {
            get => 
                (m_areClipmapsReady || !MyFakes.ENABLE_WAIT_UNTIL_CLIPMAPS_READY);
            set
            {
                m_areClipmapsReady = !m_reconfirmClipmaps & value;
                if ((MySession.Static != null) && (!value || m_reconfirmClipmaps))
                {
                    using (Dictionary<long, MyVoxelBase>.ValueCollection.Enumerator enumerator = MySession.Static.VoxelMaps.Instances.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyRenderComponentVoxelMap render = enumerator.Current.Render as MyRenderComponentVoxelMap;
                            if (render != null)
                            {
                                render.ResetLoading();
                            }
                        }
                    }
                }
                m_reconfirmClipmaps = !value;
            }
        }

        public static bool RenderTasksFinished
        {
            get => 
                true;
            set => 
                (m_renderTasksFinished = value);
        }

        public static EnumAutorestartStage AutoRestartState =>
            m_autoRestartState;

        public static bool IsAutoRestarting =>
            (m_autoRestartState == EnumAutorestartStage.Restarting);

        public bool IsGoingToUpdate =>
            this.m_isGoingToUpdate;

        public bool IsRestartingForUpdate =>
            (this.IsGoingToUpdate && IsAutoRestarting);

        public static int TotalGamePlayTimeInMilliseconds =>
            ((IsPaused ? m_pauseStartTimeInMilliseconds : TotalSimulationTimeInMilliseconds) - m_totalPauseTimeInMilliseconds);

        public static int TotalTimeInMilliseconds =>
            ((int) Static.TotalTime.Milliseconds);

        public static int TotalSimulationTimeInMilliseconds =>
            ((int) Static.SimulationTimeWithSpeed.Milliseconds);

        public static double SecondsSinceLastFrame
        {
            [CompilerGenerated]
            get => 
                <SecondsSinceLastFrame>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<SecondsSinceLastFrame>k__BackingField = value);
        }

        public IntPtr WindowHandle { get; protected set; }

        public bool EnableDamageEffects
        {
            get => 
                this.m_enableDamageEffects;
            set
            {
                this.m_enableDamageEffects = value;
                this.UpdateDamageEffectsInScene();
            }
        }

        public static IGameCustomInitialization GameCustomInitialization
        {
            [CompilerGenerated]
            get => 
                <GameCustomInitialization>k__BackingField;
            [CompilerGenerated]
            set => 
                (<GameCustomInitialization>k__BackingField = value);
        }

        public bool IsCursorVisible { get; private set; }

        public bool PauseInput { get; private set; }

        public static bool IsExitForced
        {
            [CompilerGenerated]
            get => 
                <IsExitForced>k__BackingField;
            [CompilerGenerated]
            set => 
                (<IsExitForced>k__BackingField = value);
        }

        public static bool IsPaused
        {
            get => 
                m_isPaused;
            set
            {
                if (Sync.MultiplayerActive && Sync.IsServer)
                {
                    if (m_isPaused)
                    {
                        MyAudio.Static.ResumeGameSounds();
                    }
                    m_isPaused = false;
                }
                else if (m_isPaused != value)
                {
                    m_isPaused = value;
                    if (IsPaused)
                    {
                        m_pauseStartTimeInMilliseconds = TotalSimulationTimeInMilliseconds;
                    }
                    else
                    {
                        m_totalPauseTimeInMilliseconds += TotalSimulationTimeInMilliseconds - m_pauseStartTimeInMilliseconds;
                    }
                }
                if (!m_isPaused)
                {
                    m_pauseStackCount = 0;
                }
                MyParticlesManager.Paused = m_isPaused;
            }
        }

        public static IErrorConsumer ErrorConsumer
        {
            get => 
                m_errorConsumer;
            set => 
                (m_errorConsumer = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySandboxGame.<>c <>9 = new MySandboxGame.<>c();
            public static Action <>9__117_0;
            public static Func<System.Type, bool> <>9__145_0;
            public static Func<MyProfiler.TaskType, MyProfiler.TaskType> <>9__147_1;
            public static Func<MyProfiler.TaskType, string> <>9__147_2;
            public static Action <>9__147_4;
            public static Action <>9__147_0;
            public static Func<MethodInfo, bool> <>9__163_1;
            public static Func<MethodInfo, bool> <>9__163_0;

            internal void <.ctor>b__117_0()
            {
                MyDefinitionManager.Static.LoadScenarios();
            }

            internal bool <InitIlChecker>b__163_0(MethodInfo method) => 
                ((method.Name == "TryGet") && (method.ContainsGenericParameters && (method.GetParameters().Length == 1)));

            internal bool <InitIlChecker>b__163_1(MethodInfo method) => 
                ((method.Name == "TryGet") && (method.ContainsGenericParameters && (method.GetParameters().Length == 1)));

            internal void <LoadData>b__147_0()
            {
                HkBaseSystem.InitThread(Thread.CurrentThread.Name);
            }

            internal MyProfiler.TaskType <LoadData>b__147_1(MyProfiler.TaskType x) => 
                x;

            internal string <LoadData>b__147_2(MyProfiler.TaskType x) => 
                x.ToString();

            internal void <LoadData>b__147_4()
            {
            }

            internal bool <PreloadTypesFrom>b__145_0(System.Type type) => 
                Attribute.IsDefined(type, typeof(PreloadRequiredAttribute));
        }

        public interface IGameCustomInitialization
        {
            void InitIlChecker();
            void InitIlCompiler();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyInvokeData
        {
            public System.Action Action;
            public object Context;
            public Action<object> ContextualAction;
            public string Invoker;
        }
    }
}

