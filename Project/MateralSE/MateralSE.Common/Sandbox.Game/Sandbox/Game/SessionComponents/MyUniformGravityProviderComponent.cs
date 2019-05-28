namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x29a)]
    public class MyUniformGravityProviderComponent : MySessionComponentBase, IMyGravityProvider
    {
        public readonly Vector3 Gravity = (Vector3.Down * 9.81f);

        public float GetGravityMultiplier(Vector3D worldPoint) => 
            1f;

        public void GetProxyAABB(out BoundingBoxD aabb)
        {
            throw new NotSupportedException();
        }

        public Vector3 GetWorldGravity(Vector3D worldPoint) => 
            this.Gravity;

        public Vector3 GetWorldGravityGrid(Vector3D worldPoint) => 
            this.Gravity;

        public bool IsPositionInRange(Vector3D worldPoint) => 
            true;

        public bool IsPositionInRangeGrid(Vector3D worldPoint) => 
            true;

        public override void LoadData()
        {
            MyGravityProviderSystem.AddNaturalGravityProvider(this);
        }

        protected override void UnloadData()
        {
            MyGravityProviderSystem.RemoveNaturalGravityProvider(this);
        }

        public bool IsWorking =>
            true;
    }
}

