namespace VRage.Stats
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyStatToken : IDisposable
    {
        private readonly MyGameTimer m_timer;
        private readonly MyTimeSpan m_startTime;
        private readonly MyStat m_stat;
        internal MyStatToken(MyGameTimer timer, MyStat stat)
        {
            this.m_timer = timer;
            this.m_startTime = timer.Elapsed;
            this.m_stat = stat;
        }

        public void Dispose()
        {
            this.m_stat.Write((float) (this.m_timer.Elapsed - this.m_startTime).Milliseconds);
        }
    }
}

