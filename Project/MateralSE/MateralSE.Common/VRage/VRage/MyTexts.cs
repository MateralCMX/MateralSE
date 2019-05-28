namespace VRage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Utils;

    public static class MyTexts
    {
        private static readonly string LOCALIZATION_TAG_GENERAL = "LOCG";
        private static readonly Dictionary<MyLanguagesEnum, LanguageDescription> m_languageIdToLanguage = new Dictionary<MyLanguagesEnum, LanguageDescription>();
        private static readonly Dictionary<string, MyLanguagesEnum> m_cultureToLanguageId = new Dictionary<string, MyLanguagesEnum>();
        private static readonly Dictionary<MyStringId, string> m_strings = new Dictionary<MyStringId, string>(MyStringId.Comparer);
        private static readonly Dictionary<MyStringId, StringBuilder> m_stringBuilders = new Dictionary<MyStringId, StringBuilder>(MyStringId.Comparer);
        private static readonly bool m_checkMissingTexts = false;
        private static Regex m_textReplace;
        private static readonly Dictionary<string, ITextEvaluator> m_evaluators = new Dictionary<string, ITextEvaluator>();

        static MyTexts()
        {
            AddLanguage(MyLanguagesEnum.English, "en", null, "English", 1f, false);
            AddLanguage(MyLanguagesEnum.Czech, "cs", "CZ", "Česky", 0.95f, true);
            AddLanguage(MyLanguagesEnum.Slovak, "sk", "SK", "Slovenčina", 0.95f, true);
            AddLanguage(MyLanguagesEnum.German, "de", null, "Deutsch", 0.85f, true);
            AddLanguage(MyLanguagesEnum.Russian, "ru", null, "Русский", 1f, false);
            AddLanguage(MyLanguagesEnum.Spanish_Spain, "es", null, "Espa\x00f1ol (Espa\x00f1a)", 1f, true);
            AddLanguage(MyLanguagesEnum.French, "fr", null, "Fran\x00e7ais", 1f, true);
            AddLanguage(MyLanguagesEnum.Italian, "it", null, "Italiano", 1f, true);
            AddLanguage(MyLanguagesEnum.Danish, "da", null, "Dansk", 1f, true);
            AddLanguage(MyLanguagesEnum.Dutch, "nl", null, "Nederlands", 1f, true);
            AddLanguage(MyLanguagesEnum.Icelandic, "is", "IS", "\x00cdslenska", 1f, true);
            AddLanguage(MyLanguagesEnum.Polish, "pl", "PL", "Polski", 1f, true);
            AddLanguage(MyLanguagesEnum.Finnish, "fi", null, "Suomi", 1f, true);
            AddLanguage(MyLanguagesEnum.Hungarian, "hu", "HU", "Magyar", 0.85f, true);
            AddLanguage(MyLanguagesEnum.Portuguese_Brazil, "pt", "BR", "Portugu\x00eas (Brasileiro)", 1f, true);
            AddLanguage(MyLanguagesEnum.Estonian, "et", "EE", "Eesti", 1f, true);
            AddLanguage(MyLanguagesEnum.Norwegian, "no", null, "Norsk", 1f, true);
            AddLanguage(MyLanguagesEnum.Spanish_HispanicAmerica, "es", "419", "Espa\x00f1ol (Latinoamerica)", 1f, true);
            AddLanguage(MyLanguagesEnum.Swedish, "sv", null, "Svenska", 0.9f, true);
            AddLanguage(MyLanguagesEnum.Catalan, "ca", "AD", "Catal\x00e0", 0.85f, true);
            AddLanguage(MyLanguagesEnum.Croatian, "hr", "HR", "Hrvatski", 0.9f, true);
            AddLanguage(MyLanguagesEnum.Romanian, "ro", null, "Rom\x00e2nă", 0.85f, true);
            AddLanguage(MyLanguagesEnum.Ukrainian, "uk", null, "Українська", 1f, true);
            AddLanguage(MyLanguagesEnum.Turkish, "tr", "TR", "T\x00fcrk\x00e7e", 1f, true);
            AddLanguage(MyLanguagesEnum.Latvian, "lv", null, "Latviešu", 0.87f, true);
            AddLanguage(MyLanguagesEnum.ChineseChina, "zh", "CN", "Chinese", 1f, false);
            RegisterEvaluator(LOCALIZATION_TAG_GENERAL, new GeneralEvaluate());
        }

        private static void AddLanguage(MyLanguagesEnum id, string cultureName, string subcultureName = null, string displayName = null, float guiTextScale = 1f, bool isCommunityLocalized = true)
        {
            LanguageDescription description = new LanguageDescription(id, displayName, cultureName, subcultureName, guiTextScale, isCommunityLocalized);
            m_languageIdToLanguage.Add(id, description);
            m_cultureToLanguageId.Add(description.FullCultureName, id);
        }

        public static StringBuilder AppendFormat(this StringBuilder stringBuilder, MyStringId textEnum, object arg0) => 
            stringBuilder.AppendFormat(GetString(textEnum), arg0);

        public static StringBuilder AppendFormat(this StringBuilder stringBuilder, MyStringId textEnum, MyStringId arg0) => 
            stringBuilder.AppendFormat(GetString(textEnum), GetString(arg0));

        public static void Clear()
        {
            m_strings.Clear();
            m_stringBuilders.Clear();
            MyStringId id = new MyStringId();
            m_strings[id] = "";
            id = new MyStringId();
            m_stringBuilders[id] = new StringBuilder();
        }

        public static bool Exists(MyStringId id) => 
            m_strings.ContainsKey(id);

        public static StringBuilder Get(MyStringId id)
        {
            StringBuilder builder;
            if (!m_stringBuilders.TryGetValue(id, out builder))
            {
                builder = !m_checkMissingTexts ? new StringBuilder(id.ToString()) : new StringBuilder("X_" + id.ToString());
            }
            if (m_checkMissingTexts)
            {
                StringBuilder builder1 = new StringBuilder();
                builder1.Append("T_");
                builder = builder1.Append(builder);
            }
            string input = builder.ToString();
            string str2 = m_textReplace.Replace(input, new MatchEvaluator(MyTexts.ReplaceEvaluator));
            return ((input == str2) ? builder : new StringBuilder(str2));
        }

        public static MyLanguagesEnum GetBestSuitableLanguage(string culture)
        {
            MyLanguagesEnum english = MyLanguagesEnum.English;
            if (!m_cultureToLanguageId.TryGetValue(culture, out english))
            {
                char[] separator = new char[] { '-' };
                string[] textArray1 = culture.Split(separator);
                string str = textArray1[0];
                string text1 = textArray1[1];
                using (Dictionary<MyLanguagesEnum, LanguageDescription>.Enumerator enumerator = m_languageIdToLanguage.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<MyLanguagesEnum, LanguageDescription> current = enumerator.Current;
                        if (current.Value.FullCultureName == str)
                        {
                            return current.Key;
                        }
                    }
                }
            }
            return english;
        }

        private static string GetPathWithFile(string file, List<string> allFiles)
        {
            using (List<string>.Enumerator enumerator = allFiles.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    string current = enumerator.Current;
                    if (current.Contains(file))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static string GetString(string keyString) => 
            GetString(MyStringId.GetOrCompute(keyString));

        public static string GetString(MyStringId id)
        {
            string str;
            if (!m_strings.TryGetValue(id, out str))
            {
                str = !m_checkMissingTexts ? id.ToString() : ("X_" + id.ToString());
            }
            if (m_checkMissingTexts)
            {
                str = "T_" + str;
            }
            return m_textReplace.Replace(str, new MatchEvaluator(MyTexts.ReplaceEvaluator));
        }

        public static string GetSystemLanguage() => 
            CultureInfo.InstalledUICulture.Name;

        private static void InitReplace()
        {
            StringBuilder builder = new StringBuilder();
            int num = 0;
            builder.Append("{(");
            foreach (KeyValuePair<string, ITextEvaluator> pair in m_evaluators)
            {
                if (num != 0)
                {
                    builder.Append("|");
                }
                builder.AppendFormat(pair.Key, Array.Empty<object>());
                num++;
            }
            builder.Append(@"):(\w*)}");
            m_textReplace = new Regex(builder.ToString());
        }

        public static bool IsTagged(string text, int position, string tag)
        {
            for (int i = 0; i < tag.Length; i++)
            {
                if (text[position + i] != tag[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static void LoadSupportedLanguages(string rootDirectory, HashSet<MyLanguagesEnum> outSupportedLanguages)
        {
            outSupportedLanguages.Add(MyLanguagesEnum.English);
            HashSet<string> set = new HashSet<string>();
            using (IEnumerator<string> enumerator = MyFileSystem.GetFiles(rootDirectory, "*.resx", MySearchOption.TopDirectoryOnly).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    char[] separator = new char[] { '.' };
                    string[] strArray = Path.GetFileNameWithoutExtension(enumerator.Current).Split(separator);
                    if (strArray.Length > 1)
                    {
                        set.Add(strArray[1]);
                    }
                }
            }
            foreach (string str in set)
            {
                MyLanguagesEnum enum2;
                if (m_cultureToLanguageId.TryGetValue(str, out enum2))
                {
                    outSupportedLanguages.Add(enum2);
                }
            }
        }

        public static void LoadTexts(string rootDirectory, string cultureName = null, string subcultureName = null)
        {
            HashSet<string> set = new HashSet<string>();
            HashSet<string> set2 = new HashSet<string>();
            HashSet<string> set3 = new HashSet<string>();
            List<string> allFiles = new List<string>();
            foreach (string str in MyFileSystem.GetFiles(rootDirectory, "*.resx", MySearchOption.AllDirectories))
            {
                if (str.Contains("MyCommonTexts"))
                {
                    char[] separator = new char[] { '.' };
                    set.Add(Path.GetFileNameWithoutExtension(str).Split(separator)[0]);
                }
                else if (str.Contains("MyTexts"))
                {
                    char[] separator = new char[] { '.' };
                    set2.Add(Path.GetFileNameWithoutExtension(str).Split(separator)[0]);
                }
                else
                {
                    if (!str.Contains("MyCoreTexts"))
                    {
                        continue;
                    }
                    char[] separator = new char[] { '.' };
                    set3.Add(Path.GetFileNameWithoutExtension(str).Split(separator)[0]);
                }
                allFiles.Add(str);
            }
            foreach (string str2 in set)
            {
                PatchTexts(GetPathWithFile($"{str2}.resx", allFiles));
            }
            foreach (string str3 in set2)
            {
                PatchTexts(GetPathWithFile($"{str3}.resx", allFiles));
            }
            foreach (string str4 in set3)
            {
                PatchTexts(GetPathWithFile($"{str4}.resx", allFiles));
            }
            if (cultureName != null)
            {
                foreach (string str5 in set)
                {
                    PatchTexts(GetPathWithFile($"{str5}.{cultureName}.resx", allFiles));
                }
                foreach (string str6 in set2)
                {
                    PatchTexts(GetPathWithFile($"{str6}.{cultureName}.resx", allFiles));
                }
                foreach (string str7 in set3)
                {
                    PatchTexts(GetPathWithFile($"{str7}.{cultureName}.resx", allFiles));
                }
                if (subcultureName != null)
                {
                    foreach (string str8 in set)
                    {
                        PatchTexts(GetPathWithFile($"{str8}.{cultureName}-{subcultureName}.resx", allFiles));
                    }
                    foreach (string str9 in set2)
                    {
                        PatchTexts(GetPathWithFile($"{str9}.{cultureName}-{subcultureName}.resx", allFiles));
                    }
                    foreach (string str10 in set3)
                    {
                        PatchTexts(GetPathWithFile($"{str10}.{cultureName}-{subcultureName}.resx", allFiles));
                    }
                }
            }
        }

        private static void PatchTexts(string resourceFile)
        {
            if (File.Exists(resourceFile))
            {
                using (Stream stream = MyFileSystem.OpenRead(resourceFile))
                {
                    using (ResXResourceReader reader = new ResXResourceReader(stream))
                    {
                        foreach (DictionaryEntry entry in reader)
                        {
                            string key = entry.Key as string;
                            string str2 = entry.Value as string;
                            if ((key != null) && (str2 != null))
                            {
                                MyStringId orCompute = MyStringId.GetOrCompute(key);
                                m_strings[orCompute] = str2;
                                m_stringBuilders[orCompute] = new StringBuilder(str2);
                            }
                        }
                    }
                }
            }
        }

        public static void RegisterEvaluator(string prefix, ITextEvaluator eval)
        {
            m_evaluators.Add(prefix, eval);
            InitReplace();
        }

        private static string ReplaceEvaluator(Match match) => 
            ReplaceEvaluator(match, null);

        private static string ReplaceEvaluator(Match match, string context)
        {
            ITextEvaluator evaluator;
            return ((match.Groups.Count == 3) ? (!m_evaluators.TryGetValue(match.Groups[1].Value, out evaluator) ? string.Empty : evaluator.TokenEvaluate(match.Groups[2].Value, context)) : string.Empty);
        }

        public static StringBuilder SubstituteTexts(StringBuilder text)
        {
            if (text == null)
            {
                return null;
            }
            string input = text.ToString();
            string str2 = m_textReplace.Replace(input, new MatchEvaluator(MyTexts.ReplaceEvaluator));
            return ((input == str2) ? text : new StringBuilder(str2));
        }

        public static string SubstituteTexts(string text, string context = null) => 
            ((text == null) ? null : m_textReplace.Replace(text, match => ReplaceEvaluator(match, context)));

        public static string TrySubstitute(string input)
        {
            StringBuilder builder;
            MyStringId orCompute = MyStringId.GetOrCompute(input);
            return (m_stringBuilders.TryGetValue(orCompute, out builder) ? m_textReplace.Replace(builder.ToString(), new MatchEvaluator(MyTexts.ReplaceEvaluator)) : input);
        }

        public static DictionaryReader<MyLanguagesEnum, LanguageDescription> Languages =>
            new DictionaryReader<MyLanguagesEnum, LanguageDescription>(m_languageIdToLanguage);

        private class GeneralEvaluate : ITextEvaluator
        {
            public string TokenEvaluate(string token, string context)
            {
                StringBuilder builder = MyTexts.Get(MyStringId.GetOrCompute(token));
                return ((builder == null) ? "" : builder.ToString());
            }
        }

        public class LanguageDescription
        {
            public readonly MyLanguagesEnum Id;
            public readonly string Name;
            public readonly string CultureName;
            public readonly string SubcultureName;
            public readonly string FullCultureName;
            public readonly bool IsCommunityLocalized;
            public readonly float GuiTextScale;

            internal LanguageDescription(MyLanguagesEnum id, string displayName, string cultureName, string subcultureName, float guiTextScale, bool isCommunityLocalized)
            {
                this.Id = id;
                this.Name = displayName;
                this.CultureName = cultureName;
                this.SubcultureName = subcultureName;
                this.FullCultureName = !string.IsNullOrWhiteSpace(subcultureName) ? $"{cultureName}-{subcultureName}" : cultureName;
                this.IsCommunityLocalized = isCommunityLocalized;
                this.GuiTextScale = guiTextScale;
            }
        }
    }
}

