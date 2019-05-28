namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;

    public static class MySessionLoader
    {
        private static readonly Random random = new Random();
        [CompilerGenerated]
        private static Action BattleWorldLoaded;
        [CompilerGenerated]
        private static Action ScenarioWorldLoaded;
        [CompilerGenerated]
        private static Action<MyObjectBuilder_Checkpoint> CampaignWorldLoaded;

        public static  event Action BattleWorldLoaded
        {
            [CompilerGenerated] add
            {
                Action battleWorldLoaded = BattleWorldLoaded;
                while (true)
                {
                    Action a = battleWorldLoaded;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    battleWorldLoaded = Interlocked.CompareExchange<Action>(ref BattleWorldLoaded, action3, a);
                    if (ReferenceEquals(battleWorldLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action battleWorldLoaded = BattleWorldLoaded;
                while (true)
                {
                    Action source = battleWorldLoaded;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    battleWorldLoaded = Interlocked.CompareExchange<Action>(ref BattleWorldLoaded, action3, source);
                    if (ReferenceEquals(battleWorldLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyObjectBuilder_Checkpoint> CampaignWorldLoaded
        {
            [CompilerGenerated] add
            {
                Action<MyObjectBuilder_Checkpoint> campaignWorldLoaded = CampaignWorldLoaded;
                while (true)
                {
                    Action<MyObjectBuilder_Checkpoint> a = campaignWorldLoaded;
                    Action<MyObjectBuilder_Checkpoint> action3 = (Action<MyObjectBuilder_Checkpoint>) Delegate.Combine(a, value);
                    campaignWorldLoaded = Interlocked.CompareExchange<Action<MyObjectBuilder_Checkpoint>>(ref CampaignWorldLoaded, action3, a);
                    if (ReferenceEquals(campaignWorldLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyObjectBuilder_Checkpoint> campaignWorldLoaded = CampaignWorldLoaded;
                while (true)
                {
                    Action<MyObjectBuilder_Checkpoint> source = campaignWorldLoaded;
                    Action<MyObjectBuilder_Checkpoint> action3 = (Action<MyObjectBuilder_Checkpoint>) Delegate.Remove(source, value);
                    campaignWorldLoaded = Interlocked.CompareExchange<Action<MyObjectBuilder_Checkpoint>>(ref CampaignWorldLoaded, action3, source);
                    if (ReferenceEquals(campaignWorldLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action ScenarioWorldLoaded
        {
            [CompilerGenerated] add
            {
                Action scenarioWorldLoaded = ScenarioWorldLoaded;
                while (true)
                {
                    Action a = scenarioWorldLoaded;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    scenarioWorldLoaded = Interlocked.CompareExchange<Action>(ref ScenarioWorldLoaded, action3, a);
                    if (ReferenceEquals(scenarioWorldLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action scenarioWorldLoaded = ScenarioWorldLoaded;
                while (true)
                {
                    Action source = scenarioWorldLoaded;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    scenarioWorldLoaded = Interlocked.CompareExchange<Action>(ref ScenarioWorldLoaded, action3, source);
                    if (ReferenceEquals(scenarioWorldLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        private static void CheckDx11AndLoad(MyObjectBuilder_Checkpoint checkpoint, string sessionPath, ulong checkpointSizeInBytes, Action afterLoad = null)
        {
            if ((checkpoint.RequiresDX < 11) || MySandboxGame.IsDirectX11)
            {
                LoadSingleplayerSession(checkpoint, sessionPath, checkpointSizeInBytes, afterLoad);
            }
            else
            {
                MyJoinGameHelper.HandleDx11Needed();
            }
        }

        private static void DownloadModsDone(bool success, MyObjectBuilder_Checkpoint checkpoint, string sessionPath, ulong checkpointSizeInBytes, Action afterLoad)
        {
            if (success || ((checkpoint.Settings.OnlineMode == MyOnlineModeEnum.OFFLINE) && MyWorkshop.CanRunOffline(checkpoint.Mods)))
            {
                MyScreenManager.CloseAllScreensNowExcept(null);
                MyGuiSandbox.Update(0x10);
                string customLoadingScreenPath = GetCustomLoadingScreenImagePath(checkpoint.CustomLoadingScreenImage);
                StartLoading(delegate {
                    if (MySession.Static != null)
                    {
                        MySession.Static.Unload();
                        MySession.Static = null;
                    }
                    MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Load);
                    MySession.Load(sessionPath, checkpoint, checkpointSizeInBytes, true, false);
                    if (afterLoad != null)
                    {
                        afterLoad();
                    }
                }, () => StartLoading(delegate {
                    if (MySession.Static != null)
                    {
                        MySession.Static.Unload();
                        MySession.Static = null;
                    }
                    MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Load);
                    MySession.Load(sessionPath, checkpoint, checkpointSizeInBytes, true, true);
                    if (afterLoad != null)
                    {
                        afterLoad();
                    }
                }, null, customLoadingScreenPath, checkpoint.CustomLoadingScreenText), customLoadingScreenPath, checkpoint.CustomLoadingScreenText);
            }
            else
            {
                StringBuilder builder;
                MyStringId? nullable;
                Vector2? nullable2;
                MyLog.Default.WriteLine(MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed).ToString());
                if (MyGameService.IsOnline)
                {
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (MyFakes.QUICK_LAUNCH != null)
                        {
                            UnloadAndExitToMenu();
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.DialogTextDownloadModsFailedSteamOffline), MySession.Platform)), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
        }

        public static void ExitGame()
        {
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.ReportGameQuit("Exit to Windows");
            }
            MyScreenManager.CloseAllScreensNowExcept(null);
            MySandboxGame.ExitThreadSafe();
        }

        private static string GetCustomLoadingScreenImagePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }
            string path = Path.Combine(MyFileSystem.SavesPath, relativePath);
            if (!MyFileSystem.FileExists(path))
            {
                path = Path.Combine(MyFileSystem.ContentPath, relativePath);
            }
            if (!MyFileSystem.FileExists(path))
            {
                path = Path.Combine(MyFileSystem.ModsPath, relativePath);
            }
            if (!MyFileSystem.FileExists(path))
            {
                path = null;
            }
            return path;
        }

        public static void LoadInventoryScene()
        {
            if (MyGameService.IsActive && MyFakes.ENABLE_MAIN_MENU_INVENTORY_SCENE)
            {
                ulong num;
                string sessionPath = Path.Combine(MyFileSystem.ContentPath, @"InventoryScenes\Inventory-9");
                DictionaryValuesReader<MyDefinitionId, MyMainMenuInventorySceneDefinition> mainMenuInventoryScenes = MyDefinitionManager.Static.GetMainMenuInventoryScenes();
                if (mainMenuInventoryScenes.Count > 0)
                {
                    List<MyMainMenuInventorySceneDefinition> list = mainMenuInventoryScenes.ToList<MyMainMenuInventorySceneDefinition>();
                    sessionPath = Path.Combine(MyFileSystem.ContentPath, list[random.Next(0, list.Count)].SceneDirectory);
                }
                MySession.Load(sessionPath, MyLocalCache.LoadCheckpoint(sessionPath, out num), num, false, true);
            }
        }

        public static void LoadLastSession()
        {
            string lastSessionPath = MyLocalCache.GetLastSessionPath();
            if ((lastSessionPath != null) && MyFileSystem.DirectoryExists(lastSessionPath))
            {
                MyOnlineModeEnum? onlineMode = null;
                LoadSingleplayerSession(lastSessionPath, null, null, onlineMode, 0);
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxLastSessionNotFound), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public static void LoadMultiplayerScenarioWorld(MyObjectBuilder_World world, MyMultiplayerBase multiplayerSession)
        {
            MyLog.Default.WriteLine("LoadMultiplayerScenarioWorld() - Start");
            if ((world.Checkpoint.BriefingVideo != null) && (world.Checkpoint.BriefingVideo.Length > 0))
            {
                MyGuiSandbox.OpenUrlWithFallback(world.Checkpoint.BriefingVideo, "Scenario briefing video", true);
            }
            if (MyWorkshop.CheckLocalModsAllowed(world.Checkpoint.Mods, false))
            {
                MyWorkshop.DownloadModsAsync(world.Checkpoint.Mods, delegate (bool success) {
                    if (success)
                    {
                        MyScreenManager.CloseAllScreensNowExcept(null);
                        MyGuiSandbox.Update(0x10);
                        StartLoading(delegate {
                            MySession.Static.LoadMultiplayerWorld(world, multiplayerSession);
                            if (ScenarioWorldLoaded != null)
                            {
                                ScenarioWorldLoaded();
                            }
                        }, null, null, null);
                    }
                    else
                    {
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, result => MySandboxGame.Static.Invoke(new Action(MySessionLoader.UnloadAndExitToMenu), "UnloadAndExitToMenu"), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                    MyLog.Default.WriteLine("LoadMultiplayerScenarioWorld() - End");
                }, () => UnloadAndExitToMenu());
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextLocalModsDisabledInMultiplayer), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, result => UnloadAndExitToMenu(), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MyLog.Default.WriteLine("LoadMultiplayerScenarioWorld() - End");
            }
        }

        public static void LoadMultiplayerSession(MyObjectBuilder_World world, MyMultiplayerBase multiplayerSession)
        {
            MyLog.Default.WriteLine("LoadSession() - Start");
            if (MyWorkshop.CheckLocalModsAllowed(world.Checkpoint.Mods, false))
            {
                MyWorkshop.DownloadModsAsync(world.Checkpoint.Mods, delegate (bool success) {
                    if (success)
                    {
                        MyScreenManager.CloseAllScreensNowExcept(null);
                        MyGuiSandbox.Update(0x10);
                        if (MySession.Static != null)
                        {
                            MySession.Static.Unload();
                            MySession.Static = null;
                        }
                        StartLoading(() => MySession.LoadMultiplayer(world, multiplayerSession), null, null, null);
                    }
                    else
                    {
                        StringBuilder builder;
                        MyStringId? nullable;
                        Vector2? nullable2;
                        if (MyMultiplayer.Static != null)
                        {
                            MyMultiplayer.Static.Dispose();
                        }
                        if (MyGameService.IsOnline)
                        {
                            builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                        else
                        {
                            builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.DialogTextDownloadModsFailedSteamOffline), MySession.Platform)), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    }
                    MyLog.Default.WriteLine("LoadSession() - End");
                }, () => multiplayerSession.Dispose());
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextLocalModsDisabledInMultiplayer), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MyLog.Default.WriteLine("LoadSession() - End");
            }
        }

        public static void LoadSingleplayerSession(MyObjectBuilder_Checkpoint checkpoint, string sessionPath, ulong checkpointSizeInBytes, Action afterLoad = null)
        {
            StringBuilder builder;
            MyStringId? nullable;
            Vector2? nullable2;
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
            else
            {
                int num1;
                int num2;
                if (((MyCampaignManager.Static != null) && (MyCampaignManager.Static.IsNewCampaignLevelLoading && ((MyCampaignManager.Static.ActiveCampaign != null) && ((MyCampaignManager.Static.ActiveCampaign.Name == "The First Jump") && MyCampaignManager.Static.ActiveCampaign.IsVanilla)))) && (MyCampaignManager.Static.ActiveCampaign.PublishedFileId == 0))
                {
                    num1 = 1;
                }
                else if (string.IsNullOrEmpty(checkpoint.CustomLoadingScreenImage) || !checkpoint.CustomLoadingScreenImage.StartsWith(@"Scenarios\The First Jump"))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (checkpoint.Mods == null) ? 1 : ((int) (checkpoint.Mods.Count == 0));
                }
                if (((num2 == 0) && !MySandboxGame.Config.ExperimentalMode) && checkpoint.Settings.ExperimentalMode)
                {
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SaveGameErrorExperimental), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else if (MyWorkshop.CheckLocalModsAllowed(checkpoint.Mods, checkpoint.Settings.OnlineMode == MyOnlineModeEnum.OFFLINE))
                {
                    MyWorkshop.DownloadModsAsync(checkpoint.Mods, delegate (bool success) {
                        MySandboxGame.Static.Invoke(() => DownloadModsDone(success, checkpoint, sessionPath, checkpointSizeInBytes, afterLoad), "MySessionLoader::DownloadModsDone");
                        MyLog.Default.WriteLine("LoadSession() - End");
                    }, null);
                }
                else
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
            }
        }

        public static void LoadSingleplayerSession(string sessionPath, Action afterLoad = null, string contextName = null, MyOnlineModeEnum? onlineMode = new MyOnlineModeEnum?(), int maxPlayers = 0)
        {
            ulong num;
            MyLog.Default.WriteLine("LoadSession() - Start");
            MyLog.Default.WriteLine(sessionPath);
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out num);
            if (checkpoint != null)
            {
                checkpoint.CustomLoadingScreenText = MyStatControlText.SubstituteTexts(checkpoint.CustomLoadingScreenText, contextName);
                if (onlineMode != null)
                {
                    checkpoint.OnlineMode = onlineMode.Value;
                    checkpoint.MaxPlayers = (short) maxPlayers;
                }
                CheckDx11AndLoad(checkpoint, sessionPath, num, afterLoad);
            }
            else
            {
                MyLog.Default.WriteLine(MyTexts.Get(MyCommonTexts.WorldFileIsCorruptedAndCouldNotBeLoaded).ToString());
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.WorldFileIsCorruptedAndCouldNotBeLoaded), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MyLog.Default.WriteLine("LoadSession() - End");
            }
        }

        public static void StartLoading(Action loadingAction, Action loadingActionXMLAllowed = null, string customLoadingBackground = null, string customLoadingtext = null)
        {
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.StoreLoadingStartTime();
            }
            MyGuiScreenGamePlay screenToLoad = new MyGuiScreenGamePlay();
            screenToLoad.OnLoadingAction = (Action) Delegate.Combine(screenToLoad.OnLoadingAction, loadingAction);
            MyGuiScreenLoading loading1 = new MyGuiScreenLoading(screenToLoad, MyGuiScreenGamePlay.Static, customLoadingBackground, customLoadingtext);
            MyGuiScreenLoading loading2 = new MyGuiScreenLoading(screenToLoad, MyGuiScreenGamePlay.Static, customLoadingBackground, customLoadingtext);
            loading2.OnScreenLoadingFinished += () => MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HUDScreen, Array.Empty<object>()));
            MyGuiScreenLoading screen = loading2;
            screen.OnLoadingXMLAllowed = loadingActionXMLAllowed;
            MyGuiSandbox.AddScreen(screen);
        }

        public static void StartNewSession(string sessionName, MyObjectBuilder_SessionSettings settings, List<MyObjectBuilder_Checkpoint.ModItem> mods, MyScenarioDefinition scenarioDefinition = null, int asteroidAmount = 0, string description = "", string passwd = "")
        {
            MyLog.Default.WriteLine("StartNewSandbox - Start");
            if (MyWorkshop.CheckLocalModsAllowed(mods, settings.OnlineMode == MyOnlineModeEnum.OFFLINE))
            {
                MyWorkshop.DownloadModsAsync(mods, delegate (bool success) {
                    if (!success && ((settings.OnlineMode != MyOnlineModeEnum.OFFLINE) || !MyWorkshop.CanRunOffline(mods)))
                    {
                        StringBuilder builder;
                        MyStringId? nullable;
                        Vector2? nullable2;
                        if (MyGameService.IsOnline)
                        {
                            builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextDownloadModsFailed), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                        else
                        {
                            builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.DialogTextDownloadModsFailedSteamOffline), MySession.Platform)), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    }
                    else
                    {
                        MyScreenManager.RemoveAllScreensExcept(null);
                        if (asteroidAmount < 0)
                        {
                            MyWorldGenerator.SetProceduralSettings(new int?(asteroidAmount), settings);
                            asteroidAmount = 0;
                        }
                        MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Custom);
                        StartLoading(delegate {
                            MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Custom);
                            MyWorldGenerator.Args generationArgs = new MyWorldGenerator.Args {
                                AsteroidAmount = asteroidAmount,
                                Scenario = scenarioDefinition
                            };
                            MySession.Start(sessionName, description, passwd, settings, mods, generationArgs);
                        }, null, null, null);
                    }
                    MyLog.Default.WriteLine("StartNewSandbox - End");
                }, null);
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.DialogTextLocalModsDisabledInMultiplayer), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MyLog.Default.WriteLine("LoadSession() - End");
            }
        }

        public static void Unload()
        {
            MyScreenManager.CloseAllScreensNowExcept(null);
            MyGuiSandbox.Update(0x10);
            if (MySession.Static != null)
            {
                MySession.Static.Unload();
                MySession.Static = null;
            }
            if (MyMusicController.Static != null)
            {
                MyMusicController.Static.Unload();
                MyMusicController.Static = null;
                MyAudio.Static.MusicAllowed = true;
                MyAudio.Static.Mute = false;
            }
            if (MyMultiplayer.Static != null)
            {
                MyMultiplayer.Static.Dispose();
            }
        }

        public static void UnloadAndExitToMenu()
        {
            Unload();
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.MainMenu, Array.Empty<object>()));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionLoader.<>c <>9 = new MySessionLoader.<>c();
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__13_2;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__13_4;
            public static Action <>9__13_1;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__18_3;
            public static Action <>9__19_0;

            internal void <DownloadModsDone>b__18_3(MyGuiScreenMessageBox.ResultEnum result)
            {
                if (MyFakes.QUICK_LAUNCH != null)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
            }

            internal void <LoadMultiplayerScenarioWorld>b__13_1()
            {
                MySessionLoader.UnloadAndExitToMenu();
            }

            internal void <LoadMultiplayerScenarioWorld>b__13_2(MyGuiScreenMessageBox.ResultEnum result)
            {
                MySessionLoader.UnloadAndExitToMenu();
            }

            internal void <LoadMultiplayerScenarioWorld>b__13_4(MyGuiScreenMessageBox.ResultEnum result)
            {
                MySandboxGame.Static.Invoke(new Action(MySessionLoader.UnloadAndExitToMenu), "UnloadAndExitToMenu");
            }

            internal void <StartLoading>b__19_0()
            {
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HUDScreen, Array.Empty<object>()));
            }
        }
    }
}

