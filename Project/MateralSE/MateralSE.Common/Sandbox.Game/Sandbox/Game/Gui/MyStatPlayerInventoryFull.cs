namespace Sandbox.Game.GUI
{
    using System;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyStatPlayerInventoryFull : MyStatBase
    {
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private static readonly double VISIBLE_TIME_MS = 5000.0;
        private double m_visibleFromMs;
        private bool m_inventoryFull;

        public MyStatPlayerInventoryFull()
        {
            base.Id = MyStringHash.GetOrCompute("player_inventory_full");
        }

        public override void Update()
        {
            if (this.m_inventoryFull && ((TIMER.ElapsedTimeSpan.TotalMilliseconds - this.m_visibleFromMs) > VISIBLE_TIME_MS))
            {
                this.m_inventoryFull = false;
                base.CurrentValue = 0f;
            }
        }

        public bool InventoryFull
        {
            get => 
                this.m_inventoryFull;
            set
            {
                if (value)
                {
                    this.m_visibleFromMs = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                }
                this.m_inventoryFull = value;
                this.CurrentValue = value ? ((float) 1) : ((float) 0);
            }
        }
    }
}

