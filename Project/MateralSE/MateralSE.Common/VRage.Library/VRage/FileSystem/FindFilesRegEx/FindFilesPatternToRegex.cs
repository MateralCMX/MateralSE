namespace VRage.Filesystem.FindFilesRegEx
{
    using System;
    using System.Text.RegularExpressions;

    public static class FindFilesPatternToRegex
    {
        private static Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
        private static Regex IlegalCharactersRegex = new Regex("[\\/:<>|\"]", RegexOptions.Compiled);
        private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
        private static string NonDotCharacters = "[^.]*";

        public static Regex Convert(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException();
            }
            pattern = pattern.Trim();
            if (pattern.Length == 0)
            {
                throw new ArgumentException("Pattern is empty.");
            }
            if (IlegalCharactersRegex.IsMatch(pattern))
            {
                throw new ArgumentException("Patterns contains ilegal characters.");
            }
            bool flag = CatchExtentionRegex.IsMatch(pattern);
            bool flag2 = false;
            if (HasQuestionMarkRegEx.IsMatch(pattern))
            {
                flag2 = true;
            }
            else if (flag)
            {
                flag2 = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
            }
            string input = Regex.Escape(pattern);
            input = Regex.Replace("^" + Regex.Replace(input, @"\\\*", ".*"), @"\\\?", ".");
            if (!flag2 & flag)
            {
                input = input + NonDotCharacters;
            }
            return new Regex(input + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}

