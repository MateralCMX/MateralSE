namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.Game;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    internal class MyGuiScreenStartQuickLaunch : MyGuiScreenBase
    {
        private MyQuickLaunchType m_quickLaunchType;
        private bool m_childScreenLaunched;
        public static MyGuiScreenStartQuickLaunch CurrentScreen;

        public MyGuiScreenStartQuickLaunch(MyQuickLaunchType quickLaunchType, MyStringId progressText) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_quickLaunchType = quickLaunchType;
            CurrentScreen = this;
        }

        private static MyWorldGenerator.Args CreateBasicQuickstartArgs() => 
            new MyWorldGenerator.Args { 
                Scenario = MyDefinitionManager.Static.GetScenarioDefinition(new MyDefinitionId(typeof(MyObjectBuilder_ScenarioDefinition), "EasyStart1")),
                AsteroidAmount = 0
            };

        private static MyObjectBuilder_SessionSettings CreateBasicQuickStartSettings()
        {
            MyObjectBuilder_SessionSettings sessionSettings = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SessionSettings>();
            sessionSettings.GameMode = MyGameModeEnum.Creative;
            sessionSettings.EnableToolShake = true;
            sessionSettings.EnableSunRotation = MyPerGameSettings.Game == GameEnum.SE_GAME;
            sessionSettings.VoxelGeneratorVersion = 4;
            sessionSettings.CargoShipsEnabled = true;
            sessionSettings.EnableOxygen = true;
            sessionSettings.EnableSpiders = false;
            sessionSettings.EnableWolfs = false;
            MyWorldGenerator.SetProceduralSettings(-1, sessionSettings);
            return sessionSettings;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenStartQuickLaunch";

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public static void QuickstartSandbox(MyObjectBuilder_SessionSettings quickstartSettings, MyWorldGenerator.Args? quickstartArgs)
        {
            MyLog.Default.WriteLine("QuickstartSandbox - START");
            MyScreenManager.RemoveAllScreensExcept(null);
            MySessionLoader.StartLoading(delegate {
                MyObjectBuilder_SessionSettings settings = quickstartSettings ?? CreateBasicQuickStartSettings();
                MyWorldGenerator.Args? nullable = quickstartArgs;
                MySpaceAnalytics.Instance.SetEntry(MyGameEntryEnum.Quickstart);
                MySession.Start("Created " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), "", "", settings, new List<MyObjectBuilder_Checkpoint.ModItem>(0), (nullable != null) ? nullable.GetValueOrDefault() : CreateBasicQuickstartArgs());
            }, null, null, null);
            MyLog.Default.WriteLine("QuickstartSandbox - END");
        }

        public override bool Update(bool hasFocus)
        {
            if (hasFocus)
            {
                if (this.m_childScreenLaunched & hasFocus)
                {
                    this.CloseScreenNow();
                }
                if (this.m_childScreenLaunched)
                {
                    return base.Update(hasFocus);
                }
                if (MyInput.Static.IsKeyPress(MyKeys.Escape))
                {
                    MySessionLoader.UnloadAndExitToMenu();
                    return base.Update(hasFocus);
                }
                MyQuickLaunchType quickLaunchType = this.m_quickLaunchType;
                if (quickLaunchType == MyQuickLaunchType.NEW_SANDBOX)
                {
                    MyWorldGenerator.Args? quickstartArgs = null;
                    QuickstartSandbox(null, quickstartArgs);
                    this.m_childScreenLaunched = true;
                }
                else
                {
                    if (quickLaunchType != MyQuickLaunchType.LAST_SANDBOX)
                    {
                        throw new InvalidBranchException();
                    }
                    string lastSessionPath = MyLocalCache.GetLastSessionPath();
                    if ((lastSessionPath == null) || !Directory.Exists(lastSessionPath))
                    {
                        MySandboxGame.AfterLogos();
                    }
                    else
                    {
                        MyOnlineModeEnum? onlineMode = null;
                        MySessionLoader.LoadSingleplayerSession(lastSessionPath, null, null, onlineMode, 0);
                    }
                    this.m_childScreenLaunched = true;
                }
            }
            return base.Update(hasFocus);
        }
    }
}

