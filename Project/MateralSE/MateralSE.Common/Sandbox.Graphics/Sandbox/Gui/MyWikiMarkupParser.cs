namespace Sandbox.Gui
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using VRageMath;

    public class MyWikiMarkupParser
    {
        private static Regex m_splitRegex = new Regex(@"\[.*?\]{1,2}");
        private static Regex m_markupRegex = new Regex(@"(?<=\[)(?!\[).*?(?=\])");
        private static Regex m_digitsRegex = new Regex(@"\d+");
        private static StringBuilder m_stringCache = new StringBuilder();

        private static void ParseMarkup(MyGuiControlMultilineText label, string markup)
        {
            Match match = m_markupRegex.Match(markup);
            if (!match.Value.Contains<char>('|'))
            {
                label.AppendLink(match.Value.Substring(0, match.Value.IndexOf(' ')), match.Value.Substring(match.Value.IndexOf(' ') + 1));
            }
            else
            {
                int num;
                int num2;
                char[] separator = new char[] { '|' };
                string[] strArray = match.Value.Substring(5).Split(separator);
                MatchCollection matchs = m_digitsRegex.Matches(strArray[1]);
                if (int.TryParse(matchs[0].Value, out num) && int.TryParse(matchs[1].Value, out num2))
                {
                    label.AppendImage(strArray[0], MyGuiManager.GetNormalizedSizeFromScreenSize(new Vector2((float) num, (float) num2)), Vector4.One);
                }
            }
        }

        public static void ParseText(string text, ref MyGuiControlMultilineText label)
        {
            try
            {
                string[] strArray = m_splitRegex.Split(text);
                MatchCollection matchs = m_splitRegex.Matches(text);
                for (int i = 0; (i < matchs.Count) || (i < strArray.Length); i++)
                {
                    if (i < strArray.Length)
                    {
                        label.AppendText(m_stringCache.Clear().Append(strArray[i]));
                    }
                    if (i < matchs.Count)
                    {
                        ParseMarkup(label, matchs[i].Value);
                    }
                }
            }
            catch
            {
            }
        }
    }
}

