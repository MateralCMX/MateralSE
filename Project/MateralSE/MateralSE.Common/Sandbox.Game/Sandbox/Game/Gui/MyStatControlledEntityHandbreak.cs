namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityHandbreak : MyStatBase
    {
        public MyStatControlledEntityHandbreak()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_handbreak");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                MyCubeGrid parent = controlledEntity.Entity.Parent as MyCubeGrid;
                if (parent != null)
                {
                    this.CurrentValue = parent.GridSystems.WheelSystem.HandBrake ? 1f : 0f;
                }
            }
        }
    }
}

