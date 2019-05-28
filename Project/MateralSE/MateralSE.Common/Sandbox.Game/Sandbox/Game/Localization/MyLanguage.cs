namespace Sandbox.Game.Localization
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game.Localization;
    using VRage.Utils;

    public static class MyLanguage
    {
        private static CultureInfo m_actualCulture;
        private static MyLanguagesEnum m_actualLanguage;
        private static HashSet<MyLanguagesEnum> m_supportedLanguages = new HashSet<MyLanguagesEnum>();

        public static MyLanguagesEnum ConvertLangEnum(CultureInfo info)
        {
            if (info == null)
            {
                info = CultureInfo.InvariantCulture;
            }
            string name = info.Name;
            uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
            if (num <= 0x81c97f33)
            {
                if (num <= 0x43cb3954)
                {
                    if (num <= 0x1674b64c)
                    {
                        if (num == 0x4f71cde)
                        {
                            if (name == "ru-RU")
                            {
                                return MyLanguagesEnum.Russian;
                            }
                        }
                        else if (num != 0x646b100)
                        {
                            if ((num == 0x1674b64c) && (name == "fr-FR"))
                            {
                                return MyLanguagesEnum.French;
                            }
                        }
                        else if (name == "lv-LV")
                        {
                            return MyLanguagesEnum.Latvian;
                        }
                    }
                    else if (num == 0x2606c833)
                    {
                        if (name == "zh-CN")
                        {
                            return MyLanguagesEnum.ChineseChina;
                        }
                    }
                    else if (num != 0x2b45e5d8)
                    {
                        if ((num == 0x43cb3954) && (name == "nl-NL"))
                        {
                            return MyLanguagesEnum.Dutch;
                        }
                    }
                    else if (name == "tr-TR")
                    {
                        return MyLanguagesEnum.Turkish;
                    }
                }
                else if (num <= 0x5442e56a)
                {
                    if (num == 0x461a6d69)
                    {
                        if (name == "es")
                        {
                            return MyLanguagesEnum.Spanish_HispanicAmerica;
                        }
                    }
                    else if (num != 0x4c6b9446)
                    {
                        if ((num == 0x5442e56a) && (name == "hu-HU"))
                        {
                            return MyLanguagesEnum.Hungarian;
                        }
                    }
                    else if (name == "uk-UA")
                    {
                        return MyLanguagesEnum.Ukrainian;
                    }
                }
                else if (num <= 0x5693a5a0)
                {
                    if (num != 0x558312ba)
                    {
                        if ((num == 0x5693a5a0) && (name == "is-IS"))
                        {
                            return MyLanguagesEnum.Icelandic;
                        }
                    }
                    else if (name == "it-IT")
                    {
                        return MyLanguagesEnum.Italian;
                    }
                }
                else if (num != 0x633d9960)
                {
                    if ((num == 0x81c97f33) && (name == "cs-CZ"))
                    {
                        return MyLanguagesEnum.Czech;
                    }
                }
                else if (name == "pt-BR")
                {
                    return MyLanguagesEnum.Portuguese_Brazil;
                }
            }
            else if (num <= 0xa2568579)
            {
                if (num <= 0x91da00c8)
                {
                    if (num == 0x82d369a8)
                    {
                        if (name == "es-ES")
                        {
                            return MyLanguagesEnum.Spanish_Spain;
                        }
                    }
                    else if (num != 0x82ed9afa)
                    {
                        if ((num == 0x91da00c8) && (name == "sk-SK"))
                        {
                            return MyLanguagesEnum.Slovak;
                        }
                    }
                    else if (name == "de-DE")
                    {
                        return MyLanguagesEnum.German;
                    }
                }
                else if (num == 0x98a5c1b8)
                {
                    if (name == "hr-HR")
                    {
                        return MyLanguagesEnum.Croatian;
                    }
                }
                else if (num != 0xa0d32f98)
                {
                    if ((num == 0xa2568579) && (name == "en-US"))
                    {
                        return MyLanguagesEnum.English;
                    }
                }
                else if (name == "pl-PL")
                {
                    return MyLanguagesEnum.Polish;
                }
            }
            else if (num <= 0xb8c8dbfa)
            {
                if (num == 0xa6c6cadc)
                {
                    if (name == "da-DK")
                    {
                        return MyLanguagesEnum.Danish;
                    }
                }
                else if (num != 0xb5a71055)
                {
                    if ((num == 0xb8c8dbfa) && (name == "ro-RO"))
                    {
                        return MyLanguagesEnum.Romanian;
                    }
                }
                else if (name == "et-EE")
                {
                    return MyLanguagesEnum.Estonian;
                }
            }
            else if (num <= 0xcb22d508)
            {
                if (num != 0xb9e503cb)
                {
                    if ((num == 0xcb22d508) && (name == "ca-ES"))
                    {
                        return MyLanguagesEnum.Catalan;
                    }
                }
                else if (name == "nb-NO")
                {
                    return MyLanguagesEnum.Norwegian;
                }
            }
            else if (num != 0xdfc3322e)
            {
                if ((num == 0xfea2db0f) && (name == "sv-SE"))
                {
                    return MyLanguagesEnum.Swedish;
                }
            }
            else if (name == "fi-FI")
            {
                return MyLanguagesEnum.Finnish;
            }
            return MyLanguagesEnum.English;
        }

        public static CultureInfo ConvertLangEnum(MyLanguagesEnum enumVal)
        {
            switch (enumVal)
            {
                case MyLanguagesEnum.English:
                    return new CultureInfo("en-US");

                case MyLanguagesEnum.Czech:
                    return new CultureInfo("cs-CZ");

                case MyLanguagesEnum.Slovak:
                    return new CultureInfo("sk-SK");

                case MyLanguagesEnum.German:
                    return new CultureInfo("de-DE");

                case MyLanguagesEnum.Russian:
                    return new CultureInfo("ru-RU");

                case MyLanguagesEnum.Spanish_Spain:
                    return new CultureInfo("es-ES");

                case MyLanguagesEnum.French:
                    return new CultureInfo("fr-FR");

                case MyLanguagesEnum.Italian:
                    return new CultureInfo("it-IT");

                case MyLanguagesEnum.Danish:
                    return new CultureInfo("da-DK");

                case MyLanguagesEnum.Dutch:
                    return new CultureInfo("nl-NL");

                case MyLanguagesEnum.Icelandic:
                    return new CultureInfo("is-IS");

                case MyLanguagesEnum.Polish:
                    return new CultureInfo("pl-PL");

                case MyLanguagesEnum.Finnish:
                    return new CultureInfo("fi-FI");

                case MyLanguagesEnum.Hungarian:
                    return new CultureInfo("hu-HU");

                case MyLanguagesEnum.Portuguese_Brazil:
                    return new CultureInfo("pt-BR");

                case MyLanguagesEnum.Estonian:
                    return new CultureInfo("et-EE");

                case MyLanguagesEnum.Norwegian:
                    return new CultureInfo("nb-NO");

                case MyLanguagesEnum.Spanish_HispanicAmerica:
                    return new CultureInfo("es");

                case MyLanguagesEnum.Swedish:
                    return new CultureInfo("sv-SE");

                case MyLanguagesEnum.Catalan:
                    return new CultureInfo("ca-ES");

                case MyLanguagesEnum.Croatian:
                    return new CultureInfo("hr-HR");

                case MyLanguagesEnum.Romanian:
                    return new CultureInfo("ro-RO");

                case MyLanguagesEnum.Ukrainian:
                    return new CultureInfo("uk-UA");

                case MyLanguagesEnum.Turkish:
                    return new CultureInfo("tr-TR");

                case MyLanguagesEnum.Latvian:
                    return new CultureInfo("lv-LV");

                case MyLanguagesEnum.ChineseChina:
                    return new CultureInfo("zh-CN");
            }
            return new CultureInfo("en-US");
        }

        [Conditional("DEBUG")]
        private static void GenerateCurrentLanguageCharTable()
        {
            SortedSet<char> collection = new SortedSet<char>();
            using (IEnumerator enumerator = typeof(MyStringId).GetEnumValues().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    StringBuilder builder = MyTexts.Get((MyStringId) enumerator.Current);
                    for (int i = 0; i < builder.Length; i++)
                    {
                        collection.Add(builder[i]);
                    }
                }
            }
            List<char> list = new List<char>(collection);
            string userDataPath = MyFileSystem.UserDataPath;
            using (StreamWriter writer = new StreamWriter(Path.Combine(userDataPath, $"character-table-{CurrentLanguage}.txt")))
            {
                foreach (char ch in list)
                {
                    writer.WriteLine($"{ch}	{(int) ch:x4}");
                }
            }
            using (StreamWriter writer2 = new StreamWriter(Path.Combine(userDataPath, $"character-ranges-{CurrentLanguage}.txt")))
            {
                int num2 = 0;
                while (num2 < list.Count)
                {
                    int num3;
                    int num4 = num3 = list[num2];
                    num2++;
                    while (true)
                    {
                        if ((num2 >= list.Count) || (list[num2] != (num4 + 1)))
                        {
                            writer2.WriteLine($"-range {num3:x4}-{num4:x4}");
                            break;
                        }
                        num4 = list[num2];
                        num2++;
                    }
                }
            }
        }

        private static string GetLocalizationPath() => 
            Path.Combine(MyFileSystem.ContentPath, "Data", "Localization");

        public static MyLanguagesEnum GetOsLanguageCurrent() => 
            ConvertLangEnum(CurrentOSCulture);

        public static MyLanguagesEnum GetOsLanguageCurrentOfficial()
        {
            MyTexts.LanguageDescription description;
            MyLanguagesEnum key = ConvertLangEnum(CurrentOSCulture);
            MyTexts.Languages.TryGetValue(key, out description);
            if ((description == null) || description.IsCommunityLocalized)
            {
                return MyLanguagesEnum.English;
            }
            return key;
        }

        public static void Init()
        {
            MyTexts.LoadSupportedLanguages(GetLocalizationPath(), m_supportedLanguages);
            LoadLanguage(MyLanguagesEnum.English);
        }

        private static void LoadLanguage(MyLanguagesEnum value)
        {
            MyTexts.LanguageDescription description = MyTexts.Languages[value];
            MyTexts.Clear();
            MyTexts.LoadTexts(GetLocalizationPath(), description.CultureName, description.SubcultureName);
            MyGuiManager.LanguageTextScale = description.GuiTextScale;
            m_actualLanguage = value;
            m_actualCulture = ConvertLangEnum(value);
        }

        public static void ObtainCurrentOSCulture()
        {
            CurrentOSCulture = CultureInfo.CurrentCulture;
        }

        public static CultureInfo CurrentOSCulture
        {
            [CompilerGenerated]
            get => 
                <CurrentOSCulture>k__BackingField;
            [CompilerGenerated]
            set => 
                (<CurrentOSCulture>k__BackingField = value);
        }

        public static HashSetReader<MyLanguagesEnum> SupportedLanguages =>
            m_supportedLanguages;

        public static MyLanguagesEnum CurrentLanguage
        {
            get => 
                m_actualLanguage;
            set
            {
                LoadLanguage(value);
                if (MySandboxGame.Config.Language != m_actualLanguage)
                {
                    MySandboxGame.Config.Language = m_actualLanguage;
                    MySandboxGame.Config.Save();
                }
                MyGuiManager.CurrentLanguage = m_actualLanguage;
                m_actualCulture = ConvertLangEnum(m_actualLanguage);
                MyLocalization.Static.Switch(m_actualCulture.Name);
            }
        }

        public static CultureInfo CurrentCulture
        {
            get => 
                (m_actualCulture ?? CultureInfo.CurrentCulture);
            set => 
                (m_actualCulture = value);
        }
    }
}

