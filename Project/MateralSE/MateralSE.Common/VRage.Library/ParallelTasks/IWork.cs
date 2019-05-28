namespace ParallelTasks
{
    using System;
    using System.Runtime.InteropServices;

    public interface IWork
    {
        void DoWork(WorkData workData = null);

        WorkOptions Options { get; }
    }
}

