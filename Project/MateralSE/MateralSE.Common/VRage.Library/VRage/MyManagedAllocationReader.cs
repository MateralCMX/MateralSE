namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyManagedAllocationReader
    {
        private const string NATIVE_CLR_PROFILER = "Native_CLR_Profiler";
        [ThreadStatic]
        private static IntPtr ThreadAllocationStampNativePtr;

        public static ulong GetGlobalAllocationsStamp() => 
            GetTotalAllocations();

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);
        [DllImport("Native_CLR_Profiler")]
        private static extern IntPtr GetThreadAllocationPtr();
        public static unsafe ulong GetThreadAllocationStamp()
        {
            IntPtr threadAllocationStampNativePtr = ThreadAllocationStampNativePtr;
            if (threadAllocationStampNativePtr == IntPtr.Zero)
            {
                IntPtr moduleHandle = GetModuleHandle("Native_CLR_Profiler");
                if (moduleHandle == IntPtr.Zero)
                {
                    throw new Exception("Native profiler is not attached!");
                }
                if (GetProcAddress(moduleHandle, "GetThreadAllocationPtr") == IntPtr.Zero)
                {
                    throw new Exception("GetThreadAllocationPtr not found!");
                }
                threadAllocationStampNativePtr = GetThreadAllocationPtr();
                ThreadAllocationStampNativePtr = threadAllocationStampNativePtr;
            }
            return *(((ulong*) threadAllocationStampNativePtr));
        }

        [DllImport("Native_CLR_Profiler")]
        private static extern ulong GetTotalAllocations();

        public static bool IsReady =>
            (GetModuleHandle("Native_CLR_Profiler") != IntPtr.Zero);
    }
}

