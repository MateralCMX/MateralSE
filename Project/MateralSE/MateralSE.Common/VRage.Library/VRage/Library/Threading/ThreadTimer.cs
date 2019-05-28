namespace VRage.Library.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadTimer
    {
        private IntPtr OSThreadId;
        [DllImport("kernel32.dll")]
        private static extern long GetThreadTimes(IntPtr threadHandle, out long createionTime, out long exitTime, out long kernelTime, out long userTime);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();
        public static ThreadTimer CurrentThread =>
            new ThreadTimer { OSThreadId=GetCurrentThread() };
        public bool IsValid =>
            (this.OSThreadId != IntPtr.Zero);
        public long CPUTime
        {
            get
            {
                long num;
                long num2;
                this.GetThreadTimes(out num, out num2);
                return (num + num2);
            }
        }
        public void GetThreadTimes(out long kernelTime, out long userTime)
        {
            long num;
            long num2;
            GetThreadTimes(this.OSThreadId, out num, out num2, out kernelTime, out userTime);
        }
    }
}

