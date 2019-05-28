namespace Sandbox.Game.GUI
{
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public class MyStatArtificialGravity : MyStatBase
    {
        public MyStatArtificialGravity()
        {
            base.Id = MyStringHash.GetOrCompute("artificial_gravity");
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
                position = (MySession.Static.ControlledEntity as MyEntity).PositionComp.WorldAABB.Center;
            }
            Vector3 vector = MyGravityProviderSystem.CalculateArtificialGravityInPoint(position, MyGravityProviderSystem.CalculateArtificialGravityStrengthMultiplier(MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(position)));
            base.CurrentValue = vector.Length() / 9.81f;
        }

        public override float MaxValue =>
            float.MaxValue;
    }
}

