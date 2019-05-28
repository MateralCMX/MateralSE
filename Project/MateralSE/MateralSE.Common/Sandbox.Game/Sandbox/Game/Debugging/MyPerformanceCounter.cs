namespace Sandbox.Game.Debugging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class MyPerformanceCounter
    {
        private static Stopwatch m_timer = new Stopwatch();

        static MyPerformanceCounter()
        {
            m_timer.Start();
        }

        public static double TicksToMs(long ticks) => 
            ((((double) ticks) / ((double) Stopwatch.Frequency)) * 1000.0);

        public static long ElapsedTicks =>
            m_timer.ElapsedTicks;

        [StructLayout(LayoutKind.Sequential)]
        private struct Timer
        {
            public static readonly MyPerformanceCounter.Timer Empty;
            public long StartTime;
            public long Runtime;
            public bool IsRunning =>
                (this.StartTime != 0x7fffffffffffffffL);
            static Timer()
            {
                MyPerformanceCounter.Timer timer = new MyPerformanceCounter.Timer {
                    Runtime = 0L,
                    StartTime = 0x7fffffffffffffffL
                };
                Empty = timer;
            }
        }
    }
}

