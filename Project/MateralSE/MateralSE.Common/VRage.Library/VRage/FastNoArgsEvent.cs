namespace VRage
{
    using System;
    using System.Collections.Generic;

    public class FastNoArgsEvent
    {
        private FastResourceLock m_lock = new FastResourceLock();
        private List<MyNoArgsDelegate> m_delegates = new List<MyNoArgsDelegate>(2);
        private List<MyNoArgsDelegate> m_delegatesIterator = new List<MyNoArgsDelegate>(2);

        public event MyNoArgsDelegate Event
        {
            add
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    this.m_delegates.Add(value);
                }
            }
            remove
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    this.m_delegates.Remove(value);
                }
            }
        }

        public void Raise()
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                this.m_delegatesIterator.Clear();
                foreach (MyNoArgsDelegate delegate2 in this.m_delegates)
                {
                    this.m_delegatesIterator.Add(delegate2);
                }
            }
            foreach (MyNoArgsDelegate delegate3 in this.m_delegatesIterator)
            {
                delegate3();
            }
            this.m_delegatesIterator.Clear();
        }
    }
}

