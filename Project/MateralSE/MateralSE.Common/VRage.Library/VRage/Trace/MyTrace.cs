namespace VRage.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class MyTrace
    {
        public const string TracingSymbol = "__RANDOM_UNDEFINED_PROFILING_SYMBOL__";
        private const string WindowName = "SE";
        private static Dictionary<int, ITrace> m_traces;
        private static readonly MyNullTrace m_nullTrace = new MyNullTrace();

        public static ITrace GetTrace(TraceWindow window)
        {
            ITrace nullTrace;
            if ((m_traces == null) || !m_traces.TryGetValue((int) window, out nullTrace))
            {
                nullTrace = m_nullTrace;
            }
            return nullTrace;
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Init(InitTraceHandler handler)
        {
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__"), Conditional("DEVELOP")]
        private static void InitInternal(InitTraceHandler handler)
        {
            m_traces = new Dictionary<int, ITrace>();
            string str = "SE";
            foreach (object obj2 in Enum.GetValues(typeof(TraceWindow)))
            {
                string traceId = (((TraceWindow) obj2) == TraceWindow.Default) ? str : (str + "_" + obj2.ToString());
                m_traces[(int) obj2] = handler(traceId, traceId);
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void InitWinTrace()
        {
        }

        private static ITrace InitWintraceHandler(string traceId, string traceName) => 
            MyWintraceWrapper.CreateTrace(traceId, traceName);

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Send(TraceWindow window, string msg, string comment = null)
        {
            GetTrace(window).Send(msg, comment);
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Watch(string name, object value)
        {
            GetTrace(TraceWindow.Default).Watch(name, value);
        }
    }
}

