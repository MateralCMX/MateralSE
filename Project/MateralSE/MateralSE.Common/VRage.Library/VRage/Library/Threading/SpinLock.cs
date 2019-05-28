namespace VRage.Library.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    public struct SpinLock
    {
        private Thread owner;
        private int recursion;
        public void Enter()
        {
            Thread currentThread = Thread.CurrentThread;
            if (ReferenceEquals(this.owner, currentThread))
            {
                Interlocked.Increment(ref this.recursion);
            }
            else
            {
                while (Interlocked.CompareExchange<Thread>(ref this.owner, currentThread, null) != null)
                {
                }
                Interlocked.Increment(ref this.recursion);
            }
        }

        public bool TryEnter()
        {
            Thread currentThread = Thread.CurrentThread;
            if (ReferenceEquals(this.owner, currentThread))
            {
                Interlocked.Increment(ref this.recursion);
                return true;
            }
            bool flag = Interlocked.CompareExchange<Thread>(ref this.owner, currentThread, null) == null;
            if (flag)
            {
                Interlocked.Increment(ref this.recursion);
            }
            return flag;
        }

        public void Exit()
        {
            if (!ReferenceEquals(Thread.CurrentThread, this.owner))
            {
                throw new InvalidOperationException("Exit cannot be called by a thread which does not currently own the lock.");
            }
            Interlocked.Decrement(ref this.recursion);
            if (this.recursion == 0)
            {
                this.owner = null;
            }
        }
    }
}

