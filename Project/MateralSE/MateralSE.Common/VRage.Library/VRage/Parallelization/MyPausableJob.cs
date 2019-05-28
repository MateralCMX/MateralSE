namespace VRage.Parallelization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyPausableJob
    {
        private volatile bool m_pause;
        private AutoResetEvent m_pausedEvent = new AutoResetEvent(false);
        private AutoResetEvent m_resumedEvent = new AutoResetEvent(false);

        public void AllowPauseHere()
        {
            if (this.m_pause)
            {
                this.m_pausedEvent.Set();
                this.m_resumedEvent.WaitOne();
            }
        }

        public void Pause()
        {
            this.m_pause = true;
            this.m_pausedEvent.WaitOne();
        }

        public void Resume()
        {
            this.m_pause = false;
            this.m_resumedEvent.Set();
        }

        public bool IsPaused =>
            this.m_pause;
    }
}

