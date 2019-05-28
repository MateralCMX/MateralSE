namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;

    public class WorkStealingScheduler : IWorkScheduler
    {
        private Queue<Task> tasks;
        private FastResourceLock tasksLock;

        public WorkStealingScheduler() : this(MyEnvironment.ProcessorCount, ThreadPriority.BelowNormal)
        {
        }

        public WorkStealingScheduler(int numThreads, ThreadPriority priority)
        {
            this.tasks = new Queue<Task>();
            this.tasksLock = new FastResourceLock();
            this.Workers = new List<ParallelTasks.Worker>(numThreads);
            for (int i = 0; i < numThreads; i++)
            {
                this.Workers.Add(new ParallelTasks.Worker(this, i, priority));
            }
            for (int j = 0; j < numThreads; j++)
            {
                this.Workers[j].Start();
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
                int maximumThreads = task.Item.Work.Options.MaximumThreads;
                ParallelTasks.Worker currentWorker = ParallelTasks.Worker.CurrentWorker;
                if (!task.Item.Work.Options.QueueFIFO && (currentWorker != null))
                {
                    currentWorker.AddWork(task);
                }
                else
                {
                    using (this.tasksLock.AcquireExclusiveUsing())
                    {
                        this.tasks.Enqueue(task);
                    }
                }
                for (int i = 0; i < this.Workers.Count; i++)
                {
                    this.Workers[i].Gate.Set();
                }
            }
        }

        public void ScheduleOnEachWorker(Action action)
        {
            foreach (ParallelTasks.Worker worker in this.Workers)
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
                worker.AddWork(task);
                worker.Gate.Set();
                task.Wait(false);
            }
        }

        internal bool TryGetTask(out Task task)
        {
            bool flag;
            if (this.tasks.Count == 0)
            {
                task = new Task();
                return false;
            }
            using (this.tasksLock.AcquireExclusiveUsing())
            {
                if (this.tasks.Count > 0)
                {
                    task = this.tasks.Dequeue();
                    flag = true;
                }
                else
                {
                    task = new Task();
                    flag = false;
                }
            }
            return flag;
        }

        public bool WaitForTasksToFinish(TimeSpan waitTimeout) => 
            Parallel.WaitForAll((from s in this.Workers select s.HasNoWork).ToArray<ManualResetEvent>(), waitTimeout);

        internal List<ParallelTasks.Worker> Workers { get; private set; }

        public int ThreadCount =>
            this.Workers.Count;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly WorkStealingScheduler.<>c <>9 = new WorkStealingScheduler.<>c();
            public static Func<ParallelTasks.Worker, ManualResetEvent> <>9__12_0;

            internal ManualResetEvent <WaitForTasksToFinish>b__12_0(ParallelTasks.Worker s) => 
                s.HasNoWork;
        }
    }
}

