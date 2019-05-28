namespace VRage.Profiler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;

    [DebuggerDisplay("{DisplayName} Blocks({m_profilingBlocks.Count}) Tasks({FinishedTasks.Count})")]
    public class MyProfiler
    {
        public static long LastInterestingFrameTime;
        public static long LastFrameTime;
        private static readonly bool m_enableAsserts = true;
        public static readonly int MAX_FRAMES = 0x400;
        public static readonly int UPDATE_WINDOW = 0x10;
        private const int ROOT_ID = 0;
        private int m_nextId = 1;
        private Dictionary<MyProfilerBlockKey, MyProfilerBlock> m_profilingBlocks = new Dictionary<MyProfilerBlockKey, MyProfilerBlock>(0x2000, new MyProfilerBlockKeyComparer());
        private List<MyProfilerBlock> m_rootBlocks = new List<MyProfilerBlock>(0x20);
        private readonly Stack<MyProfilerBlock> m_currentProfilingStack = new Stack<MyProfilerBlock>(0x400);
        private int m_levelLimit = -1;
        private int m_levelSkipCount;
        private volatile int m_newLevelLimit = -1;
        private int m_remainingWindow = UPDATE_WINDOW;
        private readonly FastResourceLock m_historyLock = new FastResourceLock();
        public readonly object TaskLock = new object();
        private string m_customName;
        private string m_axisName;
        private readonly Dictionary<MyProfilerBlockKey, MyProfilerBlock> m_blocksToAdd = new Dictionary<MyProfilerBlockKey, MyProfilerBlock>(0x2000, new MyProfilerBlockKeyComparer());
        private volatile int m_lastFrameIndex;
        public int[] TotalCalls = new int[MAX_FRAMES];
        public long[] CommitTimes = new long[MAX_FRAMES];
        public bool AutoCommit = true;
        public bool AutoScale;
        public bool IgnoreRoot;
        public bool AverageTimes;
        private bool AssertCommitFromOwningThread = true;
        public int ViewPriority;
        public bool EnableOptimizations = true;
        private int m_shallowMarker;
        public bool ShallowProfileEnabled;
        public volatile bool PendingShallowProfileState;
        public readonly bool AllocationProfiling;
        private readonly Thread m_ownerThread = Thread.CurrentThread;
        public bool Paused;
        private readonly Stack<TaskInfo> m_runningTasks = new Stack<TaskInfo>(5);
        private readonly List<TaskInfo> m_pendingTasks = new List<TaskInfo>();
        public MyQueue<TaskInfo> FinishedTasks = new MyQueue<TaskInfo>(100);

        public MyProfiler(bool allocationProfiling, string name, string axisName, bool shallowProfile, int viewPriority = 0x3e8)
        {
            this.m_customName = name ?? this.m_ownerThread.Name;
            this.m_axisName = axisName;
            this.AllocationProfiling = allocationProfiling;
            this.PendingShallowProfileState = this.ShallowProfileEnabled = shallowProfile;
            this.m_lastFrameIndex = MAX_FRAMES - 1;
            this.ViewPriority = viewPriority;
        }

        [Conditional("DEBUG")]
        private static void CheckEndBlock(MyProfilerBlock profilingBlock, string member, string file, int parentId)
        {
            if (((m_enableAsserts && !profilingBlock.Key.Member.Equals(member)) || (profilingBlock.Key.ParentId != parentId)) || (profilingBlock.Key.File != file))
            {
                StackTrace trace = new StackTrace(2, true);
                for (int i = 0; i < trace.FrameCount; i++)
                {
                    StackFrame frame = trace.GetFrame(i);
                    if ((frame.GetFileName() == profilingBlock.Key.File) && (frame.GetMethod().Name == member))
                    {
                        return;
                    }
                }
            }
        }

        public void ClearFrame()
        {
            bool assertCommitFromOwningThread = this.AssertCommitFromOwningThread;
            this.m_currentProfilingStack.Clear();
            if (this.m_blocksToAdd.Count > 0)
            {
                this.m_blocksToAdd.Clear();
            }
            this.m_levelLimit = this.m_newLevelLimit;
            foreach (MyProfilerBlock block in this.m_profilingBlocks.Values)
            {
                block.Clear();
            }
            this.m_pendingTasks.Clear();
        }

        public void CommitFrame()
        {
            this.CommitInternal();
        }

        private void CommitInternal()
        {
            int num;
            bool assertCommitFromOwningThread = this.AssertCommitFromOwningThread;
            this.m_shallowMarker = 0;
            this.m_currentProfilingStack.Clear();
            if (this.m_blocksToAdd.Count > 0)
            {
                using (this.m_historyLock.AcquireExclusiveUsing())
                {
                    foreach (KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> pair in this.m_blocksToAdd)
                    {
                        if (pair.Value.Parent != null)
                        {
                            pair.Value.Parent.Children.AddOrInsert<MyProfilerBlock>(pair.Value, pair.Value.ForceOrder);
                        }
                        else
                        {
                            this.m_rootBlocks.AddOrInsert<MyProfilerBlock>(pair.Value, pair.Value.ForceOrder);
                        }
                        this.m_profilingBlocks.Add(pair.Key, pair.Value);
                    }
                    this.m_blocksToAdd.Clear();
                    Interlocked.Exchange(ref this.m_remainingWindow, UPDATE_WINDOW - 1);
                    goto TR_001E;
                }
            }
            if (this.m_historyLock.TryAcquireExclusive())
            {
                Interlocked.Exchange(ref this.m_remainingWindow, UPDATE_WINDOW - 1);
                this.m_historyLock.ReleaseExclusive();
            }
            else if (Interlocked.Decrement(ref this.m_remainingWindow) < 0)
            {
                using (this.m_historyLock.AcquireExclusiveUsing())
                {
                    Interlocked.Exchange(ref this.m_remainingWindow, UPDATE_WINDOW - 1);
                }
            }
        TR_001E:
            num = 0;
            this.m_levelLimit = this.m_newLevelLimit;
            int index = (this.m_lastFrameIndex + 1) % MAX_FRAMES;
            foreach (MyProfilerBlock block in this.m_profilingBlocks.Values)
            {
                num += block.NumCalls;
                block.NumCallsArray[index] = block.NumCalls;
                block.CustomValues[index] = block.CustomValue;
                block.RawAllocations[index] = block.Allocated;
                block.AverageMilliseconds = (0.9f * block.AverageMilliseconds) + (0.1f * ((float) block.Elapsed.Milliseconds));
                block.RawMilliseconds[index] = this.AverageTimes ? block.AverageMilliseconds : ((float) block.Elapsed.Milliseconds);
                block.Clear();
            }
            bool flag = this.m_pendingTasks.Count > 0;
            if (flag)
            {
                this.m_pendingTasks.SortNoAlloc<TaskInfo>((x, y) => x.Started.CompareTo(y.Started));
            }
            object taskLock = this.TaskLock;
            lock (taskLock)
            {
                if (flag)
                {
                    foreach (TaskInfo info in this.m_pendingTasks)
                    {
                        if (this.FinishedTasks.Count >= 0x989680)
                        {
                            this.FinishedTasks.Dequeue();
                        }
                        this.FinishedTasks.Enqueue(info);
                    }
                    this.m_pendingTasks.Clear();
                }
                long lastInterestingFrameTime = LastInterestingFrameTime;
                while ((this.FinishedTasks.Count > 0) && (this.FinishedTasks.Peek().Finished < lastInterestingFrameTime))
                {
                    this.FinishedTasks.Dequeue();
                }
            }
            this.m_lastFrameIndex = index;
            this.TotalCalls[index] = num;
            this.CommitTimes[index] = Stopwatch.GetTimestamp();
            this.ShallowProfileEnabled = this.PendingShallowProfileState;
        }

        public void CommitTask(TaskInfo task)
        {
            this.m_pendingTasks.Add(task);
            if (this.m_runningTasks.Count == 0)
            {
                this.CommitWorklogIfNeeded();
            }
        }

        private void CommitWorklogIfNeeded()
        {
            if (this.AutoCommit && (this.m_currentProfilingStack.Count <= 0))
            {
                if (this.Paused)
                {
                    this.ClearFrame();
                }
                else
                {
                    this.CommitInternal();
                }
            }
        }

        public static MyProfilerBlock CreateExternalBlock(string name, int blockId)
        {
            MyProfilerBlockKey key = new MyProfilerBlockKey(string.Empty, string.Empty, name, 0, 0);
            MyProfilerBlock block = new MyProfilerBlock();
            block.SetBlockData(ref key, blockId, 0x7fffffff, true);
            return block;
        }

        public StringBuilder Dump()
        {
            StringBuilder sb = new StringBuilder();
            foreach (MyProfilerBlock block in this.m_rootBlocks)
            {
                block.Dump(sb, this.m_lastFrameIndex);
            }
            return sb;
        }

        public void EndBlock(string member, int line, string file, MyTimeSpan? customTime = new MyTimeSpan?(), float customValue = 0f, string timeFormat = null, string valueFormat = null, string callFormat = null)
        {
            if (this.m_levelSkipCount > 0)
            {
                this.m_levelSkipCount--;
            }
            else
            {
                if (this.m_currentProfilingStack.Count > 0)
                {
                    MyProfilerBlock block = this.m_currentProfilingStack.Pop();
                    block.CustomValue = Math.Max(block.CustomValue, customValue);
                    block.TimeFormat = timeFormat;
                    block.ValueFormat = valueFormat;
                    block.CallFormat = callFormat;
                    block.End(this.AllocationProfiling, customTime);
                    if (block.IsDeepTreeRoot)
                    {
                        this.m_shallowMarker--;
                    }
                }
                this.CommitWorklogIfNeeded();
            }
        }

        public MyProfilerObjectBuilderInfo GetObjectBuilderInfo()
        {
            MyProfilerObjectBuilderInfo info1 = new MyProfilerObjectBuilderInfo();
            info1.ProfilingBlocks = this.m_profilingBlocks;
            info1.RootBlocks = this.m_rootBlocks;
            info1.CustomName = this.m_customName;
            info1.AxisName = this.m_axisName;
            info1.TotalCalls = this.TotalCalls;
            info1.ShallowProfile = this.ShallowProfileEnabled;
            info1.CommitTimes = this.CommitTimes;
            MyProfilerObjectBuilderInfo info = info1;
            object taskLock = this.TaskLock;
            lock (taskLock)
            {
                info.Tasks = new List<TaskInfo>(this.FinishedTasks.Count);
                info.Tasks.AddRange(this.FinishedTasks);
            }
            return info;
        }

        private int GetParentId() => 
            ((this.m_currentProfilingStack.Count <= 0) ? 0 : this.m_currentProfilingStack.Peek().Id);

        public void Init(MyProfilerObjectBuilderInfo data)
        {
            this.m_profilingBlocks = data.ProfilingBlocks;
            foreach (KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> pair in this.m_profilingBlocks)
            {
                if (pair.Value.Id >= this.m_nextId)
                {
                    this.m_nextId = pair.Value.Id + 1;
                }
            }
            this.m_rootBlocks = data.RootBlocks;
            this.m_customName = data.CustomName;
            this.m_axisName = data.AxisName;
            this.TotalCalls = data.TotalCalls;
            this.CommitTimes = data.CommitTimes ?? new long[MAX_FRAMES];
            this.PendingShallowProfileState = this.ShallowProfileEnabled = data.ShallowProfile;
            this.FinishedTasks = new MyQueue<TaskInfo>(data.Tasks);
        }

        public void InitMemoryHack(string name)
        {
            this.StartBlock(name, "InitMemoryHack", 0, string.Empty, 0x7fffffff, false);
            MyProfilerBlock block = this.m_currentProfilingStack.Peek();
            MyTimeSpan? customTime = null;
            this.EndBlock("InitMemoryHack", 0, string.Empty, customTime, 0f, null, null, null);
            GC.GetTotalMemory(true);
            if (this.AllocationProfiling)
            {
                long workingSet = Environment.WorkingSet;
            }
        }

        public HistoryLock LockHistory(out int lastValidFrame)
        {
            HistoryLock @lock = new HistoryLock(this, this.m_historyLock);
            lastValidFrame = this.m_lastFrameIndex;
            return @lock;
        }

        private void OnHistorySafe()
        {
            Interlocked.Exchange(ref this.m_remainingWindow, UPDATE_WINDOW);
        }

        public void OnTaskFinished(TaskType? taskType, float customValue)
        {
            if (this.m_runningTasks.Count != 0)
            {
                TaskInfo task = this.m_runningTasks.Pop();
                task.Finished = Stopwatch.GetTimestamp();
                task.CustomValue = customValue;
                if (taskType != null)
                {
                    task.TaskType = taskType.Value;
                }
                this.CommitTask(task);
            }
        }

        public void OnTaskStarted(TaskType taskType, string name, long scheduledTimestamp)
        {
            TaskInfo item = new TaskInfo {
                Name = name,
                TaskType = taskType,
                Started = Stopwatch.GetTimestamp(),
                Scheduled = scheduledTimestamp
            };
            this.m_runningTasks.Push(item);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public void ProfileCustomValue(string name, string member, int line, string file, float value, MyTimeSpan? customTime, string timeFormat, string valueFormat, string callFormat = null)
        {
            this.StartBlock(name, member, line, file, 0x7fffffff, false);
            this.EndBlock(member, line, file, customTime, value, timeFormat, valueFormat, callFormat);
        }

        public void Reset()
        {
            using (new HistoryLock(this, this.m_historyLock))
            {
                foreach (MyProfilerBlock block in this.m_profilingBlocks.Values)
                {
                    block.AverageMilliseconds = 0f;
                    for (int i = 0; i < MAX_FRAMES; i++)
                    {
                        block.CustomValues[i] = 0f;
                        block.NumCallsArray[i] = 0;
                        block.RawAllocations[i] = 0f;
                        block.RawMilliseconds[i] = 0f;
                    }
                }
                this.m_lastFrameIndex = MAX_FRAMES - 1;
            }
            object taskLock = this.TaskLock;
            lock (taskLock)
            {
                this.FinishedTasks.Clear();
            }
        }

        public void SetNewLevelLimit(int newLevelLimit)
        {
            this.m_newLevelLimit = newLevelLimit;
        }

        public void SetShallowProfile(bool shallowProfile)
        {
            this.PendingShallowProfileState = shallowProfile;
        }

        public void StartBlock(string name, string memberName, int line, string file, int forceOrder = 0x7fffffff, bool isDeepTreeRoot = false)
        {
            if (((this.m_levelLimit != -1) && (this.m_currentProfilingStack.Count >= this.m_levelLimit)) || ((this.m_shallowMarker > 0) && this.ShallowProfileEnabled))
            {
                this.m_levelSkipCount++;
            }
            else
            {
                if (isDeepTreeRoot)
                {
                    this.m_shallowMarker++;
                }
                MyProfilerBlock block = null;
                MyProfilerBlockKey key = new MyProfilerBlockKey(file, memberName, name, line, this.GetParentId());
                if (!this.m_profilingBlocks.TryGetValue(key, out block) && !this.m_blocksToAdd.TryGetValue(key, out block))
                {
                    block = new MyProfilerBlock();
                    int nextId = this.m_nextId;
                    this.m_nextId = nextId + 1;
                    block.SetBlockData(ref key, nextId, forceOrder, isDeepTreeRoot);
                    if (this.m_currentProfilingStack.Count > 0)
                    {
                        block.Parent = this.m_currentProfilingStack.Peek();
                    }
                    this.m_blocksToAdd.Add(key, block);
                }
                block.Start(this.AllocationProfiling);
                this.m_currentProfilingStack.Push(block);
            }
        }

        public void SubtractFrom(MyProfiler otherProfiler)
        {
            Stack<MyProfilerBlock> stack;
            Dictionary<MyProfilerBlock, MyProfilerBlock> dictionary = new Dictionary<MyProfilerBlock, MyProfilerBlock>();
            using (Dictionary<MyProfilerBlockKey, MyProfilerBlock>.Enumerator enumerator = this.m_profilingBlocks.GetEnumerator())
            {
                KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> current;
                bool flag;
                goto TR_002D;
            TR_001B:
                if (!flag)
                {
                    current.Value.Invert();
                }
            TR_002D:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        flag = false;
                        using (Dictionary<MyProfilerBlockKey, MyProfilerBlock>.Enumerator enumerator2 = otherProfiler.m_profilingBlocks.GetEnumerator())
                        {
                            while (true)
                            {
                                while (true)
                                {
                                    if (enumerator2.MoveNext())
                                    {
                                        KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> current = enumerator2.Current;
                                        MyProfilerBlockKey key = current.Key;
                                        if (!key.IsSimilarLocation(current.Key))
                                        {
                                            continue;
                                        }
                                        MyProfilerBlock parent = current.Value;
                                        MyProfilerBlock block2 = current.Value;
                                        while (true)
                                        {
                                            if (((parent.Parent != null) && (block2.Parent != null)) && parent.Parent.Key.IsSimilarLocation(block2.Parent.Key))
                                            {
                                                parent = parent.Parent;
                                                block2 = block2.Parent;
                                                continue;
                                            }
                                            if ((parent.Parent == null) && (block2.Parent == null))
                                            {
                                                flag = true;
                                                current.Value.SubtractFrom(current.Value);
                                                dictionary.Add(current.Value, current.Value);
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        goto TR_0018;
                    }
                    break;
                }
                goto TR_001B;
            }
        TR_0018:
            stack = new Stack<MyProfilerBlock>();
            foreach (KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> pair3 in otherProfiler.m_profilingBlocks)
            {
                if (!dictionary.ContainsKey(pair3.Value))
                {
                    MyProfilerBlock item = pair3.Value;
                    stack.Push(item);
                    while (true)
                    {
                        if ((item.Parent == null) || dictionary.ContainsKey(item.Parent))
                        {
                            MyProfilerBlock parent = (item.Parent != null) ? dictionary[item.Parent] : null;
                            while (true)
                            {
                                if (stack.Count <= 0)
                                {
                                    stack.Clear();
                                    break;
                                }
                                MyProfilerBlock key = stack.Pop();
                                int nextId = this.m_nextId;
                                this.m_nextId = nextId + 1;
                                MyProfilerBlock block6 = key.Duplicate(nextId, parent);
                                if (parent == null)
                                {
                                    this.m_rootBlocks.Add(block6);
                                }
                                this.m_profilingBlocks.Add(block6.Key, block6);
                                parent = block6;
                                dictionary.Add(key, block6);
                            }
                            break;
                        }
                        item = item.Parent;
                        stack.Push(item);
                    }
                }
            }
            for (int i = 0; i < MAX_FRAMES; i++)
            {
                this.TotalCalls[i] = otherProfiler.TotalCalls[i] - this.TotalCalls[i];
            }
        }

        public MyProfilerBlock SelectedRoot { get; set; }

        public List<MyProfilerBlock> SelectedRootChildren =>
            ((this.SelectedRoot != null) ? this.SelectedRoot.Children : this.m_rootBlocks);

        public List<MyProfilerBlock> RootBlocks =>
            this.m_rootBlocks;

        public string DisplayName =>
            this.m_customName;

        public string AxisName =>
            this.m_axisName;

        public int LevelLimit =>
            this.m_levelLimit;

        public Thread OwnerThread =>
            this.m_ownerThread;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyProfiler.<>c <>9 = new MyProfiler.<>c();
            public static Comparison<MyProfiler.TaskInfo> <>9__59_0;

            internal int <CommitInternal>b__59_0(MyProfiler.TaskInfo x, MyProfiler.TaskInfo y) => 
                x.Started.CompareTo(y.Started);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HistoryLock : IDisposable
        {
            private readonly MyProfiler m_profiler;
            private FastResourceLock m_lock;
            public HistoryLock(MyProfiler profiler, FastResourceLock historyLock)
            {
                this.m_profiler = profiler;
                this.m_lock = historyLock;
                this.m_lock.AcquireExclusive();
                this.m_profiler.OnHistorySafe();
            }

            public void Dispose()
            {
                this.m_profiler.OnHistorySafe();
                this.m_lock.ReleaseExclusive();
                this.m_lock = null;
            }
        }

        public class MyProfilerObjectBuilderInfo
        {
            public Dictionary<MyProfilerBlockKey, MyProfilerBlock> ProfilingBlocks;
            public List<MyProfilerBlock> RootBlocks;
            public string CustomName;
            public string AxisName;
            public int[] TotalCalls;
            public long[] CommitTimes;
            public bool ShallowProfile;
            public List<MyProfiler.TaskInfo> Tasks;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TaskInfo
        {
            public long Started;
            public long Finished;
            public long Scheduled;
            public string Name;
            public VRage.Profiler.MyProfiler.TaskType TaskType;
            public float CustomValue;
        }

        public enum TaskType
        {
            None = 0,
            Wait = 1,
            SyncWait = 2,
            WorkItem = 3,
            Block = 4,
            Physics = 5,
            RenderCull = 6,
            Voxels = 7,
            Precalc = 8,
            Deformations = 9,
            PreparePass = 10,
            RenderPass = 11,
            ClipMap = 12,
            HK_Schedule = 0x65,
            HK_Execute = 0x66,
            HK_AwaitTasks = 0x67,
            HK_Finish = 0x68,
            HK_JOB_TYPE_DYNAMICS = 0x69,
            HK_JOB_TYPE_COLLIDE = 0x6a,
            HK_JOB_TYPE_COLLISION_QUERY = 0x6b,
            HK_JOB_TYPE_RAYCAST_QUERY = 0x6c,
            HK_JOB_TYPE_DESTRUCTION = 0x6d,
            HK_JOB_TYPE_CHARACTER_PROXY = 110,
            HK_JOB_TYPE_COLLIDE_STATIC_COMPOUND = 0x6f,
            HK_JOB_TYPE_OTHER = 0x70
        }
    }
}

