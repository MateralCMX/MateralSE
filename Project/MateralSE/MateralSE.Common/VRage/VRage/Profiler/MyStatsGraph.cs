namespace VRage.Profiler
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;

    public static class MyStatsGraph
    {
        public static string PROFILER_NAME = "Statistics";
        private static readonly MyProfiler m_profiler = MyRenderProfiler.CreateProfiler(PROFILER_NAME, "B", false);
        private static readonly Stack<float> m_stack = new Stack<float>(0x20);

        static MyStatsGraph()
        {
            m_profiler.AutoCommit = false;
            m_profiler.SetNewLevelLimit(-1);
            m_profiler.AutoScale = true;
            m_profiler.IgnoreRoot = true;
        }

        public static void Begin(string blockName = null, int forceOrder = 0x7fffffff, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            m_profiler.StartBlock(blockName, member, line, file, forceOrder, false);
            m_stack.Push(0f);
        }

        public static void Commit()
        {
            if (MyRenderProfiler.Paused)
            {
                m_profiler.ClearFrame();
            }
            else
            {
                m_profiler.CommitFrame();
            }
        }

        public static void CustomTime(string name, float customTime, string timeFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            m_profiler.StartBlock(name, member, line, file, 0x7fffffff, false);
            m_profiler.EndBlock(member, line, file, customTime.ToTime(), 0f, timeFormat, "", null);
        }

        public static void End(float? bytesTransfered = new float?(), float customValue = 0f, string customValueFormat = "", string byteFormat = "{0} B", string callFormat = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            float num = m_stack.Pop();
            float? nullable = bytesTransfered;
            float customTime = (nullable != null) ? nullable.GetValueOrDefault() : num;
            m_profiler.EndBlock(member, line, file, customTime.ToTime(), customValue, byteFormat, customValueFormat, callFormat);
            if (m_stack.Count > 0)
            {
                m_stack.Push(m_stack.Pop() + customTime);
            }
        }

        public static void ProfileAdvanced(bool begin)
        {
            if (begin)
            {
                Begin("Advanced", 0x7fffffff, "ProfileAdvanced", 0x53, @"E:\Repo1\Sources\VRage\Profiler\MyStatsGraph.cs");
            }
            else
            {
                End(0f, 0f, null, "{0}", null, "ProfileAdvanced", 0x54, @"E:\Repo1\Sources\VRage\Profiler\MyStatsGraph.cs");
            }
        }

        private static MyTimeSpan? ToTime(this float customTime) => 
            new MyTimeSpan?(MyTimeSpan.FromMilliseconds((double) customTime));
    }
}

