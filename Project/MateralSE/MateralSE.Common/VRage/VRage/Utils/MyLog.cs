namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Unsharper;
    using VRage;
    using VRage.FileSystem;
    using VRage.Library;

    [UnsharperDisableReflection]
    public class MyLog
    {
        private bool m_alwaysFlush;
        public static MyLogSeverity AssertLevel = ((MyLogSeverity) 0xff);
        private bool LogForMemoryProfiler;
        private bool m_enabled;
        private Stream m_stream;
        private StreamWriter m_streamWriter;
        private readonly FastResourceLock m_lock = new FastResourceLock();
        private Dictionary<int, int> m_indentsByThread;
        private Dictionary<MyLogIndentKey, MyLogIndentValue> m_indents;
        private string m_filepath;
        private StringBuilder m_stringBuilder = new StringBuilder(0x800);
        private char[] m_tmpWrite = new char[0x800];
        private LoggingOptions m_loggingOptions = (LoggingOptions.SESSION_SETTINGS | LoggingOptions.TRAILERS | LoggingOptions.AUDIO | LoggingOptions.MISC_RENDER_ASSETS | LoggingOptions.VOXEL_MAPS | LoggingOptions.SIMPLE_NETWORKING | LoggingOptions.CONFIG_ACCESS | LoggingOptions.VALIDATING_CUE_PARAMS | LoggingOptions.LOADING_SPRITE_VIDEO | LoggingOptions.LOADING_CUSTOM_ASSETS | LoggingOptions.LOADING_TEXTURES | LoggingOptions.ENUM_CHECKING | LoggingOptions.NONE);
        private Action<string> m_normalWriter;
        private Action<string> m_closedLogWriter;
        private static MyLog m_default;
        private readonly FastResourceLock m_consoleStringBuilderLock = new FastResourceLock();
        private StringBuilder m_consoleStringBuilder = new StringBuilder();

        public MyLog(bool alwaysFlush = false)
        {
            this.m_alwaysFlush = alwaysFlush;
        }

        private void AppendDateAndTime(StringBuilder sb)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            sb.Concat(now.Year, 4, '0', 10, false).Append('-');
            sb.Concat(now.Month, 2).Append('-');
            sb.Concat(now.Day, 2).Append(' ');
            sb.Concat(now.Hour, 2).Append(':');
            sb.Concat(now.Minute, 2).Append(':');
            sb.Concat(now.Second, 2).Append('.');
            sb.Concat(now.Millisecond, 3);
        }

        public void AppendToClosedLog(Exception e)
        {
            if (this.m_enabled)
            {
                this.WriteLine(e);
            }
            else if (this.m_filepath != null)
            {
                WriteLine(this.m_closedLogWriter, e);
            }
        }

        public void AppendToClosedLog(string text)
        {
            if (this.m_enabled)
            {
                this.WriteLine(text);
            }
            else if (this.m_filepath != null)
            {
                File.AppendAllText(this.m_filepath, text + MyEnvironment.NewLine);
            }
        }

        public void Close()
        {
            if (this.m_enabled)
            {
                this.WriteLine("Log Closed");
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        this.m_streamWriter.Close();
                        this.m_stream.Close();
                        this.m_stream = null;
                        this.m_streamWriter = null;
                        this.m_enabled = false;
                    }
                }
            }
        }

        public void DecreaseIndent()
        {
            if (this.m_enabled)
            {
                FastResourceLockExtensions.MyExclusiveLock @lock;
                using (@lock = this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        int threadId = this.GetThreadId();
                        MyLogIndentKey key = new MyLogIndentKey(threadId, this.GetIdentByThread(threadId));
                        MyLogIndentValue value2 = this.m_indents[key];
                        if (this.LogForMemoryProfiler)
                        {
                            MyMemoryLogs.MyMemoryEvent ev = new MyMemoryLogs.MyMemoryEvent();
                            ev.DeltaTime = ((float) (DateTimeOffset.Now - value2.LastDateTimeOffset).TotalMilliseconds) / 1000f;
                            ev.ManagedEndSize = this.GetManagedMemory();
                            ev.ProcessEndSize = this.GetSystemMemory();
                            ev.ManagedStartSize = value2.LastGcTotalMemory;
                            ev.ProcessStartSize = value2.LastWorkingSet;
                            MyMemoryLogs.EndEvent(ev);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                using (@lock = this.m_lock.AcquireExclusiveUsing())
                {
                    int threadId = this.GetThreadId();
                    this.m_indentsByThread[threadId] = this.GetIdentByThread(threadId) - 1;
                }
            }
        }

        public void DecreaseIndent(LoggingOptions option)
        {
            if (this.LogFlag(option))
            {
                this.DecreaseIndent();
            }
        }

        public void Flush()
        {
            this.m_streamWriter.Flush();
        }

        public string GetFilePath()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                return this.m_filepath;
            }
        }

        private string GetFormatedMemorySize(long bytesCount) => 
            (MyValueFormatter.GetFormatedFloat((((float) bytesCount) / 1024f) / 1024f, 3) + " Mb (" + MyValueFormatter.GetFormatedLong(bytesCount) + " bytes)");

        private string GetGCMemoryString(string prependText = "") => 
            $"{prependText}: GC Memory: {this.GetManagedMemory().ToString("##,#")} B";

        private int GetIdentByThread(int threadId)
        {
            int num;
            if (!this.m_indentsByThread.TryGetValue(threadId, out num))
            {
                num = 0;
            }
            return num;
        }

        private long GetManagedMemory() => 
            GC.GetTotalMemory(false);

        private long GetSystemMemory() => 
            MyEnvironment.WorkingSetForMyLog;

        public TextWriter GetTextWriter() => 
            this.m_streamWriter;

        private int GetThreadId() => 
            Thread.CurrentThread.ManagedThreadId;

        public void IncreaseIndent()
        {
            if (this.m_enabled)
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        int threadId = this.GetThreadId();
                        this.m_indentsByThread[threadId] = this.GetIdentByThread(threadId) + 1;
                        MyLogIndentKey key = new MyLogIndentKey(threadId, this.m_indentsByThread[threadId]);
                        this.m_indents[key] = new MyLogIndentValue(this.GetManagedMemory(), this.GetSystemMemory(), DateTimeOffset.Now);
                        if (this.LogForMemoryProfiler)
                        {
                            MyMemoryLogs.StartEvent();
                        }
                    }
                }
            }
        }

        public void IncreaseIndent(LoggingOptions option)
        {
            if (this.LogFlag(option))
            {
                this.IncreaseIndent();
            }
        }

        public IndentToken IndentUsing(LoggingOptions options = 1) => 
            new IndentToken(this, options);

        public void Init(string logFileName, StringBuilder appVersionString)
        {
            int num;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                try
                {
                    this.m_filepath = Path.IsPathRooted(logFileName) ? logFileName : Path.Combine(MyFileSystem.UserDataPath, logFileName);
                    this.m_stream = MyFileSystem.OpenWrite(this.m_filepath, FileMode.Create);
                    this.m_streamWriter = new StreamWriter(this.m_stream, new UTF8Encoding(false, false));
                    this.m_normalWriter = new Action<string>(this.WriteLine);
                    this.m_closedLogWriter = s => File.AppendAllText(this.m_filepath, s + MyEnvironment.NewLine);
                    this.m_enabled = true;
                }
                catch (Exception exception)
                {
                    Trace.Fail("Cannot create log file: " + exception.ToString());
                }
                this.m_indentsByThread = new Dictionary<int, int>();
                this.m_indents = new Dictionary<MyLogIndentKey, MyLogIndentValue>();
                num = (int) Math.Round((DateTime.Now - DateTime.UtcNow).TotalHours);
            }
            this.WriteLine("Log Started");
            this.WriteLine($"Timezone (local - UTC): {num}h");
            this.WriteLineAndConsole("App Version: " + appVersionString);
        }

        public bool IsIndentKeyIncreased()
        {
            bool flag;
            if (!this.m_enabled)
            {
                return false;
            }
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (!this.m_enabled)
                {
                    flag = false;
                }
                else
                {
                    int threadId = this.GetThreadId();
                    MyLogIndentKey key = new MyLogIndentKey(threadId, this.GetIdentByThread(threadId));
                    flag = this.m_indents.ContainsKey(key);
                }
            }
            return flag;
        }

        public void Log(MyLogSeverity severity, StringBuilder builder)
        {
            if (this.m_enabled)
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        this.WriteDateTimeAndThreadId();
                        StringBuilder stringBuilder = this.m_stringBuilder;
                        stringBuilder.Clear();
                        stringBuilder.AppendFormat("{0}: ", severity);
                        stringBuilder.AppendStringBuilder(builder);
                        stringBuilder.Append('\n');
                        this.WriteStringBuilder(stringBuilder);
                        if (severity >= AssertLevel)
                        {
                            Trace.Fail(stringBuilder.ToString());
                        }
                    }
                }
            }
        }

        public void Log(MyLogSeverity severity, string format, params object[] args)
        {
            if (this.m_enabled)
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        this.WriteDateTimeAndThreadId();
                        StringBuilder stringBuilder = this.m_stringBuilder;
                        stringBuilder.Clear();
                        stringBuilder.AppendFormat("{0}: ", severity);
                        stringBuilder.AppendFormat(format, args);
                        stringBuilder.Append('\n');
                        this.WriteStringBuilder(stringBuilder);
                        if (severity >= AssertLevel)
                        {
                            Trace.Fail(stringBuilder.ToString());
                        }
                    }
                }
            }
        }

        public bool LogFlag(LoggingOptions option) => 
            ((this.m_loggingOptions & option) != 0);

        public void LogThreadPoolInfo()
        {
            if (this.m_enabled)
            {
                int num;
                int num2;
                this.WriteLine("LogThreadPoolInfo - START");
                this.IncreaseIndent();
                ThreadPool.GetMaxThreads(out num, out num2);
                this.WriteLine("GetMaxThreads.WorkerThreads: " + num);
                this.WriteLine("GetMaxThreads.CompletionPortThreads: " + num2);
                ThreadPool.GetMinThreads(out num, out num2);
                this.WriteLine("GetMinThreads.WorkerThreads: " + num);
                this.WriteLine("GetMinThreads.CompletionPortThreads: " + num2);
                ThreadPool.GetAvailableThreads(out num, out num2);
                this.WriteLine("GetAvailableThreads.WorkerThreads: " + num);
                this.WriteLine("GetAvailableThreads.WompletionPortThreads: " + num2);
                this.DecreaseIndent();
                this.WriteLine("LogThreadPoolInfo - END");
            }
        }

        private void WriteDateTimeAndThreadId()
        {
            this.m_stringBuilder.Clear();
            this.AppendDateAndTime(this.m_stringBuilder);
            this.m_stringBuilder.Append(" - ");
            this.m_stringBuilder.Append("Thread: ");
            this.m_stringBuilder.Concat(this.GetThreadId(), 3, ' ');
            this.m_stringBuilder.Append(" ->  ");
            this.m_stringBuilder.Append(' ', this.GetIdentByThread(this.GetThreadId()) * 3);
            this.WriteStringBuilder(this.m_stringBuilder);
        }

        public void WriteLine(Exception ex)
        {
            if (this.m_enabled)
            {
                WriteLine(this.m_normalWriter, ex);
                this.m_streamWriter.Flush();
            }
        }

        public void WriteLine(string msg)
        {
            if (this.m_enabled)
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (this.m_enabled)
                    {
                        this.WriteDateTimeAndThreadId();
                        this.WriteString(msg);
                        this.m_streamWriter.WriteLine();
                        if (this.m_alwaysFlush)
                        {
                            this.m_streamWriter.Flush();
                        }
                    }
                }
            }
            if (this.LogForMemoryProfiler)
            {
                MyMemoryLogs.AddConsoleLine(msg);
            }
        }

        private static void WriteLine(Action<string> writer, Exception ex)
        {
            writer("Exception occured: " + ((ex == null) ? "null" : ex.ToString()));
            if ((ex != null) && (ex is ReflectionTypeLoadException))
            {
                writer("LoaderExceptions: ");
                foreach (Exception exception in ((ReflectionTypeLoadException) ex).LoaderExceptions)
                {
                    WriteLine(writer, exception);
                }
            }
            if ((ex != null) && (ex.InnerException != null))
            {
                writer("InnerException: ");
                WriteLine(writer, ex.InnerException);
            }
        }

        public void WriteLine(string message, LoggingOptions option)
        {
            if (this.LogFlag(option))
            {
                this.WriteLine(message);
            }
        }

        public void WriteLineAndConsole(string msg)
        {
            this.WriteLine(msg);
            this.WriteLineToConsole(msg);
        }

        public void WriteLineToConsole(string msg)
        {
            using (this.m_consoleStringBuilderLock.AcquireExclusiveUsing())
            {
                this.m_consoleStringBuilder.Clear();
                this.AppendDateAndTime(this.m_consoleStringBuilder);
                this.m_consoleStringBuilder.Append(": ");
                this.m_consoleStringBuilder.Append(msg);
                Console.WriteLine(this.m_consoleStringBuilder.ToString());
            }
        }

        public void WriteMemoryUsage(string prefixText)
        {
            this.WriteLine(this.GetGCMemoryString(prefixText));
        }

        private void WriteString(string text)
        {
            if (text == null)
            {
                text = "UNKNOWN ERROR: Text is null in MyLog.WriteString()!";
            }
            try
            {
                this.m_streamWriter.Write(text);
            }
            catch (Exception)
            {
                this.m_streamWriter.Write("Error: The string is corrupted and cannot be displayed");
            }
        }

        private void WriteStringBuilder(StringBuilder sb)
        {
            if (((sb != null) && (this.m_tmpWrite != null)) && (this.m_streamWriter != null))
            {
                if (this.m_tmpWrite.Length < sb.Length)
                {
                    Array.Resize<char>(ref this.m_tmpWrite, Math.Max(this.m_tmpWrite.Length * 2, sb.Length));
                }
                sb.CopyTo(0, this.m_tmpWrite, 0, sb.Length);
                try
                {
                    this.m_streamWriter.Write(this.m_tmpWrite, 0, sb.Length);
                    Array.Clear(this.m_tmpWrite, 0, sb.Length);
                }
                catch (Exception)
                {
                    this.m_streamWriter.Write("Error: The string is corrupted and cannot be written");
                    Array.Clear(this.m_tmpWrite, 0, this.m_tmpWrite.Length);
                }
            }
        }

        public static MyLog Default
        {
            get => 
                m_default;
            set => 
                (m_default = value);
        }

        public LoggingOptions Options
        {
            get => 
                this.m_loggingOptions;
            set => 
                (value = this.m_loggingOptions);
        }

        public bool LogEnabled =>
            this.m_enabled;

        [StructLayout(LayoutKind.Sequential)]
        public struct IndentToken : IDisposable
        {
            private MyLog m_log;
            private LoggingOptions m_options;
            internal IndentToken(MyLog log, LoggingOptions options)
            {
                this.m_log = log;
                this.m_options = options;
                this.m_log.IncreaseIndent(options);
            }

            public void Dispose()
            {
                if (this.m_log != null)
                {
                    this.m_log.DecreaseIndent(this.m_options);
                    this.m_log = null;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyLogIndentKey
        {
            public int ThreadId;
            public int Indent;
            public MyLogIndentKey(int threadId, int indent)
            {
                this.ThreadId = threadId;
                this.Indent = indent;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyLogIndentValue
        {
            public long LastGcTotalMemory;
            public long LastWorkingSet;
            public DateTimeOffset LastDateTimeOffset;
            public MyLogIndentValue(long lastGcTotalMemory, long lastWorkingSet, DateTimeOffset lastDateTimeOffset)
            {
                this.LastGcTotalMemory = lastGcTotalMemory;
                this.LastWorkingSet = lastWorkingSet;
                this.LastDateTimeOffset = lastDateTimeOffset;
            }
        }
    }
}

