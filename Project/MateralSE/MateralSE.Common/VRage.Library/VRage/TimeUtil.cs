namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    public static class TimeUtil
    {
        [DllImport("kernel32.dll")]
        private static extern void GetLocalTime(out SYSTEMTIME time);

        public static DateTime LocalTime
        {
            get
            {
                SYSTEMTIME systemtime;
                GetLocalTime(out systemtime);
                return new DateTime(systemtime.Year, systemtime.Month, systemtime.Day, systemtime.Hour, systemtime.Minute, systemtime.Second, systemtime.Milliseconds, DateTimeKind.Local);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;
        }
    }
}

