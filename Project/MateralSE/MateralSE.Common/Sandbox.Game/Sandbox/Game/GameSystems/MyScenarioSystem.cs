namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0x3e8)]
    public class MyScenarioSystem : MySessionComponentBase
    {
        public static int LoadTimeout = 120;
        public static MyScenarioSystem Static;
        private readonly HashSet<ulong> m_playersReadyForBattle = new HashSet<ulong>();
        private TimeSpan m_startBattlePreparationOnClients = TimeSpan.FromSeconds(0.0);
        [CompilerGenerated]
        private Action EndAction;
        private static string m_newPath;
        private static MyWorkshopItem m_newWorkshopMap;
        private static CheckpointData? m_checkpointData;
        private MyState m_gameState;
        private TimeSpan m_stateChangePlayTime;
        private TimeSpan m_startBattleTime = TimeSpan.FromSeconds(0.0);
        private StringBuilder m_tmpStringBuilder = new StringBuilder();
        private MyGuiScreenScenarioWaitForPlayers m_waitingScreen;
        private TimeSpan? m_battleTimeLimit;
        private int m_bootUpCount;

        private event Action EndAction
        {
            [CompilerGenerated] add
            {
                Action endAction = this.EndAction;
                while (true)
                {
                    Action a = endAction;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    endAction = Interlocked.CompareExchange<Action>(ref this.EndAction, action3, a);
                    if (ReferenceEquals(endAction, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action endAction = this.EndAction;
                while (true)
                {
                    Action source = endAction;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    endAction = Interlocked.CompareExchange<Action>(ref this.EndAction, action3, source);
                    if (ReferenceEquals(endAction, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyScenarioSystem()
        {
            Static = this;
            this.ServerStartGameTime = DateTime.MaxValue;
        }

        private bool AllPlayersReadyForBattle()
        {
            using (IEnumerator<MyPlayer.PlayerId> enumerator = Sync.Players.GetAllPlayers().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPlayer.PlayerId current = enumerator.Current;
                    if (!this.m_playersReadyForBattle.Contains(current.SteamId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void CheckDx11AndLoad(string sessionPath, bool multiplayer, MyOnlineModeEnum onlineMode, short maxPlayers, MyGameModeEnum gameMode, MyObjectBuilder_Checkpoint checkpoint, ulong checkpointSizeInBytes)
        {
            if ((checkpoint.RequiresDX < 11) || MySandboxGame.IsDirectX11)
            {
                LoadMission(sessionPath, multiplayer, onlineMode, maxPlayers, gameMode, checkpoint, checkpointSizeInBytes);
            }
            else
            {
                MyJoinGameHelper.HandleDx11Needed();
            }
        }

        private static void EndActionLoadLocal()
        {
            Static.EndAction -= new Action(MyScenarioSystem.EndActionLoadLocal);
            LoadMission(m_newPath, false, MyOnlineModeEnum.OFFLINE, 1, MyGameModeEnum.Survival);
        }

        private static void EndActionLoadWorkshop()
        {
            Static.EndAction -= new Action(MyScenarioSystem.EndActionLoadWorkshop);
            MyWorkshop.CreateWorldInstanceAsync(m_newWorkshopMap, MyWorkshop.MyWorkshopPathInfo.CreateScenarioInfo(), true, delegate (bool success, string sessionPath) {
                if (success)
                {
                    m_newPath = sessionPath;
                    LoadMission(sessionPath, false, MyOnlineModeEnum.OFFLINE, 1, MyGameModeEnum.Survival);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextWorkshopDownloadFailed), MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            });
        }

        internal static MyOnlineModeEnum GetOnlineModeFromCurrentLobbyType()
        {
            MyMultiplayerLobby @static = Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyMultiplayerLobby;
            if (@static != null)
            {
                switch (@static.GetLobbyType())
                {
                    case MyLobbyType.Private:
                        return MyOnlineModeEnum.PRIVATE;

                    case MyLobbyType.FriendsOnly:
                        return MyOnlineModeEnum.FRIENDS;

                    case MyLobbyType.Public:
                        return MyOnlineModeEnum.PUBLIC;
                }
            }
            return MyOnlineModeEnum.PRIVATE;
        }

        public override void LoadData()
        {
            base.LoadData();
            MySyncScenario.PlayerReadyToStartScenario += new Action<ulong>(this.MySyncScenario_PlayerReadyToStart);
            MySyncScenario.StartScenario += new Action<long>(this.MySyncScenario_StartScenario);
            MySyncScenario.ClientWorldLoaded += new Action(this.MySyncScenario_ClientWorldLoaded);
            MySyncScenario.PrepareScenario += new Action<long>(this.MySyncBattleGame_PrepareScenario);
        }

        private static void LoadMission(CheckpointData data)
        {
            MyObjectBuilder_Checkpoint checkpoint = data.Checkpoint;
            MyWorkshop.DownloadModsAsync(checkpoint.Mods, delegate (bool success) {
                if (!success && ((checkpoint.Settings.OnlineMode != MyOnlineModeEnum.OFFLINE) || !MyWorkshop.CanRunOffline(checkpoint.Mods)))
                {
                    MyLog.Default.WriteLine(MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed).ToString());
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (MyFakes.QUICK_LAUNCH != null)
                        {
                            MySessionLoader.UnloadAndExitToMenu();
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                else
                {
                    MyScreenManager.CloseAllScreensNowExcept(null);
                    MyGuiSandbox.Update(0x10);
                    if (MySession.Static != null)
                    {
                        MySession.Static.Unload();
                        MySession.Static = null;
                    }
                    if (checkpoint.Settings.ProceduralSeed == 0)
                    {
                        checkpoint.Settings.ProceduralSeed = MyRandom.Instance.Next();
                    }
                    MySessionLoader.StartLoading(delegate {
                        checkpoint.Settings.Scenario = true;
                        MySession.LoadMission(data.SessionPath, checkpoint, data.CheckpointSize, data.PersistentEditMode);
                    }, null, null, null);
                }
                MyLog.Default.WriteLine("LoadSession() - End");
            }, null);
        }

        public static void LoadMission(string sessionPath, bool multiplayer, MyOnlineModeEnum onlineMode, short maxPlayers, MyGameModeEnum gameMode = 1)
        {
            ulong num;
            MyLog.Default.WriteLine("LoadSession() - Start");
            MyLog.Default.WriteLine(sessionPath);
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out num);
            CheckDx11AndLoad(sessionPath, multiplayer, onlineMode, maxPlayers, gameMode, checkpoint, num);
        }

        public static void LoadMission(string sessionPath, bool multiplayer, MyOnlineModeEnum onlineMode, short maxPlayers, MyGameModeEnum gameMode, MyObjectBuilder_Checkpoint checkpoint, ulong checkpointSizeInBytes)
        {
            StringBuilder builder;
            MyStringId? nullable;
            Vector2? nullable2;
            bool scenarioEditMode = checkpoint.Settings.ScenarioEditMode;
            checkpoint.Settings.OnlineMode = onlineMode;
            checkpoint.Settings.MaxPlayers = maxPlayers;
            checkpoint.Settings.Scenario = true;
            checkpoint.Settings.GameMode = gameMode;
            checkpoint.Settings.ScenarioEditMode = false;
            if (!MySession.IsCompatibleVersion(checkpoint))
            {
                MyLog.Default.WriteLine(MyTexts.Get(MyCommonTexts.DialogTextIncompatibleWorldVersion).ToString());
                builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextIncompatibleWorldVersion), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                MyLog.Default.WriteLine("LoadSession() - End");
            }
            else if (!MyWorkshop.CheckLocalModsAllowed(checkpoint.Mods, checkpoint.Settings.OnlineMode == MyOnlineModeEnum.OFFLINE))
            {
                MyLog.Default.WriteLine(MyTexts.Get(MyCommonTexts.DialogTextLocalModsDisabledInMultiplayer).ToString());
                builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextLocalModsDisabledInMultiplayer), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                MyLog.Default.WriteLine("LoadSession() - End");
            }
            else
            {
                CheckpointData data = new CheckpointData {
                    Checkpoint = checkpoint,
                    CheckpointSize = checkpointSizeInBytes,
                    PersistentEditMode = scenarioEditMode,
                    SessionPath = sessionPath
                };
                m_checkpointData = new CheckpointData?(data);
                if (((checkpoint.BriefingVideo == null) || (checkpoint.BriefingVideo.Length <= 0)) || MyFakes.XBOX_PREVIEW)
                {
                    m_checkpointData = null;
                    LoadMission(m_checkpointData.Value);
                }
                else
                {
                    builder = MyTexts.Get(MySpaceTexts.MessageBoxCaptionVideo);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.MessageBoxTextWatchVideo), builder, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(MyScenarioSystem.OnVideoMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
        }

        public static void LoadNextScenario(string id)
        {
            if (MySession.Static.OnlineMode == MyOnlineModeEnum.OFFLINE)
            {
                ulong num;
                MyStringId? nullable;
                Vector2? nullable2;
                MyAPIGateway.Utilities.ShowNotification(MyTexts.GetString(MySpaceTexts.NotificationNextScenarioWillLoad), 0x2710, "White");
                if (!ulong.TryParse(id, out num))
                {
                    string path = Path.Combine(MyFileSystem.ContentPath, "Missions", id);
                    if (Directory.Exists(path))
                    {
                        m_newPath = path;
                        Static.EndAction += new Action(MyScenarioSystem.EndActionLoadLocal);
                    }
                    else
                    {
                        string str2 = Path.Combine(MyFileSystem.SavesPath, id);
                        if (Directory.Exists(str2))
                        {
                            m_newPath = str2;
                            Static.EndAction += new Action(MyScenarioSystem.EndActionLoadLocal);
                        }
                        else
                        {
                            StringBuilder messageText = new StringBuilder();
                            messageText.AppendFormat(MyTexts.GetString(MySpaceTexts.MessageBoxTextScenarioNotFound), path, str2);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    }
                }
                else if (!MyGameService.IsOnline)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextWorkshopDownloadFailed), MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    MySandboxGame.Log.WriteLine(string.Format("Querying details of file " + num, Array.Empty<object>()));
                    List<ulong> publishedFileIds = new List<ulong>();
                    publishedFileIds.Add(num);
                    List<MyWorkshopItem> modsInfo = MyWorkshop.GetModsInfo(publishedFileIds);
                    if ((modsInfo != null) && (modsInfo.Count > 0))
                    {
                        m_newWorkshopMap = modsInfo[0];
                        Static.EndAction += new Action(MyScenarioSystem.EndActionLoadWorkshop);
                    }
                    else
                    {
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextWorkshopDownloadFailed), MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
        }

        private void MySyncBattleGame_PrepareScenario(long preparationStartTime)
        {
            this.ServerPreparationStartTime = new DateTime(preparationStartTime);
        }

        private void MySyncScenario_ClientWorldLoaded()
        {
            MySyncScenario.ClientWorldLoaded -= new Action(this.MySyncScenario_ClientWorldLoaded);
            this.m_waitingScreen = new MyGuiScreenScenarioWaitForPlayers();
            MyGuiSandbox.AddScreen(this.m_waitingScreen);
        }

        private void MySyncScenario_PlayerReadyToStart(ulong steamId)
        {
            if (this.GameState == MyState.WaitingForClients)
            {
                this.m_playersReadyForBattle.Add(steamId);
                if (!this.AllPlayersReadyForBattle())
                {
                    return;
                }
                else
                {
                    this.StartScenario();
                    foreach (ulong num in this.m_playersReadyForBattle)
                    {
                        if (num != Sync.MyId)
                        {
                            MySyncScenario.StartScenarioRequest(num, this.ServerStartGameTime.Ticks);
                        }
                    }
                    return;
                }
            }
            if (this.GameState == MyState.Running)
            {
                MySyncScenario.StartScenarioRequest(steamId, this.ServerStartGameTime.Ticks);
            }
        }

        private void MySyncScenario_StartScenario(long serverStartGameTime)
        {
            this.ServerStartGameTime = new DateTime(serverStartGameTime);
            this.StartScenario();
        }

        private static void OnVideoMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGuiSandbox.OpenUrlWithFallback(m_checkpointData.Value.Checkpoint.BriefingVideo, "Scenario briefing video", true);
            }
            m_checkpointData = null;
            LoadMission(m_checkpointData.Value);
        }

        internal void PrepareForStart()
        {
            this.GameState = MyState.WaitingForClients;
            this.m_startBattlePreparationOnClients = MySession.Static.ElapsedPlayTime;
            if (GetOnlineModeFromCurrentLobbyType() == MyOnlineModeEnum.OFFLINE)
            {
                this.StartScenario();
            }
            else
            {
                this.m_waitingScreen = new MyGuiScreenScenarioWaitForPlayers();
                MyGuiSandbox.AddScreen(this.m_waitingScreen);
                this.ServerPreparationStartTime = DateTime.UtcNow;
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ScenarioStartTime = this.ServerPreparationStartTime;
                MySyncScenario.PrepareScenarioFromLobby(this.ServerPreparationStartTime.Ticks);
            }
        }

        internal static void SetLobbyTypeFromOnlineMode(MyOnlineModeEnum onlineMode)
        {
            MyMultiplayerLobby @static = Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyMultiplayerLobby;
            if (@static != null)
            {
                MyLobbyType @private = MyLobbyType.Private;
                if (onlineMode == MyOnlineModeEnum.PUBLIC)
                {
                    @private = MyLobbyType.Public;
                }
                else if (onlineMode == MyOnlineModeEnum.FRIENDS)
                {
                    @private = MyLobbyType.FriendsOnly;
                }
                @static.SetLobbyType(@private);
            }
        }

        private void StartScenario()
        {
            if (Sync.IsServer)
            {
                this.ServerStartGameTime = DateTime.UtcNow;
            }
            if (this.m_waitingScreen != null)
            {
                MyGuiSandbox.RemoveScreen(this.m_waitingScreen);
                this.m_waitingScreen = null;
            }
            this.GameState = MyState.Running;
            this.m_startBattleTime = MySession.Static.ElapsedPlayTime;
            if ((MySession.Static.LocalHumanPlayer == null) || (MySession.Static.LocalHumanPlayer.Character == null))
            {
                MyPlayerCollection.RequestLocalRespawn();
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MySyncScenario.PlayerReadyToStartScenario -= new Action<ulong>(this.MySyncScenario_PlayerReadyToStart);
            MySyncScenario.StartScenario -= new Action<long>(this.MySyncScenario_StartScenario);
            MySyncScenario.ClientWorldLoaded -= new Action(this.MySyncScenario_ClientWorldLoaded);
            MySyncScenario.PrepareScenario -= new Action<long>(this.MySyncBattleGame_PrepareScenario);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (MySession.Static.IsScenario || MySession.Static.Settings.ScenarioEditMode)
            {
                if (!Sync.IsServer)
                {
                    return;
                }
                if ((MySession.Static.OnlineMode == MyOnlineModeEnum.OFFLINE) && (this.GameState < MyState.Running))
                {
                    if (this.GameState == MyState.Loaded)
                    {
                        this.GameState = MyState.Running;
                        this.ServerStartGameTime = DateTime.UtcNow;
                    }
                    return;
                }
                switch (this.GameState)
                {
                    case MyState.Loaded:
                        if ((MySession.Static.OnlineMode != MyOnlineModeEnum.OFFLINE) && (Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null))
                        {
                            if (MyFakes.XBOX_PREVIEW)
                            {
                                this.GameState = MyState.Running;
                                return;
                            }
                            this.m_bootUpCount++;
                            if (this.m_bootUpCount > 100)
                            {
                                MyPlayerCollection.RequestLocalRespawn();
                                this.GameState = MyState.Running;
                            }
                            return;
                        }
                        if (Sandbox.Engine.Platform.Game.IsDedicated)
                        {
                            goto TR_000E;
                        }
                        else if (!MySession.Static.Settings.ScenarioEditMode)
                        {
                            if ((MySession.Static.OnlineMode == MyOnlineModeEnum.OFFLINE) || (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null))
                            {
                                if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
                                {
                                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.Scenario = true;
                                    Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ScenarioBriefing = MySession.Static.GetWorld(true).Checkpoint.Briefing;
                                }
                                MyGuiScreenScenarioMpServer screen = new MyGuiScreenScenarioMpServer();
                                screen.Briefing = MySession.Static.GetWorld(true).Checkpoint.Briefing;
                                MyGuiSandbox.AddScreen(screen);
                                this.m_playersReadyForBattle.Add(Sync.MyId);
                                this.GameState = MyState.JoinScreen;
                                return;
                            }
                        }
                        else
                        {
                            goto TR_000E;
                        }
                        return;

                    case MyState.JoinScreen:
                    case MyState.Running:
                        return;

                    case MyState.WaitingForClients:
                    {
                        TimeSpan elapsedPlayTime = MySession.Static.ElapsedPlayTime;
                        if (this.AllPlayersReadyForBattle() || ((LoadTimeout > 0) && ((elapsedPlayTime - this.m_startBattlePreparationOnClients) > TimeSpan.FromSeconds((double) LoadTimeout))))
                        {
                            this.StartScenario();
                            foreach (ulong num in this.m_playersReadyForBattle)
                            {
                                if (num != Sync.MyId)
                                {
                                    MySyncScenario.StartScenarioRequest(num, this.ServerStartGameTime.Ticks);
                                }
                            }
                        }
                        return;
                    }
                    case MyState.Ending:
                        break;

                    default:
                        return;
                }
                if ((this.EndAction != null) && ((MySession.Static.ElapsedPlayTime - this.m_stateChangePlayTime) > TimeSpan.FromSeconds(10.0)))
                {
                    this.EndAction();
                }
            }
            return;
        TR_000E:
            this.ServerPreparationStartTime = DateTime.UtcNow;
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ScenarioStartTime = this.ServerPreparationStartTime;
            this.GameState = MyState.Running;
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.StartScenario();
            }
        }

        public MyState GameState
        {
            get => 
                this.m_gameState;
            set
            {
                if (this.m_gameState != value)
                {
                    this.m_gameState = value;
                    this.m_stateChangePlayTime = MySession.Static.ElapsedPlayTime;
                }
            }
        }

        public DateTime ServerPreparationStartTime { get; private set; }

        public DateTime ServerStartGameTime { get; private set; }

        private bool OnlinePrivateMode =>
            (MySession.Static.OnlineMode == MyOnlineModeEnum.PRIVATE);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScenarioSystem.<>c <>9 = new MyScenarioSystem.<>c();
            public static Action<bool, string> <>9__47_0;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__52_2;

            internal void <EndActionLoadWorkshop>b__47_0(bool success, string sessionPath)
            {
                if (success)
                {
                    MyScenarioSystem.m_newPath = sessionPath;
                    MyScenarioSystem.LoadMission(sessionPath, false, MyOnlineModeEnum.OFFLINE, 1, MyGameModeEnum.Survival);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextWorkshopDownloadFailed), MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }

            internal void <LoadMission>b__52_2(MyGuiScreenMessageBox.ResultEnum result)
            {
                if (MyFakes.QUICK_LAUNCH != null)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CheckpointData
        {
            public MyObjectBuilder_Checkpoint Checkpoint;
            public string SessionPath;
            public ulong CheckpointSize;
            public bool PersistentEditMode;
        }

        public enum MyState
        {
            Loaded,
            JoinScreen,
            WaitingForClients,
            Running,
            Ending
        }
    }
}

