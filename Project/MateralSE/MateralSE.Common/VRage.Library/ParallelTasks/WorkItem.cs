namespace ParallelTasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Library.Exceptions;
    using VRage.Profiler;

    public class WorkItem
    {
        private IWork m_work;
        private int m_executing;
        private long m_scheduledTimestamp;
        private volatile int m_runCount;
        private List<Exception> m_exceptionBuffer;
        private object m_executionLock = new object();
        private readonly ManualResetEvent m_resetEvent = new ManualResetEvent(true);
        private Exception[] m_exceptions;
        private static readonly MyConcurrentPool<WorkItem> m_idleWorkItems = new MyConcurrentPool<WorkItem>(0x3e8, null, 0x186a0, null);
        private static readonly ConcurrentDictionary<Thread, Stack<Task>> m_runningTasks = new ConcurrentDictionary<Thread, Stack<Task>>(Environment.ProcessorCount, Environment.ProcessorCount);
        public const string PerformanceProfilingSymbol = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";
        private static Action<MyProfiler.TaskType, string, long> m_onTaskStartedDelegate = delegate (MyProfiler.TaskType x, string y, long z) {
        };
        private static Action m_onTaskFinishedDelegate = delegate {
        };
        private static Action<string> m_onProfilerBeginDelegate = delegate (string x) {
        };
        private static Action<float> m_onProfilerEndDelegate = delegate (float x) {
        };
        private static Action<int> m_initThread = delegate (int x) {
        };

        public static void Clean()
        {
            m_idleWorkItems.Clean();
        }

        public bool DoWork(int expectedID)
        {
            Stack<Task> stack;
            bool flag2;
            object executionLock = this.m_executionLock;
            lock (executionLock)
            {
                if (expectedID >= this.m_runCount)
                {
                    if (this.m_work != null)
                    {
                        if (this.m_executing != this.m_work.Options.MaximumThreads)
                        {
                            this.m_executing++;
                            goto TR_001C;
                        }
                        else
                        {
                            flag2 = false;
                        }
                    }
                    else
                    {
                        flag2 = false;
                    }
                    return flag2;
                }
                else
                {
                    return true;
                }
            }
        TR_001C:
            stack = ThisThreadTasks;
            stack.Push(new Task(this));
            try
            {
                this.m_work.DoWork(this.WorkData);
            }
            catch (Exception exception)
            {
                if (Parallel.THROW_WORKER_EXCEPTIONS)
                {
                    MyMiniDump.CollectExceptionDump(exception, MyFileSystem.UserDataPath);
                    throw;
                }
                if (this.m_exceptionBuffer == null)
                {
                    List<Exception> list = new List<Exception>();
                    Interlocked.CompareExchange<List<Exception>>(ref this.m_exceptionBuffer, list, null);
                }
                List<Exception> exceptionBuffer = this.m_exceptionBuffer;
                lock (exceptionBuffer)
                {
                    this.m_exceptionBuffer.Add(exception);
                }
            }
            stack.Pop();
            object obj3 = this.m_executionLock;
            lock (obj3)
            {
                this.m_executing--;
                if (this.m_executing != 0)
                {
                    flag2 = false;
                }
                else
                {
                    if (this.m_exceptionBuffer != null)
                    {
                        this.m_exceptions = this.m_exceptionBuffer.ToArray();
                        this.m_exceptionBuffer = null;
                    }
                    this.m_runCount++;
                    this.m_resetEvent.Set();
                    if ((this.Callback != null) || (this.DataCallback != null))
                    {
                        this.CompletionCallbacks.Add(this);
                    }
                    else
                    {
                        this.Requeue();
                    }
                    flag2 = true;
                }
            }
            return flag2;
        }

        public void Execute(int id)
        {
            if ((this.m_runCount == id) && this.DoWork(id))
            {
                this.ThrowExceptionsInternal(id);
            }
        }

        public static WorkItem Get() => 
            m_idleWorkItems.Get();

        public Exception[] GetExceptions(int runId)
        {
            object executionLock = this.m_executionLock;
            lock (executionLock)
            {
                return this.GetExceptionsInternal(runId);
            }
        }

        public Exception[] GetExceptionsInternal(int runId)
        {
            int runCount = this.m_runCount;
            if ((this.m_exceptions == null) || (runCount != (runId + 1)))
            {
                return null;
            }
            return this.m_exceptions;
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void InitThread(int priority)
        {
            m_initThread(priority);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskFinished()
        {
            m_onTaskFinishedDelegate();
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskFinished(WorkItem task)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        private static void OnTaskScheduled(WorkItem task)
        {
            task.m_scheduledTimestamp = Stopwatch.GetTimestamp();
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskStarted(WorkItem task)
        {
            WorkOptions options = task.Work.Options;
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskStarted(MyProfiler.TaskType taskType, string debugName, long scheduledTimestamp = -1L)
        {
            m_onTaskStartedDelegate(taskType, debugName, scheduledTimestamp);
        }

        public Task PrepareStart(IWork work, Thread thread = null)
        {
            if (this.m_exceptions != null)
            {
                this.m_exceptions = null;
            }
            this.m_work = work;
            this.m_resetEvent.Reset();
            return new Task(this);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void ProfilerBegin(string symbol)
        {
            m_onProfilerBeginDelegate(symbol);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void ProfilerEnd(float customValue = 0f)
        {
            m_onProfilerEndDelegate(customValue);
        }

        public void Requeue()
        {
            if ((this.m_runCount < 0x7fffffff) && (this.m_exceptions == null))
            {
                this.m_work = null;
                m_idleWorkItems.Return(this);
            }
        }

        public static void SetupProfiler(Action<MyProfiler.TaskType, string, long> onTaskStarted, Action onTaskFinished, Action<string> begin, Action<float> end, Action<int> initThread)
        {
            m_onTaskStartedDelegate = onTaskStarted;
            m_onTaskFinishedDelegate = onTaskFinished;
            m_onProfilerBeginDelegate = begin;
            m_onProfilerEndDelegate = end;
            m_initThread = initThread;
        }

        private void ThrowExceptionsInternal(int runId)
        {
            Exception[] exceptionsInternal = this.GetExceptionsInternal(runId);
            if (exceptionsInternal != null)
            {
                throw new TaskException(exceptionsInternal);
            }
        }

        public void Wait(int id, bool blocking)
        {
            if (this.m_runCount == id)
            {
                try
                {
                    if (blocking)
                    {
                        while (this.m_runCount == id)
                        {
                        }
                    }
                    else
                    {
                        SpinWait wait = new SpinWait();
                        while (this.m_runCount == id)
                        {
                            if (wait.Count > 0x3e8)
                            {
                                this.m_resetEvent.WaitOne();
                                continue;
                            }
                            wait.SpinOnce();
                        }
                    }
                }
                finally
                {
                }
            }
        }

        public void WaitOrExecute(int id, bool blocking = false)
        {
            this.WaitOrExecuteInternal(id, blocking);
            this.ThrowExceptionsInternal(id);
        }

        private void WaitOrExecuteInternal(int id, bool blocking = false)
        {
            if ((this.m_runCount == id) && !this.DoWork(id))
            {
                this.Wait(id, blocking);
            }
        }

        public IWork Work =>
            this.m_work;

        public ParallelTasks.WorkData WorkData { get; set; }

        public Action Callback { get; set; }

        public Action<ParallelTasks.WorkData> DataCallback { get; set; }

        public ConcurrentCachingList<WorkItem> CompletionCallbacks { get; set; }

        public int RunCount =>
            this.m_runCount;

        public static Stack<Task> ThisThreadTasks
        {
            get
            {
                Stack<Task> stack;
                Thread currentThread = Thread.CurrentThread;
                if (!m_runningTasks.TryGetValue(currentThread, out stack))
                {
                    stack = new Stack<Task>(5);
                    m_runningTasks.TryAdd(currentThread, stack);
                }
                return stack;
            }
        }

        public static Task? CurrentTask
        {
            get
            {
                Stack<Task> thisThreadTasks = ThisThreadTasks;
                if (thisThreadTasks.Count != 0)
                {
                    return new Task?(thisThreadTasks.Peek());
                }
                return null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly WorkItem.<>c <>9 = new WorkItem.<>c();

            internal void <.cctor>b__62_0(MyProfiler.TaskType x, string y, long z)
            {
            }

            internal void <.cctor>b__62_1()
            {
            }

            internal void <.cctor>b__62_2(string x)
            {
            }

            internal void <.cctor>b__62_3(float x)
            {
            }

            internal void <.cctor>b__62_4(int x)
            {
            }
        }
    }
}

