namespace ParallelTasks
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Profiler;

    public abstract class AbstractWork : IWork
    {
        private WorkOptions m_options;
        private string m_cachedDebugName;

        protected AbstractWork()
        {
        }

        public abstract void DoWork(WorkData workData = null);
        protected virtual void FillDebugInfo(ref WorkOptions info)
        {
            if (this.m_cachedDebugName == null)
            {
                this.m_cachedDebugName = base.GetType().Name;
            }
            this.FillDebugInfo(ref info, this.m_cachedDebugName, MyProfiler.TaskType.WorkItem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FillDebugInfo(ref WorkOptions info, string debugName, MyProfiler.TaskType taskType = 3)
        {
            if (info.DebugName == null)
            {
                info.DebugName = debugName;
            }
            if (info.TaskType == MyProfiler.TaskType.None)
            {
                info.TaskType = taskType;
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        private void FillDebugInfoInternal(ref WorkOptions info)
        {
            this.FillDebugInfo(ref info);
        }

        public virtual WorkOptions Options
        {
            get => 
                this.m_options;
            set => 
                (this.m_options = value);
        }
    }
}

