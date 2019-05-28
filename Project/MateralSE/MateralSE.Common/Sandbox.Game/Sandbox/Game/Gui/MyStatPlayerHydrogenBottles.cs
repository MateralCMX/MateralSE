namespace Sandbox.Game.GUI
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyStatPlayerHydrogenBottles : MyStatBase
    {
        private static readonly MyDefinitionId HYDROGEN_BOTTLE_ID = MyDefinitionId.Parse("MyObjectBuilder_GasContainerObject/HydrogenBottle");
        private static readonly double CHECK_INTERVAL_MS = 1000.0;
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private double m_lastCheck;

        public MyStatPlayerHydrogenBottles()
        {
            base.Id = MyStringHash.GetOrCompute("player_hydrogen_bottles");
            this.m_lastCheck = 0.0;
        }

        public override void Update()
        {
            if ((TIMER.ElapsedTimeSpan.TotalMilliseconds - CHECK_INTERVAL_MS) >= this.m_lastCheck)
            {
                this.m_lastCheck = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter == null)
                {
                    base.CurrentValue = 0f;
                }
                else
                {
                    MyInventory inventory = localCharacter.GetInventory(0);
                    if (inventory == null)
                    {
                        base.CurrentValue = 0f;
                    }
                    else
                    {
                        base.CurrentValue = 0f;
                        foreach (MyPhysicalInventoryItem item in inventory.GetItems())
                        {
                            if (!(item.Content.GetId() == HYDROGEN_BOTTLE_ID))
                            {
                                continue;
                            }
                            if (((MyObjectBuilder_GasContainerObject) item.Content).GasLevel > 1E-06f)
                            {
                                float currentValue = base.CurrentValue;
                                base.CurrentValue = currentValue + 1f;
                            }
                        }
                    }
                }
            }
        }
    }
}

