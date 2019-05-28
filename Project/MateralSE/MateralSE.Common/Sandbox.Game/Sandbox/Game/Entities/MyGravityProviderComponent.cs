namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRageMath;

    public abstract class MyGravityProviderComponent : MyEntityComponentBase, IMyGravityProvider
    {
        protected MyGravityProviderComponent()
        {
        }

        public abstract float GetGravityMultiplier(Vector3D worldPoint);
        public abstract void GetProxyAABB(out BoundingBoxD aabb);
        public abstract Vector3 GetWorldGravity(Vector3D worldPoint);
        public abstract bool IsPositionInRange(Vector3D worldPoint);

        public override string ComponentTypeDebugString =>
            base.GetType().Name;

        public abstract bool IsWorking { get; }
    }
}

