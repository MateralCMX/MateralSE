namespace Sandbox.Game.Weapons.Guns
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    internal class MyDrillSensorBox : MyDrillSensorBase
    {
        private Vector3 m_halfExtents;
        private float m_centerOffset;
        private Quaternion m_orientation;

        public MyDrillSensorBox(Vector3 halfExtents, float centerOffset)
        {
            this.m_halfExtents = halfExtents;
            this.m_centerOffset = centerOffset;
            base.Center = Vector3.Forward * centerOffset;
            base.FrontPoint = base.Center + (Vector3.Forward * this.m_halfExtents.Z);
        }

        public override void DebugDraw()
        {
            Vector3 color = new Vector3(1f, 0f, 0f);
            MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(base.Center, this.m_halfExtents, this.m_orientation), color, 0.6f, true, false, false);
        }

        public override void OnWorldPositionChanged(ref MatrixD worldMatrix)
        {
            this.m_orientation = Quaternion.CreateFromRotationMatrix(worldMatrix.GetOrientation());
            base.Center = worldMatrix.Translation + (worldMatrix.Forward * this.m_centerOffset);
            base.FrontPoint = base.Center + (worldMatrix.Forward * this.m_halfExtents.Z);
        }

        protected override void ReadEntitiesInRange()
        {
            base.m_entitiesInRange.Clear();
            MyOrientedBoundingBox box = new MyOrientedBoundingBox((Vector3) base.Center, this.m_halfExtents, this.m_orientation);
            List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref box.GetAABB());
            for (int i = 0; i < entitiesInAABB.Count; i++)
            {
                MyEntity topMostParent = entitiesInAABB[i].GetTopMostParent(null);
                if (!base.IgnoredEntities.Contains(topMostParent))
                {
                    base.m_entitiesInRange[topMostParent.EntityId] = new MyDrillSensorBase.DetectionInfo(topMostParent, base.FrontPoint);
                }
            }
            entitiesInAABB.Clear();
        }
    }
}

