namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledIsStatic : MyStatBase
    {
        public MyStatControlledIsStatic()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_is_static");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                MyCubeGrid entity = controlledEntity.Entity as MyCubeGrid;
                if (entity != null)
                {
                    this.CurrentValue = entity.IsStatic ? ((float) 1) : ((float) 0);
                    return;
                }
                MyCockpit cockpit = controlledEntity.Entity as MyCockpit;
                if (cockpit != null)
                {
                    this.CurrentValue = cockpit.CubeGrid.IsStatic ? ((float) 1) : ((float) 0);
                    return;
                }
                if (controlledEntity is MyLargeTurretBase)
                {
                    this.CurrentValue = (controlledEntity as MyLargeTurretBase).CubeGrid.IsStatic ? ((float) 1) : ((float) 0);
                }
            }
            base.CurrentValue = 0f;
        }
    }
}

