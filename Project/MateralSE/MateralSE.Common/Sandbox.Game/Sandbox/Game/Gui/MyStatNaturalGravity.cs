namespace Sandbox.Game.GUI
{
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public class MyStatNaturalGravity : MyStatBase
    {
        public MyStatNaturalGravity()
        {
            base.Id = MyStringHash.GetOrCompute("natural_gravity");
        }

        public override void Update()
        {
            Vector3D position;
            if ((MySession.Static.ControlledEntity == null) || !(MySession.Static.ControlledEntity is MyEntity))
            {
                position = MySector.MainCamera.Position;
            }
            else
            {
                position = (MySession.Static.ControlledEntity as MyEntity).WorldMatrix.Translation;
            }
            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            base.CurrentValue = vector.Length() / 9.81f;
        }

        public override float MaxValue =>
            float.MaxValue;
    }
}

