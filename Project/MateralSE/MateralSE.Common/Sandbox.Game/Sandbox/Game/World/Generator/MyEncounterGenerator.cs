namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500, typeof(MyObjectBuilder_Encounters), (Type) null)]
    internal class MyEncounterGenerator : MySessionComponentBase
    {
        private const double MIN_DISTANCE_TO_RECOGNIZE_MOVEMENT = 1000.0;
        private HashSet<MyEncounterId> m_persistentEncounters = new HashSet<MyEncounterId>();
        private HashSet<MyEncounterId> m_encounterSpawnInProgress = new HashSet<MyEncounterId>();
        private HashSet<MyEncounterId> m_encounterRemoveRequested = new HashSet<MyEncounterId>();
        private Dictionary<VRage.Game.Entity.MyEntity, MyEncounterId> m_entityToEncounterId = new Dictionary<VRage.Game.Entity.MyEntity, MyEncounterId>();
        private Dictionary<MyEncounterId, List<VRage.Game.Entity.MyEntity>> m_encounterEntities = new Dictionary<MyEncounterId, List<VRage.Game.Entity.MyEntity>>();
        private MyRandom m_random = new MyRandom();
        private MySpawnGroupDefinition[] m_encounterSpawnGroups;
        private MyConcurrentPool<List<VRage.Game.Entity.MyEntity>> m_entityListsPool;
        private static List<float> m_spawnGroupCumulativeFrequencies;
        private readonly MyVoxelBase.StorageChanged m_OnVoxelChanged;
        private readonly Action<MySlimBlock> m_OnBlockChanged;
        private readonly Action<MyCubeGrid> m_OnGridChanged;
        private readonly Action<VRage.Game.Entity.MyEntity> m_OnEntityClosed;
        private readonly Action<MyPositionComponentBase> m_OnEntityPositionChanged;

        public MyEncounterGenerator()
        {
            this.m_entityListsPool = new MyConcurrentPool<List<VRage.Game.Entity.MyEntity>>(10, x => x.Clear(), 0x3e8, () => new List<VRage.Game.Entity.MyEntity>(10));
            this.m_OnGridChanged = new Action<MyCubeGrid>(this.OnGridChanged);
            this.m_OnBlockChanged = new Action<MySlimBlock>(this.OnBlockChanged);
            this.m_OnEntityClosed = new Action<VRage.Game.Entity.MyEntity>(this.OnEntityClosed);
            this.m_OnVoxelChanged = new MyVoxelBase.StorageChanged(this.OnVoxelChanged);
            this.m_OnEntityPositionChanged = new Action<MyPositionComponentBase>(this.OnEntityPositionChanged);
        }

        private static void AssertNotClosed(VRage.Game.Entity.MyEntity entity)
        {
            bool markedForClose = entity.MarkedForClose;
        }

        public void DebugDraw()
        {
            BoundingBoxD boundingBox;
            Vector3D position = MySector.MainCamera.Position;
            foreach (MyEncounterId id in this.m_encounterEntities.Keys)
            {
                MyRenderProxy.DebugDrawAABB(id.BoundingBox, Color.Blue, 1f, 1f, true, false, false);
                boundingBox = id.BoundingBox;
                Vector3D center = boundingBox.Center;
                if (Vector3D.Distance(position, center) < 500.0)
                {
                    MyRenderProxy.DebugDrawText3D(center, id.ToString(), Color.Blue, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
            foreach (MyEncounterId id2 in this.m_persistentEncounters)
            {
                MyRenderProxy.DebugDrawAABB(id2.BoundingBox, Color.Red, 1f, 1f, true, false, false);
                boundingBox = id2.BoundingBox;
                Vector3D center = boundingBox.Center;
                if (Vector3D.Distance(position, center) < 500.0)
                {
                    MyRenderProxy.DebugDrawText3D(center, id2.ToString(), Color.Red, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_Encounters objectBuilder = (MyObjectBuilder_Encounters) base.GetObjectBuilder();
            objectBuilder.SavedEncounters = new HashSet<MyEncounterId>(this.m_persistentEncounters);
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_Encounters encounters = (MyObjectBuilder_Encounters) objectBuilder;
            if (encounters.SavedEncounters != null)
            {
                this.m_persistentEncounters = encounters.SavedEncounters;
            }
        }

        private static bool IsAcceptableEncounter(BoundingBoxD boundingBox)
        {
            Vector3D center = boundingBox.Center;
            MyWorldGeneratorStartingStateBase[] possiblePlayerStarts = MySession.Static.Scenario.PossiblePlayerStarts;
            for (int i = 0; i < possiblePlayerStarts.Length; i++)
            {
                Vector3D? startingLocation = possiblePlayerStarts[i].GetStartingLocation();
                if (Vector3D.DistanceSquared(center, (startingLocation != null) ? startingLocation.GetValueOrDefault() : Vector3D.Zero) < 225000000.0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsEncounter(VRage.Game.Entity.MyEntity entity) => 
            this.m_entityToEncounterId.ContainsKey(entity);

        public override void LoadData()
        {
            Static = this;
            this.m_encounterSpawnGroups = (from x in MyDefinitionManager.Static.GetSpawnGroupDefinitions()
                where x.IsEncounter
                select x).ToArray<MySpawnGroupDefinition>();
        }

        private void OnBlockChanged(MySlimBlock block)
        {
            AssertNotClosed(block.CubeGrid);
            this.PersistEncounter(block.CubeGrid);
        }

        private void OnEntityClosed(VRage.Game.Entity.MyEntity entity)
        {
            this.PersistEncounter(entity);
        }

        private void OnEntityPositionChanged(MyPositionComponentBase obj)
        {
            MyEncounterId id;
            VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) obj.Container.Entity;
            AssertNotClosed(entity);
            if (this.m_entityToEncounterId.TryGetValue(entity, out id))
            {
                Vector3D position = obj.GetPosition();
                if (Vector3D.Distance(id.BoundingBox.Center, position) > 1000.0)
                {
                    this.PersistEncounter(entity);
                }
            }
        }

        private void OnGridChanged(MyCubeGrid grid)
        {
            AssertNotClosed(grid);
            Static.PersistEncounter(grid);
        }

        private void OnVoxelChanged(MyVoxelBase voxel, Vector3I minvoxelchanged, Vector3I maxvoxelchanged, MyStorageDataTypeFlags changeddata)
        {
            this.PersistEncounter(voxel);
        }

        private void OpenEncounter(MyEncounterId id)
        {
            List<VRage.Game.Entity.MyEntity> list;
            if (!this.m_encounterEntities.TryGetValue(id, out list))
            {
                list = this.m_entityListsPool.Get();
                this.m_encounterEntities.Add(id, list);
            }
        }

        private void PersistEncounter(VRage.Game.Entity.MyEntity encounterEntity)
        {
            MyEncounterId id;
            if (this.m_entityToEncounterId.TryGetValue(encounterEntity, out id))
            {
                this.PersistEncounter(id);
            }
        }

        private void PersistEncounter(MyEncounterId id)
        {
            if (MySession.Static.Ready)
            {
                this.RemoveEncounter(id, true, false);
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyEncounterId>(x => new Action<MyEncounterId>(MyEncounterGenerator.PersistEncounterClient), id, targetEndpoint, position);
            }
        }

        [Event(null, 0x70), Reliable, Broadcast]
        private static void PersistEncounterClient(MyEncounterId encounterId)
        {
            if (Static.m_encounterEntities.ContainsKey(encounterId))
            {
                Static.RemoveEncounter(encounterId, true, false);
            }
            else
            {
                Static.m_persistentEncounters.Add(encounterId);
            }
        }

        private static MySpawnGroupDefinition PickRandomEncounter(MyRandom random, MySpawnGroupDefinition[] candidates)
        {
            MySpawnGroupDefinition definition2;
            using (MyUtils.ReuseCollection<float>(ref m_spawnGroupCumulativeFrequencies))
            {
                float maxValue = 0f;
                MySpawnGroupDefinition[] definitionArray = candidates;
                int index = 0;
                while (true)
                {
                    if (index >= definitionArray.Length)
                    {
                        int num2 = 0;
                        float num3 = random.NextFloat(0f, maxValue);
                        while (true)
                        {
                            if ((num2 >= m_spawnGroupCumulativeFrequencies.Count) || (num3 <= m_spawnGroupCumulativeFrequencies[num2]))
                            {
                                if (num2 >= m_spawnGroupCumulativeFrequencies.Count)
                                {
                                    num2 = m_spawnGroupCumulativeFrequencies.Count - 1;
                                }
                                definition2 = candidates[num2];
                                break;
                            }
                            num2++;
                        }
                        break;
                    }
                    MySpawnGroupDefinition definition = definitionArray[index];
                    maxValue += definition.Frequency;
                    m_spawnGroupCumulativeFrequencies.Add(maxValue);
                    index++;
                }
            }
            return definition2;
        }

        public void PlaceEncounterToWorld(BoundingBoxD boundingVolume, int seed)
        {
            if ((MySession.Static.Settings.EnableEncounters && (this.m_encounterSpawnGroups.Length != 0)) && IsAcceptableEncounter(boundingVolume))
            {
                MyEncounterId key = new MyEncounterId(boundingVolume, seed, 0);
                if (this.m_encounterEntities.ContainsKey(key))
                {
                    this.m_encounterRemoveRequested.Remove(key);
                }
                else if (!this.m_persistentEncounters.Contains(key))
                {
                    this.OpenEncounter(key);
                    this.m_random.SetSeed(seed);
                    MySpawnGroupDefinition spawnGroup = PickRandomEncounter(this.m_random, this.m_encounterSpawnGroups);
                    Vector3D center = boundingVolume.Center;
                    for (int i = 0; i < spawnGroup.Voxels.Count; i++)
                    {
                        MySpawnGroupDefinition.SpawnGroupVoxel voxel = spawnGroup.Voxels[i];
                        MyStorageBase storage = MyStorageBase.LoadFromFile(MyWorldGenerator.GetVoxelPrefabPath(voxel.StorageName), null, true);
                        if (storage != null)
                        {
                            Vector3D positionMinCorner = center + voxel.Offset;
                            string storageName = $"Asteroid_{key.GetHashCode()}_{boundingVolume.Round().GetHashCode()}_{i}";
                            long asteroidEntityId = MyProceduralAsteroidCellGenerator.GetAsteroidEntityId(storageName);
                            VRage.Game.Entity.MyEntity entity = !voxel.CenterOffset ? MyWorldGenerator.AddVoxelMap(storageName, storage, positionMinCorner, asteroidEntityId) : MyWorldGenerator.AddVoxelMap(storageName, storage, MatrixD.CreateWorld(positionMinCorner), asteroidEntityId, false, false);
                            this.RegisterEntityToEncounter(key, entity);
                        }
                    }
                    if (Sync.IsServer)
                    {
                        bool flag = true;
                        if (spawnGroup.IsPirate)
                        {
                            MyIdentity identity = MySession.Static.Players.TryGetIdentity(MyPirateAntennas.GetPiratesId());
                            if (identity == null)
                            {
                                MyLog.Default.Error("Missing pirate identity. Encounter will not spawn.", Array.Empty<object>());
                                flag = false;
                            }
                            else if (!identity.BlockLimits.HasRemainingPCU)
                            {
                                MyLog.Default.Log(MyLogSeverity.Info, "Exhausted pirate PCUs. Encounter will not spawn.", Array.Empty<object>());
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            this.SpawnEncounterGrids(key, center, spawnGroup);
                        }
                        else if (spawnGroup.Prefabs.Count > 0)
                        {
                            this.PersistEncounter(key);
                        }
                    }
                }
            }
        }

        private void RegisterEntityToEncounter(MyEncounterId id, VRage.Game.Entity.MyEntity entity)
        {
            List<VRage.Game.Entity.MyEntity> list;
            entity.Save = false;
            if (this.m_encounterEntities.TryGetValue(id, out list))
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                MyVoxelBase base2 = entity as MyVoxelBase;
                if (Sync.IsServer)
                {
                    entity.OnMarkForClose += this.m_OnEntityClosed;
                    entity.PositionComp.OnPositionChanged += this.m_OnEntityPositionChanged;
                    if (grid != null)
                    {
                        grid.OnGridChanged += this.m_OnGridChanged;
                        grid.OnBlockAdded += this.m_OnBlockChanged;
                        grid.OnBlockRemoved += this.m_OnBlockChanged;
                        grid.OnBlockIntegrityChanged += this.m_OnBlockChanged;
                    }
                    if (base2 != null)
                    {
                        base2.RangeChanged += this.m_OnVoxelChanged;
                    }
                }
                list.Add(entity);
                this.m_entityToEncounterId.Add(entity, id);
            }
        }

        public void RemoveEncounter(BoundingBoxD boundingVolume, int seed)
        {
            if (MySession.Static.Settings.EnableEncounters && IsAcceptableEncounter(boundingVolume))
            {
                MyEncounterId item = new MyEncounterId(boundingVolume, seed, 0);
                if (this.m_encounterSpawnInProgress.Contains(item))
                {
                    this.m_encounterRemoveRequested.Add(item);
                }
                else if (!this.m_encounterEntities.ContainsKey(item))
                {
                    this.m_persistentEncounters.Contains(item);
                }
                else
                {
                    this.RemoveEncounter(item, false, true);
                }
            }
        }

        private void RemoveEncounter(MyEncounterId encounter, bool markPersistent, bool close)
        {
            if (!this.m_persistentEncounters.Contains(encounter))
            {
                List<VRage.Game.Entity.MyEntity> list;
                if (markPersistent)
                {
                    this.m_persistentEncounters.Add(encounter);
                }
                if (this.m_encounterEntities.TryGetValue(encounter, out list))
                {
                    foreach (VRage.Game.Entity.MyEntity entity in list)
                    {
                        MyCubeGrid grid = entity as MyCubeGrid;
                        MyVoxelBase base2 = entity as MyVoxelBase;
                        entity.OnMarkForClose -= this.m_OnEntityClosed;
                        entity.PositionComp.OnPositionChanged -= this.m_OnEntityPositionChanged;
                        if (grid != null)
                        {
                            grid.OnBlockAdded -= this.m_OnBlockChanged;
                            grid.OnGridChanged -= this.m_OnGridChanged;
                            grid.OnBlockRemoved -= this.m_OnBlockChanged;
                            grid.OnBlockIntegrityChanged -= this.m_OnBlockChanged;
                        }
                        if (base2 != null)
                        {
                            base2.RangeChanged -= this.m_OnVoxelChanged;
                        }
                        if (close)
                        {
                            if (Sync.IsServer || (entity is MyVoxelBase))
                            {
                                entity.Close();
                            }
                        }
                        else if (markPersistent && !entity.MarkedForClose)
                        {
                            entity.Save = true;
                        }
                        this.m_entityToEncounterId.Remove(entity);
                    }
                    this.m_entityListsPool.Return(list);
                    this.m_encounterEntities.Remove(encounter);
                }
            }
        }

        private void SpawnEncounterGrids(MyEncounterId encounterId, Vector3D placePosition, MySpawnGroupDefinition spawnGroup)
        {
            this.m_encounterSpawnInProgress.Add(encounterId);
            long ownerId = 0L;
            if (spawnGroup.IsPirate)
            {
                ownerId = MyPirateAntennas.GetPiratesId();
            }
            int remainingPrefabsToSpawn = spawnGroup.Prefabs.Count + 1;
            Action item = delegate {
                int num = remainingPrefabsToSpawn;
                remainingPrefabsToSpawn = num - 1;
                if (remainingPrefabsToSpawn == 0)
                {
                    this.m_encounterSpawnInProgress.Remove(encounterId);
                    if (this.m_encounterRemoveRequested.Contains(encounterId))
                    {
                        this.RemoveEncounter(encounterId, false, true);
                    }
                }
            };
            using (List<MySpawnGroupDefinition.SpawnGroupPrefab>.Enumerator enumerator = spawnGroup.Prefabs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MySpawnGroupDefinition.SpawnGroupPrefab selectedPrefab;
                    List<MyCubeGrid> createdGrids = new List<MyCubeGrid>();
                    Vector3D forward = Vector3D.Forward;
                    Vector3D up = Vector3D.Up;
                    SpawningOptions spawningOptions = SpawningOptions.SetAuthorship | SpawningOptions.UseGridOrigin | SpawningOptions.DisableSave;
                    if (selectedPrefab.Speed > 0f)
                    {
                        spawningOptions |= SpawningOptions.DisableDampeners | SpawningOptions.SpawnRandomCargo | SpawningOptions.RotateFirstCockpitTowardsDirection;
                        float minValue = (float) Math.Atan(2000.0 / placePosition.Length());
                        forward = -Vector3D.Normalize(placePosition);
                        float num3 = this.m_random.NextFloat(minValue, minValue + 0.5f);
                        float num4 = this.m_random.NextFloat(0f, 6.283186f);
                        Vector3D vectord3 = Vector3D.CalculatePerpendicularVector(forward);
                        vectord3 *= Math.Sin((double) num3) * Math.Cos((double) num4);
                        Vector3D vectord4 = Vector3D.Cross(forward, vectord3) * (Math.Sin((double) num3) * Math.Sin((double) num4));
                        forward = ((forward * Math.Cos((double) num3)) + vectord3) + vectord4;
                        up = Vector3D.CalculatePerpendicularVector(forward);
                    }
                    if (selectedPrefab.PlaceToGridOrigin)
                    {
                        spawningOptions |= SpawningOptions.UseGridOrigin;
                    }
                    if (selectedPrefab.ResetOwnership && !spawnGroup.IsPirate)
                    {
                        spawningOptions |= SpawningOptions.SetNeutralOwner;
                    }
                    if (!spawnGroup.ReactorsOn)
                    {
                        spawningOptions |= SpawningOptions.TurnOffReactors;
                    }
                    Stack<Action> callbacks = new Stack<Action>();
                    callbacks.Push(item);
                    callbacks.Push(delegate {
                        foreach (MyCubeGrid grid in createdGrids)
                        {
                            this.RegisterEntityToEncounter(encounterId, grid);
                        }
                        string behaviour = selectedPrefab.Behaviour;
                        if (!string.IsNullOrWhiteSpace(behaviour))
                        {
                            foreach (MyCubeGrid grid2 in createdGrids)
                            {
                                if (!MyDroneAI.SetAIToGrid(grid2, behaviour, selectedPrefab.BehaviourActivationDistance))
                                {
                                    object[] args = new object[] { grid2.DisplayName };
                                    MyLog.Default.Error("Could not inject AI to encounter {0}. No remote control.", args);
                                }
                            }
                        }
                    });
                    Vector3 initialAngularVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab(createdGrids, selectedPrefab.SubtypeId, placePosition + selectedPrefab.Position, (Vector3) forward, (Vector3) up, (Vector3) (forward * selectedPrefab.Speed), initialAngularVelocity, selectedPrefab.BeaconText, null, spawningOptions, ownerId, true, callbacks);
                }
            }
            item();
        }

        protected override void UnloadData()
        {
            while (this.m_encounterEntities.Count > 0)
            {
                KeyValuePair<MyEncounterId, List<VRage.Game.Entity.MyEntity>> pair = this.m_encounterEntities.FirstPair<MyEncounterId, List<VRage.Game.Entity.MyEntity>>();
                this.RemoveEncounter(pair.Key, false, true);
            }
            base.UnloadData();
            Static = null;
        }

        public static MyEncounterGenerator Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MySector), typeof(MyPirateAntennas) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEncounterGenerator.<>c <>9 = new MyEncounterGenerator.<>c();
            public static Func<IMyEventOwner, Action<MyEncounterId>> <>9__15_0;
            public static Action<List<VRage.Game.Entity.MyEntity>> <>9__36_0;
            public static Func<List<VRage.Game.Entity.MyEntity>> <>9__36_1;
            public static Func<MySpawnGroupDefinition, bool> <>9__37_0;

            internal void <.ctor>b__36_0(List<VRage.Game.Entity.MyEntity> x)
            {
                x.Clear();
            }

            internal List<VRage.Game.Entity.MyEntity> <.ctor>b__36_1() => 
                new List<VRage.Game.Entity.MyEntity>(10);

            internal bool <LoadData>b__37_0(MySpawnGroupDefinition x) => 
                x.IsEncounter;

            internal Action<MyEncounterId> <PersistEncounter>b__15_0(IMyEventOwner x) => 
                new Action<MyEncounterId>(MyEncounterGenerator.PersistEncounterClient);
        }
    }
}

