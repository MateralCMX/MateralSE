namespace VRage.Utils
{
    using System;
    using System.Globalization;
    using System.Text;
    using VRage;

    public class MyValueFormatter
    {
        private static NumberFormatInfo m_numberFormatInfoHelper;
        private static readonly string[] m_genericUnitNames = new string[] { "", "k", "M", "G", "T" };
        private static readonly float[] m_genericUnitMultipliers = new float[] { 1f, 1000f, 1000000f, 1E+09f, 1E+12f };
        private static readonly int[] m_genericUnitDigits = new int[] { 1, 1, 1, 1, 1 };
        private static readonly string[] m_forceUnitNames;
        private static readonly float[] m_forceUnitMultipliers;
        private static readonly int[] m_forceUnitDigits;
        private static readonly string[] m_torqueUnitNames;
        private static readonly float[] m_torqueUnitMultipliers;
        private static readonly int[] m_torqueUnitDigits;
        private static readonly string[] m_workUnitNames;
        private static readonly float[] m_workUnitMultipliers;
        private static readonly int[] m_workUnitDigits;
        private static readonly string[] m_workHoursUnitNames;
        private static readonly float[] m_workHoursUnitMultipliers;
        private static readonly int[] m_workHoursUnitDigits;
        private static readonly string[] m_timeUnitNames;
        private static readonly float[] m_timeUnitMultipliers;
        private static readonly int[] m_timeUnitDigits;
        private static readonly string[] m_weightUnitNames;
        private static readonly float[] m_weightUnitMultipliers;
        private static readonly int[] m_weightUnitDigits;
        private static readonly string[] m_volumeUnitNames;
        private static readonly float[] m_volumeUnitMultipliers;
        private static readonly int[] m_volumeUnitDigits;
        private static readonly string[] m_distanceUnitNames;
        private static readonly float[] m_distanceUnitMultipliers;
        private static readonly int[] m_distanceUnitDigits;

        static MyValueFormatter()
        {
            string[] textArray2 = new string[9];
            textArray2[0] = "N";
            textArray2[1] = "kN";
            textArray2[2] = "MN";
            textArray2[3] = "GN";
            textArray2[4] = "TN";
            textArray2[5] = "PN";
            textArray2[6] = "EN";
            textArray2[7] = "ZN";
            textArray2[8] = "YN";
            m_forceUnitNames = textArray2;
            m_forceUnitMultipliers = new float[] { 1f, 1000f, 1000000f, 1E+09f, 1E+12f, 1E+15f, 1E+18f, 1E+21f, 1E+24f };
            m_forceUnitDigits = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            m_torqueUnitNames = new string[] { "Nm", "kNm", "MNm" };
            m_torqueUnitMultipliers = new float[] { 1f, 1000f, 1000000f };
            int[] numArray1 = new int[3];
            numArray1[1] = 1;
            numArray1[2] = 1;
            m_torqueUnitDigits = numArray1;
            m_workUnitNames = new string[] { "W", "kW", "MW", "GW" };
            m_workUnitMultipliers = new float[] { 1E-06f, 0.001f, 1f, 1000f };
            m_workUnitDigits = new int[] { 0, 2, 2, 2 };
            m_workHoursUnitNames = new string[] { "Wh", "kWh", "MWh", "GWh" };
            m_workHoursUnitMultipliers = new float[] { 1E-06f, 0.001f, 1f, 1000f };
            m_workHoursUnitDigits = new int[] { 0, 2, 2, 2 };
            m_timeUnitNames = new string[] { "Unit_sec", "Unit_min", "Unit_hours", "Unit_days", "Unit_years" };
            m_timeUnitMultipliers = new float[] { 1f, 60f, 3600f, 86400f, 3.1536E+07f };
            m_timeUnitDigits = new int[5];
            m_weightUnitNames = new string[] { "g", "kg", "T", "MT" };
            m_weightUnitMultipliers = new float[] { 0.001f, 1f, 1000f, 1000000f };
            m_weightUnitDigits = new int[] { 0, 2, 2, 2 };
            m_volumeUnitNames = new string[] { "mL", "cL", "dL", "L", "hL", "m\x00b3" };
            m_volumeUnitMultipliers = new float[] { 1E-06f, 1E-05f, 0.0001f, 0.001f, 0.1f, 1f };
            int[] numArray2 = new int[6];
            numArray2[4] = 2;
            numArray2[5] = 1;
            m_volumeUnitDigits = numArray2;
            m_distanceUnitNames = new string[] { "mm", "cm", "m", "km" };
            m_distanceUnitMultipliers = new float[] { 0.001f, 0.01f, 1f, 1000f };
            m_distanceUnitDigits = new int[] { 0, 1, 2, 2 };
            m_numberFormatInfoHelper = new NumberFormatInfo();
            m_numberFormatInfoHelper.NumberDecimalSeparator = ".";
            m_numberFormatInfoHelper.NumberGroupSeparator = " ";
        }

        public static void AppendDistanceInBestUnit(float distanceInMeters, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(distanceInMeters, m_distanceUnitNames, m_distanceUnitMultipliers, m_distanceUnitDigits, output);
        }

        public static void AppendForceInBestUnit(float forceInNewtons, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(forceInNewtons, m_forceUnitNames, m_forceUnitMultipliers, m_forceUnitDigits, output);
        }

        public static void AppendFormattedValueInBestUnit(float value, string[] unitNames, float[] unitMultipliers, int unitDecimalDigits, StringBuilder output)
        {
            float num = Math.Abs(value);
            int index = 1;
            while ((index < unitMultipliers.Length) && (num >= unitMultipliers[index]))
            {
                index++;
            }
            index--;
            value /= unitMultipliers[index];
            output.AppendDecimal(Math.Round((double) value, unitDecimalDigits), unitDecimalDigits);
            output.Append(' ').Append(unitNames[index]);
        }

        public static void AppendFormattedValueInBestUnit(float value, string[] unitNames, float[] unitMultipliers, int[] unitDecimalDigits, StringBuilder output)
        {
            if (float.IsInfinity(value))
            {
                output.Append('-');
            }
            else
            {
                float num = Math.Abs(value);
                int index = 1;
                while ((index < unitMultipliers.Length) && (num >= unitMultipliers[index]))
                {
                    index++;
                }
                index--;
                value /= unitMultipliers[index];
                output.AppendDecimal(Math.Round((double) value, unitDecimalDigits[index]), unitDecimalDigits[index]);
                output.Append(' ').Append(MyTexts.Get(MyStringId.GetOrCompute(unitNames[index])));
            }
        }

        public static void AppendGenericInBestUnit(float genericInBase, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(genericInBase, m_genericUnitNames, m_genericUnitMultipliers, m_genericUnitDigits, output);
        }

        public static void AppendGenericInBestUnit(float genericInBase, int numDecimals, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(genericInBase, m_genericUnitNames, m_genericUnitMultipliers, numDecimals, output);
        }

        public static void AppendTimeExact(int timeInSeconds, StringBuilder output)
        {
            if (timeInSeconds >= 0x15180)
            {
                output.Append((int) (timeInSeconds / 0x15180));
                output.Append("d ");
            }
            output.ConcatFormat<int>("{0:00}", (timeInSeconds / 0xe10) % 0x18, null);
            output.Append(":");
            output.ConcatFormat<int>("{0:00}", (timeInSeconds / 60) % 60, null);
            output.Append(":");
            output.ConcatFormat<int>("{0:00}", timeInSeconds % 60, null);
        }

        public static void AppendTimeExactMinSec(int timeInSeconds, StringBuilder output)
        {
            output.ConcatFormat<int>("{0:00}", (timeInSeconds / 60) % 0x5a0, null);
            output.Append(":");
            output.ConcatFormat<int>("{0:00}", timeInSeconds % 60, null);
        }

        public static void AppendTimeInBestUnit(float timeInSeconds, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(timeInSeconds, m_timeUnitNames, m_timeUnitMultipliers, m_timeUnitDigits, output);
        }

        public static void AppendTorqueInBestUnit(float torqueInNewtonMeters, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(torqueInNewtonMeters, m_torqueUnitNames, m_torqueUnitMultipliers, m_torqueUnitDigits, output);
        }

        public static void AppendVolumeInBestUnit(float volumeInCubicMeters, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(volumeInCubicMeters, m_volumeUnitNames, m_volumeUnitMultipliers, m_volumeUnitDigits, output);
        }

        public static void AppendWeightInBestUnit(float weightInKG, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(weightInKG, m_weightUnitNames, m_weightUnitMultipliers, m_weightUnitDigits, output);
        }

        public static void AppendWorkHoursInBestUnit(float workInMegaWatts, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(workInMegaWatts, m_workHoursUnitNames, m_workHoursUnitMultipliers, m_workHoursUnitDigits, output);
        }

        public static void AppendWorkInBestUnit(float workInMegaWatts, StringBuilder output)
        {
            AppendFormattedValueInBestUnit(workInMegaWatts, m_workUnitNames, m_workUnitMultipliers, m_workUnitDigits, output);
        }

        public static decimal GetDecimalFromString(string number, int decimalDigits)
        {
            decimal num;
            try
            {
                m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
                num = Math.Round(Convert.ToDecimal(number, m_numberFormatInfoHelper), decimalDigits);
            }
            catch
            {
                return 0M;
            }
            return num;
        }

        public static float? GetFloatFromString(string number, int decimalDigits, string groupSeparator)
        {
            float? nullable = null;
            string numberGroupSeparator = m_numberFormatInfoHelper.NumberGroupSeparator;
            m_numberFormatInfoHelper.NumberGroupSeparator = groupSeparator;
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            try
            {
                nullable = new float?((float) Math.Round((double) ((float) Convert.ToDouble(number, m_numberFormatInfoHelper)), decimalDigits));
            }
            catch
            {
            }
            m_numberFormatInfoHelper.NumberGroupSeparator = numberGroupSeparator;
            return nullable;
        }

        public static string GetFormatedArray<T>(T[] array)
        {
            string str = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                str = str + array[i].ToString();
                if (i < (array.Length - 1))
                {
                    str = str + ", ";
                }
            }
            return str;
        }

        public static string GetFormatedDateTime(DateTime dt) => 
            dt.ToString("yyyy-MM-dd HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);

        public static string GetFormatedDateTimeForFilename(DateTime dt) => 
            dt.ToString("yyyy-MM-dd-HH-mm-ss-fff", DateTimeFormatInfo.InvariantInfo);

        public static string GetFormatedDateTimeOffset(DateTimeOffset dt) => 
            dt.ToString("yyyy-MM-dd HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);

        public static string GetFormatedDecimal(decimal num, int decimalDigits)
        {
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            return num.ToString("N", m_numberFormatInfoHelper);
        }

        public static string GetFormatedDouble(double num, int decimalDigits)
        {
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            return num.ToString("N", m_numberFormatInfoHelper);
        }

        public static string GetFormatedFloat(float num, int decimalDigits)
        {
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            return num.ToString("N", m_numberFormatInfoHelper);
        }

        public static string GetFormatedFloat(float num, int decimalDigits, string groupSeparator)
        {
            string numberGroupSeparator = m_numberFormatInfoHelper.NumberGroupSeparator;
            m_numberFormatInfoHelper.NumberGroupSeparator = groupSeparator;
            m_numberFormatInfoHelper.NumberDecimalDigits = decimalDigits;
            m_numberFormatInfoHelper.NumberGroupSeparator = numberGroupSeparator;
            return num.ToString("N", m_numberFormatInfoHelper);
        }

        public static string GetFormatedGameMoney(decimal num) => 
            GetFormatedDecimal(num, 2);

        public static string GetFormatedInt(int i) => 
            i.ToString("#,0", CultureInfo.InvariantCulture);

        public static string GetFormatedLong(long l) => 
            l.ToString("#,0", CultureInfo.InvariantCulture);

        public static string GetFormatedPriceEUR(decimal num) => 
            (GetFormatedDecimal(num, 2) + " €");

        public static string GetFormatedPriceUSD(decimal num) => 
            ("$" + GetFormatedDecimal(num, 2));

        public static string GetFormatedPriceUSD(decimal priceInEur, decimal exchangeRateEurToUsd) => 
            ("~" + GetFormatedDecimal(decimal.Round(exchangeRateEurToUsd * priceInEur, 2), 2) + " $");

        public static string GetFormatedQP(decimal num) => 
            GetFormatedDecimal(num, 1);
    }
}

