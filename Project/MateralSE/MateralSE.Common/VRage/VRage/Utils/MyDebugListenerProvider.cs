namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using VRage;

    public class MyDebugListenerProvider
    {
        private static TraceListener[] m_storedListeners;

        [Conditional("DEBUG")]
        public static void Register()
        {
            m_storedListeners = new TraceListener[Debug.Listeners.Count];
            Debug.Listeners.CopyTo(m_storedListeners, 0);
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new MyTraceListener());
        }

        [Conditional("DEBUG")]
        public static void Unregister()
        {
            Debug.Listeners.Clear();
            Debug.Listeners.AddRange(m_storedListeners);
        }

        private class MyTraceListener : DefaultTraceListener
        {
            private readonly HashSet<string> m_ignoredMsgs = new HashSet<string>();
            private readonly FastResourceLock m_ignoredLock = new FastResourceLock();

            public MyTraceListener()
            {
                this.Name = "VRage Listener";
            }

            private void AddStackTrace(StringBuilder sb)
            {
                string stackTrace = Environment.StackTrace;
                int num = 0;
                int startIndex = 0;
                while ((startIndex < stackTrace.Length) && (num < 6))
                {
                    if (stackTrace[startIndex] == '\n')
                    {
                        num++;
                    }
                    startIndex++;
                }
                sb.Append('\n', 2).Append(stackTrace.Substring(startIndex));
            }

            [DebuggerHidden]
            public override void Fail(string message)
            {
                StringBuilder sb = new StringBuilder(message);
                this.AddStackTrace(sb);
                this.ShowMessageBox(sb.ToString());
            }

            [DebuggerHidden]
            public override void Fail(string message, string detailMessage)
            {
                StringBuilder sb = new StringBuilder(message).Append('\n', 2).Append("Detail: ").Append(detailMessage);
                this.AddStackTrace(sb);
                this.ShowMessageBox(sb.ToString());
            }

            [DllImport("User32.dll")]
            private static extern short GetAsyncKeyState(Keys vKey);
            [DebuggerHidden]
            private void ShowMessageBox(string msg)
            {
                // Invalid method body.
            }
        }
    }
}

