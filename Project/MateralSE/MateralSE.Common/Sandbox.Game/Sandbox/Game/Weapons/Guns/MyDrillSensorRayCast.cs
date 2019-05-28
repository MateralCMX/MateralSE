namespace Sandbox.Game.Weapons.Guns
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender;

    public class MyDrillSensorRayCast : MyDrillSensorBase
    {
        private static List<MyLineSegmentOverlapResult<MyEntity>> m_raycastResults = new List<MyLineSegmentOverlapResult<MyEntity>>();
        private float m_rayLength;
        private float m_originOffset;
        private Vector3D m_origin;
        private List<MyPhysics.HitInfo> m_hits;

        public MyDrillSensorRayCast(float originOffset, float rayLength, MyDefinitionBase drillDefinition)
        {
            this.m_rayLength = rayLength;
            this.m_originOffset = originOffset;
            this.m_hits = new List<MyPhysics.HitInfo>();
            base.m_drillDefinition = drillDefinition;
        }

        public override void DebugDraw()
        {
            MyRenderProxy.DebugDrawLine3D(this.m_origin, base.FrontPoint, Color.Red, Color.Blue, false, false);
        }

        public override void OnWorldPositionChanged(ref MatrixD worldMatrix)
        {
            Vector3D forward = worldMatrix.Forward;
            this.m_origin = worldMatrix.Translation + (forward * this.m_originOffset);
            base.FrontPoint = this.m_origin + (this.m_rayLength * forward);
            base.Center = this.m_origin;
        }

        protected override void ReadEntitiesInRange()
        {
            base.m_entitiesInRange.Clear();
            this.m_hits.Clear();
            MyPhysics.CastRay(this.m_origin, base.FrontPoint, this.m_hits, 0x18);
            MyDrillSensorBase.DetectionInfo info = new MyDrillSensorBase.DetectionInfo();
            bool flag = false;
            foreach (MyPhysics.HitInfo info2 in this.m_hits)
            {
                HkWorld.HitInfo hkHitInfo = info2.HkHitInfo;
                if (hkHitInfo.Body != null)
                {
                    IMyEntity hitEntity = hkHitInfo.GetHitEntity();
                    if (hitEntity != null)
                    {
                        IMyEntity topMostParent = hitEntity.GetTopMostParent(null);
                        if (!base.IgnoredEntities.Contains<IMyEntity>(topMostParent))
                        {
                            Vector3D position = info2.Position;
                            MyCubeGrid grid = topMostParent as MyCubeGrid;
                            if (grid != null)
                            {
                                position = (grid.GridSizeEnum != MyCubeSize.Large) ? (position + (info2.HkHitInfo.Normal * -0.02f)) : (position + (info2.HkHitInfo.Normal * -0.08f));
                            }
                            if (!base.m_entitiesInRange.TryGetValue(topMostParent.EntityId, out info))
                            {
                                base.m_entitiesInRange[topMostParent.EntityId] = new MyDrillSensorBase.DetectionInfo(topMostParent as MyEntity, position);
                            }
                            else if (Vector3.DistanceSquared((Vector3) info.DetectionPoint, (Vector3) this.m_origin) > Vector3.DistanceSquared((Vector3) position, (Vector3) this.m_origin))
                            {
                                base.m_entitiesInRange[topMostParent.EntityId] = new MyDrillSensorBase.DetectionInfo(topMostParent as MyEntity, position);
                            }
                            if ((hitEntity is MyEnvironmentSector) && !flag)
                            {
                                MyEnvironmentSector entity = hitEntity as MyEnvironmentSector;
                                int itemFromShapeKey = entity.GetItemFromShapeKey(hkHitInfo.GetShapeKey(0));
                                if (entity.DataView.Items[itemFromShapeKey].ModelIndex >= 0)
                                {
                                    flag = true;
                                    base.m_entitiesInRange[hitEntity.EntityId] = new MyDrillSensorBase.DetectionInfo(entity, position, itemFromShapeKey);
                                }
                            }
                        }
                    }
                }
            }
            LineD ray = new LineD(this.m_origin, base.FrontPoint);
            using (m_raycastResults.GetClearToken<MyLineSegmentOverlapResult<MyEntity>>())
            {
                MyGamePruningStructure.GetAllEntitiesInRay(ref ray, m_raycastResults, MyEntityQueryType.Both);
                foreach (MyLineSegmentOverlapResult<MyEntity> result in m_raycastResults)
                {
                    if (result.Element == null)
                    {
                        continue;
                    }
                    MyEntity topMostParent = result.Element.GetTopMostParent(null);
                    if (!base.IgnoredEntities.Contains(topMostParent))
                    {
                        MyCubeBlock element = result.Element as MyCubeBlock;
                        if (element != null)
                        {
                            Vector3D detectionPoint = new Vector3D();
                            if (!element.SlimBlock.BlockDefinition.HasPhysics)
                            {
                                float? nullable1;
                                MatrixD worldMatrixNormalizedInv = element.PositionComp.WorldMatrixNormalizedInv;
                                Vector3D vectord3 = Vector3D.Transform(this.m_origin, ref worldMatrixNormalizedInv);
                                Vector3D vectord4 = Vector3D.Transform(base.FrontPoint, ref worldMatrixNormalizedInv);
                                float? nullable2 = new Ray((Vector3) vectord3, Vector3.Normalize(vectord4 - vectord3)).Intersects(element.PositionComp.LocalAABB);
                                float rayLength = 0.01f;
                                if (nullable2 != null)
                                {
                                    nullable1 = new float?(nullable2.GetValueOrDefault() + rayLength);
                                }
                                else
                                {
                                    nullable1 = null;
                                }
                                float? nullable = nullable1;
                                if (nullable != null)
                                {
                                    nullable2 = nullable;
                                    rayLength = this.m_rayLength;
                                    if ((nullable2.GetValueOrDefault() <= rayLength) & (nullable2 != null))
                                    {
                                        detectionPoint = this.m_origin + (Vector3D.Normalize(base.FrontPoint - this.m_origin) * ((double) nullable.Value));
                                        if (!base.m_entitiesInRange.TryGetValue(topMostParent.EntityId, out info))
                                        {
                                            base.m_entitiesInRange[topMostParent.EntityId] = new MyDrillSensorBase.DetectionInfo(topMostParent, detectionPoint);
                                        }
                                        else if (Vector3.DistanceSquared((Vector3) info.DetectionPoint, (Vector3) this.m_origin) > Vector3.DistanceSquared((Vector3) detectionPoint, (Vector3) this.m_origin))
                                        {
                                            base.m_entitiesInRange[topMostParent.EntityId] = new MyDrillSensorBase.DetectionInfo(topMostParent, detectionPoint);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

