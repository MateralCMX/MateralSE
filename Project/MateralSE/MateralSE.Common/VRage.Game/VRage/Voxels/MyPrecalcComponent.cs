namespace VRage.Voxels
{
    using ParallelTasks;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Voxels;
    using VRage.Generics;
    using VRage.Library.Threading;
    using VRage.Profiler;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyPrecalcComponent : MySessionComponentBase
    {
        private static bool MULTITHREADED = true;
        private static Type m_isoMesherType = typeof(MyDualContouringMesher);
        public static long MaxPrecalcTime = 20;
        public static bool DebugDrawSorted = false;
        private static MyPrecalcComponent m_instance;
        private static SpinLockRef m_queueLock = new SpinLockRef();
        [ThreadStatic]
        private static IMyIsoMesher m_isoMesher;
        public static int UpdateThreadManagedId;
        private static readonly MyPrecalcJobComparer m_comparer = new MyPrecalcJobComparer();
        private readonly MyConcurrentQueue<MyPrecalcJob> m_workQueue = new MyConcurrentQueue<MyPrecalcJob>();
        private readonly MyConcurrentList<MyPrecalcJob> m_finishedJobs = new MyConcurrentList<MyPrecalcJob>();
        private MyDynamicObjectPool<Work> m_workPool;
        private volatile int m_activeWorkers;

        public MyPrecalcComponent()
        {
            base.UpdateOnPause = true;
        }

        [Conditional("DEBUG")]
        public static void AssertUpdateThread()
        {
        }

        private void Enqueue(MyPrecalcJob job)
        {
            job.Started = false;
            this.m_workQueue.Enqueue(job);
        }

        public static bool EnqueueBack(MyPrecalcJob job)
        {
            bool flag;
            using (m_queueLock.Acquire())
            {
                if (m_instance == null)
                {
                    flag = false;
                }
                else
                {
                    m_instance.Enqueue(job);
                    flag = true;
                }
            }
            return flag;
        }

        public override void LoadData()
        {
            base.LoadData();
            m_instance = this;
            if (!MULTITHREADED)
            {
                MaxPrecalcTime = 6L;
            }
            this.m_workPool = new MyDynamicObjectPool<Work>(Parallel.Scheduler.ThreadCount);
        }

        private bool TryDequeue(out MyPrecalcJob job) => 
            this.m_workQueue.TryDequeue(out job);

        protected override void UnloadData()
        {
            // Invalid method body.
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.UpdateQueue();
        }

        public override bool UpdatedBeforeInit() => 
            true;

        public void UpdateQueue()
        {
            bool flag = false;
            MyPrecalcJob instance = null;
            while (true)
            {
                if (this.m_workQueue.TryPeek(out instance))
                {
                    if (instance.IsCanceled)
                    {
                        MyPrecalcJob job2;
                        bool flag2 = false;
                        if (this.m_workQueue.TryDequeue(out job2))
                        {
                            flag2 = ReferenceEquals(instance, job2);
                        }
                        if (flag2)
                        {
                            if (instance.OnCompleteDelegate != null)
                            {
                                this.m_finishedJobs.Add(instance);
                            }
                        }
                        else
                        {
                            flag = false;
                            if (job2 != null)
                            {
                                this.m_workQueue.Enqueue(job2);
                            }
                        }
                        continue;
                    }
                    flag = true;
                }
                if (flag)
                {
                    while (this.m_workPool.Count > 0)
                    {
                        Work work = this.m_workPool.Allocate();
                        work.Parent = this;
                        work.Priority = WorkPriority.Low;
                        work.MaxPrecalcTime = MaxPrecalcTime * 0x2710L;
                        Interlocked.Increment(ref this.m_activeWorkers);
                        if (MULTITHREADED)
                        {
                            Parallel.Start(work, work.CompletionCallback);
                            continue;
                        }
                        ((IWork) work).DoWork(null);
                        work.CompletionCallback();
                    }
                }
                while (this.m_finishedJobs.TryDequeueBack(out instance))
                {
                    instance.OnCompleteDelegate();
                }
                return;
            }
        }

        public static Type IsoMesherType
        {
            get => 
                m_isoMesherType;
            set
            {
                if (typeof(IMyIsoMesher).IsAssignableFrom(m_isoMesherType))
                {
                    m_isoMesherType = value;
                }
            }
        }

        public static IMyIsoMesher IsoMesher =>
            (m_isoMesher ?? (m_isoMesher = (IMyIsoMesher) Activator.CreateInstance(IsoMesherType)));

        public static int InvalidatedRangeInflate =>
            IsoMesher.InvalidatedRangeInflate;

        private class MyPrecalcJobComparer : IComparer<MyPrecalcJob>
        {
            public int Compare(MyPrecalcJob x, MyPrecalcJob y) => 
                y.Priority.CompareTo(x.Priority);
        }

        private class Work : IPrioritizedWork, IWork
        {
            private readonly List<MyPrecalcJob> m_finishedList = new List<MyPrecalcJob>();
            private readonly Stopwatch m_timer = new Stopwatch();
            public long MaxPrecalcTime;
            public readonly Action CompletionCallback;
            public MyPrecalcComponent Parent;

            public Work()
            {
                this.CompletionCallback = new Action(this.OnComplete);
            }

            private void OnComplete()
            {
                this.Parent.m_workPool.Deallocate(this);
                this.Parent = null;
            }

            void IWork.DoWork(WorkData workData)
            {
                this.m_timer.Start();
                try
                {
                    while (true)
                    {
                        MyPrecalcJob job;
                        if (this.Parent.Loaded && this.Parent.TryDequeue(out job))
                        {
                            if (job.IsCanceled && (job.OnCompleteDelegate != null))
                            {
                                this.m_finishedList.Add(job);
                                continue;
                            }
                            job.DoWorkInternal();
                            if (job.OnCompleteDelegate != null)
                            {
                                this.m_finishedList.Add(job);
                            }
                            if (this.m_timer.ElapsedTicks < this.MaxPrecalcTime)
                            {
                                continue;
                            }
                        }
                        this.Parent.m_finishedJobs.AddRange(this.m_finishedList);
                        this.m_finishedList.Clear();
                        break;
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref this.Parent.m_activeWorkers);
                }
                this.m_timer.Reset();
            }

            public WorkPriority Priority { get; set; }

            WorkOptions IWork.Options =>
                Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Precalc, "Precalc");

            public bool ShouldRequeue
            {
                get
                {
                    if (!this.Parent.Loaded || (this.Parent.m_workQueue.Count <= 0))
                    {
                        return false;
                    }
                    Interlocked.Increment(ref this.Parent.m_activeWorkers);
                    return true;
                }
            }
        }
    }
}

