namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyStatControlledEntitySpeed : MyStatBase
    {
        public MyStatControlledEntitySpeed()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_speed");
        }

        public override void Update()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            float num = 0f;
            if (controlledEntity != null)
            {
                Vector3 zero = Vector3.Zero;
                controlledEntity.GetLinearVelocity(ref zero, true);
                num = zero.Length();
            }
            base.CurrentValue = num;
        }

        public override float MaxValue =>
            (MyGridPhysics.ShipMaxLinearVelocity() + 7f);
    }
}

