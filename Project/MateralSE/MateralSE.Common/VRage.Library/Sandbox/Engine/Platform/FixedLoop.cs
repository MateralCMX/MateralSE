namespace Sandbox.Engine.Platform
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Stats;

    public class FixedLoop : GenericLoop
    {
        private static readonly MyGameTimer m_gameTimer = new MyGameTimer();
        public readonly MyStats StatGroup;
        public readonly string StatName;
        private readonly WaitForTargetFrameRate m_waiter = new WaitForTargetFrameRate(m_gameTimer, 60f);

        public FixedLoop(MyStats statGroup = null, string statName = null)
        {
            this.StatGroup = statGroup ?? new MyStats();
            this.StatName = statName ?? "WaitForUpdate";
        }

        public override void Run(GenericLoop.VoidAction tickCallback)
        {
            base.Run(delegate {
                using (this.StatGroup.Measure(this.StatName))
                {
                    this.m_waiter.Wait();
                }
                tickCallback();
            });
        }

        public void SetNextFrameDelayDelta(float delta)
        {
            this.m_waiter.SetNextFrameDelayDelta(delta);
        }

        public long TickPerFrame =>
            this.m_waiter.TickPerFrame;

        public bool EnableMaxSpeed
        {
            get => 
                this.m_waiter.EnableMaxSpeed;
            set => 
                (this.m_waiter.EnableMaxSpeed = value);
        }
    }
}

