namespace Sandbox.Game.Weapons.Guns
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender;

    internal class MyDrillSensorSphere : MyDrillSensorBase
    {
        private float m_radius;
        private float m_centerForwardOffset;

        public MyDrillSensorSphere(float radius, float centerForwardOffset, MyDefinitionBase drillDefinition)
        {
            this.m_radius = radius;
            this.m_centerForwardOffset = centerForwardOffset;
            base.Center = (Vector3D) (centerForwardOffset * Vector3.Forward);
            base.FrontPoint = base.Center + (Vector3.Forward * this.m_radius);
            base.m_drillDefinition = drillDefinition;
        }

        public override void DebugDraw()
        {
            MyRenderProxy.DebugDrawSphere(base.Center, this.m_radius, Color.Yellow, 1f, true, false, true, false);
            MyRenderProxy.DebugDrawArrow3D(base.Center, base.FrontPoint, Color.Yellow, new Color?(Color.Red), false, 0.1, null, 0.5f, false);
        }

        public override void OnWorldPositionChanged(ref MatrixD worldMatrix)
        {
            base.Center = worldMatrix.Translation + (worldMatrix.Forward * this.m_centerForwardOffset);
            base.FrontPoint = base.Center + (worldMatrix.Forward * this.m_radius);
        }

        protected override void ReadEntitiesInRange()
        {
            base.m_entitiesInRange.Clear();
            BoundingSphereD boundingSphere = new BoundingSphereD(base.Center, (double) this.m_radius);
            List<MyEntity> topMostEntitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref boundingSphere);
            bool flag = false;
            foreach (MyEntity entity in topMostEntitiesInSphere)
            {
                if (entity is MyEnvironmentSector)
                {
                    flag = true;
                }
                if (!base.IgnoredEntities.Contains(entity))
                {
                    base.m_entitiesInRange[entity.EntityId] = new MyDrillSensorBase.DetectionInfo(entity, base.FrontPoint);
                }
            }
            topMostEntitiesInSphere.Clear();
            if (flag)
            {
                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(base.Center, base.FrontPoint, 0x18);
                if ((nullable != null) && (nullable != null))
                {
                    IMyEntity hitEntity = nullable.Value.HkHitInfo.GetHitEntity();
                    if (hitEntity is MyEnvironmentSector)
                    {
                        MyEnvironmentSector sector = hitEntity as MyEnvironmentSector;
                        int itemFromShapeKey = sector.GetItemFromShapeKey(nullable.Value.HkHitInfo.GetShapeKey(0));
                        if (sector.DataView.Items[itemFromShapeKey].ModelIndex >= 0)
                        {
                            base.m_entitiesInRange[hitEntity.EntityId] = new MyDrillSensorBase.DetectionInfo(sector, base.FrontPoint, itemFromShapeKey);
                        }
                    }
                }
            }
        }
    }
}

