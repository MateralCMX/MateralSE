namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using VRageMath;
    using VRageRender;

    internal static class MyRadioBroadcasters
    {
        private static MyDynamicAABBTreeD m_aabbTree = new MyDynamicAABBTreeD(MyConstants.GAME_PRUNING_STRUCTURE_AABB_EXTENSION, 1.0);

        public static void AddBroadcaster(MyRadioBroadcaster broadcaster)
        {
            if (broadcaster.RadioProxyID == -1)
            {
                BoundingBoxD aabb = BoundingBoxD.CreateFromSphere(new BoundingSphereD(broadcaster.BroadcastPosition, (double) broadcaster.BroadcastRadius));
                broadcaster.RadioProxyID = m_aabbTree.AddProxy(ref aabb, broadcaster, 0, true);
            }
        }

        public static void Clear()
        {
            m_aabbTree.Clear();
        }

        public static void DebugDraw()
        {
            List<MyRadioBroadcaster> elementsList = new List<MyRadioBroadcaster>();
            m_aabbTree.GetAll<MyRadioBroadcaster>(elementsList, true, new List<BoundingBoxD>());
            for (int i = 0; i < elementsList.Count; i++)
            {
                MyRenderProxy.DebugDrawSphere(elementsList[i].BroadcastPosition, elementsList[i].BroadcastRadius, Color.White, 1f, false, false, true, false);
            }
        }

        public static void GetAllBroadcastersInSphere(BoundingSphereD sphere, List<MyDataBroadcaster> result)
        {
            m_aabbTree.OverlapAllBoundingSphere<MyDataBroadcaster>(ref sphere, result, false);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                MyRadioBroadcaster broadcaster = result[i] as MyRadioBroadcaster;
                if ((broadcaster != null) && (broadcaster.Entity != null))
                {
                    double num2 = sphere.Radius + broadcaster.BroadcastRadius;
                    num2 *= num2;
                    if (Vector3D.DistanceSquared(sphere.Center, broadcaster.BroadcastPosition) > num2)
                    {
                        result.RemoveAtFast<MyDataBroadcaster>(i);
                    }
                }
            }
        }

        public static void MoveBroadcaster(MyRadioBroadcaster broadcaster)
        {
            if (broadcaster.RadioProxyID != -1)
            {
                BoundingBoxD aabb = BoundingBoxD.CreateFromSphere(new BoundingSphereD(broadcaster.BroadcastPosition, (double) broadcaster.BroadcastRadius));
                m_aabbTree.MoveProxy(broadcaster.RadioProxyID, ref aabb, Vector3.Zero);
            }
        }

        public static void RemoveBroadcaster(MyRadioBroadcaster broadcaster)
        {
            if (broadcaster.RadioProxyID != -1)
            {
                m_aabbTree.RemoveProxy(broadcaster.RadioProxyID);
                broadcaster.RadioProxyID = -1;
            }
        }
    }
}

