namespace Sandbox.Game.GUI
{
    using ParallelTasks;
    using System;

    public interface IMyAsyncResult
    {
        bool IsCompleted { get; }

        ParallelTasks.Task Task { get; }
    }
}

