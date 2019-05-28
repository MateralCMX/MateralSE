namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public class MyClipmapTiming
    {
        [ThreadStatic]
        private static System.Diagnostics.Stopwatch m_threadStopwatch;
        private static Dictionary<Thread, System.Diagnostics.Stopwatch> m_stopwatches = new Dictionary<Thread, System.Diagnostics.Stopwatch>();
        private static TimeSpan m_total;

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        private static void ReadTotal()
        {
            Dictionary<Thread, System.Diagnostics.Stopwatch> stopwatches = m_stopwatches;
            lock (stopwatches)
            {
                long ticks = 0L;
                foreach (System.Diagnostics.Stopwatch stopwatch in m_stopwatches.Values)
                {
                    ticks += stopwatch.ElapsedTicks;
                }
                m_total = new TimeSpan(ticks);
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Reset()
        {
            Dictionary<Thread, System.Diagnostics.Stopwatch> stopwatches = m_stopwatches;
            lock (stopwatches)
            {
                using (Dictionary<Thread, System.Diagnostics.Stopwatch>.ValueCollection.Enumerator enumerator = m_stopwatches.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Reset();
                    }
                }
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void StartTiming()
        {
            Stopwatch.Start();
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void StopTiming()
        {
            Stopwatch.Stop();
        }

        private static System.Diagnostics.Stopwatch Stopwatch
        {
            get
            {
                if (m_threadStopwatch == null)
                {
                    Dictionary<Thread, System.Diagnostics.Stopwatch> stopwatches = m_stopwatches;
                    lock (stopwatches)
                    {
                        m_threadStopwatch = new System.Diagnostics.Stopwatch();
                        m_stopwatches[Thread.CurrentThread] = m_threadStopwatch;
                    }
                }
                return m_threadStopwatch;
            }
        }

        public static TimeSpan Total =>
            m_total;
    }
}

