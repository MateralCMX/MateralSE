namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage;
    using VRage.Utils;

    public class MyStatControlledEntityBroadcasting : MyStatBase
    {
        public MyStatControlledEntityBroadcasting()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_broadcasting");
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
                MyCubeGrid parent = controlledEntity.Entity.Parent as MyCubeGrid;
                if (parent != null)
                {
                    float single1;
                    if ((parent.GridSystems.RadioSystem.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.AllEnabled) || (parent.GridSystems.RadioSystem.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.Mixed))
                    {
                        single1 = 1f;
                    }
                    else
                    {
                        single1 = 0f;
                    }
                    this.CurrentValue = single1;
                }
                else
                {
                    this.CurrentValue = controlledEntity.EnabledBroadcasting ? 1f : 0f;
                }
            }
        }
    }
}

