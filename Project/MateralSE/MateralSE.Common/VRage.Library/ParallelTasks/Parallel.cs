namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Profiler;

    public static class Parallel
    {
        public static readonly bool THROW_WORKER_EXCEPTIONS = false;
        public static readonly WorkOptions DefaultOptions;
        private static IWorkScheduler scheduler;
        private static Pool<List<Task>> taskPool;
        private static readonly Dictionary<Thread, ConcurrentCachingList<WorkItem>> Buffers;
        [ThreadStatic]
        private static ConcurrentCachingList<WorkItem> m_callbackBuffer;
        private static int[] _processorAffinity;

        static Parallel()
        {
            WorkOptions options = new WorkOptions {
                MaximumThreads = 1,
                TaskType = MyProfiler.TaskType.WorkItem
            };
            DefaultOptions = options;
            taskPool = new Pool<List<Task>>();
            Buffers = new Dictionary<Thread, ConcurrentCachingList<WorkItem>>(8);
            _processorAffinity = new int[] { 3, 4, 5, 1 };
        }

        public static void Clean()
        {
            CallbackBuffer.ApplyChanges();
            CallbackBuffer.ClearList();
            taskPool.Clean();
            Dictionary<Thread, ConcurrentCachingList<WorkItem>> buffers = Buffers;
            lock (buffers)
            {
                foreach (ConcurrentCachingList<WorkItem> list in Buffers.Values)
                {
                    list.ClearImmediate();
                }
                Buffers.Clear();
            }
            WorkItem.Clean();
        }

        public static void Do(params IWork[] work)
        {
            List<Task> instance = taskPool.Get(Thread.CurrentThread);
            for (int i = 0; i < work.Length; i++)
            {
                instance.Add(Start(work[i]));
            }
            for (int j = 0; j < instance.Count; j++)
            {
                instance[j].WaitOrExecute(false);
            }
            instance.Clear();
            taskPool.Return(Thread.CurrentThread, instance);
        }

        public static void Do(params Action[] actions)
        {
            List<Task> instance = taskPool.Get(Thread.CurrentThread);
            for (int i = 0; i < actions.Length; i++)
            {
                DelegateWork work = DelegateWork.GetInstance();
                work.Action = actions[i];
                work.Options = DefaultOptions;
                instance.Add(Start(work));
            }
            for (int j = 0; j < actions.Length; j++)
            {
                instance[j].WaitOrExecute(false);
            }
            instance.Clear();
            taskPool.Return(Thread.CurrentThread, instance);
        }

        public static void Do(IWork a, IWork b)
        {
            Task task = Start(b);
            a.DoWork(null);
            task.WaitOrExecute(false);
        }

        public static void Do(Action action1, Action action2)
        {
            DelegateWork instance = DelegateWork.GetInstance();
            instance.Action = action2;
            instance.Options = DefaultOptions;
            Task task = Start(instance);
            action1();
            task.WaitOrExecute(false);
        }

        public static void For(int startInclusive, int endExclusive, Action<int> body, WorkPriority priority = 2, WorkOptions? options = new WorkOptions?())
        {
            For(startInclusive, endExclusive, body, 1, priority, options, false);
        }

        public static void For(int startInclusive, int endExclusive, Action<int> body, int stride, WorkPriority priority = 2, WorkOptions? options = new WorkOptions?(), bool blocking = false)
        {
            int num2 = ((endExclusive - startInclusive) + (stride - 1)) / stride;
            if (num2 > 0)
            {
                if (num2 == 1)
                {
                    body(startInclusive);
                }
                else
                {
                    ForLoopWork work = ForLoopWork.Get();
                    work.Prepare(body, startInclusive, endExclusive, stride, priority);
                    WorkOptions? nullable = options;
                    WorkOptions options2 = (nullable != null) ? nullable.GetValueOrDefault() : DefaultOptions;
                    options2.MaximumThreads = num2;
                    work.Options = options2;
                    Start(work).WaitOrExecute(blocking);
                    work.Return();
                }
            }
        }

        public static void ForEach<T>(IEnumerable<T> collection, Action<T> action, WorkPriority priority = 2, WorkOptions? options = new WorkOptions?(), bool blocking = false)
        {
            WorkOptions defaultOptions;
            ForEachLoopWork<T> work = ForEachLoopWork<T>.Get();
            work.Prepare(action, collection.GetEnumerator(), priority);
            if (options != null)
            {
                defaultOptions = options.Value;
            }
            else
            {
                defaultOptions = DefaultOptions;
                defaultOptions.MaximumThreads = 0x7fffffff;
            }
            work.Options = defaultOptions;
            Start(work).WaitOrExecute(blocking);
            work.Return();
        }

        public static void RunCallbacks()
        {
            CallbackBuffer.ApplyChanges();
            for (int i = 0; i < CallbackBuffer.Count; i++)
            {
                WorkItem item = CallbackBuffer[i];
                if (item != null)
                {
                    if (item.Callback != null)
                    {
                        item.Callback();
                        item.Callback = null;
                    }
                    if (item.DataCallback != null)
                    {
                        item.DataCallback(item.WorkData);
                        item.DataCallback = null;
                    }
                    item.WorkData = null;
                    item.Requeue();
                }
            }
            CallbackBuffer.ClearList();
        }

        public static Task ScheduleForThread(Action<WorkData> action, WorkData workData, Thread thread = null)
        {
            if (thread == null)
            {
                thread = Thread.CurrentThread;
            }
            WorkOptions options2 = new WorkOptions {
                MaximumThreads = 1,
                QueueFIFO = false
            };
            DelegateWork instance = DelegateWork.GetInstance();
            instance.Options = options2;
            WorkItem entity = WorkItem.Get();
            Dictionary<Thread, ConcurrentCachingList<WorkItem>> buffers = Buffers;
            lock (buffers)
            {
                entity.CompletionCallbacks = Buffers[thread];
            }
            entity.DataCallback = action;
            entity.WorkData = workData;
            Task task = entity.PrepareStart(instance, null);
            entity.CompletionCallbacks.Add(entity);
            return task;
        }

        public static Task Start(IWork work) => 
            Start(work, null);

        public static Task Start(Action action) => 
            Start(action, (Action) null);

        public static Future<T> Start<T>(Func<T> function) => 
            Start<T>(function, (Action) null);

        public static Task Start(IWork work, Action completionCallback)
        {
            if (work == null)
            {
                throw new ArgumentNullException("work");
            }
            if (work.Options.MaximumThreads < 1)
            {
                throw new ArgumentException("work.Options.MaximumThreads cannot be less than one.");
            }
            WorkItem item = WorkItem.Get();
            if (completionCallback != null)
            {
                item.CompletionCallbacks = CallbackBuffer;
                item.Callback = completionCallback;
            }
            item.WorkData = null;
            Task task = item.PrepareStart(work, null);
            Scheduler.Schedule(task);
            return task;
        }

        public static Task Start(Action action, WorkOptions options) => 
            Start(action, options, null);

        public static Task Start(Action action, Action completionCallback)
        {
            WorkOptions options = new WorkOptions {
                MaximumThreads = 1,
                QueueFIFO = false
            };
            return Start(action, options, completionCallback);
        }

        public static Future<T> Start<T>(Func<T> function, WorkOptions options) => 
            Start<T>(function, options, null);

        public static Future<T> Start<T>(Func<T> function, Action completionCallback) => 
            Start<T>(function, DefaultOptions, completionCallback);

        public static Task Start(Action action, WorkOptions options, Action completionCallback)
        {
            if (options.MaximumThreads != 1)
            {
                throw new ArgumentOutOfRangeException("options", "options.MaximumThreads has to be 1 for delegate work");
            }
            DelegateWork instance = DelegateWork.GetInstance();
            instance.Action = action;
            instance.Options = options;
            return Start(instance, completionCallback);
        }

        public static Task Start(Action<WorkData> action, Action<WorkData> completionCallback, WorkData workData)
        {
            WorkOptions options2 = new WorkOptions {
                MaximumThreads = 1,
                QueueFIFO = false
            };
            DelegateWork instance = DelegateWork.GetInstance();
            instance.DataAction = action;
            instance.Options = options2;
            WorkItem item = WorkItem.Get();
            if (completionCallback != null)
            {
                item.CompletionCallbacks = CallbackBuffer;
                item.DataCallback = completionCallback;
            }
            item.WorkData = workData;
            Task task = item.PrepareStart(instance, null);
            Scheduler.Schedule(task);
            return task;
        }

        public static Future<T> Start<T>(Func<T> function, WorkOptions options, Action completionCallback)
        {
            if (options.MaximumThreads < 1)
            {
                throw new ArgumentOutOfRangeException("options", "options.MaximumThreads cannot be less than 1.");
            }
            FutureWork<T> instance = FutureWork<T>.GetInstance();
            instance.Function = function;
            instance.Options = options;
            return new Future<T>(Start(instance, completionCallback), instance);
        }

        public static Task StartBackground(IWork work) => 
            StartBackground(work, null);

        public static Task StartBackground(Action action) => 
            StartBackground(action, null);

        public static Task StartBackground(IWork work, Action completionCallback)
        {
            if (work == null)
            {
                throw new ArgumentNullException("work");
            }
            if (work.Options.MaximumThreads < 1)
            {
                throw new ArgumentException("work.Options.MaximumThreads cannot be less than one.");
            }
            WorkItem item = WorkItem.Get();
            if (completionCallback != null)
            {
                item.CompletionCallbacks = CallbackBuffer;
                item.Callback = completionCallback;
            }
            item.WorkData = null;
            Task task = item.PrepareStart(work, null);
            BackgroundWorker.StartWork(task);
            return task;
        }

        public static Task StartBackground(Action action, Action completionCallback)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            DelegateWork instance = DelegateWork.GetInstance();
            instance.Action = action;
            instance.Options = DefaultOptions;
            return StartBackground(instance, completionCallback);
        }

        public static Task StartBackground(Action<WorkData> action, Action<WorkData> completionCallback, WorkData workData)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            DelegateWork instance = DelegateWork.GetInstance();
            instance.DataAction = action;
            instance.Options = DefaultOptions;
            WorkItem item = WorkItem.Get();
            if (completionCallback != null)
            {
                item.CompletionCallbacks = CallbackBuffer;
                item.DataCallback = completionCallback;
            }
            item.WorkData = workData;
            Task work = item.PrepareStart(instance, null);
            BackgroundWorker.StartWork(work);
            return work;
        }

        public static void StartOnEachWorker(Action action)
        {
            Scheduler.ScheduleOnEachWorker(action);
        }

        public static bool WaitForAll(WaitHandle[] waitHandles, TimeSpan timeout)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                return WaitHandle.WaitAll(waitHandles, timeout);
            }
            bool result = false;
            Thread thread = new Thread(delegate {
                result = WaitHandle.WaitAll(waitHandles, timeout);
            });
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();
            return result;
        }

        public static ConcurrentCachingList<WorkItem> CallbackBuffer
        {
            get
            {
                if (m_callbackBuffer == null)
                {
                    m_callbackBuffer = new ConcurrentCachingList<WorkItem>(0x10);
                    Dictionary<Thread, ConcurrentCachingList<WorkItem>> buffers = Buffers;
                    lock (buffers)
                    {
                        Buffers.Add(Thread.CurrentThread, m_callbackBuffer);
                    }
                }
                return m_callbackBuffer;
            }
        }

        public static int[] ProcessorAffinity
        {
            get => 
                _processorAffinity;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length < 1)
                {
                    throw new ArgumentException("The Parallel.ProcessorAffinity must contain at least one value.", "value");
                }
                if (value.Any<int>(id => id < 0))
                {
                    throw new ArgumentException("The processor affinity must not be negative.", "value");
                }
                _processorAffinity = value;
            }
        }

        public static IWorkScheduler Scheduler
        {
            get
            {
                if (Parallel.scheduler == null)
                {
                    IWorkScheduler scheduler = new WorkStealingScheduler();
                    Interlocked.CompareExchange<IWorkScheduler>(ref Parallel.scheduler, scheduler, null);
                }
                return Parallel.scheduler;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                Interlocked.Exchange<IWorkScheduler>(ref scheduler, value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Parallel.<>c <>9 = new Parallel.<>c();
            public static Func<int, bool> <>9__11_0;

            internal bool <set_ProcessorAffinity>b__11_0(int id) => 
                (id < 0);
        }
    }
}

