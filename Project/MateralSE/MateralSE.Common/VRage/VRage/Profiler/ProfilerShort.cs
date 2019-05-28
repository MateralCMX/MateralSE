namespace VRage.Profiler
{
    using ParallelTasks;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unsharper;

    [UnsharperDisableReflection]
    public static class ProfilerShort
    {
        public const string PerformanceProfilingSymbol = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";
        private static MyRenderProfiler m_profiler;

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Begin(string blockName = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void BeginDeepTree(string blockName = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void BeginNextBlock(string blockName = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "", float previousBlockCustomValue = 0f)
        {
        }

        public static void Commit()
        {
            if (Profiler != null)
            {
                MyRenderProfiler.Commit("Commit", 0x7c, @"E:\Repo1\Sources\VRage\Profiler\ProfilerShort.cs");
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void CommitTask(MyProfiler.TaskInfo task)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void CustomValue(string name, float value, MyTimeSpan? customTime, string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void CustomValue(string name, float value, float customTimeMs, string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        public static void DestroyThread()
        {
            if (Profiler != null)
            {
                MyRenderProfiler.DestroyThread();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void End(float customValue = 0f, MyTimeSpan? customTime = new MyTimeSpan?(), string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void End(float customValue, float customTimeMs, string timeFormat = null, string valueFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void InitThread(int viewPriority)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnBeginSimulationFrame(long frameNumber)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskFinished(MyProfiler.TaskType? taskType = new MyProfiler.TaskType?(), float customValue = 0f)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void OnTaskStarted(MyProfiler.TaskType taskType, string debugName, long scheduledTimestamp = -1L)
        {
        }

        public static void SetProfiler(MyRenderProfiler profiler)
        {
            Profiler = profiler;
            DelegateExtensions.SetupProfiler(delegate (string x) {
            }, delegate (int x) {
            });
            WorkItem.SetupProfiler(delegate (MyProfiler.TaskType x, string y, long z) {
            }, delegate {
            }, delegate (string x) {
            }, delegate (float x) {
            }, delegate (int x) {
            });
        }

        public static MyRenderProfiler Profiler
        {
            get => 
                m_profiler;
            private set => 
                (m_profiler = value);
        }

        public static bool Autocommit
        {
            get => 
                MyRenderProfiler.GetAutocommit();
            set => 
                MyRenderProfiler.SetAutocommit(value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ProfilerShort.<>c <>9 = new ProfilerShort.<>c();
            public static Action<string> <>9__5_0;
            public static Action<int> <>9__5_1;
            public static Action<MyProfiler.TaskType, string, long> <>9__5_2;
            public static Action <>9__5_3;
            public static Action<string> <>9__5_4;
            public static Action<float> <>9__5_5;
            public static Action<int> <>9__5_6;

            internal void <SetProfiler>b__5_0(string x)
            {
            }

            internal void <SetProfiler>b__5_1(int x)
            {
            }

            internal void <SetProfiler>b__5_2(MyProfiler.TaskType x, string y, long z)
            {
            }

            internal void <SetProfiler>b__5_3()
            {
            }

            internal void <SetProfiler>b__5_4(string x)
            {
            }

            internal void <SetProfiler>b__5_5(float x)
            {
            }

            internal void <SetProfiler>b__5_6(int x)
            {
            }
        }
    }
}

