namespace Sandbox.ModAPI
{
    using ParallelTasks;
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Game.ModAPI;

    internal class MyParallelTask : IMyParallelTask
    {
        public static readonly MyParallelTask Static = new MyParallelTask();

        void IMyParallelTask.Do(params IWork[] work)
        {
            Parallel.Do(work);
        }

        void IMyParallelTask.Do(params Action[] actions)
        {
            Parallel.Do(actions);
        }

        void IMyParallelTask.Do(IWork a, IWork b)
        {
            Parallel.Do(a, b);
        }

        void IMyParallelTask.Do(Action action1, Action action2)
        {
            Parallel.Do(action1, action2);
        }

        void IMyParallelTask.For(int startInclusive, int endExclusive, Action<int> body)
        {
            WorkOptions? options = null;
            Parallel.For(startInclusive, endExclusive, body, WorkPriority.Normal, options);
        }

        void IMyParallelTask.For(int startInclusive, int endExclusive, Action<int> body, int stride)
        {
            WorkOptions? options = null;
            Parallel.For(startInclusive, endExclusive, body, stride, WorkPriority.Normal, options, false);
        }

        void IMyParallelTask.ForEach<T>(IEnumerable<T> collection, Action<T> action)
        {
            WorkOptions? options = null;
            Parallel.ForEach<T>(collection, action, WorkPriority.Normal, options, false);
        }

        void IMyParallelTask.Sleep(int millisecondsTimeout)
        {
            if (!ReferenceEquals(Thread.CurrentThread, MySandboxGame.Static.UpdateThread))
            {
                Thread.Sleep(millisecondsTimeout);
            }
        }

        void IMyParallelTask.Sleep(TimeSpan timeout)
        {
            if (!ReferenceEquals(Thread.CurrentThread, MySandboxGame.Static.UpdateThread))
            {
                Thread.Sleep(timeout);
            }
        }

        Task IMyParallelTask.Start(IWork work) => 
            Parallel.Start(work);

        Task IMyParallelTask.Start(Action action) => 
            Parallel.Start(action);

        Task IMyParallelTask.Start(IWork work, Action completionCallback) => 
            Parallel.Start(work, completionCallback);

        Task IMyParallelTask.Start(Action action, WorkOptions options) => 
            Parallel.Start(action, options);

        Task IMyParallelTask.Start(Action action, Action completionCallback) => 
            Parallel.Start(action, completionCallback);

        Task IMyParallelTask.Start(Action action, WorkOptions options, Action completionCallback) => 
            Parallel.Start(action, options, completionCallback);

        Task IMyParallelTask.Start(Action<WorkData> action, Action<WorkData> completionCallback, WorkData workData) => 
            Parallel.Start(action, completionCallback, workData);

        Task IMyParallelTask.StartBackground(IWork work) => 
            Parallel.StartBackground(work);

        Task IMyParallelTask.StartBackground(Action action) => 
            Parallel.StartBackground(action);

        Task IMyParallelTask.StartBackground(IWork work, Action completionCallback) => 
            Parallel.StartBackground(work, completionCallback);

        Task IMyParallelTask.StartBackground(Action action, Action completionCallback) => 
            Parallel.StartBackground(action, completionCallback);

        Task IMyParallelTask.StartBackground(Action<WorkData> action, Action<WorkData> completionCallback, WorkData workData) => 
            Parallel.StartBackground(action, completionCallback, workData);

        WorkOptions IMyParallelTask.DefaultOptions =>
            Parallel.DefaultOptions;
    }
}

