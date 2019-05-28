namespace VRage.Library.Exceptions
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class MyMiniDump
    {
        [ThreadStatic]
        public static long LastDumpTimestamp;

        public static bool CollectExceptionDump(Exception ex, string path)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime time2 = new DateTime(LastDumpTimestamp);
            if ((utcNow - time2).TotalSeconds > 15.0)
            {
                Write(Path.Combine(path, "MinidumpT" + Thread.CurrentThread.ManagedThreadId + ".dmp"), Options.Normal | Options.WithProcessThreadData | Options.WithThreadInfo, ExceptionInfo.Present);
                LastDumpTimestamp = utcNow.Ticks;
            }
            return true;
        }

        [DllImport("kernel32.dll", ExactSpelling=true)]
        private static extern uint GetCurrentThreadId();
        [DllImport("dbghelp.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);
        [DllImport("dbghelp.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);
        public static bool Write(string path, Options options, ExceptionInfo exceptionInfo)
        {
            MiniDumpExceptionInformation information;
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr hProcess = currentProcess.Handle;
            uint id = (uint) currentProcess.Id;
            information.ThreadId = GetCurrentThreadId();
            information.ClientPointers = false;
            information.ExceptionPointers = IntPtr.Zero;
            if (exceptionInfo == ExceptionInfo.Present)
            {
                information.ExceptionPointers = Marshal.GetExceptionPointers();
            }
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                SafeFileHandle safeFileHandle = stream.SafeFileHandle;
                return (!(information.ExceptionPointers == IntPtr.Zero) ? MiniDumpWriteDump(hProcess, id, safeFileHandle, (uint) options, ref information, IntPtr.Zero, IntPtr.Zero) : MiniDumpWriteDump(hProcess, id, safeFileHandle, (uint) options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
            }
        }

        public enum ExceptionInfo
        {
            None,
            Present
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public struct MiniDumpExceptionInformation
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            [MarshalAs(UnmanagedType.Bool)]
            public bool ClientPointers;
        }

        [Flags]
        public enum Options : uint
        {
            Normal = 0,
            WithDataSegs = 1,
            WithFullMemory = 2,
            WithHandleData = 4,
            FilterMemory = 8,
            ScanMemory = 0x10,
            WithUnloadedModules = 0x20,
            WithIndirectlyReferencedMemory = 0x40,
            FilterModulePaths = 0x80,
            WithProcessThreadData = 0x100,
            WithPrivateReadWriteMemory = 0x200,
            WithoutOptionalData = 0x400,
            WithFullMemoryInfo = 0x800,
            WithThreadInfo = 0x1000,
            WithCodeSegs = 0x2000,
            WithoutAuxiliaryState = 0x4000,
            WithFullAuxiliaryState = 0x8000,
            WithPrivateWriteCopyMemory = 0x10000,
            IgnoreInaccessibleMemory = 0x20000,
            ValidTypeFlags = 0x3ffff
        }
    }
}

