namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.EnvironmentItems;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public static class MyItemsCollector
    {
        private static List<MyFracturedPiece> m_tmpFracturePieceList = new List<MyFracturedPiece>();
        private static List<MyEnvironmentItems.ItemInfo> m_tmpEnvItemList = new List<MyEnvironmentItems.ItemInfo>();
        private static List<ItemInfo> m_tmpItemInfoList = new List<ItemInfo>();
        private static List<ComponentInfo> m_retvalBlockInfos = new List<ComponentInfo>();
        private static List<CollectibleInfo> m_retvalCollectibleInfos = new List<CollectibleInfo>();

        public static bool FindClosestCollectableItemInPlaceArea(Vector3D fromPosition, long entityId, HashSet<MyDefinitionId> itemDefinitions, out ComponentInfo result)
        {
            List<VRage.Game.Entity.MyEntity> list = null;
            bool flag2;
            result = new ComponentInfo();
            try
            {
                VRage.Game.Entity.MyEntity entity = null;
                MyPlaceArea component = null;
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    flag2 = false;
                }
                else if (!entity.Components.TryGet<MyPlaceArea>(out component))
                {
                    flag2 = false;
                }
                else
                {
                    VRage.Game.Entity.MyEntity entity2 = null;
                    MySlimBlock block = null;
                    double maxValue = double.MaxValue;
                    bool flag = false;
                    foreach (VRage.Game.Entity.MyEntity entity3 in Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref component.WorldAABB, true))
                    {
                        if (entity3 is MyCubeGrid)
                        {
                            MyCubeGrid grid = entity3 as MyCubeGrid;
                            if (grid.BlocksCount == 1)
                            {
                                block = grid.CubeBlocks.First<MySlimBlock>();
                                if (itemDefinitions.Contains(block.BlockDefinition.Id))
                                {
                                    double num2 = Vector3D.DistanceSquared(grid.GridIntegerToWorld(block.Position), fromPosition);
                                    if (num2 < maxValue)
                                    {
                                        maxValue = num2;
                                        entity2 = grid;
                                        flag = true;
                                    }
                                }
                            }
                        }
                        if (entity3 is MyFloatingObject)
                        {
                            MyFloatingObject obj2 = entity3 as MyFloatingObject;
                            MyDefinitionId item = obj2.Item.Content.GetId();
                            if (itemDefinitions.Contains(item))
                            {
                                double num3 = Vector3D.DistanceSquared(obj2.PositionComp.WorldMatrix.Translation, fromPosition);
                                if (num3 < maxValue)
                                {
                                    maxValue = num3;
                                    entity2 = obj2;
                                    flag = false;
                                }
                            }
                        }
                    }
                    if (entity2 == null)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        result.IsBlock = flag;
                        result.EntityId = entity2.EntityId;
                        if (flag)
                        {
                            result.BlockPosition = block.Position;
                            result.ComponentDefinitionId = GetComponentId(block);
                        }
                        flag2 = true;
                    }
                }
            }
            finally
            {
                if (list != null)
                {
                    list.Clear();
                }
            }
            return flag2;
        }

        public static bool FindClosestFracturedTreeInArea(Vector3D fromPosition, long areaEntityId, MyHumanoidBot bot, out EntityInfo result)
        {
            result = new EntityInfo();
            MyPlaceArea area = MyPlaceArea.FromEntity(areaEntityId);
            if (area == null)
            {
                return false;
            }
            BoundingBoxD worldAABB = area.WorldAABB;
            double searchRadius = worldAABB.HalfExtents.Length();
            return FindClosestFracturedTreeInternal(fromPosition, worldAABB.Center, searchRadius, area, bot, out result);
        }

        public static bool FindClosestFracturedTreeInRadius(Vector3D fromPosition, double radius, MyHumanoidBot bot, out EntityInfo result) => 
            FindClosestFracturedTreeInternal(fromPosition, fromPosition, radius, null, bot, out result);

        private static bool FindClosestFracturedTreeInternal(Vector3D fromPosition, Vector3D searchCenter, double searchRadius, MyPlaceArea area, MyHumanoidBot bot, out EntityInfo result)
        {
            result = new EntityInfo();
            double maxValue = double.MaxValue;
            MyFracturedPiece piece = null;
            Vector3D vectord = new Vector3D();
            BoundingSphereD searchSphere = new BoundingSphereD(searchCenter, searchRadius);
            m_tmpFracturePieceList.Clear();
            MyFracturedPiecesManager.Static.GetFracturesInSphere(ref searchSphere, ref m_tmpFracturePieceList);
            for (int i = 0; i < m_tmpFracturePieceList.Count; i++)
            {
                Vector3D vectord2;
                MyFracturedPiece entity = m_tmpFracturePieceList[i];
                if ((MyTrees.IsEntityFracturedTree(entity) && (!IsFracturedTreeStump(entity) && (bot.AgentLogic.AiTarget.IsEntityReachable(entity) && FindClosestPointOnFracturedTree(Vector3D.Transform(fromPosition, entity.PositionComp.WorldMatrixNormalizedInv), entity, out vectord2)))) && ((area == null) || area.TestPoint(vectord2)))
                {
                    double num3 = Vector3D.DistanceSquared(vectord2, fromPosition);
                    if (num3 < maxValue)
                    {
                        maxValue = num3;
                        piece = entity;
                        vectord = vectord2;
                    }
                }
            }
            m_tmpFracturePieceList.Clear();
            if (piece == null)
            {
                return false;
            }
            result.EntityId = piece.EntityId;
            result.Target = vectord;
            return true;
        }

        public static bool FindClosestPlaceAreaInSphere(BoundingSphereD sphere, string typeName, ref MyBBMemoryTarget foundTarget)
        {
            List<MyPlaceArea> result = new List<MyPlaceArea>();
            MyPlaceAreas.Static.GetAllAreasInSphere(sphere, result);
            MyStringHash orCompute = MyStringHash.GetOrCompute(typeName);
            double num = sphere.Radius * sphere.Radius;
            MyPlaceArea area = null;
            foreach (MyPlaceArea area2 in result)
            {
                if (area2.Container.Entity == null)
                {
                    continue;
                }
                if (area2.AreaType == orCompute)
                {
                    double num2 = area2.DistanceSqToPoint(sphere.Center);
                    if (num2 < num)
                    {
                        num = num2;
                        area = area2;
                    }
                }
            }
            if (area == null)
            {
                return false;
            }
            Vector3D? position = null;
            MyBBMemoryTarget.SetTargetEntity(ref foundTarget, MyAiTargetEnum.ENTITY, area.Container.Entity.EntityId, position);
            return true;
        }

        private static bool FindClosestPointOnFracturedTree(Vector3D fromPositionFractureLocal, MyFracturedPiece fracture, out Vector3D closestPoint)
        {
            Vector4 vector;
            Vector4 vector2;
            closestPoint = new Vector3D();
            if (fracture == null)
            {
                return false;
            }
            fracture.Shape.GetShape().GetLocalAABB(0f, out vector, out vector2);
            Vector3D min = new Vector3D(vector);
            Vector3D max = new Vector3D(vector2);
            closestPoint = Vector3D.Clamp(fromPositionFractureLocal, min, max);
            closestPoint.X = (closestPoint.X + ((2.0 * (max.X + min.X)) / 2.0)) / 3.0;
            closestPoint.Y = MathHelper.Clamp(closestPoint.Y + (0.25f * (((closestPoint.Y - min.Y) < (max.Y - closestPoint.Y)) ? ((float) 1) : ((float) (-1)))), min.Y, max.Y);
            closestPoint.Z = (closestPoint.Z + ((2.0 * (max.Z + min.Z)) / 2.0)) / 3.0;
            closestPoint = Vector3D.Transform(closestPoint, fracture.PositionComp.WorldMatrix);
            return true;
        }

        public static bool FindClosestTreeInPlaceArea(Vector3D fromPosition, long entityId, MyHumanoidBot bot, out ItemInfo result)
        {
            result = new ItemInfo();
            MyPlaceArea area = MyPlaceArea.FromEntity(entityId);
            if (area == null)
            {
                return false;
            }
            BoundingBoxD worldAABB = area.WorldAABB;
            List<VRage.Game.Entity.MyEntity> entitiesInAABB = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref worldAABB, true);
            double maxValue = double.MaxValue;
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInAABB)
            {
                MyTrees trees = entity as MyTrees;
                if (trees != null)
                {
                    m_tmpEnvItemList.Clear();
                    trees.GetPhysicalItemsInRadius(worldAABB.Center, (float) worldAABB.HalfExtents.Length(), m_tmpEnvItemList);
                    foreach (MyEnvironmentItems.ItemInfo info in m_tmpEnvItemList)
                    {
                        if (!area.TestPoint(info.Transform.Position))
                        {
                            continue;
                        }
                        if (bot.AgentLogic.AiTarget.IsTreeReachable(entity, info.LocalId))
                        {
                            double num2 = Vector3D.DistanceSquared(fromPosition, info.Transform.Position);
                            if (num2 < maxValue)
                            {
                                result.ItemsEntityId = entity.EntityId;
                                result.ItemId = info.LocalId;
                                result.Target = info.Transform.Position;
                                maxValue = num2;
                            }
                        }
                    }
                    m_tmpEnvItemList.Clear();
                }
            }
            entitiesInAABB.Clear();
            return !(maxValue == double.MaxValue);
        }

        public static bool FindClosestTreeInRadius(Vector3D fromPosition, float radius, out ItemInfo result)
        {
            result = new ItemInfo();
            BoundingSphereD boundingSphere = new BoundingSphereD(fromPosition, (double) radius);
            List<VRage.Game.Entity.MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            double maxValue = double.MaxValue;
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInSphere)
            {
                MyTrees trees = entity as MyTrees;
                if (trees != null)
                {
                    trees.GetPhysicalItemsInRadius(fromPosition, radius, m_tmpEnvItemList);
                    foreach (MyEnvironmentItems.ItemInfo info in m_tmpEnvItemList)
                    {
                        double num2 = Vector3D.DistanceSquared(fromPosition, info.Transform.Position);
                        if (num2 < maxValue)
                        {
                            result.ItemsEntityId = entity.EntityId;
                            result.ItemId = info.LocalId;
                            result.Target = info.Transform.Position;
                            maxValue = num2;
                        }
                    }
                }
            }
            entitiesInSphere.Clear();
            return !(maxValue == double.MaxValue);
        }

        public static bool FindCollectableItemInRadius(Vector3D position, float radius, HashSet<MyDefinitionId> itemDefs, Vector3D initialPosition, float ignoreRadius, out ComponentInfo result)
        {
            BoundingSphereD boundingSphere = new BoundingSphereD(position, (double) radius);
            List<VRage.Game.Entity.MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            result = new ComponentInfo();
            double maxValue = double.MaxValue;
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInSphere)
            {
                if (entity is MyCubeGrid)
                {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (grid.BlocksCount == 1)
                    {
                        MySlimBlock block = grid.CubeBlocks.First<MySlimBlock>();
                        if (itemDefs.Contains(block.BlockDefinition.Id))
                        {
                            Vector3D vectord = grid.GridIntegerToWorld(block.Position);
                            if (Vector3D.DistanceSquared(vectord, initialPosition) <= (ignoreRadius * ignoreRadius))
                            {
                                continue;
                            }
                            double num2 = Vector3D.DistanceSquared(vectord, position);
                            if (num2 < maxValue)
                            {
                                maxValue = num2;
                                result.EntityId = grid.EntityId;
                                result.BlockPosition = block.Position;
                                result.ComponentDefinitionId = GetComponentId(block);
                                result.IsBlock = true;
                            }
                        }
                    }
                }
                if (entity is MyFloatingObject)
                {
                    MyFloatingObject obj2 = entity as MyFloatingObject;
                    MyDefinitionId item = obj2.Item.Content.GetId();
                    if (itemDefs.Contains(item))
                    {
                        double num3 = Vector3D.DistanceSquared(obj2.PositionComp.WorldMatrix.Translation, position);
                        if (num3 < maxValue)
                        {
                            maxValue = num3;
                            result.EntityId = obj2.EntityId;
                            result.IsBlock = false;
                        }
                    }
                }
            }
            entitiesInSphere.Clear();
            return !(maxValue == double.MaxValue);
        }

        public static List<CollectibleInfo> FindCollectiblesInRadius(Vector3D fromPosition, double radius, bool doRaycast = false)
        {
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            BoundingSphereD boundingSphere = new BoundingSphereD(fromPosition, radius);
            List<VRage.Game.Entity.MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInSphere)
            {
                bool flag = false;
                CollectibleInfo item = new CollectibleInfo();
                MyCubeBlock block = null;
                Vector3D? hitPosition = null;
                MyCubeGrid grid = TryGetAsComponent(entity, out block, true, hitPosition);
                if (grid != null)
                {
                    item.EntityId = grid.EntityId;
                    item.DefinitionId = GetComponentId(block.SlimBlock);
                    item.Amount = (block.BlockDefinition.Components == null) ? 0 : block.BlockDefinition.Components[0].Count;
                    flag = true;
                }
                else if (entity is MyFloatingObject)
                {
                    MyFloatingObject obj2 = entity as MyFloatingObject;
                    MyDefinitionId objectId = obj2.Item.Content.GetObjectId();
                    if (MyDefinitionManager.Static.GetPhysicalItemDefinition(objectId).Public)
                    {
                        item.EntityId = obj2.EntityId;
                        item.DefinitionId = objectId;
                        item.Amount = obj2.Item.Amount;
                        flag = true;
                    }
                }
                if (flag)
                {
                    bool flag2 = false;
                    MyPhysics.CastRay(fromPosition, entity.WorldMatrix.Translation, toList, 15);
                    using (List<MyPhysics.HitInfo>.Enumerator enumerator2 = toList.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            IMyEntity hitEntity = enumerator2.Current.HkHitInfo.GetHitEntity();
                            if (!ReferenceEquals(hitEntity, entity) && (!(hitEntity is MyCharacter) && (!(hitEntity is MyFracturedPiece) && !(hitEntity is MyFloatingObject))))
                            {
                                MyCubeBlock block2 = null;
                                hitPosition = null;
                                if (TryGetAsComponent(hitEntity as VRage.Game.Entity.MyEntity, out block2, true, hitPosition) == null)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!flag2)
                    {
                        m_retvalCollectibleInfos.Add(item);
                    }
                }
            }
            entitiesInSphere.Clear();
            return m_retvalCollectibleInfos;
        }

        public static List<ComponentInfo> FindComponentsInRadius(Vector3D fromPosition, double radius)
        {
            BoundingSphereD boundingSphere = new BoundingSphereD(fromPosition, radius);
            List<VRage.Game.Entity.MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInSphere)
            {
                if (entity is MyFloatingObject)
                {
                    MyFloatingObject obj2 = entity as MyFloatingObject;
                    if (!(obj2.Item.Content is MyObjectBuilder_Component))
                    {
                        continue;
                    }
                    ComponentInfo item = new ComponentInfo {
                        EntityId = obj2.EntityId,
                        BlockPosition = Vector3I.Zero,
                        ComponentDefinitionId = obj2.Item.Content.GetObjectId(),
                        IsBlock = false,
                        ComponentCount = (int) obj2.Item.Amount
                    };
                    m_retvalBlockInfos.Add(item);
                    continue;
                }
                MyCubeBlock block = null;
                Vector3D? hitPosition = null;
                MyCubeGrid grid = TryGetAsComponent(entity, out block, true, hitPosition);
                if (grid != null)
                {
                    ComponentInfo item = new ComponentInfo {
                        IsBlock = true,
                        EntityId = grid.EntityId,
                        BlockPosition = block.Position,
                        ComponentDefinitionId = GetComponentId(block.SlimBlock),
                        ComponentCount = (block.BlockDefinition.Components == null) ? 0 : block.BlockDefinition.Components[0].Count
                    };
                    m_retvalBlockInfos.Add(item);
                }
            }
            entitiesInSphere.Clear();
            return m_retvalBlockInfos;
        }

        public static bool FindFallingTreeInRadius(Vector3D position, float radius, out EntityInfo result)
        {
            result = new EntityInfo();
            BoundingSphereD searchSphere = new BoundingSphereD(position, (double) radius);
            m_tmpFracturePieceList.Clear();
            MyFracturedPiecesManager.Static.GetFracturesInSphere(ref searchSphere, ref m_tmpFracturePieceList);
            using (List<MyFracturedPiece>.Enumerator enumerator = m_tmpFracturePieceList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyFracturedPiece current = enumerator.Current;
                    if ((current.Physics.RigidBody != null) && (current.Physics.RigidBody.IsActive && (!Vector3.IsZero(current.Physics.AngularVelocity) && !Vector3.IsZero(current.Physics.LinearVelocity))))
                    {
                        result.Target = Vector3D.Transform(current.Shape.CoM, current.PositionComp.WorldMatrix);
                        result.EntityId = current.EntityId;
                        m_tmpFracturePieceList.Clear();
                        return true;
                    }
                }
            }
            m_tmpFracturePieceList.Clear();
            return false;
        }

        private static void FindFracturedTreesInternal(Vector3D fromPosition, MyPlaceArea area, BoundingSphereD sphere)
        {
            MyFracturedPiecesManager.Static.GetFracturesInSphere(ref sphere, ref m_tmpFracturePieceList);
            for (int i = m_tmpFracturePieceList.Count - 1; i >= 0; i--)
            {
                MyFracturedPiece entity = m_tmpFracturePieceList[i];
                if (!MyTrees.IsEntityFracturedTree(entity))
                {
                    m_tmpFracturePieceList.RemoveAtFast<MyFracturedPiece>(i);
                }
                else if (IsFracturedTreeStump(entity))
                {
                    m_tmpFracturePieceList.RemoveAtFast<MyFracturedPiece>(i);
                }
                else
                {
                    Vector3D vectord;
                    if (!FindClosestPointOnFracturedTree(Vector3D.Transform(fromPosition, entity.PositionComp.WorldMatrixNormalizedInv), entity, out vectord))
                    {
                        m_tmpFracturePieceList.RemoveAtFast<MyFracturedPiece>(i);
                    }
                    else if (!area.TestPoint(vectord))
                    {
                        m_tmpFracturePieceList.RemoveAtFast<MyFracturedPiece>(i);
                    }
                }
            }
        }

        public static bool FindRandomCollectableItemInPlaceArea(long entityId, HashSet<MyDefinitionId> itemDefinitions, out ComponentInfo result)
        {
            bool flag;
            result = new ComponentInfo();
            result.IsBlock = true;
            MyPlaceArea area = MyPlaceArea.FromEntity(entityId);
            if (area == null)
            {
                return false;
            }
            BoundingBoxD worldAABB = area.WorldAABB;
            List<VRage.Game.Entity.MyEntity> entitiesInAABB = null;
            try
            {
                entitiesInAABB = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref worldAABB, true);
                int index = entitiesInAABB.Count - 1;
                while (true)
                {
                    if (index < 0)
                    {
                        if (entitiesInAABB.Count == 0)
                        {
                            flag = false;
                        }
                        else
                        {
                            VRage.Game.Entity.MyEntity entity = entitiesInAABB[(int) Math.Round((double) (MyRandom.Instance.NextFloat() * (entitiesInAABB.Count - 1)))];
                            result.EntityId = entity.EntityId;
                            if (entity is MyCubeGrid)
                            {
                                MyCubeGrid grid2 = entity as MyCubeGrid;
                                MySlimBlock block = grid2.GetBlocks().First<MySlimBlock>();
                                result.EntityId = grid2.EntityId;
                                result.BlockPosition = block.Position;
                                result.ComponentDefinitionId = GetComponentId(block);
                                result.IsBlock = true;
                            }
                            else
                            {
                                result.IsBlock = false;
                            }
                            flag = true;
                        }
                        break;
                    }
                    VRage.Game.Entity.MyEntity local1 = entitiesInAABB[index];
                    MyCubeGrid grid = local1 as MyCubeGrid;
                    MyFloatingObject obj2 = local1 as MyFloatingObject;
                    if ((obj2 == null) && ((grid == null) || (grid.BlocksCount != 1)))
                    {
                        entitiesInAABB.RemoveAtFast<VRage.Game.Entity.MyEntity>(index);
                    }
                    else if (grid != null)
                    {
                        MySlimBlock block = grid.CubeBlocks.First<MySlimBlock>();
                        if (!itemDefinitions.Contains(block.BlockDefinition.Id))
                        {
                            entitiesInAABB.RemoveAtFast<VRage.Game.Entity.MyEntity>(index);
                        }
                    }
                    else if (obj2 != null)
                    {
                        MyDefinitionId item = obj2.Item.Content.GetId();
                        if (!itemDefinitions.Contains(item))
                        {
                            entitiesInAABB.RemoveAtFast<VRage.Game.Entity.MyEntity>(index);
                        }
                    }
                    index--;
                }
            }
            finally
            {
                entitiesInAABB.Clear();
            }
            return flag;
        }

        public static bool FindRandomFracturedTreeInPlaceArea(Vector3D fromPosition, long entityId, out EntityInfo result)
        {
            result = new EntityInfo();
            MyPlaceArea area = MyPlaceArea.FromEntity(entityId);
            if (area == null)
            {
                return false;
            }
            BoundingBoxD worldAABB = area.WorldAABB;
            BoundingSphereD sphere = new BoundingSphereD(worldAABB.Center, worldAABB.HalfExtents.Length());
            m_tmpFracturePieceList.Clear();
            FindFracturedTreesInternal(fromPosition, area, sphere);
            if (m_tmpFracturePieceList.Count == 0)
            {
                m_tmpFracturePieceList.Clear();
                return false;
            }
            int num = (int) Math.Round((double) (MyRandom.Instance.NextFloat() * (m_tmpFracturePieceList.Count - 1)));
            MyFracturedPiece piece = m_tmpFracturePieceList[num];
            m_tmpFracturePieceList.Clear();
            result.EntityId = piece.EntityId;
            result.Target = piece.PositionComp.GetPosition();
            return true;
        }

        public static bool FindRandomTreeInPlaceArea(long entityId, out ItemInfo result)
        {
            bool flag;
            result = new ItemInfo();
            MyPlaceArea area = MyPlaceArea.FromEntity(entityId);
            if (area == null)
            {
                return false;
            }
            BoundingBoxD worldAABB = area.WorldAABB;
            List<VRage.Game.Entity.MyEntity> entitiesInAABB = null;
            try
            {
                entitiesInAABB = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref worldAABB, true);
                m_tmpItemInfoList.Clear();
                foreach (MyTrees trees in entitiesInAABB)
                {
                    if (trees != null)
                    {
                        m_tmpEnvItemList.Clear();
                        trees.GetPhysicalItemsInRadius(worldAABB.Center, (float) worldAABB.HalfExtents.Length(), m_tmpEnvItemList);
                        foreach (MyEnvironmentItems.ItemInfo info in m_tmpEnvItemList)
                        {
                            if (area.TestPoint(info.Transform.Position))
                            {
                                ItemInfo item = new ItemInfo {
                                    ItemsEntityId = trees.EntityId,
                                    ItemId = info.LocalId,
                                    Target = info.Transform.Position
                                };
                                m_tmpItemInfoList.Add(item);
                            }
                        }
                        m_tmpEnvItemList.Clear();
                    }
                }
                if (m_tmpItemInfoList.Count == 0)
                {
                    m_tmpItemInfoList.Clear();
                    flag = false;
                }
                else
                {
                    int num = (int) Math.Round((double) (MyRandom.Instance.NextFloat() * (m_tmpItemInfoList.Count - 1)));
                    result = m_tmpItemInfoList[num];
                    m_tmpItemInfoList.Clear();
                    flag = true;
                }
            }
            finally
            {
                entitiesInAABB.Clear();
            }
            return flag;
        }

        private static MyDefinitionId GetComponentId(MySlimBlock block)
        {
            MyCubeBlockDefinition.Component[] components = block.BlockDefinition.Components;
            if ((components != null) && (components.Length != 0))
            {
                return components[0].Definition.Id;
            }
            return new MyDefinitionId();
        }

        private static bool IsFracturedTreeStump(MyFracturedPiece fracture)
        {
            Vector4 vector;
            Vector4 vector2;
            fracture.Shape.GetShape().GetLocalAABB(0f, out vector, out vector2);
            return ((vector2.Y - vector.Y) < (3.5 * (vector2.X - vector.X)));
        }

        public static MyCubeGrid TryGetAsComponent(VRage.Game.Entity.MyEntity entity, out MyCubeBlock block, bool blockManipulatedEntity = true, Vector3D? hitPosition = new Vector3D?())
        {
            block = null;
            if (!entity.MarkedForClose)
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid == null)
                {
                    return null;
                }
                if (grid.GridSizeEnum != MyCubeSize.Small)
                {
                    return null;
                }
                MyCubeGrid grid2 = null;
                if (MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID && (hitPosition != null))
                {
                    Vector3I vectori;
                    grid.FixTargetCube(out vectori, (Vector3) (Vector3D.Transform(hitPosition.Value, grid.PositionComp.WorldMatrixNormalizedInv) / ((double) grid.GridSize)));
                    MySlimBlock cubeBlock = grid.GetCubeBlock(vectori);
                    if ((cubeBlock != null) && cubeBlock.IsFullIntegrity)
                    {
                        block = cubeBlock.FatBlock;
                    }
                }
                else
                {
                    if (grid.CubeBlocks.Count != 1)
                    {
                        return null;
                    }
                    if (grid.IsStatic)
                    {
                        return null;
                    }
                    if (!MyCubeGrid.IsGridInCompleteState(grid))
                    {
                        return null;
                    }
                    if (MyCubeGridSmallToLargeConnection.Static.TestGridSmallToLargeConnection(grid))
                    {
                        return null;
                    }
                    HashSet<MySlimBlock>.Enumerator enumerator = grid.CubeBlocks.GetEnumerator();
                    enumerator.MoveNext();
                    block = enumerator.Current.FatBlock;
                    enumerator.Dispose();
                    grid2 = grid;
                }
                if (block != null)
                {
                    if (!MyDefinitionManager.Static.IsComponentBlock(block.BlockDefinition.Id))
                    {
                        return null;
                    }
                    if (block.IsSubBlock)
                    {
                        return null;
                    }
                    DictionaryReader<string, MySlimBlock> subBlocks = block.GetSubBlocks();
                    if (!subBlocks.IsValid || (subBlocks.Count <= 0))
                    {
                        return grid2;
                    }
                }
            }
            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CollectibleInfo
        {
            public long EntityId;
            public MyDefinitionId DefinitionId;
            public MyFixedPoint Amount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentInfo
        {
            public long EntityId;
            public Vector3I BlockPosition;
            public MyDefinitionId ComponentDefinitionId;
            public int ComponentCount;
            public bool IsBlock;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EntityInfo
        {
            public Vector3D Target;
            public long EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ItemInfo
        {
            public Vector3D Target;
            public long ItemsEntityId;
            public int ItemId;
        }
    }
}

