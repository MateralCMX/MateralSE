namespace VRage
{
    using System;
    using System.Runtime.CompilerServices;

    public static class DateTimeExtensions
    {
        public static readonly DateTime Epoch = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime Now_GarbageFree(this DateTime dateTime) => 
            TimeUtil.LocalTime;

        public static DateTime ToDateTimeFromUnixTimestamp(this uint timestamp) => 
            Epoch.AddSeconds((double) timestamp).ToLocalTime();

        public static uint ToUnixTimestamp(this DateTime time) => 
            ((uint) DateTime.UtcNow.Subtract(Epoch).TotalSeconds);
    }
}

