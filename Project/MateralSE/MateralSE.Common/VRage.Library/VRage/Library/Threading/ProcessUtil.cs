namespace VRage.Library.Threading
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public static class ProcessUtil
    {
        public static void SetThreadProcessorAffinity(params int[] cpus)
        {
            if (cpus == null)
            {
                throw new ArgumentNullException("cpus");
            }
            if (cpus.Length == 0)
            {
                throw new ArgumentException("You must specify at least one CPU.", "cpus");
            }
            long num = 0L;
            foreach (int num3 in cpus)
            {
                if ((num3 < 0) || (num3 >= Environment.ProcessorCount))
                {
                    throw new ArgumentException("Invalid CPU number.");
                }
                num |= 1L << (num3 & 0x3f);
            }
            Thread.BeginThreadAffinity();
            int osThreadId = AppDomain.GetCurrentThreadId();
            Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Single<ProcessThread>(t => (t.Id == osThreadId)).ProcessorAffinity = new IntPtr(num);
        }
    }
}

