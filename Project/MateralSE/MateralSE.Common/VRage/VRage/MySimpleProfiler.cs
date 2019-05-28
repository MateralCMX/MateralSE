namespace VRage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.FileSystem;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MySimpleProfiler
    {
        private const int MAX_LEVELS = 100;
        private const int MAX_ITEMS_IN_SYNC_QUEUE = 20;
        private static volatile Dictionary<string, MySimpleProfilingBlock> m_profilingBlocks = new Dictionary<string, MySimpleProfilingBlock>();
        private static readonly ConcurrentQueue<MySimpleProfilingBlock> m_addUponSync = new ConcurrentQueue<MySimpleProfilingBlock>();
        [CompilerGenerated]
        private static Action<MySimpleProfilingBlock> ShowPerformanceWarning;
        [ThreadStatic]
        private static Stack<TimeKeepingItem> m_timers;
        private static readonly Stack<string> m_gpuBlocks = new Stack<string>();
        private static bool m_performanceTestEnabled;
        private static readonly List<int> m_updateTimes = new List<int>();
        private static readonly List<int> m_renderTimes = new List<int>();
        private static readonly List<int> m_gpuTimes = new List<int>();
        private static readonly List<int> m_memoryAllocs = new List<int>();
        private static ulong m_lastAllocationStamp = 0UL;
        private static int m_skipFrames;

        public static  event Action<MySimpleProfilingBlock> ShowPerformanceWarning
        {
            [CompilerGenerated] add
            {
                Action<MySimpleProfilingBlock> showPerformanceWarning = ShowPerformanceWarning;
                while (true)
                {
                    Action<MySimpleProfilingBlock> a = showPerformanceWarning;
                    Action<MySimpleProfilingBlock> action3 = (Action<MySimpleProfilingBlock>) Delegate.Combine(a, value);
                    showPerformanceWarning = Interlocked.CompareExchange<Action<MySimpleProfilingBlock>>(ref ShowPerformanceWarning, action3, a);
                    if (ReferenceEquals(showPerformanceWarning, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySimpleProfilingBlock> showPerformanceWarning = ShowPerformanceWarning;
                while (true)
                {
                    Action<MySimpleProfilingBlock> source = showPerformanceWarning;
                    Action<MySimpleProfilingBlock> action3 = (Action<MySimpleProfilingBlock>) Delegate.Remove(source, value);
                    showPerformanceWarning = Interlocked.CompareExchange<Action<MySimpleProfilingBlock>>(ref ShowPerformanceWarning, action3, source);
                    if (ReferenceEquals(showPerformanceWarning, source))
                    {
                        return;
                    }
                }
            }
        }

        static MySimpleProfiler()
        {
            CurrentWarnings = new Dictionary<MySimpleProfilingBlock, PerformanceWarning>();
            Reset(true);
        }

        private static void AddWarningToCurrent(MySimpleProfilingBlock block)
        {
            if (CurrentWarnings.ContainsKey(block))
            {
                CurrentWarnings[block].Time = 0;
            }
            else
            {
                CurrentWarnings.Add(block, new PerformanceWarning(block));
            }
        }

        public static void AttachTestingTool()
        {
            m_performanceTestEnabled = MyManagedAllocationReader.IsReady;
        }

        public static void Begin(string key, ProfilingBlockType type = 4, [CallerMemberName] string callingMember = null)
        {
            if (m_timers == null)
            {
                m_timers = new Stack<TimeKeepingItem>();
            }
            MySimpleProfilingBlock profilingBlock = GetOrMakeBlock(key, type, false);
            MyTimeSpan? timeSpan = null;
            m_timers.Push(new TimeKeepingItem(callingMember, profilingBlock, timeSpan));
        }

        public static void BeginBlock(string key, ProfilingBlockType type = 4)
        {
            Begin(key, type, "BeginBlock");
        }

        public static void BeginGPUBlock(string key)
        {
            m_gpuBlocks.Push(key);
            if (m_gpuBlocks.Count > 100)
            {
                m_gpuBlocks.Clear();
            }
        }

        private static void CheckPerformance(MySimpleProfilingBlock block, int tickTime, int average)
        {
            if ((block.Type != ProfilingBlockType.INTERNAL) && (block.Type != ProfilingBlockType.INTERNALGPU))
            {
                bool flag = false;
                if (block.ThresholdFrame > 0)
                {
                    flag |= tickTime > block.ThresholdFrame;
                }
                else if (block.ThresholdFrame < 0)
                {
                    flag |= tickTime < -block.ThresholdFrame;
                }
                if (block.ThresholdAverage > 0)
                {
                    flag |= average > block.ThresholdAverage;
                }
                else if (block.ThresholdAverage < 0)
                {
                    flag |= average < -block.ThresholdAverage;
                }
                if (flag && !SkipProfiling)
                {
                    InvokePerformanceWarning(block);
                }
            }
        }

        public static void Commit()
        {
            MySimpleProfilingBlock block;
            Dictionary<string, MySimpleProfilingBlock> profilingBlocks = m_profilingBlocks;
            bool flag = false;
            while (m_addUponSync.TryDequeue(out block))
            {
                MySimpleProfilingBlock block2;
                if (!flag)
                {
                    flag = true;
                    profilingBlocks = new Dictionary<string, MySimpleProfilingBlock>(profilingBlocks);
                }
                string name = block.Name;
                if (profilingBlocks.TryGetValue(name, out block2))
                {
                    block2.MergeFrom(block);
                }
                else
                {
                    profilingBlocks.Add(name, block);
                }
            }
            if (flag)
            {
                m_profilingBlocks = profilingBlocks;
            }
            foreach (MySimpleProfilingBlock block3 in profilingBlocks.Values)
            {
                int num;
                int num2;
                block3.CommitSimulationFrame(out num, out num2);
                CheckPerformance(block3, num, num2);
                if (m_performanceTestEnabled)
                {
                    if (block3.Name == "UpdateFrame")
                    {
                        m_updateTimes.Add(num);
                        continue;
                    }
                    if (block3.Name == "RenderFrame")
                    {
                        m_renderTimes.Add(num);
                        continue;
                    }
                    if (block3.Name == "GPUFrame")
                    {
                        m_gpuTimes.Add(num2);
                    }
                }
            }
            if (m_performanceTestEnabled)
            {
                ulong globalAllocationsStamp = MyManagedAllocationReader.GetGlobalAllocationsStamp();
                m_memoryAllocs.Add((int) (globalAllocationsStamp - m_lastAllocationStamp));
                m_lastAllocationStamp = globalAllocationsStamp;
            }
            foreach (KeyValuePair<MySimpleProfilingBlock, PerformanceWarning> pair in CurrentWarnings)
            {
                PerformanceWarning local1 = pair.Value;
                local1.Time++;
            }
            if (m_skipFrames > 0)
            {
                m_skipFrames--;
                if (m_skipFrames == 0)
                {
                    Reset(false);
                }
            }
        }

        public static void End([CallerMemberName] string callingMember = "")
        {
            EndNoMemberPairingCheck();
        }

        public static void EndGPUBlock(MyTimeSpan time)
        {
            if (m_gpuBlocks.Count != 0)
            {
                string key = m_gpuBlocks.Pop();
                if (m_profilingBlocks.ContainsKey(key))
                {
                    GetOrMakeBlock(key, ProfilingBlockType.GPU, false).CommitTime((int) time.Microseconds);
                }
            }
        }

        public static void EndMemberPairingCheck([CallerMemberName] string callingMember = "")
        {
            if (m_timers.Count != 0)
            {
                double microseconds = new MyTimeSpan(Stopwatch.GetTimestamp()).Microseconds;
                TimeKeepingItem item = m_timers.Pop();
                if (callingMember != item.InvokingMember)
                {
                    EndMemberPairingCheck(callingMember);
                }
                item.ProfilingBlock.CommitTime((int) (microseconds - item.Timespan.Microseconds));
            }
        }

        public static void EndNoMemberPairingCheck()
        {
            if (m_timers.Count != 0)
            {
                double microseconds = new MyTimeSpan(Stopwatch.GetTimestamp()).Microseconds;
                TimeKeepingItem item = m_timers.Pop();
                item.ProfilingBlock.CommitTime((int) (microseconds - item.Timespan.Microseconds));
            }
        }

        private static MySimpleProfilingBlock GetOrMakeBlock(string key, ProfilingBlockType type, bool forceAdd = false)
        {
            MySimpleProfilingBlock block;
            if (m_profilingBlocks.TryGetValue(key, out block))
            {
                return block;
            }
            MySimpleProfilingBlock item = new MySimpleProfilingBlock(key, type);
            if (forceAdd || (m_addUponSync.Count < 20))
            {
                m_addUponSync.Enqueue(item);
            }
            return item;
        }

        private static void InvokePerformanceWarning(MySimpleProfilingBlock block)
        {
            AddWarningToCurrent(block);
            ShowPerformanceWarning.InvokeIfNotNull<MySimpleProfilingBlock>(block);
        }

        public static void LogPerformanceTestResults()
        {
            if (m_performanceTestEnabled)
            {
                GC.Collect();
                long totalMemory = GC.GetTotalMemory(true);
                Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Performance_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".csv"), FileMode.Create);
                StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false, false));
                writer.WriteLine("Update, Render, GPU, Memory");
                for (int i = 0; ((i < m_updateTimes.Count) && ((i < m_renderTimes.Count) && (i < m_gpuTimes.Count))) && (i < m_memoryAllocs.Count); i++)
                {
                    object[] arg = new object[] { m_updateTimes[i], m_renderTimes[i], m_gpuTimes[i], m_memoryAllocs[i] };
                    writer.WriteLine("{0},{1},{2},{3}", arg);
                }
                writer.WriteLine("Final memory: {0}", totalMemory);
                writer.Close();
                stream.Close();
            }
        }

        public static void Reset(bool resetFrameCounter = false)
        {
            CurrentWarnings.Clear();
            m_profilingBlocks = new Dictionary<string, MySimpleProfilingBlock>();
            int microseconds = (int) MyTimeSpan.FromMilliseconds(100.0).Microseconds;
            int thresholdAverage = (int) MyTimeSpan.FromMilliseconds(40.0).Microseconds;
            SetBlockSettings("GPUFrame", microseconds, thresholdAverage, ProfilingBlockType.GPU);
            SetBlockSettings("RenderFrame", microseconds, thresholdAverage, ProfilingBlockType.RENDER);
            if (resetFrameCounter)
            {
                m_skipFrames = 10;
            }
        }

        public static void SetBlockSettings(string key, int thresholdFrame = 0x186a0, int thresholdAverage = 0x2710, ProfilingBlockType type = 4)
        {
            MySimpleProfilingBlock block1 = GetOrMakeBlock(key, type, true);
            block1.ThresholdFrame = thresholdFrame;
            block1.ThresholdAverage = thresholdAverage;
        }

        public static void ShowServerPerformanceWarning(string key, ProfilingBlockType type)
        {
            InvokePerformanceWarning(GetOrMakeBlock(key, type, false));
        }

        public static Dictionary<MySimpleProfilingBlock, PerformanceWarning> CurrentWarnings
        {
            [CompilerGenerated]
            get => 
                <CurrentWarnings>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<CurrentWarnings>k__BackingField = value);
        }

        private static bool SkipProfiling =>
            (m_skipFrames > 0);

        public class MySimpleProfilingBlock
        {
            public const int MEASURE_AVG_OVER_FRAMES = 60;
            public readonly string Name;
            public readonly MyStringId Description;
            public readonly MyStringId DisplayStringId;
            public readonly MySimpleProfiler.ProfilingBlockType Type;
            public int ThresholdFrame = 0x186a0;
            public int ThresholdAverage = 0x2710;
            private int m_tickTime;
            private float m_avgTickTime;

            public MySimpleProfilingBlock(string key, MySimpleProfiler.ProfilingBlockType type)
            {
                this.Name = key;
                this.Type = type;
                if (type != MySimpleProfiler.ProfilingBlockType.MOD)
                {
                    this.DisplayStringId = MyStringId.TryGet("PerformanceWarningArea" + key);
                    this.Description = MyStringId.TryGet("PerformanceWarningArea" + key + "Description");
                }
                else
                {
                    this.ThresholdFrame = 0xc350;
                    this.ThresholdAverage = 0x2710;
                    this.DisplayStringId = MyStringId.GetOrCompute(key);
                    this.Description = MyStringId.TryGet("PerformanceWarningAreaModsDescription");
                }
                if ((this.DisplayStringId == MyStringId.NullOrEmpty) && (type == MySimpleProfiler.ProfilingBlockType.GPU))
                {
                    this.DisplayStringId = MyStringId.TryGet("PerformanceWarningAreaRenderGPU");
                    this.Description = MyStringId.TryGet("PerformanceWarningAreaRenderGPUDescription");
                }
            }

            private void CommitAvgTime(int tickTime)
            {
                float avgTickTime = this.m_avgTickTime;
                this.m_avgTickTime += (tickTime - avgTickTime) / 60f;
            }

            public void CommitSimulationFrame(out int tickTime, out int avgTime)
            {
                if ((this.Type == MySimpleProfiler.ProfilingBlockType.GPU) || (this.Type == MySimpleProfiler.ProfilingBlockType.RENDER))
                {
                    tickTime = 0;
                    avgTime = (int) Interlocked.CompareExchange(ref this.m_avgTickTime, 0f, 0f);
                }
                else
                {
                    tickTime = Interlocked.Exchange(ref this.m_tickTime, 0);
                    this.CommitAvgTime(tickTime);
                    avgTime = (int) this.m_avgTickTime;
                }
            }

            public void CommitTime(int microseconds)
            {
                if (((this.Type == MySimpleProfiler.ProfilingBlockType.GPU) || (this.Type == MySimpleProfiler.ProfilingBlockType.INTERNALGPU)) || (this.Type == MySimpleProfiler.ProfilingBlockType.RENDER))
                {
                    this.CommitAvgTime(microseconds);
                }
                else
                {
                    Interlocked.Add(ref this.m_tickTime, microseconds);
                }
            }

            public void MergeFrom(MySimpleProfiler.MySimpleProfilingBlock other)
            {
                this.CommitTime(other.m_tickTime);
            }

            public string DisplayName =>
                ((this.DisplayStringId == MyStringId.NullOrEmpty) ? this.Name : MyTexts.GetString(this.DisplayStringId));
        }

        public class PerformanceWarning
        {
            public int Time = 0;
            public MySimpleProfiler.MySimpleProfilingBlock Block;

            public PerformanceWarning(MySimpleProfiler.MySimpleProfilingBlock block)
            {
                this.Block = block;
            }
        }

        public enum ProfilingBlockType : byte
        {
            GPU = 0,
            MOD = 1,
            BLOCK = 2,
            RENDER = 3,
            OTHER = 4,
            INTERNAL = 5,
            INTERNALGPU = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeKeepingItem
        {
            public readonly string InvokingMember;
            public readonly MyTimeSpan Timespan;
            public readonly MySimpleProfiler.MySimpleProfilingBlock ProfilingBlock;
            public TimeKeepingItem(string invokingMember, MySimpleProfiler.MySimpleProfilingBlock profilingBlock, MyTimeSpan? timeSpan = new MyTimeSpan?())
            {
                this.InvokingMember = invokingMember;
                this.ProfilingBlock = profilingBlock;
                MyTimeSpan? nullable = timeSpan;
                this.Timespan = (nullable != null) ? nullable.GetValueOrDefault() : new MyTimeSpan(Stopwatch.GetTimestamp());
            }
        }
    }
}

