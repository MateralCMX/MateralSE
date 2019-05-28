namespace Sandbox.Game.Entities
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Groups;
    using VRage.Library;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner]
    public static class MyEntities
    {
        private static readonly long EntityNativeMemoryLimit = 0x51400000L;
        private static readonly long EntityManagedMemoryLimit = 0x28a00000L;
        private static MyConcurrentHashSet<MyEntity> m_entities = new MyConcurrentHashSet<MyEntity>();
        private static CachingList<MyEntity> m_entitiesForUpdateOnce = new CachingList<MyEntity>();
        private static MyDistributedUpdater<ConcurrentCachingList<MyEntity>, MyEntity> m_entitiesForUpdate = new MyDistributedUpdater<ConcurrentCachingList<MyEntity>, MyEntity>(1);
        private static MyDistributedUpdater<CachingList<MyEntity>, MyEntity> m_entitiesForUpdate10 = new MyDistributedUpdater<CachingList<MyEntity>, MyEntity>(10);
        private static MyDistributedUpdater<CachingList<MyEntity>, MyEntity> m_entitiesForUpdate100 = new MyDistributedUpdater<CachingList<MyEntity>, MyEntity>(100);
        private static MyDistributedUpdater<CachingList<MyEntity>, MyEntity> m_entitiesForSimulate = new MyDistributedUpdater<CachingList<MyEntity>, MyEntity>(1);
        private static CachingList<IMyEntity> m_entitiesForDraw = new CachingList<IMyEntity>();
        private static readonly List<IMySceneComponent> m_sceneComponents = new List<IMySceneComponent>();
        [ThreadStatic]
        private static MyEntityIdRemapHelper m_remapHelper;
        private static readonly int MAX_ENTITIES_CLOSE_PER_UPDATE = 10;
        [CompilerGenerated]
        private static Action<MyEntity> OnEntityRemove;
        [CompilerGenerated]
        private static Action<MyEntity> OnEntityAdd;
        [CompilerGenerated]
        private static Action<MyEntity> OnEntityCreate;
        [CompilerGenerated]
        private static Action<MyEntity> OnEntityDelete;
        [CompilerGenerated]
        private static Action OnCloseAll;
        [CompilerGenerated]
        private static Action<MyEntity, string, string> OnEntityNameSet;
        public static bool IsClosingAll = false;
        public static bool IgnoreMemoryLimits = false;
        private static MyEntityCreationThread m_creationThread;
        private static Dictionary<uint, IMyEntity> m_renderObjectToEntityMap = new Dictionary<uint, IMyEntity>();
        private static readonly FastResourceLock m_renderObjectToEntityMapLock = new FastResourceLock();
        [ThreadStatic]
        private static List<MyEntity> m_overlapRBElementList;
        private static readonly List<List<MyEntity>> m_overlapRBElementListCollection = new List<List<MyEntity>>();
        private static List<HkBodyCollision> m_rigidBodyList = new List<HkBodyCollision>();
        private static readonly List<MyLineSegmentOverlapResult<MyEntity>> LineOverlapEntityList = new List<MyLineSegmentOverlapResult<MyEntity>>();
        private static readonly List<MyPhysics.HitInfo> m_hits = new List<MyPhysics.HitInfo>();
        [ThreadStatic]
        private static HashSet<IMyEntity> m_entityResultSet;
        private static readonly List<HashSet<IMyEntity>> m_entityResultSetCollection = new List<HashSet<IMyEntity>>();
        [ThreadStatic]
        private static List<MyEntity> m_entityInputList;
        private static readonly List<List<MyEntity>> m_entityInputListCollection = new List<List<MyEntity>>();
        private static HashSet<MyEntity> m_entitiesToDelete = new HashSet<MyEntity>();
        private static HashSet<MyEntity> m_entitiesToDeleteNextFrame = new HashSet<MyEntity>();
        public static ConcurrentDictionary<string, MyEntity> m_entityNameDictionary = new ConcurrentDictionary<string, MyEntity>();
        private static bool m_isLoaded = false;
        private static HkShape m_cameraSphere;
        public static FastResourceLock EntityCloseLock = new FastResourceLock();
        public static FastResourceLock EntityMarkForCloseLock = new FastResourceLock();
        public static FastResourceLock UnloadDataLock = new FastResourceLock();
        public static bool UpdateInProgress = false;
        public static bool CloseAllowed = false;
        private static int m_update10Index = 0;
        private static int m_update100Index = 0;
        private static float m_update10Count = 0f;
        private static float m_update100Count = 0f;
        [ThreadStatic]
        private static List<MyEntity> m_allIgnoredEntities;
        private static readonly List<List<MyEntity>> m_allIgnoredEntitiesCollection = new List<List<MyEntity>>();
        private static readonly HashSet<System.Type> m_hiddenTypes = new HashSet<System.Type>();
        public static bool SafeAreasHidden;
        public static bool SafeAreasSelectable;
        public static bool DetectorsHidden;
        public static bool DetectorsSelectable;
        public static bool ParticleEffectsHidden;
        public static bool ParticleEffectsSelectable;
        private static readonly Dictionary<string, int> m_typesStats = new Dictionary<string, int>();
        private static List<MyCubeGrid> m_cubeGridList = new List<MyCubeGrid>();
        private static readonly HashSet<MyCubeGrid> m_cubeGridHash = new HashSet<MyCubeGrid>();
        private static readonly HashSet<IMyEntity> m_entitiesForDebugDraw = new HashSet<IMyEntity>();
        private static readonly HashSet<object> m_groupDebugHelper = new HashSet<object>();
        private static readonly MyStringId GIZMO_LINE_MATERIAL = MyStringId.GetOrCompute("GizmoDrawLine");
        private static readonly CachingDictionary<MyEntity, BoundingBoxDrawArgs> m_entitiesForBBoxDraw = new CachingDictionary<MyEntity, BoundingBoxDrawArgs>();

        public static  event Action OnCloseAll
        {
            [CompilerGenerated] add
            {
                Action onCloseAll = OnCloseAll;
                while (true)
                {
                    Action a = onCloseAll;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onCloseAll = Interlocked.CompareExchange<Action>(ref OnCloseAll, action3, a);
                    if (ReferenceEquals(onCloseAll, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onCloseAll = OnCloseAll;
                while (true)
                {
                    Action source = onCloseAll;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onCloseAll = Interlocked.CompareExchange<Action>(ref OnCloseAll, action3, source);
                    if (ReferenceEquals(onCloseAll, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntity> OnEntityAdd
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onEntityAdd = OnEntityAdd;
                while (true)
                {
                    Action<MyEntity> a = onEntityAdd;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onEntityAdd = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityAdd, action3, a);
                    if (ReferenceEquals(onEntityAdd, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onEntityAdd = OnEntityAdd;
                while (true)
                {
                    Action<MyEntity> source = onEntityAdd;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onEntityAdd = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityAdd, action3, source);
                    if (ReferenceEquals(onEntityAdd, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntity> OnEntityCreate
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onEntityCreate = OnEntityCreate;
                while (true)
                {
                    Action<MyEntity> a = onEntityCreate;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onEntityCreate = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityCreate, action3, a);
                    if (ReferenceEquals(onEntityCreate, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onEntityCreate = OnEntityCreate;
                while (true)
                {
                    Action<MyEntity> source = onEntityCreate;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onEntityCreate = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityCreate, action3, source);
                    if (ReferenceEquals(onEntityCreate, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntity> OnEntityDelete
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onEntityDelete = OnEntityDelete;
                while (true)
                {
                    Action<MyEntity> a = onEntityDelete;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onEntityDelete = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityDelete, action3, a);
                    if (ReferenceEquals(onEntityDelete, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onEntityDelete = OnEntityDelete;
                while (true)
                {
                    Action<MyEntity> source = onEntityDelete;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onEntityDelete = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityDelete, action3, source);
                    if (ReferenceEquals(onEntityDelete, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntity, string, string> OnEntityNameSet
        {
            [CompilerGenerated] add
            {
                Action<MyEntity, string, string> onEntityNameSet = OnEntityNameSet;
                while (true)
                {
                    Action<MyEntity, string, string> a = onEntityNameSet;
                    Action<MyEntity, string, string> action3 = (Action<MyEntity, string, string>) Delegate.Combine(a, value);
                    onEntityNameSet = Interlocked.CompareExchange<Action<MyEntity, string, string>>(ref OnEntityNameSet, action3, a);
                    if (ReferenceEquals(onEntityNameSet, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity, string, string> onEntityNameSet = OnEntityNameSet;
                while (true)
                {
                    Action<MyEntity, string, string> source = onEntityNameSet;
                    Action<MyEntity, string, string> action3 = (Action<MyEntity, string, string>) Delegate.Remove(source, value);
                    onEntityNameSet = Interlocked.CompareExchange<Action<MyEntity, string, string>>(ref OnEntityNameSet, action3, source);
                    if (ReferenceEquals(onEntityNameSet, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntity> OnEntityRemove
        {
            [CompilerGenerated] add
            {
                Action<MyEntity> onEntityRemove = OnEntityRemove;
                while (true)
                {
                    Action<MyEntity> a = onEntityRemove;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Combine(a, value);
                    onEntityRemove = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityRemove, action3, a);
                    if (ReferenceEquals(onEntityRemove, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity> onEntityRemove = OnEntityRemove;
                while (true)
                {
                    Action<MyEntity> source = onEntityRemove;
                    Action<MyEntity> action3 = (Action<MyEntity>) Delegate.Remove(source, value);
                    onEntityRemove = Interlocked.CompareExchange<Action<MyEntity>>(ref OnEntityRemove, action3, source);
                    if (ReferenceEquals(onEntityRemove, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyEntities()
        {
            System.Type type = typeof(MyEntity);
            MyEntityFactory.RegisterDescriptor(type.GetCustomAttribute<MyEntityTypeAttribute>(false), type);
            MyEntityFactory.RegisterDescriptorsFromAssembly(typeof(MyEntities).Assembly);
            MyEntityFactory.RegisterDescriptorsFromAssembly(MyPlugins.GameAssembly);
            MyEntityFactory.RegisterDescriptorsFromAssembly(MyPlugins.SandboxAssembly);
            MyEntityFactory.RegisterDescriptorsFromAssembly(MyPlugins.UserAssemblies);
            MyEntityExtensions.SetCallbacks();
            MyEntitiesInterface.RegisterUpdate = new Action<MyEntity>(MyEntities.RegisterForUpdate);
            MyEntitiesInterface.UnregisterUpdate = new Action<MyEntity, bool>(MyEntities.UnregisterForUpdate);
            MyEntitiesInterface.RegisterDraw = new Action<MyEntity>(MyEntities.RegisterForDraw);
            MyEntitiesInterface.UnregisterDraw = new Action<MyEntity>(MyEntities.UnregisterForDraw);
            MyEntitiesInterface.SetEntityName = new Action<MyEntity, bool>(MyEntities.SetEntityName);
            MyEntitiesInterface.IsUpdateInProgress = new Func<bool>(MyEntities.IsUpdateInProgress);
            MyEntitiesInterface.IsCloseAllowed = new Func<bool>(MyEntities.IsCloseAllowed);
            MyEntitiesInterface.RemoveName = new Action<MyEntity>(MyEntities.RemoveName);
            MyEntitiesInterface.RemoveFromClosedEntities = new Action<MyEntity>(MyEntities.RemoveFromClosedEntities);
            MyEntitiesInterface.Remove = new Func<MyEntity, bool>(MyEntities.Remove);
            MyEntitiesInterface.RaiseEntityRemove = new Action<MyEntity>(MyEntities.RaiseEntityRemove);
            MyEntitiesInterface.Close = new Action<MyEntity>(MyEntities.Close);
        }

        public static void Add(MyEntity entity, bool insertIntoScene = true)
        {
            if (insertIntoScene)
            {
                entity.OnAddedToScene(entity);
            }
            if (!Exist(entity))
            {
                if (entity is MyVoxelBase)
                {
                    MySession.Static.VoxelMaps.Add((MyVoxelBase) entity);
                }
                m_entities.Add(entity);
                RaiseEntityAdd(entity);
            }
        }

        private static void AddComponents()
        {
            m_sceneComponents.Add(new MyCubeGridGroups());
            m_sceneComponents.Add(new MyWeldingGroups());
            m_sceneComponents.Add(new MyGridPhysicalHierarchy());
            m_sceneComponents.Add(new MySharedTensorsGroups());
            m_sceneComponents.Add(new MyFixedGrids());
        }

        public static void AddRenderObjectToMap(uint id, IMyEntity entity)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                using (m_renderObjectToEntityMapLock.AcquireExclusiveUsing())
                {
                    m_renderObjectToEntityMap.Add(id, entity);
                }
            }
        }

        public static void CallAsync(Action doneHandler)
        {
            InitAsync(null, null, false, e => doneHandler(), 0, 0.0, false);
        }

        public static void CallAsync(MyEntity entity, Action<MyEntity> doneHandler)
        {
            InitAsync(entity, null, false, doneHandler, 0, 0.0, false);
        }

        private static bool CallInitEntity(WorkData workData)
        {
            InitEntityData data = workData as InitEntityData;
            if (data != null)
            {
                return data.CallInitEntity();
            }
            workData.FlagAsFailed();
            return false;
        }

        public static void Close(MyEntity entity)
        {
            if (CloseAllowed)
            {
                m_entitiesToDeleteNextFrame.Add(entity);
            }
            else if (!m_entitiesToDelete.Contains(entity))
            {
                using (EntityMarkForCloseLock.AcquireExclusiveUsing())
                {
                    m_entitiesToDelete.Add(entity);
                }
            }
        }

        public static void CloseAll()
        {
            IsClosingAll = true;
            if (OnCloseAll != null)
            {
                OnCloseAll();
            }
            CloseAllowed = true;
            List<MyEntity> source = new List<MyEntity>();
            foreach (MyEntity entity in m_entities)
            {
                entity.Close();
                m_entitiesToDelete.Add(entity);
            }
            foreach (MyEntity entity2 in m_entitiesToDelete.ToArray<MyEntity>())
            {
                if (entity2.Pinned)
                {
                    source.Add(entity2);
                }
                else
                {
                    entity2.Render.FadeOut = false;
                    entity2.Delete();
                }
            }
            while (source.Count > 0)
            {
                MyEntity item = source.First<MyEntity>();
                if (item.Pinned)
                {
                    Thread.Sleep(10);
                    continue;
                }
                item.Render.FadeOut = false;
                item.Delete();
                source.Remove(item);
            }
            m_entitiesForUpdateOnce.ApplyRemovals();
            m_entitiesForUpdate.List.ApplyRemovals();
            m_entitiesForUpdate10.List.ApplyRemovals();
            m_entitiesForUpdate100.List.ApplyRemovals();
            CloseAllowed = false;
            m_entitiesToDelete.Clear();
            MyEntityIdentifier.Clear();
            MyGamePruningStructure.Clear();
            MyRadioBroadcasters.Clear();
            m_entitiesForDraw.ApplyChanges();
            IsClosingAll = false;
        }

        public static void CreateAsync(MyObjectBuilder_EntityBase objectBuilder, bool addToScene, Action<MyEntity> doneHandler = null)
        {
            if (m_creationThread != null)
            {
                m_creationThread.SubmitWork(objectBuilder, addToScene, doneHandler, null, 0, 0.0, false);
            }
        }

        public static MyEntity CreateEntity(MyDefinitionId entityContainerId, bool fadeIn, bool setPosAndRot = false, Vector3? position = new Vector3?(), Vector3? up = new Vector3?(), Vector3? forward = new Vector3?())
        {
            MyContainerDefinition definition;
            if (!MyDefinitionManager.Static.TryGetContainerDefinition(entityContainerId, out definition))
            {
                return null;
            }
            MyObjectBuilder_EntityBase objectBuilder = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) entityContainerId) as MyObjectBuilder_EntityBase;
            if (objectBuilder == null)
            {
                return null;
            }
            if (setPosAndRot)
            {
                objectBuilder.PositionAndOrientation = new MyPositionAndOrientation((position != null) ? ((Vector3D) position.Value) : Vector3.Zero, (forward != null) ? forward.Value : Vector3.Forward, (up != null) ? up.Value : Vector3.Up);
            }
            return CreateFromObjectBuilder(objectBuilder, fadeIn);
        }

        public static MyEntity CreateEntityAndAdd(MyDefinitionId entityContainerId, bool fadeIn, bool setPosAndRot = false, Vector3? position = new Vector3?(), Vector3? up = new Vector3?(), Vector3? forward = new Vector3?())
        {
            MyContainerDefinition definition;
            if (!MyDefinitionManager.Static.TryGetContainerDefinition(entityContainerId, out definition))
            {
                return null;
            }
            MyObjectBuilder_EntityBase objectBuilder = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) entityContainerId) as MyObjectBuilder_EntityBase;
            if (objectBuilder == null)
            {
                return null;
            }
            if (setPosAndRot)
            {
                objectBuilder.PositionAndOrientation = new MyPositionAndOrientation((position != null) ? ((Vector3D) position.Value) : Vector3.Zero, (forward != null) ? forward.Value : Vector3.Forward, (up != null) ? up.Value : Vector3.Up);
            }
            return CreateFromObjectBuilderAndAdd(objectBuilder, fadeIn);
        }

        public static MyEntity CreateFromComponentContainerDefinitionAndAdd(MyDefinitionId entityContainerDefinitionId, bool fadeIn, bool insertIntoScene = true)
        {
            MyContainerDefinition definition;
            if (!typeof(MyObjectBuilder_EntityBase).IsAssignableFrom((System.Type) entityContainerDefinitionId.TypeId))
            {
                return null;
            }
            if (!MyComponentContainerExtension.TryGetContainerDefinition(entityContainerDefinitionId.TypeId, entityContainerDefinitionId.SubtypeId, out definition))
            {
                MySandboxGame.Log.WriteLine("Entity container definition not found: " + entityContainerDefinitionId);
                return null;
            }
            MyObjectBuilder_EntityBase objectBuilder = MyObjectBuilderSerializer.CreateNewObject(entityContainerDefinitionId.TypeId, entityContainerDefinitionId.SubtypeName) as MyObjectBuilder_EntityBase;
            if (objectBuilder == null)
            {
                MySandboxGame.Log.WriteLine("Entity builder was not created: " + entityContainerDefinitionId);
                return null;
            }
            if (insertIntoScene)
            {
                objectBuilder.PersistentFlags |= MyPersistentEntityFlags2.InScene;
            }
            return CreateFromObjectBuilderAndAdd(objectBuilder, fadeIn);
        }

        public static MyEntity CreateFromObjectBuilder(MyObjectBuilder_EntityBase objectBuilder, bool fadeIn)
        {
            MyEntity entity = CreateFromObjectBuilderNoinit(objectBuilder);
            entity.Render.FadeIn = fadeIn;
            InitEntity(objectBuilder, ref entity);
            return entity;
        }

        public static MyEntity CreateFromObjectBuilderAndAdd(MyObjectBuilder_EntityBase objectBuilder, bool fadeIn)
        {
            bool insertIntoScene = (objectBuilder.PersistentFlags & MyPersistentEntityFlags2.InScene) > MyPersistentEntityFlags2.None;
            if (MyFakes.ENABLE_LARGE_OFFSET && (objectBuilder.PositionAndOrientation.Value.Position.X < 10000.0))
            {
                MyPositionAndOrientation orientation = new MyPositionAndOrientation {
                    Forward = objectBuilder.PositionAndOrientation.Value.Forward,
                    Up = objectBuilder.PositionAndOrientation.Value.Up,
                    Position = new SerializableVector3D(((Vector3D) objectBuilder.PositionAndOrientation.Value.Position) + new Vector3D(1000000000.0))
                };
                objectBuilder.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
            }
            MyEntity entity = CreateFromObjectBuilder(objectBuilder, fadeIn);
            if (entity != null)
            {
                if (entity.EntityId == 0)
                {
                    entity = null;
                }
                else
                {
                    Add(entity, insertIntoScene);
                }
            }
            return entity;
        }

        public static MyEntity CreateFromObjectBuilderNoinit(MyObjectBuilder_EntityBase objectBuilder)
        {
            if ((((objectBuilder.TypeId != typeof(MyObjectBuilder_CubeGrid)) && (objectBuilder.TypeId != typeof(MyObjectBuilder_VoxelMap))) || IgnoreMemoryLimits) || !MemoryLimitReachedReport)
            {
                return MyEntityFactory.CreateEntity(objectBuilder);
            }
            MemoryLimitAddFailure = true;
            MySandboxGame.Log.WriteLine("WARNING: MemoryLimitAddFailure reached");
            return null;
        }

        public static MyEntity CreateFromObjectBuilderParallel(MyObjectBuilder_EntityBase objectBuilder, bool addToScene = false, Action<MyEntity> completionCallback = null, MyEntity entity = null, MyEntity relativeSpawner = null, Vector3D? relativeOffset = new Vector3D?(), bool checkPosition = false, bool fadeIn = false)
        {
            if (entity == null)
            {
                entity = CreateFromObjectBuilderNoinit(objectBuilder);
                if (entity == null)
                {
                    return null;
                }
            }
            InitEntityData initData = new InitEntityData(objectBuilder, addToScene, completionCallback, entity, fadeIn, relativeSpawner, relativeOffset, checkPosition);
            Parallel.Start(delegate {
                if (CallInitEntity(initData))
                {
                    Action <>9__1;
                    Action action = <>9__1;
                    if (<>9__1 == null)
                    {
                        Action local1 = <>9__1;
                        action = <>9__1 = () => OnEntityInitialized(initData);
                    }
                    MySandboxGame.Static.Invoke(action, "CreateFromObjectBuilderParallel(alreadyParallel: true)");
                }
            });
            return entity;
        }

        public static void DebugDraw()
        {
            MyEntityComponentsDebugDraw.DebugDraw();
            if (MyCubeGridGroups.Static != null)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_GRID_GROUPS_PHYSICAL)
                {
                    DebugDrawGroups<MyCubeGrid, MyGridPhysicalGroupData>(MyCubeGridGroups.Static.Physical);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_GRID_GROUPS_LOGICAL)
                {
                    DebugDrawGroups<MyCubeGrid, MyGridLogicalGroupData>(MyCubeGridGroups.Static.Logical);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_SMALL_TO_LARGE_BLOCK_GROUPS)
                {
                    MyCubeGridGroups.DebugDrawBlockGroups<MySlimBlock, MyBlockGroupData>(MyCubeGridGroups.Static.SmallToLargeBlockConnections);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_DYNAMIC_PHYSICAL_GROUPS)
                {
                    DebugDrawGroups<MyCubeGrid, MyGridPhysicalDynamicGroupData>(MyCubeGridGroups.Static.PhysicalDynamic);
                }
            }
            if ((MyDebugDrawSettings.DEBUG_DRAW_PHYSICS || MyDebugDrawSettings.ENABLE_DEBUG_DRAW) || MyFakes.SHOW_INVALID_TRIANGLES)
            {
                using (m_renderObjectToEntityMapLock.AcquireSharedUsing())
                {
                    m_entitiesForDebugDraw.Clear();
                    foreach (uint num in MyRenderProxy.VisibleObjectsRead)
                    {
                        IMyEntity entity;
                        m_renderObjectToEntityMap.TryGetValue(num, out entity);
                        if (entity != null)
                        {
                            IMyEntity topMostParent = entity.GetTopMostParent(null);
                            if (!m_entitiesForDebugDraw.Contains(topMostParent))
                            {
                                m_entitiesForDebugDraw.Add(topMostParent);
                            }
                        }
                    }
                    if (MyDebugDrawSettings.DEBUG_DRAW_GRID_COUNTER)
                    {
                        MyRenderProxy.DebugDrawText2D(new Vector2(700f, 0f), "Grid number: " + MyCubeGrid.GridCounter, Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, false);
                    }
                    foreach (MyEntity entity3 in m_entities)
                    {
                        m_entitiesForDebugDraw.Add(entity3);
                    }
                    foreach (IMyEntity entity4 in m_entitiesForDebugDraw)
                    {
                        if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                        {
                            entity4.DebugDraw();
                        }
                        if (MyFakes.SHOW_INVALID_TRIANGLES)
                        {
                            entity4.DebugDrawInvalidTriangles();
                        }
                    }
                    if ((MyDebugDrawSettings.DEBUG_DRAW_VELOCITIES | MyDebugDrawSettings.DEBUG_DRAW_INTERPOLATED_VELOCITIES) | MyDebugDrawSettings.DEBUG_DRAW_RIGID_BODY_ACTIONS)
                    {
                        foreach (IMyEntity entity5 in m_entitiesForDebugDraw)
                        {
                            if (entity5.Physics == null)
                            {
                                continue;
                            }
                            if (Vector3D.Distance(MySector.MainCamera.Position, entity5.WorldAABB.Center) < 500.0)
                            {
                                MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD(entity5.LocalAABB, entity5.WorldMatrix);
                                if (MyDebugDrawSettings.DEBUG_DRAW_VELOCITIES)
                                {
                                    Color yellow = Color.Yellow;
                                    if (entity5.Physics.IsStatic)
                                    {
                                        yellow = Color.RoyalBlue;
                                    }
                                    else if (!entity5.Physics.IsActive)
                                    {
                                        yellow = Color.Red;
                                    }
                                    MyRenderProxy.DebugDrawOBB(obb, yellow, 1f, false, false, false);
                                    MyRenderProxy.DebugDrawLine3D(entity5.WorldAABB.Center, entity5.WorldAABB.Center + (entity5.Physics.LinearVelocity * 100f), Color.Green, Color.White, false, false);
                                }
                                if (MyDebugDrawSettings.DEBUG_DRAW_INTERPOLATED_VELOCITIES)
                                {
                                    Vector3 vector;
                                    HkRigidBody rigidBody = entity5.Physics.RigidBody;
                                    if ((rigidBody != null) && rigidBody.GetCustomVelocity(out vector))
                                    {
                                        MyRenderProxy.DebugDrawOBB(obb, Color.RoyalBlue, 1f, false, false, false);
                                        MyRenderProxy.DebugDrawLine3D(entity5.WorldAABB.Center, entity5.WorldAABB.Center + (vector * 100f), Color.Green, Color.White, false, false);
                                    }
                                }
                            }
                        }
                    }
                    m_entitiesForDebugDraw.Clear();
                    if (MyDebugDrawSettings.DEBUG_DRAW_GAME_PRUNNING)
                    {
                        MyGamePruningStructure.DebugDraw();
                    }
                    if (MyDebugDrawSettings.DEBUG_DRAW_RADIO_BROADCASTERS)
                    {
                        MyRadioBroadcasters.DebugDraw();
                    }
                }
                m_entitiesForDebugDraw.Clear();
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS)
            {
                if ((MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS != MyPhysics.DebugDrawClustersEnable) && (MySector.MainCamera != null))
                {
                    MyPhysics.DebugDrawClustersMatrix = MySector.MainCamera.WorldMatrix;
                }
                MyPhysics.DebugDrawClusters();
            }
            MyPhysics.DebugDrawClustersEnable = MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS;
            if (MyDebugDrawSettings.DEBUG_DRAW_ENTITY_STATISTICS)
            {
                DebugDrawStatistics();
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_STATISTICS)
            {
                DebugDrawGridStatistics();
            }
        }

        public static unsafe void DebugDrawGridStatistics()
        {
            m_cubeGridList.Clear();
            m_cubeGridHash.Clear();
            int num = 0;
            int num2 = 0;
            Vector2 screenCoord = new Vector2(100f, 0f);
            MyRenderProxy.DebugDrawText2D(screenCoord, "Detailed grid statistics", Color.Yellow, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (MyEntity entity in GetEntities())
            {
                if (entity is MyCubeGrid)
                {
                    m_cubeGridList.Add(entity as MyCubeGrid);
                    m_cubeGridHash.Add(MyGridPhysicalHierarchy.Static.GetRoot(entity as MyCubeGrid));
                    if ((entity as MyCubeGrid).NeedsPerFrameUpdate)
                    {
                        num++;
                    }
                    if ((entity as MyCubeGrid).NeedsPerFrameDraw)
                    {
                        num2++;
                    }
                }
            }
            m_cubeGridList = (from x in m_cubeGridList
                orderby x.BlocksCount descending
                select x).ToList<MyCubeGrid>();
            float scale = 0.7f;
            float* singlePtr1 = (float*) ref screenCoord.Y;
            singlePtr1[0] += 50f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Grids by blocks (" + m_cubeGridList.Count + "):", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr2 = (float*) ref screenCoord.Y;
            singlePtr2[0] += 30f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Grids needing update: " + num, Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr3 = (float*) ref screenCoord.Y;
            singlePtr3[0] += 30f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Grids needing draw: " + num2, Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr4 = (float*) ref screenCoord.Y;
            singlePtr4[0] += 30f;
            foreach (MyCubeGrid grid in m_cubeGridList)
            {
                MyRenderProxy.DebugDrawText2D(screenCoord, grid.DisplayName + ": " + grid.BlocksCount.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr5 = (float*) ref screenCoord.Y;
                singlePtr5[0] += 20f;
            }
            screenCoord.Y = 0f;
            float* singlePtr6 = (float*) ref screenCoord.X;
            singlePtr6[0] += 800f;
            float* singlePtr7 = (float*) ref screenCoord.Y;
            singlePtr7[0] += 50f;
            m_cubeGridList = m_cubeGridHash.OrderByDescending<MyCubeGrid, int>(delegate (MyCubeGrid x) {
                if (MyGridPhysicalHierarchy.Static.GetNode(x) == null)
                {
                    return 0;
                }
                return MyGridPhysicalHierarchy.Static.GetNode(x).Children.Count;
            }).ToList<MyCubeGrid>();
            m_cubeGridList.RemoveAll(delegate (MyCubeGrid x) {
                if (MyGridPhysicalHierarchy.Static.GetNode(x) != null)
                {
                    return MyGridPhysicalHierarchy.Static.GetNode(x).Children.Count == 0;
                }
                return true;
            });
            MyRenderProxy.DebugDrawText2D(screenCoord, "Root grids (" + m_cubeGridList.Count + "):", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr8 = (float*) ref screenCoord.Y;
            singlePtr8[0] += 30f;
            foreach (MyCubeGrid grid2 in m_cubeGridList)
            {
                int count;
                if (MyGridPhysicalHierarchy.Static.GetNode(grid2) == null)
                {
                    count = 0;
                }
                else
                {
                    count = MyGridPhysicalHierarchy.Static.GetNode(grid2).Children.Count;
                }
                MyRenderProxy.DebugDrawText2D(screenCoord, grid2.DisplayName + ": " + count.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr9 = (float*) ref screenCoord.Y;
                singlePtr9[0] += 20f;
            }
        }

        private static void DebugDrawGroups<TNode, TGroupData>(MyGroups<TNode, TGroupData> groups) where TNode: MyCubeGrid where TGroupData: IGroupData<TNode>, new()
        {
            int num = 0;
            using (HashSet<MyGroups<TNode, TGroupData>.Group>.Enumerator enumerator = groups.Groups.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num++;
                    Color colorFrom = new Vector3(((float) (num % 15)) / 15f, 1f, 1f).HSVtoColor();
                    HashSetReader<MyGroups<TNode, TGroupData>.Node> nodes = enumerator.Current.Nodes;
                    foreach (MyGroups<TNode, TGroupData>.Node node in nodes)
                    {
                        try
                        {
                            SortedDictionaryValuesReader<long, MyGroups<TNode, TGroupData>.Node> children = node.Children;
                            foreach (MyGroups<TNode, TGroupData>.Node node2 in children)
                            {
                                m_groupDebugHelper.Add(node2);
                            }
                            foreach (object obj2 in m_groupDebugHelper)
                            {
                                MyGroups<TNode, TGroupData>.Node node3 = null;
                                int num2 = 0;
                                children = node.Children;
                                foreach (MyGroups<TNode, TGroupData>.Node node4 in children)
                                {
                                    if (obj2 == node4)
                                    {
                                        node3 = node4;
                                        num2++;
                                    }
                                }
                                MyRenderProxy.DebugDrawLine3D(node.NodeData.PositionComp.WorldAABB.Center, node3.NodeData.PositionComp.WorldAABB.Center, colorFrom, colorFrom, false, false);
                                MyRenderProxy.DebugDrawText3D((node.NodeData.PositionComp.WorldAABB.Center + node3.NodeData.PositionComp.WorldAABB.Center) * 0.5, num2.ToString(), colorFrom, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            }
                            Color color = new Color(colorFrom.ToVector3() + 0.25f);
                            MyRenderProxy.DebugDrawSphere(node.NodeData.PositionComp.WorldAABB.Center, 0.2f, color.ToVector3(), 0.5f, false, true, true, false);
                            MyRenderProxy.DebugDrawText3D(node.NodeData.PositionComp.WorldAABB.Center, node.LinkCount.ToString(), color, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        finally
                        {
                            m_groupDebugHelper.Clear();
                        }
                    }
                }
            }
        }

        public static unsafe void DebugDrawStatistics()
        {
            string str2;
            int num2;
            List<MyEntity>.Enumerator enumerator3;
            m_typesStats.Clear();
            Vector2 screenCoord = new Vector2(100f, 0f);
            MyRenderProxy.DebugDrawText2D(screenCoord, "Detailed entity statistics", Color.Yellow, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyEntity, List<MyEntity>.Enumerator> enumerator = m_entitiesForUpdate.List.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.GetType().Name.ToString();
                    if (!m_typesStats.ContainsKey(key))
                    {
                        m_typesStats.Add(key, 0);
                    }
                    str2 = key;
                    m_typesStats[str2] += 1;
                }
            }
            float scale = 0.7f;
            float* singlePtr1 = (float*) ref screenCoord.Y;
            singlePtr1[0] += 50f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Entities for update:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr2 = (float*) ref screenCoord.Y;
            singlePtr2[0] += 30f;
            foreach (KeyValuePair<string, int> pair in from x in m_typesStats
                orderby x.Value descending
                select x)
            {
                num2 = pair.Value;
                MyRenderProxy.DebugDrawText2D(screenCoord, pair.Key + ": " + num2.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr3 = (float*) ref screenCoord.Y;
                singlePtr3[0] += 20f;
            }
            m_typesStats.Clear();
            screenCoord.Y = 0f;
            using (enumerator3 = m_entitiesForUpdate10.List.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    string key = enumerator3.Current.GetType().Name.ToString();
                    if (!m_typesStats.ContainsKey(key))
                    {
                        m_typesStats.Add(key, 0);
                    }
                    str2 = key;
                    m_typesStats[str2] += 1;
                }
            }
            float* singlePtr4 = (float*) ref screenCoord.X;
            singlePtr4[0] += 300f;
            float* singlePtr5 = (float*) ref screenCoord.Y;
            singlePtr5[0] += 50f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Entities for update10:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr6 = (float*) ref screenCoord.Y;
            singlePtr6[0] += 30f;
            foreach (KeyValuePair<string, int> pair2 in from x in m_typesStats
                orderby x.Value descending
                select x)
            {
                num2 = pair2.Value;
                MyRenderProxy.DebugDrawText2D(screenCoord, pair2.Key + ": " + num2.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr7 = (float*) ref screenCoord.Y;
                singlePtr7[0] += 20f;
            }
            m_typesStats.Clear();
            screenCoord.Y = 0f;
            using (enumerator3 = m_entitiesForUpdate100.List.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    string key = enumerator3.Current.GetType().Name.ToString();
                    if (!m_typesStats.ContainsKey(key))
                    {
                        m_typesStats.Add(key, 0);
                    }
                    str2 = key;
                    m_typesStats[str2] += 1;
                }
            }
            float* singlePtr8 = (float*) ref screenCoord.X;
            singlePtr8[0] += 300f;
            float* singlePtr9 = (float*) ref screenCoord.Y;
            singlePtr9[0] += 50f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "Entities for update100:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr10 = (float*) ref screenCoord.Y;
            singlePtr10[0] += 30f;
            foreach (KeyValuePair<string, int> pair3 in from x in m_typesStats
                orderby x.Value descending
                select x)
            {
                num2 = pair3.Value;
                MyRenderProxy.DebugDrawText2D(screenCoord, pair3.Key + ": " + num2.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr11 = (float*) ref screenCoord.Y;
                singlePtr11[0] += 20f;
            }
            m_typesStats.Clear();
            screenCoord.Y = 0f;
            using (ConcurrentEnumerator<SpinLockRef.Token, MyEntity, HashSet<MyEntity>.Enumerator> enumerator4 = m_entities.GetEnumerator())
            {
                while (enumerator4.MoveNext())
                {
                    string key = enumerator4.Current.GetType().Name.ToString();
                    if (!m_typesStats.ContainsKey(key))
                    {
                        m_typesStats.Add(key, 0);
                    }
                    str2 = key;
                    m_typesStats[str2] += 1;
                }
            }
            float* singlePtr12 = (float*) ref screenCoord.X;
            singlePtr12[0] += 300f;
            float* singlePtr13 = (float*) ref screenCoord.Y;
            singlePtr13[0] += 50f;
            scale = 0.7f;
            float* singlePtr14 = (float*) ref screenCoord.Y;
            singlePtr14[0] += 50f;
            MyRenderProxy.DebugDrawText2D(screenCoord, "All entities:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr15 = (float*) ref screenCoord.Y;
            singlePtr15[0] += 30f;
            foreach (KeyValuePair<string, int> pair4 in from x in m_typesStats
                orderby x.Value descending
                select x)
            {
                MyRenderProxy.DebugDrawText2D(screenCoord, pair4.Key + ": " + pair4.Value.ToString() + "x", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr16 = (float*) ref screenCoord.Y;
                singlePtr16[0] += 20f;
            }
        }

        public static void DeleteRememberedEntities()
        {
            CloseAllowed = true;
            while (m_entitiesToDelete.Count > 0)
            {
                using (EntityCloseLock.AcquireExclusiveUsing())
                {
                    MyEntity entity = m_entitiesToDelete.FirstElement<MyEntity>();
                    if (entity.Pinned)
                    {
                        Remove(entity);
                        m_entitiesToDelete.Remove(entity);
                        m_entitiesToDeleteNextFrame.Add(entity);
                        continue;
                    }
                    Action<MyEntity> onEntityDelete = OnEntityDelete;
                    if (onEntityDelete != null)
                    {
                        onEntityDelete(entity);
                    }
                    entity.Delete();
                }
            }
            CloseAllowed = false;
            m_entitiesToDelete = m_entitiesToDeleteNextFrame;
            m_entitiesToDeleteNextFrame = m_entitiesToDelete;
        }

        public static unsafe void Draw()
        {
            m_entitiesForDraw.ApplyChanges();
            foreach (MyEntity entity in m_entitiesForDraw)
            {
                if (IsAnyRenderObjectVisible(entity))
                {
                    entity.PrepareForDraw();
                    string name = entity.GetType().Name;
                    entity.Render.Draw();
                }
            }
            m_entitiesForBBoxDraw.ApplyChanges();
            foreach (KeyValuePair<MyEntity, BoundingBoxDrawArgs> pair in m_entitiesForBBoxDraw)
            {
                MatrixD worldMatrix = pair.Key.WorldMatrix;
                BoundingBoxD localAABB = pair.Key.PositionComp.LocalAABB;
                BoundingBoxDrawArgs args = pair.Value;
                Vector3D* vectordPtr1 = (Vector3D*) ref localAABB.Min;
                vectordPtr1[0] -= args.InflateAmount;
                Vector3D* vectordPtr2 = (Vector3D*) ref localAABB.Max;
                vectordPtr2[0] += args.InflateAmount;
                MatrixD worldToLocal = MatrixD.Invert(worldMatrix);
                MyStringId? faceMaterial = null;
                MySimpleObjectDraw.DrawAttachedTransparentBox(ref worldMatrix, ref localAABB, ref args.Color, pair.Key.Render.GetRenderObjectID(), ref worldToLocal, MySimpleObjectRasterizer.Wireframe, Vector3I.One, args.LineWidth, faceMaterial, new MyStringId?(args.lineMaterial), false, MyBillboard.BlendTypeEnum.LDR);
            }
        }

        public static unsafe void EnableEntityBoundingBoxDraw(MyEntity entity, bool enable, Vector4? color = new Vector4?(), float lineWidth = 0.01f, Vector3? inflateAmount = new Vector3?(), MyStringId? lineMaterial = new MyStringId?())
        {
            if (!enable)
            {
                m_entitiesForBBoxDraw.Remove(entity, false);
                entity.OnClose -= new Action<MyEntity>(MyEntities.entityForBBoxDraw_OnClose);
            }
            else
            {
                BoundingBoxDrawArgs* argsPtr1;
                BoundingBoxDrawArgs* argsPtr2;
                BoundingBoxDrawArgs* argsPtr3;
                if (!m_entitiesForBBoxDraw.ContainsKey(entity))
                {
                    entity.OnClose += new Action<MyEntity>(MyEntities.entityForBBoxDraw_OnClose);
                }
                BoundingBoxDrawArgs args = new BoundingBoxDrawArgs();
                Vector4? nullable = color;
                argsPtr1->Color = (nullable != null) ? ((Color) nullable.GetValueOrDefault()) : Vector4.One;
                argsPtr1 = (BoundingBoxDrawArgs*) ref args;
                args.LineWidth = lineWidth;
                Vector3? nullable2 = inflateAmount;
                argsPtr2->InflateAmount = (nullable2 != null) ? nullable2.GetValueOrDefault() : Vector3.Zero;
                argsPtr2 = (BoundingBoxDrawArgs*) ref args;
                MyStringId? nullable3 = lineMaterial;
                argsPtr3->lineMaterial = (nullable3 != null) ? nullable3.GetValueOrDefault() : GIZMO_LINE_MATERIAL;
                argsPtr3 = (BoundingBoxDrawArgs*) ref args;
                m_entitiesForBBoxDraw[entity] = args;
            }
        }

        public static bool EntityExists(long entityId) => 
            MyEntityIdentifier.ExistsById(entityId);

        public static bool EntityExists(string name) => 
            m_entityNameDictionary.ContainsKey(name);

        private static void entityForBBoxDraw_OnClose(MyEntity entity)
        {
            m_entitiesForBBoxDraw.Remove(entity, false);
        }

        public static bool Exist(MyEntity entity) => 
            ((m_entities != null) ? m_entities.Contains(entity) : false);

        public static Vector3D? FindFreePlace(Vector3D basePos, float radius, int maxTestCount = 20, int testsPerDistance = 5, float stepSize = 1f)
        {
            Vector3D? nullable;
            Vector3D pos = basePos;
            Quaternion identity = Quaternion.Identity;
            HkShape shape = (HkShape) new HkSphereShape(radius);
            try
            {
                int num;
                float num2;
                int num3;
                if (!IsInsideWorld(pos))
                {
                    goto TR_0018;
                }
                else if (IsShapePenetrating(shape, ref pos, ref identity, 15))
                {
                    goto TR_0018;
                }
                else
                {
                    BoundingSphereD sphere = new BoundingSphereD(pos, (double) radius);
                    MyVoxelBase overlappingWithSphere = MySession.Static.VoxelMaps.GetOverlappingWithSphere(ref sphere);
                    if (overlappingWithSphere == null)
                    {
                        nullable = new Vector3D?(pos);
                    }
                    else
                    {
                        if (overlappingWithSphere is MyPlanet)
                        {
                            (overlappingWithSphere as MyPlanet).CorrectSpawnLocation(ref basePos, (double) radius);
                        }
                        nullable = new Vector3D?(basePos);
                    }
                }
                return nullable;
            TR_0017:
                while (true)
                {
                    if (num3 < num)
                    {
                        num2 += radius * stepSize;
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 < testsPerDistance)
                            {
                                pos = basePos + (MyUtils.GetRandomVector3Normalized() * num2);
                                if (IsInsideWorld(pos) && !IsShapePenetrating(shape, ref pos, ref identity, 15))
                                {
                                    BoundingSphereD sphere = new BoundingSphereD(pos, (double) radius);
                                    MyVoxelBase overlappingWithSphere = MySession.Static.VoxelMaps.GetOverlappingWithSphere(ref sphere);
                                    if (overlappingWithSphere == null)
                                    {
                                        nullable = new Vector3D?(pos);
                                        break;
                                    }
                                    if (overlappingWithSphere is MyPlanet)
                                    {
                                        (overlappingWithSphere as MyPlanet).CorrectSpawnLocation(ref basePos, (double) radius);
                                    }
                                }
                                num4++;
                                continue;
                            }
                            num3++;
                        }
                    }
                    else
                    {
                        nullable = null;
                    }
                    break;
                }
                return nullable;
            TR_0018:
                num = (int) Math.Ceiling((double) (((float) maxTestCount) / ((float) testsPerDistance)));
                num2 = 0f;
                num3 = 0;
                goto TR_0017;
            }
            finally
            {
                shape.RemoveReference();
            }
            return nullable;
        }

        public static Vector3D? FindFreePlace(ref MatrixD matrix, Vector3 axis, float radius, int maxTestCount = 20, int testsPerDistance = 5, float stepSize = 1f)
        {
            Vector3D? nullable;
            Vector3 forward = (Vector3) matrix.Forward;
            forward.Normalize();
            Vector3D translation = matrix.Translation;
            Quaternion identity = Quaternion.Identity;
            HkShape shape = (HkShape) new HkSphereShape(radius);
            try
            {
                int num;
                float num2;
                float num3;
                int num4;
                Vector3D vectord2;
                float num5;
                int num6;
                if (!IsInsideWorld(translation))
                {
                    goto TR_0012;
                }
                else if (IsShapePenetrating(shape, ref translation, ref identity, 15))
                {
                    goto TR_0012;
                }
                else if (!FindFreePlaceVoxelMap(translation, radius, ref shape, ref translation))
                {
                    goto TR_0012;
                }
                else
                {
                    nullable = new Vector3D?(translation);
                }
                return nullable;
            TR_000E:
                while (true)
                {
                    if (num6 < testsPerDistance)
                    {
                        if (num6 != 0)
                        {
                            num5 += num2;
                            Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, num5);
                            vectord2 = Vector3D.Transform(forward, rotation);
                        }
                        translation = matrix.Translation + (vectord2 * num3);
                        if (!IsInsideWorld(translation))
                        {
                            break;
                        }
                        if (IsShapePenetrating(shape, ref translation, ref identity, 15))
                        {
                            break;
                        }
                        if (!FindFreePlaceVoxelMap(translation, radius, ref shape, ref translation))
                        {
                            break;
                        }
                        return new Vector3D?(translation);
                    }
                    else
                    {
                        num4++;
                        goto TR_0011;
                    }
                    break;
                }
                num6++;
                goto TR_000E;
            TR_0011:
                while (true)
                {
                    if (num4 < num)
                    {
                        num3 += radius * stepSize;
                        vectord2 = forward;
                        num5 = 0f;
                        num6 = 0;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                }
                goto TR_000E;
            TR_0012:
                num = (int) Math.Ceiling((double) (((float) maxTestCount) / ((float) testsPerDistance)));
                num2 = 6.283185f / ((float) testsPerDistance);
                num3 = 0f;
                num4 = 0;
                goto TR_0011;
            }
            finally
            {
                shape.RemoveReference();
            }
            return nullable;
        }

        private static bool FindFreePlaceVoxelMap(Vector3D currentPos, float radius, ref HkShape shape, ref Vector3D ret)
        {
            BoundingSphereD sphere = new BoundingSphereD(currentPos, (double) radius);
            MyVoxelBase overlappingWithSphere = MySession.Static.VoxelMaps.GetOverlappingWithSphere(ref sphere);
            overlappingWithSphere = overlappingWithSphere?.RootVoxel;
            if (overlappingWithSphere == null)
            {
                ret = currentPos;
                return true;
            }
            MyPlanet planet = overlappingWithSphere as MyPlanet;
            if (planet != null)
            {
                Quaternion identity = Quaternion.Identity;
                if (planet.CorrectSpawnLocation2(ref currentPos, (double) radius, false))
                {
                    if (!IsShapePenetrating(shape, ref currentPos, ref identity, 15))
                    {
                        ret = currentPos;
                        return true;
                    }
                    if (planet.CorrectSpawnLocation2(ref currentPos, (double) radius, true) && !IsShapePenetrating(shape, ref currentPos, ref identity, 15))
                    {
                        ret = currentPos;
                        return true;
                    }
                }
            }
            return false;
        }

        [Event(null, 0xb01), Reliable, Broadcast]
        public static void ForceCloseEntityOnClients(long entityId)
        {
            MyEntity entity;
            TryGetEntityById(entityId, out entity, false);
            if ((entity != null) && !entity.MarkedForClose)
            {
                entity.Close();
            }
        }

        public static void GetElementsInBox(ref BoundingBoxD boundingBox, List<MyEntity> foundElements)
        {
            MyGamePruningStructure.GetAllEntitiesInBox(ref boundingBox, foundElements, MyEntityQueryType.Both);
        }

        public static MyConcurrentHashSet<MyEntity> GetEntities() => 
            m_entities;

        public static List<MyEntity> GetEntitiesInAABB(ref BoundingBox boundingBox)
        {
            MyGamePruningStructure.GetAllEntitiesInBox(ref boundingBox, OverlapRBElementList, MyEntityQueryType.Both);
            return OverlapRBElementList;
        }

        public static List<MyEntity> GetEntitiesInAABB(ref BoundingBoxD boundingBox, bool exact = false)
        {
            MyGamePruningStructure.GetAllEntitiesInBox(ref boundingBox, OverlapRBElementList, MyEntityQueryType.Both);
            if (exact)
            {
                int index = 0;
                while (index < OverlapRBElementList.Count)
                {
                    MyEntity entity = OverlapRBElementList[index];
                    if (!boundingBox.Intersects(entity.PositionComp.WorldAABB))
                    {
                        OverlapRBElementList.RemoveAt(index);
                        continue;
                    }
                    index++;
                }
            }
            return OverlapRBElementList;
        }

        public static List<MyEntity> GetEntitiesInOBB(ref MyOrientedBoundingBoxD obb)
        {
            MyGamePruningStructure.GetAllEntitiesInOBB(ref obb, OverlapRBElementList, MyEntityQueryType.Both);
            return OverlapRBElementList;
        }

        public static List<MyEntity> GetEntitiesInSphere(ref BoundingSphereD boundingSphere)
        {
            MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphere, OverlapRBElementList, MyEntityQueryType.Both);
            return OverlapRBElementList;
        }

        public static MyEntity GetEntityById(long entityId, bool allowClosed = false) => 
            (MyEntityIdentifier.GetEntityById(entityId, allowClosed) as MyEntity);

        public static MyEntity GetEntityByIdOrDefault(long entityId, MyEntity defaultValue = null, bool allowClosed = false)
        {
            IMyEntity entity;
            MyEntityIdentifier.TryGetEntity(entityId, out entity, allowClosed);
            MyEntity entity1 = entity as MyEntity;
            return (entity1 ?? defaultValue);
        }

        public static T GetEntityByIdOrDefault<T>(long entityId, T defaultValue = null, bool allowClosed = false) where T: MyEntity
        {
            IMyEntity entity;
            MyEntityIdentifier.TryGetEntity(entityId, out entity, allowClosed);
            T local1 = entity as T;
            T local3 = local1;
            if (local1 == null)
            {
                T local2 = local1;
                local3 = defaultValue;
            }
            return local3;
        }

        public static MyEntity GetEntityByName(string name) => 
            m_entityNameDictionary[name];

        public static IMyEntity GetEntityFromRenderObjectID(uint renderObjectID)
        {
            using (m_renderObjectToEntityMapLock.AcquireSharedUsing())
            {
                IMyEntity entity = null;
                m_renderObjectToEntityMap.TryGetValue(renderObjectID, out entity);
                return entity;
            }
        }

        public static void GetInflatedPlayerBoundingBox(ref BoundingBoxD playerBox, float inflation)
        {
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                playerBox.Include(player.GetPosition());
            }
            playerBox.Inflate((double) inflation);
        }

        public static MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(ref LineD line, MyEntity ignoreEntity0, MyEntity ignoreEntity1, bool ignoreChildren = false, bool ignoreFloatingObjects = true, bool ignoreHandWeapons = true, IntersectionFlags flags = 3, float timeFrame = 0f, bool ignoreObjectsWithoutPhysics = true)
        {
            EntityResultSet.Clear();
            if (ignoreChildren)
            {
                if (ignoreEntity0 != null)
                {
                    ignoreEntity0 = ignoreEntity0.GetBaseEntity();
                    ignoreEntity0.Hierarchy.GetChildrenRecursive(EntityResultSet);
                }
                if (ignoreEntity1 != null)
                {
                    ignoreEntity1 = ignoreEntity1.GetBaseEntity();
                    ignoreEntity1.Hierarchy.GetChildrenRecursive(EntityResultSet);
                }
            }
            LineOverlapEntityList.Clear();
            MyGamePruningStructure.GetAllEntitiesInRay(ref line, LineOverlapEntityList, MyEntityQueryType.Both);
            LineOverlapEntityList.Sort(MyLineSegmentOverlapResult<MyEntity>.DistanceComparer);
            MyIntersectionResultLineTriangleEx? a = null;
            RayD ray = new RayD(line.From, line.Direction);
            foreach (MyLineSegmentOverlapResult<MyEntity> result in LineOverlapEntityList)
            {
                if (a != null)
                {
                    double? nullable3 = result.Element.PositionComp.WorldAABB.Intersects(ray);
                    if ((nullable3 != null) && (Vector3D.DistanceSquared(line.From, a.Value.IntersectionPointInWorldSpace) < (nullable3.Value * nullable3.Value)))
                    {
                        break;
                    }
                }
                MyEntity element = result.Element;
                if ((!ReferenceEquals(element, ignoreEntity0) && (!ReferenceEquals(element, ignoreEntity1) && ((!ignoreChildren || !EntityResultSet.Contains(element)) && ((!ignoreObjectsWithoutPhysics || ((element.Physics != null) && element.Physics.Enabled)) && (!element.MarkedForClose && (!ignoreFloatingObjects || (!(element is MyFloatingObject) && !(element is MyDebrisBase)))))))) && (!ignoreHandWeapons || (!(element is IMyHandheldGunObject<MyDeviceBase>) && !(element.Parent is IMyHandheldGunObject<MyDeviceBase>))))
                {
                    MyIntersectionResultLineTriangleEx? t = null;
                    if (((timeFrame == 0f) || ((element.Physics == null) || (element.Physics.LinearVelocity.LengthSquared() < 0.1f))) || !element.IsCCDForProjectiles)
                    {
                        element.GetIntersectionWithLine(ref line, out t, flags);
                    }
                    else
                    {
                        float num2 = element.Physics.LinearVelocity.Length() * timeFrame;
                        float radius = element.PositionComp.LocalVolume.Radius;
                        float num4 = 0f;
                        Vector3D position = element.PositionComp.GetPosition();
                        Vector3 vector2 = Vector3.Normalize(element.Physics.LinearVelocity);
                        while (true)
                        {
                            if ((t != null) || (num4 >= num2))
                            {
                                element.PositionComp.SetPosition(position, null, false, true);
                                break;
                            }
                            element.PositionComp.SetPosition(position + (num4 * vector2), null, false, true);
                            element.GetIntersectionWithLine(ref line, out t, flags);
                            num4 += radius;
                        }
                    }
                    if (((t != null) && (!ReferenceEquals(t.Value.Entity, ignoreEntity0) && !ReferenceEquals(t.Value.Entity, ignoreEntity1))) && (!ignoreChildren || !EntityResultSet.Contains(t.Value.Entity)))
                    {
                        a = MyIntersectionResultLineTriangleEx.GetCloserIntersection(ref a, ref t);
                    }
                }
            }
            LineOverlapEntityList.Clear();
            return a;
        }

        public static MyEntity GetIntersectionWithSphere(ref BoundingSphereD sphere) => 
            GetIntersectionWithSphere(ref sphere, null, null, false, false, false, true, true);

        public static MyEntity GetIntersectionWithSphere(ref BoundingSphereD sphere, MyEntity ignoreEntity0, MyEntity ignoreEntity1) => 
            GetIntersectionWithSphere(ref sphere, ignoreEntity0, ignoreEntity1, false, true, false, true, true);

        public static void GetIntersectionWithSphere(ref BoundingSphereD sphere, MyEntity ignoreEntity0, MyEntity ignoreEntity1, bool ignoreVoxelMaps, bool volumetricTest, ref List<MyEntity> result)
        {
            List<MyEntity> entitiesInAABB = GetEntitiesInAABB(ref BoundingBoxD.CreateInvalid().Include((BoundingSphereD) sphere), false);
            foreach (MyEntity entity in entitiesInAABB)
            {
                if ((!ignoreVoxelMaps || !(entity is MyVoxelMap)) && (!ReferenceEquals(entity, ignoreEntity0) && !ReferenceEquals(entity, ignoreEntity1)))
                {
                    if (entity.GetIntersectionWithSphere(ref sphere))
                    {
                        result.Add(entity);
                    }
                    if ((volumetricTest && (entity is MyVoxelMap)) && (entity as MyVoxelMap).DoOverlapSphereTest((float) sphere.Radius, sphere.Center))
                    {
                        result.Add(entity);
                    }
                }
            }
            entitiesInAABB.Clear();
        }

        public static MyEntity GetIntersectionWithSphere(ref BoundingSphereD sphere, MyEntity ignoreEntity0, MyEntity ignoreEntity1, bool ignoreVoxelMaps, bool volumetricTest, bool excludeEntitiesWithDisabledPhysics = false, bool ignoreFloatingObjects = true, bool ignoreHandWeapons = true)
        {
            BoundingBoxD boundingBox = BoundingBoxD.CreateInvalid().Include((BoundingSphereD) sphere);
            MyEntity entity = null;
            List<MyEntity> entitiesInAABB = GetEntitiesInAABB(ref boundingBox, false);
            foreach (MyEntity entity2 in entitiesInAABB)
            {
                if ((!ignoreVoxelMaps || !(entity2 is MyVoxelMap)) && ((!ReferenceEquals(entity2, ignoreEntity0) && (!ReferenceEquals(entity2, ignoreEntity1) && (((!excludeEntitiesWithDisabledPhysics || (entity2.Physics == null)) || entity2.Physics.Enabled) && (!ignoreFloatingObjects || (!(entity2 is MyFloatingObject) && !(entity2 is MyDebrisBase)))))) && (!ignoreHandWeapons || (!(entity2 is IMyHandheldGunObject<MyDeviceBase>) && !(entity2.Parent is IMyHandheldGunObject<MyDeviceBase>)))))
                {
                    if ((volumetricTest && entity2.IsVolumetric) && entity2.DoOverlapSphereTest((float) sphere.Radius, sphere.Center))
                    {
                        entity = entity2;
                        break;
                    }
                    if (entity2.GetIntersectionWithSphere(ref sphere))
                    {
                        entity = entity2;
                        break;
                    }
                }
            }
            entitiesInAABB.Clear();
            return entity;
        }

        public static void GetTopMostEntitiesInBox(ref BoundingBoxD boundingBox, List<MyEntity> foundElements, MyEntityQueryType qtype = 3)
        {
            MyGamePruningStructure.GetAllTopMostStaticEntitiesInBox(ref boundingBox, foundElements, qtype);
        }

        public static List<MyEntity> GetTopMostEntitiesInSphere(ref BoundingSphereD boundingSphere)
        {
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref boundingSphere, OverlapRBElementList, MyEntityQueryType.Both);
            return OverlapRBElementList;
        }

        public static bool HasEntitiesToDelete() => 
            (m_entitiesToDelete.Count > 0);

        public static void InitAsync(MyEntity entity, MyObjectBuilder_EntityBase objectBuilder, bool addToScene, Action<MyEntity> doneHandler = null, byte islandIndex = 0, double serializationTimestamp = 0.0, bool fadeIn = false)
        {
            if (m_creationThread != null)
            {
                m_creationThread.SubmitWork(objectBuilder, addToScene, doneHandler, entity, islandIndex, serializationTimestamp, fadeIn);
            }
        }

        public static void InitEntity(MyObjectBuilder_EntityBase objectBuilder, ref MyEntity entity)
        {
            if (entity != null)
            {
                try
                {
                    entity.Init(objectBuilder);
                }
                catch (Exception exception)
                {
                    MySandboxGame.Log.WriteLine("ERROR Entity init!: " + exception);
                    entity.EntityId = 0L;
                    entity = null;
                }
            }
        }

        private static bool IsAnyRenderObjectVisible(MyEntity entity)
        {
            foreach (uint num2 in entity.Render.RenderObjectIDs)
            {
                if (MyRenderProxy.VisibleObjectsRead.Contains(num2))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsCloseAllowed() => 
            CloseAllowed;

        public static bool IsEntityIdValid(long entityId)
        {
            MyEntity entityById = MyEntityIdentifier.GetEntityById(entityId, true) as MyEntity;
            return ((entityById != null) && !entityById.GetTopMostParent(null).MarkedForClose);
        }

        public static bool IsInsideVoxel(Vector3D pos, Vector3D hintPosition, out Vector3D lastOutsidePos)
        {
            m_hits.Clear();
            lastOutsidePos = pos;
            MyPhysics.CastRay(hintPosition, pos, m_hits, 15);
            int num = 0;
            foreach (MyPhysics.HitInfo info in m_hits)
            {
                if (info.HkHitInfo.GetHitEntity() is MyVoxelMap)
                {
                    num++;
                    lastOutsidePos = info.Position;
                }
            }
            m_hits.Clear();
            return ((num % 2) != 0);
        }

        public static bool IsInsideWorld(Vector3D pos)
        {
            float num = WorldHalfExtent();
            return ((num != 0f) ? (pos.AbsMax() <= num) : true);
        }

        public static bool IsNameExists(MyEntity entity, string name)
        {
            using (IEnumerator<KeyValuePair<string, MyEntity>> enumerator = m_entityNameDictionary.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<string, MyEntity> current = enumerator.Current;
                    if ((current.Key == name) && (current.Value != entity))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsRaycastBlocked(Vector3D pos, Vector3D target)
        {
            m_hits.Clear();
            MyPhysics.CastRay(pos, target, m_hits, 0);
            return (m_hits.Count > 0);
        }

        public static bool IsShapePenetrating(HkShape shape, ref Vector3D position, ref Quaternion rotation, int filter = 15)
        {
            using (MyUtils.ReuseCollection<HkBodyCollision>(ref m_rigidBodyList))
            {
                MyPhysics.GetPenetrationsShape(shape, ref position, ref rotation, m_rigidBodyList, filter);
                return (m_rigidBodyList.Count > 0);
            }
        }

        public static bool IsSpherePenetrating(ref BoundingSphereD bs) => 
            IsShapePenetrating(m_cameraSphere, ref bs.Center, ref Quaternion.Identity, 15);

        public static bool IsTypeHidden(System.Type type)
        {
            using (HashSet<System.Type>.Enumerator enumerator = m_hiddenTypes.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsUpdateInProgress() => 
            UpdateInProgress;

        public static bool IsVisible(IMyEntity entity) => 
            !IsTypeHidden(entity.GetType());

        public static bool IsWorldLimited() => 
            ((MySession.Static != null) && (MySession.Static.Settings.WorldSizeKm != 0));

        public static bool Load(List<MyObjectBuilder_EntityBase> objectBuilders)
        {
            MyEntityIdentifier.AllocationSuspended = true;
            bool allEntitiesAdded = true;
            InitEntityData[] results = null;
            try
            {
                if (objectBuilders != null)
                {
                    results = new InitEntityData[objectBuilders.Count];
                    if (MySandboxGame.Config.SyncRendering)
                    {
                        MyEntityIdentifier.PrepareSwapData();
                        MyEntityIdentifier.SwapPerThreadData();
                    }
                    WorkOptions? options = null;
                    Parallel.For(0, objectBuilders.Count, delegate (int i) {
                        allEntitiesAdded &= LoadEntity(i, results, objectBuilders);
                    }, WorkPriority.Normal, options);
                    if (MySandboxGame.Config.SyncRendering)
                    {
                        MyEntityIdentifier.ClearSwapDataAndRestore();
                    }
                }
            }
            finally
            {
                MyEntityIdentifier.AllocationSuspended = false;
            }
            if (results != null)
            {
                MyEntityIdentifier.InEntityCreationBlock = true;
                InitEntityData[] dataArray = results;
                int index = 0;
                while (true)
                {
                    if (index >= dataArray.Length)
                    {
                        MyEntityIdentifier.InEntityCreationBlock = false;
                        break;
                    }
                    InitEntityData data = dataArray[index];
                    if (data != null)
                    {
                        data.OnEntityInitialized();
                    }
                    index++;
                }
            }
            return allEntitiesAdded;
        }

        public static void LoadData()
        {
            m_entities.Clear();
            m_entitiesToDelete.Clear();
            m_entitiesToDeleteNextFrame.Clear();
            m_cameraSphere = (HkShape) new HkSphereShape(0.125f);
            AddComponents();
            using (List<IMySceneComponent>.Enumerator enumerator = m_sceneComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Load();
                }
            }
            m_creationThread = new MyEntityCreationThread();
            m_isLoaded = true;
        }

        private static bool LoadEntity(int i, InitEntityData[] results, List<MyObjectBuilder_EntityBase> objectBuilders)
        {
            MyObjectBuilder_EntityBase objectBuilder = objectBuilders[i];
            MyObjectBuilder_Character character = objectBuilder as MyObjectBuilder_Character;
            if (((character != null) && ((MyMultiplayer.Static != null) && Sync.IsServer)) && !character.IsPersistenceCharacter)
            {
                return true;
            }
            if ((MyFakes.SKIP_VOXELS_DURING_LOAD && (objectBuilder.TypeId == typeof(MyObjectBuilder_VoxelMap))) && ((objectBuilder as MyObjectBuilder_VoxelMap).StorageName != "BaseAsteroid"))
            {
                return true;
            }
            bool flag = true;
            MyEntity entity = CreateFromObjectBuilderNoinit(objectBuilder);
            if (entity == null)
            {
                flag = false;
            }
            else
            {
                Vector3D? relativeOffset = null;
                InitEntityData data = new InitEntityData(objectBuilder, true, null, entity, false, null, relativeOffset, false);
                if (data.CallInitEntity())
                {
                    results[i] = data;
                }
            }
            return flag;
        }

        public static void MemoryLimitAddFailureReset()
        {
            MemoryLimitAddFailure = false;
        }

        [Event(null, 0xaf0), Reliable, Server]
        private static void OnEntityCloseRequest(long entityId)
        {
            if ((!MyEventContext.Current.IsLocallyInvoked && (!MySession.Static.IsCopyPastingEnabledForUser(MyEventContext.Current.Sender.Value) && !MySession.Static.CreativeMode)) && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyEntity entity;
                TryGetEntityById(entityId, out entity, false);
                if ((entity != null) && !entity.MarkedForClose)
                {
                    entity.Close();
                }
            }
        }

        private static void OnEntityInitialized(WorkData workData)
        {
            InitEntityData data = workData as InitEntityData;
            if (data == null)
            {
                workData.FlagAsFailed();
            }
            else
            {
                data.OnEntityInitialized();
            }
        }

        public static void OverlapAllLineSegment(ref LineD line, List<MyLineSegmentOverlapResult<MyEntity>> resultList)
        {
            MyGamePruningStructure.GetAllEntitiesInRay(ref line, resultList, MyEntityQueryType.Both);
        }

        public static void RaiseEntityAdd(MyEntity entity)
        {
            if (OnEntityAdd != null)
            {
                OnEntityAdd(entity);
            }
        }

        public static void RaiseEntityCreated(MyEntity entity)
        {
            Action<MyEntity> onEntityCreate = OnEntityCreate;
            if (onEntityCreate != null)
            {
                onEntityCreate(entity);
            }
        }

        public static void RaiseEntityRemove(MyEntity entity)
        {
            if (OnEntityRemove != null)
            {
                OnEntityRemove(entity);
            }
        }

        public static void RegisterForDraw(IMyEntity entity)
        {
            if (entity.Render.NeedsDraw)
            {
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    m_entitiesForDraw.Add(entity);
                }
                entity.Render.SetVisibilityUpdates(true);
            }
        }

        public static void RegisterForUpdate(MyEntity entity)
        {
            if ((entity.NeedsUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) > MyEntityUpdateEnum.NONE)
            {
                m_entitiesForUpdateOnce.Add(entity);
            }
            if ((entity.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) > MyEntityUpdateEnum.NONE)
            {
                m_entitiesForUpdate.List.Add(entity);
            }
            if ((entity.NeedsUpdate & MyEntityUpdateEnum.EACH_10TH_FRAME) > MyEntityUpdateEnum.NONE)
            {
                m_entitiesForUpdate10.List.Add(entity);
            }
            if ((entity.NeedsUpdate & MyEntityUpdateEnum.EACH_100TH_FRAME) > MyEntityUpdateEnum.NONE)
            {
                m_entitiesForUpdate100.List.Add(entity);
            }
            if ((entity.NeedsUpdate & MyEntityUpdateEnum.SIMULATE) > MyEntityUpdateEnum.NONE)
            {
                m_entitiesForSimulate.List.Add(entity);
            }
        }

        public static void ReleaseWaitingAsync(byte index, Dictionary<long, MatrixD> matrices)
        {
            m_creationThread.ReleaseWaiting(index, matrices);
        }

        public static void RemapObjectBuilder(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (m_remapHelper == null)
            {
                m_remapHelper = new MyEntityIdRemapHelper();
            }
            objectBuilder.Remap(m_remapHelper);
            m_remapHelper.Clear();
        }

        public static void RemapObjectBuilderCollection(IEnumerable<MyObjectBuilder_EntityBase> objectBuilders)
        {
            if (m_remapHelper == null)
            {
                m_remapHelper = new MyEntityIdRemapHelper();
            }
            using (IEnumerator<MyObjectBuilder_EntityBase> enumerator = objectBuilders.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Remap(m_remapHelper);
                }
            }
            m_remapHelper.Clear();
        }

        public static bool Remove(MyEntity entity)
        {
            if (entity is MyVoxelBase)
            {
                MySession.Static.VoxelMaps.RemoveVoxelMap((MyVoxelBase) entity);
            }
            if (!m_entities.Remove(entity))
            {
                return false;
            }
            entity.OnRemovedFromScene(entity);
            RaiseEntityRemove(entity);
            return true;
        }

        public static void RemoveFromClosedEntities(MyEntity entity)
        {
            if (m_entitiesToDelete.Count > 0)
            {
                m_entitiesToDelete.Remove(entity);
            }
            if (m_entitiesToDeleteNextFrame.Count > 0)
            {
                m_entitiesToDeleteNextFrame.Remove(entity);
            }
        }

        public static void RemoveName(MyEntity entity)
        {
            if (!string.IsNullOrEmpty(entity.Name))
            {
                m_entityNameDictionary.Remove<string, MyEntity>(entity.Name);
            }
        }

        public static void RemoveRenderObjectFromMap(uint id)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                using (m_renderObjectToEntityMapLock.AcquireExclusiveUsing())
                {
                    m_renderObjectToEntityMap.Remove(id);
                }
            }
        }

        internal static List<MyObjectBuilder_EntityBase> Save()
        {
            List<MyObjectBuilder_EntityBase> list = new List<MyObjectBuilder_EntityBase>();
            foreach (MyEntity entity in m_entities)
            {
                if (!entity.Save)
                {
                    continue;
                }
                if (!m_entitiesToDelete.Contains(entity) && !entity.MarkedForClose)
                {
                    entity.BeforeSave();
                    list.Add(entity.GetObjectBuilder(false));
                }
            }
            return list;
        }

        public static void SendCloseRequest(IMyEntity entity)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyEntities.OnEntityCloseRequest), entity.EntityId, targetEndpoint, position);
            IMyEventProxy proxy = entity as IMyEventProxy;
            if (((MyMultiplayer.Static != null) && !Sync.IsServer) && ((proxy == null) || (MyMultiplayer.Static.ReplicationLayer.GetProxyTarget(proxy) == null)))
            {
                entity.Close();
            }
        }

        public static void SetEntityName(MyEntity myEntity, bool possibleRename = true)
        {
            string key = null;
            string name = myEntity.Name;
            if (possibleRename)
            {
                foreach (KeyValuePair<string, MyEntity> pair in m_entityNameDictionary)
                {
                    if (pair.Value == myEntity)
                    {
                        m_entityNameDictionary.Remove<string, MyEntity>(pair.Key);
                        key = pair.Key;
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(myEntity.Name) && !m_entityNameDictionary.ContainsKey(myEntity.Name))
            {
                m_entityNameDictionary.TryAdd(myEntity.Name, myEntity);
            }
            if (OnEntityNameSet != null)
            {
                OnEntityNameSet(myEntity, key, name);
            }
        }

        public static void SetTypeHidden(System.Type type, bool hidden)
        {
            if (hidden != m_hiddenTypes.Contains(type))
            {
                if (hidden)
                {
                    m_hiddenTypes.Add(type);
                }
                else
                {
                    m_hiddenTypes.Remove(type);
                }
            }
        }

        public static void Simulate()
        {
            if (MySandboxGame.IsGameReady)
            {
                UpdateInProgress = true;
                m_entitiesForSimulate.List.ApplyChanges();
                m_entitiesForSimulate.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.Simulate();
                    }
                });
                UpdateInProgress = false;
            }
        }

        public static Vector3D? TestPlaceInSpace(Vector3D basePos, float radius)
        {
            Vector3D? nullable;
            List<MyVoxelBase> voxels = new List<MyVoxelBase>();
            Vector3D pos = basePos;
            Quaternion identity = Quaternion.Identity;
            HkShape shape = (HkShape) new HkSphereShape(radius);
            try
            {
                if (IsInsideWorld(pos) && !IsShapePenetrating(shape, ref pos, ref identity, 15))
                {
                    BoundingSphereD sphere = new BoundingSphereD(pos, (double) radius);
                    MySession.Static.VoxelMaps.GetAllOverlappingWithSphere(ref sphere, voxels);
                    if (voxels.Count != 0)
                    {
                        bool flag = true;
                        foreach (MyPlanet planet in voxels)
                        {
                            if (planet == null)
                            {
                                flag = false;
                            }
                            else
                            {
                                if ((pos - planet.MaximumRadius).Length() >= planet.MaximumRadius)
                                {
                                    continue;
                                }
                                flag = false;
                            }
                            break;
                        }
                        if (flag)
                        {
                            return new Vector3D?(pos);
                        }
                    }
                    else
                    {
                        return new Vector3D?(pos);
                    }
                }
                nullable = null;
            }
            finally
            {
                shape.RemoveReference();
            }
            return nullable;
        }

        public static bool TryGetEntityById(long entityId, out MyEntity entity, bool allowClosed = false) => 
            MyEntityIdentifier.TryGetEntity<MyEntity>(entityId, out entity, allowClosed);

        public static bool TryGetEntityById<T>(long entityId, out T entity, bool allowClosed = false) where T: MyEntity
        {
            MyEntity entity2;
            entity = entity2 as T;
            return (MyEntityIdentifier.TryGetEntity<MyEntity>(entityId, out entity2, allowClosed) && (entity2 is T));
        }

        public static bool TryGetEntityByName(string name, out MyEntity entity) => 
            m_entityNameDictionary.TryGetValue(name, out entity);

        public static void UnhideAllTypes()
        {
            using (List<System.Type>.Enumerator enumerator = m_hiddenTypes.ToList<System.Type>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    SetTypeHidden(enumerator.Current, false);
                }
            }
        }

        public static void UnloadData()
        {
            if (m_isLoaded)
            {
                m_cameraSphere.RemoveReference();
            }
            using (UnloadDataLock.AcquireExclusiveUsing())
            {
                List<List<MyEntity>>.Enumerator enumerator;
                m_creationThread.Dispose();
                m_creationThread = null;
                CloseAll();
                m_overlapRBElementList = null;
                m_entityResultSet = null;
                m_isLoaded = false;
                List<List<MyEntity>> entityInputListCollection = m_entityInputListCollection;
                lock (entityInputListCollection)
                {
                    using (enumerator = m_entityInputListCollection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Clear();
                        }
                    }
                }
                entityInputListCollection = m_overlapRBElementListCollection;
                lock (entityInputListCollection)
                {
                    using (enumerator = m_overlapRBElementListCollection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Clear();
                        }
                    }
                }
                List<HashSet<IMyEntity>> entityResultSetCollection = m_entityResultSetCollection;
                lock (entityResultSetCollection)
                {
                    using (List<HashSet<IMyEntity>>.Enumerator enumerator2 = m_entityResultSetCollection.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.Clear();
                        }
                    }
                }
                entityInputListCollection = m_allIgnoredEntitiesCollection;
                lock (entityInputListCollection)
                {
                    using (enumerator = m_allIgnoredEntitiesCollection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Clear();
                        }
                    }
                }
            }
            for (int i = m_sceneComponents.Count - 1; i >= 0; i--)
            {
                m_sceneComponents[i].Unload();
            }
            m_sceneComponents.Clear();
            OnEntityRemove = null;
            OnEntityAdd = null;
            OnEntityCreate = null;
            OnEntityDelete = null;
            m_entities = new MyConcurrentHashSet<MyEntity>();
            m_entitiesForUpdateOnce = new CachingList<MyEntity>();
            m_entitiesForUpdate = new MyDistributedUpdater<ConcurrentCachingList<MyEntity>, MyEntity>(1);
            m_entitiesForUpdate10 = new MyDistributedUpdater<CachingList<MyEntity>, MyEntity>(10);
            m_entitiesForUpdate100 = new MyDistributedUpdater<CachingList<MyEntity>, MyEntity>(100);
            m_entitiesForDraw = new CachingList<IMyEntity>();
            m_remapHelper = new MyEntityIdRemapHelper();
            m_renderObjectToEntityMap = new Dictionary<uint, IMyEntity>();
            m_entityNameDictionary.Clear();
            m_entitiesForBBoxDraw.Clear();
        }

        public static void UnregisterForDraw(IMyEntity entity)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                m_entitiesForDraw.Remove(entity, false);
            }
            entity.Render.SetVisibilityUpdates(false);
        }

        public static void UnregisterForUpdate(MyEntity entity, bool immediate = false)
        {
            if ((entity.Flags & EntityFlags.NeedsUpdateBeforeNextFrame) != 0)
            {
                m_entitiesForUpdateOnce.Remove(entity, immediate);
            }
            if ((entity.Flags & EntityFlags.NeedsUpdate) != 0)
            {
                m_entitiesForUpdate.List.Remove(entity, immediate);
            }
            if ((entity.Flags & EntityFlags.NeedsUpdate10) != 0)
            {
                m_entitiesForUpdate10.List.Remove(entity, immediate);
            }
            if ((entity.Flags & EntityFlags.NeedsUpdate100) != 0)
            {
                m_entitiesForUpdate100.List.Remove(entity, immediate);
            }
            if ((entity.Flags & EntityFlags.NeedsSimulate) != 0)
            {
                m_entitiesForSimulate.List.Remove(entity, immediate);
            }
        }

        public static void UpdateAfterSimulation()
        {
            if (MySandboxGame.IsGameReady)
            {
                UpdateInProgress = true;
                m_entitiesForUpdate.List.ApplyChanges();
                MySimpleProfiler.Begin("Blocks", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateAfterSimulation");
                m_entitiesForUpdate.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.UpdateAfterSimulation();
                    }
                });
                m_entitiesForUpdate10.List.ApplyChanges();
                m_entitiesForUpdate10.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.UpdateAfterSimulation10();
                    }
                });
                m_entitiesForUpdate100.List.ApplyChanges();
                m_entitiesForUpdate100.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.UpdateAfterSimulation100();
                    }
                });
                MySimpleProfiler.End("UpdateAfterSimulation");
                UpdateInProgress = false;
                DeleteRememberedEntities();
                if ((MyMultiplayer.Static != null) && m_creationThread.AnyResult)
                {
                    while (m_creationThread.ConsumeResult(MyMultiplayer.Static.ReplicationLayer.GetSimulationUpdateTime()))
                    {
                    }
                }
            }
        }

        public static void UpdateBeforeSimulation()
        {
            if (MySandboxGame.IsGameReady)
            {
                UpdateInProgress = true;
                UpdateOnceBeforeFrame();
                m_entitiesForUpdate.List.ApplyChanges();
                m_entitiesForUpdate.Update();
                MySimpleProfiler.Begin("Blocks", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation");
                m_entitiesForUpdate.Iterate(delegate (MyEntity x) {
                    if (!x.MarkedForClose)
                    {
                        x.UpdateBeforeSimulation();
                    }
                });
                m_entitiesForUpdate10.List.ApplyChanges();
                m_entitiesForUpdate10.Update();
                m_entitiesForUpdate10.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.UpdateBeforeSimulation10();
                    }
                });
                m_entitiesForUpdate100.List.ApplyChanges();
                m_entitiesForUpdate100.Update();
                m_entitiesForUpdate100.Iterate(delegate (MyEntity x) {
                    string name = x.GetType().Name;
                    if (!x.MarkedForClose)
                    {
                        x.UpdateBeforeSimulation100();
                    }
                });
                MySimpleProfiler.End("UpdateBeforeSimulation");
                UpdateInProgress = false;
            }
        }

        public static void UpdateOnceBeforeFrame()
        {
            m_entitiesForUpdateOnce.ApplyChanges();
            foreach (MyEntity entity in m_entitiesForUpdateOnce)
            {
                entity.NeedsUpdate &= ~MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                if (!entity.MarkedForClose)
                {
                    entity.UpdateOnceBeforeFrame();
                }
            }
        }

        public static void UpdatingStopped()
        {
            for (int i = 0; i < m_entitiesForUpdate.List.Count; i++)
            {
                m_entitiesForUpdate.List[i].UpdatingStopped();
            }
        }

        public static float WorldHalfExtent() => 
            ((MySession.Static == null) ? ((float) 0) : ((float) (MySession.Static.Settings.WorldSizeKm * 500)));

        public static float WorldSafeHalfExtent()
        {
            float num = WorldHalfExtent();
            return ((num == 0f) ? 0f : (num - 600f));
        }

        private static List<MyEntity> OverlapRBElementList
        {
            get
            {
                if (m_overlapRBElementList == null)
                {
                    m_overlapRBElementList = new List<MyEntity>(0x100);
                    List<List<MyEntity>> overlapRBElementListCollection = m_overlapRBElementListCollection;
                    lock (overlapRBElementListCollection)
                    {
                        m_overlapRBElementListCollection.Add(m_overlapRBElementList);
                    }
                }
                return m_overlapRBElementList;
            }
        }

        private static HashSet<IMyEntity> EntityResultSet
        {
            get
            {
                if (m_entityResultSet == null)
                {
                    m_entityResultSet = new HashSet<IMyEntity>();
                    List<HashSet<IMyEntity>> entityResultSetCollection = m_entityResultSetCollection;
                    lock (entityResultSetCollection)
                    {
                        m_entityResultSetCollection.Add(m_entityResultSet);
                    }
                }
                return m_entityResultSet;
            }
        }

        private static List<MyEntity> EntityInputList
        {
            get
            {
                if (m_entityInputList == null)
                {
                    m_entityInputList = new List<MyEntity>(0x20);
                    List<List<MyEntity>> entityInputListCollection = m_entityInputListCollection;
                    lock (entityInputListCollection)
                    {
                        m_entityInputListCollection.Add(m_entityInputList);
                    }
                }
                return m_entityInputList;
            }
        }

        public static bool IsLoaded =>
            m_isLoaded;

        private static List<MyEntity> AllIgnoredEntities
        {
            get
            {
                if (m_allIgnoredEntities == null)
                {
                    m_allIgnoredEntities = new List<MyEntity>();
                    m_allIgnoredEntitiesCollection.Add(m_allIgnoredEntities);
                }
                return m_allIgnoredEntities;
            }
        }

        public static bool MemoryLimitReached =>
            (!VRage.Library.MyEnvironment.Is64BitProcess && (MySandboxGame.Config.MemoryLimits && ((GC.GetTotalMemory(false) > EntityManagedMemoryLimit) || (WinApi.WorkingSet > EntityNativeMemoryLimit))));

        public static bool MemoryLimitReachedReport
        {
            get
            {
                if (!MemoryLimitReached)
                {
                    return false;
                }
                MySandboxGame.Log.WriteLine("Memory limit reached");
                MySandboxGame.Log.WriteLine("GC Memory: " + GC.GetTotalMemory(false).ToString());
                MyHud.Notifications.Add(MyNotificationSingletons.GameOverload);
                return true;
            }
        }

        public static bool MemoryLimitAddFailure
        {
            [CompilerGenerated]
            get => 
                <MemoryLimitAddFailure>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<MemoryLimitAddFailure>k__BackingField = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEntities.<>c <>9 = new MyEntities.<>c();
            public static Action<MyEntity> <>9__109_0;
            public static Action<MyEntity> <>9__109_1;
            public static Action<MyEntity> <>9__109_2;
            public static Action<MyEntity> <>9__111_0;
            public static Action<MyEntity> <>9__112_0;
            public static Action<MyEntity> <>9__112_1;
            public static Action<MyEntity> <>9__112_2;
            public static Func<MyCubeGrid, int> <>9__153_0;
            public static Func<MyCubeGrid, int> <>9__153_1;
            public static Predicate<MyCubeGrid> <>9__153_2;
            public static Func<KeyValuePair<string, int>, int> <>9__154_0;
            public static Func<KeyValuePair<string, int>, int> <>9__154_1;
            public static Func<KeyValuePair<string, int>, int> <>9__154_2;
            public static Func<KeyValuePair<string, int>, int> <>9__154_3;
            public static Func<IMyEventOwner, Action<long>> <>9__196_0;

            internal int <DebugDrawGridStatistics>b__153_0(MyCubeGrid x) => 
                x.BlocksCount;

            internal int <DebugDrawGridStatistics>b__153_1(MyCubeGrid x)
            {
                if (MyGridPhysicalHierarchy.Static.GetNode(x) == null)
                {
                    return 0;
                }
                return MyGridPhysicalHierarchy.Static.GetNode(x).Children.Count;
            }

            internal bool <DebugDrawGridStatistics>b__153_2(MyCubeGrid x)
            {
                if (MyGridPhysicalHierarchy.Static.GetNode(x) != null)
                {
                    return (MyGridPhysicalHierarchy.Static.GetNode(x).Children.Count == 0);
                }
                return true;
            }

            internal int <DebugDrawStatistics>b__154_0(KeyValuePair<string, int> x) => 
                x.Value;

            internal int <DebugDrawStatistics>b__154_1(KeyValuePair<string, int> x) => 
                x.Value;

            internal int <DebugDrawStatistics>b__154_2(KeyValuePair<string, int> x) => 
                x.Value;

            internal int <DebugDrawStatistics>b__154_3(KeyValuePair<string, int> x) => 
                x.Value;

            internal Action<long> <SendCloseRequest>b__196_0(IMyEventOwner s) => 
                new Action<long>(MyEntities.OnEntityCloseRequest);

            internal void <Simulate>b__111_0(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.Simulate();
                }
            }

            internal void <UpdateAfterSimulation>b__112_0(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.UpdateAfterSimulation();
                }
            }

            internal void <UpdateAfterSimulation>b__112_1(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.UpdateAfterSimulation10();
                }
            }

            internal void <UpdateAfterSimulation>b__112_2(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.UpdateAfterSimulation100();
                }
            }

            internal void <UpdateBeforeSimulation>b__109_0(MyEntity x)
            {
                if (!x.MarkedForClose)
                {
                    x.UpdateBeforeSimulation();
                }
            }

            internal void <UpdateBeforeSimulation>b__109_1(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.UpdateBeforeSimulation10();
                }
            }

            internal void <UpdateBeforeSimulation>b__109_2(MyEntity x)
            {
                string name = x.GetType().Name;
                if (!x.MarkedForClose)
                {
                    x.UpdateBeforeSimulation100();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BoundingBoxDrawArgs
        {
            public VRageMath.Color Color;
            public float LineWidth;
            public Vector3 InflateAmount;
            public MyStringId lineMaterial;
        }

        public class InitEntityData : WorkData
        {
            private readonly MyObjectBuilder_EntityBase m_objectBuilder;
            private readonly bool m_addToScene;
            private readonly Action<MyEntity> m_completionCallback;
            private MyEntity m_entity;
            private List<IMyEntity> m_resultIDs;
            private readonly MyEntity m_relativeSpawner;
            private Vector3D? m_relativeOffset;
            private readonly bool m_checkPosition;
            private readonly bool m_fadeIn;

            public InitEntityData(MyObjectBuilder_EntityBase objectBuilder, bool addToScene, Action<MyEntity> completionCallback, MyEntity entity, bool fadeIn, MyEntity relativeSpawner = null, Vector3D? relativeOffset = new Vector3D?(), bool checkPosition = false)
            {
                this.m_objectBuilder = objectBuilder;
                this.m_addToScene = addToScene;
                this.m_completionCallback = completionCallback;
                this.m_entity = entity;
                this.m_fadeIn = fadeIn;
                this.m_relativeSpawner = relativeSpawner;
                this.m_relativeOffset = relativeOffset;
                this.m_checkPosition = checkPosition;
            }

            public bool CallInitEntity()
            {
                bool flag;
                try
                {
                    MyEntityIdentifier.InEntityCreationBlock = true;
                    MyEntityIdentifier.LazyInitPerThreadStorage(0x800);
                    this.m_entity.Render.FadeIn = this.m_fadeIn;
                    MyEntities.InitEntity(this.m_objectBuilder, ref this.m_entity);
                    flag = this.m_entity != null;
                }
                finally
                {
                    this.m_resultIDs = new List<IMyEntity>();
                    MyEntityIdentifier.GetPerThreadEntities(this.m_resultIDs);
                    MyEntityIdentifier.ClearPerThreadEntities();
                    MyEntityIdentifier.InEntityCreationBlock = false;
                }
                return flag;
            }

            public void OnEntityInitialized()
            {
                if ((this.m_relativeSpawner != null) && (this.m_relativeOffset != null))
                {
                    MatrixD worldMatrix = this.m_entity.WorldMatrix;
                    worldMatrix.Translation = this.m_relativeSpawner.WorldMatrix.Translation + this.m_relativeOffset.Value;
                    this.m_entity.WorldMatrix = worldMatrix;
                }
                MyCubeGrid targetGrid = this.m_entity as MyCubeGrid;
                if ((MyFakes.ENABLE_GRID_PLACEMENT_TEST && (this.m_checkPosition && (targetGrid != null))) && (targetGrid.CubeBlocks.Count == 1))
                {
                    MyGridPlacementSettings gridPlacementSettings = MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(targetGrid.GridSizeEnum, targetGrid.IsStatic);
                    if (!MyCubeGrid.TestPlacementArea(targetGrid, targetGrid.IsStatic, ref gridPlacementSettings, targetGrid.PositionComp.LocalAABB, false, null, true, true))
                    {
                        this.m_entity.Close();
                        return;
                    }
                }
                foreach (IMyEntity entity in this.m_resultIDs)
                {
                    IMyEntity entity2;
                    MyEntityIdentifier.TryGetEntity(entity.EntityId, out entity2, false);
                    if (entity2 == null)
                    {
                        MyEntityIdentifier.AddEntityWithId(entity);
                        continue;
                    }
                    MyLog.Default.WriteLineAndConsole("Dropping entity with duplicated id: " + entity.EntityId);
                    entity.Close();
                }
                if ((this.m_entity != null) && (this.m_entity.EntityId != 0))
                {
                    if (this.m_addToScene)
                    {
                        bool insertIntoScene = (this.m_objectBuilder.PersistentFlags & MyPersistentEntityFlags2.InScene) > MyPersistentEntityFlags2.None;
                        MyEntities.Add(this.m_entity, insertIntoScene);
                    }
                    if (this.m_completionCallback != null)
                    {
                        this.m_completionCallback(this.m_entity);
                    }
                }
            }
        }
    }
}

