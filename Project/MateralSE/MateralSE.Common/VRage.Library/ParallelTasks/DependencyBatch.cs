namespace ParallelTasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.FileSystem;
    using VRage.Library.Exceptions;
    using VRage.Profiler;

    public class DependencyBatch : AbstractWork, IPrioritizedWork, IWork, IDisposable
    {
        private int m_maxThreads;
        private int m_jobCount;
        private int m_completedJobs;
        private Action[] m_jobs;
        private int[] m_jobStates;
        private int[] m_dependencies;
        private int[] m_dependencyStarts;
        private List<Exception> m_exceptionBuffer;
        private readonly AutoResetEvent m_completionAwaiter;
        private int m_controlThreadState = 3;
        private const int WORKERS_MASK = 0xffff;
        private const int JOBS_MASK = -65536;
        private const int JOBS_SHIFT = 0x10;
        private long m_scheduledJobsAndWorkers;
        private int m_allCompletedCachedIndex;

        public DependencyBatch(WorkPriority priority = 2)
        {
            this.Priority = priority;
            this.m_completionAwaiter = new AutoResetEvent(false);
            WorkOptions defaultOptions = Parallel.DefaultOptions;
            defaultOptions.MaximumThreads = 0x7fffffff;
            this.Options = defaultOptions.WithDebugInfo(MyProfiler.TaskType.Wait, "Batch");
        }

        public int Add(Action job)
        {
            int jobCount = this.m_jobCount;
            this.m_jobCount = jobCount + 1;
            int index = jobCount;
            this.EnsureCapacity(this.m_jobCount);
            this.m_jobs[index] = job;
            return index;
        }

        private void AllocateInternal(int size)
        {
            this.m_jobs = new Action[size];
            this.m_jobStates = new int[size];
            this.m_dependencyStarts = new int[size + 1];
            if ((this.m_dependencies == null) || (this.m_dependencies.Length < size))
            {
                this.m_dependencies = new int[size];
            }
        }

        [Conditional("DEBUG")]
        private void AssertDependencyOrder(int currentJobId)
        {
            for (int i = currentJobId + 1; i < this.m_dependencyStarts.Length; i++)
            {
            }
        }

        [Conditional("DEBUG")]
        private void AssertExecutionConsistency()
        {
            int num = this.m_dependencyStarts[0];
            for (int i = 1; i <= this.m_jobCount; i++)
            {
                int num3 = this.m_dependencyStarts[i];
                if (num3 != -1)
                {
                    num = num3;
                }
            }
            if (num > 0)
            {
                for (int j = 0; j < num; j++)
                {
                }
            }
        }

        public void Clear(int length)
        {
            this.m_jobCount = 0;
            this.m_completedJobs = 0;
            this.m_dependencyStarts[0] = 0;
            this.m_allCompletedCachedIndex = 0;
            if (this.m_exceptionBuffer != null)
            {
                this.m_exceptionBuffer.Clear();
            }
            for (int i = length - 1; i >= 0; i--)
            {
                this.m_jobs[i] = null;
                this.m_dependencyStarts[i + 1] = -1;
                this.m_jobStates[i] = 0x7ffffffc;
            }
        }

        public void Dispose()
        {
            this.m_completionAwaiter.Dispose();
        }

        public override void DoWork(WorkData workData = null)
        {
            this.WorkerLoop();
        }

        private void EnsureCapacity(int size)
        {
            int length = (this.m_jobs == null) ? 0 : this.m_jobs.Length;
            if (length < size)
            {
                Action[] jobs = this.m_jobs;
                int[] jobStates = this.m_jobStates;
                int[] dependencies = this.m_dependencies;
                int[] dependencyStarts = this.m_dependencyStarts;
                this.AllocateInternal((this.m_jobs == null) ? 50 : (length * 2));
                if (jobs != null)
                {
                    Array.Copy(jobs, this.m_jobs, length);
                    Array.Copy(jobStates, this.m_jobStates, length);
                    Array.Copy(dependencyStarts, this.m_dependencyStarts, (int) (length + 1));
                }
                int index = length;
                while (true)
                {
                    if (index >= this.m_jobStates.Length)
                    {
                        if ((this.m_dependencies != dependencies) && (dependencies != null))
                        {
                            Array.Copy(dependencies, this.m_dependencies, dependencies.Length);
                        }
                        break;
                    }
                    this.m_dependencyStarts[index + 1] = -1;
                    this.m_jobStates[index] = 0x7ffffffc;
                    index++;
                }
            }
        }

        public void Execute()
        {
            this.m_completedJobs = 0;
            if (this.m_jobCount != 0)
            {
                int jobCount = this.m_jobCount;
                int num3 = this.GetLastInitializedStart(this.m_jobCount) - 1;
                for (int i = 0; i <= num3; i++)
                {
                    int index = this.m_dependencies[i];
                    int num6 = this.m_jobStates[index];
                    if (num6 == 0x7ffffffc)
                    {
                        jobCount--;
                    }
                    this.m_jobStates[index] = num6 - 1;
                }
                this.m_controlThreadState = 0;
                this.RegisterJobsForConsumption(jobCount);
                int num2 = Interlocked.CompareExchange(ref this.m_controlThreadState, 2, 1);
                this.m_completionAwaiter.Reset();
                if (num2 != 3)
                {
                    do
                    {
                        this.WorkerLoop();
                    }
                    while (!this.MainThreadAwaiter());
                }
                try
                {
                    if ((this.m_exceptionBuffer != null) && (this.m_exceptionBuffer.Count > 0))
                    {
                        throw new TaskException(this.m_exceptionBuffer.ToArray());
                    }
                }
                finally
                {
                    this.Clear(this.m_jobCount);
                }
            }
        }

        private int ExecuteSingleJob(ref Exception e)
        {
            int num2;
            int index = -1;
            while (true)
            {
                bool flag = false;
                int allCompletedCachedIndex = this.m_allCompletedCachedIndex;
                while (true)
                {
                    if ((allCompletedCachedIndex < this.m_jobCount) && (this.m_jobStates[allCompletedCachedIndex] >= 0x7ffffffd))
                    {
                        flag = true;
                        allCompletedCachedIndex++;
                        continue;
                    }
                    if (flag)
                    {
                        int num5 = allCompletedCachedIndex - 1;
                        int comparand = this.m_allCompletedCachedIndex;
                        if (comparand < num5)
                        {
                            Interlocked.CompareExchange(ref this.m_allCompletedCachedIndex, num5, comparand);
                        }
                    }
                    while (true)
                    {
                        if (allCompletedCachedIndex < this.m_jobCount)
                        {
                            if ((this.m_jobStates[allCompletedCachedIndex] != 0x7ffffffc) || (Interlocked.CompareExchange(ref this.m_jobStates[allCompletedCachedIndex], 0x7ffffffd, 0x7ffffffc) != 0x7ffffffc))
                            {
                                allCompletedCachedIndex++;
                                continue;
                            }
                            index = allCompletedCachedIndex;
                            break;
                        }
                        else if (Volatile.Read(ref this.m_completedJobs) >= this.m_jobCount)
                        {
                            if (index == -1)
                            {
                                return 0;
                            }
                            break;
                        }
                        break;
                    }
                    break;
                }
            }
            try
            {
                this.m_jobs[index]();
            }
            catch (Exception exception)
            {
                MyMiniDump.CollectExceptionDump(exception, MyFileSystem.UserDataPath);
                e = exception;
            }
            Volatile.Write(ref this.m_jobStates[index], 0x7ffffffe);
            int num8 = this.m_dependencyStarts[index];
            int num9 = this.m_dependencyStarts[index + 1];
            if ((num8 == -1) || (num9 == -1))
            {
                num2 = 0;
            }
            else
            {
                num2 = num9 - num8;
                for (int i = num8; i < num9; i++)
                {
                    int num11 = Interlocked.Increment(ref this.m_jobStates[this.m_dependencies[i]]);
                    if (num11 != 0x7ffffffc)
                    {
                        num2--;
                    }
                }
            }
            if (Interlocked.Increment(ref this.m_completedJobs) == this.m_jobCount)
            {
                this.ReleaseMainThread();
            }
            return num2;
        }

        private int GetLastInitializedStart(int maxIndex)
        {
            while (true)
            {
                int num = this.m_dependencyStarts[maxIndex];
                if (num != -1)
                {
                    return num;
                }
                maxIndex--;
            }
        }

        public StartToken Job(int jobId)
        {
            if (this.m_dependencyStarts[jobId] == -1)
            {
                this.m_dependencyStarts[jobId] = this.GetLastInitializedStart(jobId - 1);
            }
            return new StartToken(jobId, this);
        }

        private bool MainThreadAwaiter()
        {
            if (Interlocked.CompareExchange(ref this.m_controlThreadState, 0, 2) == 3)
            {
                return true;
            }
            this.m_completionAwaiter.WaitOne();
            return (Interlocked.CompareExchange(ref this.m_controlThreadState, 2, 1) == 3);
        }

        public void Preallocate(int size)
        {
            if ((this.m_jobs == null) || (this.m_jobs.Length < size))
            {
                this.AllocateInternal(size);
            }
            this.Clear(this.m_jobs.Length);
        }

        private void RegisterJobsForConsumption(int count)
        {
            long scheduledJobsAndWorkers = this.m_scheduledJobsAndWorkers;
            while (true)
            {
                int num3 = (int) ((scheduledJobsAndWorkers & -65536L) >> 0x10);
                long num4 = scheduledJobsAndWorkers & 0xffffL;
                int num5 = Math.Min(this.m_maxThreads, ((int) num4) + count);
                int num = num5 - ((int) num4);
                long num6 = num4 + num;
                long num7 = (num3 + count) << 0x10;
                long num8 = Interlocked.CompareExchange(ref this.m_scheduledJobsAndWorkers, num7 | num6, scheduledJobsAndWorkers);
                if (num8 == scheduledJobsAndWorkers)
                {
                    if (num > 0)
                    {
                        if (this.TryWakingUpMainThread())
                        {
                            num--;
                        }
                        if (num > 0)
                        {
                            for (int i = 0; i < num; i++)
                            {
                                Parallel.Start(this);
                            }
                        }
                    }
                    return;
                }
                scheduledJobsAndWorkers = num8;
            }
        }

        private void ReleaseMainThread()
        {
            int comparand = 2;
            while (true)
            {
                bool flag = false;
                bool flag2 = false;
                switch (comparand)
                {
                    case 0:
                        flag = true;
                        break;

                    case 1:
                        flag2 = true;
                        break;

                    case 2:
                        break;

                    case 3:
                        return;

                    default:
                        return;
                }
                int num2 = Interlocked.CompareExchange(ref this.m_controlThreadState, 3, comparand);
                if (num2 == comparand)
                {
                    if (flag2)
                    {
                        bool flag3 = this.TryAcquireJob();
                    }
                    if (flag)
                    {
                        this.m_completionAwaiter.Set();
                    }
                    return;
                }
                comparand = num2;
            }
        }

        private bool TryAcquireJob()
        {
            long scheduledJobsAndWorkers = this.m_scheduledJobsAndWorkers;
            while (true)
            {
                long num2 = scheduledJobsAndWorkers & 0xffffL;
                long num3 = scheduledJobsAndWorkers & -65536L;
                bool flag = num3 != 0L;
                if (flag)
                {
                    num3 = ((num3 >> 0x10) - 1L) << 0x10;
                }
                else
                {
                    num2 -= 1L;
                }
                long num4 = Interlocked.CompareExchange(ref this.m_scheduledJobsAndWorkers, num3 | num2, scheduledJobsAndWorkers);
                if (num4 == scheduledJobsAndWorkers)
                {
                    return flag;
                }
                scheduledJobsAndWorkers = num4;
            }
        }

        private bool TryWakingUpMainThread()
        {
            if (Interlocked.CompareExchange(ref this.m_controlThreadState, 1, 0) != 0)
            {
                return false;
            }
            this.m_completionAwaiter.Set();
            return true;
        }

        private void WorkerLoop()
        {
            if (this.TryAcquireJob())
            {
                do
                {
                    Exception e = null;
                    int num = this.ExecuteSingleJob(ref e);
                    if (e != null)
                    {
                        if (this.m_exceptionBuffer == null)
                        {
                            List<Exception> list = new List<Exception>();
                            Interlocked.CompareExchange<List<Exception>>(ref this.m_exceptionBuffer, list, null);
                        }
                        List<Exception> exceptionBuffer = this.m_exceptionBuffer;
                        lock (exceptionBuffer)
                        {
                            this.m_exceptionBuffer.Add(e);
                        }
                    }
                    if (num > 0)
                    {
                        if (num > 1)
                        {
                            this.RegisterJobsForConsumption(num - 1);
                            continue;
                        }
                        int num2 = 0x10000;
                        Interlocked.Add(ref this.m_scheduledJobsAndWorkers, (long) num2);
                        if (this.TryWakingUpMainThread())
                        {
                            break;
                        }
                    }
                }
                while (this.TryAcquireJob());
            }
        }

        public WorkPriority Priority { get; private set; }

        WorkOptions IWork.Options
        {
            get
            {
                WorkOptions options = base.Options;
                options.MaximumThreads = 1;
                return options;
            }
        }

        public int MaxThreads =>
            this.m_maxThreads;

        public sealed override WorkOptions Options
        {
            get => 
                base.Options;
            set
            {
                base.Options = value;
                this.m_maxThreads = Math.Min(value.MaximumThreads, Parallel.Scheduler.ThreadCount + 1);
            }
        }

        private static class ControlThreadState
        {
            public const int Waiting = 0;
            public const int Scheduled = 1;
            public const int Running = 2;
            public const int Exit = 3;
        }

        private static class JobState
        {
            public const int DependencyPending = 0x7ffffffb;
            public const int Scheduled = 0x7ffffffc;
            public const int Running = 0x7ffffffd;
            public const int Finished = 0x7ffffffe;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartToken : IDisposable
        {
            private readonly int m_jobId;
            private readonly DependencyBatch m_batch;
            private int m_writeOffset;
            public StartToken(int jobId, DependencyBatch batch)
            {
                this.m_jobId = jobId;
                this.m_batch = batch;
                this.m_writeOffset = batch.m_dependencyStarts[jobId];
            }

            public unsafe void Starts(int jobId)
            {
                int[] dependencies = this.m_batch.m_dependencies;
                int writeOffset = this.m_writeOffset;
                this.m_writeOffset = writeOffset + 1;
                int index = writeOffset;
                if (dependencies.Length <= index)
                {
                    int[]* array = ref dependencies;
                    Array.Resize<int>(ref array, dependencies.Length * 2);
                    this.m_batch.m_dependencies = dependencies;
                }
                dependencies[index] = jobId;
            }

            public void Dispose()
            {
                this.m_batch.m_dependencyStarts[this.m_jobId + 1] = this.m_writeOffset;
            }
        }
    }
}

