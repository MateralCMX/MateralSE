namespace Sandbox.Game.GUI
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage;
    using VRage.Utils;

    public class MyStatPlayerInventoryCapacity : MyStatBase
    {
        private float m_max;

        public MyStatPlayerInventoryCapacity()
        {
            base.Id = MyStringHash.GetOrCompute("player_inventory_capacity");
        }

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                MyInventory inventory = localCharacter.GetInventory(0);
                if (inventory != null)
                {
                    this.m_max = MyFixedPoint.MultiplySafe(inventory.MaxVolume, 0x3e8).ToIntSafe();
                    base.CurrentValue = MyFixedPoint.MultiplySafe(inventory.CurrentVolume, 0x3e8).ToIntSafe();
                }
            }
        }

        public override float MaxValue =>
            this.m_max;
    }
}

