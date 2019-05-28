namespace Sandbox.Engine.Utils
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    internal class MyMemoryProfiler
    {
        private static List<MyMemoryLogs.MyMemoryEvent> m_managed;
        private static List<MyMemoryLogs.MyMemoryEvent> m_native;
        private static List<MyMemoryLogs.MyMemoryEvent> m_timed;
        private static List<MyMemoryLogs.MyMemoryEvent> m_events;
        private static bool m_initialized = false;
        private static bool Enabled = false;
        private static Vector2 GraphOffset = new Vector2(0.1f, 0.5f);
        private static Vector2 GraphSize = new Vector2(0.8f, -0.3f);

        public static unsafe void Draw()
        {
            if (!m_initialized)
            {
                m_managed = MyMemoryLogs.GetManaged();
                m_native = MyMemoryLogs.GetNative();
                m_timed = MyMemoryLogs.GetTimed();
                m_events = MyMemoryLogs.GetEvents();
                m_initialized = true;
            }
            float x = 0f;
            float num2 = 0f;
            float num3 = 0f;
            if (m_events.Count > 0)
            {
                x = (float) (m_events[m_events.Count - 1].EndTime - m_events[0].StartTime).TotalSeconds;
            }
            for (int i = 0; i < m_events.Count; i++)
            {
                num3 += m_events[i].ProcessDelta;
                num2 = Math.Max(Math.Max(num2, m_events[i].ProcessStartSize), m_events[i].ProcessEndSize);
            }
            MyMemoryLogs.MyMemoryEvent eventFromCursor = GetEventFromCursor(((MyGuiSandbox.MouseCursorPosition - GraphOffset) * new Vector2(GraphSize.X, GraphSize.Y)) * new Vector2(x, num2));
            if (eventFromCursor != null)
            {
                new StringBuilder(100).Append(eventFromCursor.Name);
            }
            float num4 = (num2 > 0f) ? (1f / num2) : 0f;
            float num5 = (x > 0f) ? (1f / x) : 0f;
            int num6 = 0;
            foreach (MyMemoryLogs.MyMemoryEvent local1 in m_events)
            {
                float totalSeconds = (float) (local1.StartTime - m_events[0].StartTime).TotalSeconds;
                TimeSpan span2 = (TimeSpan) (local1.EndTime - m_events[0].StartTime);
                float single1 = (float) span2.TotalSeconds;
                float managedStartSize = local1.ManagedStartSize;
                float managedEndSize = local1.ManagedEndSize;
                float processStartSize = local1.ProcessStartSize;
                float processEndSize = local1.ProcessEndSize;
                if ((num6 % 2) != 1)
                {
                    Color lightGreen = Color.LightGreen;
                }
                else
                {
                    Color green = Color.Green;
                }
                num6++;
                if ((num6 % 2) != 1)
                {
                    Color lightBlue = Color.LightBlue;
                }
                else
                {
                    Color blue = Color.Blue;
                }
                if (local1 == eventFromCursor)
                {
                    Color yellow = Color.Yellow;
                    Color orange = Color.Orange;
                }
            }
            StringBuilder builder = new StringBuilder();
            Vector2 vector = new Vector2(100f, 500f);
            for (int j = 0; (j < 50) && (j < m_native.Count); j++)
            {
                builder.Clear();
                builder.Append(m_native[j].Name);
                builder.Append((9.536743E-07f * m_native[j].ManagedDelta).ToString("GC: 0.0 MB "));
                builder.Clear();
                builder.Append((9.536743E-07f * m_native[j].ProcessDelta).ToString("Process: 0.0 MB "));
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] += 13f;
            }
            vector = new Vector2(1000f, 500f);
            float* singlePtr2 = (float*) ref vector.Y;
            singlePtr2[0] += 10f;
            for (int k = 0; (k < 50) && (k < m_timed.Count); k++)
            {
                builder.Clear();
                builder.Append(m_native[k].Name);
                builder.Append(m_timed[k].DeltaTime.ToString(" 0.000 s"));
                float* singlePtr3 = (float*) ref vector.Y;
                singlePtr3[0] += 13f;
            }
        }

        private static MyMemoryLogs.MyMemoryEvent GetEventFromCursor(Vector2 screenPosition)
        {
            Vector2 vector = screenPosition;
            for (int i = 0; i < m_events.Count; i++)
            {
                float totalSeconds = (float) (m_events[i].StartTime - m_events[0].StartTime).TotalSeconds;
                TimeSpan span = (TimeSpan) (m_events[i].EndTime - m_events[0].StartTime);
                float num3 = (float) span.TotalSeconds;
                if (((vector.X >= totalSeconds) && ((vector.X <= num3) && (vector.Y >= 0f))) && (vector.Y <= m_events[i].ProcessEndSize))
                {
                    return m_events[i];
                }
            }
            return null;
        }

        private static void SaveSnapshot()
        {
        }
    }
}

