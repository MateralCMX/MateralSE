namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyStatPlayerInventoryCapacityChanged : MyStatBase
    {
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private static readonly double VISIBLE_TIME_MS = 3000.0;
        private int m_lastVolume;
        private double m_timeToggled;

        public MyStatPlayerInventoryCapacityChanged()
        {
            base.Id = MyStringHash.GetOrCompute("player_inventory_capacity_changed");
        }

        public override void Update()
        {
            double totalMilliseconds = TIMER.ElapsedTimeSpan.TotalMilliseconds;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                int num2 = MyFixedPoint.MultiplySafe(localCharacter.GetInventory(0).CurrentVolume, 0x3e8).ToIntSafe();
                if (this.m_lastVolume != num2)
                {
                    base.CurrentValue = 1f;
                    this.m_timeToggled = totalMilliseconds;
                    this.m_lastVolume = num2;
                }
            }
            if ((base.CurrentValue == 1f) && ((totalMilliseconds - this.m_timeToggled) > VISIBLE_TIME_MS))
            {
                base.CurrentValue = 0f;
            }
        }
    }
}

