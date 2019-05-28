namespace Sandbox.Engine.Physics
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.Library.Threading;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.Spatial;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.Simulation, 0x3e8), StaticEventOwner]
    public class MyPhysics : MySessionComponentBase
    {
        [ThreadStatic]
        private static List<HkWorld.HitInfo> m_resultHits;
        [ThreadStatic]
        private static List<MyClusterTree.MyClusterQueryResult> m_resultWorlds;
        private static List<HkShapeCollision> m_shapeCollisionResultsCache;
        private static ParallelRayCastQuery m_pendingRayCasts;
        private static MyConcurrentPool<DeliverData> m_pendingRayCastsParallelPool = new MyConcurrentPool<DeliverData>(0, null, 100, null);
        private bool ParallelSteppingInitialized;
        public static int ThreadId;
        public static MyClusterTree Clusters;
        private static bool ClustersNeedSync = false;
        private static HkJobThreadPool m_threadPool;
        private static HkJobQueue m_jobQueue;
        private static MyProfiler[] m_havokThreadProfilers;
        private bool m_updateKinematicBodies = (MyFakes.ENABLE_ANIMATED_KINEMATIC_UPDATE && !Sync.IsServer);
        private List<MyPhysicsBody> m_iterationBodies = new List<MyPhysicsBody>();
        private List<HkCharacterRigidBody> m_characterIterationBodies = new List<HkCharacterRigidBody>();
        private static List<MyEntity> m_tmpEntityResults = new List<MyEntity>();
        private static Queue<long> m_timestamps = new Queue<long>(120);
        private static SpinLockRef m_raycastLock = new SpinLockRef();
        private const string HkProfilerSymbol = "HkProfiler";
        private List<HkSimulationIslandInfo> m_simulationIslandInfos;
        private static Queue<FractureImpactDetails> m_destructionQueue = new Queue<FractureImpactDetails>();
        public static bool DebugDrawClustersEnable = false;
        public static MatrixD DebugDrawClustersMatrix = MatrixD.Identity;
        private static List<BoundingBoxD> m_clusterStaticObjects = new List<BoundingBoxD>();
        private static List<MyLineSegmentOverlapResult<MyVoxelBase>> m_foundEntities = new List<MyLineSegmentOverlapResult<MyVoxelBase>>();
        private static List<HkWorld.HitInfo?> m_resultShapeCasts;
        public static bool SyncVDBCamera = false;
        private static MatrixD? ClientCameraWM;
        private static bool IsVDBRecording = false;
        private static string VDBRecordFile = null;
        private const int SWITCH_HISTERESIS_FRAMES = 10;
        private const float SLOW_FRAME_DEVIATION_FACTOR = 3f;
        private const float FAST_FRAME_DEVIATION_FACTOR = 1.5f;
        private const float SLOW_FRAME_ABSOLUTE_THRESHOLD = 30f;
        private int m_slowFrames;
        private int m_fastFrames;
        private bool m_optimizeNextFrame;
        private bool m_optimizationsEnabled;
        private float m_averageFrameTime;
        private const int NUM_FRAMES_TO_CONSIDER = 100;
        private const int NUM_SLOW_FRAMES_TO_CONSIDER = 0x3e8;
        private List<IPhysicsStepOptimizer> m_optimizers;
        private List<MyTuple<HkWorld, MyTimeSpan>> m_timings = new List<MyTuple<HkWorld, MyTimeSpan>>();

        public static void ActivateInBox(ref BoundingBoxD box)
        {
            using (m_tmpEntityResults.GetClearToken<MyEntity>())
            {
                MyGamePruningStructure.GetTopMostEntitiesInBox(ref box, m_tmpEntityResults, MyEntityQueryType.Dynamic);
                foreach (MyEntity entity in m_tmpEntityResults)
                {
                    if (entity.Physics == null)
                    {
                        continue;
                    }
                    if (entity.Physics.Enabled && (entity.Physics.RigidBody != null))
                    {
                        entity.Physics.RigidBody.Activate();
                    }
                }
            }
        }

        public static ulong AddObject(BoundingBoxD bbox, MyPhysicsBody activationHandler, ulong? customId, string tag, long entityId, bool batch)
        {
            ulong num = (Clusters == null) ? ulong.MaxValue : Clusters.AddObject(bbox, activationHandler, customId, tag, entityId, batch);
            if (num == ulong.MaxValue)
            {
                HavokWorld_EntityLeftWorld(activationHandler.RigidBody);
            }
            return num;
        }

        private void AddTimestamp()
        {
            long timestamp = Stopwatch.GetTimestamp();
            m_timestamps.Enqueue(timestamp);
            long num2 = timestamp - Stopwatch.Frequency;
            while (m_timestamps.Peek() < num2)
            {
                m_timestamps.Dequeue();
            }
        }

        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void AssertThread()
        {
        }

        public static HitInfo? CastLongRay(Vector3D from, Vector3D to, bool any = false)
        {
            HitInfo? nullable3;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.CastRay(from, to, m_resultWorlds);
                    HitInfo? nullable = null;
                    nullable = CastRayInternal(from, to, m_resultWorlds, 9);
                    if (nullable != null)
                    {
                        if (!any)
                        {
                            to = nullable.Value.Position + nullable.Value.Position;
                        }
                        else
                        {
                            return nullable;
                        }
                    }
                    LineD ray = new LineD(from, to);
                    MyGamePruningStructure.GetVoxelMapsOverlappingRay(ref ray, m_foundEntities);
                    double num = 1.0;
                    double num2 = 0.0;
                    bool flag = false;
                    foreach (MyLineSegmentOverlapResult<MyVoxelBase> result in m_foundEntities)
                    {
                        if (result.Element.GetOrePriority() != -1)
                        {
                            continue;
                        }
                        MyVoxelBase rootVoxel = result.Element.RootVoxel;
                        if (rootVoxel.Storage.DataProvider != null)
                        {
                            double num3;
                            double num4;
                            LineD line = new LineD(Vector3D.Transform(ray.From, rootVoxel.PositionComp.WorldMatrixInvScaled) + rootVoxel.SizeInMetresHalf, Vector3D.Transform(ray.To, rootVoxel.PositionComp.WorldMatrixInvScaled) + rootVoxel.SizeInMetresHalf);
                            if (rootVoxel.Storage.DataProvider.Intersect(ref line, out num3, out num4))
                            {
                                if (num3 < num)
                                {
                                    num = num3;
                                }
                                if (num4 > num2)
                                {
                                    num2 = num4;
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        nullable3 = nullable;
                    }
                    else
                    {
                        to = from + ((ray.Direction * ray.Length) * num2);
                        from += (ray.Direction * ray.Length) * num;
                        m_foundEntities.Clear();
                        HitInfo? nullable2 = CastRayInternal(from, to, m_resultWorlds, 0x1c);
                        if (nullable == null)
                        {
                            nullable3 = nullable2;
                        }
                        else if (((nullable2 == null) || (nullable == null)) || (nullable2.Value.HkHitInfo.HitFraction >= nullable.Value.HkHitInfo.HitFraction))
                        {
                            nullable3 = nullable;
                        }
                        else
                        {
                            nullable3 = nullable2;
                        }
                    }
                }
            }
            return nullable3;
        }

        public static HitInfo? CastRay(Vector3D from, Vector3D to, int raycastFilterLayer = 0) => 
            CastRayInternal(ref from, ref to, raycastFilterLayer);

        public static void CastRay(Vector3D from, Vector3D to, List<HitInfo> toList, int raycastFilterLayer = 0)
        {
            CastRayInternal(ref from, ref to, toList, raycastFilterLayer);
        }

        public static bool CastRay(Vector3D from, Vector3D to, out HitInfo hitInfo, uint raycastCollisionFilter, bool ignoreConvexShape)
        {
            using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
            {
                Clusters.CastRay(from, to, m_resultWorlds);
                hitInfo = new HitInfo();
                using (List<MyClusterTree.MyClusterQueryResult>.Enumerator enumerator = m_resultWorlds.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyClusterTree.MyClusterQueryResult current = enumerator.Current;
                        Vector3 vector = (Vector3) (from - current.AABB.Center);
                        BoundingBoxD aABB = current.AABB;
                        Vector3 vector2 = (Vector3) (to - aABB.Center);
                        m_resultHits.Clear();
                        HkWorld.HitInfo info = new HkWorld.HitInfo();
                        bool flag = ((HkWorld) current.UserData).CastRay(vector, vector2, out info, raycastCollisionFilter, ignoreConvexShape);
                        if (flag)
                        {
                            hitInfo.Position = info.Position + current.AABB.Center;
                            hitInfo.HkHitInfo = info;
                            return flag;
                        }
                    }
                }
            }
            return false;
        }

        private static HitInfo? CastRayInternal(ref Vector3D from, ref Vector3D to, int raycastFilterLayer)
        {
            using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
            {
                Clusters.CastRay(from, to, m_resultWorlds);
                return CastRayInternal(from, to, m_resultWorlds, raycastFilterLayer);
            }
        }

        private static void CastRayInternal(ref Vector3D from, ref Vector3D to, List<HitInfo> toList, int raycastFilterLayer = 0)
        {
            toList.Clear();
            using (MyUtils.ReuseCollection<HkWorld.HitInfo>(ref m_resultHits))
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.CastRay(from, to, m_resultWorlds);
                    foreach (MyClusterTree.MyClusterQueryResult result in m_resultWorlds)
                    {
                        Vector3D vectord = from - result.AABB.Center;
                        BoundingBoxD aABB = result.AABB;
                        Vector3D vectord2 = to - aABB.Center;
                        HkWorld userData = (HkWorld) result.UserData;
                        if (userData != null)
                        {
                            userData.CastRay((Vector3) vectord, (Vector3) vectord2, m_resultHits, raycastFilterLayer);
                        }
                        foreach (HkWorld.HitInfo info in m_resultHits)
                        {
                            HitInfo item = new HitInfo {
                                HkHitInfo = info
                            };
                            aABB = result.AABB;
                            item.Position = info.Position + aABB.Center;
                            toList.Add(item);
                        }
                    }
                }
            }
        }

        private static HitInfo? CastRayInternal(Vector3D from, Vector3D to, List<MyClusterTree.MyClusterQueryResult> worlds, int raycastFilterLayer = 0)
        {
            float maxValue = float.MaxValue;
            using (List<MyClusterTree.MyClusterQueryResult>.Enumerator enumerator = worlds.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyClusterTree.MyClusterQueryResult current = enumerator.Current;
                    Vector3 vector = (Vector3) (from - current.AABB.Center);
                    BoundingBoxD aABB = current.AABB;
                    Vector3 vector2 = (Vector3) (to - aABB.Center);
                    HkWorld.HitInfo? nullable = ((HkWorld) current.UserData).CastRay(vector, vector2, raycastFilterLayer);
                    if ((nullable != null) && (nullable.Value.HitFraction < maxValue))
                    {
                        Vector3D worldPosition = nullable.Value.Position + current.AABB.Center;
                        maxValue = nullable.Value.HitFraction;
                        return new HitInfo(nullable.Value, worldPosition);
                    }
                }
            }
            return null;
        }

        public static void CastRayParallel(ref Vector3D from, ref Vector3D to, int raycastFilterLayer, Action<HitInfo?> callback)
        {
            ParallelRayCastQuery query = ParallelRayCastQuery.Allocate();
            query.Kind = ParallelRayCastQuery.TKind.RaycastSingle;
            RayCastData data = new RayCastData {
                To = to,
                From = from,
                Callback = callback,
                RayCastFilterLayer = raycastFilterLayer
            };
            query.RayCastData = data;
            EnqueueParallelQuery(query);
        }

        public static void CastRayParallel(ref Vector3D from, ref Vector3D to, List<HitInfo> collector, int raycastFilterLayer, Action<List<HitInfo>> callback)
        {
            ParallelRayCastQuery query = ParallelRayCastQuery.Allocate();
            query.Kind = ParallelRayCastQuery.TKind.RaycastAll;
            RayCastData data = new RayCastData {
                To = to,
                From = from,
                Callback = callback,
                Collector = collector,
                RayCastFilterLayer = raycastFilterLayer
            };
            query.RayCastData = data;
            EnqueueParallelQuery(query);
        }

        public static float? CastShape(Vector3D to, HkShape shape, ref MatrixD transform, int filterLayer, float extraPenetration = 0f)
        {
            float? nullable;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(to, m_resultWorlds);
                    if (m_resultWorlds.Count == 0)
                    {
                        nullable = null;
                        nullable = nullable;
                    }
                    else
                    {
                        MyClusterTree.MyClusterQueryResult result = m_resultWorlds[0];
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result.AABB.Center);
                        Vector3 vector = (Vector3) (to - result.AABB.Center);
                        nullable = ((HkWorld) result.UserData).CastShape(vector, shape, ref matrix, filterLayer, extraPenetration);
                    }
                }
            }
            return nullable;
        }

        public static float? CastShapeInAllWorlds(Vector3D to, HkShape shape, ref MatrixD transform, int filterLayer, float extraPenetration = 0f)
        {
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.CastRay(transform.Translation, to, m_resultWorlds);
                    using (List<MyClusterTree.MyClusterQueryResult>.Enumerator enumerator = m_resultWorlds.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyClusterTree.MyClusterQueryResult current = enumerator.Current;
                            Matrix matrix = (Matrix) transform;
                            matrix.Translation = (Vector3) (transform.Translation - current.AABB.Center);
                            BoundingBoxD aABB = current.AABB;
                            Vector3 vector = (Vector3) (to - aABB.Center);
                            float? nullable = ((HkWorld) current.UserData).CastShape(vector, shape, ref matrix, filterLayer, extraPenetration);
                            if (nullable != null)
                            {
                                return nullable;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static HkContactPoint? CastShapeReturnContact(Vector3D to, HkShape shape, ref MatrixD transform, int filterLayer, float extraPenetration, out Vector3 worldTranslation)
        {
            HkContactPoint? nullable2;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(to, m_resultWorlds);
                    worldTranslation = Vector3.Zero;
                    if (m_resultWorlds.Count == 0)
                    {
                        nullable2 = null;
                        nullable2 = nullable2;
                    }
                    else
                    {
                        MyClusterTree.MyClusterQueryResult result = m_resultWorlds[0];
                        worldTranslation = (Vector3) result.AABB.Center;
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result.AABB.Center);
                        Vector3 vector = (Vector3) (to - result.AABB.Center);
                        HkContactPoint? nullable = ((HkWorld) result.UserData).CastShapeReturnContact(vector, shape, ref matrix, filterLayer, extraPenetration);
                        if (nullable != null)
                        {
                            nullable2 = nullable;
                        }
                        else
                        {
                            nullable2 = null;
                        }
                    }
                }
            }
            return nullable2;
        }

        public static HitInfo? CastShapeReturnContactBodyData(Vector3D to, HkShape shape, ref MatrixD transform, uint collisionFilter, float extraPenetration, bool ignoreConvexShape = true)
        {
            HitInfo? nullable2;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(to, m_resultWorlds);
                    if (m_resultWorlds.Count == 0)
                    {
                        nullable2 = null;
                        nullable2 = nullable2;
                    }
                    else
                    {
                        MyClusterTree.MyClusterQueryResult result = m_resultWorlds[0];
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result.AABB.Center);
                        Vector3 vector = (Vector3) (to - result.AABB.Center);
                        HkWorld.HitInfo? nullable = ((HkWorld) result.UserData).CastShapeReturnContactBodyData(vector, shape, ref matrix, collisionFilter, extraPenetration);
                        if (nullable == null)
                        {
                            nullable2 = null;
                        }
                        else
                        {
                            HkWorld.HitInfo hi = nullable.Value;
                            nullable2 = new HitInfo(hi, hi.Position + result.AABB.Center);
                        }
                    }
                }
            }
            return nullable2;
        }

        public static bool CastShapeReturnContactBodyDatas(Vector3D to, HkShape shape, ref MatrixD transform, uint collisionFilter, float extraPenetration, List<HitInfo> result, bool ignoreConvexShape = true)
        {
            bool flag;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(to, m_resultWorlds);
                    if (m_resultWorlds.Count != 0)
                    {
                        MyClusterTree.MyClusterQueryResult result2 = m_resultWorlds[0];
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result2.AABB.Center);
                        Vector3 vector = (Vector3) (to - result2.AABB.Center);
                        using (MyUtils.ReuseCollection<HkWorld.HitInfo?>(ref m_resultShapeCasts))
                        {
                            if (((HkWorld) result2.UserData).CastShapeReturnContactBodyDatas(vector, shape, ref matrix, collisionFilter, extraPenetration, m_resultShapeCasts))
                            {
                                foreach (HkWorld.HitInfo? nullable in m_resultShapeCasts)
                                {
                                    HkWorld.HitInfo info = nullable.Value;
                                    HitInfo item = new HitInfo {
                                        HkHitInfo = info,
                                        Position = info.Position + result2.AABB.Center
                                    };
                                    result.Add(item);
                                }
                                return true;
                            }
                        }
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }
            return flag;
        }

        public static unsafe HkContactPointData? CastShapeReturnContactData(Vector3D to, HkShape shape, ref MatrixD transform, uint collisionFilter, float extraPenetration, bool ignoreConvexShape = true)
        {
            HkContactPointData? nullable2;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(to, m_resultWorlds);
                    if (m_resultWorlds.Count == 0)
                    {
                        nullable2 = null;
                        nullable2 = nullable2;
                    }
                    else
                    {
                        MyClusterTree.MyClusterQueryResult result = m_resultWorlds[0];
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result.AABB.Center);
                        Vector3 vector = (Vector3) (to - result.AABB.Center);
                        HkContactPointData? nullable = ((HkWorld) result.UserData).CastShapeReturnContactData(vector, shape, ref matrix, collisionFilter, extraPenetration);
                        if (nullable == null)
                        {
                            nullable2 = null;
                        }
                        else
                        {
                            HkContactPointData data = nullable.Value;
                            Vector3D* vectordPtr1 = (Vector3D*) ref data.HitPosition;
                            vectordPtr1[0] += result.AABB.Center;
                            nullable2 = new HkContactPointData?(data);
                        }
                    }
                }
            }
            return nullable2;
        }

        public static Vector3D? CastShapeReturnPoint(Vector3D to, HkShape shape, ref MatrixD transform, int filterLayer, float extraPenetration)
        {
            Vector3D? nullable2;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    m_resultWorlds.Clear();
                    Clusters.Intersects(to, m_resultWorlds);
                    if (m_resultWorlds.Count == 0)
                    {
                        nullable2 = null;
                        nullable2 = nullable2;
                    }
                    else
                    {
                        MyClusterTree.MyClusterQueryResult result = m_resultWorlds[0];
                        Matrix matrix = (Matrix) transform;
                        matrix.Translation = (Vector3) (transform.Translation - result.AABB.Center);
                        Vector3 vector = (Vector3) (to - result.AABB.Center);
                        Vector3? nullable = ((HkWorld) result.UserData).CastShapeReturnPoint(vector, shape, ref matrix, filterLayer, extraPenetration);
                        if (nullable != null)
                        {
                            nullable2 = new Vector3D?(nullable.Value + result.AABB.Center);
                        }
                        else
                        {
                            nullable2 = null;
                        }
                    }
                }
            }
            return nullable2;
        }

        public static void CommitSchedulingSettingToServer()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<bool, bool>(x => new Action<bool, bool>(MyPhysics.SetScheduling), MyFakes.ENABLE_HAVOK_MULTITHREADING, MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING, targetEndpoint, position);
        }

        [Event(null, 0x787), Reliable, Server]
        public static void ControlVDBRecording(string fileName)
        {
            if (MyEventContext.Current.IsLocallyInvoked || MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                VDBRecordFile = (fileName == null) ? null : Path.Combine(MyFileSystem.UserDataPath, fileName.Replace('\\', '_').Replace('/', '_').Replace(':', '_'));
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        public static HkWorld CreateHkWorld(float broadphaseSize = 100000f)
        {
            HkWorld world = new HkWorld(MyPerGameSettings.EnableGlobalGravity, broadphaseSize, MyFakes.WHEEL_SOFTNESS ? float.MaxValue : RestingVelocity, MyFakes.ENABLE_HAVOK_MULTITHREADING, MySession.Static.Settings.PhysicsIterations);
            world.MarkForWrite();
            world.SetVersionGetter(HashSetExtensions.HashSetInternalAccessor<HkRigidBody>.VersionGetter);
            if (MySession.Static.Settings.WorldSizeKm > 0)
            {
                world.EntityLeftWorld += new HkEntityHandler(MyPhysics.HavokWorld_EntityLeftWorld);
            }
            if (MyPerGameSettings.Destruction && Sync.IsServer)
            {
                world.DestructionWorld = new HkdWorld(world);
            }
            if (MyFakes.ENABLE_HAVOK_MULTITHREADING)
            {
                world.InitMultithreading(m_threadPool, m_jobQueue);
            }
            world.DeactivationRotationSqrdA /= 3f;
            world.DeactivationRotationSqrdB /= 3f;
            InitCollisionFilters(world);
            return world;
        }

        public static unsafe void DebugDrawClusters()
        {
            if (Clusters != null)
            {
                double num = 2000.0;
                MatrixD transform = MatrixD.CreateWorld(DebugDrawClustersMatrix.Translation + (num * DebugDrawClustersMatrix.Forward), Vector3D.Forward, Vector3D.Up);
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.GetAll(m_resultWorlds);
                    BoundingBoxD xd2 = BoundingBoxD.CreateInvalid();
                    foreach (MyClusterTree.MyClusterQueryResult result in m_resultWorlds)
                    {
                        xd2 = xd2.Include(result.AABB);
                    }
                    double num3 = num / xd2.Size.AbsMax();
                    Vector3D center = xd2.Center;
                    Vector3D* vectordPtr1 = (Vector3D*) ref xd2.Min;
                    vectordPtr1[0] -= center;
                    Vector3D* vectordPtr2 = (Vector3D*) ref xd2.Max;
                    vectordPtr2[0] -= center;
                    BoundingBoxD box = new BoundingBoxD((xd2.Min * num3) * 1.0199999809265137, (xd2.Max * num3) * 1.0199999809265137);
                    MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(box, transform), Color.Green, 0.2f, false, false, false);
                    MyRenderProxy.DebugDrawAxis(transform, 50f, false, false, false);
                    if (MySession.Static != null)
                    {
                        foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                        {
                            if (player.Character != null)
                            {
                                MyRenderProxy.DebugDrawSphere(Vector3D.Transform((player.Character.PositionComp.GetPosition() - center) * num3, transform), 1f, Vector3.One, 1f, false, false, true, false);
                            }
                        }
                    }
                    Clusters.GetAllStaticObjects(m_clusterStaticObjects);
                    foreach (BoundingBoxD xd4 in m_clusterStaticObjects)
                    {
                        BoundingBoxD xd5 = new BoundingBoxD((xd4.Min - center) * num3, (xd4.Max - center) * num3);
                        MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(xd5, transform), Color.Blue, 0.2f, false, false, false);
                    }
                    foreach (MyClusterTree.MyClusterQueryResult result2 in m_resultWorlds)
                    {
                        BoundingBoxD xd6 = new BoundingBoxD((result2.AABB.Min - center) * num3, (result2.AABB.Max - center) * num3);
                        MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(xd6, transform), Color.White, 0.2f, false, false, false);
                        foreach (HkCharacterRigidBody body in ((HkWorld) result2.UserData).CharacterRigidBodies)
                        {
                            BoundingBoxD aABB = result2.AABB;
                            Vector3D pointFrom = Vector3D.Transform(((aABB.Center + body.Position) - center) * num3, transform);
                            Vector3D vectord4 = Vector3D.TransformNormal((Vector3D) body.LinearVelocity, transform) * 10.0;
                            if (vectord4.Length() > 0.0099999997764825821)
                            {
                                MyRenderProxy.DebugDrawLine3D(pointFrom, pointFrom + vectord4, Color.Blue, Color.White, false, false);
                            }
                        }
                        foreach (HkRigidBody body2 in ((HkWorld) result2.UserData).RigidBodies)
                        {
                            if (body2.GetEntity(0) != null)
                            {
                                MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD(body2.GetEntity(0).LocalAABB, body2.GetEntity(0).WorldMatrix);
                                MyOrientedBoundingBoxD* xdPtr1 = (MyOrientedBoundingBoxD*) ref obb;
                                xdPtr1->Center = (obb.Center - center) * num3;
                                Vector3D* vectordPtr3 = (Vector3D*) ref obb.HalfExtent;
                                vectordPtr3[0] *= num3;
                                obb.Transform(transform);
                                Color yellow = Color.Yellow;
                                if (body2.GetEntity(0).LocalAABB.Size.Max() > 1000f)
                                {
                                    yellow = Color.Red;
                                }
                                MyRenderProxy.DebugDrawOBB(obb, yellow, 1f, false, false, false);
                                Vector3D vectord5 = Vector3D.TransformNormal((Vector3D) body2.LinearVelocity, transform) * 10.0;
                                if (vectord5.Length() > 0.0099999997764825821)
                                {
                                    MyRenderProxy.DebugDrawLine3D(obb.Center, obb.Center + vectord5, Color.Red, Color.White, false, false);
                                }
                                if (Vector3D.Distance(obb.Center, MySector.MainCamera.Position) < 10.0)
                                {
                                    MyRenderProxy.DebugDrawText3D(obb.Center, body2.GetEntity(0).ToString(), Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DeserializeClusters(List<BoundingBoxD> list)
        {
            Clusters.Deserialize(list);
        }

        private void DisableOptimizations()
        {
            this.m_optimizationsEnabled = false;
            using (List<IPhysicsStepOptimizer>.Enumerator enumerator = this.m_optimizers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DisableOptimizations();
                }
            }
        }

        private void DrawIslands()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SIMULATION_ISLANDS)
            {
                foreach (MyClusterTree.MyCluster local1 in Clusters.GetClusters())
                {
                    Vector3D center = local1.AABB.Center;
                    HkWorld userData = (HkWorld) local1.UserData;
                    using (MyUtils.ReuseCollection<HkSimulationIslandInfo>(ref this.m_simulationIslandInfos))
                    {
                        userData.ReadSimulationIslandInfos(this.m_simulationIslandInfos);
                        foreach (HkSimulationIslandInfo info in this.m_simulationIslandInfos)
                        {
                            BoundingBoxD aabb = new BoundingBoxD(info.AABB.Min + center, info.AABB.Max + center);
                            if ((aabb.Distance(MySector.MainCamera.Position) < 500.0) && info.IsActive)
                            {
                                MyRenderProxy.DebugDrawAABB(aabb, info.IsActive ? Color.Red : Color.RoyalBlue, 1f, 1f, false, false, false);
                            }
                        }
                    }
                }
            }
        }

        private void EnableOptimizations(List<MyTuple<HkWorld, MyTimeSpan>> timings)
        {
            this.m_optimizationsEnabled = true;
            MyLog.Default.WriteLine("Optimizing physics step of " + timings.Count + " worlds");
            using (List<IPhysicsStepOptimizer>.Enumerator enumerator = this.m_optimizers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.EnableOptimizations(timings);
                }
            }
        }

        public static void EnqueueDestruction(FractureImpactDetails details)
        {
            m_destructionQueue.Enqueue(details);
        }

        private static void EnqueueParallelQuery(ParallelRayCastQuery query)
        {
            ParallelRayCastQuery pendingRayCasts = m_pendingRayCasts;
            while (true)
            {
                query.Next = pendingRayCasts;
                ParallelRayCastQuery objA = Interlocked.CompareExchange<ParallelRayCastQuery>(ref m_pendingRayCasts, query, pendingRayCasts);
                if (ReferenceEquals(objA, pendingRayCasts))
                {
                    return;
                }
                pendingRayCasts = objA;
            }
        }

        private void EnsureClusterSpace()
        {
            if (MyFakes.FORCE_CLUSTER_REORDER)
            {
                ForceClustersReorder();
                MyFakes.FORCE_CLUSTER_REORDER = false;
            }
            foreach (MyPhysicsBody body in this.m_iterationBodies)
            {
                Vector3 linearVelocity = body.LinearVelocity;
                if (linearVelocity.LengthSquared() > 0.1f)
                {
                    BoundingBoxD aabb = MyClusterTree.AdjustAABBByVelocity(body.Entity.WorldAABB, linearVelocity, 1.1f);
                    Clusters.EnsureClusterSpace(aabb);
                }
            }
            foreach (HkCharacterRigidBody body2 in this.m_characterIterationBodies)
            {
                Vector3 linearVelocity = body2.LinearVelocity;
                if (linearVelocity.LengthSquared() > 0.1f)
                {
                    BoundingBoxD aabb = MyClusterTree.AdjustAABBByVelocity(((MyPhysicsBody) body2.GetHitRigidBody().UserObject).Entity.PositionComp.WorldAABB, body2.LinearVelocity, 1.1f);
                    Clusters.EnsureClusterSpace(aabb);
                }
            }
        }

        public static void EnsurePhysicsSpace(BoundingBoxD aabb)
        {
            using (m_raycastLock.Acquire())
            {
                Clusters.EnsureClusterSpace(aabb);
            }
        }

        private void ExecuteParallelRayCasts()
        {
            ParallelRayCastQuery item = Interlocked.Exchange<ParallelRayCastQuery>(ref m_pendingRayCasts, null);
            if (item != null)
            {
                DeliverData work = m_pendingRayCastsParallelPool.Get();
                while (true)
                {
                    if (item == null)
                    {
                        WorkOptions? options = null;
                        Parallel.For(0, work.Jobs.Count, work.ExecuteJob, 1, WorkPriority.VeryHigh, options, false);
                        Parallel.Start(work);
                        break;
                    }
                    work.Jobs.Add(item);
                    item = item.Next;
                }
            }
        }

        internal static void ForceClustersReorder()
        {
            Clusters.ReorderClusters(BoundingBoxD.CreateInvalid(), ulong.MaxValue);
        }

        public static void GetAll(List<MyClusterTree.MyClusterQueryResult> results)
        {
            Clusters.GetAll(results);
        }

        public static ListReader<object>? GetClusterList()
        {
            if (Clusters != null)
            {
                return new ListReader<object>?(Clusters.GetList());
            }
            return null;
        }

        public static int GetCollisionLayer(string strLayer) => 
            ((strLayer != "LightFloatingObjectCollisionLayer") ? ((strLayer != "VoxelLod1CollisionLayer") ? ((strLayer != "NotCollideWithStaticLayer") ? ((strLayer != "StaticCollisionLayer") ? ((strLayer != "CollideWithStaticLayer") ? ((strLayer != "DefaultCollisionLayer") ? ((strLayer != "DynamicDoubledCollisionLayer") ? ((strLayer != "KinematicDoubledCollisionLayer") ? ((strLayer != "CharacterCollisionLayer") ? ((strLayer != "NoCollisionLayer") ? ((strLayer != "DebrisCollisionLayer") ? ((strLayer != "GravityPhantomLayer") ? ((strLayer != "CharacterNetworkCollisionLayer") ? ((strLayer != "FloatingObjectCollisionLayer") ? ((strLayer != "ObjectDetectionCollisionLayer") ? ((strLayer != "VirtualMassLayer") ? ((strLayer != "CollectorCollisionLayer") ? ((strLayer != "AmmoLayer") ? ((strLayer != "VoxelCollisionLayer") ? ((strLayer != "ExplosionRaycastLayer") ? ((strLayer != "CollisionLayerWithoutCharacter") ? ((strLayer != "RagdollCollisionLayer") ? ((strLayer != "NoVoxelCollisionLayer") ? ((strLayer != "MissileLayer") ? 15 : 8) : 9) : 0x1f) : 30) : 0x1d) : 0x1c) : 0x1b) : 0x1a) : 0x19) : 0x18) : 0x17) : 0x16) : 0x15) : 20) : 0x13) : 0x12) : 0x11) : 0x10) : 15) : 14) : 13) : 12) : 11) : 10);

        public bool GetEntityReplicableExistsById(long entityId)
        {
            MyEntity entity = MyEntities.GetEntityByIdOrDefault(entityId, null, false);
            return ((entity != null) && (MyExternalReplicable.FindByObject(entity) != null));
        }

        public static Vector3D GetObjectOffset(ulong id) => 
            Clusters.GetObjectOffset(id);

        public static void GetPenetrationsBox(ref Vector3 halfExtents, ref Vector3D translation, ref Quaternion rotation, List<HkBodyCollision> results, int filter)
        {
            GetPenetrationsBoxInternal(ref halfExtents, ref translation, ref rotation, results, filter);
        }

        private static void GetPenetrationsBoxInternal(ref Vector3 halfExtents, ref Vector3D translation, ref Quaternion rotation, List<HkBodyCollision> results, int filter)
        {
            using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
            {
                Clusters.Intersects(translation, m_resultWorlds);
                foreach (MyClusterTree.MyClusterQueryResult result in m_resultWorlds)
                {
                    BoundingBoxD aABB = result.AABB;
                    Vector3 vector = (Vector3) (translation - aABB.Center);
                    ((HkWorld) result.UserData).GetPenetrationsBox(ref halfExtents, ref vector, ref rotation, results, filter);
                }
            }
        }

        public static void GetPenetrationsBoxParallel(ref Vector3 halfExtents, ref Vector3D translation, ref Quaternion rotation, List<HkBodyCollision> results, int filter, Action<List<HkBodyCollision>> callback)
        {
            ParallelRayCastQuery query = ParallelRayCastQuery.Allocate();
            query.Kind = ParallelRayCastQuery.TKind.GetPenetrationsBox;
            QueryBoxData data = new QueryBoxData {
                Filter = filter,
                Results = results,
                Rotation = rotation,
                Callback = callback,
                Translation = translation,
                HalfExtents = halfExtents
            };
            query.QueryBoxData = data;
            EnqueueParallelQuery(query);
        }

        public static void GetPenetrationsShape(HkShape shape, ref Vector3D translation, ref Quaternion rotation, List<HkBodyCollision> results, int filter)
        {
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    Clusters.Intersects(translation, m_resultWorlds);
                    foreach (MyClusterTree.MyClusterQueryResult result in m_resultWorlds)
                    {
                        BoundingBoxD aABB = result.AABB;
                        Vector3 vector = (Vector3) (translation - aABB.Center);
                        ((HkWorld) result.UserData).GetPenetrationsShape(shape, ref vector, ref rotation, results, filter);
                    }
                }
            }
        }

        public static ClearToken<HkShapeCollision> GetPenetrationsShapeShape(HkShape shape1, ref Vector3 translation1, ref Quaternion rotation1, HkShape shape2, ref Vector3 translation2, ref Quaternion rotation2)
        {
            MyUtils.Init<List<HkShapeCollision>>(ref m_shapeCollisionResultsCache).AssertEmpty<HkShapeCollision>();
            ((HkWorld) Clusters.GetList()[0]).GetPenetrationsShapeShape(shape1, ref translation1, ref rotation1, shape2, ref translation2, ref rotation2, m_shapeCollisionResultsCache);
            return m_shapeCollisionResultsCache.GetClearToken<HkShapeCollision>();
        }

        private static void HavokWorld_EntityLeftWorld(HkEntity hkEntity)
        {
            List<IMyEntity> allEntities = hkEntity.GetAllEntities();
            foreach (IMyEntity entity in allEntities)
            {
                if (!Sync.IsServer)
                {
                    continue;
                }
                switch (entity)
                {
                    case (null):
                        break;

                    case (MyCharacter _):
                        ((MyCharacter) entity).DoDamage(1000f, MyDamageType.Suicide, true, 0L);
                        continue;
                        break;

                    case ((MyVoxelMap _) || (MyCubeBlock _)):
                        break;

                    case (MyCubeGrid _):
                        MyCubeGrid.KillAllCharacters(entity as MyCubeGrid);
                        entity.Close();
                        continue;
                        break;

                    case (MyFloatingObject _):
                        MyFloatingObjects.RemoveFloatingObject((MyFloatingObject) entity);
                        continue;
                        break;

                    case (MyFracturedPiece _):
                        MyFracturedPiecesManager.Static.RemoveFracturePiece((MyFracturedPiece) entity, 0f, false, true);
                        continue;
                        break;
                }
            }
            allEntities.Clear();
        }

        private static void InitCollisionFilters(HkWorld world)
        {
            world.DisableCollisionsBetween(0x10, 0x11);
            world.DisableCollisionsBetween(0x11, 0x12);
            world.DisableCollisionsBetween(0x10, 0x16);
            world.DisableCollisionsBetween(12, 13);
            world.DisableCollisionsBetween(12, 0x1c);
            world.DisableCollisionsBetween(0x11, 15);
            world.DisableCollisionsBetween(0x11, 13);
            world.DisableCollisionsBetween(0x11, 0x1c);
            world.DisableCollisionsBetween(0x11, 8);
            world.DisableCollisionsBetween(0x15, 13);
            world.DisableCollisionsBetween(0x15, 0x1c);
            world.DisableCollisionsBetween(0x15, 15);
            world.DisableCollisionsBetween(0x15, 0x10);
            world.DisableCollisionsBetween(0x15, 0x11);
            world.DisableCollisionsBetween(0x15, 0x12);
            world.DisableCollisionsBetween(0x15, 0x16);
            world.DisableCollisionsBetween(0x15, 0x18);
            world.DisableCollisionsBetween(0x15, 8);
            world.DisableCollisionsBetween(0x19, 13);
            world.DisableCollisionsBetween(0x19, 0x1c);
            world.DisableCollisionsBetween(0x19, 15);
            world.DisableCollisionsBetween(0x19, 0x12);
            world.DisableCollisionsBetween(0x19, 0x16);
            world.DisableCollisionsBetween(0x19, 0x10);
            world.DisableCollisionsBetween(0x19, 0x11);
            world.DisableCollisionsBetween(0x19, 20);
            world.DisableCollisionsBetween(0x19, 0x17);
            world.DisableCollisionsBetween(0x19, 10);
            world.DisableCollisionsBetween(0x19, 0x18);
            world.DisableCollisionsBetween(0x19, 0x19);
            world.DisableCollisionsBetween(0x19, 8);
            world.DisableCollisionsBetween(0x13, 13);
            world.DisableCollisionsBetween(0x13, 0x1c);
            world.DisableCollisionsBetween(0x13, 15);
            world.DisableCollisionsBetween(0x13, 0x12);
            world.DisableCollisionsBetween(0x13, 0x16);
            world.DisableCollisionsBetween(0x13, 0x10);
            world.DisableCollisionsBetween(0x13, 0x11);
            world.DisableCollisionsBetween(0x13, 20);
            world.DisableCollisionsBetween(0x13, 0x17);
            world.DisableCollisionsBetween(0x13, 10);
            world.DisableCollisionsBetween(0x13, 0x15);
            world.DisableCollisionsBetween(0x13, 0x18);
            world.DisableCollisionsBetween(0x13, 0x19);
            world.DisableCollisionsBetween(0x13, 0x13);
            world.DisableCollisionsBetween(0x13, 8);
            if (MyPerGameSettings.PhysicsNoCollisionLayerWithDefault)
            {
                world.DisableCollisionsBetween(0x13, 0);
            }
            world.DisableCollisionsBetween(0x18, 0x18);
            world.DisableCollisionsBetween(0x1a, 13);
            world.DisableCollisionsBetween(0x1a, 0x1c);
            world.DisableCollisionsBetween(0x1a, 15);
            world.DisableCollisionsBetween(0x1a, 0x12);
            world.DisableCollisionsBetween(0x1a, 0x16);
            world.DisableCollisionsBetween(0x1a, 0x10);
            world.DisableCollisionsBetween(0x1a, 0x11);
            world.DisableCollisionsBetween(0x1a, 20);
            world.DisableCollisionsBetween(0x1a, 0x15);
            world.DisableCollisionsBetween(0x1a, 0x18);
            world.DisableCollisionsBetween(0x1a, 0x19);
            world.DisableCollisionsBetween(0x1a, 8);
            bool isServer = Sync.IsServer;
            if (!MyFakes.ENABLE_CHARACTER_AND_DEBRIS_COLLISIONS)
            {
                world.DisableCollisionsBetween(20, 0x12);
                world.DisableCollisionsBetween(20, 0x16);
                world.DisableCollisionsBetween(20, 8);
                world.DisableCollisionsBetween(10, 0x12);
                world.DisableCollisionsBetween(10, 0x16);
                world.DisableCollisionsBetween(10, 8);
            }
            world.DisableCollisionsBetween(0x1d, 0x1c);
            world.DisableCollisionsBetween(0x1d, 0x12);
            world.DisableCollisionsBetween(0x1d, 0x13);
            world.DisableCollisionsBetween(0x1d, 20);
            world.DisableCollisionsBetween(0x1d, 0x15);
            world.DisableCollisionsBetween(0x1d, 0x16);
            world.DisableCollisionsBetween(0x1d, 0x17);
            world.DisableCollisionsBetween(0x1d, 10);
            world.DisableCollisionsBetween(0x1d, 0x18);
            world.DisableCollisionsBetween(0x1d, 0x19);
            world.DisableCollisionsBetween(0x1d, 0x1a);
            world.DisableCollisionsBetween(0x1d, 0x1b);
            world.DisableCollisionsBetween(30, 0x12);
            world.DisableCollisionsBetween(30, 0x13);
            world.DisableCollisionsBetween(14, 14);
            world.DisableCollisionsBetween(14, 0x10);
            world.DisableCollisionsBetween(14, 15);
            world.DisableCollisionsBetween(14, 0x12);
            world.DisableCollisionsBetween(14, 0x13);
            world.DisableCollisionsBetween(14, 20);
            world.DisableCollisionsBetween(14, 0x15);
            world.DisableCollisionsBetween(14, 0x16);
            world.DisableCollisionsBetween(14, 0x17);
            world.DisableCollisionsBetween(14, 10);
            world.DisableCollisionsBetween(14, 0x18);
            world.DisableCollisionsBetween(14, 0x19);
            world.DisableCollisionsBetween(14, 0x1a);
            world.DisableCollisionsBetween(14, 0x1b);
            world.DisableCollisionsBetween(14, 8);
            world.DisableCollisionsBetween(0x1f, 13);
            world.DisableCollisionsBetween(0x1f, 0x1c);
            world.DisableCollisionsBetween(0x1f, 15);
            world.DisableCollisionsBetween(0x1f, 0x12);
            world.DisableCollisionsBetween(0x1f, 0x16);
            world.DisableCollisionsBetween(0x1f, 0x10);
            world.DisableCollisionsBetween(0x1f, 0x11);
            world.DisableCollisionsBetween(0x1f, 20);
            world.DisableCollisionsBetween(0x1f, 0x17);
            world.DisableCollisionsBetween(0x1f, 10);
            world.DisableCollisionsBetween(0x1f, 0x15);
            world.DisableCollisionsBetween(0x1f, 0x18);
            world.DisableCollisionsBetween(0x1f, 0x19);
            world.DisableCollisionsBetween(0x1f, 0x13);
            world.DisableCollisionsBetween(0x1f, 0x1d);
            world.DisableCollisionsBetween(0x1f, 30);
            world.DisableCollisionsBetween(0x1f, 14);
            world.DisableCollisionsBetween(0x1f, 0x1a);
            world.DisableCollisionsBetween(0x1f, 0x1b);
            world.DisableCollisionsBetween(0x1f, 8);
            if (!MyFakes.ENABLE_JETPACK_RAGDOLL_COLLISIONS)
            {
                world.DisableCollisionsBetween(0x1f, 0x1f);
            }
            if (MyVoxelPhysicsBody.UseLod1VoxelPhysics)
            {
                world.DisableCollisionsBetween(0x10, 0x1c);
                world.DisableCollisionsBetween(0x11, 0x1c);
                world.DisableCollisionsBetween(15, 0x1c);
                world.DisableCollisionsBetween(14, 0x1c);
                world.DisableCollisionsBetween(20, 0x1c);
                world.DisableCollisionsBetween(0x17, 0x1c);
                world.DisableCollisionsBetween(8, 0x1c);
                world.DisableCollisionsBetween(0x18, 11);
                world.DisableCollisionsBetween(0x12, 11);
                world.DisableCollisionsBetween(0x16, 11);
                world.DisableCollisionsBetween(10, 11);
                world.DisableCollisionsBetween(0x1f, 11);
                world.DisableCollisionsBetween(0x1d, 11);
                world.DisableCollisionsBetween(0x1a, 11);
                world.DisableCollisionsBetween(0x15, 11);
                world.DisableCollisionsBetween(0x13, 11);
                world.DisableCollisionsBetween(0x19, 11);
                world.DisableCollisionsBetween(0x11, 11);
                world.DisableCollisionsBetween(12, 11);
            }
            world.DisableCollisionsBetween(9, 0x11);
            world.DisableCollisionsBetween(9, 0x15);
            world.DisableCollisionsBetween(9, 0x19);
            world.DisableCollisionsBetween(9, 0x13);
            world.DisableCollisionsBetween(9, 0x1a);
            world.DisableCollisionsBetween(9, 14);
            world.DisableCollisionsBetween(9, 0x1f);
            world.DisableCollisionsBetween(9, 0x1c);
            world.DisableCollisionsBetween(9, 11);
            if (!Sync.IsServer)
            {
                world.DisableCollisionsBetween(9, 0x16);
            }
            world.DisableCollisionsBetween(7, 0x11);
            world.DisableCollisionsBetween(7, 0x15);
            world.DisableCollisionsBetween(7, 0x19);
            world.DisableCollisionsBetween(7, 0x13);
            world.DisableCollisionsBetween(7, 0x1a);
            world.DisableCollisionsBetween(7, 14);
            world.DisableCollisionsBetween(7, 0x1f);
            world.DisableCollisionsBetween(7, 0x1c);
            world.DisableCollisionsBetween(7, 11);
        }

        private void InitStepOptimizer()
        {
            this.m_optimizeNextFrame = false;
            this.m_optimizationsEnabled = false;
            this.m_averageFrameTime = 16.66667f;
            List<IPhysicsStepOptimizer> list1 = new List<IPhysicsStepOptimizer>();
            list1.Add(new DisableGridTOIsOptimizer());
            this.m_optimizers = list1;
        }

        public static bool IsPenetratingShapeShape(HkShape shape1, ref Matrix transform1, HkShape shape2, ref Matrix transform2)
        {
            using (m_raycastLock.Acquire())
            {
                return (Clusters.GetList()[0] as HkWorld).IsPenetratingShapeShape(shape1, ref transform1, shape2, ref transform2);
            }
        }

        public static bool IsPenetratingShapeShape(HkShape shape1, ref Vector3D translation1, ref Quaternion rotation1, HkShape shape2, ref Vector3D translation2, ref Quaternion rotation2)
        {
            bool flag;
            using (m_raycastLock.Acquire())
            {
                using (MyUtils.ReuseCollection<MyClusterTree.MyClusterQueryResult>(ref m_resultWorlds))
                {
                    rotation1.Normalize();
                    rotation2.Normalize();
                    Clusters.Intersects(translation1, m_resultWorlds);
                    using (List<MyClusterTree.MyClusterQueryResult>.Enumerator enumerator = m_resultWorlds.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyClusterTree.MyClusterQueryResult current = enumerator.Current;
                            if (current.AABB.Contains(translation2) != ContainmentType.Contains)
                            {
                                flag = false;
                            }
                            else
                            {
                                Vector3 vector = (Vector3) (translation1 - current.AABB.Center);
                                BoundingBoxD aABB = current.AABB;
                                Vector3 vector2 = (Vector3) (translation2 - aABB.Center);
                                if (!((HkWorld) current.UserData).IsPenetratingShapeShape(shape1, ref vector, ref rotation1, shape2, ref vector2, ref rotation2))
                                {
                                    continue;
                                }
                                flag = true;
                            }
                            return flag;
                        }
                    }
                    flag = false;
                }
            }
            return flag;
        }

        private void IterateBodies(HkWorld world)
        {
            bool flag;
            int num;
            List<HkRigidBody> activeRigidBodiesCache = world.GetActiveRigidBodiesCache(out num, out flag);
            if (!flag)
            {
                activeRigidBodiesCache.Clear();
                foreach (HkRigidBody body in world.ActiveRigidBodies)
                {
                    MyPhysicsBody userObject = (MyPhysicsBody) body.UserObject;
                    if ((userObject != null) && ((!userObject.IsKinematic || this.m_updateKinematicBodies) && ((userObject.Entity.Parent == null) && (body.Layer != 0x11))))
                    {
                        activeRigidBodiesCache.Add(body);
                    }
                }
                world.UpdateActiveRigidBodiesCache(activeRigidBodiesCache, num);
            }
            using (List<HkRigidBody>.Enumerator enumerator2 = activeRigidBodiesCache.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    MyPhysicsBody userObject = (MyPhysicsBody) enumerator2.Current.UserObject;
                    this.m_iterationBodies.Add(userObject);
                }
            }
        }

        private void IterateCharacters(HkWorld world)
        {
            foreach (HkCharacterRigidBody body in world.CharacterRigidBodies)
            {
                this.m_characterIterationBodies.Add(body);
            }
        }

        public override void LoadData()
        {
            HkVDB.Port = Sync.IsServer ? 0x61a9 : 0x61aa;
            HkBaseSystem.EnableAssert(-668493307, false);
            HkBaseSystem.EnableAssert(0x38c5ec40, false);
            HkBaseSystem.EnableAssert(0x59810264, false);
            HkBaseSystem.EnableAssert(-258736554, false);
            HkBaseSystem.EnableAssert(0x1f476204, false);
            HkBaseSystem.EnableAssert(0x407443ff, false);
            HkBaseSystem.EnableAssert(-1383504214, false);
            HkBaseSystem.EnableAssert(-265005969, false);
            HkBaseSystem.EnableAssert(0x75d662fb, false);
            HkBaseSystem.EnableAssert(-252450131, false);
            HkBaseSystem.EnableAssert(-1400416854, false);
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            Clusters = new MyClusterTree(MySession.Static.WorldBoundaries, MyFakes.MP_SYNC_CLUSTERTREE && !Sync.IsServer);
            Clusters.OnClusterCreated = (Func<int, BoundingBoxD, object>) Delegate.Combine(Clusters.OnClusterCreated, new Func<int, BoundingBoxD, object>(this.OnClusterCreated));
            Clusters.OnClusterRemoved = (Action<object>) Delegate.Combine(Clusters.OnClusterRemoved, new Action<object>(this.OnClusterRemoved));
            Clusters.OnFinishBatch = (Action<object>) Delegate.Combine(Clusters.OnFinishBatch, new Action<object>(this.OnFinishBatch));
            Clusters.OnClustersReordered = (Action) Delegate.Combine(Clusters.OnClustersReordered, new Action(this.Tree_OnClustersReordered));
            Clusters.GetEntityReplicableExistsById = (Func<long, bool>) Delegate.Combine(Clusters.GetEntityReplicableExistsById, new Func<long, bool>(this.GetEntityReplicableExistsById));
            QueuedForces = new Queue<ForceInfo>();
            if (MyFakes.ENABLE_HAVOK_MULTITHREADING)
            {
                this.ParallelSteppingInitialized = false;
                m_threadPool = new HkJobThreadPool();
                m_jobQueue = new HkJobQueue(m_threadPool.ThreadCount + 1);
            }
            HkCylinderShape.SetNumberOfVirtualSideSegments(0x20);
            this.InitStepOptimizer();
        }

        public static void MoveObject(ulong id, BoundingBoxD aabb, Vector3 velocity)
        {
            Clusters.MoveObject(id, aabb, velocity);
        }

        private HkWorld OnClusterCreated(int clusterId, BoundingBoxD bbox) => 
            CreateHkWorld((float) bbox.Size.Max());

        private void OnClusterRemoved(object world)
        {
            HkWorld world2 = (HkWorld) world;
            if (world2.DestructionWorld != null)
            {
                world2.DestructionWorld.Dispose();
                world2.DestructionWorld = null;
            }
            world2.Dispose();
        }

        [Event(null, 0x73e), Reliable, Broadcast]
        private static void OnClustersReordered(List<BoundingBoxD> list)
        {
            DeserializeClusters(list);
        }

        private void OnFinishBatch(object world)
        {
            ((HkWorld) world).FinishBatch();
        }

        private static unsafe void ProcessDestructions()
        {
            int num = 0;
            while (m_destructionQueue.Count > 0)
            {
                num++;
                FractureImpactDetails details = m_destructionQueue.Dequeue();
                HkdFractureImpactDetails details2 = details.Details;
                if (details2.IsValid())
                {
                    HkdFractureImpactDetails* detailsPtr1 = (HkdFractureImpactDetails*) ref details2;
                    detailsPtr1.Flag = details2.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_DELAY_OPERATION;
                    int i = 0;
                    while (true)
                    {
                        if (i >= details2.GetBreakingBody().BreakableBody.BreakableShape.GetChildrenCount())
                        {
                            details.World.DestructionWorld.TriggerDestruction(ref details2);
                            MySyncDestructions.AddDestructionEffect(MyPerGameSettings.CollisionParticle.LargeGridClose, details.ContactInWorld, (Vector3) Vector3D.Forward, 0.2f);
                            MySyncDestructions.AddDestructionEffect(MyPerGameSettings.DestructionParticle.DestructionHit, details.ContactInWorld, (Vector3) Vector3D.Forward, 0.1f);
                            break;
                        }
                        HkdShapeInstanceInfo child = details2.GetBreakingBody().BreakableBody.BreakableShape.GetChild(i);
                        child.Shape.GetStrenght();
                        int num3 = 0;
                        while (true)
                        {
                            HkdBreakableShape shape = child.Shape;
                            if (num3 >= shape.GetChildrenCount())
                            {
                                i++;
                                break;
                            }
                            HkdShapeInstanceInfo info2 = child.Shape.GetChild(num3);
                            info2.Shape.GetStrenght();
                            num3++;
                        }
                    }
                }
                details.Details.RemoveReference();
            }
        }

        public static void ProfileHkCall(Action action)
        {
            HkVDB.SyncTimers(m_threadPool);
            action();
            HkTaskProfiler.ReplayTimers(delegate (string x) {
            }, delegate (long x) {
            });
        }

        private static void ProfilerBegin(string block)
        {
        }

        private static void ProfilerEnd(long elapsedTicks)
        {
            MyTimeSpan.FromTicks(elapsedTicks);
        }

        public static void RemoveDestructions(HkRigidBody body)
        {
            List<FractureImpactDetails> list = m_destructionQueue.ToList<FractureImpactDetails>();
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Details.IsValid() || (list[i].Details.GetBreakingBody() == body))
                {
                    FractureImpactDetails details = list[i];
                    details.Details.RemoveReference();
                    list.RemoveAt(i);
                    i--;
                }
            }
            m_destructionQueue.Clear();
            foreach (FractureImpactDetails details2 in list)
            {
                m_destructionQueue.Enqueue(details2);
            }
        }

        public static void RemoveDestructions(MyEntity entity)
        {
            List<FractureImpactDetails> list = m_destructionQueue.ToList<FractureImpactDetails>();
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i].Entity, entity))
                {
                    list[i].Details.RemoveReference();
                    list.RemoveAt(i);
                    i--;
                }
            }
            m_destructionQueue.Clear();
            foreach (FractureImpactDetails details2 in list)
            {
                m_destructionQueue.Enqueue(details2);
            }
        }

        public static void RemoveObject(ulong id)
        {
            Clusters.RemoveObject(id);
        }

        private static void ReplayHavokTimers()
        {
        }

        public static void SerializeClusters(List<BoundingBoxD> list)
        {
            Clusters.Serialize(list);
        }

        [Event(null, 0xa6), Reliable, Server]
        public static void SetScheduling(bool multithread, bool parallel)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyFakes.ENABLE_HAVOK_MULTITHREADING = multithread;
                MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING = parallel;
            }
        }

        public override void Simulate()
        {
            if (MySandboxGame.IsGameReady)
            {
                this.AddTimestamp();
                if (!MyFakes.PAUSE_PHYSICS || MyFakes.STEP_PHYSICS)
                {
                    MyFakes.STEP_PHYSICS = false;
                    MySimpleProfiler.Begin("Physics", MySimpleProfiler.ProfilingBlockType.OTHER, "Simulate");
                    while (QueuedForces.Count > 0)
                    {
                        ForceInfo info = QueuedForces.Dequeue();
                        info.Body.AddForce(info.Type, info.Force, info.Position, info.Torque, info.MaxSpeed, true, info.ActiveOnly);
                    }
                    using (m_raycastLock.Acquire())
                    {
                        this.SimulateInternal();
                    }
                    this.m_iterationBodies.Clear();
                    this.m_characterIterationBodies.Clear();
                    if ((Sync.IsServer && MyFakes.MP_SYNC_CLUSTERTREE) && ClustersNeedSync)
                    {
                        List<BoundingBoxD> list = new List<BoundingBoxD>();
                        SerializeClusters(list);
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<List<BoundingBoxD>>(s => new Action<List<BoundingBoxD>>(MyPhysics.OnClustersReordered), list, targetEndpoint, position);
                        ClustersNeedSync = false;
                    }
                    MySimpleProfiler.End("Simulate");
                }
            }
        }

        private void SimulateInternal()
        {
            InsideSimulation = true;
            this.ExecuteParallelRayCasts();
            HkBaseSystem.OnSimulationFrameStarted((long) MySandboxGame.Static.SimulationFrameCounter);
            StepVDB();
            ProcessDestructions();
            this.StepWorlds();
            HkBaseSystem.OnSimulationFrameFinished();
            InsideSimulation = false;
            ReplayHavokTimers();
            this.DrawIslands();
            this.UpdateActiveRigidBodies();
            this.UpdateCharacters();
            this.EnsureClusterSpace();
        }

        private void StepSingleWorld(HkWorld world)
        {
            bool flag1 = MyFakes.DEFORMATION_LOGGING;
            world.ExecutePendingCriticalOperations();
            world.UnmarkForWrite();
            if (!MyFakes.TWO_STEP_SIMULATIONS)
            {
                world.StepSimulation(0.01666667f * MyFakes.SIMULATION_SPEED, MyFakes.ENABLE_HAVOK_MULTITHREADING);
            }
            else
            {
                world.StepSimulation(0.008333334f * MyFakes.SIMULATION_SPEED, MyFakes.ENABLE_HAVOK_MULTITHREADING);
                world.StepSimulation(0.008333334f * MyFakes.SIMULATION_SPEED, MyFakes.ENABLE_HAVOK_MULTITHREADING);
            }
            world.MarkForWrite();
            bool flag2 = MyFakes.DEFORMATION_LOGGING;
        }

        private static void StepVDB()
        {
            if ((MyInput.Static.ENABLE_DEVELOPER_KEYS || (ClientCameraWM != null)) || SyncVDBCamera)
            {
                MatrixD identity = MatrixD.Identity;
                if (MySector.MainCamera != null)
                {
                    identity = MySector.MainCamera.WorldMatrix;
                }
                HkWorld userData = null;
                Vector3D zero = Vector3D.Zero;
                if (Sync.IsDedicated && (ClientCameraWM != null))
                {
                    MyClusterTree.MyCluster clusterForPosition = Clusters.GetClusterForPosition(ClientCameraWM.Value.Translation);
                    if (clusterForPosition != null)
                    {
                        zero = -clusterForPosition.AABB.Center;
                        userData = (HkWorld) clusterForPosition.UserData;
                    }
                }
                if (userData == null)
                {
                    if ((MyFakes.VDB_ENTITY != null) && (MyFakes.VDB_ENTITY.GetTopMostParent(null).GetPhysicsBody() != null))
                    {
                        MyPhysicsBody physicsBody = MyFakes.VDB_ENTITY.GetTopMostParent(null).GetPhysicsBody();
                        zero = physicsBody.WorldToCluster(Vector3D.Zero);
                        userData = physicsBody.HavokWorld;
                    }
                    else if ((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).GetPhysicsBody() != null))
                    {
                        MyPhysicsBody physicsBody = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).GetPhysicsBody();
                        zero = physicsBody.WorldToCluster(Vector3D.Zero);
                        userData = physicsBody.HavokWorld;
                    }
                    else if (Clusters.GetList().Count > 0)
                    {
                        MyClusterTree.MyCluster local1 = Clusters.GetClusters()[0];
                        zero = -local1.AABB.Center;
                        userData = (HkWorld) local1.UserData;
                    }
                }
                if (userData != null)
                {
                    HkVDB.SyncTimers(m_threadPool);
                    HkVDB.StepVDB(userData, 0.01666667f);
                    if (Sync.IsDedicated)
                    {
                        if (ClientCameraWM != null)
                        {
                            identity = ClientCameraWM.Value;
                        }
                    }
                    else if (!Sync.IsServer && SyncVDBCamera)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<MatrixD>(x => new Action<MatrixD>(MyPhysics.UpdateServerDebugCamera), identity, targetEndpoint, position);
                    }
                    Vector3 up = (Vector3) identity.Up;
                    Vector3 from = (Vector3) (identity.Translation + zero);
                    Vector3 to = from + identity.Forward;
                    HkVDB.UpdateCamera(ref from, ref to, ref up);
                    bool flag = VDBRecordFile != null;
                    if (IsVDBRecording != flag)
                    {
                        IsVDBRecording = flag;
                        if (flag)
                        {
                            HkVDB.Capture(VDBRecordFile);
                        }
                        else
                        {
                            HkVDB.EndCapture();
                        }
                    }
                }
            }
        }

        private void StepWorlds()
        {
            if (!MyFakes.ENABLE_HAVOK_STEP_OPTIMIZERS)
            {
                this.StepWorldsInternal(null);
            }
            else
            {
                if (this.m_optimizeNextFrame)
                {
                    this.m_optimizeNextFrame = false;
                    this.StepWorldsInternal(this.m_timings);
                    this.EnableOptimizations(this.m_timings);
                    this.m_timings.Clear();
                }
                else
                {
                    int num1;
                    this.StepWorldsInternal(null);
                    double totalMilliseconds = Stopwatch.StartNew().Elapsed.TotalMilliseconds;
                    if ((totalMilliseconds <= (this.m_averageFrameTime * 3f)) || (totalMilliseconds <= 10.0))
                    {
                        num1 = (int) (totalMilliseconds >= 30.0);
                    }
                    else
                    {
                        num1 = 1;
                    }
                    bool flag = (bool) num1;
                    bool flag2 = (totalMilliseconds < (this.m_averageFrameTime * 1.5f)) && !flag;
                    int num2 = flag ? 0x3e8 : 100;
                    this.m_averageFrameTime = (this.m_averageFrameTime * (((float) (num2 - 1)) / ((float) num2))) + ((float) (totalMilliseconds * (1f / ((float) num2))));
                    bool flag1 = MyFakes.ENABLE_HAVOK_STEP_OPTIMIZERS_TIMINGS;
                    if (this.m_optimizationsEnabled)
                    {
                        if (!flag2)
                        {
                            this.m_fastFrames = 0;
                        }
                        else
                        {
                            this.m_fastFrames++;
                            if (this.m_fastFrames > 10)
                            {
                                this.DisableOptimizations();
                            }
                        }
                    }
                    else if (!flag)
                    {
                        if (flag2 && (this.m_slowFrames > 0))
                        {
                            this.m_slowFrames--;
                        }
                    }
                    else
                    {
                        this.m_slowFrames++;
                        if (this.m_slowFrames > 10)
                        {
                            this.m_slowFrames = 0;
                            this.m_optimizeNextFrame = true;
                        }
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_TOI_OPTIMIZED_GRIDS)
                {
                    DisableGridTOIsOptimizer.Static.DebugDraw();
                }
            }
        }

        private void StepWorldsInternal(List<MyTuple<HkWorld, MyTimeSpan>> timings)
        {
            if (timings != null)
            {
                this.StepWorldsMeasured(timings);
            }
            else if (MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING)
            {
                this.StepWorldsParallel();
            }
            else
            {
                this.StepWorldsSequential();
            }
            if (HkBaseSystem.IsOutOfMemory)
            {
                throw new OutOfMemoryException("Havok run out of memory");
            }
        }

        private void StepWorldsMeasured(List<MyTuple<HkWorld, MyTimeSpan>> timings)
        {
            foreach (HkWorld world in Clusters.GetList())
            {
                this.StepSingleWorld(world);
                MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.StartNew().ElapsedTicks);
                timings.Add(MyTuple.Create<HkWorld, MyTimeSpan>(world, span));
            }
        }

        private void StepWorldsParallel()
        {
            ListReader<object> list = Clusters.GetList();
            m_jobQueue.WaitPolicy = HkJobQueue.WaitPolicyT.WAIT_INDEFINITELY;
            m_threadPool.ExecuteJobQueue(m_jobQueue);
            foreach (HkWorld world1 in list)
            {
                world1.ExecutePendingCriticalOperations();
                world1.UnmarkForWrite();
                world1.initMtStep(m_jobQueue, 0.01666667f * MyFakes.SIMULATION_SPEED);
            }
            m_jobQueue.WaitPolicy = HkJobQueue.WaitPolicyT.WAIT_UNTIL_ALL_WORK_COMPLETE;
            m_jobQueue.ProcessAllJobs();
            m_threadPool.WaitForCompletion();
            foreach (HkWorld world2 in list)
            {
                world2.finishMtStep(m_jobQueue, m_threadPool);
                world2.MarkForWrite();
            }
        }

        private void StepWorldsSequential()
        {
            foreach (HkWorld world in Clusters.GetList())
            {
                this.StepSingleWorld(world);
            }
        }

        private void Tree_OnClustersReordered()
        {
            MySandboxGame.Log.WriteLine("Clusters reordered");
            ClustersNeedSync = true;
        }

        protected override void UnloadData()
        {
            Clusters.Dispose();
            Clusters.OnClusterCreated = (Func<int, BoundingBoxD, object>) Delegate.Remove(Clusters.OnClusterCreated, new Func<int, BoundingBoxD, object>(this.OnClusterCreated));
            Clusters.OnClusterRemoved = (Action<object>) Delegate.Remove(Clusters.OnClusterRemoved, new Action<object>(this.OnClusterRemoved));
            Clusters = null;
            QueuedForces = null;
            if (MyFakes.ENABLE_HAVOK_MULTITHREADING)
            {
                m_threadPool.RemoveReference();
                m_threadPool = null;
                m_jobQueue.Dispose();
                m_jobQueue = null;
            }
            m_destructionQueue.Clear();
            if (MyPerGameSettings.Destruction)
            {
                HkdBreakableShape.DisposeSharedMaterial();
            }
            this.UnloadStepOptimizer();
        }

        private void UnloadStepOptimizer()
        {
            using (List<IPhysicsStepOptimizer>.Enumerator enumerator = this.m_optimizers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Unload();
                }
            }
            this.m_optimizers.Clear();
        }

        private void UpdateActiveRigidBodies()
        {
            long num = 0L;
            foreach (HkWorld world in Clusters.GetList())
            {
                this.IterateBodies(world);
                num += world.ActiveRigidBodies.Count;
            }
            MyPerformanceCounter.PerCameraDrawWrite["Active rigid bodies"] = num;
            Clusters.SuppressClusterReorder = true;
            foreach (MyPhysicsBody body in this.m_iterationBodies)
            {
                if (!this.m_updateKinematicBodies || !body.IsKinematic)
                {
                    body.OnMotion(body.RigidBody, 0.01666667f, false);
                    continue;
                }
                body.OnMotionKinematic(body.RigidBody);
            }
            Clusters.SuppressClusterReorder = false;
        }

        private void UpdateCharacters()
        {
            foreach (HkWorld world in Clusters.GetList())
            {
                this.IterateCharacters(world);
            }
            Clusters.SuppressClusterReorder = true;
            using (List<HkCharacterRigidBody>.Enumerator enumerator2 = this.m_characterIterationBodies.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    int num1;
                    MyPhysicsBody userObject = (MyPhysicsBody) enumerator2.Current.GetHitRigidBody().UserObject;
                    if (userObject.Entity.Parent == null)
                    {
                        num1 = (int) (Vector3D.DistanceSquared(userObject.Entity.WorldMatrix.Translation, userObject.GetWorldMatrix().Translation) > 9.9999997473787516E-05);
                    }
                    else
                    {
                        num1 = 0;
                    }
                    MyCharacter entity = userObject.Entity as MyCharacter;
                    if (entity != null)
                    {
                        entity.UpdatePhysicalMovement();
                    }
                    if (num1 != 0)
                    {
                        userObject.UpdateCluster();
                    }
                }
            }
            Clusters.SuppressClusterReorder = false;
        }

        [Event(null, 0x77b), Reliable, Server]
        private static void UpdateServerDebugCamera(MatrixD wm)
        {
            if (MyEventContext.Current.IsLocallyInvoked || MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                ClientCameraWM = new MatrixD?(wm);
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        public static int StepsLastSecond =>
            m_timestamps.Count;

        public static float SimulationRatio
        {
            get
            {
                if (MyFakes.ENABLE_SIMSPEED_LOCKING || MyFakes.PRECISE_SIM_SPEED)
                {
                    return Sandbox.Engine.Platform.Game.SimulationRatio;
                }
                return (float) Math.Round((double) (Math.Max(0.5f, (float) StepsLastSecond) / 60f), 2);
            }
        }

        public static float RestingVelocity =>
            (MyPerGameSettings.BallFriendlyPhysics ? 3f : float.MaxValue);

        public static Queue<ForceInfo> QueuedForces
        {
            [CompilerGenerated]
            get => 
                <QueuedForces>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<QueuedForces>k__BackingField = value);
        }

        public static SpinLockRef RaycastLock =>
            m_raycastLock;

        public static HkWorld SingleWorld =>
            (Clusters.GetList()[0] as HkWorld);

        public static bool InsideSimulation
        {
            [CompilerGenerated]
            get => 
                <InsideSimulation>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<InsideSimulation>k__BackingField = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPhysics.<>c <>9 = new MyPhysics.<>c();
            public static Func<IMyEventOwner, Action<bool, bool>> <>9__29_0;
            public static Action <>9__67_0;
            public static Action <>9__77_0;
            public static Func<IMyEventOwner, Action<List<BoundingBoxD>>> <>9__78_0;
            public static Func<IMyEventOwner, Action<MatrixD>> <>9__79_0;
            public static Action<string> <>9__127_0;
            public static Action<long> <>9__127_1;

            internal Action<bool, bool> <CommitSchedulingSettingToServer>b__29_0(IMyEventOwner x) => 
                new Action<bool, bool>(MyPhysics.SetScheduling);

            internal void <ProfileHkCall>b__127_0(string x)
            {
            }

            internal void <ProfileHkCall>b__127_1(long x)
            {
            }

            internal void <ReplayHavokTimers>b__77_0()
            {
                HkTaskProfiler.ReplayTimers(new Action<string>(MyPhysics.ProfilerBegin), new Action<long>(MyPhysics.ProfilerEnd));
                ProfilerShort.Commit();
            }

            internal Action<List<BoundingBoxD>> <Simulate>b__78_0(IMyEventOwner s) => 
                new Action<List<BoundingBoxD>>(MyPhysics.OnClustersReordered);

            internal Action<MatrixD> <StepVDB>b__79_0(IMyEventOwner x) => 
                new Action<MatrixD>(MyPhysics.UpdateServerDebugCamera);

            internal void <UnloadData>b__67_0()
            {
                ProfilerShort.DestroyThread();
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        public struct CollisionLayers
        {
            public const int BlockPlacementTestCollisionLayer = 7;
            public const int MissileLayer = 8;
            public const int NoVoxelCollisionLayer = 9;
            public const int LightFloatingObjectCollisionLayer = 10;
            public const int VoxelLod1CollisionLayer = 11;
            public const int NotCollideWithStaticLayer = 12;
            public const int StaticCollisionLayer = 13;
            public const int CollideWithStaticLayer = 14;
            public const int DefaultCollisionLayer = 15;
            public const int DynamicDoubledCollisionLayer = 0x10;
            public const int KinematicDoubledCollisionLayer = 0x11;
            public const int CharacterCollisionLayer = 0x12;
            public const int NoCollisionLayer = 0x13;
            public const int DebrisCollisionLayer = 20;
            public const int GravityPhantomLayer = 0x15;
            public const int CharacterNetworkCollisionLayer = 0x16;
            public const int FloatingObjectCollisionLayer = 0x17;
            public const int ObjectDetectionCollisionLayer = 0x18;
            public const int VirtualMassLayer = 0x19;
            public const int CollectorCollisionLayer = 0x1a;
            public const int AmmoLayer = 0x1b;
            public const int VoxelCollisionLayer = 0x1c;
            public const int ExplosionRaycastLayer = 0x1d;
            public const int CollisionLayerWithoutCharacter = 30;
            public const int RagdollCollisionLayer = 0x1f;
        }

        private class DeliverData : AbstractWork
        {
            public readonly Action<int> ExecuteJob;
            public readonly List<MyPhysics.ParallelRayCastQuery> Jobs = new List<MyPhysics.ParallelRayCastQuery>();

            public DeliverData()
            {
                this.ExecuteJob = new Action<int>(this.ExecuteJobImpl);
                this.Options = Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.HK_JOB_TYPE_RAYCAST_QUERY, "RayCastResults");
            }

            private void DeliverResults()
            {
                using (List<MyPhysics.ParallelRayCastQuery>.Enumerator enumerator = this.Jobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.DeliverResults();
                    }
                }
                this.Jobs.Clear();
                MyPhysics.m_pendingRayCastsParallelPool.Return(this);
            }

            public override void DoWork(WorkData unused)
            {
                this.DeliverResults();
            }

            private void ExecuteJobImpl(int i)
            {
                this.Jobs[i].ExecuteRayCast();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ForceInfo
        {
            public readonly bool ActiveOnly;
            public readonly float? MaxSpeed;
            public readonly Vector3? Force;
            public readonly Vector3? Torque;
            public readonly Vector3D? Position;
            public readonly MyPhysicsBody Body;
            public readonly MyPhysicsForceType Type;
            public ForceInfo(MyPhysicsBody body, bool activeOnly, float? maxSpeed, Vector3? force, Vector3? torque, Vector3D? position, MyPhysicsForceType type)
            {
                this.Body = body;
                this.Type = type;
                this.Force = force;
                this.Torque = torque;
                this.Position = position;
                this.MaxSpeed = maxSpeed;
                this.ActiveOnly = activeOnly;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FractureImpactDetails
        {
            public HkdFractureImpactDetails Details;
            public HkWorld World;
            public Vector3D ContactInWorld;
            public MyEntity Entity;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HitInfo : IHitInfo
        {
            public HkWorld.HitInfo HkHitInfo;
            public Vector3D Position;
            public HitInfo(HkWorld.HitInfo hi, Vector3D worldPosition)
            {
                this.HkHitInfo = hi;
                this.Position = worldPosition;
            }

            Vector3D IHitInfo.Position =>
                this.Position;
            IMyEntity IHitInfo.HitEntity =>
                this.HkHitInfo.GetHitEntity();
            Vector3 IHitInfo.Normal =>
                this.HkHitInfo.Normal;
            float IHitInfo.Fraction =>
                this.HkHitInfo.HitFraction;
            public override string ToString()
            {
                IMyEntity hitEntity = this.HkHitInfo.GetHitEntity();
                return ((hitEntity == null) ? this.ToString() : hitEntity.ToString());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyContactPointEvent
        {
            public HkContactPointEvent ContactPointEvent;
            public Vector3D Position;
            public Vector3 Normal =>
                this.ContactPointEvent.ContactPoint.Normal;
        }

        private class ParallelRayCastQuery
        {
            private static MyConcurrentPool<MyPhysics.ParallelRayCastQuery> m_pool = new MyConcurrentPool<MyPhysics.ParallelRayCastQuery>(0, null, 0x186a0, null);
            public MyPhysics.ParallelRayCastQuery Next;
            public TKind Kind;
            public Sandbox.Engine.Physics.MyPhysics.RayCastData RayCastData;
            public Sandbox.Engine.Physics.MyPhysics.QueryBoxData QueryBoxData;

            public static MyPhysics.ParallelRayCastQuery Allocate() => 
                m_pool.Get();

            public void DeliverResults()
            {
                try
                {
                    switch (this.Kind)
                    {
                        case TKind.RaycastSingle:
                            ((Action<MyPhysics.HitInfo?>) this.RayCastData.Callback).InvokeIfNotNull<MyPhysics.HitInfo?>(this.RayCastData.HitInfo);
                            break;

                        case TKind.RaycastAll:
                            ((Action<List<MyPhysics.HitInfo>>) this.RayCastData.Callback).InvokeIfNotNull<List<MyPhysics.HitInfo>>(this.RayCastData.Collector);
                            break;

                        case TKind.GetPenetrationsBox:
                            this.QueryBoxData.Callback.InvokeIfNotNull<List<HkBodyCollision>>(this.QueryBoxData.Results);
                            break;

                        default:
                            break;
                    }
                }
                finally
                {
                    this.Return();
                }
            }

            public void ExecuteRayCast()
            {
                switch (this.Kind)
                {
                    case TKind.RaycastSingle:
                        this.RayCastData.HitInfo = MyPhysics.CastRayInternal(ref this.RayCastData.From, ref this.RayCastData.To, this.RayCastData.RayCastFilterLayer);
                        return;

                    case TKind.RaycastAll:
                        MyPhysics.CastRayInternal(ref this.RayCastData.From, ref this.RayCastData.To, this.RayCastData.Collector, this.RayCastData.RayCastFilterLayer);
                        return;

                    case TKind.GetPenetrationsBox:
                        MyPhysics.GetPenetrationsBox(ref this.QueryBoxData.HalfExtents, ref this.QueryBoxData.Translation, ref this.QueryBoxData.Rotation, this.QueryBoxData.Results, this.QueryBoxData.Filter);
                        return;
                }
            }

            public void Return()
            {
                this.Next = null;
                this.RayCastData = new Sandbox.Engine.Physics.MyPhysics.RayCastData();
                this.QueryBoxData = new Sandbox.Engine.Physics.MyPhysics.QueryBoxData();
                m_pool.Return(this);
            }

            public enum TKind
            {
                RaycastSingle,
                RaycastAll,
                GetPenetrationsBox
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct QueryBoxData
        {
            public int Filter;
            public Quaternion Rotation;
            public Vector3 HalfExtents;
            public Vector3D Translation;
            public List<HkBodyCollision> Results;
            public Action<List<HkBodyCollision>> Callback;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RayCastData
        {
            public object Callback;
            public Vector3D To;
            public Vector3D From;
            public Sandbox.Engine.Physics.MyPhysics.HitInfo? HitInfo;
            public int RayCastFilterLayer;
            public List<Sandbox.Engine.Physics.MyPhysics.HitInfo> Collector;
        }
    }
}

