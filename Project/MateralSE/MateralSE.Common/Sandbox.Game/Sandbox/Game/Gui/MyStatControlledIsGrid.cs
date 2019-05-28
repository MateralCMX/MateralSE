namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledIsGrid : MyStatBase
    {
        public MyStatControlledIsGrid()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_is_grid");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity == null)
            {
                base.CurrentValue = 0f;
            }
            if (controlledEntity is MyLargeTurretBase)
            {
                controlledEntity = (controlledEntity as MyLargeTurretBase).PreviousControlledEntity;
            }
            this.CurrentValue = (controlledEntity is MyShipController) ? ((float) 1) : ((float) 0);
        }
    }
}

