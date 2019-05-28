namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Gui;
    using System;
    using System.Text;
    using VRage.Utils;

    public class MyStatControlledEntityEnergyEstimatedTimeRemaining : MyStatBase
    {
        private readonly StringBuilder m_stringBuilder = new StringBuilder();

        public MyStatControlledEntityEnergyEstimatedTimeRemaining()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_estimated_time_remaining_energy");
        }

        public override string ToString()
        {
            this.m_stringBuilder.Clear();
            MyValueFormatter.AppendTimeInBestUnit(base.CurrentValue * 3600f, this.m_stringBuilder);
            return this.m_stringBuilder.ToString();
        }

        public override void Update()
        {
            base.CurrentValue = MyHud.ShipInfo.FuelRemainingTime;
        }
    }
}

