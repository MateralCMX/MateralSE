namespace ParallelTasks
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;

    public class FixedPriorityScheduler : IWorkScheduler
    {
        private readonly MyConcurrentQueue<Task>[] m_taskQueuesByPriority = new MyConcurrentQueue<Task>[typeof(WorkPriority).GetEnumValues().Length];
        private readonly Worker[] m_workers;
        private readonly ManualResetEvent[] m_hasNoWork;
        private long m_scheduledTaskCount;

        public FixedPriorityScheduler(int threadCount, ThreadPriority priority)
        {
            for (int i = 0; i < this.m_taskQueuesByPriority.Length; i++)
            {
                this.m_taskQueuesByPriority[i] = new MyConcurrentQueue<Task>();
            }
            this.m_hasNoWork = new ManualResetEvent[threadCount];
            this.m_workers = new Worker[threadCount];
            for (int j = 0; j < threadCount; j++)
            {
                this.m_workers[j] = new Worker(this, "Parallel " + j, priority);
                this.m_hasNoWork[j] = this.m_workers[j].HasNoWork;
            }
        }

        public int ReadAndClearExecutionTime()
        {
            throw new NotImplementedException();
        }

        public void Schedule(Task task)
        {
            if (task.Item.Work != null)
            {
                WorkPriority normal = WorkPriority.Normal;
                IPrioritizedWork work = task.Item.Work as IPrioritizedWork;
                if (work != null)
                {
                    normal = work.Priority;
                }
                this.m_taskQueuesByPriority[(int) normal].Enqueue(task);
                Interlocked.Increment(ref this.m_scheduledTaskCount);
                foreach (Worker worker in this.m_workers)
                {
                    worker.Gate.Set();
                }
            }
        }

        public void ScheduleOnEachWorker(Action action)
        {
            foreach (Worker worker in this.m_workers)
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
                this.m_taskQueuesByPriority[0].Enqueue(task);
                Interlocked.Increment(ref this.m_scheduledTaskCount);
                worker.Gate.Set();
                task.Wait(false);
            }
        }

        private bool TryGetTask(out Task task)
        {
            while (this.m_scheduledTaskCount > 0L)
            {
                for (int i = 0; i < this.m_taskQueuesByPriority.Length; i++)
                {
                    if (this.m_taskQueuesByPriority[i].TryDequeue(out task))
                    {
                        Interlocked.Decrement(ref this.m_scheduledTaskCount);
                        return true;
                    }
                }
            }
            task = new Task();
            return false;
        }

        public bool WaitForTasksToFinish(TimeSpan waitTimeout) => 
            Parallel.WaitForAll(this.m_hasNoWork, waitTimeout);

        public int ThreadCount =>
            this.m_workers.Length;

        private class Worker
        {
            private readonly FixedPriorityScheduler m_scheduler;
            private readonly Thread m_thread;
            public readonly ManualResetEvent HasNoWork;
            public readonly AutoResetEvent Gate;

            public Worker(FixedPriorityScheduler scheduler, string name, ThreadPriority priority)
            {
                this.m_scheduler = scheduler;
                this.m_thread = new Thread(new ParameterizedThreadStart(this.WorkerLoop));
                this.HasNoWork = new ManualResetEvent(false);
                this.Gate = new AutoResetEvent(false);
                this.m_thread.Name = name;
                this.m_thread.IsBackground = true;
                this.m_thread.Priority = priority;
                this.m_thread.CurrentCulture = CultureInfo.InvariantCulture;
                this.m_thread.CurrentUICulture = CultureInfo.InvariantCulture;
                this.m_thread.Start(null);
            }

            private void WorkerLoop(object o)
            {
                while (true)
                {
                    Task task;
                    if (this.m_scheduler.TryGetTask(out task))
                    {
                        task.DoWork();
                        continue;
                    }
                    this.HasNoWork.Set();
                    this.Gate.WaitOne();
                    this.HasNoWork.Reset();
                }
            }
        }
    }
}

