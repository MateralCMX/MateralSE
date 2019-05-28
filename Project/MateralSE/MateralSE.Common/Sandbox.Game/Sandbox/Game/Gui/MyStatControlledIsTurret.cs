namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using VRage.Utils;

    public class MyStatControlledIsTurret : MyStatBase
    {
        public MyStatControlledIsTurret()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_is_turret");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity == null)
            {
                base.CurrentValue = 0f;
            }
            else
            {
                this.CurrentValue = (controlledEntity is IMyUserControllableGun) ? ((float) 1) : ((float) 0);
            }
        }
    }
}

