namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;

    public class WorkData
    {
        public WorkData()
        {
            this.Priority = WorkPriority.Normal;
        }

        public void FlagAsFailed()
        {
        }

        public WorkPriority Priority { get; set; }
    }
}

