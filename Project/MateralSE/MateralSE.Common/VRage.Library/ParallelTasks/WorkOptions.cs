namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Profiler;

    [StructLayout(LayoutKind.Sequential)]
    public struct WorkOptions
    {
        public int MaximumThreads { get; set; }
        public bool QueueFIFO { get; set; }
        public string DebugName { get; set; }
        public VRage.Profiler.MyProfiler.TaskType TaskType { get; set; }
        public WorkOptions WithDebugInfo(VRage.Profiler.MyProfiler.TaskType taskType, string debugName = null)
        {
            WorkOptions options = this;
            options.TaskType = taskType;
            options.DebugName = debugName;
            return options;
        }
    }
}

