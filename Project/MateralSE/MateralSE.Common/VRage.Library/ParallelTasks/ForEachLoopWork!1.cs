namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Profiler;

    internal class ForEachLoopWork<T> : AbstractWork, IPrioritizedWork, IWork
    {
        private static MyConcurrentPool<ForEachLoopWork<T>> pool;
        private bool done;
        private object syncLock;
        private Action<T> action;
        private IEnumerator<T> enumerator;

        static ForEachLoopWork()
        {
            ForEachLoopWork<T>.pool = new MyConcurrentPool<ForEachLoopWork<T>>(10, null, 0x2710, null);
        }

        public ForEachLoopWork()
        {
            this.syncLock = new object();
        }

        public override void DoWork(WorkData workData = null)
        {
            T current = default(T);
            while (!this.done)
            {
                object syncLock = this.syncLock;
                lock (syncLock)
                {
                    if (this.done)
                    {
                        break;
                    }
                    this.done = !this.enumerator.MoveNext();
                    if (this.done)
                    {
                        break;
                    }
                    current = this.enumerator.Current;
                }
                this.action(current);
            }
        }

        protected override void FillDebugInfo(ref WorkOptions info)
        {
            base.FillDebugInfo(ref info, this.action.Method.Name, MyProfiler.TaskType.WorkItem);
        }

        public static ForEachLoopWork<T> Get() => 
            ForEachLoopWork<T>.pool.Get();

        public void Prepare(Action<T> action, IEnumerator<T> enumerator, WorkPriority priority)
        {
            this.done = false;
            this.action = action;
            this.Priority = priority;
            this.enumerator = enumerator;
        }

        public void Return()
        {
            this.enumerator = null;
            this.action = null;
            ForEachLoopWork<T>.pool.Return((ForEachLoopWork<T>) this);
        }

        public WorkPriority Priority { get; private set; }
    }
}

