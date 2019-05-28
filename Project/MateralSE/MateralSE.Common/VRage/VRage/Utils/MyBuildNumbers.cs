namespace VRage.Utils
{
    using System;
    using System.IO;

    public static class MyBuildNumbers
    {
        private const int LENGTH_MAJOR = 2;
        private const int LENGTH_MINOR1 = 3;
        private const int LENGTH_MINOR2 = 3;
        public const string SEPARATOR = "_";

        public static string ConvertBuildNumberFromIntToString(int buildNumberInt) => 
            ConvertBuildNumberFromIntToString(buildNumberInt, "_");

        public static string ConvertBuildNumberFromIntToString(int buildNumberInt, string separator)
        {
            string str = MyUtils.AlignIntToRight(buildNumberInt, 8, '0');
            string[] textArray1 = new string[] { str.Substring(0, 2), separator, str.Substring(2, 3), separator, str.Substring(5, 3) };
            return string.Concat(textArray1);
        }

        public static string ConvertBuildNumberFromIntToStringFriendly(int buildNumberInt, string separator)
        {
            int length = 1;
            string str = MyUtils.AlignIntToRight(buildNumberInt, (length + 3) + 3, '0');
            string[] textArray1 = new string[] { str.Substring(0, length), separator, str.Substring(length, 3), separator, str.Substring(length + 3, 3) };
            return string.Concat(textArray1);
        }

        public static int? ConvertBuildNumberFromStringToInt(string buildNumberString)
        {
            if (buildNumberString.Length >= ((((2 * "_".Length) + 2) + 3) + 3))
            {
                int num;
                int num2;
                int num3;
                if ((buildNumberString.Substring(2, "_".Length) != "_") || (buildNumberString.Substring((2 + "_".Length) + 3, "_".Length) != "_"))
                {
                    return null;
                }
                string s = buildNumberString.Substring(0, 2);
                string str2 = buildNumberString.Substring(2 + "_".Length, 3);
                string str3 = buildNumberString.Substring(((2 + "_".Length) + 3) + "_".Length, 3);
                if (!int.TryParse(s, out num))
                {
                    return null;
                }
                if (!int.TryParse(str2, out num2))
                {
                    return null;
                }
                if (int.TryParse(str3, out num3))
                {
                    return new int?(int.Parse(s + str2 + str3));
                }
            }
            return null;
        }

        public static int? GetBuildNumberFromFileName(string filename, string executableFileName, string extensionName)
        {
            if (filename.Length >= ((((executableFileName.Length + (3 * "_".Length)) + 2) + 3) + 3))
            {
                if (filename.Substring(executableFileName.Length, "_".Length) != "_")
                {
                    return null;
                }
                if (new FileInfo(filename).Extension == extensionName)
                {
                    return ConvertBuildNumberFromStringToInt(filename.Substring(executableFileName.Length + "_".Length, (((2 + "_".Length) + 3) + "_".Length) + 3));
                }
            }
            return null;
        }

        public static int GetBuildNumberWithoutMajor(int buildNumberInt)
        {
            int num = 1;
            for (int i = 0; i < 6; i++)
            {
                num *= 10;
            }
            return (buildNumberInt - ((buildNumberInt / num) * num));
        }

        public static string GetFilenameFromBuildNumber(int buildNumber, string executableFileName) => 
            (executableFileName + "_" + ConvertBuildNumberFromIntToString(buildNumber) + ".exe");

        public static bool IsValidBuildNumber(string buildNumberString) => 
            (ConvertBuildNumberFromStringToInt(buildNumberString) != null);
    }
}

