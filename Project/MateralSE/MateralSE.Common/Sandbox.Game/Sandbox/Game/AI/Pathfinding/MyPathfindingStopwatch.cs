namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Diagnostics;
    using VRage.Utils;

    internal static class MyPathfindingStopwatch
    {
        private static Stopwatch s_stopWatch = null;
        private static Stopwatch s_gloabalStopwatch = null;
        private static MyLog s_log = new MyLog(false);
        private const int StopTimeMs = 0x2710;
        private static int s_levelOfStarting = 0;

        static MyPathfindingStopwatch()
        {
            s_stopWatch = new Stopwatch();
            s_gloabalStopwatch = new Stopwatch();
            s_log = new MyLog(false);
        }

        [Conditional("DEBUG")]
        public static void CheckStopMeasuring()
        {
            if (s_gloabalStopwatch.IsRunning)
            {
                long elapsedMilliseconds = s_gloabalStopwatch.ElapsedMilliseconds;
            }
        }

        [Conditional("DEBUG")]
        public static void Reset()
        {
            s_stopWatch.Reset();
        }

        [Conditional("DEBUG")]
        public static void Start()
        {
            if (s_stopWatch.IsRunning)
            {
                s_levelOfStarting++;
            }
            else
            {
                s_stopWatch.Start();
                s_levelOfStarting = 1;
            }
        }

        [Conditional("DEBUG")]
        public static void StartMeasuring()
        {
            s_stopWatch.Reset();
            s_gloabalStopwatch.Reset();
            s_gloabalStopwatch.Start();
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            if (s_stopWatch.IsRunning)
            {
                s_levelOfStarting--;
                if (s_levelOfStarting == 0)
                {
                    s_stopWatch.Stop();
                }
            }
        }

        [Conditional("DEBUG")]
        public static void StopMeasuring()
        {
            s_gloabalStopwatch.Stop();
            string msg = $"pathfinding elapsed time: {s_stopWatch.ElapsedMilliseconds} ms / in {0x2710} ms";
            s_log.WriteLineAndConsole(msg);
        }
    }
}

