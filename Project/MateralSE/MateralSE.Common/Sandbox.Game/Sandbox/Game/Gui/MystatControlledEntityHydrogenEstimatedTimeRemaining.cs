namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Gui;
    using System;
    using VRage.Utils;

    public class MystatControlledEntityHydrogenEstimatedTimeRemaining : MyStatBase
    {
        private MyStatBase m_usageStat;

        public MystatControlledEntityHydrogenEstimatedTimeRemaining()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_estimated_time_remaining_hydrogen");
        }

        public override void Update()
        {
            if (this.m_usageStat == null)
            {
                this.m_usageStat = MyHud.Stats.GetStat<MyStatControlledEntityHydrogenCapacity>();
            }
            else
            {
                base.CurrentValue = this.m_usageStat.CurrentValue / this.m_usageStat.MaxValue;
            }
        }
    }
}

