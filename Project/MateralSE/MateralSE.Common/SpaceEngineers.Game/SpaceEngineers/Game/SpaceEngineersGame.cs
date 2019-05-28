namespace SpaceEngineers.Game
{
    using Multiplayer;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Screens.Helpers;
    using SpaceEngineers.Game.AI;
    using SpaceEngineers.Game.GUI;
    using SpaceEngineers.Game.VoiceChat;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Data.Audio;
    using VRage.Input;
    using VRage.Utils;
    using VRageRender;
    using VRageRender.Messages;
    using World;

    public class SpaceEngineersGame : MySandboxGame
    {
        public const int SE_VERSION = 0x122879;

        public SpaceEngineersGame(string[] commandlineArgs) : base(commandlineArgs)
        {
            MySandboxGame.GameCustomInitialization = new MySpaceGameCustomInitialization();
        }

        protected override void CheckGraphicsCard(MyRenderMessageVideoAdaptersResponse msgVideoAdapters)
        {
            base.CheckGraphicsCard(msgVideoAdapters);
            MyAdapterInfo info = msgVideoAdapters.Adapters[MyVideoSettingsManager.CurrentDeviceSettings.AdapterOrdinal];
            MyPerformanceSettings preset = MyGuiScreenOptionsGraphics.GetPreset(info.Quality);
            if (info.VRAM < 0x1e848000L)
            {
                preset.RenderSettings.TextureQuality = MyTextureQuality.LOW;
            }
            else if ((info.VRAM < 0x77359400L) && (info.Quality == MyRenderQualityEnum.HIGH))
            {
                preset.RenderSettings.TextureQuality = MyTextureQuality.MEDIUM;
            }
            MyVideoSettingsManager.UpdateRenderSettingsFromConfig(ref preset);
        }

        private static void FillCredits()
        {
            MyCreditsDepartment item = new MyCreditsDepartment("{LOCG:Department_ExecutiveProducer}");
            MyPerGameSettings.Credits.Departments.Add(item);
            item.Persons = new List<MyCreditsPerson>();
            item.Persons.Add(new MyCreditsPerson("MAREK ROSA"));
            MyCreditsDepartment department2 = new MyCreditsDepartment("{LOCG:Department_LeadProducer}");
            MyPerGameSettings.Credits.Departments.Add(department2);
            department2.Persons = new List<MyCreditsPerson>();
            department2.Persons.Add(new MyCreditsPerson("PETR MINARIK"));
            MyCreditsDepartment department3 = new MyCreditsDepartment("{LOCG:Department_TeamOperations}");
            MyPerGameSettings.Credits.Departments.Add(department3);
            department3.Persons = new List<MyCreditsPerson>();
            department3.Persons.Add(new MyCreditsPerson("VLADISLAV POLGAR"));
            MyCreditsDepartment department4 = new MyCreditsDepartment("{LOCG:Department_TechnicalDirector}");
            MyPerGameSettings.Credits.Departments.Add(department4);
            department4.Persons = new List<MyCreditsPerson>();
            department4.Persons.Add(new MyCreditsPerson("JAN \"CENDA\" HLOUSEK"));
            MyCreditsDepartment department5 = new MyCreditsDepartment("{LOCG:Department_LeadProgrammers}");
            MyPerGameSettings.Credits.Departments.Add(department5);
            department5.Persons = new List<MyCreditsPerson>();
            department5.Persons.Add(new MyCreditsPerson("FILIP DUSEK"));
            department5.Persons.Add(new MyCreditsPerson("JAN \"CENDA\" HLOUSEK"));
            department5.Persons.Add(new MyCreditsPerson("PETR MINARIK"));
            MyCreditsDepartment department6 = new MyCreditsDepartment("{LOCG:Department_Programmers}");
            MyPerGameSettings.Credits.Departments.Add(department6);
            department6.Persons = new List<MyCreditsPerson>();
            department6.Persons.Add(new MyCreditsPerson("PETR BERANEK"));
            department6.Persons.Add(new MyCreditsPerson("MIRO FARKAS"));
            department6.Persons.Add(new MyCreditsPerson("SANDRA LENARDOVA"));
            department6.Persons.Add(new MyCreditsPerson("MARTIN PAVLICEK"));
            MyCreditsDepartment department7 = new MyCreditsDepartment("{LOCG:Department_LeadDesigner}");
            MyPerGameSettings.Credits.Departments.Add(department7);
            department7.Persons = new List<MyCreditsPerson>();
            department7.Persons.Add(new MyCreditsPerson("JOACHIM KOOLHOF"));
            MyCreditsDepartment department8 = new MyCreditsDepartment("{LOCG:Department_Designers}");
            MyPerGameSettings.Credits.Departments.Add(department8);
            department8.Persons = new List<MyCreditsPerson>();
            department8.Persons.Add(new MyCreditsPerson("ALES KOZAK"));
            MyCreditsDepartment department9 = new MyCreditsDepartment("{LOCG:Department_LeadArtist}");
            MyPerGameSettings.Credits.Departments.Add(department9);
            department9.Persons = new List<MyCreditsPerson>();
            department9.Persons.Add(new MyCreditsPerson("NATIQ AGHAYEV"));
            MyCreditsDepartment department10 = new MyCreditsDepartment("{LOCG:Department_Artists}");
            MyPerGameSettings.Credits.Departments.Add(department10);
            department10.Persons = new List<MyCreditsPerson>();
            department10.Persons.Add(new MyCreditsPerson("KRISTIAAN RENAERTS"));
            department10.Persons.Add(new MyCreditsPerson("JAN TRAUSKE"));
            MyCreditsDepartment department11 = new MyCreditsDepartment("{LOCG:Department_SoundDesign}");
            MyPerGameSettings.Credits.Departments.Add(department11);
            department11.Persons = new List<MyCreditsPerson>();
            department11.Persons.Add(new MyCreditsPerson("LUKAS TVRDON"));
            MyCreditsDepartment department12 = new MyCreditsDepartment("{LOCG:Department_Music}");
            MyPerGameSettings.Credits.Departments.Add(department12);
            department12.Persons = new List<MyCreditsPerson>();
            department12.Persons.Add(new MyCreditsPerson("KAREL ANTONIN"));
            department12.Persons.Add(new MyCreditsPerson("ANNA KALHAUSOVA (cello)"));
            department12.Persons.Add(new MyCreditsPerson("MARIE SVOBODOVA (vocals)"));
            MyCreditsDepartment department13 = new MyCreditsDepartment("{LOCG:Department_Video}");
            MyPerGameSettings.Credits.Departments.Add(department13);
            department13.Persons = new List<MyCreditsPerson>();
            department13.Persons.Add(new MyCreditsPerson("JOEL \"XOCLIW\" WILCOX"));
            MyCreditsDepartment department14 = new MyCreditsDepartment("{LOCG:Department_LeadTester}");
            MyPerGameSettings.Credits.Departments.Add(department14);
            department14.Persons = new List<MyCreditsPerson>();
            department14.Persons.Add(new MyCreditsPerson("ONDREJ NAHALKA"));
            MyCreditsDepartment department15 = new MyCreditsDepartment("{LOCG:Department_Testers}");
            MyPerGameSettings.Credits.Departments.Add(department15);
            department15.Persons = new List<MyCreditsPerson>();
            department15.Persons.Add(new MyCreditsPerson("KATERINA CERVENA"));
            department15.Persons.Add(new MyCreditsPerson("JAN HRIVNAC"));
            department15.Persons.Add(new MyCreditsPerson("ALES KOZAK"));
            department15.Persons.Add(new MyCreditsPerson("VOJTECH NEORAL"));
            department15.Persons.Add(new MyCreditsPerson("JAN PETRZILKA"));
            MyCreditsDepartment department16 = new MyCreditsDepartment("{LOCG:Department_CommunityPr}");
            MyPerGameSettings.Credits.Departments.Add(department16);
            department16.Persons = new List<MyCreditsPerson>();
            department16.Persons.Add(new MyCreditsPerson("JESSE BAULE"));
            department16.Persons.Add(new MyCreditsPerson("JOEL \"XOCLIW\" WILCOX"));
            MyCreditsDepartment department17 = new MyCreditsDepartment("{LOCG:Department_Office}");
            MyPerGameSettings.Credits.Departments.Add(department17);
            department17.Persons = new List<MyCreditsPerson>();
            department17.Persons.Add(new MyCreditsPerson("MARIANNA HIRCAKOVA"));
            department17.Persons.Add(new MyCreditsPerson("PETR KREJCI"));
            department17.Persons.Add(new MyCreditsPerson("LUCIE KRESTOVA"));
            department17.Persons.Add(new MyCreditsPerson("VACLAV NOVOTNY"));
            department17.Persons.Add(new MyCreditsPerson("TOMAS STROUHAL"));
            MyCreditsDepartment department18 = new MyCreditsDepartment("{LOCG:Department_CommunityManagers}");
            MyPerGameSettings.Credits.Departments.Add(department18);
            department18.Persons = new List<MyCreditsPerson>();
            department18.Persons.Add(new MyCreditsPerson("Dr Vagax"));
            department18.Persons.Add(new MyCreditsPerson("Conrad Larson"));
            department18.Persons.Add(new MyCreditsPerson("Dan2D3D"));
            department18.Persons.Add(new MyCreditsPerson("RayvenQ"));
            department18.Persons.Add(new MyCreditsPerson("Redphoenix"));
            department18.Persons.Add(new MyCreditsPerson("TodesRitter"));
            MyCreditsDepartment department19 = new MyCreditsDepartment("{LOCG:Department_ModContributors}");
            MyPerGameSettings.Credits.Departments.Add(department19);
            department19.Persons = new List<MyCreditsPerson>();
            department19.Persons.Add(new MyCreditsPerson("Tyrsis"));
            department19.Persons.Add(new MyCreditsPerson("Daniel \"Phoenix84\" Osborne"));
            department19.Persons.Add(new MyCreditsPerson("Morten \"Malware\" Aune Lyrstad"));
            department19.Persons.Add(new MyCreditsPerson("Arindel"));
            department19.Persons.Add(new MyCreditsPerson("Darth Biomech"));
            department19.Persons.Add(new MyCreditsPerson("Night Lone"));
            department19.Persons.Add(new MyCreditsPerson("Mexmer"));
            department19.Persons.Add(new MyCreditsPerson("JD.Horx"));
            department19.Persons.Add(new MyCreditsPerson("John \"Jimmacle\" Gross"));
            MyCreditsDepartment department20 = new MyCreditsDepartment("{LOCG:Department_Translators}");
            MyPerGameSettings.Credits.Departments.Add(department20);
            department20.Persons = new List<MyCreditsPerson>();
            department20.Persons.Add(new MyCreditsPerson("Damian \"Truzaku\" Komarek"));
            department20.Persons.Add(new MyCreditsPerson("Julian Tomaszewski"));
            department20.Persons.Add(new MyCreditsPerson("George Grivas"));
            department20.Persons.Add(new MyCreditsPerson("Олег \"AaLeSsHhKka\" Цюпка"));
            department20.Persons.Add(new MyCreditsPerson("Maxim \"Ma)(imuM\" Lyashuk"));
            department20.Persons.Add(new MyCreditsPerson("Axazel"));
            department20.Persons.Add(new MyCreditsPerson("Baly94"));
            department20.Persons.Add(new MyCreditsPerson("Dyret"));
            department20.Persons.Add(new MyCreditsPerson("gon.gged"));
            department20.Persons.Add(new MyCreditsPerson("Huberto"));
            department20.Persons.Add(new MyCreditsPerson("HunterNephilim"));
            department20.Persons.Add(new MyCreditsPerson("nintendo22"));
            department20.Persons.Add(new MyCreditsPerson("Quellix"));
            department20.Persons.Add(new MyCreditsPerson("raviool"));
            department20.Persons.Add(new MyCreditsPerson("Dr. Bell"));
            department20.Persons.Add(new MyCreditsPerson("Dominik Frydl"));
            department20.Persons.Add(new MyCreditsPerson("Daniel Hloušek"));
            department20.LogoTexture = @"Textures\Logo\TranslatorsCN.dds";
            department20.LogoScale = 0.85f;
            department20.LogoOffsetPost = 0.11f;
            MyCreditsDepartment department21 = new MyCreditsDepartment("{LOCG:Department_SpecialThanks}");
            MyPerGameSettings.Credits.Departments.Add(department21);
            department21.Persons = new List<MyCreditsPerson>();
            department21.Persons.Add(new MyCreditsPerson("ABDULAZIZ ALDIGS"));
            department21.Persons.Add(new MyCreditsPerson("DUSAN ANDRAS"));
            department21.Persons.Add(new MyCreditsPerson("ONDREJ ANGELOVIC"));
            department21.Persons.Add(new MyCreditsPerson("IVAN BARAN"));
            department21.Persons.Add(new MyCreditsPerson("ANTON \"TOTAL\" BAUER"));
            department21.Persons.Add(new MyCreditsPerson("ALES BRICH"));
            department21.Persons.Add(new MyCreditsPerson("JOAO CARIAS"));
            department21.Persons.Add(new MyCreditsPerson("THEO ESCAMEZ"));
            department21.Persons.Add(new MyCreditsPerson("ALEX FLOREA"));
            department21.Persons.Add(new MyCreditsPerson("JAN GOLMIC"));
            department21.Persons.Add(new MyCreditsPerson("CESTMIR HOUSKA"));
            department21.Persons.Add(new MyCreditsPerson("JAKUB HRNCIR"));
            department21.Persons.Add(new MyCreditsPerson("LUKAS CHRAPEK"));
            department21.Persons.Add(new MyCreditsPerson("DANIEL ILHA"));
            department21.Persons.Add(new MyCreditsPerson("LUKAS JANDIK"));
            department21.Persons.Add(new MyCreditsPerson("MARKETA JAROSOVA"));
            department21.Persons.Add(new MyCreditsPerson("MARTIN KOCISEK"));
            department21.Persons.Add(new MyCreditsPerson("JOELLEN KOESTER"));
            department21.Persons.Add(new MyCreditsPerson("GREGORY KONTADAKIS"));
            department21.Persons.Add(new MyCreditsPerson("MARKO KORHONEN"));
            department21.Persons.Add(new MyCreditsPerson("TOMAS KOSEK"));
            department21.Persons.Add(new MyCreditsPerson("RADOVAN KOTRLA"));
            department21.Persons.Add(new MyCreditsPerson("MARTIN KROSLAK"));
            department21.Persons.Add(new MyCreditsPerson("MICHAL KUCIS"));
            department21.Persons.Add(new MyCreditsPerson("DANIEL LEIMBACH"));
            department21.Persons.Add(new MyCreditsPerson("RADKA LISA"));
            department21.Persons.Add(new MyCreditsPerson("PERCY LIU"));
            department21.Persons.Add(new MyCreditsPerson("GEORGE MAMAKOS"));
            department21.Persons.Add(new MyCreditsPerson("BRANT MARTIN"));
            department21.Persons.Add(new MyCreditsPerson("JAN NEKVAPIL"));
            department21.Persons.Add(new MyCreditsPerson("MAREK OBRSAL"));
            department21.Persons.Add(new MyCreditsPerson("PAVEL OCOVAJ"));
            department21.Persons.Add(new MyCreditsPerson("ONDREJ PETRZILKA"));
            department21.Persons.Add(new MyCreditsPerson("FRANCESKO PRETTO"));
            department21.Persons.Add(new MyCreditsPerson("TOMAS PSENICKA"));
            department21.Persons.Add(new MyCreditsPerson("DOMINIK RAGANCIK"));
            department21.Persons.Add(new MyCreditsPerson("TOMAS RAMPAS"));
            department21.Persons.Add(new MyCreditsPerson("DUSAN REPIK"));
            department21.Persons.Add(new MyCreditsPerson("VILEM SOULAK"));
            department21.Persons.Add(new MyCreditsPerson("RASTKO STANOJEVIC"));
            department21.Persons.Add(new MyCreditsPerson("SLOBODAN STEVIC"));
            department21.Persons.Add(new MyCreditsPerson("TIM TOXOPEUS"));
            department21.Persons.Add(new MyCreditsPerson("JAN VEBERSIK"));
            department21.Persons.Add(new MyCreditsPerson("LUKAS VILIM"));
            department21.Persons.Add(new MyCreditsPerson("MATEJ VLK"));
            department21.Persons.Add(new MyCreditsPerson("ADAM WILLIAMS"));
            department21.Persons.Add(new MyCreditsPerson("CHARLES WINTERS"));
            department21.Persons.Add(new MyCreditsPerson("MICHAL WROBEL"));
            department21.Persons.Add(new MyCreditsPerson("MICHAL ZAK"));
            department21.Persons.Add(new MyCreditsPerson("MICHAL ZAVADAK"));
            MyCreditsDepartment department22 = new MyCreditsDepartment("{LOCG:Department_MoreInfo}");
            MyPerGameSettings.Credits.Departments.Add(department22);
            department22.Persons = new List<MyCreditsPerson>();
            department22.Persons.Add(new MyCreditsPerson("{LOCG:Person_Web}"));
            department22.Persons.Add(new MyCreditsPerson("{LOCG:Person_FB}"));
            department22.Persons.Add(new MyCreditsPerson("{LOCG:Person_Twitter}"));
            MyCreditsNotice notice = new MyCreditsNotice {
                LogoTexture = @"Textures\Logo\vrage_logo_2_0_small.dds",
                LogoScale = 0.8f
            };
            notice.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_01}"));
            notice.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_02}"));
            notice.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_03}"));
            notice.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_04}"));
            MyPerGameSettings.Credits.CreditNotices.Add(notice);
            MyCreditsNotice notice2 = new MyCreditsNotice {
                LogoTexture = @"Textures\Logo\havok.dds",
                LogoScale = 0.65f
            };
            notice2.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_05}"));
            notice2.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_06}"));
            notice2.CreditNoticeLines.Add(new StringBuilder("{LOCG:NoticeLine_07}"));
            MyPerGameSettings.Credits.CreditNotices.Add(notice2);
            SetupSecrets();
        }

        protected override void InitInput()
        {
            base.InitInput();
            MyGuiDescriptor descriptor = new MyGuiDescriptor(MyCommonTexts.ControlName_ToggleSignalsMode, new MyStringId?(MyCommonTexts.ControlName_ToggleSignalsMode_Tooltip));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.TOGGLE_SIGNALS, descriptor);
            MyMouseButtonsEnum? defaultControlMouse = null;
            MyStringId? helpText = null;
            MyKeys? nullable3 = null;
            MyControl control = new MyControl(MyControlsSpace.TOGGLE_SIGNALS, descriptor.NameEnum, MyGuiControlTypeEnum.Spectator, defaultControlMouse, 0x48, helpText, nullable3, descriptor.DescriptionEnum);
            MyInput.Static.AddDefaultControl(MyControlsSpace.TOGGLE_SIGNALS, control);
            descriptor = new MyGuiDescriptor(MyCommonTexts.ControlName_CubeSizeMode, new MyStringId?(MyCommonTexts.ControlName_CubeSizeMode_Tooltip));
            MyGuiGameControlsHelpers.Add(MyControlsSpace.CUBE_BUILDER_CUBESIZE_MODE, descriptor);
            defaultControlMouse = null;
            helpText = null;
            nullable3 = null;
            control = new MyControl(MyControlsSpace.CUBE_BUILDER_CUBESIZE_MODE, descriptor.NameEnum, MyGuiControlTypeEnum.Systems2, defaultControlMouse, 0x52, helpText, nullable3, descriptor.DescriptionEnum);
            MyInput.Static.AddDefaultControl(MyControlsSpace.CUBE_BUILDER_CUBESIZE_MODE, control);
        }

        public static void SetupAnalytics()
        {
            IMyAnalytics tracker = new MyGameAnalytics("27bae5ba5219bcd64ddbf83113eabb30:d04e0431f97f90fae73b9d6ea99fc9746695bd11", 0x122879.ToString());
            MyAnalyticsManager.Instance.RegisterAnalyticsTracker(tracker);
        }

        public static void SetupBasicGameInfo()
        {
            MyPerGameSettings.BasicGameInfo.GameVersion = 0x122879;
            MyPerGameSettings.BasicGameInfo.GameName = "Space Engineers";
            MyPerGameSettings.BasicGameInfo.GameNameSafe = "SpaceEngineers";
            MyPerGameSettings.BasicGameInfo.ApplicationName = "SpaceEngineers";
            MyPerGameSettings.BasicGameInfo.GameAcronym = "SE";
            MyPerGameSettings.BasicGameInfo.MinimumRequirementsWeb = "http://www.spaceengineersgame.com";
            MyPerGameSettings.BasicGameInfo.SplashScreenImage = @"..\Content\Textures\Logo\splashscreen.png";
        }

        public static void SetupPerGameSettings()
        {
            MyPerGameSettings.Game = GameEnum.SE_GAME;
            MyPerGameSettings.GameIcon = "SpaceEngineers.ico";
            MyPerGameSettings.EnableGlobalGravity = false;
            MyPerGameSettings.GameModAssembly = "SpaceEngineers.Game.dll";
            MyPerGameSettings.GameModObjBuildersAssembly = "SpaceEngineers.ObjectBuilders.dll";
            MyPerGameSettings.OffsetVoxelMapByHalfVoxel = true;
            MyPerGameSettings.EnablePregeneratedAsteroidHack = true;
            if (Game.IsDedicated)
            {
                MySandboxGame.ConfigDedicated = new MyConfigDedicated<MyObjectBuilder_SessionSettings>("SpaceEngineers-Dedicated.cfg");
            }
            MySandboxGame.GameCustomInitialization = new MySpaceGameCustomInitialization();
            MyPerGameSettings.ShowObfuscationStatus = false;
            MyPerGameSettings.UseNewDamageEffects = true;
            MyPerGameSettings.EnableResearch = true;
            MyPerGameSettings.UseVolumeLimiter = MyFakes.ENABLE_NEW_SOUNDS && MyFakes.ENABLE_REALISTIC_LIMITER;
            MyPerGameSettings.UseSameSoundLimiter = true;
            MyPerGameSettings.UseMusicController = true;
            MyPerGameSettings.UseReverbEffect = true;
            MyPerGameSettings.Destruction = false;
            MyMusicTrack track = new MyMusicTrack {
                TransitionCategory = MyStringId.GetOrCompute("NoRandom"),
                MusicCategory = MyStringId.GetOrCompute("MusicMenu")
            };
            MyPerGameSettings.MainMenuTrack = new MyMusicTrack?(track);
            MyPerGameSettings.BallFriendlyPhysics = false;
            MyPerGameSettings.PathfindingType = !MyFakes.ENABLE_CESTMIR_PATHFINDING ? typeof(MyRDPathfinding) : typeof(MyPathfinding);
            MyPerGameSettings.BotFactoryType = typeof(MySpaceBotFactory);
            MyPerGameSettings.ControlMenuInitializerType = typeof(MySpaceControlMenuInitializer);
            MyPerGameSettings.EnableScenarios = true;
            MyPerGameSettings.EnableJumpDrive = true;
            MyPerGameSettings.EnableShipSoundSystem = true;
            MyFakes.ENABLE_PLANETS_JETPACK_LIMIT_IN_CREATIVE = true;
            MyFakes.ENABLE_DRIVING_PARTICLES = true;
            MyPerGameSettings.EnablePathfinding = false;
            MyPerGameSettings.CharacterGravityMultiplier = 2f;
            MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS_HELPERS = true;
            MyPerGameSettings.EnableRagdollInJetpack = true;
            MyPerGameSettings.GUI.OptionsScreen = typeof(MyGuiScreenOptionsSpace);
            MyPerGameSettings.GUI.PerformanceWarningScreen = typeof(MyGuiScreenPerformanceWarnings);
            MyPerGameSettings.GUI.CreateFactionScreen = typeof(MyGuiScreenCreateOrEditFactionSpace);
            MyPerGameSettings.GUI.MainMenu = typeof(MyGuiScreenMainMenu);
            MyPerGameSettings.DefaultGraphicsRenderer = MySandboxGame.DirectX11RendererKey;
            MyPerGameSettings.EnableWelderAutoswitch = true;
            MyPerGameSettings.InventoryMass = true;
            MyPerGameSettings.CompatHelperType = typeof(MySpaceSessionCompatHelper);
            string[] textArray1 = new string[10];
            textArray1[0] = @"Videos\Background01_720p.wmv";
            textArray1[1] = @"Videos\Background02_720p.wmv";
            textArray1[2] = @"Videos\Background03_720p.wmv";
            textArray1[3] = @"Videos\Background04_720p.wmv";
            textArray1[4] = @"Videos\Background05_720p.wmv";
            textArray1[5] = @"Videos\Background09_720p.wmv";
            textArray1[6] = @"Videos\Background10_720p.wmv";
            textArray1[7] = @"Videos\Background11_720p.wmv";
            textArray1[8] = @"Videos\Background12_720p.wmv";
            textArray1[9] = @"Videos\Background13_720p.wmv";
            MyPerGameSettings.GUI.MainMenuBackgroundVideos = textArray1;
            SetupRender();
            FillCredits();
            MyPerGameSettings.VoiceChatEnabled = true;
            MyPerGameSettings.VoiceChatLogic = typeof(MyVoiceChatLogic);
            MyPerGameSettings.ClientStateType = typeof(MySpaceClientState);
            MyVoxelPhysicsBody.UseLod1VoxelPhysics = false;
            MyPerGameSettings.EnableAi = true;
            MyPerGameSettings.EnablePathfinding = true;
            MyFakesLocal.SetupLocalPerGameSettings();
        }

        [Obsolete]
        public static void SetupRender()
        {
            MyRenderProxy.Settings.DrawMergeInstanced = false;
            MyRenderProxy.Settings.UseGeometryArrayTextures = false;
            MyRenderProxy.Settings.HDREnabled = true;
            MyRenderProxy.SwitchRenderSettings(MyRenderProxy.Settings);
        }

        private static void SetupSecrets()
        {
            MyPerGameSettings.GA_Public_GameKey = "27bae5ba5219bcd64ddbf83113eabb30";
            MyPerGameSettings.GA_Public_SecretKey = "d04e0431f97f90fae73b9d6ea99fc9746695bd11";
            MyPerGameSettings.GA_Dev_GameKey = "3a6b6ebdc48552beba3efe173488d8ba";
            MyPerGameSettings.GA_Dev_SecretKey = "caecaaa4a91f6b2598cf8ffb931b3573f20b4343";
            MyPerGameSettings.GA_Pirate_GameKey = "41827f7c8bfed902495e0e27cb57c495";
            MyPerGameSettings.GA_Pirate_SecretKey = "493b7cb3f0a472f940c0ba0c38efbb49e902cbec";
            MyPerGameSettings.GA_Other_GameKey = "4f02769277e62b4344da70967e99a2a0";
            MyPerGameSettings.GA_Other_SecretKey = "7fa773c228ce9534181adcfebf30d18bc6807d2b";
        }
    }
}

