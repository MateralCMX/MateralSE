namespace Sandbox.Game.World
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyPrefabManager : IMyPrefabManager
    {
        private static FastResourceLock m_builderLock = new FastResourceLock();
        public static EventWaitHandle FinishedProcessingGrids = new AutoResetEvent(false);
        public static int PendingGrids;
        public static readonly MyPrefabManager Static = new MyPrefabManager();
        private static List<MyPhysics.HitInfo> m_raycastHits = new List<MyPhysics.HitInfo>();

        public void AddShipPrefab(string prefabName, Matrix? worldMatrix = new Matrix?(), long ownerId = 0L, bool spawnAtOrigin = false)
        {
            Matrix? nullable = worldMatrix;
            long num = ownerId;
            CreateGridsData workData = new CreateGridsData(new List<MyCubeGrid>(), prefabName, (nullable != null) ? ((MatrixD) nullable.GetValueOrDefault()) : Matrix.Identity, spawnAtOrigin ? SpawningOptions.UseGridOrigin : SpawningOptions.None, true, num, null);
            Interlocked.Increment(ref PendingGrids);
            Parallel.Start(new Action<WorkData>(workData.CallCreateGridsFromPrefab), new Action<WorkData>(workData.OnGridsCreated), workData);
        }

        public void AddShipPrefabRandomPosition(string prefabName, Vector3D position, float distance, long ownerId = 0L, bool spawnAtOrigin = false)
        {
            MyPrefabDefinition prefabDefinition = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
            if (prefabDefinition != null)
            {
                BoundingSphereD sphere = new BoundingSphereD(Vector3D.Zero, (double) prefabDefinition.BoundingSphere.Radius);
                int num = 0;
                while (true)
                {
                    Vector3 vector = ((Vector3) position) + ((MyUtils.GetRandomVector3Normalized() * MyUtils.GetRandomFloat(0.5f, 1f)) * distance);
                    sphere.Center = vector;
                    VRage.Game.Entity.MyEntity intersectionWithSphere = Sandbox.Game.Entities.MyEntities.GetIntersectionWithSphere(ref sphere);
                    num++;
                    if ((num % 8) == 0)
                    {
                        distance += ((float) sphere.Radius) / 2f;
                    }
                    if (intersectionWithSphere == null)
                    {
                        long num2 = ownerId;
                        CreateGridsData workData = new CreateGridsData(new List<MyCubeGrid>(), prefabName, Matrix.CreateWorld(vector, Vector3.Forward, Vector3.Up), spawnAtOrigin ? SpawningOptions.UseGridOrigin : SpawningOptions.None, true, num2, null);
                        Interlocked.Increment(ref PendingGrids);
                        Parallel.Start(new Action<WorkData>(workData.CallCreateGridsFromPrefab), new Action<WorkData>(workData.OnGridsCreated), workData);
                        return;
                    }
                }
            }
        }

        private void CreateGridsFromPrefab(List<MyCubeGrid> results, string prefabName, MatrixD worldMatrix, SpawningOptions spawningOptions, bool ignoreMemoryLimits, long ownerId, Stack<Action> callbacks)
        {
            MyPrefabDefinition prefabDefinition = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
            if (prefabDefinition != null)
            {
                MyObjectBuilder_CubeGrid[] objectBuilders = new MyObjectBuilder_CubeGrid[prefabDefinition.CubeGrids.Length];
                if (objectBuilders.Length != 0)
                {
                    MatrixD xd;
                    for (int i = 0; i < objectBuilders.Length; i++)
                    {
                        objectBuilders[i] = (MyObjectBuilder_CubeGrid) prefabDefinition.CubeGrids[i].Clone();
                    }
                    Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection(objectBuilders);
                    if (!spawningOptions.HasFlag(SpawningOptions.UseGridOrigin))
                    {
                        xd = MatrixD.CreateWorld(-prefabDefinition.BoundingSphere.Center, Vector3D.Forward, Vector3D.Up);
                    }
                    else
                    {
                        Vector3D zero = Vector3D.Zero;
                        if (prefabDefinition.CubeGrids[0].PositionAndOrientation != null)
                        {
                            zero = (Vector3D) prefabDefinition.CubeGrids[0].PositionAndOrientation.Value.Position;
                        }
                        xd = MatrixD.CreateWorld(-zero, Vector3D.Forward, Vector3D.Up);
                    }
                    bool flag = Sandbox.Game.Entities.MyEntities.IgnoreMemoryLimits;
                    Sandbox.Game.Entities.MyEntities.IgnoreMemoryLimits = ignoreMemoryLimits;
                    bool flag2 = spawningOptions.HasFlag(SpawningOptions.SetAuthorship);
                    for (int j = 0; j < objectBuilders.Length; j++)
                    {
                        MatrixD identity;
                        if (ownerId != 0)
                        {
                            foreach (MyObjectBuilder_CubeBlock block in objectBuilders[j].CubeBlocks)
                            {
                                block.Owner = ownerId;
                                block.ShareMode = MyOwnershipShareModeEnum.Faction;
                                if (flag2)
                                {
                                    block.BuiltBy = ownerId;
                                }
                            }
                        }
                        if (objectBuilders[j].PositionAndOrientation == null)
                        {
                            identity = MatrixD.Identity;
                        }
                        else
                        {
                            identity = objectBuilders[j].PositionAndOrientation.Value.GetMatrix();
                        }
                        MatrixD newWorldMatrix = MatrixD.Multiply(identity, MatrixD.Multiply(xd, worldMatrix));
                        objectBuilders[j].PositionAndOrientation = new MyPositionAndOrientation(newWorldMatrix);
                        VRage.Game.Entity.MyEntity entity = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(objectBuilders[j], false);
                        MyCubeGrid item = entity as MyCubeGrid;
                        if ((item != null) && (item.CubeBlocks.Count > 0))
                        {
                            results.Add(item);
                            callbacks.Push(() => this.SetPrefabPosition(entity, newWorldMatrix));
                        }
                    }
                    Sandbox.Game.Entities.MyEntities.IgnoreMemoryLimits = flag;
                }
            }
        }

        public MyObjectBuilder_CubeGrid[] GetGridPrefab(string prefabName)
        {
            MyPrefabDefinition prefabDefinition = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);
            if (prefabDefinition == null)
            {
                return null;
            }
            MyObjectBuilder_CubeGrid[] cubeGrids = prefabDefinition.CubeGrids;
            Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection(cubeGrids);
            return cubeGrids;
        }

        public static MyObjectBuilder_PrefabDefinition SavePrefab(string prefabName, List<MyObjectBuilder_CubeGrid> copiedPrefab)
        {
            string path = Path.Combine(MyFileSystem.ContentPath, Path.Combine("Data", "Prefabs", prefabName + ".sbc"));
            return SavePrefabToPath(prefabName, path, copiedPrefab);
        }

        public static void SavePrefab(string prefabName, MyObjectBuilder_EntityBase entity)
        {
            MyObjectBuilder_PrefabDefinition definition = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PrefabDefinition>();
            definition.Id = (SerializableDefinitionId) new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_PrefabDefinition)), prefabName);
            definition.CubeGrid = (MyObjectBuilder_CubeGrid) entity;
            MyObjectBuilder_Definitions objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
            objectBuilder.Prefabs = new MyObjectBuilder_PrefabDefinition[] { definition };
            MyObjectBuilderSerializer.SerializeXML(Path.Combine(MyFileSystem.ContentPath, Path.Combine("Data", "Prefabs", prefabName + ".sbc")), false, objectBuilder, null);
        }

        public static MyObjectBuilder_PrefabDefinition SavePrefabToPath(string prefabName, string path, List<MyObjectBuilder_CubeGrid> copiedPrefab)
        {
            MyObjectBuilder_PrefabDefinition definition = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PrefabDefinition>();
            definition.Id = (SerializableDefinitionId) new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_PrefabDefinition)), prefabName);
            definition.CubeGrids = (from x in copiedPrefab select (MyObjectBuilder_CubeGrid) x.Clone()).ToArray<MyObjectBuilder_CubeGrid>();
            MyObjectBuilder_CubeGrid[] cubeGrids = definition.CubeGrids;
            for (int i = 0; i < cubeGrids.Length; i++)
            {
                foreach (MyObjectBuilder_CubeBlock local2 in cubeGrids[i].CubeBlocks)
                {
                    local2.Owner = 0L;
                    local2.BuiltBy = 0L;
                }
            }
            MyObjectBuilder_Definitions objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
            objectBuilder.Prefabs = new MyObjectBuilder_PrefabDefinition[] { definition };
            MyObjectBuilderSerializer.SerializeXML(path, false, objectBuilder, null);
            return definition;
        }

        private void SetPrefabPosition(VRage.Game.Entity.MyEntity entity, MatrixD newWorldMatrix)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                grid.PositionComp.SetWorldMatrix(newWorldMatrix, null, true, true, true, false, false, false);
                if ((MyPerGameSettings.Destruction && (grid.IsStatic && (grid.Physics != null))) && (grid.Physics.Shape != null))
                {
                    grid.Physics.Shape.RecalculateConnectionsToWorld(grid.GetBlocks());
                }
            }
        }

        public void SpawnPrefab(string prefabName, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity = new Vector3(), Vector3 initialAngularVelocity = new Vector3(), string beaconName = null, string entityName = null, SpawningOptions spawningOptions = 0, long ownerId = 0L, bool updateSync = false, Stack<Action> callbacks = null)
        {
            if (callbacks == null)
            {
                callbacks = new Stack<Action>();
            }
            this.SpawnPrefabInternal(new List<MyCubeGrid>(), prefabName, position, forward, up, initialLinearVelocity, initialAngularVelocity, beaconName, entityName, spawningOptions, ownerId, updateSync, callbacks);
        }

        public void SpawnPrefab(List<MyCubeGrid> resultList, string prefabName, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity = new Vector3(), Vector3 initialAngularVelocity = new Vector3(), string beaconName = null, string entityName = null, SpawningOptions spawningOptions = 0, long ownerId = 0L, bool updateSync = false, Stack<Action> callbacks = null)
        {
            if (callbacks == null)
            {
                callbacks = new Stack<Action>();
            }
            this.SpawnPrefabInternal(resultList, prefabName, position, forward, up, initialLinearVelocity, initialAngularVelocity, beaconName, entityName, spawningOptions, ownerId, updateSync, callbacks);
        }

        private void SpawnPrefabInternal(List<MyCubeGrid> resultList, string prefabName, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity, Vector3 initialAngularVelocity, string beaconName, string entityName, SpawningOptions spawningOptions, long ownerId, bool updateSync, Stack<Action> callbacks)
        {
            CreateGridsData workData = new CreateGridsData(resultList, prefabName, MatrixD.CreateWorld(position, forward, up), spawningOptions, true, ownerId, callbacks);
            Interlocked.Increment(ref PendingGrids);
            callbacks.Push(() => this.SpawnPrefabInternalSetProperties(resultList, position, forward, up, initialLinearVelocity, initialAngularVelocity, beaconName, entityName, spawningOptions, updateSync));
            if (MySandboxGame.Config.SyncRendering)
            {
                MyEntityIdentifier.PrepareSwapData();
                MyEntityIdentifier.SwapPerThreadData();
            }
            Parallel.Start(new Action<WorkData>(workData.CallCreateGridsFromPrefab), new Action<WorkData>(workData.OnGridsCreated), workData);
            if (MySandboxGame.Config.SyncRendering)
            {
                MyEntityIdentifier.ClearSwapDataAndRestore();
            }
        }

        private void SpawnPrefabInternalSetProperties(List<MyCubeGrid> resultList, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity, Vector3 initialAngularVelocity, string beaconName, string entityName, SpawningOptions spawningOptions, bool updateSync)
        {
            MyRandom.StateToken token1;
            int num = 0;
            if (updateSync)
            {
                token1 = MyRandom.Instance.PushSeed(num = MyRandom.Instance.CreateRandomSeed());
            }
            else
            {
                token1 = new MyRandom.StateToken();
            }
            using (token1)
            {
                int num1;
                bool flag = spawningOptions.HasFlag(SpawningOptions.RotateFirstCockpitTowardsDirection);
                bool flag2 = spawningOptions.HasFlag(SpawningOptions.SpawnRandomCargo);
                bool flag3 = spawningOptions.HasFlag(SpawningOptions.SetNeutralOwner);
                if (((flag2 | flag) | flag3) || (beaconName != null))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) !string.IsNullOrEmpty(entityName);
                }
                bool flag4 = (bool) num1;
                List<MyCockpit> list = new List<MyCockpit>();
                List<MyRemoteControl> list2 = new List<MyRemoteControl>();
                bool flag5 = spawningOptions.HasFlag(SpawningOptions.TurnOffReactors);
                foreach (MyCubeGrid grid in resultList)
                {
                    grid.ClearSymmetries();
                    if (spawningOptions.HasFlag(SpawningOptions.DisableDampeners))
                    {
                        MyEntityThrustComponent component = grid.Components.Get<MyEntityThrustComponent>();
                        if (component != null)
                        {
                            component.DampenersEnabled = false;
                        }
                    }
                    if (spawningOptions.HasFlag(SpawningOptions.DisableSave))
                    {
                        grid.Save = false;
                    }
                    if (flag4 | flag5)
                    {
                        bool flag6 = false;
                        foreach (MySlimBlock block2 in grid.GetBlocks())
                        {
                            if ((block2.FatBlock is MyCockpit) && block2.FatBlock.IsFunctional)
                            {
                                list.Add((MyCockpit) block2.FatBlock);
                                continue;
                            }
                            if ((block2.FatBlock is MyCargoContainer) & flag2)
                            {
                                (block2.FatBlock as MyCargoContainer).SpawnRandomCargo();
                            }
                            else if ((block2.FatBlock is MyBeacon) && (beaconName != null))
                            {
                                (block2.FatBlock as MyBeacon).SetCustomName(beaconName);
                            }
                            else if ((!flag5 || (block2.FatBlock == null)) || !block2.FatBlock.Components.Contains(typeof(MyResourceSourceComponent)))
                            {
                                if (block2.FatBlock is MyRemoteControl)
                                {
                                    list2.Add(block2.FatBlock as MyRemoteControl);
                                    flag6 = true;
                                }
                            }
                            else
                            {
                                MyResourceSourceComponent component2 = block2.FatBlock.Components.Get<MyResourceSourceComponent>();
                                if ((component2 != null) && component2.ResourceTypes.Contains<MyDefinitionId>(MyResourceDistributorComponent.ElectricityId))
                                {
                                    component2.Enabled = false;
                                }
                            }
                        }
                        if (flag6 && !string.IsNullOrEmpty(entityName))
                        {
                            grid.Name = entityName;
                            Sandbox.Game.Entities.MyEntities.SetEntityName(grid, false);
                        }
                    }
                }
                if (list.Count > 1)
                {
                    list.Sort(delegate (MyCockpit cockpitA, MyCockpit cockpitB) {
                        int num = cockpitB.IsMainCockpit.CompareTo(cockpitA.IsMainCockpit);
                        if (num != 0)
                        {
                            return num;
                        }
                        return cockpitB.EnableShipControl.CompareTo(cockpitA.EnableShipControl);
                    });
                }
                MyCubeBlock block = null;
                if ((list.Count > 0) && (list[0].EnableShipControl || (list2.Count <= 0)))
                {
                    block = list[0];
                }
                if ((block == null) && (list2.Count > 0))
                {
                    if (list2.Count > 1)
                    {
                        list2.Sort((remoteA, remoteB) => remoteB.IsMainRemoteControl.CompareTo(remoteA.IsMainCockpit));
                    }
                    block = list2[0];
                }
                Matrix identity = Matrix.Identity;
                if (flag && (block != null))
                {
                    identity = Matrix.Multiply(Matrix.Invert((Matrix) block.WorldMatrix), Matrix.CreateWorld((Vector3) block.WorldMatrix.Translation, forward, up));
                }
                foreach (MyCubeGrid grid2 in resultList)
                {
                    if ((block != null) & flag)
                    {
                        grid2.WorldMatrix *= identity;
                    }
                    if (grid2.Physics != null)
                    {
                        grid2.Physics.LinearVelocity = initialLinearVelocity;
                        grid2.Physics.AngularVelocity = initialAngularVelocity;
                    }
                    SingleKeyEntityNameEvent prefabSpawned = MyVisualScriptLogicProvider.PrefabSpawned;
                    if (prefabSpawned != null)
                    {
                        prefabSpawned(grid2.Name);
                    }
                }
            }
        }

        bool IMyPrefabManager.IsPathClear(Vector3D from, Vector3D to)
        {
            MyPhysics.CastRay(from, to, m_raycastHits, 0x18);
            m_raycastHits.Clear();
            return (m_raycastHits.Count == 0);
        }

        bool IMyPrefabManager.IsPathClear(Vector3D from, Vector3D to, double halfSize)
        {
            Vector3D vectord = new Vector3D {
                X = 1.0
            };
            Vector3D vectord2 = to - from;
            vectord2.Normalize();
            if ((Vector3D.Dot(vectord2, vectord) > 0.89999997615814209) || (Vector3D.Dot(vectord2, vectord) < -0.89999997615814209))
            {
                vectord.X = 0.0;
                vectord.Y = 1.0;
            }
            vectord = Vector3D.Cross(vectord2, vectord);
            vectord.Normalize();
            vectord *= halfSize;
            MyPhysics.CastRay(from + vectord, to + vectord, m_raycastHits, 0x18);
            if (m_raycastHits.Count > 0)
            {
                m_raycastHits.Clear();
                return false;
            }
            vectord *= -1.0;
            MyPhysics.CastRay(from + vectord, to + vectord, m_raycastHits, 0x18);
            if (m_raycastHits.Count > 0)
            {
                m_raycastHits.Clear();
                return false;
            }
            vectord = Vector3D.Cross(vectord2, vectord);
            MyPhysics.CastRay(from + vectord, to + vectord, m_raycastHits, 0x18);
            if (m_raycastHits.Count > 0)
            {
                m_raycastHits.Clear();
                return false;
            }
            vectord *= -1.0;
            MyPhysics.CastRay(from + vectord, to + vectord, m_raycastHits, 0x18);
            if (m_raycastHits.Count <= 0)
            {
                return true;
            }
            m_raycastHits.Clear();
            return false;
        }

        void IMyPrefabManager.SpawnPrefab(List<IMyCubeGrid> resultList, string prefabName, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity, Vector3 initialAngularVelocity, string beaconName, SpawningOptions spawningOptions, bool updateSync, Action callback)
        {
            ((IMyPrefabManager) this).SpawnPrefab(resultList, prefabName, position, forward, up, initialAngularVelocity, initialAngularVelocity, beaconName, spawningOptions, 0L, updateSync, callback);
        }

        void IMyPrefabManager.SpawnPrefab(List<IMyCubeGrid> resultList, string prefabName, Vector3D position, Vector3 forward, Vector3 up, Vector3 initialLinearVelocity, Vector3 initialAngularVelocity, string beaconName, SpawningOptions spawningOptions, long ownerId, bool updateSync, Action callback)
        {
            Stack<Action> callbacks = new Stack<Action>();
            if (callback != null)
            {
                callbacks.Push(callback);
            }
            List<MyCubeGrid> results = new List<MyCubeGrid>();
            this.SpawnPrefab(results, prefabName, position, forward, up, initialLinearVelocity, initialAngularVelocity, beaconName, null, spawningOptions, ownerId, updateSync, callbacks);
            callbacks.Push(delegate {
                foreach (MyCubeGrid grid in results)
                {
                    resultList.Add(grid);
                }
            });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPrefabManager.<>c <>9 = new MyPrefabManager.<>c();
            public static Func<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeGrid> <>9__7_0;
            public static Comparison<MyCockpit> <>9__19_0;
            public static Comparison<MyRemoteControl> <>9__19_1;

            internal MyObjectBuilder_CubeGrid <SavePrefabToPath>b__7_0(MyObjectBuilder_CubeGrid x) => 
                ((MyObjectBuilder_CubeGrid) x.Clone());

            internal int <SpawnPrefabInternalSetProperties>b__19_0(MyCockpit cockpitA, MyCockpit cockpitB)
            {
                int num = cockpitB.IsMainCockpit.CompareTo(cockpitA.IsMainCockpit);
                if (num != 0)
                {
                    return num;
                }
                return cockpitB.EnableShipControl.CompareTo(cockpitA.EnableShipControl);
            }

            internal int <SpawnPrefabInternalSetProperties>b__19_1(MyRemoteControl remoteA, MyRemoteControl remoteB) => 
                remoteB.IsMainRemoteControl.CompareTo(remoteA.IsMainCockpit);
        }

        public class CreateGridsData : WorkData
        {
            private List<MyCubeGrid> m_results;
            private string m_prefabName;
            private MatrixD m_worldMatrix;
            private SpawningOptions m_spawnOptions;
            private bool m_ignoreMemoryLimits;
            private long m_ownerId;
            private Stack<Action> m_callbacks;
            private List<IMyEntity> m_resultIDs;

            public CreateGridsData(List<MyCubeGrid> results, string prefabName, MatrixD worldMatrix, SpawningOptions spawnOptions, bool ignoreMemoryLimits = true, long ownerId = 0L, Stack<Action> callbacks = null)
            {
                this.m_results = results;
                this.m_ownerId = ownerId;
                this.m_prefabName = prefabName;
                this.m_worldMatrix = worldMatrix;
                this.m_spawnOptions = spawnOptions;
                this.m_ignoreMemoryLimits = ignoreMemoryLimits;
                this.m_callbacks = callbacks ?? new Stack<Action>();
                if (spawnOptions.HasFlag(SpawningOptions.SetNeutralOwner))
                {
                    string name = "NPC " + MyRandom.Instance.Next(0x3e8, 0x270f);
                    Vector3? colorMask = null;
                    MyIdentity identity = Sync.Players.CreateNewIdentity(name, null, colorMask, false);
                    this.m_ownerId = identity.IdentityId;
                }
            }

            public void CallCreateGridsFromPrefab(WorkData workData)
            {
                try
                {
                    MyEntityIdentifier.InEntityCreationBlock = true;
                    MyEntityIdentifier.LazyInitPerThreadStorage(0x800);
                    MyPrefabManager.Static.CreateGridsFromPrefab(this.m_results, this.m_prefabName, this.m_worldMatrix, this.m_spawnOptions, this.m_ignoreMemoryLimits, this.m_ownerId, this.m_callbacks);
                }
                finally
                {
                    this.m_resultIDs = new List<IMyEntity>();
                    MyEntityIdentifier.GetPerThreadEntities(this.m_resultIDs);
                    MyEntityIdentifier.ClearPerThreadEntities();
                    MyEntityIdentifier.InEntityCreationBlock = false;
                    Interlocked.Decrement(ref MyPrefabManager.PendingGrids);
                    if (MyPrefabManager.PendingGrids <= 0)
                    {
                        MyPrefabManager.FinishedProcessingGrids.Set();
                    }
                }
            }

            public void OnGridsCreated(WorkData workData)
            {
                foreach (IMyEntity entity in this.m_resultIDs)
                {
                    IMyEntity entity2;
                    MyEntityIdentifier.TryGetEntity(entity.EntityId, out entity2, false);
                    if (entity2 == null)
                    {
                        MyEntityIdentifier.AddEntityWithId(entity);
                    }
                }
                using (List<MyCubeGrid>.Enumerator enumerator2 = this.m_results.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Sandbox.Game.Entities.MyEntities.Add(enumerator2.Current, true);
                    }
                }
                if (this.m_callbacks != null)
                {
                    while (this.m_callbacks.Count > 0)
                    {
                        this.m_callbacks.Pop().InvokeIfNotNull();
                    }
                }
            }
        }
    }
}

