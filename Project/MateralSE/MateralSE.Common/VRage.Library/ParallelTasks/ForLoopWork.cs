namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Profiler;

    internal class ForLoopWork : AbstractWork, IPrioritizedWork, IWork
    {
        private static MyConcurrentPool<ForLoopWork> pool = new MyConcurrentPool<ForLoopWork>(10, null, 0x2710, null);
        private int index;
        private int length;
        private int stride;
        private Action<int> action;

        public override void DoWork(WorkData workData = null)
        {
            int num;
            while ((num = this.IncrementIndex()) < this.length)
            {
                int num2 = Math.Min(num + this.stride, this.length);
                for (int i = num; i < num2; i++)
                {
                    this.action(i);
                }
            }
        }

        protected override void FillDebugInfo(ref WorkOptions info)
        {
            base.FillDebugInfo(ref info, this.action.Method.Name, MyProfiler.TaskType.WorkItem);
        }

        public static ForLoopWork Get() => 
            pool.Get();

        private int IncrementIndex() => 
            (Interlocked.Add(ref this.index, this.stride) - this.stride);

        public void Prepare(Action<int> action, int startInclusive, int endExclusive, int stride, WorkPriority priority)
        {
            this.action = action;
            this.index = startInclusive;
            this.length = endExclusive;
            this.stride = stride;
            this.Priority = priority;
        }

        public void Return()
        {
            this.action = null;
            pool.Return(this);
        }

        public WorkPriority Priority { get; private set; }
    }
}

