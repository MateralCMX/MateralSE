namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityMass : MyStatBase
    {
        public MyStatControlledEntityMass()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_mass");
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
                MyCubeGrid cubeGrid = null;
                MyCockpit entity = controlledEntity.Entity as MyCockpit;
                if (entity != null)
                {
                    cubeGrid = entity.CubeGrid;
                }
                else
                {
                    MyRemoteControl control = controlledEntity as MyRemoteControl;
                    if (control != null)
                    {
                        cubeGrid = control.CubeGrid;
                    }
                    else
                    {
                        MyLargeTurretBase base2 = controlledEntity as MyLargeTurretBase;
                        if (base2 != null)
                        {
                            cubeGrid = base2.CubeGrid;
                        }
                    }
                }
                this.CurrentValue = (cubeGrid != null) ? ((float) cubeGrid.GetCurrentMass()) : ((float) 0);
            }
        }

        public override float MaxValue =>
            0f;
    }
}

