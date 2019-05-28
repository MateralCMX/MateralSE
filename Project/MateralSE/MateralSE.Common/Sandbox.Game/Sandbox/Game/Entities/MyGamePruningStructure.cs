namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Threading;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.Simulation)]
    public class MyGamePruningStructure : MySessionComponentBase
    {
        private static MyDynamicAABBTreeD m_dynamicObjectsTree;
        private static MyDynamicAABBTreeD m_staticObjectsTree;
        private static MyDynamicAABBTreeD m_voxelMapsTree;
        [ThreadStatic]
        private static List<MyVoxelBase> m_cachedVoxelList;
        private static readonly SpinLockRef m_movedLock = new SpinLockRef();
        private static HashSet<MyEntity> m_moved = new HashSet<MyEntity>();
        private static HashSet<MyEntity> m_movedUpdate = new HashSet<MyEntity>();

        static MyGamePruningStructure()
        {
            Init();
        }

        public static void Add(MyEntity entity)
        {
            if (((entity.Parent == null) || ((entity.Flags & EntityFlags.IsGamePrunningStructureObject) != 0)) && (entity.TopMostPruningProxyId == -1))
            {
                BoundingBoxD entityAABB = GetEntityAABB(entity);
                if (entityAABB.Size != Vector3D.Zero)
                {
                    if (IsEntityStatic(entity))
                    {
                        entity.TopMostPruningProxyId = m_staticObjectsTree.AddProxy(ref entityAABB, entity, 0, true);
                        entity.StaticForPruningStructure = true;
                    }
                    else
                    {
                        entity.TopMostPruningProxyId = m_dynamicObjectsTree.AddProxy(ref entityAABB, entity, 0, true);
                        entity.StaticForPruningStructure = false;
                    }
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if (base2 != null)
                    {
                        base2.VoxelMapPruningProxyId = m_voxelMapsTree.AddProxy(ref entityAABB, entity, 0, true);
                    }
                }
            }
        }

        public static bool AnyVoxelMapInBox(ref BoundingBoxD box) => 
            m_voxelMapsTree.OverlapsAnyLeafBoundingBox(ref box);

        public static void Clear()
        {
            m_voxelMapsTree.Clear();
            m_dynamicObjectsTree.Clear();
            m_staticObjectsTree.Clear();
            using (m_movedLock.Acquire())
            {
                m_moved.Clear();
            }
        }

        public static void DebugDraw()
        {
            Color? nullable;
            List<BoundingBoxD> boxsList = new List<BoundingBoxD>();
            m_dynamicObjectsTree.GetAllNodeBounds(boxsList);
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(MatrixD.Identity, new Color(Color.SkyBlue, 0.05f), false, false))
            {
                foreach (BoundingBoxD xd in boxsList)
                {
                    nullable = null;
                    aabb.Add(ref xd, nullable);
                }
            }
            boxsList.Clear();
            m_staticObjectsTree.GetAllNodeBounds(boxsList);
            using (IMyDebugDrawBatchAabb aabb2 = MyRenderProxy.DebugDrawBatchAABB(MatrixD.Identity, new Color(Color.Aquamarine, 0.05f), false, false))
            {
                foreach (BoundingBoxD xd2 in boxsList)
                {
                    nullable = null;
                    aabb2.Add(ref xd2, nullable);
                }
            }
        }

        public static void GetAllEntitiesInBox(ref BoundingBoxD box, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                if (result[i].Hierarchy != null)
                {
                    result[i].Hierarchy.QueryAABB(ref box, result);
                }
            }
        }

        public static void GetAllEntitiesInOBB(ref MyOrientedBoundingBoxD obb, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingBox<MyEntity>(ref obb, result, 0, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingBox<MyEntity>(ref obb, result, 0, false);
            }
        }

        public static void GetAllEntitiesInRay(ref LineD ray, List<MyLineSegmentOverlapResult<MyEntity>> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllLineSegment<MyEntity>(ref ray, result, true);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllLineSegment<MyEntity>(ref ray, result, false);
            }
            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                if (result[i].Element.Hierarchy != null)
                {
                    result[i].Element.Hierarchy.QueryLine(ref ray, result);
                }
            }
        }

        public static void GetAllEntitiesInSphere(ref BoundingSphereD sphere, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                if (result[i].Hierarchy != null)
                {
                    result[i].Hierarchy.QuerySphere(ref sphere, result);
                }
            }
        }

        public static void GetAllTargetsInSphere(ref BoundingSphereD sphere, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                if (result[i].Hierarchy != null)
                {
                    result[i].Hierarchy.QuerySphere(ref sphere, result);
                }
            }
        }

        public static void GetAllTopMostEntitiesInSphere(ref BoundingSphereD sphere, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingSphere<MyEntity>(ref sphere, result, false);
            }
        }

        public static void GetAllTopMostStaticEntitiesInBox(ref BoundingBoxD box, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
        }

        public static void GetAllVoxelMapsInBox(ref BoundingBoxD box, List<MyVoxelBase> result)
        {
            m_voxelMapsTree.OverlapAllBoundingBox<MyVoxelBase>(ref box, result, 0, false);
        }

        public static void GetAllVoxelMapsInSphere(ref BoundingSphereD sphere, List<MyVoxelBase> result)
        {
            m_voxelMapsTree.OverlapAllBoundingSphere<MyVoxelBase>(ref sphere, result, false);
        }

        public static void GetAproximateDynamicClustersForSize(ref BoundingBoxD container, double clusterSize, List<BoundingBoxD> clusters)
        {
            m_dynamicObjectsTree.GetAproximateClustersForAabb(ref container, clusterSize, clusters);
        }

        public static MyPlanet GetClosestPlanet(Vector3D position)
        {
            BoundingBoxD box = new BoundingBoxD(position, position);
            return GetClosestPlanet(ref box);
        }

        public static MyPlanet GetClosestPlanet(ref BoundingBoxD box)
        {
            using (MyUtils.ReuseCollection<MyVoxelBase>(ref m_cachedVoxelList))
            {
                m_voxelMapsTree.OverlapAllBoundingBox<MyVoxelBase>(ref box, m_cachedVoxelList, 0, false);
                MyPlanet planet = null;
                Vector3D center = box.Center;
                double positiveInfinity = double.PositiveInfinity;
                foreach (MyVoxelBase base2 in m_cachedVoxelList)
                {
                    if (!(base2 is MyPlanet))
                    {
                        continue;
                    }
                    double num2 = (center - base2.WorldMatrix.Translation).LengthSquared();
                    if (num2 < positiveInfinity)
                    {
                        positiveInfinity = num2;
                        planet = (MyPlanet) base2;
                    }
                }
                return planet;
            }
        }

        private static BoundingBoxD GetEntityAABB(MyEntity entity)
        {
            BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
            if (entity.Physics != null)
            {
                worldAABB = worldAABB.Include(entity.WorldMatrix.Translation + ((entity.Physics.LinearVelocity * 0.01666667f) * 5f));
            }
            return worldAABB;
        }

        public static void GetTopmostEntitiesInBox(ref BoundingBoxD box, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
        }

        public static void GetTopMostEntitiesInBox(ref BoundingBoxD box, List<MyEntity> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllBoundingBox<MyEntity>(ref box, result, 0, false);
            }
        }

        public static void GetTopmostEntitiesOverlappingRay(ref LineD ray, List<MyLineSegmentOverlapResult<MyEntity>> result, MyEntityQueryType qtype = 3)
        {
            if (qtype.HasDynamic())
            {
                m_dynamicObjectsTree.OverlapAllLineSegment<MyEntity>(ref ray, result, true);
            }
            if (qtype.HasStatic())
            {
                m_staticObjectsTree.OverlapAllLineSegment<MyEntity>(ref ray, result, false);
            }
        }

        public static void GetVoxelMapsOverlappingRay(ref LineD ray, List<MyLineSegmentOverlapResult<MyVoxelBase>> result)
        {
            m_voxelMapsTree.OverlapAllLineSegment<MyVoxelBase>(ref ray, result, true);
        }

        private static void Init()
        {
            m_dynamicObjectsTree = new MyDynamicAABBTreeD(MyConstants.GAME_PRUNING_STRUCTURE_AABB_EXTENSION, 1.0);
            m_voxelMapsTree = new MyDynamicAABBTreeD(MyConstants.GAME_PRUNING_STRUCTURE_AABB_EXTENSION, 1.0);
            m_staticObjectsTree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
        }

        private static bool IsEntityStatic(MyEntity entity) => 
            ((entity.Physics == null) || entity.Physics.IsStatic);

        public static void Move(MyEntity entity)
        {
            using (m_movedLock.Acquire())
            {
                m_moved.Add(entity);
            }
        }

        private static void MoveInternal(MyEntity entity)
        {
            if (entity.TopMostPruningProxyId != -1)
            {
                BoundingBoxD entityAABB = GetEntityAABB(entity);
                if (entityAABB.Size == Vector3D.Zero)
                {
                    Remove(entity);
                }
                else
                {
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if (base2 != null)
                    {
                        m_voxelMapsTree.MoveProxy(base2.VoxelMapPruningProxyId, ref entityAABB, Vector3D.Zero);
                    }
                    if (entity.TopMostPruningProxyId != -1)
                    {
                        bool flag = IsEntityStatic(entity);
                        if (flag != entity.StaticForPruningStructure)
                        {
                            if (entity.StaticForPruningStructure)
                            {
                                m_staticObjectsTree.RemoveProxy(entity.TopMostPruningProxyId);
                                entity.TopMostPruningProxyId = m_dynamicObjectsTree.AddProxy(ref entityAABB, entity, 0, true);
                            }
                            else
                            {
                                m_dynamicObjectsTree.RemoveProxy(entity.TopMostPruningProxyId);
                                entity.TopMostPruningProxyId = m_staticObjectsTree.AddProxy(ref entityAABB, entity, 0, true);
                            }
                            entity.StaticForPruningStructure = flag;
                        }
                        else if (entity.StaticForPruningStructure)
                        {
                            m_staticObjectsTree.MoveProxy(entity.TopMostPruningProxyId, ref entityAABB, Vector3D.Zero);
                        }
                        else
                        {
                            m_dynamicObjectsTree.MoveProxy(entity.TopMostPruningProxyId, ref entityAABB, Vector3D.Zero);
                        }
                    }
                }
            }
        }

        public static void Remove(MyEntity entity)
        {
            MyVoxelBase base2 = entity as MyVoxelBase;
            if ((base2 != null) && (base2.VoxelMapPruningProxyId != -1))
            {
                m_voxelMapsTree.RemoveProxy(base2.VoxelMapPruningProxyId);
                base2.VoxelMapPruningProxyId = -1;
            }
            if (entity.TopMostPruningProxyId != -1)
            {
                if (entity.StaticForPruningStructure)
                {
                    m_staticObjectsTree.RemoveProxy(entity.TopMostPruningProxyId);
                }
                else
                {
                    m_dynamicObjectsTree.RemoveProxy(entity.TopMostPruningProxyId);
                }
                entity.TopMostPruningProxyId = -1;
            }
        }

        public override void Simulate()
        {
            base.Simulate();
            Update();
        }

        private static void Update()
        {
            using (m_movedLock.Acquire())
            {
                MyUtils.Swap<HashSet<MyEntity>>(ref m_moved, ref m_movedUpdate);
            }
            using (HashSet<MyEntity>.Enumerator enumerator = m_movedUpdate.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MoveInternal(enumerator.Current);
                }
            }
            m_movedUpdate.Clear();
        }
    }
}

