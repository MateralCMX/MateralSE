namespace ParallelTasks
{
    using System;
    using System.Threading;

    public class Semaphore
    {
        private AutoResetEvent gate;
        private int free;
        private object free_lock = new object();

        public Semaphore(int maximumCount)
        {
            this.free = maximumCount;
            this.gate = new AutoResetEvent(this.free > 0);
        }

        public void Release()
        {
            object obj2 = this.free_lock;
            lock (obj2)
            {
                this.free++;
                this.gate.Set();
            }
        }

        public void WaitOne()
        {
            this.gate.WaitOne();
            object obj2 = this.free_lock;
            lock (obj2)
            {
                this.free--;
                if (this.free > 0)
                {
                    this.gate.Set();
                }
            }
        }
    }
}

