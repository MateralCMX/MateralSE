namespace ParallelTasks
{
    using System;

    public interface IWorkScheduler
    {
        int ReadAndClearExecutionTime();
        void Schedule(Task item);
        void ScheduleOnEachWorker(Action action);
        bool WaitForTasksToFinish(TimeSpan waitTimeout);

        int ThreadCount { get; }
    }
}

