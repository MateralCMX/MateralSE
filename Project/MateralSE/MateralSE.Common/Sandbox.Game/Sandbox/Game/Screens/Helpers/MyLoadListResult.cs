namespace Sandbox.Game.Screens.Helpers
{
    using ParallelTasks;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;

    public abstract class MyLoadListResult : IMyAsyncResult
    {
        public List<Tuple<string, MyWorldInfo>> AvailableSaves = new List<Tuple<string, MyWorldInfo>>();
        public bool ContainsCorruptedWorlds;
        public readonly string CustomPath;

        public MyLoadListResult(string customPath = null)
        {
            this.CustomPath = customPath;
            this.Task = Parallel.Start(() => this.LoadListAsync());
        }

        protected abstract List<Tuple<string, MyWorldInfo>> GetAvailableSaves();
        private void LoadListAsync()
        {
            this.AvailableSaves = this.GetAvailableSaves();
            this.ContainsCorruptedWorlds = false;
            StringBuilder builder = new StringBuilder();
            foreach (Tuple<string, MyWorldInfo> tuple in this.AvailableSaves)
            {
                if (tuple.Item2 == null)
                {
                    continue;
                }
                if (tuple.Item2.IsCorrupted)
                {
                    builder.Append(Path.GetFileNameWithoutExtension(tuple.Item1)).Append("\n");
                    this.ContainsCorruptedWorlds = true;
                }
            }
            this.AvailableSaves.RemoveAll(x => (x == null) || (x.Item2 == null));
            if (this.ContainsCorruptedWorlds && (MyLog.Default != null))
            {
                MyLog.Default.WriteLine("Corrupted worlds: ");
                MyLog.Default.WriteLine(builder.ToString());
            }
            if (this.AvailableSaves.Count != 0)
            {
                this.AvailableSaves.Sort((a, b) => b.Item2.LastSaveTime.CompareTo(a.Item2.LastSaveTime));
            }
        }

        [Conditional("DEBUG")]
        private void VerifyUniqueWorldID(List<Tuple<string, MyWorldInfo>> availableWorlds)
        {
            if (MyLog.Default != null)
            {
                HashSet<string> set = new HashSet<string>();
                foreach (Tuple<string, MyWorldInfo> tuple in availableWorlds)
                {
                    MyWorldInfo info = tuple.Item2;
                    if (set.Contains(tuple.Item1))
                    {
                        MyLog.Default.WriteLine(string.Format("Non-unique WorldID detected. WorldID = {0}; World Folder Path = '{2}', World Name = '{1}'", tuple.Item1, info.SessionName, tuple.Item1));
                    }
                    set.Add(tuple.Item1);
                }
            }
        }

        public bool IsCompleted =>
            this.Task.IsComplete;

        public ParallelTasks.Task Task { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLoadListResult.<>c <>9 = new MyLoadListResult.<>c();
            public static Predicate<Tuple<string, MyWorldInfo>> <>9__10_0;
            public static Comparison<Tuple<string, MyWorldInfo>> <>9__10_1;

            internal bool <LoadListAsync>b__10_0(Tuple<string, MyWorldInfo> x) => 
                ((x == null) || (x.Item2 == null));

            internal int <LoadListAsync>b__10_1(Tuple<string, MyWorldInfo> a, Tuple<string, MyWorldInfo> b) => 
                b.Item2.LastSaveTime.CompareTo(a.Item2.LastSaveTime);
        }
    }
}

