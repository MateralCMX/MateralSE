namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.World;
    using System;
    using VRage.Library.Utils;

    public class MyStatPlayerGasRefillingBase : MyStatBase
    {
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private static readonly double VISIBLE_TIME_MS = 2000.0;
        private float m_lastGasLevel;
        private double m_lastTimeChecked;

        protected virtual float GetGassLevel(MyCharacterOxygenComponent oxygenComp) => 
            0f;

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if ((localCharacter != null) && (localCharacter.OxygenComponent != null))
            {
                double totalMilliseconds = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                if ((totalMilliseconds - this.m_lastTimeChecked) > VISIBLE_TIME_MS)
                {
                    float gassLevel = this.GetGassLevel(localCharacter.OxygenComponent);
                    this.CurrentValue = (gassLevel > this.m_lastGasLevel) ? ((float) 1) : ((float) 0);
                    this.m_lastTimeChecked = totalMilliseconds;
                    this.m_lastGasLevel = gassLevel;
                }
            }
        }
    }
}

