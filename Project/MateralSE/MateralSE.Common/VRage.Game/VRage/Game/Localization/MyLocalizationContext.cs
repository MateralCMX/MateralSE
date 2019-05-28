namespace VRage.Game.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game.ObjectBuilders;
    using VRage.Utils;

    public class MyLocalizationContext
    {
        protected readonly MyStringId m_contextName;
        protected readonly List<string> m_languagesHelper = new List<string>();
        private readonly List<LocalizationFileInfo> m_localizationFileInfos = new List<LocalizationFileInfo>();
        protected readonly Dictionary<MyStringId, MyObjectBuilder_Localization> m_loadedFiles = new Dictionary<MyStringId, MyObjectBuilder_Localization>(MyStringId.Comparer);
        protected readonly Dictionary<MyStringId, StringBuilder> m_idsToTexts = new Dictionary<MyStringId, StringBuilder>(MyStringId.Comparer);
        private static readonly ConcurrentDictionary<MyStringId, StringBuilder> m_allocatedStringBuilders = new ConcurrentDictionary<MyStringId, StringBuilder>(MyStringId.Comparer);
        private MyLocalizationContext m_twinContext;
        private readonly HashSet<ulong> m_switchHelper = new HashSet<ulong>();

        internal MyLocalizationContext(MyStringId name)
        {
            this.m_contextName = name;
        }

        protected StringBuilder AllocateOrGet(string text)
        {
            StringBuilder builder;
            m_allocatedStringBuilders.TryGetValue(MyStringId.GetOrCompute(text), out builder);
            if (builder == null)
            {
                builder = new StringBuilder(text);
                m_allocatedStringBuilders.TryAdd(MyStringId.GetOrCompute(text), builder);
            }
            return builder;
        }

        public void Dispose()
        {
            this.m_languagesHelper.Clear();
            this.m_idsToTexts.Clear();
            this.m_loadedFiles.Clear();
            this.m_switchHelper.Clear();
            this.m_localizationFileInfos.Clear();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is MyStringId)
            {
                return this.m_contextName.Equals((MyStringId) obj);
            }
            return this.Equals((MyLocalizationContext) obj);
        }

        protected bool Equals(MyLocalizationContext other) => 
            this.m_contextName.Equals(other.m_contextName);

        public override int GetHashCode() => 
            this.m_contextName.Id;

        internal void InsertFileInfo(LocalizationFileInfo info)
        {
            this.m_localizationFileInfos.Add(info);
            if (!this.m_languagesHelper.Contains(info.Header.Language))
            {
                this.m_languagesHelper.Add(info.Header.Language);
            }
        }

        private void Load(LocalizationFileInfo fileInfo)
        {
            if (!string.IsNullOrEmpty(fileInfo.Header.ResXName) && (fileInfo.Header.Entries.Count <= 0))
            {
                string directoryName = Path.GetDirectoryName(fileInfo.HeaderPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    using (ResXResourceReader reader = new ResXResourceReader(Path.Combine(directoryName, fileInfo.Header.ResXName)))
                    {
                        IDictionaryEnumerator enumerator = reader.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            MyObjectBuilder_Localization.KeyEntry item = new MyObjectBuilder_Localization.KeyEntry {
                                Key = (string) enumerator.Key,
                                Value = (string) enumerator.Value
                            };
                            fileInfo.Header.Entries.Add(item);
                        }
                    }
                }
            }
        }

        private void LoadLocalizationFileData(MyObjectBuilder_Localization localization, bool overrideExisting = false, bool suppressError = false)
        {
            if (localization != null)
            {
                foreach (MyObjectBuilder_Localization.KeyEntry entry in localization.Entries)
                {
                    StringBuilder builder = this.AllocateOrGet(entry.Value);
                    MyStringId orCompute = MyStringId.GetOrCompute(entry.Key);
                    if (!this.m_idsToTexts.ContainsKey(orCompute))
                    {
                        this.m_idsToTexts.Add(orCompute, builder);
                        continue;
                    }
                    if (overrideExisting)
                    {
                        this.m_idsToTexts[orCompute] = builder;
                        continue;
                    }
                    if (!suppressError)
                    {
                        string[] textArray1 = new string[] { "LocalizationContext: Context ", this.m_contextName.String, " already contains id ", entry.Key, " conflicting entry won't be overriten." };
                        string msg = string.Concat(textArray1);
                        MyLog.Default.WriteLine(msg);
                    }
                }
            }
        }

        public StringBuilder Localize(MyStringId id)
        {
            StringBuilder builder;
            if (this.m_idsToTexts.TryGetValue(id, out builder))
            {
                return MyTexts.SubstituteTexts(builder);
            }
            return ((this.TwinContext == null) ? null : MyTexts.SubstituteTexts(this.TwinContext.Localize(id)));
        }

        public void Switch(string language)
        {
            this.CurrentLanguage = language;
            this.m_idsToTexts.Clear();
            this.m_switchHelper.Clear();
            foreach (LocalizationFileInfo info in this.m_localizationFileInfos)
            {
                if (info.Header.Language != language)
                {
                    continue;
                }
                if (info.Bundle == MyStringId.NullOrEmpty)
                {
                    this.m_switchHelper.Add((ulong) info.Header.Id);
                    this.Load(info);
                    this.LoadLocalizationFileData(info.Header, false, false);
                }
            }
            foreach (LocalizationFileInfo info2 in this.m_localizationFileInfos)
            {
                if (info2.Header.Language != language)
                {
                    continue;
                }
                if (info2.Bundle != MyStringId.NullOrEmpty)
                {
                    this.m_switchHelper.Add((ulong) info2.Header.Id);
                    this.Load(info2);
                    this.LoadLocalizationFileData(info2.Header, true, false);
                }
            }
            foreach (LocalizationFileInfo info3 in this.m_localizationFileInfos)
            {
                if (info3.Header.Default)
                {
                    this.Load(info3);
                    this.LoadLocalizationFileData(info3.Header, false, true);
                }
            }
        }

        internal void UnloadBundle(MyStringId bundleId)
        {
            int index = 0;
            while (index < this.m_localizationFileInfos.Count)
            {
                LocalizationFileInfo info = this.m_localizationFileInfos[index];
                if (((bundleId != MyStringId.NullOrEmpty) || (info.Bundle == MyStringId.NullOrEmpty)) && ((info.Bundle != bundleId) || (info.Bundle == MyStringId.NullOrEmpty)))
                {
                    index++;
                    continue;
                }
                this.m_loadedFiles.Remove(MyStringId.GetOrCompute(info.HeaderPath));
                this.m_localizationFileInfos.RemoveAt(index);
            }
            this.Switch(this.CurrentLanguage);
        }

        public ListReader<string> Languages =>
            this.m_languagesHelper;

        public IEnumerable<MyStringId> Ids =>
            this.m_idsToTexts.Keys;

        public int IdsCount =>
            this.m_idsToTexts.Count;

        public MyStringId Name =>
            this.m_contextName;

        public string CurrentLanguage { get; private set; }

        internal MyLocalizationContext TwinContext
        {
            get => 
                this.m_twinContext;
            set => 
                (this.m_twinContext = value);
        }

        public StringBuilder this[MyStringId id] =>
            this.Localize(id);

        public StringBuilder this[string nameId] =>
            this.Localize(MyStringId.GetOrCompute(nameId));

        [StructLayout(LayoutKind.Sequential)]
        internal struct LocalizationFileInfo
        {
            public readonly MyObjectBuilder_Localization Header;
            public readonly MyStringId Bundle;
            public readonly string HeaderPath;
            public LocalizationFileInfo(string headerFilePath, MyObjectBuilder_Localization header, MyStringId bundle)
            {
                this.Bundle = bundle;
                this.Header = header;
                this.HeaderPath = headerFilePath;
            }
        }
    }
}

