namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyStatPlayerSuffocating : MyStatBase
    {
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private static readonly double VISIBLE_TIME_MS = 2000.0;
        private static readonly MyStringHash LOW_PRESSURE_DAMANGE_TYPE = MyStringHash.GetOrCompute("LowPressure");
        private float m_lastHealthRatio;
        private double m_lastTimeChecked;

        public MyStatPlayerSuffocating()
        {
            base.Id = MyStringHash.GetOrCompute("player_suffocating");
            this.m_lastHealthRatio = 1f;
        }

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if ((localCharacter != null) && (localCharacter.StatComp != null))
            {
                double totalMilliseconds = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                if ((totalMilliseconds - this.m_lastTimeChecked) > VISIBLE_TIME_MS)
                {
                    this.CurrentValue = (localCharacter.StatComp.LastDamage.Type == LOW_PRESSURE_DAMANGE_TYPE) ? 1f : 0f;
                    if (localCharacter.StatComp.HealthRatio >= this.m_lastHealthRatio)
                    {
                        base.CurrentValue = 0f;
                    }
                    this.m_lastTimeChecked = totalMilliseconds;
                    this.m_lastHealthRatio = localCharacter.StatComp.HealthRatio;
                }
            }
        }
    }
}

