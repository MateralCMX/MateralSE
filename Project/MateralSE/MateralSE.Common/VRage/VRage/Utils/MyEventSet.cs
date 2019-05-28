namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;

    public sealed class MyEventSet
    {
        private readonly Dictionary<MyStringId, Delegate> m_events = new Dictionary<MyStringId, Delegate>(MyStringId.Comparer);

        public void Add(MyStringId eventKey, Delegate handler)
        {
            Delegate delegate2;
            this.m_events.TryGetValue(eventKey, out delegate2);
            this.m_events[eventKey] = Delegate.Combine(delegate2, handler);
        }

        public void Raise(MyStringId eventKey, object sender, EventArgs e)
        {
            Delegate delegate2;
            this.m_events.TryGetValue(eventKey, out delegate2);
            if (delegate2 != null)
            {
                object[] args = new object[] { sender, e };
                delegate2.DynamicInvoke(args);
            }
        }

        public void Remove(MyStringId eventKey, Delegate handler)
        {
            Delegate delegate2;
            if (this.m_events.TryGetValue(eventKey, out delegate2))
            {
                delegate2 = Delegate.Remove(delegate2, handler);
                if (delegate2 != null)
                {
                    this.m_events[eventKey] = delegate2;
                }
                else
                {
                    this.m_events.Remove(eventKey);
                }
            }
        }
    }
}

