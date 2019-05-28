namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;

    public class MyMemoryLogs
    {
        private static MyManagedComparer m_managedComparer = new MyManagedComparer();
        private static MyNativeComparer m_nativeComparer = new MyNativeComparer();
        private static MyTimedeltaComparer m_timeComparer = new MyTimedeltaComparer();
        private static List<MyMemoryEvent> m_events = new List<MyMemoryEvent>();
        private static List<string> m_consoleLogSTART = new List<string>();
        private static List<string> m_consoleLogEND = new List<string>();
        private static Stack<MyMemoryEvent> m_stack = new Stack<MyMemoryEvent>();
        private static int IdCounter = 1;

        public static void AddConsoleLine(string line)
        {
            if (!line.EndsWith("START"))
            {
                if (line.EndsWith("END"))
                {
                    line = line.Substring(0, line.Length - 5);
                    m_consoleLogEND.Add(line);
                    m_consoleLogSTART.Clear();
                }
            }
            else
            {
                m_consoleLogEND.Clear();
                line = line.Substring(0, line.Length - 5);
                if ((m_stack.Count <= 0) || !m_stack.Peek().HasStart)
                {
                    m_consoleLogSTART.Add(line);
                }
                else
                {
                    m_events[m_events.Count].HasStart = true;
                    m_events[m_events.Count].Name = line;
                }
            }
        }

        public static void DumpMemoryUsage()
        {
            m_events.Sort(m_managedComparer);
            MyLog.Default.WriteLine("\n\n");
            MyLog.Default.WriteLine("Managed MemoryUsage: \n");
            float num = 0f;
            for (int i = 0; (i < m_events.Count) && (i < 30); i++)
            {
                float num4 = (m_events[i].ManagedDelta * 1f) / 1048576f;
                num += num4;
                MyLog.Default.WriteLine(m_events[i].Name + num4.ToString());
            }
            MyLog.Default.WriteLine("Total Managed MemoryUsage: " + num + " [MB]");
            m_events.Sort(m_nativeComparer);
            MyLog.Default.WriteLine("\n\n");
            MyLog.Default.WriteLine("Process MemoryUsage: \n");
            num = 0f;
            for (int j = 0; (j < m_events.Count) && (j < 30); j++)
            {
                float num6 = (m_events[j].ProcessDelta * 1f) / 1048576f;
                num += num6;
                MyLog.Default.WriteLine(m_events[j].Name + num6.ToString());
            }
            MyLog.Default.WriteLine("Total Process MemoryUsage: " + num + " [MB]");
            m_events.Sort(m_timeComparer);
            MyLog.Default.WriteLine("\n\n");
            MyLog.Default.WriteLine("Load time comparison: \n");
            float num2 = 0f;
            for (int k = 0; (k < m_events.Count) && (k < 30); k++)
            {
                float deltaTime = m_events[k].DeltaTime;
                num2 += deltaTime;
                MyLog.Default.WriteLine(m_events[k].Name + " " + deltaTime.ToString());
            }
            MyLog.Default.WriteLine("Total load time: " + num2 + " [s]");
        }

        public static void EndEvent(MyMemoryEvent ev)
        {
            if (m_stack.Count > 0)
            {
                MyMemoryEvent event2 = m_stack.Peek();
                ev.Name = event2.Name;
                ev.Id = event2.Id;
                ev.StartTime = event2.StartTime;
                ev.EndTime = DateTime.Now;
                m_events.Add(ev);
                m_stack.Pop();
            }
        }

        public static List<MyMemoryEvent> GetEvents() => 
            m_events;

        public static List<MyMemoryEvent> GetManaged()
        {
            List<MyMemoryEvent> list1 = new List<MyMemoryEvent>(m_events);
            list1.Sort(m_managedComparer);
            return list1;
        }

        public static List<MyMemoryEvent> GetNative()
        {
            List<MyMemoryEvent> list1 = new List<MyMemoryEvent>(m_events);
            list1.Sort(m_nativeComparer);
            return list1;
        }

        public static List<MyMemoryEvent> GetTimed()
        {
            List<MyMemoryEvent> list1 = new List<MyMemoryEvent>(m_events);
            list1.Sort(m_timeComparer);
            return list1;
        }

        public static void StartEvent()
        {
            MyMemoryEvent item = new MyMemoryEvent();
            if (m_consoleLogSTART.Count > 0)
            {
                item.Name = m_consoleLogSTART[m_consoleLogSTART.Count - 1];
                IdCounter++;
                item.Id = IdCounter;
                item.StartTime = DateTime.Now;
                m_consoleLogSTART.Clear();
                m_stack.Push(item);
            }
        }

        private class MyManagedComparer : IComparer<MyMemoryLogs.MyMemoryEvent>
        {
            public int Compare(MyMemoryLogs.MyMemoryEvent x, MyMemoryLogs.MyMemoryEvent y) => 
                (-1 * x.ManagedDelta.CompareTo(y.ManagedDelta));
        }

        public class MyMemoryEvent
        {
            public string Name;
            public bool HasStart;
            public bool HasEnd;
            public float ManagedStartSize;
            public float ManagedEndSize;
            public float ProcessStartSize;
            public float ProcessEndSize;
            public float DeltaTime;
            public int Id;
            public bool Selected;
            public DateTime StartTime;
            public DateTime EndTime;
            private List<MyMemoryLogs.MyMemoryEvent> m_childs = new List<MyMemoryLogs.MyMemoryEvent>();

            public float ManagedDelta =>
                (this.ManagedEndSize - this.ManagedStartSize);

            public float ProcessDelta =>
                (this.ProcessEndSize - this.ProcessStartSize);
        }

        private class MyNativeComparer : IComparer<MyMemoryLogs.MyMemoryEvent>
        {
            public int Compare(MyMemoryLogs.MyMemoryEvent x, MyMemoryLogs.MyMemoryEvent y) => 
                (-1 * x.ProcessDelta.CompareTo(y.ProcessDelta));
        }

        private class MyTimedeltaComparer : IComparer<MyMemoryLogs.MyMemoryEvent>
        {
            public int Compare(MyMemoryLogs.MyMemoryEvent x, MyMemoryLogs.MyMemoryEvent y) => 
                (-1 * x.DeltaTime.CompareTo(y.DeltaTime));
        }
    }
}

