namespace Sandbox.Game
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Localization;
    using VRage.Game.ObjectBuilders.Campaign;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyCampaignManager
    {
        private const string CAMPAIGN_CONTENT_RELATIVE_PATH = "Campaigns";
        private readonly string m_scenariosContentRelativePath = "Scenarios";
        private readonly string m_scenarioFileExtension = "*.scf";
        private const string CAMPAIGN_DEBUG_RELATIVE_PATH = @"Worlds\Campaigns";
        private static MyCampaignManager m_instance;
        private string m_activeCampaignName;
        private MyObjectBuilder_Campaign m_activeCampaign;
        private readonly Dictionary<string, List<MyObjectBuilder_Campaign>> m_campaignsByNames = new Dictionary<string, List<MyObjectBuilder_Campaign>>();
        private readonly List<string> m_activeCampaignLevelNames = new List<string>();
        private Dictionary<string, MyLocalization.MyBundle> m_campaignMenuLocalizationBundle = new Dictionary<string, MyLocalization.MyBundle>();
        private readonly HashSet<MyLocalizationContext> m_campaignLocContexts = new HashSet<MyLocalizationContext>();
        private MyLocalization.MyBundle? m_currentMenuBundle;
        private readonly List<MyWorkshopItem> m_subscribedCampaignItems = new List<MyWorkshopItem>();
        private Task m_refreshTask;
        public static Action AfterCampaignLocalizationsLoaded;
        [CompilerGenerated]
        private Action OnCampaignFinished;

        public event Action OnCampaignFinished
        {
            [CompilerGenerated] add
            {
                Action onCampaignFinished = this.OnCampaignFinished;
                while (true)
                {
                    Action a = onCampaignFinished;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onCampaignFinished = Interlocked.CompareExchange<Action>(ref this.OnCampaignFinished, action3, a);
                    if (ReferenceEquals(onCampaignFinished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onCampaignFinished = this.OnCampaignFinished;
                while (true)
                {
                    Action source = onCampaignFinished;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onCampaignFinished = Interlocked.CompareExchange<Action>(ref this.OnCampaignFinished, action3, source);
                    if (ReferenceEquals(onCampaignFinished, source))
                    {
                        return;
                    }
                }
            }
        }

        public bool DownloadCampaign(ulong publisherFileId)
        {
            MyWorkshop.ResultData data = new MyWorkshop.ResultData {
                Success = false
            };
            MyWorkshopItem item1 = new MyWorkshopItem();
            item1.Id = publisherFileId;
            MyWorkshopItem[] collection = new MyWorkshopItem[] { item1 };
            data = MyWorkshop.DownloadModsBlockingUGC(new List<MyWorkshopItem>(collection), null);
            if (!data.Success || (data.Mods.Count == 0))
            {
                return false;
            }
            this.RegisterWorshopModDataUGC(data.Mods[0]);
            return true;
        }

        private MyObjectBuilder_CampaignSMNode FindStartingState()
        {
            if (this.m_activeCampaign != null)
            {
                bool flag = false;
                MyObjectBuilder_CampaignSMNode[] nodes = this.m_activeCampaign.StateMachine.Nodes;
                int index = 0;
                while (index < nodes.Length)
                {
                    MyObjectBuilder_CampaignSMNode node = nodes[index];
                    MyObjectBuilder_CampaignSMTransition[] transitions = this.m_activeCampaign.StateMachine.Transitions;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 < transitions.Length)
                        {
                            if (transitions[num2].To != node.Name)
                            {
                                num2++;
                                continue;
                            }
                            flag = true;
                        }
                        if (!flag)
                        {
                            return node;
                        }
                        flag = false;
                        index++;
                        break;
                    }
                }
            }
            return null;
        }

        private string GetModFolderPath(string path)
        {
            int index = path.IndexOf("Campaigns", StringComparison.InvariantCulture);
            if (index == -1)
            {
                index = path.IndexOf(this.m_scenariosContentRelativePath, StringComparison.InvariantCulture);
            }
            return path.Remove(index - 1);
        }

        public void Init()
        {
            MyLocalization.Static.InitLoader(new Action(this.LoadCampaignLocalization));
            MySandboxGame.Log.WriteLine("MyCampaignManager.Constructor() - START");
            using (IEnumerator<string> enumerator = MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, this.m_scenariosContentRelativePath), this.m_scenarioFileExtension, MySearchOption.AllDirectories).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyObjectBuilder_VSFiles files;
                    if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(enumerator.Current, out files))
                    {
                        continue;
                    }
                    if (files.Campaign != null)
                    {
                        files.Campaign.IsVanilla = true;
                        files.Campaign.IsLocalMod = false;
                        this.LoadCampaignData(files.Campaign);
                    }
                }
            }
            MySandboxGame.Log.WriteLine("MyCampaignManager.Constructor() - END");
        }

        private void LoadCampaignData(MyObjectBuilder_Campaign campaignOb)
        {
            if (!this.m_campaignsByNames.ContainsKey(campaignOb.Name))
            {
                this.m_campaignsByNames.Add(campaignOb.Name, new List<MyObjectBuilder_Campaign>());
                this.m_campaignsByNames[campaignOb.Name].Add(campaignOb);
            }
            else
            {
                List<MyObjectBuilder_Campaign> list = this.m_campaignsByNames[campaignOb.Name];
                using (List<MyObjectBuilder_Campaign>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_Campaign current = enumerator.Current;
                        if ((current.IsLocalMod == campaignOb.IsLocalMod) && ((current.IsMultiplayer == campaignOb.IsMultiplayer) && ((current.IsVanilla == campaignOb.IsVanilla) && (current.PublishedFileId == campaignOb.PublishedFileId))))
                        {
                            return;
                        }
                    }
                }
                list.Add(campaignOb);
            }
            if (!string.IsNullOrEmpty(campaignOb.DescriptionLocalizationFile))
            {
                FileInfo info = new FileInfo(Path.Combine(campaignOb.ModFolderPath ?? MyFileSystem.ContentPath, campaignOb.DescriptionLocalizationFile));
                if (info.Exists)
                {
                    string str = string.IsNullOrEmpty(campaignOb.ModFolderPath) ? campaignOb.Name : Path.Combine(campaignOb.ModFolderPath, campaignOb.Name);
                    MyLocalization.MyBundle bundle = new MyLocalization.MyBundle {
                        BundleId = MyStringId.GetOrCompute(str),
                        FilePaths = new List<string>()
                    };
                    string[] strArray = Directory.GetFiles(info.Directory.FullName, Path.GetFileNameWithoutExtension(info.Name) + "*.sbl", SearchOption.TopDirectoryOnly);
                    int index = 0;
                    while (true)
                    {
                        if (index >= strArray.Length)
                        {
                            if (!this.m_campaignMenuLocalizationBundle.ContainsKey(str))
                            {
                                this.m_campaignMenuLocalizationBundle.Add(str, bundle);
                                break;
                            }
                            this.m_campaignMenuLocalizationBundle[str] = bundle;
                            return;
                        }
                        string item = strArray[index];
                        if (!bundle.FilePaths.Contains(item))
                        {
                            bundle.FilePaths.Add(item);
                        }
                        index++;
                    }
                }
            }
        }

        public void LoadCampaignLocalization()
        {
            string name = MyLanguage.CurrentCulture.Name;
            if (MySession.Static != null)
            {
                MyLocalizationSessionComponent component = MySession.Static.GetComponent<MyLocalizationSessionComponent>();
                if ((component != null) && (this.m_activeCampaign != null))
                {
                    component.LoadCampaignLocalization(this.m_activeCampaign.LocalizationPaths, this.m_activeCampaign.ModFolderPath);
                    component.SwitchLanguage(name);
                }
            }
        }

        private void LoadScenarioFile(string modFile)
        {
            MyObjectBuilder_VSFiles files;
            if (MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(modFile, out files) && (files.Campaign != null))
            {
                files.Campaign.IsVanilla = false;
                files.Campaign.IsLocalMod = true;
                files.Campaign.ModFolderPath = this.GetModFolderPath(modFile);
                this.LoadCampaignData(files.Campaign);
            }
        }

        private void LoadScenarioMod(MyWorkshopItem mod, IEnumerable<string> visualScriptingFiles)
        {
            foreach (string str in visualScriptingFiles)
            {
                MyObjectBuilder_VSFiles files;
                if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(str, out files))
                {
                    continue;
                }
                if (files.Campaign != null)
                {
                    files.Campaign.IsVanilla = false;
                    files.Campaign.IsLocalMod = false;
                    files.Campaign.PublishedFileId = mod.Id;
                    files.Campaign.ModFolderPath = this.GetModFolderPath(str);
                    this.LoadCampaignData(files.Campaign);
                }
            }
        }

        public void LoadSessionFromActiveCampaign(string relativePath, Action afterLoad = null, string campaignDirectoryName = null, string campaignName = null, MyOnlineModeEnum onlineMode = 0, int maxPlayers = 0)
        {
            string str2;
            string str = relativePath;
            if (this.m_activeCampaign.IsVanilla || this.m_activeCampaign.IsDebug)
            {
                str2 = Path.Combine(MyFileSystem.ContentPath, str);
                if (!MyFileSystem.FileExists(str2))
                {
                    MySandboxGame.Log.WriteLine("ERROR: Missing vanilla world file in campaign: " + this.m_activeCampaignName);
                    return;
                }
            }
            else
            {
                str2 = Path.Combine(this.m_activeCampaign.ModFolderPath, str);
                if (!MyFileSystem.FileExists(str2))
                {
                    str2 = Path.Combine(MyFileSystem.ContentPath, str);
                    if (!MyFileSystem.FileExists(str2))
                    {
                        MySandboxGame.Log.WriteLine("ERROR: Missing world file in campaign: " + this.m_activeCampaignName);
                        return;
                    }
                }
            }
            if (string.IsNullOrEmpty(campaignDirectoryName))
            {
                campaignDirectoryName = this.ActiveCampaignName + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            }
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(str2));
            string path = Path.Combine(MyFileSystem.SavesPath, campaignDirectoryName, info.Name);
            while (MyFileSystem.DirectoryExists(path))
            {
                path = Path.Combine(MyFileSystem.SavesPath, campaignDirectoryName, info.Name + " " + MyUtils.GetRandomInt(0x7fffffff).ToString("########"));
            }
            if (!File.Exists(str2))
            {
                string destinationDirectoryName = Path.Combine(Path.GetTempPath(), "TMP_CAMPAIGN_MOD_FOLDER");
                MyZipArchive.ExtractToDirectory(this.m_activeCampaign.ModFolderPath, destinationDirectoryName);
                MyUtils.CopyDirectory(Path.Combine(destinationDirectoryName, Path.GetDirectoryName(relativePath)), path);
                Directory.Delete(destinationDirectoryName, true);
            }
            else
            {
                MyUtils.CopyDirectory(info.FullName, path);
                if (this.m_activeCampaign != null)
                {
                    MyObjectBuilder_Checkpoint checkpoint;
                    string str4 = Path.Combine(path, Path.GetFileName(str2));
                    if (MyFileSystem.FileExists(str4) && MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Checkpoint>(str4, out checkpoint))
                    {
                        foreach (MyObjectBuilder_LocalizationSessionComponent component in checkpoint.SessionComponents)
                        {
                            if (component != null)
                            {
                                component.CampaignModFolderName = this.m_activeCampaign.ModFolderPath;
                                break;
                            }
                        }
                        MyObjectBuilderSerializer.SerializeXML(str4, false, checkpoint, null);
                    }
                }
            }
            string currentLanguage = MyLanguage.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(currentLanguage))
            {
                afterLoad = (Action) Delegate.Combine(afterLoad, delegate {
                    MyLocalizationSessionComponent component = MySession.Static.GetComponent<MyLocalizationSessionComponent>();
                    component.LoadCampaignLocalization(this.m_activeCampaign.LocalizationPaths, this.m_activeCampaign.ModFolderPath);
                    component.SwitchLanguage(currentLanguage);
                    if (AfterCampaignLocalizationsLoaded != null)
                    {
                        AfterCampaignLocalizationsLoaded();
                    }
                });
            }
            afterLoad = (Action) Delegate.Combine(afterLoad, delegate {
                MySession.Static.Save(null);
                this.IsNewCampaignLevelLoading = false;
            });
            this.IsNewCampaignLevelLoading = true;
            if (MyLocalization.Static != null)
            {
                MyLocalization.Static.DisposeAll();
            }
            MySessionLoader.LoadSingleplayerSession(path, afterLoad, campaignName, new MyOnlineModeEnum?(onlineMode), maxPlayers);
        }

        public void NotifyCampaignFinished()
        {
            Action onCampaignFinished = this.OnCampaignFinished;
            if (onCampaignFinished != null)
            {
                onCampaignFinished();
            }
        }

        private void OnPublishFinished(bool publishSuccess, MyGameServiceCallResult publishResult, ulong publishedFileId)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            if (publishSuccess)
            {
                MyWorkshop.GenerateModInfo(this.m_activeCampaign.ModFolderPath, publishedFileId, Sync.MyId);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextCampaignPublished), MySession.Platform)), MyTexts.Get(MyCommonTexts.MessageBoxCaptionCampaignPublished), nullable, nullable, nullable, nullable, a => MyGameService.OpenOverlayUrl($"http://steamcommunity.com/sharedfiles/filedetails/?id={publishedFileId}"), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            else
            {
                StringBuilder messageText = (publishResult != MyGameServiceCallResult.AccessDenied) ? new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublishFailed), MySession.Platform) : MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_AccessDenied);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionModCampaignPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
        }

        public void PublishActive()
        {
            ulong workshopIdFromLocalMod = MyWorkshop.GetWorkshopIdFromLocalMod(this.m_activeCampaign.ModFolderPath);
            string[] tags = new string[] { "campaign" };
            MyWorkshop.PublishModAsync(this.m_activeCampaign.ModFolderPath, this.m_activeCampaign.Name, this.m_activeCampaign.Description, workshopIdFromLocalMod, tags, MyPublishedFileVisibility.Public, new Action<bool, MyGameServiceCallResult, ulong>(this.OnPublishFinished));
        }

        private void RefreshLocalModData()
        {
            string[] directories = Directory.GetDirectories(MyFileSystem.ModsPath);
            using (Dictionary<string, List<MyObjectBuilder_Campaign>>.ValueCollection.Enumerator enumerator = this.m_campaignsByNames.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.RemoveAll(campaign => campaign.IsLocalMod);
                }
            }
            foreach (string str in directories)
            {
                this.RegisterLocalModData(str);
            }
        }

        public Task RefreshModData()
        {
            Task task;
            if (!this.m_refreshTask.IsComplete)
            {
                return this.m_refreshTask;
            }
            this.m_refreshTask = task = Parallel.Start(delegate {
                this.RefreshLocalModData();
                this.RefreshSubscribedModData();
            });
            return task;
        }

        private void RefreshSubscribedModData()
        {
            if (MyWorkshop.GetSubscribedCampaignsBlocking(this.m_subscribedCampaignItems))
            {
                List<MyObjectBuilder_Campaign> list = new List<MyObjectBuilder_Campaign>();
                using (Dictionary<string, List<MyObjectBuilder_Campaign>>.ValueCollection.Enumerator enumerator = this.m_campaignsByNames.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        List<MyObjectBuilder_Campaign> campaignList;
                        foreach (MyObjectBuilder_Campaign campaign in campaignList)
                        {
                            if (campaign.PublishedFileId != 0)
                            {
                                bool flag = false;
                                int index = 0;
                                while (true)
                                {
                                    if (index < this.m_subscribedCampaignItems.Count)
                                    {
                                        if (this.m_subscribedCampaignItems[index].Id != campaign.PublishedFileId)
                                        {
                                            index++;
                                            continue;
                                        }
                                        this.m_subscribedCampaignItems.RemoveAtFast<MyWorkshopItem>(index);
                                        flag = true;
                                    }
                                    if (!flag)
                                    {
                                        list.Add(campaign);
                                    }
                                    break;
                                }
                            }
                        }
                        list.ForEach(campaignToRemove => campaignList.Remove(campaignToRemove));
                        list.Clear();
                    }
                }
                MyWorkshop.DownloadModsBlockingUGC(this.m_subscribedCampaignItems, null);
                foreach (MyWorkshopItem item in this.m_subscribedCampaignItems)
                {
                    this.RegisterWorshopModDataUGC(item);
                }
            }
        }

        private void RegisterLocalModData(string localModPath)
        {
            foreach (string str in MyFileSystem.GetFiles(Path.Combine(localModPath, "Campaigns"), "*.vs", MySearchOption.TopDirectoryOnly))
            {
                this.LoadScenarioFile(str);
            }
            foreach (string str2 in MyFileSystem.GetFiles(Path.Combine(localModPath, this.m_scenariosContentRelativePath), this.m_scenarioFileExtension, MySearchOption.AllDirectories))
            {
                this.LoadScenarioFile(str2);
            }
        }

        private void RegisterWorshopModDataUGC(MyWorkshopItem mod)
        {
            string folder = mod.Folder;
            IEnumerable<string> visualScriptingFiles = MyFileSystem.GetFiles(folder, "*.vs", MySearchOption.AllDirectories);
            this.LoadScenarioMod(mod, visualScriptingFiles);
            IEnumerable<string> enumerable2 = MyFileSystem.GetFiles(folder, this.m_scenarioFileExtension, MySearchOption.AllDirectories);
            this.LoadScenarioMod(mod, enumerable2);
        }

        public void ReloadMenuLocalization(string name)
        {
            if (this.m_currentMenuBundle != null)
            {
                MyLocalization.Static.UnloadBundle(this.m_currentMenuBundle.Value.BundleId);
                this.m_campaignLocContexts.Clear();
            }
            if (this.m_campaignMenuLocalizationBundle.ContainsKey(name))
            {
                this.m_currentMenuBundle = new MyLocalization.MyBundle?(this.m_campaignMenuLocalizationBundle[name]);
                if (this.m_currentMenuBundle != null)
                {
                    MyLocalization.Static.LoadBundle(this.m_currentMenuBundle.Value, this.m_campaignLocContexts, false);
                    using (HashSet<MyLocalizationContext>.Enumerator enumerator = this.m_campaignLocContexts.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Switch(MyLanguage.CurrentCulture.Name);
                        }
                    }
                }
            }
        }

        public void RunNewCampaign(string campaignName, MyOnlineModeEnum onlineMode, int maxPlayers)
        {
            MyObjectBuilder_CampaignSMNode node = this.FindStartingState();
            if (node != null)
            {
                this.LoadSessionFromActiveCampaign(node.SaveFilePath, null, null, campaignName, onlineMode, maxPlayers);
            }
        }

        public bool SwitchCampaign(string name, bool isVanilla = true, ulong publisherFileId = 0UL, string localModFolder = null)
        {
            if (this.m_campaignsByNames.ContainsKey(name))
            {
                using (List<MyObjectBuilder_Campaign>.Enumerator enumerator = this.m_campaignsByNames[name].GetEnumerator())
                {
                    while (true)
                    {
                        bool flag;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_Campaign current = enumerator.Current;
                        if (((current.IsVanilla != isVanilla) || (current.IsLocalMod != ((localModFolder != null) && (publisherFileId == 0L)))) || (current.PublishedFileId != publisherFileId))
                        {
                            if ((publisherFileId != 0) || (current.PublishedFileId == 0))
                            {
                                continue;
                            }
                            publisherFileId = current.PublishedFileId;
                            flag = true;
                        }
                        else
                        {
                            this.m_activeCampaign = current;
                            this.m_activeCampaignName = name;
                            this.m_activeCampaignLevelNames.Clear();
                            MyObjectBuilder_CampaignSMNode[] nodes = this.m_activeCampaign.StateMachine.Nodes;
                            int index = 0;
                            while (true)
                            {
                                if (index >= nodes.Length)
                                {
                                    flag = true;
                                    break;
                                }
                                MyObjectBuilder_CampaignSMNode node = nodes[index];
                                this.m_activeCampaignLevelNames.Add(node.Name);
                                index++;
                            }
                        }
                        return flag;
                    }
                }
            }
            if (publisherFileId != 0)
            {
                return (this.DownloadCampaign(publisherFileId) && this.SwitchCampaign(name, isVanilla, publisherFileId, localModFolder));
            }
            if ((isVanilla || (localModFolder == null)) || !MyFileSystem.DirectoryExists(localModFolder))
            {
                return false;
            }
            this.RegisterLocalModData(localModFolder);
            return this.SwitchCampaign(name, isVanilla, publisherFileId, localModFolder);
        }

        public static MyCampaignManager Static
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyCampaignManager();
                }
                return m_instance;
            }
        }

        public IEnumerable<MyObjectBuilder_Campaign> Campaigns
        {
            get
            {
                List<MyObjectBuilder_Campaign> list = new List<MyObjectBuilder_Campaign>();
                foreach (List<MyObjectBuilder_Campaign> list2 in this.m_campaignsByNames.Values)
                {
                    list.AddRange(list2);
                }
                return list;
            }
        }

        public IEnumerable<string> CampaignNames =>
            this.m_campaignsByNames.Keys;

        public IEnumerable<string> ActiveCampaignLevels =>
            this.m_activeCampaignLevelNames;

        public string ActiveCampaignName =>
            this.m_activeCampaignName;

        public MyObjectBuilder_Campaign ActiveCampaign =>
            this.m_activeCampaign;

        public bool IsCampaignRunning
        {
            get
            {
                if (MySession.Static == null)
                {
                    return false;
                }
                MyCampaignSessionComponent component = MySession.Static.GetComponent<MyCampaignSessionComponent>();
                return ((component != null) ? component.Running : false);
            }
        }

        public IEnumerable<string> LocalizationLanguages =>
            ((this.m_activeCampaign != null) ? this.m_activeCampaign.LocalizationLanguages : null);

        public bool IsNewCampaignLevelLoading { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCampaignManager.<>c <>9 = new MyCampaignManager.<>c();
            public static Predicate<MyObjectBuilder_Campaign> <>9__40_0;

            internal bool <RefreshLocalModData>b__40_0(MyObjectBuilder_Campaign campaign) => 
                campaign.IsLocalMod;
        }
    }
}

