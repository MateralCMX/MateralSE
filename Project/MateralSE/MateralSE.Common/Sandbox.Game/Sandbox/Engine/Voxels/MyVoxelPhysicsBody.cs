namespace Sandbox.Engine.Voxels
{
    using Havok;
    using ParallelTasks;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Definitions.Components;
    using VRage.Entities.Components;
    using VRage.Factory;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.ObjectBuilders.Definitions.Components;
    using VRage.Profiler;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.DualContouring;
    using VRageMath;
    using VRageRender;

    [MyDependency(typeof(MyVoxelMesherComponent), Critical=true)]
    public class MyVoxelPhysicsBody : MyPhysicsBody
    {
        public static int ActiveVoxelPhysicsBodies = 0;
        public static int ActiveVoxelPhysicsBodiesWithExtendedCache = 0;
        public static bool EnableShapeDiscard = true;
        private const bool EnableAabbPhantom = true;
        private const int ShapeDiscardThreshold = 0;
        private const int ShapeDiscardCheckInterval = 0x12;
        private static Vector3I[] m_cellsToGenerateBuffer = new Vector3I[0x80];
        internal readonly HashSet<Vector3I>[] InvalidCells;
        internal readonly MyPrecalcJobPhysicsBatch[] RunningBatchTask;
        public readonly MyVoxelBase m_voxelMap;
        private bool m_needsShapeUpdate;
        private HkpAabbPhantom m_aabbPhantom;
        private bool m_bodiesInitialized;
        private readonly HashSet<IMyEntity> m_nearbyEntities;
        private MyVoxelMesherComponent m_mesher;
        private readonly FastResourceLock m_nearbyEntitiesLock;
        private readonly MyWorkTracker<MyCellCoord, MyPrecalcJobPhysicsPrefetch> m_workTracker;
        private readonly Vector3I m_cellsOffset;
        private bool m_staticForCluster;
        private float m_phantomExtend;
        private float m_predictionSize;
        private int m_lastDiscardCheck;
        private BoundingBoxI m_queuedRange;
        private bool m_queueInvalidation;
        private int m_hasExtendedCache;
        private int m_voxelQueriesInLastFrames;
        private const int EXTENDED_CACHE_QUERY_THRESHOLD = 0x4e20;
        private const int EXTENDED_CACHE_QUERIES_THRESHOLD = 0x2710;
        private static List<MyCellCoord> m_toBeCancelledCache;
        [ThreadStatic]
        private static List<int> m_indexListCached;
        [ThreadStatic]
        private static List<byte> m_materialListCached;
        [ThreadStatic]
        private static List<Vector3> m_vertexListCached;
        public static bool UseLod1VoxelPhysics = false;

        internal MyVoxelPhysicsBody(MyVoxelBase voxelMap, float phantomExtend, float predictionSize = 3f, bool lazyPhysics = false) : base(voxelMap, RigidBodyFlag.RBF_STATIC)
        {
            this.RunningBatchTask = new MyPrecalcJobPhysicsBatch[2];
            this.m_nearbyEntities = new HashSet<IMyEntity>();
            this.m_nearbyEntitiesLock = new FastResourceLock();
            this.m_workTracker = new MyWorkTracker<MyCellCoord, MyPrecalcJobPhysicsPrefetch>(MyCellCoord.Comparer);
            this.m_cellsOffset = new Vector3I(0, 0, 0);
            this.m_staticForCluster = true;
            this.m_predictionSize = 3f;
            this.m_queuedRange = new BoundingBoxI(-1, -1);
            this.InvalidCells = new HashSet<Vector3I>[] { new HashSet<Vector3I>(), new HashSet<Vector3I>() };
            this.m_predictionSize = predictionSize;
            this.m_phantomExtend = phantomExtend;
            this.m_voxelMap = voxelMap;
            Vector3I vectori1 = this.m_voxelMap.Size >> 3;
            this.m_cellsOffset = this.m_voxelMap.StorageMin >> 3;
            if (!MyFakes.ENABLE_LAZY_VOXEL_PHYSICS || !lazyPhysics)
            {
                this.CreateRigidBodies();
            }
            base.MaterialType = VRage.Game.MyMaterialType.ROCK;
        }

        private void AabbPhantom_CollidableAdded(ref HkpCollidableAddedEvent eventData)
        {
            HkRigidBody rigidBody = eventData.RigidBody;
            if (rigidBody != null)
            {
                List<IMyEntity> allEntities = rigidBody.GetAllEntities();
                if (rigidBody.IsFixedOrKeyframed)
                {
                    allEntities.Clear();
                }
                else
                {
                    if (!this.m_bodiesInitialized)
                    {
                        this.CreateRigidBodies();
                    }
                    foreach (IMyEntity entity in allEntities)
                    {
                        this.AddNearbyEntity(entity);
                    }
                    allEntities.Clear();
                }
            }
        }

        private void AabbPhantom_CollidableRemoved(ref HkpCollidableRemovedEvent eventData)
        {
            if (this.m_mesher != null)
            {
                HkRigidBody rigidBody = eventData.RigidBody;
                if (rigidBody != null)
                {
                    List<IMyEntity> allEntities = rigidBody.GetAllEntities();
                    foreach (IMyEntity entity in allEntities)
                    {
                        MyGridPhysics physics = entity.Physics as MyGridPhysics;
                        MyPhysicsBody body2 = entity.Physics as MyPhysicsBody;
                        if ((physics != null) && (physics.RigidBody == rigidBody))
                        {
                            using (this.m_nearbyEntitiesLock.AcquireExclusiveUsing())
                            {
                                MyPhysicsBody body1 = body2;
                                this.m_nearbyEntities.Remove(entity);
                            }
                        }
                    }
                    allEntities.Clear();
                }
            }
        }

        public override void Activate(object world, ulong clusterObjectID)
        {
            base.Activate(world, clusterObjectID);
            this.ActivatePhantom();
        }

        public override void ActivateBatch(object world, ulong clusterObjectID)
        {
            base.ActivateBatch(world, clusterObjectID);
            this.ActivatePhantom();
        }

        private void ActivatePhantom()
        {
            Vector3 translation = base.GetRigidBodyMatrix().Translation;
            Vector3 vector2 = this.m_voxelMap.SizeInMetres * this.m_phantomExtend;
            BoundingBox boundingBox = new BoundingBox(translation - (0.5f * vector2), translation + (0.5f * vector2));
            if (this.m_aabbPhantom == null)
            {
                this.CreatePhantom(boundingBox);
            }
            else
            {
                this.m_aabbPhantom.Aabb = boundingBox;
            }
            base.HavokWorld.AddPhantom(this.m_aabbPhantom);
            BoundingBoxD xd = new BoundingBoxD(this.m_voxelMap.PositionComp.WorldAABB.Center - (0.5f * vector2), this.m_voxelMap.PositionComp.WorldAABB.Center + (0.5f * vector2));
            List<VRage.Game.Entity.MyEntity> entitiesInAABB = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref xd, false);
            foreach (VRage.Game.Entity.MyEntity entity in entitiesInAABB)
            {
                this.AddNearbyEntity(entity);
            }
            entitiesInAABB.Clear();
        }

        private void AddNearbyEntity(IMyEntity entity)
        {
            if ((entity.Physics != null) && ((entity.Physics.RigidBody != null) || (entity is MyCharacter)))
            {
                HkRigidBody rigidBody = entity.Physics.RigidBody;
                if ((entity is MyCharacter) || (!rigidBody.IsFixedOrKeyframed && (rigidBody.Layer != 20)))
                {
                    using (this.m_nearbyEntitiesLock.AcquireExclusiveUsing())
                    {
                        this.m_nearbyEntities.Add(entity);
                    }
                }
            }
        }

        private void CheckAndDiscardShapes()
        {
            this.m_lastDiscardCheck++;
            if (((this.m_lastDiscardCheck > 0x12) && ((this.m_nearbyEntities.Count == 0) && (this.RigidBody != null))) && EnableShapeDiscard)
            {
                this.m_lastDiscardCheck = 0;
                HkUniformGridShape shape = (HkUniformGridShape) this.GetShape();
                int hitsAndClear = shape.GetHitsAndClear();
                if ((shape.ShapeCount > 0) && (hitsAndClear <= 0))
                {
                    shape.DiscardLargeData();
                    if (this.RigidBody2 != null)
                    {
                        shape = (HkUniformGridShape) this.RigidBody2.GetShape();
                        if (shape.GetHitsAndClear() <= 0)
                        {
                            shape.DiscardLargeData();
                        }
                    }
                }
            }
        }

        private void ClampVoxelCoords(ref Vector3 localPositionMin, ref Vector3 localPositionMax, out Vector3I min, out Vector3I max)
        {
            MyVoxelCoordSystems.LocalPositionToVoxelCoord(ref localPositionMin, out min);
            MyVoxelCoordSystems.LocalPositionToVoxelCoord(ref localPositionMax, out max);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref min, 1);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref max, 1);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref min, out min);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref max, out max);
        }

        public override void Close()
        {
            base.Close();
            this.m_workTracker.CancelAll();
            for (int i = 0; i < this.RunningBatchTask.Length; i++)
            {
                if (this.RunningBatchTask[i] != null)
                {
                    this.RunningBatchTask[i].Cancel();
                    this.RunningBatchTask[i] = null;
                }
            }
            if (this.m_aabbPhantom != null)
            {
                this.m_aabbPhantom.Dispose();
                this.m_aabbPhantom = null;
            }
        }

        public Vector3 ComputeCellCenterOffset(Vector3 bodyLocal) => 
            ((Vector3) (((Vector3I) (bodyLocal / 8f)) * 8f));

        private Vector3 ComputePredictionOffset(IMyEntity entity) => 
            entity.Physics.LinearVelocity;

        internal unsafe VrVoxelMesh CreateMesh(MyCellCoord coord)
        {
            Vector3I* vectoriPtr1 = (Vector3I*) ref coord.CoordInLod;
            vectoriPtr1[0] = (Vector3I) (vectoriPtr1[0] + (this.m_cellsOffset >> coord.Lod));
            Vector3I lodVoxelMin = coord.CoordInLod << 3;
            lodVoxelMin -= 1;
            MyMesherResult result = this.m_mesher.CalculateMesh(coord.Lod, lodVoxelMin, (Vector3I) ((lodVoxelMin + 8) + 2), MyStorageDataTypeFlags.All, MyVoxelRequestFlags.ForPhysics | MyVoxelRequestFlags.SurfaceMaterial, null);
            if (!result.MeshProduced)
            {
            }
            return result.Mesh;
        }

        private void CreatePhantom(BoundingBox boundingBox)
        {
            this.m_aabbPhantom = new HkpAabbPhantom(boundingBox, 0);
            this.m_aabbPhantom.CollidableAdded = new CollidableAddedDelegate(this.AabbPhantom_CollidableAdded);
            this.m_aabbPhantom.CollidableRemoved = new CollidableRemovedDelegate(this.AabbPhantom_CollidableRemoved);
        }

        private void CreateRigidBodies()
        {
            if ((!base.Entity.MarkedForClose && (this.m_mesher != null)) && (base.m_world != null))
            {
                try
                {
                    if (!this.m_bodiesInitialized)
                    {
                        HkUniformGridShape shape;
                        HkUniformGridShapeArgs args;
                        HkMassProperties? nullable;
                        Vector3I vectori = this.m_voxelMap.Size >> 3;
                        HkRigidBody rigidBody = null;
                        if (UseLod1VoxelPhysics)
                        {
                            args = new HkUniformGridShapeArgs {
                                CellsCount = vectori >> 1,
                                CellSize = 16f,
                                CellOffset = 0.5f,
                                CellExpand = 1f
                            };
                            shape = new HkUniformGridShape(args);
                            shape.SetShapeRequestHandler(new RequestShapeBatchBlockingDelegate(this.RequestShapeBlockingLod1));
                            nullable = null;
                            this.CreateFromCollisionObject((HkShape) shape, -this.m_voxelMap.SizeInMetresHalf, this.m_voxelMap.WorldMatrix, nullable, 11);
                            shape.Base.RemoveReference();
                            rigidBody = this.RigidBody;
                            this.RigidBody = null;
                        }
                        args = new HkUniformGridShapeArgs {
                            CellsCount = vectori,
                            CellSize = 8f,
                            CellOffset = 0.5f,
                            CellExpand = 1f
                        };
                        shape = new HkUniformGridShape(args);
                        shape.SetShapeRequestHandler(new RequestShapeBatchBlockingDelegate(this.RequestShapeBlocking));
                        nullable = null;
                        this.CreateFromCollisionObject((HkShape) shape, -this.m_voxelMap.SizeInMetresHalf, this.m_voxelMap.WorldMatrix, nullable, 0x1c);
                        shape.Base.RemoveReference();
                        this.RigidBody.IsEnvironment = true;
                        if (UseLod1VoxelPhysics)
                        {
                            this.RigidBody2 = rigidBody;
                        }
                        if (MyFakes.ENABLE_PHYSICS_HIGH_FRICTION)
                        {
                            this.Friction = 0.65f;
                        }
                        this.m_bodiesInitialized = true;
                        if (this.Enabled)
                        {
                            Matrix rigidBodyMatrix = base.GetRigidBodyMatrix();
                            this.RigidBody.SetWorldMatrix(rigidBodyMatrix);
                            base.m_world.AddRigidBody(this.RigidBody);
                            if (UseLod1VoxelPhysics)
                            {
                                this.RigidBody2.SetWorldMatrix(rigidBodyMatrix);
                                base.m_world.AddRigidBody(this.RigidBody2);
                            }
                        }
                    }
                }
                finally
                {
                }
            }
        }

        internal unsafe HkBvCompressedMeshShape CreateShape(VrVoxelMesh mesh, int lod)
        {
            HkBvCompressedMeshShape shape;
            if (((mesh == null) || (mesh.TriangleCount == 0)) || (mesh.VertexCount == 0))
            {
                return (HkBvCompressedMeshShape) HkShape.Empty;
            }
            using (MyUtils.ReuseCollection<int>(ref m_indexListCached))
            {
                using (MyUtils.ReuseCollection<Vector3>(ref m_vertexListCached))
                {
                    using (MyUtils.ReuseCollection<byte>(ref m_materialListCached))
                    {
                        List<int> indexListCached = m_indexListCached;
                        List<Vector3> vertexListCached = m_vertexListCached;
                        List<byte> materialListCached = m_materialListCached;
                        vertexListCached.EnsureCapacity<Vector3>(mesh.VertexCount);
                        indexListCached.EnsureCapacity<int>(mesh.TriangleCount * 3);
                        materialListCached.EnsureCapacity<byte>(mesh.TriangleCount);
                        int index = 0;
                        while (true)
                        {
                            if (index >= mesh.TriangleCount)
                            {
                                float scale = mesh.Scale;
                                VrVoxelVertex* vertices = mesh.Vertices;
                                Vector3 vector = (Vector3) ((mesh.Start * scale) - (this.m_voxelMap.StorageMin * 1f));
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= mesh.VertexCount)
                                    {
                                        uint userData = 0xfffffffe;
                                        int num5 = 0;
                                        while (true)
                                        {
                                            if (num5 >= mesh.TriangleCount)
                                            {
                                                int[] pinned numArray;
                                                try
                                                {
                                                    int* numPtr;
                                                    byte[] pinned buffer;
                                                    if (((numArray = indexListCached.GetInternalArray<int>()) == null) || (numArray.Length == 0))
                                                    {
                                                        numPtr = null;
                                                    }
                                                    else
                                                    {
                                                        numPtr = numArray;
                                                    }
                                                    try
                                                    {
                                                        byte* numPtr2;
                                                        Vector3[] pinned vectorArray;
                                                        if (((buffer = materialListCached.GetInternalArray<byte>()) == null) || (buffer.Length == 0))
                                                        {
                                                            numPtr2 = null;
                                                        }
                                                        else
                                                        {
                                                            numPtr2 = buffer;
                                                        }
                                                        try
                                                        {
                                                            Vector3* vectorPtr;
                                                            if (((vectorArray = vertexListCached.GetInternalArray<Vector3>()) == null) || (vectorArray.Length == 0))
                                                            {
                                                                vectorPtr = null;
                                                            }
                                                            else
                                                            {
                                                                vectorPtr = vectorArray;
                                                            }
                                                            float physicsConvexRadius = MyPerGameSettings.PhysicsConvexRadius;
                                                            if (userData == -2)
                                                            {
                                                                userData = uint.MaxValue;
                                                            }
                                                            HkBvCompressedMeshShape local1 = new HkBvCompressedMeshShape(vectorPtr, vertexListCached.Count, numPtr, indexListCached.Count, numPtr2, materialListCached.Count, HkWeldingType.None, physicsConvexRadius);
                                                            HkShape.SetUserData((HkShape) local1, userData);
                                                            shape = local1;
                                                        }
                                                        finally
                                                        {
                                                            vectorArray = null;
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        buffer = null;
                                                    }
                                                }
                                                finally
                                                {
                                                    numArray = null;
                                                }
                                                break;
                                            }
                                            VrVoxelTriangle triangle = mesh.Triangles[num5];
                                            byte material = vertices[triangle.V0].Material;
                                            if (userData == -2)
                                            {
                                                userData = material;
                                            }
                                            else if (userData != material)
                                            {
                                                userData = uint.MaxValue;
                                            }
                                            materialListCached.Add(material);
                                            num5++;
                                        }
                                        break;
                                    }
                                    vertexListCached.Add((vertices[num4].Position * scale) + vector);
                                    num4++;
                                }
                                break;
                            }
                            indexListCached.Add(mesh.Triangles[index].V0);
                            indexListCached.Add(mesh.Triangles[index].V2);
                            indexListCached.Add(mesh.Triangles[index].V1);
                            index++;
                        }
                    }
                }
            }
            return shape;
        }

        public override void Deactivate(object world)
        {
            this.DeactivatePhantom();
            base.Deactivate(world);
        }

        public override void DeactivateBatch(object world)
        {
            this.DeactivatePhantom();
            base.DeactivateBatch(world);
        }

        private void DeactivatePhantom()
        {
            base.HavokWorld.RemovePhantom(this.m_aabbPhantom);
            this.m_nearbyEntities.Clear();
        }

        public override unsafe void DebugDraw()
        {
            base.DebugDraw();
            if (((this.m_aabbPhantom != null) && MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MAP_AABB) && this.IsInWorld)
            {
                BoundingBoxD aabb = this.m_aabbPhantom.Aabb;
                aabb.Translate(this.ClusterToWorld(Vector3.Zero));
                MyRenderProxy.DebugDrawAABB(aabb, Color.Orange, 1f, 1f, true, false, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_PHYSICS_PREDICTION)
            {
                foreach (IMyEntity entity in this.m_nearbyEntities)
                {
                    if (!entity.MarkedForClose)
                    {
                        BoundingBoxD xd3;
                        BoundingBoxD worldAABB = entity.WorldAABB;
                        MyRenderProxy.DebugDrawAABB(worldAABB, Color.Bisque, 1f, 1f, true, false, false);
                        MyRenderProxy.DebugDrawLine3D(this.GetWorldMatrix().Translation, worldAABB.Center, Color.Bisque, Color.BlanchedAlmond, true, false);
                        this.GetPrediction(entity, out xd3);
                        MyRenderProxy.DebugDrawAABB(xd3, Color.Crimson, 1f, 1f, true, false, false);
                    }
                }
                using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(this.GetWorldMatrix(), new Color(Color.Cyan, 0.2f), true, false))
                {
                    int num = 0;
                    foreach (KeyValuePair<MyCellCoord, MyPrecalcJobPhysicsPrefetch> pair in this.m_workTracker)
                    {
                        BoundingBoxD xd5;
                        num++;
                        MyCellCoord key = pair.Key;
                        xd5.Min = (Vector3D) (key.CoordInLod << key.Lod);
                        Vector3D* vectordPtr1 = (Vector3D*) ref xd5.Min;
                        vectordPtr1[0] *= 8.0;
                        Vector3D* vectordPtr2 = (Vector3D*) ref xd5.Min;
                        vectordPtr2[0] -= this.m_voxelMap.SizeInMetresHalf;
                        BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref xd5;
                        xdPtr1->Max = xd5.Min + 8f;
                        Color? color = null;
                        aabb.Add(ref xd5, color);
                        if (num > 250)
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal void GenerateAllShapes()
        {
            if (this.m_mesher != null)
            {
                if (!this.m_bodiesInitialized)
                {
                    this.CreateRigidBodies();
                }
                Vector3I zero = Vector3I.Zero;
                Vector3I size = this.m_voxelMap.Size;
                Vector3I end = new Vector3I(0, 0, 0) {
                    X = size.X >> 3,
                    Y = size.Y >> 3,
                    Z = size.Z >> 3
                };
                end = (Vector3I) (end + zero);
                MyPrecalcJobPhysicsPrefetch.Args args = new MyPrecalcJobPhysicsPrefetch.Args {
                    GeometryCell = new MyCellCoord(1, zero),
                    Storage = this.m_voxelMap.Storage,
                    TargetPhysics = this,
                    Tracker = this.m_workTracker
                };
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref zero, ref end);
                while (iterator.IsValid())
                {
                    MyPrecalcJobPhysicsPrefetch.Start(args);
                    iterator.GetNext(out args.GeometryCell.CoordInLod);
                }
            }
        }

        public override MyStringHash GetMaterialAt(Vector3D worldPos)
        {
            MyVoxelMaterialDefinition definition = (this.m_voxelMap != null) ? this.m_voxelMap.GetMaterialAt(ref worldPos) : null;
            return ((definition != null) ? MyStringHash.GetOrCompute(definition.MaterialTypeName) : MyStringHash.NullOrEmpty);
        }

        private void GetPrediction(IMyEntity entity, out BoundingBoxD box)
        {
            Vector3 vctTranlsation = this.ComputePredictionOffset(entity);
            box = entity.WorldAABB;
            if (entity.Physics.AngularVelocity.Sum > 0.03f)
            {
                float num = entity.LocalAABB.HalfExtents.Length();
                box = new BoundingBoxD(box.Center - num, box.Center + num);
            }
            if (box.Extents.Max() > 8.0)
            {
                box.Inflate((double) 8.0);
            }
            else
            {
                box.InflateToMinimum(new Vector3(8f));
            }
            box.Translate(vctTranlsation);
        }

        public HkRigidBody GetRigidBody(int lod)
        {
            if (!UseLod1VoxelPhysics || (lod != 1))
            {
                return this.RigidBody;
            }
            return this.RigidBody2;
        }

        public bool GetShape(int lod, out HkUniformGridShape gridShape)
        {
            HkRigidBody rigidBody = this.GetRigidBody(lod);
            if (rigidBody == null)
            {
                gridShape = new HkUniformGridShape();
                return false;
            }
            gridShape = (HkUniformGridShape) rigidBody.GetShape();
            return true;
        }

        public bool GetShape(int lod, Vector3D localPos, out HkBvCompressedMeshShape mesh)
        {
            HkUniformGridShape shape = (HkUniformGridShape) this.GetRigidBody(lod).GetShape();
            localPos += this.m_voxelMap.SizeInMetresHalf;
            Vector3I vectori = new Vector3I(localPos / ((double) (8f * (1 << (lod & 0x1f)))));
            return shape.GetChild(vectori.X, vectori.Y, vectori.Z, out mesh);
        }

        internal void InvalidateRange(Vector3I minVoxelChanged, Vector3I maxVoxelChanged)
        {
            this.InvalidateRange(minVoxelChanged, maxVoxelChanged, 0);
            if (UseLod1VoxelPhysics)
            {
                this.InvalidateRange(minVoxelChanged, maxVoxelChanged, 1);
            }
        }

        internal unsafe void InvalidateRange(Vector3I minVoxelChanged, Vector3I maxVoxelChanged, int lod)
        {
            if (this.m_bodiesInitialized)
            {
                if (this.m_queueInvalidation)
                {
                    if (this.m_queuedRange.Max.X < 0)
                    {
                        this.m_queuedRange = new BoundingBoxI(minVoxelChanged, maxVoxelChanged);
                    }
                    else
                    {
                        BoundingBoxI box = new BoundingBoxI(minVoxelChanged, maxVoxelChanged);
                        this.m_queuedRange.Include(ref box);
                    }
                }
                else
                {
                    Vector3I vectori;
                    Vector3I vectori2;
                    minVoxelChanged -= 2;
                    maxVoxelChanged = (Vector3I) (maxVoxelChanged + 1);
                    this.m_voxelMap.Storage.ClampVoxelCoord(ref minVoxelChanged, 1);
                    this.m_voxelMap.Storage.ClampVoxelCoord(ref maxVoxelChanged, 1);
                    MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref minVoxelChanged, out vectori);
                    MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref maxVoxelChanged, out vectori2);
                    Vector3I minChanged = (vectori - this.m_cellsOffset) >> lod;
                    Vector3I result = (vectori2 - this.m_cellsOffset) >> lod;
                    Vector3I geometryCellCoord = this.m_voxelMap.Size - 1;
                    Vector3I* vectoriPtr1 = (Vector3I*) ref geometryCellCoord;
                    MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref (Vector3I) ref vectoriPtr1, out geometryCellCoord);
                    geometryCellCoord = geometryCellCoord >> lod;
                    Vector3I* vectoriPtr2 = (Vector3I*) ref result;
                    Vector3I.Min(ref (Vector3I) ref vectoriPtr2, ref geometryCellCoord, out result);
                    HkRigidBody rigidBody = this.GetRigidBody(lod);
                    if ((minChanged == Vector3I.Zero) && (result == geometryCellCoord))
                    {
                        this.m_workTracker.CancelAll();
                    }
                    else
                    {
                        using (MyUtils.ReuseCollection<MyCellCoord>(ref m_toBeCancelledCache))
                        {
                            BoundingBoxI xi2 = new BoundingBoxI(vectori, vectori2);
                            foreach (KeyValuePair<MyCellCoord, MyPrecalcJobPhysicsPrefetch> pair in this.m_workTracker)
                            {
                                if (xi2.Contains(pair.Key.CoordInLod) != ContainmentType.Disjoint)
                                {
                                    m_toBeCancelledCache.Add(pair.Key);
                                }
                            }
                            foreach (MyCellCoord coord in m_toBeCancelledCache)
                            {
                                this.m_workTracker.CancelIfStarted(coord);
                            }
                        }
                    }
                    if (rigidBody != null)
                    {
                        HkUniformGridShape shape = (HkUniformGridShape) rigidBody.GetShape();
                        int size = ((result - minChanged) + 1).Size;
                        if (size >= m_cellsToGenerateBuffer.Length)
                        {
                            m_cellsToGenerateBuffer = new Vector3I[MathHelper.GetNearestBiggerPowerOfTwo(size)];
                        }
                        int num2 = shape.InvalidateRange(ref minChanged, ref result, m_cellsToGenerateBuffer);
                        for (int i = 0; i < num2; i++)
                        {
                            this.StartPrecalcJobPhysicsIfNeeded(lod, i);
                        }
                    }
                    this.m_voxelMap.RaisePhysicsChanged();
                }
            }
        }

        private static bool IsDynamicGrid(HkRigidBody rb, MyGridPhysics grid) => 
            ((grid != null) && ((grid.RigidBody == rb) && !grid.IsStatic));

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (this.m_mesher == null)
            {
                this.m_mesher = new MyVoxelMesherComponent();
                MyPlanet rootVoxel = this.m_voxelMap.RootVoxel as MyPlanet;
                if (rootVoxel != null)
                {
                    MyObjectBuilder_VoxelMesherComponentDefinition mesherPostprocessing = rootVoxel.Generator.MesherPostprocessing;
                    if (mesherPostprocessing != null)
                    {
                        MyVoxelMesherComponentDefinition def = new MyVoxelMesherComponentDefinition();
                        def.Init(mesherPostprocessing, MyModContext.BaseGame);
                        this.m_mesher.Init(def);
                    }
                }
                this.m_mesher.SetContainer(base.Entity.Components);
                this.m_mesher.OnAddedToScene();
            }
            if (!this.m_bodiesInitialized)
            {
                this.CreateRigidBodies();
            }
            ActiveVoxelPhysicsBodies++;
        }

        internal void OnBatchTaskComplete(Dictionary<Vector3I, HkBvCompressedMeshShape> newShapes, int lod)
        {
            if (this.RigidBody != null)
            {
                HkUniformGridShape shape;
                this.GetShape(lod, out shape);
                bool flag = false;
                foreach (KeyValuePair<Vector3I, HkBvCompressedMeshShape> pair in newShapes)
                {
                    Vector3I key = pair.Key;
                    HkBvCompressedMeshShape shape2 = pair.Value;
                    shape.SetChild(key.X, key.Y, key.Z, shape2, HkReferencePolicy.None);
                    flag |= !shape2.IsZero;
                }
                if (flag)
                {
                    this.m_needsShapeUpdate = true;
                }
            }
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            ActiveVoxelPhysicsBodies--;
            if (this.m_hasExtendedCache != 0)
            {
                Interlocked.Decrement(ref ActiveVoxelPhysicsBodiesWithExtendedCache);
            }
        }

        internal void OnTaskComplete(MyCellCoord coord, HkBvCompressedMeshShape childShape)
        {
            if (this.RigidBody != null)
            {
                HkUniformGridShape shape;
                this.GetShape(coord.Lod, out shape);
                shape.SetChild(coord.CoordInLod.X, coord.CoordInLod.Y, coord.CoordInLod.Z, childShape, HkReferencePolicy.None);
                if (!childShape.IsZero)
                {
                    this.m_needsShapeUpdate = true;
                }
            }
        }

        public void PrefetchShapeOnRay(ref LineD ray)
        {
            if (this.m_mesher != null)
            {
                Vector3 vector;
                Vector3 vector2;
                HkUniformGridShape shape;
                int lod = UseLod1VoxelPhysics ? 1 : 0;
                MyVoxelCoordSystems.WorldPositionToLocalPosition(this.m_voxelMap.PositionLeftBottomCorner, ref ray.From, out vector);
                MyVoxelCoordSystems.WorldPositionToLocalPosition(this.m_voxelMap.PositionLeftBottomCorner, ref ray.To, out vector2);
                if (this.GetShape(lod, out shape))
                {
                    if (m_cellsToGenerateBuffer.Length < 0x40)
                    {
                        m_cellsToGenerateBuffer = new Vector3I[0x40];
                    }
                    int num2 = shape.GetHitCellsInRange(vector, vector2, m_cellsToGenerateBuffer);
                    if (num2 != 0)
                    {
                        for (int i = 0; i < num2; i++)
                        {
                            MyCellCoord id = new MyCellCoord(lod, m_cellsToGenerateBuffer[i]);
                            if (!this.m_workTracker.Exists(id))
                            {
                                MyPrecalcJobPhysicsPrefetch.Args args = new MyPrecalcJobPhysicsPrefetch.Args {
                                    TargetPhysics = this,
                                    Tracker = this.m_workTracker,
                                    GeometryCell = id,
                                    Storage = this.m_voxelMap.Storage
                                };
                                MyPrecalcJobPhysicsPrefetch.Start(args);
                            }
                        }
                    }
                }
            }
        }

        private bool QueryEmptyOrFull(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            BoundingBoxI box = new BoundingBoxI(new Vector3I(minX, minY, minZ), new Vector3I(maxX, maxY, maxZ));
            if (box.Volume() < 100f)
            {
                return false;
            }
            bool flag = this.m_voxelMap.Storage.Intersect(ref box, 0, true) != ContainmentType.Intersects;
            BoundingBoxD xd = new BoundingBoxD(new Vector3((float) minX, (float) minY, (float) minZ) * 8f, new Vector3((float) maxX, (float) maxY, (float) maxZ) * 8f);
            xd.TransformFast(base.Entity.WorldMatrix);
            MyOrientedBoundingBoxD xd1 = new MyOrientedBoundingBoxD(xd, base.Entity.WorldMatrix);
            MyRenderProxy.DebugDrawAABB(xd, flag ? Color.Green : Color.Red, 1f, 1f, false, false, false);
            return flag;
        }

        private void RequestShapeBatchBlockingInternal(HkShapeBatch info, bool lod1physics)
        {
            if (!MyPhysics.InsideSimulation && !ReferenceEquals(Thread.CurrentThread, MyUtils.MainThread))
            {
                MyLog.Default.Error("Invalid request shape Thread id: " + Thread.CurrentThread.ManagedThreadId.ToString() + " Stack trace: " + Environment.StackTrace, Array.Empty<object>());
            }
            if (!this.m_voxelMap.MarkedForClose)
            {
                if (!this.m_bodiesInitialized)
                {
                    this.CreateRigidBodies();
                }
                this.m_needsShapeUpdate = true;
                int count = info.Count;
                if (((count > 0x4e20) || (Interlocked.Add(ref this.m_voxelQueriesInLastFrames, count) >= 0x2710)) && (Interlocked.Exchange(ref this.m_hasExtendedCache, 1) == 0))
                {
                    HkUniformGridShape shape;
                    Interlocked.Increment(ref ActiveVoxelPhysicsBodiesWithExtendedCache);
                    if (this.GetShape(lod1physics ? 1 : 0, out shape))
                    {
                        shape.EnableExtendedCache();
                    }
                }
                Parallel.For(0, count, delegate (int i) {
                    Vector3I vectori;
                    info.GetInfo(i, out vectori);
                    int lod = lod1physics ? 1 : 0;
                    MyCellCoord geometryCellCoord = new MyCellCoord(lod, vectori);
                    if (MyDebugDrawSettings.DEBUG_DRAW_REQUEST_SHAPE_BLOCKING)
                    {
                        BoundingBoxD xd;
                        MyVoxelCoordSystems.GeometryCellCoordToWorldAABB(this.m_voxelMap.PositionLeftBottomCorner, ref geometryCellCoord, out xd);
                        MyRenderProxy.DebugDrawAABB(xd, lod1physics ? Color.Yellow : Color.Red, 1f, 1f, false, false, false);
                    }
                    bool flag = false;
                    HkBvCompressedMeshShape empty = (HkBvCompressedMeshShape) HkShape.Empty;
                    MyPrecalcJobPhysicsPrefetch prefetch = this.m_workTracker.Cancel(geometryCellCoord);
                    if (((prefetch != null) && prefetch.ResultComplete) && (Interlocked.Exchange(ref prefetch.Taken, 1) == 0))
                    {
                        flag = true;
                        empty = prefetch.Result;
                    }
                    if (!flag)
                    {
                        VrVoxelMesh mesh = this.CreateMesh(geometryCellCoord);
                        empty = this.CreateShape(mesh, lod);
                        if (mesh != null)
                        {
                            mesh.Dispose();
                        }
                    }
                    info.SetResult(i, ref empty);
                }, 1, WorkPriority.VeryHigh, new WorkOptions?(Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Voxels, "RequestBatch")), true);
            }
        }

        private void RequestShapeBlocking(HkShapeBatch batch)
        {
            this.RequestShapeBatchBlockingInternal(batch, false);
        }

        private void RequestShapeBlockingLod1(HkShapeBatch batch)
        {
            this.RequestShapeBatchBlockingInternal(batch, true);
        }

        private void ScheduleBatchJobs()
        {
            for (int i = 0; i < 2; i++)
            {
                if ((this.InvalidCells[i].Count > 0) && (this.RunningBatchTask[i] == null))
                {
                    MyPrecalcJobPhysicsBatch.Start(this, ref this.InvalidCells[i], i);
                }
            }
        }

        private void SetEmptyShapes(int lod, ref HkUniformGridShape shape, int requiredCellsCount)
        {
            for (int i = 0; i < requiredCellsCount; i++)
            {
                Vector3I coordInLod = m_cellsToGenerateBuffer[i];
                this.m_workTracker.Cancel(new MyCellCoord(lod, coordInLod));
                shape.SetChild(coordInLod.X, coordInLod.Y, coordInLod.Z, (HkBvCompressedMeshShape) HkShape.Empty, HkReferencePolicy.TakeOwnership);
            }
        }

        private void StartPrecalcJobPhysicsIfNeeded(int lod, int i)
        {
            MyCellCoord id = new MyCellCoord(lod, m_cellsToGenerateBuffer[i]);
            if (!this.m_workTracker.Exists(id))
            {
                MyPrecalcJobPhysicsPrefetch.Args args = new MyPrecalcJobPhysicsPrefetch.Args {
                    TargetPhysics = this,
                    Tracker = this.m_workTracker,
                    GeometryCell = id,
                    Storage = this.m_voxelMap.Storage
                };
                MyPrecalcJobPhysicsPrefetch.Start(args);
            }
        }

        internal void UpdateAfterSimulation10()
        {
            this.UpdateRigidBodyShape();
            if (this.m_voxelMap.Storage != null)
            {
                foreach (IMyEntity entity in this.m_nearbyEntities)
                {
                    if (entity != null)
                    {
                        bool flag = false;
                        MyPhysicsBody physics = entity.Physics as MyPhysicsBody;
                        if (((physics != null) && (physics.RigidBody != null)) && ((physics.RigidBody.Layer == 0x17) || (physics.RigidBody.Layer == 10)))
                        {
                            flag = true;
                        }
                        if ((((entity is MyCubeGrid) || flag) && !entity.MarkedForClose) && (entity.Physics != null))
                        {
                            BoundingBoxD xd;
                            this.GetPrediction(entity, out xd);
                            if (xd.Intersects(this.m_voxelMap.PositionComp.WorldAABB))
                            {
                                Vector3I vectori;
                                Vector3I vectori2;
                                HkUniformGridShape shape;
                                int num1;
                                if (flag || !UseLod1VoxelPhysics)
                                {
                                    num1 = 0;
                                }
                                else
                                {
                                    num1 = 1;
                                }
                                int lod = num1;
                                int num2 = 1 << (lod & 0x1f);
                                BoundingBoxD xd2 = xd.TransformFast(this.m_voxelMap.PositionComp.WorldMatrixInvScaled);
                                xd2.Translate(this.m_voxelMap.SizeInMetresHalf);
                                Vector3 max = (Vector3) xd2.Max;
                                Vector3 min = (Vector3) xd2.Min;
                                this.ClampVoxelCoords(ref min, ref max, out vectori, out vectori2);
                                vectori = vectori >> lod;
                                vectori2 = vectori2 >> lod;
                                int size = ((vectori2 - vectori) + 1).Size;
                                if (size >= m_cellsToGenerateBuffer.Length)
                                {
                                    m_cellsToGenerateBuffer = new Vector3I[MathHelper.GetNearestBiggerPowerOfTwo(size)];
                                }
                                if (this.GetShape(lod, out shape))
                                {
                                    if (shape.Base.IsZero)
                                    {
                                        MyAnalyticsHelper.ReportBug("Null voxel shape", "SE-7366", true, @"E:\Repo1\Sources\Sandbox.Game\Engine\Voxels\MyVoxelPhysicsBody.cs", 680);
                                    }
                                    else
                                    {
                                        int requiredCellsCount = shape.GetMissingCellsInRange(ref vectori, ref vectori2, m_cellsToGenerateBuffer);
                                        if (requiredCellsCount != 0)
                                        {
                                            BoundingBoxI box = new BoundingBoxI((vectori * 8) * num2, (Vector3I) (((vectori2 + 1) * 8) * num2));
                                            box.Translate(this.m_voxelMap.StorageMin);
                                            if ((requiredCellsCount > 0) && (this.m_voxelMap.Storage.Intersect(ref box, lod, true) != ContainmentType.Intersects))
                                            {
                                                this.SetEmptyShapes(lod, ref shape, requiredCellsCount);
                                            }
                                            else
                                            {
                                                for (int i = 0; i < requiredCellsCount; i++)
                                                {
                                                    this.StartPrecalcJobPhysicsIfNeeded(lod, i);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                this.ScheduleBatchJobs();
                if (this.m_bodiesInitialized)
                {
                    this.CheckAndDiscardShapes();
                }
            }
        }

        internal void UpdateBeforeSimulation10()
        {
            this.UpdateRigidBodyShape();
            this.m_voxelQueriesInLastFrames = 0;
        }

        private void UpdateRigidBodyShape()
        {
            if (!this.m_bodiesInitialized)
            {
                this.CreateRigidBodies();
            }
            if (this.m_needsShapeUpdate)
            {
                this.m_needsShapeUpdate = false;
                if (this.RigidBody != null)
                {
                    this.RigidBody.UpdateShape();
                }
                if (this.RigidBody2 != null)
                {
                    this.RigidBody2.UpdateShape();
                }
            }
        }

        public override bool IsStatic =>
            true;

        public bool QueueInvalidate
        {
            get => 
                this.m_queueInvalidation;
            set
            {
                this.m_queueInvalidation = value;
                if (!value && (this.m_queuedRange.Max.X >= 0))
                {
                    this.InvalidateRange(this.m_queuedRange.Min, this.m_queuedRange.Max);
                    this.m_queuedRange = new BoundingBoxI(-1, -1);
                }
            }
        }

        public override bool IsStaticForCluster
        {
            get => 
                this.m_staticForCluster;
            set => 
                (this.m_staticForCluster = value);
        }
    }
}

