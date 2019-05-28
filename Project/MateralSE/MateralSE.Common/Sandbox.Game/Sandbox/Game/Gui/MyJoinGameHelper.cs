namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Utils;

    public static class MyJoinGameHelper
    {
        private static void CheckDx11AndJoin(MyObjectBuilder_World world, MyMultiplayerBase multiplayer)
        {
            if ((world.Checkpoint.RequiresDX >= 11) && !MySandboxGame.IsDirectX11)
            {
                HandleDx11Needed();
            }
            else if (multiplayer.Scenario)
            {
                MySessionLoader.LoadMultiplayerScenarioWorld(world, multiplayer);
            }
            else
            {
                MySessionLoader.LoadMultiplayerSession(world, multiplayer);
            }
        }

        private static void DownloadWorld(MyGuiScreenProgress progress, MyMultiplayerBase multiplayer)
        {
            if (progress.Text != null)
            {
                progress.Text.Clear();
                progress.Text.Append(MyTexts.Get(MyCommonTexts.MultiplayerStateConnectingToServer));
            }
            MyLog.Default.WriteLine("World requested");
            Stopwatch worldRequestTime = Stopwatch.StartNew();
            ulong serverId = multiplayer.GetOwner();
            bool connected = false;
            progress.Tick += delegate {
                MyStringId id;
                MyP2PSessionState state = new MyP2PSessionState();
                MyGameService.Peer2Peer.GetSessionState(multiplayer.ServerId, ref state);
                if (!connected && state.ConnectionActive)
                {
                    MyLog.Default.WriteLine("World requested - connection alive");
                    connected = true;
                    if (progress.Text != null)
                    {
                        progress.Text.Clear();
                        progress.Text.Append(MyTexts.Get(MyCommonTexts.MultiplayerStateWaitingForServer));
                    }
                }
                if (serverId != multiplayer.GetOwner())
                {
                    MyLog.Default.WriteLine("World requested - failed, server changed");
                    progress.Cancel();
                    id = new MyStringId();
                    MyGuiSandbox.Show(MyCommonTexts.MultiplayerErrorServerHasLeft, id, MyMessageBoxStyleEnum.Error);
                    multiplayer.Dispose();
                }
                bool flag = MyScreenManager.IsScreenOnTop(progress);
                if (flag && !worldRequestTime.IsRunning)
                {
                    worldRequestTime.Start();
                }
                else if (!flag && worldRequestTime.IsRunning)
                {
                    worldRequestTime.Stop();
                }
                if (worldRequestTime.IsRunning && (worldRequestTime.Elapsed.TotalSeconds > 40.0))
                {
                    MyLog.Default.WriteLine("World requested - failed, server changed");
                    progress.Cancel();
                    id = new MyStringId();
                    MyGuiSandbox.Show(MyCommonTexts.MultiplaterJoin_ServerIsNotResponding, id, MyMessageBoxStyleEnum.Error);
                    multiplayer.Dispose();
                }
            };
            multiplayer.DownloadWorld();
        }

        public static void HandleDx11Needed()
        {
            MyStringId? nullable;
            Vector2? nullable2;
            MySessionLoader.UnloadAndExitToMenu();
            if (!MyDirectXHelper.IsDx11Supported())
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartNoDx9SelectDifferent), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.QuickstartDX11SwitchQuestion), messageCaption, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(MyJoinGameHelper.OnDX11SwitchRequestAnswer), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
        }

        public static void JoinGame(ulong lobbyId)
        {
            MyGuiScreenProgress progress = new MyGuiScreenProgress(MyTexts.Get(MyCommonTexts.DialogTextJoiningWorld), new MyStringId?(MyCommonTexts.Cancel), true, true);
            MyGuiSandbox.AddScreen(progress);
            progress.ProgressCancelled += () => MySessionLoader.UnloadAndExitToMenu();
            MyLog.Default.WriteLine("Joining lobby: " + lobbyId);
            MyMultiplayerJoinResult result = MyMultiplayer.JoinLobby(lobbyId);
            result.JoinDone += (success, lobby, response, multiplayer) => OnJoin(progress, success, lobby, response, multiplayer);
            progress.ProgressCancelled += () => result.Cancel();
        }

        public static void JoinGame(IMyLobby lobby, bool requestData = true)
        {
            if (MySession.Static != null)
            {
                MySession.Static.Unload();
                MySession.Static = null;
            }
            if (requestData && string.IsNullOrEmpty(lobby.GetData("appVersion")))
            {
                MyLobbyHelper helper1 = new MyLobbyHelper(lobby);
                MyLobbyHelper helper2 = new MyLobbyHelper(lobby);
                helper2.OnSuccess += l => JoinGame(l, false);
                if (helper2.RequestData())
                {
                    return;
                }
            }
            if (JoinGameTest(lobby))
            {
                JoinGame(lobby.LobbyId);
            }
        }

        public static void JoinGame(MyGameServerItem server, bool enableGuiBackgroundFade = true)
        {
            if (MySession.Static != null)
            {
                MySession.Static.Unload();
                MySession.Static = null;
            }
            MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Join);
            if (server.ServerVersion != MyFinalBuildConstants.APP_VERSION)
            {
                StringBuilder text = new StringBuilder();
                text.AppendFormat(MyTexts.GetString(MyCommonTexts.MultiplayerError_IncorrectVersion), MyFinalBuildConstants.APP_VERSION, server.ServerVersion);
                MyGuiSandbox.Show(text, MyCommonTexts.MessageBoxCaptionError, MyMessageBoxStyleEnum.Error);
            }
            else
            {
                if (MyFakes.ENABLE_MP_DATA_HASHES)
                {
                    string gameTagByPrefix = server.GetGameTagByPrefix("datahash");
                    if ((gameTagByPrefix != "") && (gameTagByPrefix != MyDataIntegrityChecker.GetHashBase64()))
                    {
                        MyStringId caption = new MyStringId();
                        MyGuiSandbox.Show(MyCommonTexts.MultiplayerError_DifferentData, caption, MyMessageBoxStyleEnum.Error);
                        MySandboxGame.Log.WriteLine("Different game data when connecting to server. Local hash: " + MyDataIntegrityChecker.GetHashBase64() + ", server hash: " + gameTagByPrefix);
                        return;
                    }
                }
                MyGameService.AddHistoryGame(server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.NetAdr.Port, (ushort) server.NetAdr.Port);
                MyMultiplayerClient multiplayer = new MyMultiplayerClient(server, new MySyncLayer(new MyTransportLayer(2))) {
                    ExperimentalMode = MySandboxGame.Config.ExperimentalMode
                };
                MyMultiplayer.Static = multiplayer;
                multiplayer.SendPlayerData(MyGameService.UserName);
                server.GetGameTagByPrefix("gamemode");
                MyGuiScreenProgress progress = new MyGuiScreenProgress(MyTexts.Get(MyCommonTexts.DialogTextJoiningWorld), new MyStringId?(MyCommonTexts.Cancel), false, enableGuiBackgroundFade);
                MyGuiSandbox.AddScreen(progress);
                progress.ProgressCancelled += delegate {
                    multiplayer.Dispose();
                    MySessionLoader.UnloadAndExitToMenu();
                    if (MyMultiplayer.Static != null)
                    {
                        MyMultiplayer.Static.Dispose();
                    }
                };
                multiplayer.OnJoin = (Action) Delegate.Combine(multiplayer.OnJoin, () => OnJoin(progress, true, null, MyLobbyEnterResponseEnum.Success, multiplayer));
                Action<string> onProfilerCommandExecuted = delegate (string desc) {
                    MyHudNotification notification = new MyHudNotification(MyStringId.GetOrCompute(desc), 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    MyHud.Notifications.Add(notification);
                    MyLog.Default.WriteLine(desc);
                };
                VRage.Profiler.MyRenderProfiler.GetProfilerFromServer = delegate {
                    onProfilerCommandExecuted("Command executed: Download profiler");
                    MyMultiplayer.Static.ProfilerDone = onProfilerCommandExecuted;
                    MyMultiplayer.Static.DownloadProfiler();
                };
                Sandbox.MyRenderProfiler.ServerInvoke = delegate (RenderProfilerCommand cmd, int payload) {
                    onProfilerCommandExecuted("Command executed: " + cmd.ToString());
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<RenderProfilerCommand, int>(_ => new Action<RenderProfilerCommand, int>(Sandbox.MyRenderProfiler.OnCommandReceived), cmd, payload, targetEndpoint, position);
                };
            }
        }

        private static bool JoinGameTest(IMyLobby lobby)
        {
            if (lobby.IsValid)
            {
                MyStringId id;
                if (!MyMultiplayerLobby.IsLobbyCorrectVersion(lobby))
                {
                    id = new MyStringId();
                    MyGuiSandbox.Show(new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MultiplayerError_IncorrectVersion), MyBuildNumbers.ConvertBuildNumberFromIntToString((int) MyFinalBuildConstants.APP_VERSION), MyBuildNumbers.ConvertBuildNumberFromIntToString(MyMultiplayerLobby.GetLobbyAppVersion(lobby)))), id, MyMessageBoxStyleEnum.Error);
                    return false;
                }
                if (!MyFakes.ENABLE_MP_DATA_HASHES || MyMultiplayerLobby.HasSameData(lobby))
                {
                    return true;
                }
                id = new MyStringId();
                MyGuiSandbox.Show(MyCommonTexts.MultiplayerError_DifferentData, id, MyMessageBoxStyleEnum.Error);
                MySandboxGame.Log.WriteLine("Different game data when connecting to server. Local hash: " + MyDataIntegrityChecker.GetHashBase64() + ", server hash: " + MyMultiplayerLobby.GetDataHash(lobby));
            }
            return false;
        }

        public static void OnDX11SwitchRequestAnswer(MyGuiScreenMessageBox.ResultEnum result)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            if (result != MyGuiScreenMessageBox.ResultEnum.YES)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartSelectDifferent), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            else
            {
                MySandboxGame.Config.GraphicsRenderer = MySandboxGame.DirectX11RendererKey;
                MySandboxGame.Config.Save();
                MyGuiSandbox.BackToMainMenu();
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartDX11PleaseRestartGame), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
        }

        public static void OnJoin(MyGuiScreenProgress progress, bool success, IMyLobby lobby, MyLobbyEnterResponseEnum response, MyMultiplayerBase multiplayer)
        {
            MyLog.Default.WriteLine($"Lobby join response: {success}, enter state: {response}");
            if ((!success || (response != MyLobbyEnterResponseEnum.Success)) || (multiplayer.GetOwner() == Sync.MyId))
            {
                OnJoinFailed(progress, multiplayer, response);
            }
            else
            {
                DownloadWorld(progress, multiplayer);
            }
        }

        private static void OnJoinFailed(MyGuiScreenProgress progress, MyMultiplayerBase multiplayer, MyLobbyEnterResponseEnum response)
        {
            if (multiplayer != null)
            {
                multiplayer.Dispose();
            }
            progress.Cancel();
            if (response == MyLobbyEnterResponseEnum.FriendsOnly)
            {
                MyStringId caption = new MyStringId();
                MyGuiSandbox.Show(MyCommonTexts.OnlyFriendsCanJoinThisGame, caption, MyMessageBoxStyleEnum.Error);
            }
            else if (response != MyLobbyEnterResponseEnum.Success)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(MyCommonTexts.DialogTextJoinWorldFailed, response);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, stringBuilder, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public static void WorldReceived(MyObjectBuilder_World world, MyMultiplayerBase multiplayer)
        {
            if (((world == null) || ((world.Checkpoint == null) || ((world.Checkpoint.Settings == null) || MySandboxGame.Config.ExperimentalMode))) || !(world.Checkpoint.Settings.IsSettingsExperimental() || ((world.Checkpoint.Mods != null) && (world.Checkpoint.Mods.Count != 0))))
            {
                CheckDx11AndJoin(world, multiplayer);
            }
            else
            {
                MySessionLoader.UnloadAndExitToMenu();
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(MyCommonTexts.DialogTextJoinWorldFailed, MyTexts.GetString(MyCommonTexts.MultiplayerErrorExperimental));
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, stringBuilder, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyJoinGameHelper.<>c <>9 = new MyJoinGameHelper.<>c();
            public static Action<IMyLobby> <>9__1_0;
            public static Action<string> <>9__2_1;
            public static Func<IMyEventOwner, Action<RenderProfilerCommand, int>> <>9__2_5;
            public static Action <>9__3_0;

            internal void <JoinGame>b__1_0(IMyLobby l)
            {
                MyJoinGameHelper.JoinGame(l, false);
            }

            internal void <JoinGame>b__2_1(string desc)
            {
                MyHudNotification notification = new MyHudNotification(MyStringId.GetOrCompute(desc), 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                MyHud.Notifications.Add(notification);
                MyLog.Default.WriteLine(desc);
            }

            internal Action<RenderProfilerCommand, int> <JoinGame>b__2_5(IMyEventOwner _) => 
                new Action<RenderProfilerCommand, int>(Sandbox.MyRenderProfiler.OnCommandReceived);

            internal void <JoinGame>b__3_0()
            {
                MySessionLoader.UnloadAndExitToMenu();
            }
        }
    }
}

