namespace Sandbox.Game.Entities.Planet
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;

    [MyComponentBuilder(typeof(MyObjectBuilder_PlanetEnvironmentComponent), true)]
    public class MyPlanetEnvironmentComponent : MyEntityComponentBase, IMy2DClipmapManager, IMyEnvironmentOwner
    {
        private List<BoundingBoxD> m_sectorBoxes = new List<BoundingBoxD>();
        private readonly My2DClipmap<MyPlanetEnvironmentClipmapProxy>[] m_clipmaps = new My2DClipmap<MyPlanetEnvironmentClipmapProxy>[6];
        internal My2DClipmap<MyPlanetEnvironmentClipmapProxy> ActiveClipmap;
        internal Dictionary<long, MyEnvironmentSector> PhysicsSectors = new Dictionary<long, MyEnvironmentSector>();
        internal Dictionary<long, MyEnvironmentSector> HeldSectors = new Dictionary<long, MyEnvironmentSector>();
        internal Dictionary<long, MyPlanetEnvironmentClipmapProxy> Proxies = new Dictionary<long, MyPlanetEnvironmentClipmapProxy>();
        internal Dictionary<long, MyPlanetEnvironmentClipmapProxy> OutgoingProxies = new Dictionary<long, MyPlanetEnvironmentClipmapProxy>();
        internal readonly IMyEnvironmentDataProvider[] Providers = new IMyEnvironmentDataProvider[6];
        private MyObjectBuilder_EnvironmentDataProvider[] m_providerData = new MyObjectBuilder_EnvironmentDataProvider[6];
        private float m_cachedVegetationDrawDistance;
        private readonly ManualResetEvent m_parallelSyncPoint = new ManualResetEvent(true);
        private const long ParallelWorkTimeMilliseconds = 100L;
        private const int SequentialWorkCount = 10;
        private bool m_parallelInProgress;
        private readonly HashSet<MyEnvironmentSector> m_sectorsClosing = new HashSet<MyEnvironmentSector>();
        private readonly List<MyEnvironmentSector> m_sectorsClosed = new List<MyEnvironmentSector>();
        private readonly MyIterableComplementSet<MyEnvironmentSector> m_sectorsWithPhysics = new MyIterableComplementSet<MyEnvironmentSector>();
        private readonly MyConcurrentQueue<MyEnvironmentSector> m_sectorsToWorkParallel = new MyConcurrentQueue<MyEnvironmentSector>(10);
        private readonly MyConcurrentQueue<MyEnvironmentSector> m_sectorsToWorkSerial = new MyConcurrentQueue<MyEnvironmentSector>(10);
        private readonly Action m_parallelWorkDelegate;
        private readonly Action m_serialWorkDelegate;
        private readonly Dictionary<long, Operation> m_sectorOperations = new Dictionary<long, Operation>();
        private readonly List<MyPhysicalModelDefinition> m_physicalModels = new List<MyPhysicalModelDefinition>();
        private readonly Dictionary<MyPhysicalModelDefinition, short> m_physicalModelToKey = new Dictionary<MyPhysicalModelDefinition, short>();
        private Dictionary<long, List<MyOrientedBoundingBoxD>> m_obstructorsPerSector;
        private int m_InstanceHash;

        public MyPlanetEnvironmentComponent()
        {
            this.m_parallelWorkDelegate = new Action(this.ParallelWorkCallback);
            this.m_serialWorkDelegate = new Action(this.SerialWorkCallback);
        }

        internal bool CheckOnGraphicsClose(MyEnvironmentSector sector)
        {
            if ((sector.HasPhysics != sector.IsPendingPhysicsToggle) || sector.IsPinned)
            {
                return false;
            }
            this.EnqueueClosing(sector);
            return true;
        }

        public void CloseAll()
        {
            this.m_parallelSyncPoint.Reset();
            foreach (MyEnvironmentSector sector in this.PhysicsSectors.Values)
            {
                sector.EnablePhysics(false);
                if ((sector.LodLevel == -1) && !sector.IsPendingLodSwitch)
                {
                    this.m_sectorsClosing.Add(sector);
                }
            }
            this.m_sectorsWithPhysics.Clear();
            this.PhysicsSectors.Clear();
            for (int i = 0; i < this.m_clipmaps.Length; i++)
            {
                My2DClipmap<MyPlanetEnvironmentClipmapProxy> clipmap;
                this.ActiveFace = i;
                this.ActiveClipmap = clipmap = this.m_clipmaps[i];
                clipmap.Clear();
                this.EvaluateOperations();
            }
            this.ActiveFace = -1;
            this.ActiveClipmap = null;
            foreach (MyEnvironmentSector sector2 in this.m_sectorsClosing)
            {
                if (sector2.HasParallelWorkPending)
                {
                    sector2.DoParallelWork();
                }
                if (sector2.HasSerialWorkPending)
                {
                    sector2.DoSerialWork();
                }
                sector2.Close();
            }
            this.m_sectorsClosing.Clear();
            this.m_sectorsToWorkParallel.Clear();
            this.m_sectorsToWorkSerial.Clear();
            this.m_parallelSyncPoint.Set();
        }

        public void DebugDraw()
        {
            if (MyPlanetEnvironmentSessionComponent.DebugDrawSectors && MyPlanetEnvironmentSessionComponent.DebugDrawDynamicObjectClusters)
            {
                using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(MatrixD.Identity, new Color(Color.Green, 0.2f), true, true))
                {
                    foreach (BoundingBoxD xd in this.m_sectorBoxes)
                    {
                        Color? color = null;
                        aabb.Add(ref xd, color);
                    }
                }
            }
            if (MyPlanetEnvironmentSessionComponent.DebugDrawProxies)
            {
                Dictionary<long, MyPlanetEnvironmentClipmapProxy>.ValueCollection.Enumerator enumerator2;
                using (enumerator2 = this.Proxies.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.DebugDraw(false);
                    }
                }
                using (enumerator2 = this.OutgoingProxies.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.DebugDraw(true);
                    }
                }
            }
            if (MyPlanetEnvironmentSessionComponent.DebugDrawCollisionCheckers && (this.m_obstructorsPerSector != null))
            {
                using (Dictionary<long, List<MyOrientedBoundingBoxD>>.ValueCollection.Enumerator enumerator3 = this.m_obstructorsPerSector.Values.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        using (List<MyOrientedBoundingBoxD>.Enumerator enumerator4 = enumerator3.Current.GetEnumerator())
                        {
                            while (enumerator4.MoveNext())
                            {
                                MyRenderProxy.DebugDrawOBB(enumerator4.Current, Color.Red, 0.1f, true, true, false);
                            }
                        }
                    }
                }
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            MyObjectBuilder_PlanetEnvironmentComponent component = builder as MyObjectBuilder_PlanetEnvironmentComponent;
            if (component != null)
            {
                this.m_providerData = component.DataProviders;
                if (component.SectorObstructions != null)
                {
                    this.CollisionCheckEnabled = true;
                    this.m_obstructorsPerSector = new Dictionary<long, List<MyOrientedBoundingBoxD>>();
                    foreach (MyObjectBuilder_PlanetEnvironmentComponent.ObstructingBox box in component.SectorObstructions)
                    {
                        this.m_obstructorsPerSector[box.SectorId] = new List<MyOrientedBoundingBoxD>();
                        if (box.ObstructingBoxes != null)
                        {
                            foreach (SerializableOrientedBoundingBoxD xd in box.ObstructingBoxes)
                            {
                                this.m_obstructorsPerSector[box.SectorId].Add((MyOrientedBoundingBoxD) xd);
                            }
                        }
                    }
                }
            }
        }

        internal void EnqueueClosing(MyEnvironmentSector sector)
        {
            this.m_sectorsClosing.Add(sector);
        }

        internal void EnqueueOperation(MyPlanetEnvironmentClipmapProxy proxy, int lod, bool close = false)
        {
            Operation operation;
            long id = proxy.Id;
            if (this.m_sectorOperations.TryGetValue(id, out operation))
            {
                operation.LodToSet = lod;
                operation.ShouldClose = close;
                this.m_sectorOperations[id] = operation;
            }
            else
            {
                operation.LodToSet = lod;
                operation.Proxy = proxy;
                operation.ShouldClose = close;
                this.m_sectorOperations.Add(id, operation);
            }
        }

        private unsafe void EnsurePhysicsSector(int x, int y, int face)
        {
            MyEnvironmentSector environmentSector;
            long key = MyPlanetSectorId.MakeSectorId(x, y, face, 0);
            if (!this.PhysicsSectors.TryGetValue(key, out environmentSector))
            {
                MyPlanetEnvironmentClipmapProxy proxy;
                if (this.Proxies.TryGetValue(key, out proxy))
                {
                    environmentSector = proxy.EnvironmentSector;
                    environmentSector.EnablePhysics(true);
                }
                else if (!this.HeldSectors.TryGetValue(key, out environmentSector))
                {
                    environmentSector = this.EnvironmentDefinition.CreateSector();
                    MyEnvironmentSectorParameters parameters = new MyEnvironmentSectorParameters();
                    double leafSize = this.m_clipmaps[face].LeafSize;
                    double num3 = this.m_clipmaps[face].LeafSize / 2.0;
                    int num4 = 1 << ((this.m_clipmaps[face].Depth - 1) & 0x1f);
                    Matrix worldMatrix = (Matrix) this.m_clipmaps[face].WorldMatrix;
                    parameters.SurfaceBasisX = new Vector3(num3, 0.0, 0.0);
                    Vector3.RotateAndScale(ref parameters.SurfaceBasisX, ref worldMatrix, out parameters.SurfaceBasisX);
                    parameters.SurfaceBasisY = new Vector3(0.0, num3, 0.0);
                    Vector3.RotateAndScale(ref parameters.SurfaceBasisY, ref worldMatrix, out parameters.SurfaceBasisY);
                    parameters.Environment = this.EnvironmentDefinition;
                    parameters.Center = Vector3D.Transform(new Vector3D(((x - num4) + 0.5) * leafSize, ((y - num4) + 0.5) * leafSize, 0.0), this.m_clipmaps[face].WorldMatrix);
                    parameters.DataRange = new BoundingBox2I(new Vector2I(x, y), new Vector2I(x, y));
                    parameters.Provider = this.Providers[face];
                    parameters.EntityId = MyPlanetSectorId.MakeSectorEntityId(x, y, 0, face, this.Planet.EntityId);
                    parameters.SectorId = MyPlanetSectorId.MakeSectorId(x, y, face, 0);
                    MyEnvironmentSectorParameters* parametersPtr1 = (MyEnvironmentSectorParameters*) ref parameters;
                    parametersPtr1->Bounds = this.GetBoundingShape(ref parameters.Center, ref parameters.SurfaceBasisX, ref parameters.SurfaceBasisY);
                    environmentSector.Init(this, ref parameters);
                    environmentSector.EnablePhysics(true);
                    this.Planet.AddChildEntity(environmentSector);
                }
                this.PhysicsSectors.Add(key, environmentSector);
            }
            this.m_sectorsWithPhysics.AddOrEnsureOnComplement<MyEnvironmentSector>(environmentSector);
        }

        private void EvaluateOperations()
        {
            foreach (Operation operation in this.m_sectorOperations.Values)
            {
                MyPlanetEnvironmentClipmapProxy proxy = operation.Proxy;
                proxy.EnvironmentSector.SetLod(operation.LodToSet);
                if (operation.ShouldClose && (operation.LodToSet == -1))
                {
                    this.CheckOnGraphicsClose(proxy.EnvironmentSector);
                }
            }
            this.m_sectorOperations.Clear();
        }

        public unsafe Vector3D[] GetBoundingShape(ref Vector3D worldPos, ref Vector3 basisX, ref Vector3 basisY)
        {
            BoundingBox box = BoundingBox.CreateInvalid();
            box.Include(-basisX - basisY);
            box.Include(basisX + basisY);
            box.Translate((Vector3) (worldPos - this.Planet.WorldMatrix.Translation));
            this.Planet.Provider.Shape.GetBounds(ref box);
            float* singlePtr1 = (float*) ref box.Min.Z;
            singlePtr1[0]--;
            float* singlePtr2 = (float*) ref box.Max.Z;
            singlePtr2[0]++;
            Vector3D[] vectordArray = new Vector3D[] { (worldPos - basisX) - basisY, (worldPos + basisX) - basisY, (worldPos - basisX) + basisY, (worldPos + basisX) + basisY };
            for (int i = 0; i < 4; i++)
            {
                Vector3D* vectordPtr1 = (Vector3D*) ref vectordArray[i];
                vectordPtr1[0] -= this.Planet.WorldMatrix.Translation;
                vectordArray[i].Normalize();
                vectordArray[i + 4] = vectordArray[i] * box.Max.Z;
                Vector3D* vectordPtr2 = (Vector3D*) ref vectordArray[i];
                vectordPtr2[0] *= box.Min.Z;
                Vector3D* vectordPtr3 = (Vector3D*) ref vectordArray[i];
                vectordPtr3[0] += this.Planet.WorldMatrix.Translation;
                Vector3D* vectordPtr4 = (Vector3D*) ref vectordArray[i + 4];
                MatrixD worldMatrix = this.Planet.WorldMatrix;
                vectordPtr4[0] += worldMatrix.Translation;
            }
            return vectordArray;
        }

        public List<MyOrientedBoundingBoxD> GetCollidedBoxes(long sectorId)
        {
            List<MyOrientedBoundingBoxD> list;
            if (this.m_obstructorsPerSector.TryGetValue(sectorId, out list))
            {
                this.m_obstructorsPerSector.Remove(sectorId);
            }
            return list;
        }

        public void GetDefinition(ushort index, out MyRuntimeEnvironmentItemInfo def)
        {
            def = this.EnvironmentDefinition.Items[index];
        }

        public MyLogicalEnvironmentSectorBase GetLogicalSector(long packedSectorId)
        {
            int face = MyPlanetSectorId.GetFace(packedSectorId);
            return this.Providers[face].GetLogicalSector(packedSectorId);
        }

        public MyPhysicalModelDefinition GetModelForId(short id) => 
            ((id >= this.m_physicalModels.Count) ? null : this.m_physicalModels[id]);

        public short GetModelId(MyPhysicalModelDefinition def)
        {
            short count;
            if (!this.m_physicalModelToKey.TryGetValue(def, out count))
            {
                count = (short) this.m_physicalModels.Count;
                this.m_physicalModelToKey.Add(def, count);
                this.m_physicalModels.Add(def);
            }
            return count;
        }

        public MyEnvironmentSector GetSectorById(long packedSectorId)
        {
            MyEnvironmentSector sector;
            MyPlanetEnvironmentClipmapProxy proxy;
            return (this.PhysicsSectors.TryGetValue(packedSectorId, out sector) ? sector : (this.Proxies.TryGetValue(packedSectorId, out proxy) ? proxy.EnvironmentSector : null));
        }

        public MyEnvironmentSector GetSectorForPosition(Vector3D positionWorld)
        {
            int num;
            Vector2D vectord2;
            MyPlanetCubemapHelper.ProjectToCube(ref positionWorld - this.PlanetTranslation, out num, out vectord2);
            MyPlanetEnvironmentClipmapProxy handler = this.m_clipmaps[num].GetHandler(vectord2 * this.m_clipmaps[num].FaceHalf);
            return ((handler == null) ? null : handler.EnvironmentSector);
        }

        public void GetSectorsInRange(ref BoundingBoxD bb, List<MyEntity> outSectors)
        {
            (base.Container.Get<MyHierarchyComponentBase>() as MyHierarchyComponent<MyEntity>).QueryAABB(ref bb, outSectors);
        }

        public int GetSeed() => 
            this.m_InstanceHash;

        public void GetSurfaceNormalForPoint(ref Vector3D point, out Vector3D normal)
        {
            normal = point - this.PlanetTranslation;
            normal.Normalize();
        }

        public void InitClearAreasManagement()
        {
            this.m_obstructorsPerSector = new Dictionary<long, List<MyOrientedBoundingBoxD>>();
            BoundingBoxD worldAABB = this.Planet.PositionComp.WorldAABB;
            List<MyEntity> result = new List<MyEntity>();
            MyGamePruningStructure.GetTopMostEntitiesInBox(ref worldAABB, result, MyEntityQueryType.Both);
            foreach (MyEntity entity in result)
            {
                this.RasterSectorsForCollision(entity);
            }
            this.CollisionCheckEnabled = true;
        }

        public unsafe void InitEnvironment()
        {
            this.EnvironmentDefinition = this.Planet.Generator.EnvironmentDefinition;
            this.PlanetTranslation = this.Planet.WorldMatrix.Translation;
            this.m_InstanceHash = this.Planet.GetInstanceHash();
            double faceSize = this.Planet.AverageRadius * Math.Sqrt(2.0);
            double num2 = faceSize / 2.0;
            double sectorSize = this.EnvironmentDefinition.SectorSize;
            for (int i = 0; i < 6; i++)
            {
                Vector3D vectord;
                Vector3D vectord2;
                MatrixD xd2;
                MyPlanetCubemapHelper.GetForwardUp((Base6Directions.Direction) ((byte) i), out vectord, out vectord2);
                Vector3D position = (vectord * num2) + this.PlanetTranslation;
                vectord = -vectord;
                MatrixD.CreateWorld(ref position, ref vectord, ref vectord2, out xd2);
                Vector3D result = new Vector3D(-num2, -num2, 0.0);
                Vector3D* vectordPtr1 = (Vector3D*) ref result;
                Vector3D.Transform(ref (Vector3D) ref vectordPtr1, ref xd2, out result);
                Vector3D vectord5 = new Vector3D(1.0, 0.0, 0.0);
                Vector3D vectord6 = new Vector3D(0.0, 1.0, 0.0);
                Vector3D* vectordPtr2 = (Vector3D*) ref vectord5;
                Vector3D.RotateAndScale(ref (Vector3D) ref vectordPtr2, ref xd2, out vectord5);
                Vector3D* vectordPtr3 = (Vector3D*) ref vectord6;
                Vector3D.RotateAndScale(ref (Vector3D) ref vectordPtr3, ref xd2, out vectord6);
                this.m_clipmaps[i] = new My2DClipmap<MyPlanetEnvironmentClipmapProxy>();
                this.ActiveClipmap = this.m_clipmaps[i];
                this.ActiveFace = i;
                this.m_clipmaps[i].Init(this, ref xd2, sectorSize, faceSize);
                this.ActiveFace = -1;
                MyProceduralEnvironmentProvider provider1 = new MyProceduralEnvironmentProvider();
                provider1.ProviderId = i;
                MyProceduralEnvironmentProvider provider = provider1;
                provider.Init(this, ref result, ref vectord5, ref vectord6, this.ActiveClipmap.LeafSize, this.m_providerData[i]);
                this.Providers[i] = provider;
            }
        }

        internal bool IsQueued(MyPlanetEnvironmentClipmapProxy sector) => 
            this.m_sectorOperations.ContainsKey(sector.Id);

        public override bool IsSerialized() => 
            true;

        private void LazyUpdate()
        {
            foreach (MyEnvironmentSector sector in this.m_sectorsWithPhysics.Set())
            {
                sector.EnablePhysics(false);
                this.PhysicsSectors.Remove(sector.SectorId);
                if (!this.Proxies.ContainsKey(sector.SectorId) && (!this.OutgoingProxies.ContainsKey(sector.SectorId) && !sector.IsPinned))
                {
                    this.m_sectorsClosing.Add(sector);
                }
            }
            this.m_sectorsWithPhysics.ClearSet();
            this.m_sectorsWithPhysics.AllToSet();
            foreach (MyEnvironmentSector sector2 in this.m_sectorsClosing)
            {
                if (!sector2.HasWorkPending())
                {
                    sector2.Close();
                    this.Planet.RemoveChildEntity(sector2);
                    this.m_sectorsClosed.Add(sector2);
                    continue;
                }
                sector2.CancelParallel();
                if (sector2.HasSerialWorkPending)
                {
                    sector2.DoSerialWork();
                }
            }
            foreach (MyEnvironmentSector sector3 in this.m_sectorsClosed)
            {
                this.m_sectorsClosing.Remove(sector3);
            }
            this.m_sectorsClosed.Clear();
        }

        internal void MarkProxyOutgoingProxy(MyPlanetEnvironmentClipmapProxy proxy)
        {
            this.Proxies.Remove(proxy.Id);
            this.OutgoingProxies[proxy.Id] = proxy;
        }

        public override void OnAddedToScene()
        {
            MySession.Static.GetComponent<MyPlanetEnvironmentSessionComponent>().RegisterPlanetEnvironment(this);
        }

        public override void OnRemovedFromScene()
        {
            MySession.Static.GetComponent<MyPlanetEnvironmentSessionComponent>().UnregisterPlanetEnvironment(this);
            this.CloseAll();
        }

        private void ParallelWorkCallback()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.m_parallelSyncPoint.Reset();
            MyPlanet planet = this.Planet;
            if (planet != null)
            {
                using (planet.Pin())
                {
                    if (!planet.MarkedForClose)
                    {
                        MyEnvironmentSector sector;
                        while ((stopwatch.ElapsedMilliseconds < 100) && this.m_sectorsToWorkParallel.TryDequeue(out sector))
                        {
                            using (sector.Pin())
                            {
                                if (sector.MarkedForClose)
                                {
                                    continue;
                                }
                                sector.DoParallelWork();
                            }
                        }
                    }
                }
            }
            this.m_parallelSyncPoint.Set();
        }

        public void ProjectPointToSurface(ref Vector3D center)
        {
            center = this.Planet.GetClosestSurfacePointGlobal(ref center);
        }

        public unsafe void QuerySurfaceParameters(Vector3D localOrigin, ref BoundingBoxD queryBounds, List<Vector3> queries, List<MySurfaceParams> results)
        {
            localOrigin -= this.Planet.PositionLeftBottomCorner;
            using (this.Planet.Storage.Pin())
            {
                MySurfaceParams[] pinned paramsArray;
                BoundingBox request = (BoundingBox) queryBounds.Translate(-this.Planet.PositionLeftBottomCorner);
                this.Planet.Provider.Shape.PrepareCache();
                this.Planet.Provider.Material.PrepareRulesForBox(ref request);
                if (results.Capacity != queries.Count)
                {
                    results.Capacity = queries.Count;
                }
                try
                {
                    MySurfaceParams* paramsPtr;
                    if (((paramsArray = results.GetInternalArray<MySurfaceParams>()) == null) || (paramsArray.Length == 0))
                    {
                        paramsPtr = null;
                    }
                    else
                    {
                        paramsPtr = paramsArray;
                    }
                    for (int i = 0; i < queries.Count; i++)
                    {
                        this.Planet.Provider.ComputeCombinedMaterialAndSurface(queries[i] + localOrigin, true, out (MySurfaceParams) ref (paramsPtr + i));
                        Vector3* vectorPtr1 = (Vector3*) ref paramsPtr[i].Position;
                        vectorPtr1[0] -= localOrigin;
                    }
                }
                finally
                {
                    paramsArray = null;
                }
                results.SetSize<MySurfaceParams>(queries.Count);
            }
        }

        internal int QueuedLod(MyPlanetEnvironmentClipmapProxy sector)
        {
            Operation operation;
            return (!this.m_sectorOperations.TryGetValue(sector.Id, out operation) ? sector.Lod : operation.LodToSet);
        }

        private unsafe void RasterSectorsForCollision(MyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                worldAABB.Inflate((double) 8.0);
                worldAABB.Translate(-this.PlanetTranslation);
                Vector2I vectori = new Vector2I(1 << (this.m_clipmaps[0].Depth & 0x1f)) - 1;
                Vector3D* corners = (Vector3D*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3D))];
                worldAABB.GetCornersUnsafe(corners);
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < 8; i++)
                {
                    Vector3D localPos = corners[i];
                    int num6 = MyPlanetCubemapHelper.FindCubeFace(ref localPos);
                    num2 = num6;
                    num6 = 1 << (num6 & 0x1f);
                    if ((num & ~num6) != 0)
                    {
                        num |= 0x40;
                    }
                    num |= num6;
                }
                int num3 = 0;
                int num4 = 5;
                if ((num & 0x40) == 0)
                {
                    num3 = num4 = num2;
                }
                for (int j = num3; j <= num4; j++)
                {
                    if (((1 << (j & 0x1f)) & num) != 0)
                    {
                        int num8 = 1 << ((this.m_clipmaps[j].Depth - 1) & 0x1f);
                        BoundingBox2D boxd = BoundingBox2D.CreateInvalid();
                        int index = 0;
                        while (true)
                        {
                            Vector2D vectord3;
                            if (index >= 8)
                            {
                                Vector2D* vectordPtr1 = (Vector2D*) ref boxd.Min;
                                vectordPtr1[0] += 1.0;
                                Vector2D* vectordPtr2 = (Vector2D*) ref boxd.Min;
                                vectordPtr2[0] *= num8;
                                Vector2D* vectordPtr3 = (Vector2D*) ref boxd.Max;
                                vectordPtr3[0] += 1.0;
                                Vector2D* vectordPtr4 = (Vector2D*) ref boxd.Max;
                                vectordPtr4[0] *= num8;
                                Vector2I max = new Vector2I((int) boxd.Min.X, (int) boxd.Min.Y);
                                Vector2I min = new Vector2I((int) boxd.Max.X, (int) boxd.Max.Y);
                                Vector2I* vectoriPtr1 = (Vector2I*) ref max;
                                Vector2I.Max(ref (Vector2I) ref vectoriPtr1, ref Vector2I.Zero, out max);
                                Vector2I* vectoriPtr2 = (Vector2I*) ref min;
                                Vector2I.Min(ref (Vector2I) ref vectoriPtr2, ref vectori, out min);
                                int x = max.X;
                                while (x <= min.X)
                                {
                                    int y = max.Y;
                                    while (true)
                                    {
                                        List<MyOrientedBoundingBoxD> list;
                                        if (y > min.Y)
                                        {
                                            x++;
                                            break;
                                        }
                                        long key = MyPlanetSectorId.MakeSectorId(x, y, j, 0);
                                        if (!this.m_obstructorsPerSector.TryGetValue(key, out list))
                                        {
                                            list = new List<MyOrientedBoundingBoxD>();
                                            this.m_obstructorsPerSector.Add(key, list);
                                        }
                                        BoundingBox localAABB = entity.PositionComp.LocalAABB;
                                        localAABB.Inflate((float) 8f);
                                        list.Add(new MyOrientedBoundingBoxD(localAABB, entity.PositionComp.WorldMatrix));
                                        y++;
                                    }
                                }
                                break;
                            }
                            Vector3D localPos = corners[index];
                            MyPlanetCubemapHelper.ProjectForFace(ref localPos, j, out vectord3);
                            boxd.Include(vectord3);
                            index++;
                        }
                    }
                }
            }
        }

        private unsafe void RasterSectorsForPhysics(BoundingBoxD range)
        {
            range.InflateToMinimum(this.EnvironmentDefinition.SectorSize);
            Vector2I vectori = new Vector2I(1 << (this.m_clipmaps[0].Depth & 0x1f)) - 1;
            Vector3D* corners = (Vector3D*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3D))];
            range.GetCornersUnsafe(corners);
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector3D localPos = corners[i];
                int num6 = MyPlanetCubemapHelper.FindCubeFace(ref localPos);
                num2 = num6;
                num6 = 1 << (num6 & 0x1f);
                if ((num & ~num6) != 0)
                {
                    num |= 0x40;
                }
                num |= num6;
            }
            int num3 = 0;
            int num4 = 5;
            if ((num & 0x40) == 0)
            {
                num3 = num4 = num2;
            }
            for (int j = num3; j <= num4; j++)
            {
                if (((1 << (j & 0x1f)) & num) != 0)
                {
                    double leafSize = this.m_clipmaps[j].LeafSize;
                    int num8 = 1 << ((this.m_clipmaps[j].Depth - 1) & 0x1f);
                    BoundingBox2D boxd = BoundingBox2D.CreateInvalid();
                    int index = 0;
                    while (true)
                    {
                        Vector2D vectord3;
                        if (index >= 8)
                        {
                            Vector2D* vectordPtr1 = (Vector2D*) ref boxd.Min;
                            vectordPtr1[0] += 1.0;
                            Vector2D* vectordPtr2 = (Vector2D*) ref boxd.Min;
                            vectordPtr2[0] *= num8;
                            Vector2D* vectordPtr3 = (Vector2D*) ref boxd.Max;
                            vectordPtr3[0] += 1.0;
                            Vector2D* vectordPtr4 = (Vector2D*) ref boxd.Max;
                            vectordPtr4[0] *= num8;
                            Vector2I max = new Vector2I((int) boxd.Min.X, (int) boxd.Min.Y);
                            Vector2I min = new Vector2I((int) boxd.Max.X, (int) boxd.Max.Y);
                            Vector2I* vectoriPtr1 = (Vector2I*) ref max;
                            Vector2I.Max(ref (Vector2I) ref vectoriPtr1, ref Vector2I.Zero, out max);
                            Vector2I* vectoriPtr2 = (Vector2I*) ref min;
                            Vector2I.Min(ref (Vector2I) ref vectoriPtr2, ref vectori, out min);
                            int x = max.X;
                            while (x <= min.X)
                            {
                                int y = max.Y;
                                while (true)
                                {
                                    if (y > min.Y)
                                    {
                                        x++;
                                        break;
                                    }
                                    this.EnsurePhysicsSector(x, y, j);
                                    y++;
                                }
                            }
                            break;
                        }
                        Vector3D localPos = corners[index];
                        MyPlanetCubemapHelper.ProjectForFace(ref localPos, j, out vectord3);
                        boxd.Include(vectord3);
                        index++;
                    }
                }
            }
        }

        internal void RegisterProxy(MyPlanetEnvironmentClipmapProxy proxy)
        {
            this.Proxies.Add(proxy.Id, proxy);
        }

        public void ScheduleWork(MyEnvironmentSector sector, bool parallel)
        {
            if (parallel)
            {
                this.m_sectorsToWorkParallel.Enqueue(sector);
            }
            else
            {
                this.m_sectorsToWorkSerial.Enqueue(sector);
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_PlanetEnvironmentComponent component = new MyObjectBuilder_PlanetEnvironmentComponent {
                DataProviders = new MyObjectBuilder_EnvironmentDataProvider[this.Providers.Length]
            };
            for (int i = 0; i < this.Providers.Length; i++)
            {
                component.DataProviders[i] = this.Providers[i].GetObjectBuilder();
                component.DataProviders[i].Face = (Base6Directions.Direction) ((byte) i);
            }
            if (this.CollisionCheckEnabled && (this.m_obstructorsPerSector.Count > 0))
            {
                component.SectorObstructions = new List<MyObjectBuilder_PlanetEnvironmentComponent.ObstructingBox>();
                foreach (KeyValuePair<long, List<MyOrientedBoundingBoxD>> pair in this.m_obstructorsPerSector)
                {
                    MyObjectBuilder_PlanetEnvironmentComponent.ObstructingBox item = new MyObjectBuilder_PlanetEnvironmentComponent.ObstructingBox {
                        SectorId = pair.Key
                    };
                    item.ObstructingBoxes = new List<SerializableOrientedBoundingBoxD>();
                    if (pair.Value != null)
                    {
                        foreach (MyOrientedBoundingBoxD xd in pair.Value)
                        {
                            item.ObstructingBoxes.Add(xd);
                        }
                    }
                    component.SectorObstructions.Add(item);
                }
            }
            return component;
        }

        private void SerialWorkCallback()
        {
            for (int i = this.m_sectorsToWorkSerial.Count; (i > 0) && (this.m_sectorsToWorkSerial.Count > 0); i--)
            {
                MyEnvironmentSector instance = this.m_sectorsToWorkSerial.Dequeue();
                if (!instance.HasParallelWorkPending)
                {
                    instance.DoSerialWork();
                }
                else
                {
                    this.m_sectorsToWorkSerial.Enqueue(instance);
                }
            }
            this.m_parallelInProgress = false;
        }

        public void SetSectorPinned(MyEnvironmentSector sector, bool pinned)
        {
            if (pinned != sector.IsPinned)
            {
                if (pinned)
                {
                    sector.IsPinned = true;
                    this.HeldSectors.Add(sector.SectorId, sector);
                }
                else
                {
                    sector.IsPinned = false;
                    this.HeldSectors.Remove(sector.SectorId);
                }
            }
        }

        public bool TryGetSector(long id, out MyEnvironmentSector environmentSector) => 
            (!this.PhysicsSectors.TryGetValue(id, out environmentSector) && this.HeldSectors.TryGetValue(id, out environmentSector));

        internal void UnmarkProxyOutgoingProxy(MyPlanetEnvironmentClipmapProxy proxy)
        {
            this.OutgoingProxies.Remove(proxy.Id);
            this.Proxies.Add(proxy.Id, proxy);
        }

        internal void UnregisterOutgoingProxy(MyPlanetEnvironmentClipmapProxy proxy)
        {
            this.OutgoingProxies.Remove(proxy.Id);
        }

        internal void UnregisterProxy(MyPlanetEnvironmentClipmapProxy proxy)
        {
            this.Proxies.Remove(proxy.Id);
        }

        public void Update(bool doLazyUpdates = true, bool forceUpdate = false)
        {
            float num2;
            int maxLod = this.MaxLod;
            if (MySandboxGame.Config.VegetationDrawDistance != null)
            {
                num2 = MySandboxGame.Config.VegetationDrawDistance.Value;
            }
            else
            {
                num2 = 100f;
            }
            if (this.m_cachedVegetationDrawDistance != num2)
            {
                this.m_cachedVegetationDrawDistance = num2;
                this.MaxLod = MathHelper.Log2Floor((int) ((((double) num2) / this.EnvironmentDefinition.SectorSize) + 0.5));
                if (this.MaxLod != maxLod)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= this.m_clipmaps.Length)
                        {
                            this.ActiveFace = -1;
                            this.ActiveClipmap = null;
                            break;
                        }
                        this.ActiveFace = index;
                        this.ActiveClipmap = this.m_clipmaps[index];
                        this.ActiveClipmap.Clear();
                        this.ActiveClipmap.LastPosition = Vector3D.PositiveInfinity;
                        this.EvaluateOperations();
                        index++;
                    }
                }
            }
            this.UpdateClipmaps();
            this.UpdatePhysics();
            if (doLazyUpdates)
            {
                this.LazyUpdate();
            }
            if (!this.m_parallelInProgress)
            {
                if (this.m_sectorsToWorkParallel.Count > 0)
                {
                    if (!forceUpdate)
                    {
                        this.m_parallelInProgress = true;
                        Parallel.Start(this.m_parallelWorkDelegate, this.m_serialWorkDelegate);
                    }
                    else
                    {
                        MyEnvironmentSector sector;
                        while (this.m_sectorsToWorkParallel.TryDequeue(out sector))
                        {
                            sector.DoParallelWork();
                        }
                        while (this.m_sectorsToWorkSerial.TryDequeue(out sector))
                        {
                            sector.DoSerialWork();
                        }
                    }
                }
                else if (this.m_sectorsToWorkSerial.Count > 0)
                {
                    this.SerialWorkCallback();
                }
            }
        }

        private void UpdateClipmaps()
        {
            if (!Game.IsDedicated && (this.m_sectorsToWorkParallel.Count <= 0))
            {
                Vector3D localPos = MySector.MainCamera.Position - this.PlanetTranslation;
                if ((localPos.Length() <= (this.Planet.AverageRadius + this.m_clipmaps[0].FaceHalf)) || (this.Proxies.Count != 0))
                {
                    Vector2D vectord2;
                    int num2;
                    double num = Math.Abs(this.Planet.Provider.Shape.GetDistanceToSurfaceCacheless((Vector3) localPos));
                    MyPlanetCubemapHelper.ProjectToCube(ref localPos, out num2, out vectord2);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2D vectord3;
                        Vector3D vectord4;
                        this.ActiveFace = i;
                        this.ActiveClipmap = this.m_clipmaps[i];
                        MyPlanetCubemapHelper.TranslateTexcoordsToFace(ref vectord2, num2, i, out vectord3);
                        vectord4.X = vectord3.X * this.ActiveClipmap.FaceHalf;
                        vectord4.Y = vectord3.Y * this.ActiveClipmap.FaceHalf;
                        vectord4.Z = ((i ^ num2) != 1) ? num : (num + (this.Planet.AverageRadius * 2f));
                        this.ActiveClipmap.Update(vectord4);
                        this.EvaluateOperations();
                    }
                    this.ActiveFace = -1;
                }
            }
        }

        private unsafe void UpdatePhysics()
        {
            BoundingBoxD worldAABB = this.Planet.PositionComp.WorldAABB;
            Vector3D* vectordPtr1 = (Vector3D*) ref worldAABB.Min;
            vectordPtr1[0] -= 1024.0;
            Vector3D* vectordPtr2 = (Vector3D*) ref worldAABB.Max;
            vectordPtr2[0] += 1024f;
            this.m_sectorBoxes.Clear();
            MyGamePruningStructure.GetAproximateDynamicClustersForSize(ref worldAABB, this.EnvironmentDefinition.SectorSize / 2.0, this.m_sectorBoxes);
            foreach (BoundingBoxD xd2 in this.m_sectorBoxes)
            {
                xd2.Translate(-this.PlanetTranslation);
                xd2.Inflate((double) (this.EnvironmentDefinition.SectorSize / 2.0));
                double num = xd2.Center.Length();
                Vector3D size = xd2.Size;
                double num2 = size.Length() / 2.0;
                if ((num >= (this.Planet.MinimumRadius - num2)) && (num <= (this.Planet.MaximumRadius + num2)))
                {
                    this.RasterSectorsForPhysics(xd2);
                }
            }
        }

        internal int ActiveFace { get; private set; }

        internal MyPlanet Planet =>
            ((MyPlanet) base.Entity);

        internal Vector3D PlanetTranslation { get; private set; }

        public int MaxLod { get; private set; }

        public override string ComponentTypeDebugString =>
            "Planet Environment Component";

        public MyWorldEnvironmentDefinition EnvironmentDefinition { get; private set; }

        MyEntity IMyEnvironmentOwner.Entity =>
            this.Planet;

        public IMyEnvironmentDataProvider DataProvider =>
            null;

        public bool CollisionCheckEnabled { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct Operation
        {
            public MyPlanetEnvironmentClipmapProxy Proxy;
            public int LodToSet;
            public bool ShouldClose;
        }
    }
}

