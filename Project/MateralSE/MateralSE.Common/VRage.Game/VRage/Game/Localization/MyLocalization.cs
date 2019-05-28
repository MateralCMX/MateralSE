namespace VRage.Game.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyLocalization
    {
        private Dictionary<string, string> m_pathToContextTranslator = new Dictionary<string, string>();
        private static readonly FastResourceLock m_localizationLoadingLock = new FastResourceLock();
        public static readonly string LOCALIZATION_FOLDER = @"Data\Localization";
        private static readonly StringBuilder m_defaultLocalization = new StringBuilder("Failed localization attempt. Missing or not loaded contexts.");
        private static MyLocalization m_instance;
        private readonly Dictionary<MyStringId, MyLocalizationContext> m_contexts = new Dictionary<MyStringId, MyLocalizationContext>(MyStringId.Comparer);
        private readonly Dictionary<MyStringId, MyLocalizationContext> m_disposableContexts = new Dictionary<MyStringId, MyLocalizationContext>(MyStringId.Comparer);
        private readonly Dictionary<MyStringId, MyBundle> m_loadedBundles = new Dictionary<MyStringId, MyBundle>(MyStringId.Comparer);
        private static readonly string LOCALIZATION_TAG_CAMPAIGN = "LOCC";
        private static readonly string LOCALIZATION_TAG = "LOC";

        private MyLocalization()
        {
        }

        private MyLocalizationContext CreateOrGetContext(MyStringId contextId, bool disposable)
        {
            MyLocalizationContext context = null;
            if (!disposable)
            {
                this.m_contexts.TryGetValue(contextId, out context);
                if (context == null)
                {
                    MyLocalizationContext context2;
                    this.m_contexts.Add(contextId, context = new MyLocalizationContext(contextId));
                    if (this.m_disposableContexts.TryGetValue(contextId, out context2))
                    {
                        context.TwinContext = context2;
                        context2.TwinContext = context;
                    }
                }
            }
            else
            {
                this.m_disposableContexts.TryGetValue(contextId, out context);
                if (context == null)
                {
                    MyLocalizationContext context3;
                    this.m_disposableContexts.Add(contextId, context = new MyLocalizationContext(contextId));
                    if (this.m_contexts.TryGetValue(contextId, out context3))
                    {
                        context.TwinContext = context3;
                        context3.TwinContext = context;
                    }
                }
            }
            return context;
        }

        public void DisposeAll()
        {
            this.m_disposableContexts.Values.ForEach<MyLocalizationContext>(context => context.Dispose());
            this.m_disposableContexts.Clear();
        }

        public bool DisposeContext(MyStringId nameId)
        {
            MyLocalizationContext context;
            if (!this.m_disposableContexts.TryGetValue(nameId, out context))
            {
                return false;
            }
            context.Dispose();
            this.m_disposableContexts.Remove(nameId);
            return true;
        }

        public StringBuilder Get(MyStringId contextId, MyStringId id)
        {
            MyLocalizationContext context;
            StringBuilder defaultLocalization = m_defaultLocalization;
            if (this.m_disposableContexts.TryGetValue(contextId, out context))
            {
                defaultLocalization = context.Localize(id);
                if (defaultLocalization != null)
                {
                    return defaultLocalization;
                }
            }
            if (this.m_contexts.TryGetValue(contextId, out context))
            {
                defaultLocalization = context.Localize(id);
            }
            if (defaultLocalization == null)
            {
                defaultLocalization = new StringBuilder();
            }
            return defaultLocalization;
        }

        private void Init()
        {
            foreach (string str in MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, LOCALIZATION_FOLDER), "*.sbl", MySearchOption.AllDirectories))
            {
                this.LoadLocalizationFile(str, MyStringId.NullOrEmpty, false);
            }
        }

        public static void Initialize()
        {
            MyLocalization @static = Static;
        }

        public void InitLoader(Action loader)
        {
            MyTexts.RegisterEvaluator(LOCALIZATION_TAG_CAMPAIGN, new CampaignEvaluate(loader));
            MyTexts.RegisterEvaluator(LOCALIZATION_TAG, new UniversalEvaluate());
        }

        public void LoadBundle(MyBundle bundle, HashSet<MyLocalizationContext> influencedContexts = null, bool disposableContexts = true)
        {
            if (this.m_loadedBundles.ContainsKey(bundle.BundleId))
            {
                this.NotifyBundleConflict(bundle.BundleId);
            }
            else
            {
                foreach (string str in bundle.FilePaths)
                {
                    MyLocalizationContext item = this.LoadLocalizationFile(str, bundle.BundleId, true);
                    if ((item != null) && (influencedContexts != null))
                    {
                        influencedContexts.Add(item);
                    }
                    if (item != null)
                    {
                        if (this.m_pathToContextTranslator.ContainsKey(str))
                        {
                            this.m_pathToContextTranslator[str] = item.Name.String;
                        }
                        else
                        {
                            this.m_pathToContextTranslator.Add(str, item.Name.String);
                        }
                    }
                }
            }
        }

        private MyLocalizationContext LoadLocalizationFile(string filePath, MyStringId bundleId, bool disposableContext = false)
        {
            MyObjectBuilder_Localization localization;
            if (!MyFileSystem.FileExists(filePath))
            {
                MyLog.Default.WriteLine("File does not exist: " + filePath);
                return null;
            }
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Localization>(filePath, out localization))
            {
                return null;
            }
            MyLocalizationContext context1 = this.CreateOrGetContext(MyStringId.GetOrCompute(localization.Context), disposableContext);
            context1.InsertFileInfo(new MyLocalizationContext.LocalizationFileInfo(filePath, localization, bundleId));
            return context1;
        }

        private void NotifyBundleConflict(MyStringId bundleId)
        {
            string msg = "MyLocalization: Bundle conflict - Bundle already loaded: " + bundleId.String;
            MyLog.Default.WriteLine(msg);
        }

        public void Switch(string language)
        {
            Dictionary<MyStringId, MyLocalizationContext>.ValueCollection.Enumerator enumerator;
            using (enumerator = this.m_contexts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Switch(language);
                }
            }
            using (enumerator = this.m_disposableContexts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Switch(language);
                }
            }
        }

        public void UnloadBundle(MyStringId bundleId)
        {
            Dictionary<MyStringId, MyLocalizationContext>.ValueCollection.Enumerator enumerator;
            using (enumerator = this.m_contexts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UnloadBundle(bundleId);
                }
            }
            using (enumerator = this.m_disposableContexts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UnloadBundle(bundleId);
                }
            }
        }

        public Dictionary<string, string> PathToContextTranslator =>
            this.m_pathToContextTranslator;

        public static MyLocalization Static
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyLocalization();
                    m_instance.Init();
                }
                return m_instance;
            }
        }

        public StringBuilder this[MyStringId contextName, MyStringId tag] =>
            this.Get(contextName, tag);

        public StringBuilder this[string contexName, string tag] =>
            this[MyStringId.GetOrCompute(contexName), MyStringId.GetOrCompute(tag)];

        public MyLocalizationContext this[MyStringId contextName]
        {
            get
            {
                MyLocalizationContext context;
                if (!this.m_disposableContexts.TryGetValue(contextName, out context))
                {
                    this.m_contexts.TryGetValue(contextName, out context);
                }
                return context;
            }
        }

        public MyLocalizationContext this[string contextName] =>
            this[MyStringId.GetOrCompute(contextName)];

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLocalization.<>c <>9 = new MyLocalization.<>c();
            public static Action<MyLocalizationContext> <>9__25_0;

            internal void <DisposeAll>b__25_0(MyLocalizationContext context)
            {
                context.Dispose();
            }
        }

        private class CampaignEvaluate : ITextEvaluator
        {
            private static Action m_loader;

            public CampaignEvaluate(Action loader)
            {
                m_loader = loader;
            }

            public static string Evaluate(string token, string context, bool assert = true)
            {
                MyLocalizationContext context2 = MyLocalization.Static[context ?? "Common"];
                if (context2 == null)
                {
                    return "";
                }
                if (context2.IdsCount == 0)
                {
                    MyLocalization.m_localizationLoadingLock.AcquireExclusive();
                    if (context2.IdsCount == 0)
                    {
                        m_loader();
                    }
                    MyLocalization.m_localizationLoadingLock.ReleaseExclusive();
                }
                StringBuilder builder = context2[MyStringId.GetOrCompute(token)];
                return ((builder == null) ? "" : builder.ToString());
            }

            public string TokenEvaluate(string token, string context) => 
                Evaluate(token, context, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyBundle
        {
            public MyStringId BundleId;
            public List<string> FilePaths;
        }

        private class UniversalEvaluate : ITextEvaluator
        {
            public string TokenEvaluate(string token, string context)
            {
                string str = MyLocalization.CampaignEvaluate.Evaluate(token, context, false);
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
                StringBuilder builder = MyTexts.Get(MyStringId.GetOrCompute(token));
                return ((builder == null) ? "" : builder.ToString());
            }
        }
    }
}

