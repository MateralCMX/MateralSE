namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class WaitForTargetFrameRate
    {
        private long m_targetTicks;
        public bool EnableMaxSpeed;
        private const bool EnableUpdateWait = true;
        private readonly MyGameTimer m_timer;
        private readonly float m_targetFrequency;
        private readonly ManualResetEventSlim m_waiter = new ManualResetEventSlim(false, 0);
        private readonly MyTimer.TimerEventHandler m_handler;
        private float m_delta;

        public WaitForTargetFrameRate(MyGameTimer timer, float targetFrequency = 60f)
        {
            this.m_timer = timer;
            this.m_targetFrequency = targetFrequency;
            this.m_handler = (a, b, c, d, e) => this.m_waiter.Set();
        }

        public void SetNextFrameDelayDelta(float delta)
        {
            this.m_delta = delta;
        }

        public void Wait()
        {
            this.m_timer.AddElapsed(MyTimeSpan.FromMilliseconds((double) -this.m_delta));
            long elapsedTicks = this.m_timer.ElapsedTicks;
            this.m_targetTicks += this.TickPerFrame;
            if ((elapsedTicks > (this.m_targetTicks + (this.TickPerFrame * 5L))) || this.EnableMaxSpeed)
            {
                this.m_targetTicks = elapsedTicks;
            }
            else
            {
                int intervalMS = (int) (MyTimeSpan.FromTicks(this.m_targetTicks - elapsedTicks).Milliseconds - 0.1);
                if (intervalMS > 0)
                {
                    this.m_waiter.Reset();
                    MyTimer.StartOneShot(intervalMS, this.m_handler);
                    this.m_waiter.Wait((int) (0x11 + ((int) this.m_delta)));
                }
                if (this.m_targetTicks >= ((this.m_timer.ElapsedTicks + this.TickPerFrame) + (this.TickPerFrame / 4L)))
                {
                    this.m_targetTicks = this.m_timer.ElapsedTicks;
                }
                else
                {
                    while (this.m_timer.ElapsedTicks < this.m_targetTicks)
                    {
                    }
                }
            }
            this.m_delta = 0f;
        }

        public long TickPerFrame =>
            ((long) ((int) Math.Round((double) (((float) MyGameTimer.Frequency) / this.m_targetFrequency))));
    }
}

