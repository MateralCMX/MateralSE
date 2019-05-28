namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityDampeners : MyStatBase
    {
        public MyStatControlledEntityDampeners()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_dampeners");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                if (controlledEntity is MyLargeTurretBase)
                {
                    controlledEntity = (controlledEntity as MyLargeTurretBase).PreviousControlledEntity;
                }
                if (controlledEntity.EnabledDamping)
                {
                    if (controlledEntity.RelativeDampeningEntity == null)
                    {
                        base.CurrentValue = 1f;
                    }
                    else
                    {
                        base.CurrentValue = 0.5f;
                    }
                }
                else
                {
                    base.CurrentValue = 0f;
                }
            }
        }
    }
}

