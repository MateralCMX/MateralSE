namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class PrioritizedScheduler : IWorkScheduler
    {
        private readonly int[] m_mappingPriorityToWorker = new int[] { 0, 1, 1, 1, 2 };
        private readonly ThreadPriority[] m_mappingWorkerToThreadPriority;
        private WorkerArray[] m_workerArrays;
        private WaitHandle[] m_hasNoWork;

        public PrioritizedScheduler(int threadCount)
        {
            ThreadPriority[] priorityArray1 = new ThreadPriority[3];
            priorityArray1[0] = ThreadPriority.Highest;
            priorityArray1[1] = ThreadPriority.Normal;
            this.m_mappingWorkerToThreadPriority = priorityArray1;
            this.InitializeWorkerArrays(threadCount);
        }

        private WorkerArray GetWorkerArray(WorkPriority priority) => 
            this.m_workerArrays[this.m_mappingPriorityToWorker[(int) priority]];

        private void InitializeWorkerArrays(int threadCount)
        {
            int num = 0;
            foreach (int num4 in this.m_mappingPriorityToWorker)
            {
                num = (num4 > num) ? num4 : num;
            }
            this.m_workerArrays = new WorkerArray[num + 1];
            this.m_hasNoWork = new WaitHandle[(num + 1) * threadCount];
            int index = 0;
            int num5 = 0;
            while (num5 <= num)
            {
                this.m_workerArrays[num5] = new WorkerArray(this, num5, threadCount, this.m_mappingWorkerToThreadPriority[num5]);
                int num6 = 0;
                while (true)
                {
                    if (num6 >= threadCount)
                    {
                        num5++;
                        break;
                    }
                    index++;
                    this.m_hasNoWork[index] = this.m_workerArrays[num5].Workers[num6].HasNoWork;
                    num6++;
                }
            }
        }

        public int ReadAndClearExecutionTime()
        {
            int num = 0;
            foreach (WorkerArray array in this.m_workerArrays)
            {
                num += array.ReadAndClearWorklog();
            }
            return num;
        }

        public void Schedule(Task task)
        {
            if (task.Item.Work != null)
            {
                WorkPriority priority = (task.Item.WorkData != null) ? task.Item.WorkData.Priority : WorkPriority.Normal;
                IPrioritizedWork work = task.Item.Work as IPrioritizedWork;
                if (work != null)
                {
                    priority = work.Priority;
                }
                this.GetWorkerArray(priority).Schedule(task);
            }
        }

        public void ScheduleOnEachWorker(Action action)
        {
            foreach (WorkerArray array in this.m_workerArrays)
            {
                array.ScheduleOnEachWorker(action);
            }
        }

        public bool WaitForTasksToFinish(TimeSpan waitTimeout) => 
            Parallel.WaitForAll(this.m_hasNoWork, waitTimeout);

        public int ThreadCount =>
            this.m_workerArrays[0].Workers.Length;

        private class Worker
        {
            private readonly PrioritizedScheduler.WorkerArray m_workerArray;
            private readonly int m_workerIndex;
            private readonly System.Threading.Thread m_thread;
            public readonly ManualResetEvent HasNoWork;
            public readonly AutoResetEvent Gate;
            private long Worklog;
            private int ExecutedWork;

            public Worker(PrioritizedScheduler.WorkerArray workerArray, string name, ThreadPriority priority, int workerIndex)
            {
                this.m_workerArray = workerArray;
                this.m_workerIndex = workerIndex;
                this.m_thread = new System.Threading.Thread(new ThreadStart(this.WorkerLoop));
                this.HasNoWork = new ManualResetEvent(false);
                this.Gate = new AutoResetEvent(false);
                this.m_thread.Name = name;
                this.m_thread.IsBackground = true;
                this.m_thread.Priority = priority;
                this.m_thread.CurrentCulture = CultureInfo.InvariantCulture;
                this.m_thread.CurrentUICulture = CultureInfo.InvariantCulture;
                this.m_thread.Start();
            }

            private void CloseWork()
            {
                long timestamp = Stopwatch.GetTimestamp();
                long num2 = Interlocked.Exchange(ref this.Worklog, 0L);
                long num1 = num2;
                int num3 = (int) (timestamp - num2);
                Interlocked.Add(ref this.ExecutedWork, num3);
            }

            private void OpenWork()
            {
                long timestamp = Stopwatch.GetTimestamp();
                long num2 = Interlocked.Exchange(ref this.Worklog, timestamp);
            }

            public int ReadAndClearWorklog()
            {
                long worklog = this.Worklog;
                if (worklog != 0)
                {
                    long timestamp = Stopwatch.GetTimestamp();
                    worklog = Interlocked.CompareExchange(ref this.Worklog, timestamp, worklog);
                }
                return Interlocked.Exchange(ref this.ExecutedWork, 0);
            }

            private void WorkerLoop()
            {
                while (true)
                {
                    Task task;
                    if (this.m_workerArray.TryGetTask(out task))
                    {
                        this.OpenWork();
                        task.DoWork();
                        this.CloseWork();
                        continue;
                    }
                    this.HasNoWork.Set();
                    this.Gate.WaitOne();
                    this.HasNoWork.Reset();
                }
            }

            public System.Threading.Thread Thread =>
                this.m_thread;
        }

        private class WorkerArray
        {
            private PrioritizedScheduler m_prioritizedScheduler;
            private readonly int m_workerArrayIndex;
            private readonly Queue<Task> m_taskQueue = new Queue<Task>(0x40);
            private long m_scheduledTaskCount;
            private readonly PrioritizedScheduler.Worker[] m_workers;
            private const int DEFAULT_QUEUE_CAPACITY = 0x40;

            public WorkerArray(PrioritizedScheduler prioritizedScheduler, int workerArrayIndex, int threadCount, ThreadPriority systemThreadPriority)
            {
                this.m_workerArrayIndex = workerArrayIndex;
                this.m_prioritizedScheduler = prioritizedScheduler;
                this.m_workers = new PrioritizedScheduler.Worker[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    object[] objArray1 = new object[] { "Parallel ", systemThreadPriority, "_", i };
                    this.m_workers[i] = new PrioritizedScheduler.Worker(this, string.Concat(objArray1), systemThreadPriority, i);
                }
            }

            public int ReadAndClearWorklog()
            {
                int num = 0;
                foreach (PrioritizedScheduler.Worker worker in this.m_workers)
                {
                    num += worker.ReadAndClearWorklog();
                }
                return num;
            }

            public void Schedule(Task task)
            {
                int maximumThreads = task.Item.Work.Options.MaximumThreads;
                if (maximumThreads < 1)
                {
                    maximumThreads = 1;
                }
                maximumThreads = Math.Min(maximumThreads, this.m_workers.Length);
                Queue<Task> taskQueue = this.m_taskQueue;
                lock (taskQueue)
                {
                    for (int i = 0; i < maximumThreads; i++)
                    {
                        this.m_taskQueue.Enqueue(task);
                    }
                }
                foreach (PrioritizedScheduler.Worker worker in this.m_workers)
                {
                    worker.Gate.Set();
                }
            }

            public void ScheduleOnEachWorker(Action action)
            {
                foreach (PrioritizedScheduler.Worker worker in this.Workers)
                {
                    DelegateWork instance = DelegateWork.GetInstance();
                    instance.Action = action;
                    WorkOptions options = new WorkOptions {
                        MaximumThreads = 1,
                        QueueFIFO = false
                    };
                    instance.Options = options;
                    WorkItem item = WorkItem.Get();
                    item.CompletionCallbacks = null;
                    item.Callback = null;
                    item.WorkData = null;
                    Task task = item.PrepareStart(instance, null);
                    Queue<Task> taskQueue = this.m_taskQueue;
                    lock (taskQueue)
                    {
                        this.m_taskQueue.Enqueue(task);
                    }
                    Interlocked.Increment(ref this.m_scheduledTaskCount);
                    worker.Gate.Set();
                    task.Wait(false);
                    worker.HasNoWork.WaitOne();
                }
            }

            public bool TryGetTask(out Task task)
            {
                Queue<Task> taskQueue = this.m_taskQueue;
                lock (taskQueue)
                {
                    return this.m_taskQueue.TryDequeue<Task>(out task);
                }
            }

            public PrioritizedScheduler.Worker[] Workers =>
                this.m_workers;
        }
    }
}

