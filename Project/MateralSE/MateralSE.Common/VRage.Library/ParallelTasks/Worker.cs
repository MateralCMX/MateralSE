namespace ParallelTasks
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class Worker
    {
        private Thread thread;
        private Deque<Task> tasks;
        private WorkStealingScheduler scheduler;
        private static Hashtable<Thread, ParallelTasks.Worker> workers = new Hashtable<Thread, ParallelTasks.Worker>(MyEnvironment.ProcessorCount);

        public Worker(WorkStealingScheduler scheduler, int index, ThreadPriority priority)
        {
            this.thread = new Thread(new ThreadStart(this.Work));
            this.thread.Name = "Parallel " + index;
            this.thread.IsBackground = true;
            this.thread.Priority = priority;
            this.thread.CurrentCulture = CultureInfo.InvariantCulture;
            this.thread.CurrentUICulture = CultureInfo.InvariantCulture;
            this.tasks = new Deque<Task>();
            this.scheduler = scheduler;
            this.Gate = new AutoResetEvent(false);
            this.HasNoWork = new ManualResetEvent(false);
            workers.Add(this.thread, this);
        }

        public void AddWork(Task task)
        {
            this.tasks.LocalPush(task);
        }

        private void FindWork(out Task task)
        {
            bool flag = false;
            task = new Task();
            while (true)
            {
                while (true)
                {
                    if (this.tasks.LocalPop(ref task))
                    {
                        return;
                    }
                    else
                    {
                        if (!this.scheduler.TryGetTask(out task))
                        {
                            int num = 0;
                            do
                            {
                                if (num < this.scheduler.Workers.Count)
                                {
                                    ParallelTasks.Worker objA = this.scheduler.Workers[num];
                                    if (ReferenceEquals(objA, this) || !objA.tasks.TrySteal(ref task))
                                    {
                                        num++;
                                        continue;
                                    }
                                    flag = true;
                                }
                                if (!flag)
                                {
                                    this.HasNoWork.Set();
                                    this.Gate.WaitOne();
                                    this.HasNoWork.Reset();
                                }
                            }
                            while (!flag);
                        }
                        return;
                    }
                }
            }
        }

        public void Start()
        {
            this.thread.Start();
        }

        private void Work()
        {
            while (true)
            {
                Task task;
                this.FindWork(out task);
                task.DoWork();
            }
        }

        public AutoResetEvent Gate { get; private set; }

        public ManualResetEvent HasNoWork { get; private set; }

        public static ParallelTasks.Worker CurrentWorker
        {
            get
            {
                ParallelTasks.Worker worker;
                Thread currentThread = Thread.CurrentThread;
                return (!workers.TryGet(currentThread, out worker) ? null : worker);
            }
        }
    }
}

