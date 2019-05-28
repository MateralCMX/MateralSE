namespace ParallelTasks
{
    public interface IPrioritizedWork : IWork
    {
        WorkPriority Priority { get; }
    }
}

