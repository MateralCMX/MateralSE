namespace VRage.Library.Utils
{
    using System;
    using System.Diagnostics;

    public class MyGameTimer
    {
        private long m_startTicks = Stopwatch.GetTimestamp();
        private long m_elapsedTicks = 0L;
        public static readonly long Frequency = Stopwatch.Frequency;

        public void AddElapsed(MyTimeSpan timespan)
        {
            this.m_startTicks -= timespan.Ticks;
        }

        public TimeSpan ElapsedTimeSpan =>
            this.Elapsed.TimeSpan;

        public long ElapsedTicks =>
            (this.m_elapsedTicks + (Stopwatch.GetTimestamp() - this.m_startTicks));

        public MyTimeSpan Elapsed =>
            new MyTimeSpan(this.ElapsedTicks);
    }
}

