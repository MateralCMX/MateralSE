namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class MyTimer : IDisposable
    {
        private int m_interval;
        private Action m_callback;
        private int mTimerId;
        private TimerEventHandler mHandler;
        private const int TIME_ONESHOT = 0;
        private const int TIME_PERIODIC = 1;

        public MyTimer(int intervalMS, Action callback)
        {
            this.m_interval = intervalMS;
            this.m_callback = callback;
            this.mHandler = new TimerEventHandler(this.OnTimer);
        }

        public void Dispose()
        {
            this.Stop();
            GC.SuppressFinalize(this);
        }

        ~MyTimer()
        {
            this.Stop();
        }

        private void OnTimer(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            this.m_callback();
        }

        public void Start()
        {
            timeBeginPeriod(1);
            this.mTimerId = timeSetEvent(this.m_interval, 1, this.mHandler, IntPtr.Zero, 1);
        }

        public static void StartOneShot(int intervalMS, TimerEventHandler handler)
        {
            timeSetEvent(intervalMS, 1, handler, IntPtr.Zero, 0);
        }

        public void Stop()
        {
            if (this.mTimerId != 0)
            {
                timeKillEvent(this.mTimerId);
                timeEndPeriod(1);
                this.mTimerId = 0;
            }
        }

        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventHandler handler, IntPtr user, int eventType);

        public delegate void TimerEventHandler(int id, int msg, IntPtr user, int dw1, int dw2);
    }
}

