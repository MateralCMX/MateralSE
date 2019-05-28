namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Screens.Helpers.InputRecording;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game.ObjectBuilders.Components;
    using VRageMath;

    public class MyTestingToolHelper
    {
        private static MyTestingToolHelper m_instance;
        private MyBlockTestGenerationState BTGS;
        private bool m_syncRendering;
        private bool m_smallBlock;
        private bool m_isSaving;
        private bool m_savingFinished;
        private bool m_isLoading;
        private bool m_loadingFinished;
        private EnumTestingToolHelperStateOuter m_stateOuter;
        private int m_stateInner;
        private int m_stateMicro;
        private int m_timer;
        private readonly int m_timer_Max = 100;

        private MyTestingToolHelper()
        {
            this.ChangeStateOuter_Force(EnumTestingToolHelperStateOuter.Idle);
        }

        public void Action_SpawnBlockSaveTestReload()
        {
            this.StateOuter = EnumTestingToolHelperStateOuter.Action_1;
        }

        public void Action_Test()
        {
            MyCubeBlockDefinition large = MyDefinitionManager.Static.GetDefinitionGroup("Monolith").Large;
            MyCubeBuilder component = MySession.Static.GetComponent<MyCubeBuilder>();
            if (component != null)
            {
                MatrixD identity = MatrixD.Identity;
                component.AddBlocksToBuildQueueOrSpawn(large, ref identity, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), Quaternion.Identity);
            }
        }

        private void Action1_State0_PrepareBase()
        {
            MySession.Static.Name = MySession.Static.Name.Replace(":", "-") + "_BlockTests";
            this.SaveAs(MySession.Static.Name);
            this.m_stateInner = 1;
        }

        private void Action1_State1_SpawningCyclePreparation()
        {
            if (!this.m_isSaving && this.m_savingFinished)
            {
                this.BTGS = new MyBlockTestGenerationState();
                this.BTGS.BasePath = MySession.Static.CurrentPath;
                this.BTGS.SessionOrder = -1;
                this.BTGS.SessionOrder_Max = MyDefinitionManager.Static.GetDefinitionPairNames().Count * 2;
                this.BTGS.UsedKeys.Clear();
                this.BTGS.CurrentBlockName = string.Empty;
                this.m_smallBlock = false;
                this.m_stateInner = 2;
            }
        }

        private void Action1_State2_SpawningCycle()
        {
            switch (this.m_stateMicro)
            {
                case 0:
                    this.BTGS.SessionOrder++;
                    this.BTGS.TestStart = DateTime.UtcNow;
                    this.Load(this.BTGS.BasePath);
                    this.BTGS.ResultTestCaseName = string.Empty;
                    this.m_stateMicro = 1;
                    return;

                case 1:
                    if (this.ConsumeLoadingCompletion())
                    {
                        if (!this.m_smallBlock)
                        {
                            bool flag = false;
                            foreach (string str in MyDefinitionManager.Static.GetDefinitionPairNames())
                            {
                                if (!this.BTGS.UsedKeys.Contains(str))
                                {
                                    this.BTGS.CurrentBlockName = str;
                                    this.m_smallBlock = false;
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                this.m_stateInner = 3;
                                return;
                            }
                        }
                        MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.BTGS.CurrentBlockName);
                        MyCubeBlockDefinition def = null;
                        if (definitionGroup != null)
                        {
                            if (this.m_smallBlock)
                            {
                                def = definitionGroup.Small;
                            }
                            else
                            {
                                def = definitionGroup.Large;
                                this.BTGS.UsedKeys.Add(this.BTGS.CurrentBlockName);
                            }
                        }
                        if (def == null)
                        {
                            this.m_stateMicro = 0;
                        }
                        else
                        {
                            this.SpawnBlockAtCenter(def);
                            this.BTGS.ResultSaveName = MySession.Static.Name + "_" + this.BTGS.CurrentBlockName + (this.m_smallBlock ? "_S" : "_L");
                            this.BTGS.ResultTestCaseName = this.BTGS.ResultSaveName + (this.m_syncRendering ? "-sync" : "-async");
                            this.m_stateMicro = 2;
                        }
                        this.m_smallBlock = !this.m_smallBlock;
                    }
                    return;

                case 2:
                    this.SaveAs(this.BTGS.ResultSaveName);
                    this.m_stateMicro = 3;
                    return;

                case 3:
                    if (this.ConsumeSavingCompletion())
                    {
                        if (this.m_syncRendering)
                        {
                            AddSyncRenderingToCfg(this.m_syncRendering, null);
                        }
                        this.BTGS.SourceSaveWithBlockPath = MySession.Static.CurrentPath;
                        this.BTGS.ResultTestCaseSavePath = Path.Combine(TestCasesDir, this.BTGS.ResultTestCaseName, Path.GetFileName(this.BTGS.SourceSaveWithBlockPath));
                        Directory.CreateDirectory(Path.Combine(TestCasesDir, this.BTGS.ResultTestCaseName));
                        DirectoryCopy(MySession.Static.CurrentPath, this.BTGS.ResultTestCaseSavePath, true);
                        File.Copy(ConfigFile, Path.Combine(TestCasesDir, this.BTGS.ResultTestCaseName, "SpaceEngineers.cfg"), true);
                        this.m_stateMicro = 4;
                    }
                    return;

                case 4:
                    MySessionComponentReplay.Static.StartReplay();
                    this.m_stateMicro = 5;
                    return;

                case 5:
                    PerFrameData data;
                    if (MySession.Static == null)
                    {
                        this.ClearTimer();
                        this.m_stateInner = 3;
                        return;
                    }
                    if (MySessionComponentReplay.Static.IsEntityBeingReplayed(MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).EntityId, out data))
                    {
                        this.ClearTimer();
                        return;
                    }
                    MySessionComponentReplay.Static.StopReplay();
                    this.m_stateMicro = 6;
                    return;

                case 6:
                    this.SaveAs(Path.Combine(MyFileSystem.SavesPath, "..", "TestingToolSave"));
                    MyGuiSandbox.TakeScreenshot(MySandboxGame.ScreenSize.X, MySandboxGame.ScreenSize.Y, Path.Combine(Path.Combine(MyFileSystem.UserDataPath, "Screenshots"), "TestingToolResult.png"), true, false);
                    this.m_stateMicro = 7;
                    return;

                case 7:
                    if (this.ConsumeSavingCompletion())
                    {
                        CopyScreenshots(this.BTGS.ResultTestCaseName, this.BTGS.TestStart, true);
                        CopyLastGamelog(this.BTGS.ResultTestCaseName, "result.log");
                        this.CopyLastSave(this.BTGS.ResultTestCaseName, "result");
                        MyInputRecording recording1 = new MyInputRecording();
                        recording1.Name = Path.Combine(TestCasesDir, this.BTGS.ResultTestCaseName);
                        recording1.Description = this.BTGS.SourceSaveWithBlockPath;
                        recording1.Session = MyInputRecordingSession.Specific;
                        recording1.SetStartingScreenDimensions(MySandboxGame.ScreenSize.X, MySandboxGame.ScreenSize.Y);
                        recording1.UseReplayInstead = true;
                        recording1.Save();
                        this.m_stateMicro = 0;
                    }
                    return;
            }
        }

        private void Action1_State3_Finish()
        {
            this.BTGS = null;
            this.StateOuter = EnumTestingToolHelperStateOuter.Idle;
        }

        public static void AddSyncRenderingToCfg(bool value, string cfgPath = null)
        {
            string[] source = File.ReadAllLines(ConfigFile);
            bool flag = false;
            int index = 0;
            while (true)
            {
                if (index < source.Length)
                {
                    flag = source[index].Contains("SyncRendering");
                    if (!flag)
                    {
                        index++;
                        continue;
                    }
                    source[index + 1] = "      <Value xsi:type=\"xsd:string\">" + value.ToString() + "</Value>";
                }
                if (!flag)
                {
                    List<string> list1 = source.ToList<string>();
                    list1.Insert(list1.Count - 2, "    <item>");
                    list1.Insert(list1.Count - 2, "      <Key xsi:type=\"xsd:string\">SyncRendering</Key>");
                    list1.Insert(list1.Count - 2, "      <Value xsi:type=\"xsd:string\">" + value.ToString() + "</Value>");
                    list1.Insert(list1.Count - 2, "    </item>");
                    source = list1.ToArray();
                }
                File.Delete((cfgPath != null) ? cfgPath : ConfigFile);
                File.WriteAllLines((cfgPath != null) ? cfgPath : ConfigFile, source);
                return;
            }
        }

        public bool CanChangeOuterStateTo(EnumTestingToolHelperStateOuter value)
        {
            if (this.m_stateOuter == value)
            {
                return false;
            }
            switch (this.m_stateOuter)
            {
                case EnumTestingToolHelperStateOuter.Disabled:
                    return false;

                case EnumTestingToolHelperStateOuter.Idle:
                    return true;

                case EnumTestingToolHelperStateOuter.Action_1:
                    return ((value == EnumTestingToolHelperStateOuter.Idle) || (value == EnumTestingToolHelperStateOuter.Disabled));
            }
            return true;
        }

        private void ChangeStateOuter_Force(EnumTestingToolHelperStateOuter value)
        {
            if (this.m_stateOuter != value)
            {
                this.m_stateOuter = value;
                this.OnStateOuterUpdate();
            }
        }

        private void ClearTimer()
        {
            this.m_timer = 0;
        }

        private bool ConsumeLoadingCompletion()
        {
            if (this.m_isLoading || !this.m_loadingFinished)
            {
                return false;
            }
            this.m_loadingFinished = false;
            return true;
        }

        private bool ConsumeSavingCompletion()
        {
            if (this.m_isSaving || !this.m_savingFinished)
            {
                return false;
            }
            this.m_savingFinished = false;
            return true;
        }

        public static void CopyLastGamelog(string testFolder, string resultType)
        {
            File.Copy(GameLogPath, Path.Combine(TestCasesDir, testFolder, resultType), true);
        }

        public void CopyLastSave(string testCasePath, string resultName)
        {
            string str = Path.Combine(UserSaveFolder, "TestingToolSave");
            string str2 = Path.Combine(TestCasesDir, testCasePath);
            if (File.Exists(Path.Combine(str, "Sandbox.sbc")))
            {
                File.Copy(Path.Combine(str, "Sandbox.sbc"), Path.Combine(str2, resultName + ".sbc"), true);
                File.Copy(Path.Combine(str, "SANDBOX_0_0_0_.sbs"), Path.Combine(str2, resultName + ".sbs"), true);
            }
            string[] paths = new string[] { str };
            if (Directory.Exists(Path.Combine(paths)))
            {
                string[] textArray2 = new string[] { str };
                new DirectoryInfo(Path.Combine(textArray2)).Delete(true);
            }
        }

        public static void CopyScreenshots(string testFolder, DateTime startTime, bool isAddCase = false)
        {
            List<string> list = new List<string>();
            int num = 0;
            foreach (FileInfo info in new DirectoryInfo(ScreenshotsDir).GetFiles())
            {
                if (info.LastWriteTime >= startTime)
                {
                    list.Add(info.FullName);
                    File.Copy(info.FullName, Path.Combine(TestCasesDir, testFolder, LastTestRunResultFilename + num + ".png"), true);
                    if (isAddCase)
                    {
                        File.Copy(info.FullName, Path.Combine(TestCasesDir, testFolder, TestResultFilename + num + ".png"), true);
                    }
                    File.Delete(info.FullName);
                    num++;
                }
            }
            string path = Path.Combine(ScreenshotsDir, "TestingToolResult.png");
            if (File.Exists(path))
            {
                File.Copy(path, Path.Combine(TestCasesDir, testFolder, LastTestRunResultFilename + ".png"), true);
                File.Delete(path);
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo info1 = new DirectoryInfo(sourceDirName);
            if (!info1.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }
            DirectoryInfo local1 = info1;
            DirectoryInfo[] directories = local1.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            foreach (FileInfo info in local1.GetFiles())
            {
                string destFileName = Path.Combine(destDirName, info.Name);
                info.CopyTo(destFileName, true);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo info2 in directories)
                {
                    string str2 = Path.Combine(destDirName, info2.Name);
                    DirectoryCopy(info2.FullName, str2, copySubDirs);
                }
            }
        }

        private bool FakeLoadCompletion()
        {
            if (this.m_isLoading)
            {
                return false;
            }
            this.m_loadingFinished = true;
            return true;
        }

        private bool FakeSaveCompletion()
        {
            if (this.m_isSaving)
            {
                return false;
            }
            this.m_savingFinished = true;
            return true;
        }

        private bool Load(string path)
        {
            if (this.m_isLoading)
            {
                return false;
            }
            this.m_isLoading = true;
            MyOnlineModeEnum? onlineMode = null;
            MySessionLoader.LoadSingleplayerSession(path, delegate {
                this.OnLoadComplete();
            }, null, onlineMode, 0);
            return true;
        }

        public void OnLoadComplete()
        {
            this.m_loadingFinished = true;
            this.m_isLoading = false;
        }

        public void OnSaveAsComplete()
        {
            this.m_savingFinished = true;
            this.m_isSaving = false;
        }

        public void OnStateOuterUpdate()
        {
            this.IsEnabled = this.m_stateOuter != EnumTestingToolHelperStateOuter.Disabled;
            this.IsIdle = this.m_stateOuter == EnumTestingToolHelperStateOuter.Idle;
            this.NeedsUpdate = this.IsEnabled && !this.IsIdle;
        }

        private bool SaveAs(string name)
        {
            if (this.m_isSaving)
            {
                return false;
            }
            this.m_isSaving = true;
            MyAsyncSaving.Start(delegate {
                this.OnSaveAsComplete();
            }, name, false);
            return true;
        }

        private bool SpawnBlockAtCenter(MyCubeBlockDefinition def)
        {
            MyCubeBuilder component = MySession.Static.GetComponent<MyCubeBuilder>();
            return ((component != null) ? component.AddBlocksToBuildQueueOrSpawn(def, ref MatrixD.Identity, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), Quaternion.Identity) : false);
        }

        public void Update()
        {
            if (this.NeedsUpdate)
            {
                this.m_timer--;
                if (this.m_timer < 0)
                {
                    this.m_timer = this.m_timer_Max;
                    switch (this.StateOuter)
                    {
                        case EnumTestingToolHelperStateOuter.Disabled:
                        case EnumTestingToolHelperStateOuter.Idle:
                            break;

                        case EnumTestingToolHelperStateOuter.Action_1:
                            this.Update_Action1();
                            break;

                        default:
                            return;
                    }
                }
            }
        }

        private void Update_Action1()
        {
            switch (this.m_stateInner)
            {
                case 0:
                    this.Action1_State0_PrepareBase();
                    return;

                case 1:
                    this.Action1_State1_SpawningCyclePreparation();
                    return;

                case 2:
                    this.Action1_State2_SpawningCycle();
                    return;

                case 3:
                    this.Action1_State3_Finish();
                    return;
            }
        }

        public static MyTestingToolHelper Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyTestingToolHelper();
                }
                return m_instance;
            }
        }

        public EnumTestingToolHelperStateOuter StateOuter
        {
            get => 
                this.m_stateOuter;
            set
            {
                if (this.CanChangeOuterStateTo(value))
                {
                    this.m_stateOuter = value;
                    this.m_stateInner = 0;
                    this.m_stateMicro = 0;
                    this.OnStateOuterUpdate();
                }
            }
        }

        public bool IsEnabled { get; private set; }

        public bool IsIdle { get; private set; }

        public bool NeedsUpdate { get; private set; }

        public static string ScreenshotsDir
        {
            get
            {
                string str = "SpaceEngineers";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), str, "Screenshots");
            }
        }

        private static string UserSaveFolder
        {
            get
            {
                string str = "SpaceEngineers";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), str + (false ? "Dedicated" : ""), "Saves");
            }
        }

        private static string TestCasesDir
        {
            get
            {
                string str = "SpaceEngineers";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), str + (false ? "Dedicated" : ""), "TestCases");
            }
        }

        public static string GameLogPath
        {
            get
            {
                bool flag = false;
                string str = "SpaceEngineers";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), str + (flag ? "Dedicated" : ""), str + (flag ? "Dedicated" : "") + ".log");
            }
        }

        public static string ConfigFile
        {
            get
            {
                string str = "SpaceEngineers";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), str, str + ".cfg");
            }
        }

        public static string TestResultFilename =>
            "result";

        public static string LastTestRunResultFilename =>
            "last_test_run";

        public enum EnumTestingToolHelperStateOuter
        {
            Disabled,
            Idle,
            Action_1
        }

        protected class MyBlockTestGenerationState
        {
            public int SessionOrder;
            public int SessionOrder_Max;
            public List<string> UsedKeys = new List<string>();
            public DateTime TestStart = DateTime.UtcNow;
            public string CurrentBlockName = string.Empty;
            public string BasePath = string.Empty;
            public string ResultSaveName = string.Empty;
            public string ResultTestCaseName = string.Empty;
            public string SourceSaveWithBlockPath = string.Empty;
            public string ResultTestCaseSavePath = string.Empty;
        }
    }
}

