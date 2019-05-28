namespace System.Text
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unsharper;
    using VRage;

    [UnsharperDisableReflection]
    public static class StringBuilderExtensions_2
    {
        private static NumberFormatInfo m_numberFormatInfoHelper;

        static StringBuilderExtensions_2()
        {
            if (m_numberFormatInfoHelper == null)
            {
                m_numberFormatInfoHelper = CultureInfo.InvariantCulture.NumberFormat.Clone() as NumberFormatInfo;
            }
        }

        public static StringBuilder AppendDecimal(this StringBuilder sb, double number, int decimals)
        {
            m_numberFormatInfoHelper.NumberDecimalDigits = Math.Max(0, Math.Min(decimals, 0x63));
            sb.ConcatFormat<double>("{0}", number, m_numberFormatInfoHelper);
            return sb;
        }

        public static StringBuilder AppendDecimal(this StringBuilder sb, float number, int decimals)
        {
            m_numberFormatInfoHelper.NumberDecimalDigits = Math.Max(0, Math.Min(decimals, 0x63));
            sb.ConcatFormat<float>("{0}", number, m_numberFormatInfoHelper);
            return sb;
        }

        public static StringBuilder AppendDecimalDigit(this StringBuilder sb, double number, int validDigitCount) => 
            sb.AppendDecimal(number, GetDecimalCount(number, validDigitCount));

        public static StringBuilder AppendDecimalDigit(this StringBuilder sb, float number, int validDigitCount) => 
            sb.AppendDecimal(number, GetDecimalCount(number, validDigitCount));

        public static StringBuilder AppendFormatedDateTime(this StringBuilder sb, DateTime value)
        {
            sb.Concat(value.Day, 2);
            sb.Append("/");
            sb.Concat(value.Month, 2);
            sb.Append("/");
            sb.Concat(value.Year, 0, '0', 10, false);
            sb.Append(" ");
            sb.Concat(value.Hour, 2);
            sb.Append(":");
            sb.Concat(value.Minute, 2);
            sb.Append(":");
            sb.Concat(value.Second, 2);
            return sb;
        }

        public static StringBuilder AppendFormatedDecimal(this StringBuilder sb, string before, float value, int decimalDigits, string after = "")
        {
            sb.Clear();
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            sb.ConcatFormat<string, float, string>("{0}{1 }{2}", before, value, after, m_numberFormatInfoHelper);
            return sb;
        }

        public static StringBuilder AppendInt32(this StringBuilder sb, int number)
        {
            sb.ConcatFormat<int>("{0}", number, null);
            return sb;
        }

        public static StringBuilder AppendInt64(this StringBuilder sb, long number)
        {
            sb.ConcatFormat<long>("{0}", number, null);
            return sb;
        }

        public static int CompareTo(this StringBuilder self, string other)
        {
            int num = 0;
            while (true)
            {
                bool flag = num < self.Length;
                bool flag2 = num < other.Length;
                if (!(flag | flag2))
                {
                    return 0;
                }
                if (!flag)
                {
                    return -1;
                }
                if (!flag2)
                {
                    return 1;
                }
                int num2 = self[num].CompareTo(other[num]);
                if (num2 != 0)
                {
                    return num2;
                }
                num++;
            }
        }

        public static int CompareTo(this StringBuilder self, StringBuilder other)
        {
            int num = 0;
            while (true)
            {
                bool flag = num < self.Length;
                bool flag2 = num < other.Length;
                if (!(flag | flag2))
                {
                    return 0;
                }
                if (!flag)
                {
                    return -1;
                }
                if (!flag2)
                {
                    return 1;
                }
                int num2 = self[num].CompareTo(other[num]);
                if (num2 != 0)
                {
                    return num2;
                }
                num++;
            }
        }

        public static int CompareToIgnoreCase(this StringBuilder self, StringBuilder other)
        {
            int num = 0;
            while (true)
            {
                bool flag = num < self.Length;
                bool flag2 = num < other.Length;
                if (!(flag | flag2))
                {
                    return 0;
                }
                if (!flag)
                {
                    return -1;
                }
                if (!flag2)
                {
                    return 1;
                }
                int num2 = char.ToLowerInvariant(self[num]).CompareTo(char.ToLowerInvariant(other[num]));
                if (num2 != 0)
                {
                    return num2;
                }
                num++;
            }
        }

        public static bool CompareUpdate(this StringBuilder sb, string text)
        {
            if (sb.CompareTo(text) == 0)
            {
                return false;
            }
            sb.Clear();
            sb.Append(text);
            return true;
        }

        public static bool CompareUpdate(this StringBuilder sb, StringBuilder text)
        {
            if (sb.CompareTo(text) == 0)
            {
                return false;
            }
            sb.Clear();
            sb.AppendStringBuilder(text);
            return true;
        }

        public static int GetDecimalCount(decimal number, int validDigitCount)
        {
            int num = validDigitCount;
            while ((number >= 1M) && (num > 0))
            {
                number /= 10M;
                num--;
            }
            return num;
        }

        public static int GetDecimalCount(double number, int validDigitCount)
        {
            int num = validDigitCount;
            while ((number >= 1.0) && (num > 0))
            {
                number /= 10.0;
                num--;
            }
            return num;
        }

        public static int GetDecimalCount(float number, int validDigitCount)
        {
            int num = validDigitCount;
            while ((number >= 1f) && (num > 0))
            {
                number /= 10f;
                num--;
            }
            return num;
        }

        public static StringBuilder GetFormatedBool(this StringBuilder sb, string before, bool value, string after = "")
        {
            sb.Clear();
            sb.ConcatFormat<string, bool, string>("{0}{1}{2}", before, value, after, null);
            return sb;
        }

        public static StringBuilder GetFormatedDateTime(this StringBuilder sb, DateTime value)
        {
            sb.Clear();
            sb.Concat(value.Day, 2);
            sb.Append("/");
            sb.Concat(value.Month, 2);
            sb.Append("/");
            sb.Concat(value.Year, 0, '0', 10, false);
            sb.Append(" ");
            sb.Concat(value.Hour, 2);
            sb.Append(":");
            sb.Concat(value.Minute, 2);
            sb.Append(":");
            sb.Concat(value.Second, 2);
            return sb;
        }

        public static StringBuilder GetFormatedDateTimeForFilename(this StringBuilder sb, DateTime value)
        {
            sb.Clear();
            sb.Concat(value.Year, 0, '0', 10, false);
            sb.Concat(value.Month, 2);
            sb.Concat(value.Day, 2);
            sb.Append("_");
            sb.Concat(value.Hour, 2);
            sb.Concat(value.Minute, 2);
            sb.Concat(value.Second, 2);
            return sb;
        }

        public static StringBuilder GetFormatedDateTimeOffset(this StringBuilder sb, string before, DateTime value, string after = "")
        {
            sb.Clear();
            sb.Append(before);
            sb.Concat(value.Year, 4, '0', 10, false);
            sb.Append("-");
            sb.Concat(value.Month, 2);
            sb.Append("-");
            sb.Concat(value.Day, 2);
            sb.Append(" ");
            sb.Concat(value.Hour, 2);
            sb.Append(":");
            sb.Concat(value.Minute, 2);
            sb.Append(":");
            sb.Concat(value.Second, 2);
            sb.Append(".");
            sb.Concat(value.Millisecond, 3);
            sb.Append(after);
            return sb;
        }

        public static StringBuilder GetFormatedDateTimeOffset(this StringBuilder sb, string before, DateTimeOffset value, string after = "") => 
            sb.GetFormatedDateTimeOffset(before, value.DateTime, after);

        public static StringBuilder GetFormatedFloat(this StringBuilder sb, string before, float value, string after = "")
        {
            sb.Clear();
            sb.ConcatFormat<string, float, string>("{0}{1: #,0}{2}", before, value, after, null);
            return sb;
        }

        public static StringBuilder GetFormatedInt(this StringBuilder sb, string before, int value, string after = "")
        {
            sb.Clear();
            sb.ConcatFormat<string, int, string>("{0}{1: #,0}{2}", before, value, after, null);
            return sb;
        }

        public static StringBuilder GetFormatedLong(this StringBuilder sb, string before, long value, string after = "")
        {
            sb.Clear();
            sb.ConcatFormat<string, long, string>("{0}{1: #,0}{2}", before, value, after, null);
            return sb;
        }

        public static StringBuilder GetFormatedTimeSpan(this StringBuilder sb, string before, TimeSpan value, string after = "")
        {
            sb.Clear();
            sb.Clear();
            sb.Append(before);
            sb.Concat(value.Hours, 2);
            sb.Append(":");
            sb.Concat(value.Minutes, 2);
            sb.Append(":");
            sb.Concat(value.Seconds, 2);
            sb.Append(".");
            sb.Concat(value.Milliseconds, 3);
            sb.Append(after);
            return sb;
        }

        public static StringBuilder GetStrings(this StringBuilder sb, string before, string value = "", string after = "")
        {
            sb.Clear();
            sb.ConcatFormat<string, string, string>("{0}{1}{2}", before, value, after, null);
            return sb;
        }

        public static List<StringBuilder> Split(this StringBuilder sb, char separator)
        {
            List<StringBuilder> list = new List<StringBuilder>();
            StringBuilder item = new StringBuilder();
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != separator)
                {
                    item.Append(sb[i]);
                }
                else
                {
                    list.Add(item);
                    item = new StringBuilder();
                }
            }
            if (item.Length > 0)
            {
                list.Add(item);
            }
            return list;
        }

        public static void TrimEnd(this StringBuilder sb, int length)
        {
            Exceptions.ThrowIf<ArgumentException>(length > sb.Length, "String builder contains less characters then requested number!");
            sb.Length -= length;
        }

        public static StringBuilder TrimTrailingWhitespace(this StringBuilder sb)
        {
            int length = sb.Length;
            while ((length > 0) && (((sb[length - 1] == ' ') || (sb[length - 1] == '\r')) || (sb[length - 1] == '\n')))
            {
                length--;
            }
            sb.Length = length;
            return sb;
        }
    }
}

