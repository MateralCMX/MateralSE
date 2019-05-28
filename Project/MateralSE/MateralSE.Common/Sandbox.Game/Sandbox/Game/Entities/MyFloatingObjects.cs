namespace Sandbox.Game.Entities
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Input;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 800), StaticEventOwner]
    public class MyFloatingObjects : MySessionComponentBase
    {
        private static MyFloatingObjectComparer m_entityComparer = new MyFloatingObjectComparer();
        private static MyFloatingObjectTimestampComparer m_comparer = new MyFloatingObjectTimestampComparer();
        private static SortedSet<MyFloatingObject> m_floatingOres = new SortedSet<MyFloatingObject>(m_comparer);
        private static SortedSet<MyFloatingObject> m_floatingItems = new SortedSet<MyFloatingObject>(m_comparer);
        private static List<MyVoxelBase> m_tmpResultList = new List<MyVoxelBase>();
        private static List<MyFloatingObject> m_synchronizedFloatingObjects = new List<MyFloatingObject>();
        private static List<MyFloatingObject> m_floatingObjectsToSyncCreate = new List<MyFloatingObject>();
        private static MyFloatingObjectsSynchronizationComparer m_synchronizationComparer = new MyFloatingObjectsSynchronizationComparer();
        private static List<MyFloatingObject> m_highPriority = new List<MyFloatingObject>();
        private static List<MyFloatingObject> m_normalPriority = new List<MyFloatingObject>();
        private static List<MyFloatingObject> m_lowPriority = new List<MyFloatingObject>();
        private static int m_updateCounter = 0;
        private static bool m_needReupdateNewObjects = false;
        private static int m_checkObjectInsideVoxel = 0;
        private static List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>> m_itemsToSpawnNextUpdate = new List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>>();
        private static readonly MyConcurrentPool<List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>>> m_itemsToSpawnPool = new MyConcurrentPool<List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>>>(0, null, 0x2710, null);

        public static unsafe void AddFloatingObjectAmount(MyFloatingObject obj, MyFixedPoint amount)
        {
            MyPhysicalInventoryItem item = obj.Item;
            MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
            pointPtr1[0] += amount;
            obj.Item = item;
            obj.Amount.Value = item.Amount;
            obj.UpdateInternalState();
        }

        private static void AddToPos(MyEntity thrownEntity, Vector3D pos, MyPhysicsComponentBase motionInheritedFrom)
        {
            Vector3 forward = MyUtils.GetRandomVector3Normalized();
            Vector3 vector2 = MyUtils.GetRandomVector3Normalized();
            while (forward == vector2)
            {
                vector2 = MyUtils.GetRandomVector3Normalized();
            }
            thrownEntity.WorldMatrix = MatrixD.CreateWorld(pos, forward, Vector3.Cross(Vector3.Cross(forward, vector2), forward));
            MyEntities.Add(thrownEntity, true);
            ApplyPhysics(thrownEntity, motionInheritedFrom);
        }

        private static void AddToSynchronization(MyFloatingObject floatingObject)
        {
            m_floatingObjectsToSyncCreate.Add(floatingObject);
            m_synchronizedFloatingObjects.Add(floatingObject);
            floatingObject.OnClose += new Action<MyEntity>(MyFloatingObjects.floatingObject_OnClose);
            m_needReupdateNewObjects = true;
        }

        private static void ApplyPhysics(MyEntity thrownEntity, MyPhysicsComponentBase motionInheritedFrom)
        {
            if ((thrownEntity.Physics != null) && (motionInheritedFrom != null))
            {
                thrownEntity.Physics.LinearVelocity = motionInheritedFrom.LinearVelocity;
                thrownEntity.Physics.AngularVelocity = motionInheritedFrom.AngularVelocity;
            }
        }

        public static MyObjectBuilder_FloatingObject ChangeObjectBuilder(MyComponentDefinition componentDef, MyObjectBuilder_EntityBase entityOb)
        {
            MyObjectBuilder_PhysicalObject content = MyObjectBuilderSerializer.CreateNewObject(componentDef.Id.TypeId, componentDef.Id.SubtypeName) as MyObjectBuilder_PhysicalObject;
            Vector3D position = (Vector3D) entityOb.PositionAndOrientation.Value.Position;
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem(1, content, 1f);
            MyObjectBuilder_FloatingObject obj1 = PrepareBuilder(ref item);
            obj1.PositionAndOrientation = new MyPositionAndOrientation(position, (Vector3) entityOb.PositionAndOrientation.Value.Forward, (Vector3) entityOb.PositionAndOrientation.Value.Up);
            obj1.EntityId = entityOb.EntityId;
            return obj1;
        }

        private void CheckObjectInVoxel()
        {
            if (m_checkObjectInsideVoxel >= m_synchronizedFloatingObjects.Count)
            {
                m_checkObjectInsideVoxel = 0;
            }
            if (m_synchronizedFloatingObjects.Count > 0)
            {
                MyFloatingObject obj2 = m_synchronizedFloatingObjects[m_checkObjectInsideVoxel];
                BoundingBoxD localAABB = obj2.PositionComp.LocalAABB;
                MatrixD worldMatrix = obj2.PositionComp.WorldMatrix;
                BoundingBoxD worldAABB = obj2.PositionComp.WorldAABB;
                using (m_tmpResultList.GetClearToken<MyVoxelBase>())
                {
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref worldAABB, m_tmpResultList);
                    foreach (MyVoxelBase base2 in m_tmpResultList)
                    {
                        if (base2 == null)
                        {
                            continue;
                        }
                        if (!base2.MarkedForClose && !(base2 is MyVoxelPhysics))
                        {
                            if (!base2.AreAllAabbCornersInside(ref worldMatrix, localAABB))
                            {
                                obj2.NumberOfFramesInsideVoxel = 0;
                                continue;
                            }
                            obj2.NumberOfFramesInsideVoxel++;
                            if ((obj2.NumberOfFramesInsideVoxel > 5) && Sync.IsServer)
                            {
                                RemoveFloatingObject(obj2);
                            }
                        }
                    }
                }
            }
            m_checkObjectInsideVoxel++;
        }

        public static void EnqueueInventoryItemSpawn(MyPhysicalInventoryItem inventoryItem, BoundingBoxD boundingBox, Vector3D inheritedVelocity)
        {
            m_itemsToSpawnNextUpdate.Add(Tuple.Create<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>(inventoryItem, boundingBox, inheritedVelocity));
        }

        private static void floatingObject_OnClose(MyEntity obj)
        {
        }

        public override void LoadData()
        {
            base.LoadData();
            foreach (MyPhysicalItemDefinition definition2 in MyDefinitionManager.Static.GetPhysicalItemDefinitions())
            {
                if (!definition2.HasModelVariants)
                {
                    if (string.IsNullOrEmpty(definition2.Model))
                    {
                        continue;
                    }
                    MyModels.GetModelOnlyData(definition2.Model);
                    continue;
                }
                if (definition2.Models != null)
                {
                    string[] models = definition2.Models;
                    for (int i = 0; i < models.Length; i++)
                    {
                        MyModels.GetModelOnlyData(models[i]);
                    }
                }
            }
            MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition((byte) 0);
            BoundingSphereD sphere = new BoundingSphereD();
            Spawn(new MyPhysicalInventoryItem(1, MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(voxelMaterialDefinition.MinedOre), 1f), sphere, null, voxelMaterialDefinition, item => item.Close());
        }

        private void OptimizeCloseDistances()
        {
            this.UpdateClosestDistancesToPlayers();
            m_synchronizedFloatingObjects.Sort(m_synchronizationComparer);
            m_highPriority.Clear();
            m_normalPriority.Clear();
            m_lowPriority.Clear();
            m_needReupdateNewObjects = false;
            float num = 16f;
            float num2 = 256f;
            int num3 = 0x20;
            float num4 = 4096f;
            int num5 = 0x80;
            float num6 = 0.0025f;
            for (int i = 0; i < m_synchronizedFloatingObjects.Count; i++)
            {
                MyFloatingObject item = m_synchronizedFloatingObjects[i];
                m_needReupdateNewObjects |= (item.ClosestDistanceToAnyPlayerSquared == -1f) || ((item.ClosestDistanceToAnyPlayerSquared < num) && (item.SyncWaitCounter > 5L));
                float num8 = item.Physics.LinearVelocity.LengthSquared();
                float num9 = item.Physics.AngularVelocity.LengthSquared();
                if (((item.ClosestDistanceToAnyPlayerSquared == -1f) || (num8 > num6)) || (num9 > num6))
                {
                    if ((item.ClosestDistanceToAnyPlayerSquared < num2) && (i < num3))
                    {
                        m_highPriority.Add(item);
                    }
                    else if ((item.ClosestDistanceToAnyPlayerSquared >= num4) || (i >= num5))
                    {
                        m_lowPriority.Add(item);
                    }
                    else
                    {
                        m_normalPriority.Add(item);
                    }
                }
            }
        }

        private void OptimizeFloatingObjects()
        {
            ReduceFloatingObjects();
            this.OptimizeCloseDistances();
            this.OptimizeQualityType();
        }

        private void OptimizeQualityType()
        {
            for (int i = 0; i < m_synchronizedFloatingObjects.Count; i++)
            {
                m_synchronizedFloatingObjects[i].Physics.ChangeQualityType(HkCollidableQualityType.Critical);
            }
        }

        private static MyObjectBuilder_FloatingObject PrepareBuilder(ref MyPhysicalInventoryItem item)
        {
            MyObjectBuilder_FloatingObject local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_FloatingObject>();
            local1.Item = item.GetObjectBuilder();
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content);
            local1.ModelVariant = physicalItemDefinition.HasModelVariants ? MyUtils.GetRandomInt(physicalItemDefinition.Models.Length) : 0;
            MyObjectBuilder_FloatingObject local2 = local1;
            local2.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            return local2;
        }

        public static void ReduceFloatingObjects()
        {
            int num = m_floatingOres.Count + m_floatingItems.Count;
            int num2 = Math.Max(MySession.Static.MaxFloatingObjects / 5, 4);
            while (num > MySession.Static.MaxFloatingObjects)
            {
                SortedSet<MyFloatingObject> floatingOres;
                if ((m_floatingOres.Count > num2) || (m_floatingItems.Count == 0))
                {
                    floatingOres = m_floatingOres;
                }
                else
                {
                    floatingOres = m_floatingItems;
                }
                if (floatingOres.Count > 0)
                {
                    MyFloatingObject item = floatingOres.Last<MyFloatingObject>();
                    floatingOres.Remove(item);
                    if (Sync.IsServer)
                    {
                        RemoveFloatingObject(item);
                    }
                }
                num--;
            }
        }

        internal static void RegisterFloatingObject(MyFloatingObject obj)
        {
            if (!obj.WasRemovedFromWorld)
            {
                obj.CreationTime = Stopwatch.GetTimestamp();
                if (obj.VoxelMaterial != null)
                {
                    m_floatingOres.Add(obj);
                }
                else
                {
                    m_floatingItems.Add(obj);
                }
                if (Sync.IsServer)
                {
                    AddToSynchronization(obj);
                }
            }
        }

        public static void RemoveFloatingObject(MyFloatingObject obj)
        {
            RemoveFloatingObject(obj, MyFixedPoint.MaxValue);
        }

        public static void RemoveFloatingObject(MyFloatingObject obj, bool sync)
        {
            if (!sync)
            {
                RemoveFloatingObject(obj);
            }
            else if (Sync.IsServer)
            {
                RemoveFloatingObject(obj);
            }
            else
            {
                obj.SendCloseRequest();
            }
        }

        internal static void RemoveFloatingObject(MyFloatingObject obj, MyFixedPoint amount)
        {
            if (amount > 0)
            {
                if (amount < obj.Item.Amount)
                {
                    obj.Amount.Value -= amount;
                    obj.RefreshDisplayName();
                }
                else
                {
                    obj.Render.FadeOut = false;
                    obj.Close();
                    obj.WasRemovedFromWorld = true;
                }
            }
        }

        private static void RemoveFromSynchronization(MyFloatingObject floatingObject)
        {
            floatingObject.OnClose -= new Action<MyEntity>(MyFloatingObjects.floatingObject_OnClose);
            m_synchronizedFloatingObjects.Remove(floatingObject);
            m_floatingObjectsToSyncCreate.Remove(floatingObject);
            m_highPriority.Remove(floatingObject);
            m_normalPriority.Remove(floatingObject);
            m_lowPriority.Remove(floatingObject);
        }

        public static void RequestSpawnCreative(MyObjectBuilder_FloatingObject obj)
        {
            if (MySession.Static.HasCreativeRights || MySession.Static.CreativeMode)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyObjectBuilder_FloatingObject>(x => new Action<MyObjectBuilder_FloatingObject>(MyFloatingObjects.RequestSpawnCreative_Implementation), obj, targetEndpoint, position);
            }
        }

        [Event(null, 0x2f5), Reliable, Server]
        private static void RequestSpawnCreative_Implementation(MyObjectBuilder_FloatingObject obj)
        {
            if ((MySession.Static.CreativeMode || MyEventContext.Current.IsLocallyInvoked) || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                MyEntities.CreateFromObjectBuilderAndAdd(obj, false);
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        internal static MyEntity Spawn(MyPhysicalInventoryItem item, BoundingBoxD box, MyPhysicsComponentBase motionInheritedFrom = null)
        {
            MyEntity thrownEntity = MyEntities.CreateFromObjectBuilder(PrepareBuilder(ref item), false);
            if (thrownEntity != null)
            {
                float radius = thrownEntity.PositionComp.LocalVolume.Radius;
                Vector3D vectord = Vector3.Max(((Vector3) (box.Size / 2.0)) - new Vector3(radius), Vector3.Zero);
                box = new BoundingBoxD(box.Center - vectord, box.Center + vectord);
                Vector3D randomPosition = MyUtils.GetRandomPosition(ref box);
                AddToPos(thrownEntity, randomPosition, motionInheritedFrom);
                thrownEntity.Physics.ForceActivate();
                if (MyVisualScriptLogicProvider.ItemSpawned != null)
                {
                    MyVisualScriptLogicProvider.ItemSpawned(item.Content.TypeId.ToString(), item.Content.SubtypeName, thrownEntity.EntityId, item.Amount.ToIntSafe(), randomPosition);
                }
            }
            return thrownEntity;
        }

        public static void Spawn(MyPhysicalInventoryItem item, MatrixD worldMatrix, MyPhysicsComponentBase motionInheritedFrom, Action<MyEntity> completionCallback)
        {
            if (MyEntities.IsInsideWorld(worldMatrix.Translation))
            {
                MyObjectBuilder_FloatingObject objectBuilder = PrepareBuilder(ref item);
                objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix);
                Vector3D? relativeOffset = null;
                MyEntities.CreateFromObjectBuilderParallel(objectBuilder, true, delegate (MyEntity entity) {
                    if ((entity != null) && (entity.Physics != null))
                    {
                        entity.Physics.ForceActivate();
                        ApplyPhysics(entity, motionInheritedFrom);
                        if (MyVisualScriptLogicProvider.ItemSpawned != null)
                        {
                            MyVisualScriptLogicProvider.ItemSpawned(item.Content.TypeId.ToString(), item.Content.SubtypeName, entity.EntityId, item.Amount.ToIntSafe(), worldMatrix.Translation);
                        }
                        if (completionCallback != null)
                        {
                            completionCallback(entity);
                        }
                    }
                }, null, null, relativeOffset, false, false);
            }
        }

        public static void Spawn(MyPhysicalInventoryItem item, BoundingSphereD sphere, MyPhysicsComponentBase motionInheritedFrom, MyVoxelMaterialDefinition voxelMaterial, Action<MyEntity> OnDone)
        {
            Vector3D? relativeOffset = null;
            MyEntities.CreateFromObjectBuilderParallel(PrepareBuilder(ref item), false, delegate (MyEntity entity) {
                if (voxelMaterial.DamagedMaterial != MyStringHash.NullOrEmpty)
                {
                    voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(voxelMaterial.DamagedMaterial.ToString());
                }
                ((MyFloatingObject) entity).VoxelMaterial = voxelMaterial;
                float radius = entity.PositionComp.LocalVolume.Radius;
                double num2 = Math.Max((double) (sphere.Radius - radius), (double) 0.0);
                sphere = new BoundingSphereD(sphere.Center, num2);
                Vector3D randomBorderPosition = MyUtils.GetRandomBorderPosition(ref sphere);
                AddToPos(entity, randomBorderPosition, motionInheritedFrom);
                if (MyVisualScriptLogicProvider.ItemSpawned != null)
                {
                    MyVisualScriptLogicProvider.ItemSpawned(item.Content.TypeId.ToString(), item.Content.SubtypeName, entity.EntityId, item.Amount.ToIntSafe(), randomBorderPosition);
                }
                OnDone(entity);
            }, null, null, relativeOffset, false, false);
        }

        public static void Spawn(MyPhysicalItemDefinition itemDefinition, Vector3D translation, Vector3D forward, Vector3D up, int amount = 1, float scale = 1f)
        {
            MyObjectBuilder_PhysicalObject content = MyObjectBuilderSerializer.CreateNewObject(itemDefinition.Id.TypeId, itemDefinition.Id.SubtypeName) as MyObjectBuilder_PhysicalObject;
            Spawn(new MyPhysicalInventoryItem(amount, content, scale), translation, forward, up, null, null);
        }

        public static void Spawn(MyPhysicalInventoryItem item, Vector3D position, Vector3D forward, Vector3D up, MyPhysicsComponentBase motionInheritedFrom = null, Action<MyEntity> completionCallback = null)
        {
            if (MyEntities.IsInsideWorld(position))
            {
                Vector3D vectord = forward;
                Vector3D vectord2 = up;
                Vector3D vectord3 = Vector3D.Cross(up, forward);
                MyPhysicalItemDefinition definition = null;
                if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalItemDefinition>(item.Content.GetObjectId(), out definition))
                {
                    if (definition.RotateOnSpawnX)
                    {
                        vectord = up;
                        vectord2 = -forward;
                    }
                    if (definition.RotateOnSpawnY)
                    {
                        vectord = vectord3;
                    }
                    if (definition.RotateOnSpawnZ)
                    {
                        vectord2 = -vectord3;
                    }
                }
                Spawn(item, MatrixD.CreateWorld(position, vectord, vectord2), motionInheritedFrom, completionCallback);
            }
        }

        private void SpawnInventoryItems(List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>> itemsList)
        {
            for (int i = 0; i < itemsList.Count; i++)
            {
                Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D> item = itemsList[0];
                itemsList.RemoveAt(0);
                item.Item1.Spawn(item.Item1.Amount, item.Item2, null, delegate (MyEntity entity) {
                    entity.Physics.LinearVelocity = item.Item3;
                    entity.Physics.ApplyImpulse((MyUtils.GetRandomVector3Normalized() * entity.Physics.Mass) / 5f, entity.PositionComp.GetPosition());
                });
            }
        }

        internal static void UnregisterFloatingObject(MyFloatingObject obj)
        {
            if (obj.VoxelMaterial != null)
            {
                m_floatingOres.Remove(obj);
            }
            else
            {
                m_floatingItems.Remove(obj);
            }
            if (Sync.IsServer)
            {
                RemoveFromSynchronization(obj);
            }
            obj.WasRemovedFromWorld = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (Sync.IsServer)
            {
                this.CheckObjectInVoxel();
                m_updateCounter++;
                if (m_updateCounter > 100)
                {
                    ReduceFloatingObjects();
                }
                if (m_itemsToSpawnNextUpdate.Count > 0)
                {
                    List<Tuple<MyPhysicalInventoryItem, BoundingBoxD, Vector3D>> tmp = m_itemsToSpawnNextUpdate;
                    m_itemsToSpawnNextUpdate = m_itemsToSpawnPool.Get();
                    if (MySandboxGame.Config.SyncRendering)
                    {
                        MyEntityIdentifier.PrepareSwapData();
                        MyEntityIdentifier.SwapPerThreadData();
                    }
                    Parallel.Start(delegate {
                        this.SpawnInventoryItems(tmp);
                        tmp.Clear();
                        m_itemsToSpawnPool.Return(tmp);
                    });
                    if (MySandboxGame.Config.SyncRendering)
                    {
                        MyEntityIdentifier.ClearSwapDataAndRestore();
                    }
                }
                base.UpdateAfterSimulation();
                if (m_updateCounter > 100)
                {
                    this.OptimizeFloatingObjects();
                }
                else
                {
                    if (m_needReupdateNewObjects)
                    {
                        this.OptimizeCloseDistances();
                    }
                    this.OptimizeQualityType();
                }
                if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
                {
                    this.UpdateObjectCounters();
                }
                if (m_updateCounter > 100)
                {
                    m_updateCounter = 0;
                }
            }
        }

        private void UpdateClosestDistancesToPlayers()
        {
            foreach (MyFloatingObject obj2 in m_synchronizedFloatingObjects)
            {
                if (obj2.ClosestDistanceToAnyPlayerSquared == -1f)
                {
                    continue;
                }
                obj2.ClosestDistanceToAnyPlayerSquared = float.MaxValue;
                IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        MyPlayer current = enumerator.Current;
                        if (current.Identity.Character != null)
                        {
                            float num = (float) Vector3D.DistanceSquared(obj2.PositionComp.GetPosition(), current.Identity.Character.PositionComp.GetPosition());
                            if (num < obj2.ClosestDistanceToAnyPlayerSquared)
                            {
                                obj2.ClosestDistanceToAnyPlayerSquared = num;
                            }
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
        }

        private void UpdateObjectCounters()
        {
            MyPerformanceCounter.PerCameraDrawRead.CustomCounters["Floating Ores"] = FloatingOreCount;
            MyPerformanceCounter.PerCameraDrawRead.CustomCounters["Floating Items"] = FloatingItemCount;
            MyPerformanceCounter.PerCameraDrawWrite.CustomCounters["Floating Ores"] = FloatingOreCount;
            MyPerformanceCounter.PerCameraDrawWrite.CustomCounters["Floating Items"] = FloatingItemCount;
        }

        public override System.Type[] Dependencies =>
            new System.Type[] { typeof(MyDebris) };

        public static int FloatingOreCount =>
            m_floatingOres.Count;

        public static int FloatingItemCount =>
            m_floatingItems.Count;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFloatingObjects.<>c <>9 = new MyFloatingObjects.<>c();
            public static Action<MyEntity> <>9__20_0;
            public static Func<IMyEventOwner, Action<MyObjectBuilder_FloatingObject>> <>9__55_0;

            internal void <LoadData>b__20_0(MyEntity item)
            {
                item.Close();
            }

            internal Action<MyObjectBuilder_FloatingObject> <RequestSpawnCreative>b__55_0(IMyEventOwner x) => 
                new Action<MyObjectBuilder_FloatingObject>(MyFloatingObjects.RequestSpawnCreative_Implementation);
        }

        private class MyFloatingObjectComparer : IEqualityComparer<MyFloatingObject>
        {
            public bool Equals(MyFloatingObject x, MyFloatingObject y) => 
                (x.EntityId == y.EntityId);

            public int GetHashCode(MyFloatingObject obj) => 
                ((int) obj.EntityId);
        }

        private class MyFloatingObjectsSynchronizationComparer : IComparer<MyFloatingObject>
        {
            public int Compare(MyFloatingObject x, MyFloatingObject y) => 
                x.ClosestDistanceToAnyPlayerSquared.CompareTo(y.ClosestDistanceToAnyPlayerSquared);
        }

        private class MyFloatingObjectTimestampComparer : IComparer<MyFloatingObject>
        {
            public int Compare(MyFloatingObject x, MyFloatingObject y)
            {
                if (x.CreationTime != y.CreationTime)
                {
                    return y.CreationTime.CompareTo(x.CreationTime);
                }
                return y.EntityId.CompareTo(x.EntityId);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StabilityInfo
        {
            public MyPositionAndOrientation PositionAndOr;
            public StabilityInfo(MyPositionAndOrientation posAndOr)
            {
                this.PositionAndOr = posAndOr;
            }
        }
    }
}

