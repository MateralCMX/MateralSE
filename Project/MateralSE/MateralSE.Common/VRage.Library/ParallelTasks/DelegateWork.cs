namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Profiler;

    internal class DelegateWork : AbstractWork
    {
        private static MyConcurrentPool<DelegateWork> instances = new MyConcurrentPool<DelegateWork>(100, null, 0x186a0, null);

        public override void DoWork(WorkData workData = null)
        {
            try
            {
                if (this.Action != null)
                {
                    this.Action();
                    this.Action = null;
                }
                if (this.DataAction != null)
                {
                    this.DataAction(workData);
                    this.DataAction = null;
                }
            }
            finally
            {
                instances.Return(this);
            }
        }

        protected override void FillDebugInfo(ref WorkOptions info)
        {
            if (info.DebugName == null)
            {
                info.DebugName = (this.Action == null) ? ((this.DataAction == null) ? string.Empty : this.DataAction.Method.Name) : this.Action.Method.Name;
            }
            if (info.TaskType == MyProfiler.TaskType.None)
            {
                info.TaskType = MyProfiler.TaskType.WorkItem;
            }
        }

        internal static DelegateWork GetInstance() => 
            instances.Get();

        public System.Action Action { get; set; }

        public Action<WorkData> DataAction { get; set; }
    }
}

