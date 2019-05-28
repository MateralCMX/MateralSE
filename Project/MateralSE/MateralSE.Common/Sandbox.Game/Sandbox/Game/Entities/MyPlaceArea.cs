namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyPlaceArea : MyEntityComponentBase
    {
        public int PlaceAreaProxyId = -1;

        public MyPlaceArea(MyStringHash areaType)
        {
            this.AreaType = areaType;
        }

        public abstract double DistanceSqToPoint(Vector3D point);
        public static MyPlaceArea FromEntity(long entityId)
        {
            MyPlaceArea component = null;
            MyEntity entity = null;
            return (MyEntities.TryGetEntityById(entityId, out entity, false) ? (!entity.Components.TryGet<MyPlaceArea>(out component) ? null : component) : component);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            MyPlaceAreas.Static.AddPlaceArea(this);
        }

        public override void OnBeforeRemovedFromContainer()
        {
            MyPlaceAreas.Static.RemovePlaceArea(this);
            base.OnBeforeRemovedFromContainer();
        }

        public abstract bool TestPoint(Vector3D point);

        public abstract BoundingBoxD WorldAABB { get; }

        public MyStringHash AreaType { get; private set; }

        public override string ComponentTypeDebugString =>
            "Place Area";
    }
}

