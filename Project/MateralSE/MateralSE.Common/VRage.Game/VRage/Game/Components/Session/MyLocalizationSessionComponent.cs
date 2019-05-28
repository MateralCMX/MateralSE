namespace VRage.Game.Components.Session
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Localization;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x29a, typeof(MyObjectBuilder_LocalizationSessionComponent), (Type) null)]
    public class MyLocalizationSessionComponent : MySessionComponentBase
    {
        public static readonly string MOD_BUNDLE_NAME = "MySession - Mod Bundle";
        public static readonly string CAMPAIGN_BUNDLE_NAME = "MySession - Campaing Bundle";
        private string m_language;
        private MyContentPath m_campaignModFolder;
        private MyLocalization.MyBundle m_modBundle;
        private MyLocalization.MyBundle m_campaignBundle;
        private readonly HashSet<MyLocalizationContext> m_influencedContexts = new HashSet<MyLocalizationContext>();

        public MyLocalizationSessionComponent()
        {
            this.m_modBundle.BundleId = MyStringId.GetOrCompute(MOD_BUNDLE_NAME);
            this.m_campaignBundle.BundleId = MyStringId.GetOrCompute(CAMPAIGN_BUNDLE_NAME);
            this.m_campaignBundle.FilePaths = new List<string>();
            this.m_modBundle.FilePaths = new List<string>();
        }

        public override void BeforeStart()
        {
            foreach (MyObjectBuilder_Checkpoint.ModItem item in base.Session.Mods)
            {
                string path = item.GetPath();
                try
                {
                    IEnumerator<string> enumerator = MyFileSystem.GetFiles(path, "*.sbl", MySearchOption.AllDirectories).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            this.m_modBundle.FilePaths.Add(current);
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
                catch (Exception exception)
                {
                    object[] objArray1 = new object[] { "MyLocalizationSessionComponent: Problem deserializing ", path, "\n", exception };
                    MyLog.Default.WriteLine(string.Concat(objArray1));
                }
            }
            MyLocalization.Static.LoadBundle(this.m_modBundle, this.m_influencedContexts, true);
            this.SwitchLanguage(this.m_language);
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_LocalizationSessionComponent objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_LocalizationSessionComponent;
            if (objectBuilder != null)
            {
                objectBuilder.Language = this.m_language;
                objectBuilder.CampaignModFolderName = this.m_campaignModFolder.ModFolder;
                foreach (string str in this.m_campaignBundle.FilePaths)
                {
                    objectBuilder.CampaignPaths.Add(str.Replace(MyFileSystem.ContentPath + @"\", ""));
                }
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_LocalizationSessionComponent component = sessionComponent as MyObjectBuilder_LocalizationSessionComponent;
            if (component != null)
            {
                this.m_language = component.Language;
                this.m_campaignModFolder = component.CampaignModFolderName;
                this.LoadCampaignLocalization(component.CampaignPaths, this.m_campaignModFolder.Absolute);
            }
        }

        public unsafe void LoadCampaignLocalization(IEnumerable<string> paths, string campaignModFolderPath = null)
        {
            MyContentPath path;
            MyContentPath* pathPtr1 = (MyContentPath*) new MyContentPath(campaignModFolderPath ?? MyFileSystem.ContentPath, null);
            this.m_campaignModFolder = campaignModFolderPath;
            this.m_campaignBundle.FilePaths.Clear();
            if (!string.IsNullOrEmpty(campaignModFolderPath) && MyFileSystem.IsDirectory(campaignModFolderPath))
            {
                this.m_campaignBundle.FilePaths.Add(campaignModFolderPath);
            }
            pathPtr1 = (MyContentPath*) ref path;
            string str = string.IsNullOrEmpty(path.Path) ? path.RootFolder : path.Path;
            foreach (string str2 in paths)
            {
                try
                {
                    MyContentPath path2 = new MyContentPath(Path.Combine(str, str2), null);
                    if (path2.AbsoluteFileExists)
                    {
                        this.m_campaignBundle.FilePaths.Add(path2.Absolute);
                        continue;
                    }
                    if (path2.AlternateFileExists)
                    {
                        this.m_campaignBundle.FilePaths.Add(path2.AlternatePath);
                        continue;
                    }
                    IEnumerator<string> enumerator = MyFileSystem.GetFiles(path2.AbsoluteDirExists ? path2.Absolute : path2.AlternatePath, "*.sbl", MySearchOption.AllDirectories).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            this.m_campaignBundle.FilePaths.Add(current);
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
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine("Wrong Path for localization component: " + str2);
                    MyLog.Default.WriteLine(exception.Message);
                }
            }
            if (this.m_campaignBundle.FilePaths.Count > 0)
            {
                MyLocalization.Static.LoadBundle(this.m_campaignBundle, this.m_influencedContexts, true);
            }
        }

        public void SwitchLanguage(string language)
        {
            this.m_language = language;
            using (HashSet<MyLocalizationContext>.Enumerator enumerator = this.m_influencedContexts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Switch(language);
                }
            }
        }

        protected override void UnloadData()
        {
            MyLocalization.Static.DisposeAll();
        }

        public string Language =>
            this.m_language;
    }
}

