namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;

    internal class MyGameStats
    {
        private DateTime m_lastStatMeasurePerSecond;
        private long m_previousUpdateCount = 0L;

        static MyGameStats()
        {
            Static = new MyGameStats();
        }

        private MyGameStats()
        {
            this.UpdateCount = 0L;
        }

        public void Update()
        {
            long updateCount = this.UpdateCount;
            this.UpdateCount = updateCount + 1L;
            if ((DateTime.UtcNow - this.m_lastStatMeasurePerSecond).TotalSeconds >= 1.0)
            {
                this.UpdatesPerSecond = this.UpdateCount - this.m_previousUpdateCount;
                this.m_previousUpdateCount = this.UpdateCount;
                this.m_lastStatMeasurePerSecond = DateTime.UtcNow;
            }
        }

        public static MyGameStats Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public long UpdateCount { get; private set; }

        public long UpdatesPerSecond { get; private set; }
    }
}

