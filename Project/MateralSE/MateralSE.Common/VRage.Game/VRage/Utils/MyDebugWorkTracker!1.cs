namespace VRage.Utils
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Threading;

    public class MyDebugWorkTracker<T> where T: new()
    {
        private SpinLockRef m_lock;
        public readonly MyQueue<T> History;
        public T Current;
        private uint m_historyLength;

        public MyDebugWorkTracker(uint historyLength = 10)
        {
            this.m_lock = new SpinLockRef();
            this.m_historyLength = historyLength;
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public void Wrap()
        {
            using (this.m_lock.Acquire())
            {
                if (this.History.Count >= this.m_historyLength)
                {
                    this.History.Dequeue();
                }
                this.History.Enqueue(this.Current);
                this.Current = Activator.CreateInstance<T>();
            }
        }
    }
}

