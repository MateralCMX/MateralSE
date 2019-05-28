namespace ParallelTasks
{
    using System;

    public class FakeTaskScheduler : IWorkScheduler
    {
        public int ReadAndClearExecutionTime() => 
            0;

        public void Schedule(Task item)
        {
            item.DoWork();
        }

        public void ScheduleOnEachWorker(Action action)
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
            item.PrepareStart(instance, null).DoWork();
        }

        public bool WaitForTasksToFinish(TimeSpan waitTimeout) => 
            true;

        public int ThreadCount =>
            1;
    }
}

