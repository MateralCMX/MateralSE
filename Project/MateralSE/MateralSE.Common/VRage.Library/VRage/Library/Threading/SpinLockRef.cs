namespace VRage.Library.Threading
{
    using System;
    using System.Runtime.InteropServices;

    public class SpinLockRef
    {
        private SpinLock m_spinLock;

        public Token Acquire() => 
            new Token(this);

        public void Enter()
        {
            this.m_spinLock.Enter();
        }

        public void Exit()
        {
            this.m_spinLock.Exit();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Token : IDisposable
        {
            private SpinLockRef m_lock;
            public Token(SpinLockRef spin)
            {
                this.m_lock = spin;
                this.m_lock.Enter();
            }

            public void Dispose()
            {
                this.m_lock.Exit();
            }
        }
    }
}

