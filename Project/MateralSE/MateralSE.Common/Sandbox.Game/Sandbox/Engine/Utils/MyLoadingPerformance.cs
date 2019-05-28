namespace Sandbox.Engine.Utils
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class MyLoadingPerformance
    {
        private static MyLoadingPerformance m_instance;
        private Dictionary<uint, Tuple<int, string>> m_voxelCounts = new Dictionary<uint, Tuple<int, string>>();
        private TimeSpan m_loadingTime;
        private Stopwatch m_stopwatch;

        public void AddVoxelHandCount(int count, uint entityID, string name)
        {
            if (this.IsTiming && !this.m_voxelCounts.ContainsKey(entityID))
            {
                this.m_voxelCounts.Add(entityID, new Tuple<int, string>(count, name));
            }
        }

        public void FinishTiming()
        {
            this.m_stopwatch.Stop();
            this.IsTiming = false;
            this.m_loadingTime = this.m_stopwatch.Elapsed;
            this.WriteToLog();
        }

        private void Reset()
        {
            this.LoadingName = null;
            this.m_loadingTime = TimeSpan.Zero;
            this.m_voxelCounts.Clear();
        }

        public void StartTiming()
        {
            if (!this.IsTiming)
            {
                this.Reset();
                this.IsTiming = true;
                this.m_stopwatch = Stopwatch.StartNew();
            }
        }

        public void WriteToLog()
        {
            MySandboxGame.Log.WriteLine("LOADING REPORT FOR: " + this.LoadingName);
            MySandboxGame.Log.IncreaseIndent();
            MySandboxGame.Log.WriteLine("Loading time: " + this.m_loadingTime);
            MySandboxGame.Log.IncreaseIndent();
            foreach (KeyValuePair<uint, Tuple<int, string>> pair in this.m_voxelCounts)
            {
                if (pair.Value.Item1 > 0)
                {
                    object[] objArray1 = new object[] { "Asteroid: ", pair.Key, " voxel hands: ", pair.Value.Item1, ". Voxel File: ", pair.Value.Item2 };
                    MySandboxGame.Log.WriteLine(string.Concat(objArray1));
                }
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("END OF LOADING REPORT");
        }

        public static MyLoadingPerformance Instance =>
            (m_instance ?? (m_instance = new MyLoadingPerformance()));

        public string LoadingName { get; set; }

        public bool IsTiming { get; private set; }
    }
}

