namespace VRage.Profiler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using VRage.FileSystem;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyRenderProfiler
    {
        private static readonly object m_pauseLock = new object();
        protected static RenderProfilerSortingOrder m_sortingOrder = RenderProfilerSortingOrder.MillisecondsLastFrame;
        protected static ProfilerGraphContent m_graphContent = ProfilerGraphContent.Elapsed;
        protected static BlockRender m_blockRender = BlockRender.Name;
        protected static SnapshotType m_dataType = SnapshotType.Online;
        public const string PERFORMANCE_PROFILING_SYMBOL = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";
        private static bool m_profilerProcessingEnabled = false;
        public static bool ShallowProfileOnly = true;
        public static bool AverageTimes = false;
        protected static readonly MyDrawArea MemoryGraphScale = new MyDrawArea(0.49f, 0f, 0.745f, 0.6f, 0.001f);
        private static readonly MyDrawArea m_milisecondsGraphScale = new MyDrawArea(0.49f, 0f, 0.745f, 0.9f, 25f);
        private static readonly MyDrawArea m_allocationsGraphScale = new MyDrawArea(0.49f, 0f, 0.745f, 0.9f, 25000f);
        private static readonly Color[] m_colors;
        protected readonly StringBuilder Text = new StringBuilder(100);
        protected const bool ALLOCATION_PROFILING = false;
        protected static readonly MyProfilerBlock FpsBlock;
        protected static float m_fpsPctg;
        private static int m_pauseCount;
        public static bool Paused;
        public static Action GetProfilerFromServer;
        public static Action<int> SaveProfilerToFile;
        public static Action<int, bool> LoadProfilerFromFile;
        [ThreadStatic]
        private static MyProfiler m_threadProfiler;
        private static MyProfiler m_gpuProfiler;
        public static List<MyProfiler> ThreadProfilers;
        private static readonly List<MyProfiler> m_onlineThreadProfilers;
        protected static MyProfiler m_selectedProfiler;
        protected static bool m_enabled;
        protected static int m_selectedFrame;
        private static int m_levelLimit;
        protected static bool m_useCustomFrame;
        protected static int m_frameLocalArea;
        private int m_currentDumpNumber;
        protected static long m_targetTaskRenderTime;
        protected static long m_taskRenderDispersion;
        public static ConcurrentQueue<FrameInfo> FrameTimestamps;
        private static readonly ConcurrentQueue<FrameInfo> m_onlineFrameTimestamps;
        private static MyTimeSpan m_nextAutoScale;
        private static readonly MyTimeSpan AUTO_SCALE_UPDATE;

        static MyRenderProfiler()
        {
            Color[] colorArray1 = new Color[0x13];
            colorArray1[0] = new Color(0, 0xc0, 0xc0);
            colorArray1[1] = Color.Orange;
            colorArray1[2] = Color.BlueViolet * 1.5f;
            colorArray1[3] = Color.BurlyWood;
            colorArray1[4] = Color.Chartreuse;
            colorArray1[5] = Color.CornflowerBlue;
            colorArray1[6] = Color.Cyan;
            colorArray1[7] = Color.ForestGreen;
            colorArray1[8] = Color.Fuchsia;
            colorArray1[9] = Color.Gold;
            colorArray1[10] = Color.GreenYellow;
            colorArray1[11] = Color.LightBlue;
            colorArray1[12] = Color.LightGreen;
            colorArray1[13] = Color.LimeGreen;
            colorArray1[14] = Color.Magenta;
            colorArray1[15] = Color.MintCream;
            colorArray1[0x10] = Color.Orchid;
            colorArray1[0x11] = Color.PeachPuff;
            colorArray1[0x12] = Color.Purple;
            m_colors = colorArray1;
            ThreadProfilers = new List<MyProfiler>(0x10);
            m_onlineThreadProfilers = ThreadProfilers;
            m_frameLocalArea = MyProfiler.MAX_FRAMES;
            m_taskRenderDispersion = MyTimeSpan.FromMilliseconds(30.0).Ticks;
            FrameTimestamps = new ConcurrentQueue<FrameInfo>();
            m_onlineFrameTimestamps = FrameTimestamps;
            AUTO_SCALE_UPDATE = MyTimeSpan.FromSeconds(1.0);
            m_levelLimit = 0;
            FpsBlock = MyProfiler.CreateExternalBlock("FPS", -2);
        }

        protected MyRenderProfiler()
        {
        }

        public static void AddPause(bool pause)
        {
            object pauseLock = m_pauseLock;
            lock (pauseLock)
            {
                m_pauseCount += pause ? 1 : -1;
                ApplyPause(m_pauseCount > 0);
            }
        }

        private static void ApplyPause(bool paused)
        {
            if (!paused && (m_dataType != SnapshotType.Online))
            {
                RestoreOnlineSnapshot();
            }
            if (paused && (m_graphContent == ProfilerGraphContent.Tasks))
            {
                m_targetTaskRenderTime = Stopwatch.GetTimestamp() - m_taskRenderDispersion;
            }
            Thread.MemoryBarrier();
            Paused = paused;
            using (List<MyProfiler>.Enumerator enumerator = ThreadProfilers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Paused = paused;
                }
            }
            GpuProfiler.AutoCommit = !paused;
        }

        public static void Commit([CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            long timestamp = Stopwatch.GetTimestamp();
            MyProfiler threadProfiler = ThreadProfiler;
            if (Paused)
            {
                threadProfiler.ClearFrame();
            }
            else
            {
                threadProfiler.CommitFrame();
                m_useCustomFrame = true;
            }
            MyTimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void CommitTask(MyProfiler.TaskInfo task)
        {
            ThreadProfiler.CommitTask(task);
        }

        public static MyProfiler CreateProfiler(string name, string axisName = null, bool allocationProfiling = false)
        {
            List<MyProfiler> threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                MyProfiler item = new MyProfiler(allocationProfiling, name, axisName ?? "[ms]", ShallowProfileOnly, 0x3e8);
                ThreadProfilers.Add(item);
                SortProfilersLocked();
                item.SetNewLevelLimit(m_profilerProcessingEnabled ? m_levelLimit : 0);
                item.Paused = Paused;
                if (m_selectedProfiler == null)
                {
                    m_selectedProfiler = item;
                }
                return item;
            }
        }

        internal static void DestroyThread()
        {
            List<MyProfiler> threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                ThreadProfilers.Remove(m_threadProfiler);
                if (ReferenceEquals(m_selectedProfiler, m_threadProfiler))
                {
                    m_selectedProfiler = (ThreadProfilers.Count > 0) ? ThreadProfilers[0] : null;
                }
                m_threadProfiler = null;
            }
        }

        public void Draw([CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (m_enabled)
            {
                long timestamp = Stopwatch.GetTimestamp();
                MyProfiler selectedProfiler = m_selectedProfiler;
                if (selectedProfiler != null)
                {
                    int num2;
                    using (selectedProfiler.LockHistory(out num2))
                    {
                        this.Draw(selectedProfiler, num2, m_useCustomFrame ? m_selectedFrame : num2);
                    }
                    MyTimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp);
                }
            }
        }

        protected abstract void Draw(MyProfiler drawProfiler, int lastFrameIndex, int frameToDraw);
        public void Dump()
        {
            try
            {
                string path = null;
                while (true)
                {
                    if (this.m_currentDumpNumber < 100)
                    {
                        path = MyFileSystem.UserDataPath + $"\dump{this.m_currentDumpNumber}.xml";
                        if (MyFileSystem.FileExists(path))
                        {
                            this.m_currentDumpNumber++;
                            continue;
                        }
                    }
                    if (path != null)
                    {
                        Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create);
                        if (stream != null)
                        {
                            StringBuilder builder = ThreadProfiler.Dump();
                            StreamWriter writer1 = new StreamWriter(stream);
                            writer1.Write(builder);
                            writer1.Close();
                            stream.Close();
                        }
                    }
                    break;
                }
            }
            catch
            {
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void EndProfilingBlock(float customValue = 0f, MyTimeSpan? customTime = new MyTimeSpan?(), string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            ThreadProfiler.EndBlock(member, line, file, customTime, customValue, timeFormat, valueFormat, null);
        }

        private static MyProfilerBlock FindBlockByIndex(int index)
        {
            List<MyProfilerBlock> sortedChildren = GetSortedChildren(m_selectedFrame);
            if ((index >= 0) && (index < sortedChildren.Count))
            {
                return sortedChildren[index];
            }
            if ((index != -1) || (m_selectedProfiler.SelectedRoot == null))
            {
                return null;
            }
            return m_selectedProfiler.SelectedRoot.Parent;
        }

        private static void FindMax(MyProfilerBlock.DataReader data, int start, int end, ref float max, ref int maxIndex)
        {
            for (int i = start; i <= end; i++)
            {
                float num2 = data[i];
                if (num2 > max)
                {
                    max = num2;
                    maxIndex = i;
                }
            }
        }

        private static void FindMax(MyProfilerBlock.DataReader data, int lower, int upper, int lastValidFrame, ref float max, ref int maxIndex)
        {
            int num = ((lastValidFrame + 1) + MyProfiler.UPDATE_WINDOW) % MyProfiler.MAX_FRAMES;
            if (lastValidFrame > num)
            {
                FindMax(data, Math.Max(lower, num), Math.Min(lastValidFrame, upper), ref max, ref maxIndex);
            }
            else
            {
                FindMax(data, lower, Math.Min(lastValidFrame, upper), ref max, ref maxIndex);
                FindMax(data, Math.Max(lower, num), upper, ref max, ref maxIndex);
            }
        }

        protected static float FindMaxWrap(MyProfilerBlock.DataReader data, int lower, int upper, int lastValidFrame, out int maxIndex)
        {
            lower = (lower + MyProfiler.MAX_FRAMES) % MyProfiler.MAX_FRAMES;
            upper = (upper + MyProfiler.MAX_FRAMES) % MyProfiler.MAX_FRAMES;
            float max = 0f;
            maxIndex = -1;
            if (upper > lower)
            {
                FindMax(data, lower, upper, lastValidFrame, ref max, ref maxIndex);
            }
            else
            {
                FindMax(data, 0, upper, lastValidFrame, ref max, ref maxIndex);
                FindMax(data, lower, MyProfiler.MAX_FRAMES - 1, lastValidFrame, ref max, ref maxIndex);
            }
            return max;
        }

        public static bool GetAutocommit() => 
            ThreadProfiler.AutoCommit;

        protected static MyDrawArea GetCurrentGraphScale()
        {
            ProfilerGraphContent graphContent = m_graphContent;
            if (graphContent <= ProfilerGraphContent.Tasks)
            {
                return m_milisecondsGraphScale;
            }
            if (graphContent != ProfilerGraphContent.Allocations)
            {
                throw new Exception("Unhandled enum value" + m_graphContent);
            }
            return m_allocationsGraphScale;
        }

        protected static MyProfilerBlock.DataReader GetGraphData(MyProfilerBlock block)
        {
            bool enableOptimizations = m_selectedProfiler.EnableOptimizations;
            ProfilerGraphContent graphContent = m_graphContent;
            if (graphContent == ProfilerGraphContent.Elapsed)
            {
                return block.GetMillisecondsReader(enableOptimizations);
            }
            if (graphContent != ProfilerGraphContent.Allocations)
            {
                throw new Exception("Unhandled enum value" + m_graphContent);
            }
            return block.GetAllocationsReader(enableOptimizations);
        }

        protected static List<MyProfilerBlock> GetSortedChildren(int frameToSortBy)
        {
            List<MyProfilerBlock> list = new List<MyProfilerBlock>(m_selectedProfiler.SelectedRootChildren);
            switch (m_sortingOrder)
            {
                case RenderProfilerSortingOrder.Id:
                    list.Sort((a, b) => a.Id.CompareTo(b.Id));
                    break;

                case RenderProfilerSortingOrder.MillisecondsLastFrame:
                    list.Sort(delegate (MyProfilerBlock a, MyProfilerBlock b) {
                        int num = b.RawMilliseconds[frameToSortBy].CompareTo(a.RawMilliseconds[frameToSortBy]);
                        if (num != 0)
                        {
                            return num;
                        }
                        return a.Id.CompareTo(b.Id);
                    });
                    break;

                case RenderProfilerSortingOrder.AllocatedLastFrame:
                    list.Sort(delegate (MyProfilerBlock a, MyProfilerBlock b) {
                        int num = b.RawAllocations[frameToSortBy].CompareTo(a.RawAllocations[frameToSortBy]);
                        if (num != 0)
                        {
                            return num;
                        }
                        return a.Id.CompareTo(b.Id);
                    });
                    break;

                case RenderProfilerSortingOrder.MillisecondsAverage:
                    list.Sort(delegate (MyProfilerBlock a, MyProfilerBlock b) {
                        int num = b.AverageMilliseconds.CompareTo(a.AverageMilliseconds);
                        if (num != 0)
                        {
                            return num;
                        }
                        return a.Id.CompareTo(b.Id);
                    });
                    break;

                default:
                    break;
            }
            return list;
        }

        protected static int GetWindowEnd(int lastFrameIndex) => 
            (((lastFrameIndex + 1) + MyProfiler.UPDATE_WINDOW) % MyProfiler.MAX_FRAMES);

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void GPU_EndProfilingBlock(float customValue = 0f, MyTimeSpan? customTime = new MyTimeSpan?(), string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            GpuProfiler.EndBlock(member, line, file, customTime, customValue, timeFormat, valueFormat, null);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void GPU_StartProfilingBlock(string blockName = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            GpuProfiler.StartBlock(blockName, member, line, file, 0x7fffffff, false);
        }

        public static void HandleInput(RenderProfilerCommand command, int index, string value)
        {
            List<MyProfiler> threadProfilers;
            List<MyProfiler>.Enumerator enumerator;
            switch (command)
            {
                case RenderProfilerCommand.Enable:
                    if (!m_enabled)
                    {
                        m_enabled = true;
                        m_profilerProcessingEnabled = true;
                        SetLevel();
                        SelectThead(value);
                        return;
                    }
                    return;

                case RenderProfilerCommand.ToggleEnabled:
                    if (m_enabled)
                    {
                        m_enabled = false;
                        m_useCustomFrame = false;
                        return;
                    }
                    m_enabled = true;
                    m_profilerProcessingEnabled = true;
                    SelectThead(value);
                    return;

                case RenderProfilerCommand.JumpToLevel:
                    if ((index != 0) || m_enabled)
                    {
                        m_selectedProfiler.SelectedRoot = FindBlockByIndex(index - 1);
                        m_nextAutoScale = MyTimeSpan.Zero;
                        return;
                    }
                    m_enabled = true;
                    m_profilerProcessingEnabled = true;
                    return;

                case RenderProfilerCommand.JumpToRoot:
                    m_selectedProfiler.SelectedRoot = null;
                    return;

                case RenderProfilerCommand.Pause:
                    SwitchPause();
                    return;

                case RenderProfilerCommand.NextFrame:
                    goto TR_000D;

                case RenderProfilerCommand.PreviousFrame:
                    PreviousFrame(index);
                    return;

                case RenderProfilerCommand.DisableFrameSelection:
                    m_useCustomFrame = false;
                    return;

                case RenderProfilerCommand.NextThread:
                    if (m_graphContent != ProfilerGraphContent.Tasks)
                    {
                        threadProfilers = ThreadProfilers;
                        lock (threadProfilers)
                        {
                            int num2 = (ThreadProfilers.IndexOf(m_selectedProfiler) + 1) % ThreadProfilers.Count;
                            m_selectedProfiler = ThreadProfilers[num2];
                            m_nextAutoScale = MyTimeSpan.Zero;
                            return;
                        }
                    }
                    else
                    {
                        long num = (long) (((double) m_taskRenderDispersion) / 1.1);
                        if (num > 10)
                        {
                            m_taskRenderDispersion = num;
                            return;
                        }
                        return;
                    }
                    goto TR_0024;

                case RenderProfilerCommand.PreviousThread:
                    goto TR_0024;

                case RenderProfilerCommand.IncreaseLevel:
                    m_levelLimit++;
                    SetLevel();
                    return;

                case RenderProfilerCommand.DecreaseLevel:
                    m_levelLimit--;
                    if (m_levelLimit < -1)
                    {
                        m_levelLimit = -1;
                    }
                    SetLevel();
                    return;

                case RenderProfilerCommand.IncreaseLocalArea:
                    m_frameLocalArea = Math.Min(MyProfiler.MAX_FRAMES, m_frameLocalArea * 2);
                    return;

                case RenderProfilerCommand.DecreaseLocalArea:
                    m_frameLocalArea = Math.Max(2, m_frameLocalArea / 2);
                    return;

                case RenderProfilerCommand.IncreaseRange:
                    m_selectedProfiler.AutoScale = false;
                    GetCurrentGraphScale().IncreaseYRange();
                    return;

                case RenderProfilerCommand.DecreaseRange:
                    m_selectedProfiler.AutoScale = false;
                    GetCurrentGraphScale().DecreaseYRange();
                    return;

                case RenderProfilerCommand.Reset:
                    goto TR_001E;

                case RenderProfilerCommand.SetLevel:
                    m_levelLimit = index;
                    if (m_levelLimit < -1)
                    {
                        m_levelLimit = -1;
                    }
                    SetLevel();
                    return;

                case RenderProfilerCommand.ChangeSortingOrder:
                    m_sortingOrder += 1;
                    if (m_sortingOrder >= RenderProfilerSortingOrder.NumSortingTypes)
                    {
                        m_sortingOrder = RenderProfilerSortingOrder.Id;
                        return;
                    }
                    return;

                case RenderProfilerCommand.CopyPathToClipboard:
                {
                    StringBuilder builder = new StringBuilder(200);
                    MyProfilerBlock selectedRoot = m_selectedProfiler.SelectedRoot;
                    while (true)
                    {
                        if (selectedRoot == null)
                        {
                            if (builder.Length <= 0)
                            {
                                break;
                            }
                            MyClipboardHelper.SetClipboard(builder.ToString());
                            return;
                        }
                        if (builder.Length > 0)
                        {
                            builder.Insert(0, " > ");
                        }
                        builder.Insert(0, selectedRoot.Name);
                        selectedRoot = selectedRoot.Parent;
                    }
                    return;
                }
                case RenderProfilerCommand.TryGoToPathInClipboard:
                {
                    MyProfilerBlock block2;
                    string fullPath = string.Empty;
                    Thread thread1 = new Thread(delegate {
                        try
                        {
                            fullPath = Clipboard.GetText();
                        }
                        catch
                        {
                        }
                    });
                    thread1.SetApartmentState(ApartmentState.STA);
                    thread1.Start();
                    thread1.Join();
                    if (string.IsNullOrEmpty(fullPath))
                    {
                        return;
                    }
                    else
                    {
                        string[] separator = new string[] { " > " };
                        string[] strArray = fullPath.Split(separator, StringSplitOptions.None);
                        block2 = null;
                        List<MyProfilerBlock> rootBlocks = m_selectedProfiler.RootBlocks;
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 >= strArray.Length)
                            {
                                break;
                            }
                            string str = strArray[num4];
                            MyProfilerBlock objA = block2;
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 < rootBlocks.Count)
                                {
                                    MyProfilerBlock block4 = rootBlocks[num5];
                                    if (block4.Name != str)
                                    {
                                        num5++;
                                        continue;
                                    }
                                    block2 = block4;
                                    rootBlocks = block2.Children;
                                }
                                if (ReferenceEquals(objA, block2))
                                {
                                    break;
                                }
                                else
                                {
                                    num4++;
                                }
                                break;
                            }
                        }
                    }
                    if (block2 != null)
                    {
                        m_selectedProfiler.SelectedRoot = block2;
                        return;
                    }
                    return;
                }
                case RenderProfilerCommand.GetFomServer:
                    if (m_enabled && (GetProfilerFromServer != null))
                    {
                        Pause();
                        GetProfilerFromServer();
                        return;
                    }
                    return;

                case RenderProfilerCommand.GetFromClient:
                    RestoreOnlineSnapshot();
                    return;

                case RenderProfilerCommand.SaveToFile:
                    SaveProfilerToFile(index);
                    return;

                case RenderProfilerCommand.LoadFromFile:
                    Pause();
                    LoadProfilerFromFile(index, false);
                    return;

                case RenderProfilerCommand.SwapBlockOptimized:
                    if (index == 0)
                    {
                        foreach (MyProfilerBlock local1 in m_selectedProfiler.SelectedRootChildren)
                        {
                            local1.IsOptimized = !local1.IsOptimized;
                        }
                    }
                    else
                    {
                        MyProfilerBlock block5 = FindBlockByIndex(index - 1);
                        if (block5 != null)
                        {
                            block5.IsOptimized = !block5.IsOptimized;
                            return;
                        }
                    }
                    return;

                case RenderProfilerCommand.ToggleOptimizationsEnabled:
                    m_selectedProfiler.EnableOptimizations = !m_selectedProfiler.EnableOptimizations;
                    return;

                case RenderProfilerCommand.ResetAllOptimizations:
                {
                    using (List<MyProfilerBlock>.Enumerator enumerator2 = m_selectedProfiler.RootBlocks.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            ResetOptimizationsRecursive(enumerator2.Current);
                        }
                        return;
                    }
                }
                case RenderProfilerCommand.SwitchBlockRender:
                    goto TR_006B;

                case RenderProfilerCommand.SwitchGraphContent:
                    m_graphContent += 1;
                    if (m_graphContent >= ProfilerGraphContent.ProfilerGraphContentMax)
                    {
                        m_graphContent = ProfilerGraphContent.Elapsed;
                    }
                    switch (m_graphContent)
                    {
                        case ProfilerGraphContent.Elapsed:
                            m_sortingOrder = RenderProfilerSortingOrder.MillisecondsLastFrame;
                            break;

                        case ProfilerGraphContent.Tasks:
                            if (!FrameTimestamps.IsEmpty)
                            {
                                m_targetTaskRenderTime = (m_selectedProfiler != null) ? m_selectedProfiler.CommitTimes[m_selectedFrame] : (FrameTimestamps.Last<FrameInfo>().Time - m_taskRenderDispersion);
                            }
                            break;

                        case ProfilerGraphContent.Allocations:
                            m_sortingOrder = RenderProfilerSortingOrder.AllocatedLastFrame;
                            break;

                        default:
                            break;
                    }
                    m_nextAutoScale = MyTimeSpan.Zero;
                    return;

                case RenderProfilerCommand.SwitchShallowProfile:
                    ShallowProfileOnly = !ShallowProfileOnly;
                    threadProfilers = ThreadProfilers;
                    lock (threadProfilers)
                    {
                        using (enumerator = ThreadProfilers.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.SetShallowProfile(ShallowProfileOnly);
                            }
                            return;
                        }
                    }
                    break;

                case RenderProfilerCommand.ToggleAutoScale:
                    m_selectedProfiler.AutoScale = !m_selectedProfiler.AutoScale;
                    m_nextAutoScale = MyTimeSpan.Zero;
                    return;

                case RenderProfilerCommand.SwitchAverageTimes:
                    break;

                case RenderProfilerCommand.SubtractFromFile:
                    Pause();
                    LoadProfilerFromFile(index, true);
                    return;

                case RenderProfilerCommand.EnableAutoScale:
                    m_selectedProfiler.AutoScale = true;
                    return;

                default:
                    return;
            }
            AverageTimes = !AverageTimes;
            threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                using (enumerator = ThreadProfilers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.AverageTimes = AverageTimes;
                    }
                    return;
                }
            }
            goto TR_006B;
        TR_000D:
            NextFrame(index);
            return;
        TR_001E:
            threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                using (enumerator = ThreadProfilers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Reset();
                    }
                }
                FrameTimestamps = new ConcurrentQueue<FrameInfo>();
                m_selectedFrame = 0;
                return;
            }
            goto TR_000D;
        TR_0024:
            if (m_graphContent == ProfilerGraphContent.Tasks)
            {
                m_taskRenderDispersion = (long) (m_taskRenderDispersion * 1.1);
                return;
            }
            threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                int num3 = ((ThreadProfilers.IndexOf(m_selectedProfiler) - 1) + ThreadProfilers.Count) % ThreadProfilers.Count;
                m_selectedProfiler = ThreadProfilers[num3];
                m_nextAutoScale = MyTimeSpan.Zero;
                return;
            }
            goto TR_001E;
        TR_006B:
            m_blockRender += 1;
            if (m_blockRender == BlockRender.BlockRenderMax)
            {
                m_blockRender = BlockRender.Name;
            }
        }

        protected static Color IndexToColor(int index) => 
            m_colors[index % m_colors.Length];

        public static void InitMemoryHack(string name)
        {
            ThreadProfiler.InitMemoryHack(name);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void InitThraedInfo(int viewPriority)
        {
            ThreadProfiler.ViewPriority = viewPriority;
            List<MyProfiler> threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                SortProfilersLocked();
            }
        }

        protected static bool IsValidIndex(int frameIndex, int lastValidFrame) => 
            (((frameIndex > lastValidFrame) ? frameIndex : (frameIndex + MyProfiler.MAX_FRAMES)) > (lastValidFrame + MyProfiler.UPDATE_WINDOW));

        private static void NextFrame(int step)
        {
            if (m_graphContent == ProfilerGraphContent.Tasks)
            {
                m_targetTaskRenderTime += (long) ((m_taskRenderDispersion * step) * 0.05f);
            }
            else
            {
                m_useCustomFrame = true;
                m_selectedFrame += step;
                while (m_selectedFrame >= MyProfiler.MAX_FRAMES)
                {
                    m_selectedFrame -= MyProfiler.MAX_FRAMES;
                }
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnBeginSimulationFrame(long frameNumber)
        {
            if (!Paused)
            {
                FrameInfo info;
                long timestamp = Stopwatch.GetTimestamp();
                FrameInfo item = new FrameInfo {
                    Time = timestamp,
                    FrameNumber = frameNumber
                };
                FrameTimestamps.Enqueue(item);
                if (FrameTimestamps.Count > MyProfiler.MAX_FRAMES)
                {
                    FrameInfo info3;
                    FrameTimestamps.TryDequeue(out info3);
                }
                FrameTimestamps.TryPeek(out info);
                MyProfiler.LastFrameTime = timestamp;
                MyProfiler.LastInterestingFrameTime = info.Time;
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskFinished(MyProfiler.TaskType? taskType, float customValue)
        {
            ThreadProfiler.OnTaskFinished(taskType, customValue);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskStarted(MyProfiler.TaskType taskType, string debugName, long scheduledTimestamp)
        {
            ThreadProfiler.OnTaskStarted(taskType, debugName, scheduledTimestamp);
        }

        private static void Pause()
        {
            object pauseLock = m_pauseLock;
            lock (pauseLock)
            {
                m_pauseCount = 1;
                ApplyPause(true);
            }
        }

        private static void PreviousFrame(int step)
        {
            if (m_graphContent == ProfilerGraphContent.Tasks)
            {
                m_targetTaskRenderTime -= (long) ((m_taskRenderDispersion * step) * 0.05f);
            }
            else
            {
                m_useCustomFrame = true;
                m_selectedFrame -= step;
                while (m_selectedFrame < 0)
                {
                    m_selectedFrame += MyProfiler.MAX_FRAMES - 1;
                }
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void ProfileCustomValue(string name, float value, MyTimeSpan? customTime = new MyTimeSpan?(), string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            int levelLimit = m_levelLimit;
        }

        public static void PushOnlineSnapshot(SnapshotType type, List<MyProfiler> threadProfilers, ConcurrentQueue<FrameInfo> frameTimestamps)
        {
            m_dataType = type;
            if (!FrameTimestamps.IsEmpty)
            {
                MyProfiler.LastFrameTime = FrameTimestamps.Last<FrameInfo>().Time;
                MyProfiler.LastInterestingFrameTime = FrameTimestamps.First<FrameInfo>().Time;
                m_targetTaskRenderTime = MyProfiler.LastFrameTime - m_taskRenderDispersion;
            }
            Volatile.Write<List<MyProfiler>>(ref ThreadProfilers, threadProfilers);
            FrameTimestamps = frameTimestamps;
        }

        private static void ResetOptimizationsRecursive(MyProfilerBlock block)
        {
            using (List<MyProfilerBlock>.Enumerator enumerator = block.Children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ResetOptimizationsRecursive(enumerator.Current);
                }
            }
            block.IsOptimized = false;
        }

        private static void RestoreOnlineSnapshot()
        {
            m_dataType = SnapshotType.Online;
            ThreadProfilers = m_onlineThreadProfilers;
            FrameTimestamps = m_onlineFrameTimestamps;
            List<MyProfiler> threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                SelectedProfiler = ThreadProfilers[0];
                long time = FrameTimestamps.LastOrDefault<FrameInfo>().Time;
                if (time > 0L)
                {
                    MyProfiler.LastFrameTime = time;
                    MyProfiler.LastInterestingFrameTime = time;
                }
            }
        }

        private static void SelectThead(string threadName)
        {
            if (threadName != null)
            {
                List<MyProfiler> threadProfilers = ThreadProfilers;
                lock (threadProfilers)
                {
                    foreach (MyProfiler profiler in ThreadProfilers)
                    {
                        if (profiler.DisplayName == threadName)
                        {
                            m_selectedProfiler = profiler;
                            m_graphContent = ProfilerGraphContent.Elapsed;
                            m_nextAutoScale = MyTimeSpan.Zero;
                        }
                    }
                }
            }
        }

        public static void SetAutocommit(bool val)
        {
            ThreadProfiler.AutoCommit = val;
        }

        private static void SetLevel()
        {
            List<MyProfiler> threadProfilers = ThreadProfilers;
            lock (threadProfilers)
            {
                using (List<MyProfiler>.Enumerator enumerator = ThreadProfilers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetNewLevelLimit(m_profilerProcessingEnabled ? m_levelLimit : 0);
                    }
                }
            }
        }

        public static void SetLevel(int index)
        {
            m_levelLimit = index;
            if (m_levelLimit < -1)
            {
                m_levelLimit = -1;
            }
            SetLevel();
        }

        private static void SortProfilersLocked()
        {
            ThreadProfilers.SortNoAlloc<MyProfiler>((x, y) => x.ViewPriority.CompareTo(y.ViewPriority));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void StartNextBlock(string name, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", float previousBlockCustomValue = 0f)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void StartProfilingBlock(string blockName = null, bool isDeepTreeRoot = false, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            ThreadProfiler.StartBlock(blockName, member, line, file, 0x7fffffff, isDeepTreeRoot);
        }

        public static void SubtractOnlineSnapshot(SnapshotType type, List<MyProfiler> threadProfilers, ConcurrentQueue<FrameInfo> frameTimestamps)
        {
            m_dataType = type;
            if (!FrameTimestamps.IsEmpty)
            {
                MyProfiler.LastFrameTime = FrameTimestamps.Last<FrameInfo>().Time;
                MyProfiler.LastInterestingFrameTime = FrameTimestamps.First<FrameInfo>().Time;
                m_targetTaskRenderTime = MyProfiler.LastFrameTime - m_taskRenderDispersion;
            }
            foreach (MyProfiler profiler in threadProfilers)
            {
                if (!string.IsNullOrEmpty(profiler.DisplayName))
                {
                    foreach (MyProfiler profiler2 in ThreadProfilers)
                    {
                        if (profiler2.DisplayName == profiler.DisplayName)
                        {
                            profiler.SubtractFrom(profiler2);
                        }
                    }
                }
            }
            Volatile.Write<List<MyProfiler>>(ref ThreadProfilers, threadProfilers);
            FrameTimestamps = frameTimestamps;
        }

        private static void SwitchPause()
        {
            object pauseLock = m_pauseLock;
            lock (pauseLock)
            {
                m_pauseCount = Paused ? 1 : 0;
                ApplyPause(!Paused);
            }
        }

        protected static void UpdateAutoScale(int lastFrameIndex)
        {
            MyTimeSpan span = new MyTimeSpan(Stopwatch.GetTimestamp());
            if (m_selectedProfiler.AutoScale && (span > m_nextAutoScale))
            {
                MyDrawArea currentGraphScale = GetCurrentGraphScale();
                MyStats stats = new MyStats {
                    Min = currentGraphScale.GetYRange(currentGraphScale.Index - 1),
                    Max = currentGraphScale.YRange
                };
                int windowEnd = GetWindowEnd(lastFrameIndex);
                List<MyProfilerBlock> selectedRootChildren = m_selectedProfiler.SelectedRootChildren;
                if ((m_selectedProfiler.SelectedRoot != null) && (!m_selectedProfiler.IgnoreRoot || (selectedRootChildren.Count == 0)))
                {
                    UpdateStatsSeparated(ref stats, GetGraphData(m_selectedProfiler.SelectedRoot), lastFrameIndex, windowEnd);
                }
                using (List<MyProfilerBlock>.Enumerator enumerator = selectedRootChildren.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyProfilerBlock.DataReader graphData = GetGraphData(enumerator.Current);
                        UpdateStatsSeparated(ref stats, graphData, lastFrameIndex, windowEnd);
                    }
                }
                if (stats.MaxCount > 0)
                {
                    if (stats.MaxCount > 10)
                    {
                        currentGraphScale.IncreaseYRange();
                        UpdateAutoScale(lastFrameIndex);
                    }
                }
                else if ((stats.MinCount < 10) && stats.Any)
                {
                    currentGraphScale.DecreaseYRange();
                    UpdateAutoScale(lastFrameIndex);
                }
                m_nextAutoScale = span + AUTO_SCALE_UPDATE;
            }
        }

        private static unsafe void UpdateStats(ref MyStats stats, MyProfilerBlock.DataReader data, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                float num2 = data[i];
                if (num2 > 0.01f)
                {
                    if (num2 > stats.Min)
                    {
                        int* numPtr1 = (int*) ref stats.MinCount;
                        numPtr1[0]++;
                        if (num2 > stats.Max)
                        {
                            int* numPtr2 = (int*) ref stats.MaxCount;
                            numPtr2[0]++;
                        }
                    }
                    stats.Any = true;
                }
            }
        }

        private static void UpdateStatsSeparated(ref MyStats stats, MyProfilerBlock.DataReader data, int lastFrameIndex, int windowEnd)
        {
            if (lastFrameIndex > windowEnd)
            {
                UpdateStats(ref stats, data, windowEnd, lastFrameIndex);
            }
            else
            {
                UpdateStats(ref stats, data, 0, lastFrameIndex);
                UpdateStats(ref stats, data, windowEnd, MyProfiler.MAX_FRAMES - 1);
            }
        }

        protected static bool ProfilerProcessingEnabled =>
            m_profilerProcessingEnabled;

        public static bool ProfilerVisible =>
            m_enabled;

        private static MyProfiler GpuProfiler
        {
            get
            {
                MyProfiler gpuProfiler = m_gpuProfiler;
                if (gpuProfiler == null)
                {
                    m_gpuProfiler = gpuProfiler = CreateProfiler("GPU", null, false);
                    gpuProfiler.ViewPriority = 30;
                    List<MyProfiler> threadProfilers = ThreadProfilers;
                    lock (threadProfilers)
                    {
                        SortProfilersLocked();
                    }
                }
                return gpuProfiler;
            }
        }

        public static MyProfiler ThreadProfiler =>
            (m_threadProfiler ?? (m_threadProfiler = CreateProfiler(null, null, false)));

        public static MyProfiler SelectedProfiler
        {
            get => 
                m_selectedProfiler;
            set => 
                (m_selectedProfiler = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRenderProfiler.<>c <>9 = new MyRenderProfiler.<>c();
            public static Comparison<MyProfilerBlock> <>9__47_0;
            public static Comparison<MyProfilerBlock> <>9__47_2;
            public static Comparison<MyProfiler> <>9__100_0;

            internal int <GetSortedChildren>b__47_0(MyProfilerBlock a, MyProfilerBlock b) => 
                a.Id.CompareTo(b.Id);

            internal int <GetSortedChildren>b__47_2(MyProfilerBlock a, MyProfilerBlock b)
            {
                int num = b.AverageMilliseconds.CompareTo(a.AverageMilliseconds);
                if (num != 0)
                {
                    return num;
                }
                return a.Id.CompareTo(b.Id);
            }

            internal int <SortProfilersLocked>b__100_0(MyProfiler x, MyProfiler y) => 
                x.ViewPriority.CompareTo(y.ViewPriority);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrameInfo
        {
            public long Time;
            public long FrameNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyStats
        {
            public float Min;
            public float Max;
            public int MinCount;
            public int MaxCount;
            public bool Any;
        }
    }
}

