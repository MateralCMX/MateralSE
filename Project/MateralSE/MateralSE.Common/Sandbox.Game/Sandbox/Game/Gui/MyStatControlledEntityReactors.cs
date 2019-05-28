namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityReactors : MyStatBase
    {
        public MyStatControlledEntityReactors()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_reactors");
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
                this.CurrentValue = controlledEntity.EnabledReactors ? 1f : 0f;
            }
        }
    }
}

