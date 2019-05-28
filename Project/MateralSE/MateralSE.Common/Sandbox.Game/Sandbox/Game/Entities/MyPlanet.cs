namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [MyEntityType(typeof(MyObjectBuilder_Planet), true)]
    public class MyPlanet : MyVoxelBase, IMyOxygenProvider
    {
        private MyDynamicAABBTreeD m_children;
        public const int PHYSICS_SECTOR_SIZE_METERS = 0x400;
        private const double INTRASECTOR_OBJECT_CLUSTER_SIZE = 512.0;
        public static bool RUN_SECTORS;
        private List<BoundingBoxD> m_clustersIntersection = new List<BoundingBoxD>();
        private MyConcurrentDictionary<Vector3I, MyVoxelPhysics> m_physicsShapes;
        private HashSet<Vector3I> m_sectorsPhysicsToRemove = new HashSet<Vector3I>();
        private Vector3I m_numCells;
        private bool m_canSpawnSectors = true;
        private MyPlanetInitArguments m_planetInitValues;
        private List<MyEntity> m_entities = new List<MyEntity>();

        public MyPlanet()
        {
            (base.PositionComp as MyPositionComponent).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            base.Render = new MyRenderComponentPlanet();
            base.AddDebugRenderComponent(new MyDebugRenderComponentPlanet(this));
            base.Render.DrawOutsideViewDistance = true;
        }

        public void AddChildEntity(MyEntity child)
        {
            if (!MyFakes.ENABLE_PLANET_HIERARCHY)
            {
                MyEntities.Add(child, true);
            }
            else
            {
                BoundingBoxD worldAABB = child.PositionComp.WorldAABB;
                int num = this.m_children.AddProxy(ref worldAABB, child, 0, true);
                base.Hierarchy.AddChild(child, true, true);
                child.Components.Get<MyHierarchyComponentBase>().ChildId = num;
            }
        }

        public override void AfterPaste()
        {
        }

        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            if (this.m_physicsShapes != null)
            {
                foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                {
                    if (pair.Value != null)
                    {
                        MySession.Static.VoxelMaps.RemoveVoxelMap(pair.Value);
                        pair.Value.RemoveFromGamePruningStructure();
                    }
                }
            }
            MySession.Static.VoxelMaps.RemoveVoxelMap(this);
            if (base.m_storage != null)
            {
                base.m_storage.RangeChanged -= new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChangedPlanet);
                base.m_storage = null;
            }
            this.Provider = null;
            this.m_planetInitValues = new MyPlanetInitArguments();
        }

        public override void BeforePaste()
        {
        }

        public void ClearPhysicsShapes()
        {
            if (this.m_physicsShapes != null)
            {
                foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.Valid = false;
                    }
                }
            }
        }

        internal void CloseChildEntity(MyEntity child)
        {
            this.RemoveChildEntity(child);
            child.Close();
        }

        protected override void Closing()
        {
            base.Closing();
            if (this.m_physicsShapes != null)
            {
                foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.Close();
                    }
                }
            }
        }

        public void CorrectSpawnLocation(ref Vector3D position, double radius)
        {
            Vector3 vector;
            Vector3D vectord = position - base.WorldMatrix.Translation;
            vectord.Normalize();
            MyVoxelCoordSystems.WorldPositionToLocalPosition(this.PositionLeftBottomCorner, ref position, out vector);
            Vector3D vectord2 = new Vector3D(radius, radius, radius);
            BoundingBox box = new BoundingBox(vector - vectord2, vector + vectord2);
            ContainmentType type = this.Storage.Intersect(ref box, true);
            for (int i = 0; (i < 10) && ((type == ContainmentType.Intersects) || (type == ContainmentType.Contains)); i++)
            {
                Vector3D closestSurfacePointGlobal = this.GetClosestSurfacePointGlobal(ref position);
                position = closestSurfacePointGlobal + (vectord * radius);
                MyVoxelCoordSystems.WorldPositionToLocalPosition(this.PositionLeftBottomCorner, ref position, out vector);
                box = new BoundingBox(vector - vectord2, vector + vectord2);
                type = this.Storage.Intersect(ref box, true);
            }
        }

        public bool CorrectSpawnLocation2(ref Vector3D position, double radius, bool resumeSearch = false)
        {
            Vector3 vector;
            BoundingBox box;
            Vector3D closestSurfacePointGlobal;
            Vector3D vectord = position - base.WorldMatrix.Translation;
            vectord.Normalize();
            Vector3D vectord2 = new Vector3D(radius, radius, radius);
            if (resumeSearch)
            {
                closestSurfacePointGlobal = position;
            }
            else
            {
                MyVoxelCoordSystems.WorldPositionToLocalPosition(this.PositionLeftBottomCorner, ref position, out vector);
                box = new BoundingBox(vector - vectord2, vector + vectord2);
                if (this.Storage.Intersect(ref box, true) == ContainmentType.Disjoint)
                {
                    return true;
                }
                closestSurfacePointGlobal = this.GetClosestSurfacePointGlobal(ref position);
            }
            for (int i = 0; i < 10; i++)
            {
                closestSurfacePointGlobal += vectord * radius;
                MyVoxelCoordSystems.WorldPositionToLocalPosition(this.PositionLeftBottomCorner, ref closestSurfacePointGlobal, out vector);
                box = new BoundingBox(vector - vectord2, vector + vectord2);
                if (this.Storage.Intersect(ref box, true) == ContainmentType.Disjoint)
                {
                    position = closestSurfacePointGlobal;
                    return true;
                }
            }
            return false;
        }

        private MyVoxelPhysics CreateVoxelPhysics(ref Vector3I increment, ref Vector3I_RangeIterator it)
        {
            if (this.m_physicsShapes == null)
            {
                this.m_physicsShapes = new MyConcurrentDictionary<Vector3I, MyVoxelPhysics>(0, null);
            }
            MyVoxelPhysics physics = null;
            if (this.m_physicsShapes.TryGetValue(it.Current, out physics) && (physics != null))
            {
                if ((physics != null) && !physics.Valid)
                {
                    physics.RefreshPhysics(base.m_storage);
                }
            }
            else
            {
                Vector3I storageMin = it.Current * increment;
                Vector3I storageMax = (Vector3I) (storageMin + increment);
                BoundingBox box = new BoundingBox((Vector3) storageMin, (Vector3) storageMax);
                if (this.Storage.Intersect(ref box, false) == ContainmentType.Intersects)
                {
                    physics = new MyVoxelPhysics();
                    physics.Init(base.m_storage, this.PositionLeftBottomCorner + (storageMin * 1f), storageMin, storageMax, this);
                    physics.Save = false;
                    MyEntities.Add(physics, true);
                }
                this.m_physicsShapes[it.Current] = physics;
            }
            return physics;
        }

        public override void DebugDrawPhysics()
        {
            if (this.m_physicsShapes != null)
            {
                foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                {
                    Vector3 min = (pair.Key * 1024f) + this.PositionLeftBottomCorner;
                    BoundingBoxD aabb = new BoundingBoxD(min, min + 1024f);
                    if ((pair.Value == null) || pair.Value.Closed)
                    {
                        MyRenderProxy.DebugDrawAABB(aabb, Color.DarkGreen, 1f, 1f, true, false, false);
                        continue;
                    }
                    pair.Value.Physics.DebugDraw();
                    MyRenderProxy.DebugDrawAABB(aabb, Color.Cyan, 1f, 1f, true, false, false);
                }
            }
        }

        private void GeneratePhysicalShapeForBox(ref Vector3I increment, ref BoundingBoxD shapeBox)
        {
            if (shapeBox.Intersects(base.PositionComp.WorldAABB))
            {
                Vector3I vectori;
                Vector3I vectori2;
                if ((!shapeBox.Valid || !shapeBox.Min.IsValid()) || !shapeBox.Max.IsValid())
                {
                    string message = "Invalid shapeBox: " + ((BoundingBoxD) shapeBox);
                    throw new ArgumentOutOfRangeException("shapeBox", message);
                }
                MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref shapeBox.Min, out vectori);
                MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref shapeBox.Max, out vectori2);
                vectori = (Vector3I) (vectori / 0x400);
                vectori2 = (Vector3I) (vectori2 / 0x400);
                Vector3I_RangeIterator it = new Vector3I_RangeIterator(ref vectori, ref vectori2);
                while (it.IsValid())
                {
                    this.CreateVoxelPhysics(ref increment, ref it);
                    it.MoveNext();
                }
            }
        }

        public float GetAirDensity(Vector3D worldPosition)
        {
            if ((this.Generator != null) && this.Generator.HasAtmosphere)
            {
                return (((float) MathHelper.Clamp((double) (1.0 - (((worldPosition - base.WorldMatrix.Translation).Length() - this.AverageRadius) / ((double) this.AtmosphereAltitude))), (double) 0.0, (double) 1.0)) * this.Generator.Atmosphere.Density);
            }
            return 0f;
        }

        public Vector3D GetClosestSurfacePointGlobal(Vector3D globalPos) => 
            this.GetClosestSurfacePointGlobal(ref globalPos);

        public Vector3D GetClosestSurfacePointGlobal(ref Vector3D globalPos)
        {
            Vector3D translation = base.WorldMatrix.Translation;
            Vector3 localPos = (Vector3) (globalPos - translation);
            return (this.GetClosestSurfacePointLocal(ref localPos) + translation);
        }

        public Vector3D GetClosestSurfacePointLocal(ref Vector3 localPos)
        {
            Vector3 vector;
            if (!localPos.IsValid())
            {
                return Vector3D.Zero;
            }
            this.Provider.Shape.ProjectToSurface(localPos, out vector);
            return vector;
        }

        public int GetInstanceHash() => 
            base.Name.GetHashCode();

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            string text1;
            MyObjectBuilder_Planet objectBuilder = (MyObjectBuilder_Planet) base.GetObjectBuilder(copy);
            objectBuilder.Radius = this.m_planetInitValues.Radius;
            objectBuilder.Seed = this.m_planetInitValues.Seed;
            objectBuilder.HasAtmosphere = this.m_planetInitValues.HasAtmosphere;
            objectBuilder.AtmosphereRadius = this.m_planetInitValues.AtmosphereRadius;
            objectBuilder.MinimumSurfaceRadius = this.m_planetInitValues.MinRadius;
            objectBuilder.MaximumHillRadius = this.m_planetInitValues.MaxRadius;
            objectBuilder.AtmosphereWavelengths = this.m_planetInitValues.AtmosphereWavelengths;
            objectBuilder.GravityFalloff = this.m_planetInitValues.GravityFalloff;
            objectBuilder.MarkAreaEmpty = this.m_planetInitValues.MarkAreaEmpty;
            objectBuilder.AtmosphereSettings = new MyAtmosphereSettings?(this.m_planetInitValues.AtmosphereSettings);
            objectBuilder.SurfaceGravity = this.m_planetInitValues.SurfaceGravity;
            objectBuilder.ShowGPS = this.m_planetInitValues.AddGps;
            objectBuilder.SpherizeWithDistance = this.m_planetInitValues.SpherizeWithDistance;
            MyObjectBuilder_Planet planet2 = objectBuilder;
            if (this.Generator == null)
            {
                text1 = null;
            }
            else
            {
                text1 = this.Generator.Id.SubtypeId.ToString();
            }
            planet2.PlanetGenerator = text1;
            return planet2;
        }

        public override int GetOrePriority() => 
            -1;

        public float GetOxygenForPosition(Vector3D worldPoint) => 
            ((this.Generator != null) ? (!this.Generator.Atmosphere.Breathable ? 0f : (this.GetAirDensity(worldPoint) * this.Generator.Atmosphere.OxygenDensity)) : 0f);

        public float GetWindSpeed(Vector3D worldPosition)
        {
            if (this.Generator == null)
            {
                return 0f;
            }
            float airDensity = this.GetAirDensity(worldPosition);
            return (this.Generator.Atmosphere.MaxWindSpeed * airDensity);
        }

        private void Hierarchy_QueryAABB(BoundingBoxD query, List<MyEntity> results)
        {
            this.m_children.OverlapAllBoundingBox<MyEntity>(ref query, results, 0, false);
        }

        private void Hierarchy_QueryLine(LineD query, List<MyLineSegmentOverlapResult<MyEntity>> results)
        {
            this.m_children.OverlapAllLineSegment<MyEntity>(ref query, results, false);
        }

        private void Hierarchy_QuerySphere(BoundingSphereD query, List<MyEntity> results)
        {
            this.m_children.OverlapAllBoundingSphere<MyEntity>(ref query, results, false);
        }

        public void Init(MyPlanetInitArguments arguments)
        {
            if (!MyFakes.ENABLE_PLANETS)
            {
                throw new PlanetsNotEnabledException();
            }
            base.SyncFlag = true;
            this.m_planetInitValues = arguments;
            object[] args = new object[] { this.m_planetInitValues.ToString() };
            MyLog.Default.Log(MyLogSeverity.Info, "Planet init values: {0}", args);
            if (this.m_planetInitValues.Storage == null)
            {
                MyLog.Default.Log(MyLogSeverity.Error, "MyPlanet.Init: Planet storage is null! Init of the planet was cancelled.", Array.Empty<object>());
            }
            else
            {
                this.Provider = this.m_planetInitValues.Storage.DataProvider as MyPlanetStorageProvider;
                if (this.Provider == null)
                {
                    MyLog.Default.Error("MyPlanet.Init: Planet storage provider is null! Init of the planet was cancelled.", Array.Empty<object>());
                }
                else if (arguments.Generator == null)
                {
                    MyLog.Default.Error("MyPlanet.Init: Planet generator is null! Init of the planet was cancelled.", Array.Empty<object>());
                }
                else
                {
                    this.m_planetInitValues.Radius = this.Provider.Radius;
                    this.m_planetInitValues.MaxRadius = this.Provider.Shape.OuterRadius;
                    this.m_planetInitValues.MinRadius = this.Provider.Shape.InnerRadius;
                    this.Generator = arguments.Generator;
                    this.AtmosphereAltitude = this.Provider.Shape.MaxHillHeight * ((this.Generator != null) ? this.Generator.Atmosphere.LimitAltitude : 1f);
                    base.Init(this.m_planetInitValues.StorageName, this.m_planetInitValues.Storage, this.m_planetInitValues.PositionMinCorner);
                    ((MyStorageBase) this.Storage).InitWriteCache(0x80);
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
                    base.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChangedPlanet);
                    if (this.m_planetInitValues.MarkAreaEmpty && (MyProceduralWorldGenerator.Static != null))
                    {
                        MyProceduralWorldGenerator.Static.MarkEmptyArea(base.PositionComp.GetPosition(), this.m_planetInitValues.MaxRadius);
                    }
                    if (base.Physics != null)
                    {
                        base.Physics.Enabled = false;
                        base.Physics.Close();
                        base.Physics = null;
                    }
                    if (base.Name == null)
                    {
                        base.Name = base.StorageName + "-" + base.EntityId.ToString();
                    }
                    Vector3I size = this.m_planetInitValues.Storage.Size;
                    this.m_numCells = new Vector3I(size.X / 0x400, size.Y / 0x400, size.Z / 0x400);
                    this.m_numCells -= 1;
                    this.m_numCells = Vector3I.Max(Vector3I.Zero, this.m_numCells);
                    base.StorageName = this.m_planetInitValues.StorageName;
                    base.m_storageMax = this.m_planetInitValues.Storage.Size;
                    this.PrepareSectors();
                    if ((this.Generator != null) && (this.Generator.EnvironmentDefinition != null))
                    {
                        if (!base.Components.Contains(typeof(MyPlanetEnvironmentComponent)))
                        {
                            base.Components.Add<MyPlanetEnvironmentComponent>(new MyPlanetEnvironmentComponent());
                        }
                        base.Components.Get<MyPlanetEnvironmentComponent>().InitEnvironment();
                    }
                    base.Components.Add<MyGravityProviderComponent>(new MySphericalNaturalGravityComponent((double) this.m_planetInitValues.MinRadius, (double) this.m_planetInitValues.MaxRadius, (double) this.m_planetInitValues.GravityFalloff, (double) this.m_planetInitValues.SurfaceGravity));
                    base.CreatedByUser = this.m_planetInitValues.UserCreated;
                    base.Render.FadeIn = this.m_planetInitValues.FadeIn;
                }
            }
        }

        public override void Init(MyObjectBuilder_EntityBase builder)
        {
            this.Init(builder, null);
        }

        public override void Init(MyObjectBuilder_EntityBase builder, VRage.Game.Voxels.IMyStorage storage)
        {
            if (!MyFakes.ENABLE_PLANETS)
            {
                throw new PlanetsNotEnabledException();
            }
            base.SyncFlag = true;
            base.Init(builder);
            MyObjectBuilder_Planet planet = (MyObjectBuilder_Planet) builder;
            if (planet != null)
            {
                string[] textArray1 = new string[] { "Planet init info - MutableStorage:", planet.MutableStorage.ToString(), " StorageName:", planet.StorageName, " storage?:", (storage != null).ToString() };
                MyLog.Default.WriteLine(string.Concat(textArray1));
                base.StorageName = !planet.MutableStorage ? $"{planet.StorageName}" : planet.StorageName;
                this.m_planetInitValues.Seed = planet.Seed;
                this.m_planetInitValues.StorageName = base.StorageName;
                this.m_planetInitValues.PositionMinCorner = (Vector3D) planet.PositionAndOrientation.Value.Position;
                this.m_planetInitValues.HasAtmosphere = planet.HasAtmosphere;
                this.m_planetInitValues.AtmosphereRadius = planet.AtmosphereRadius;
                this.m_planetInitValues.AtmosphereWavelengths = planet.AtmosphereWavelengths;
                this.m_planetInitValues.GravityFalloff = planet.GravityFalloff;
                this.m_planetInitValues.MarkAreaEmpty = planet.MarkAreaEmpty;
                this.m_planetInitValues.SurfaceGravity = planet.SurfaceGravity;
                this.m_planetInitValues.AddGps = planet.ShowGPS;
                this.m_planetInitValues.SpherizeWithDistance = planet.SpherizeWithDistance;
                this.m_planetInitValues.Generator = (planet.PlanetGenerator == "") ? null : MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(planet.PlanetGenerator));
                if (this.m_planetInitValues.Generator == null)
                {
                    string msg = $"No definition found for planet generator {planet.PlanetGenerator}.";
                    MyLog.Default.WriteLine(msg);
                    throw new MyIncompatibleDataException(msg);
                }
                MyAtmosphereSettings? atmosphereSettings = this.m_planetInitValues.Generator.AtmosphereSettings;
                this.m_planetInitValues.AtmosphereSettings = (atmosphereSettings != null) ? atmosphereSettings.GetValueOrDefault() : MyAtmosphereSettings.Defaults();
                this.m_planetInitValues.UserCreated = false;
                if (storage != null)
                {
                    this.m_planetInitValues.Storage = storage;
                }
                else
                {
                    this.m_planetInitValues.Storage = MyStorageBase.Load(planet.StorageName, false);
                    if (this.m_planetInitValues.Storage == null)
                    {
                        string msg = $"No storage loaded for planet {planet.StorageName}.";
                        MyLog.Default.WriteLine(msg);
                        throw new MyIncompatibleDataException(msg);
                    }
                }
                this.m_planetInitValues.InitializeComponents = false;
                object[] objArray1 = new object[] { planet.PlanetGenerator ?? "<null>" };
                object[] args = new object[] { planet.PlanetGenerator ?? "<null>" };
                MyLog.Default.Log(MyLogSeverity.Info, "Planet generator name: {0}", args);
                this.Init(this.m_planetInitValues);
            }
        }

        public bool IntersectsWithGravityFast(ref BoundingBoxD boundingBox)
        {
            ContainmentType type;
            new BoundingSphereD(base.PositionComp.GetPosition(), (double) ((MySphericalNaturalGravityComponent) base.Components.Get<MyGravityProviderComponent>()).GravityLimit).Contains(ref boundingBox, out type);
            return (type != ContainmentType.Disjoint);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            MyPlanets.Register(this);
            MyGravityProviderSystem.AddNaturalGravityProvider(base.Components.Get<MyGravityProviderComponent>());
            MyOxygenProviderSystem.AddOxygenGenerator(this);
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            MyPlanets.UnRegister(this);
            MyGravityProviderSystem.RemoveNaturalGravityProvider(base.Components.Get<MyGravityProviderComponent>());
            MyOxygenProviderSystem.RemoveOxygenGenerator(this);
        }

        public void PrefetchShapeOnRay(ref LineD ray)
        {
            Vector3I vectori;
            Vector3I vectori2;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref ray.From, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref ray.To, out vectori2);
            vectori = (Vector3I) (vectori / 0x400);
            vectori2 = (Vector3I) (vectori2 / 0x400);
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                if (this.m_physicsShapes.ContainsKey(iterator.Current))
                {
                    this.m_physicsShapes[iterator.Current].PrefetchShapeOnRay(ref ray);
                }
                iterator.MoveNext();
            }
        }

        private void PrepareSectors()
        {
            this.m_children = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
            base.Hierarchy.QueryAABBImpl = new Action<BoundingBoxD, List<MyEntity>>(this.Hierarchy_QueryAABB);
            base.Hierarchy.QueryLineImpl = new Action<LineD, List<MyLineSegmentOverlapResult<MyEntity>>>(this.Hierarchy_QueryLine);
            base.Hierarchy.QuerySphereImpl = new Action<BoundingSphereD, List<MyEntity>>(this.Hierarchy_QuerySphere);
        }

        public void RemoveChildEntity(MyEntity child)
        {
            if (MyFakes.ENABLE_PLANET_HIERARCHY && ReferenceEquals(child.Parent, this))
            {
                MyHierarchyComponentBase base2 = child.Components.Get<MyHierarchyComponentBase>();
                this.m_children.RemoveProxy((int) base2.ChildId);
                base.Hierarchy.RemoveChild(child, true);
            }
        }

        private void storage_RangeChangedPlanet(Vector3I minChanged, Vector3I maxChanged, MyStorageDataTypeFlags dataChanged)
        {
            Vector3I start = (Vector3I) (minChanged / 0x400);
            Vector3I end = (Vector3I) (maxChanged / 0x400);
            if (this.m_physicsShapes != null)
            {
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                while (iterator.IsValid())
                {
                    MyVoxelPhysics physics;
                    if (this.m_physicsShapes.TryGetValue(iterator.Current, out physics) && (physics != null))
                    {
                        physics.OnStorageChanged(minChanged, maxChanged, dataChanged);
                    }
                    iterator.MoveNext();
                }
            }
            if (base.Render is MyRenderComponentVoxelMap)
            {
                (base.Render as MyRenderComponentVoxelMap).InvalidateRange(minChanged, maxChanged);
            }
            base.OnRangeChanged(minChanged, maxChanged, dataChanged);
            base.ContentChanged = true;
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.UpdateFloraAndPhysics(false);
        }

        public override unsafe void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (this.m_physicsShapes != null)
            {
                foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                {
                    BoundingBoxD worldAABB;
                    if (pair.Value == null)
                    {
                        Vector3 min = (pair.Key * 1024f) + this.PositionLeftBottomCorner;
                        worldAABB = new BoundingBoxD(min, min + 1024f);
                    }
                    else
                    {
                        worldAABB = pair.Value.PositionComp.WorldAABB;
                        Vector3D* vectordPtr1 = (Vector3D*) ref worldAABB.Min;
                        vectordPtr1[0] -= worldAABB.HalfExtents;
                        Vector3D* vectordPtr2 = (Vector3D*) ref worldAABB.Max;
                        vectordPtr2[0] += worldAABB.HalfExtents;
                    }
                    bool flag = false;
                    using (MyUtils.ReuseCollection<MyEntity>(ref this.m_entities))
                    {
                        MyGamePruningStructure.GetTopMostEntitiesInBox(ref worldAABB, this.m_entities, MyEntityQueryType.Both);
                        foreach (MyEntity entity in this.m_entities)
                        {
                            if (entity.Physics == null)
                            {
                                continue;
                            }
                            if (!entity.Physics.IsStatic)
                            {
                                flag = true;
                                continue;
                            }
                            MyCubeGrid grid = entity as MyCubeGrid;
                            if ((grid != null) && !grid.IsStatic)
                            {
                                flag = true;
                            }
                        }
                    }
                    if (!flag)
                    {
                        this.m_sectorsPhysicsToRemove.Add(pair.Key);
                    }
                }
                foreach (Vector3I vectori in this.m_sectorsPhysicsToRemove)
                {
                    MyVoxelPhysics physics;
                    if (this.m_physicsShapes.TryGetValue(vectori, out physics) && (physics != null))
                    {
                        physics.Close();
                    }
                    this.m_physicsShapes.Remove(vectori);
                }
                this.m_sectorsPhysicsToRemove.Clear();
            }
        }

        private unsafe void UpdateFloraAndPhysics(bool serial = false)
        {
            BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
            Vector3D* vectordPtr1 = (Vector3D*) ref worldAABB.Min;
            vectordPtr1[0] -= 1024.0;
            Vector3D* vectordPtr2 = (Vector3D*) ref worldAABB.Max;
            vectordPtr2[0] += 1024f;
            this.UpdatePlanetPhysics(ref worldAABB);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateFloraAndPhysics(true);
            if (this.m_planetInitValues.AddGps)
            {
                MyGps gps1 = new MyGps();
                gps1.Name = base.StorageName;
                gps1.Coords = base.PositionComp.GetPosition();
                gps1.ShowOnHud = true;
                MyGps gps = gps1;
                gps.UpdateHash();
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
            }
        }

        private void UpdatePlanetPhysics(ref BoundingBoxD box)
        {
            Vector3I increment = (Vector3I) (base.m_storage.Size / (this.m_numCells + 1));
            MyGamePruningStructure.GetAproximateDynamicClustersForSize(ref box, 512.0, this.m_clustersIntersection);
            foreach (BoundingBoxD xd in this.m_clustersIntersection)
            {
                xd.Inflate((double) 32.0);
                this.GeneratePhysicalShapeForBox(ref increment, ref xd);
            }
            if (MySession.Static.ControlledEntity != null)
            {
                BoundingBoxD worldAABB = MySession.Static.ControlledEntity.Entity.PositionComp.WorldAABB;
                worldAABB.Inflate((double) 32.0);
                this.GeneratePhysicalShapeForBox(ref increment, ref worldAABB);
            }
            this.m_clustersIntersection.Clear();
        }

        bool IMyOxygenProvider.IsPositionInRange(Vector3D worldPoint)
        {
            if (((this.Generator == null) || !this.Generator.HasAtmosphere) || !this.Generator.Atmosphere.Breathable)
            {
                return false;
            }
            return ((base.WorldMatrix.Translation - worldPoint).Length() < (this.AtmosphereAltitude + this.AverageRadius));
        }

        public float AtmosphereAltitude { get; private set; }

        public MyPlanetStorageProvider Provider { get; private set; }

        public override MyVoxelBase RootVoxel =>
            this;

        public MyPlanetGeneratorDefinition Generator { get; private set; }

        public VRage.Game.Voxels.IMyStorage Storage
        {
            get => 
                base.m_storage;
            set
            {
                bool flag = false;
                if (base.m_storage != null)
                {
                    base.m_storage.RangeChanged -= new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChangedPlanet);
                    flag = true;
                }
                base.m_storage = value;
                base.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChangedPlanet);
                base.m_storageMax = base.m_storage.Size;
                if (flag)
                {
                    this.ClearPhysicsShapes();
                    (base.Render as MyRenderComponentVoxelMap).Clipmap.InvalidateAll();
                }
            }
        }

        public override Vector3D PositionLeftBottomCorner
        {
            get => 
                base.PositionLeftBottomCorner;
            set
            {
                if (value != base.PositionLeftBottomCorner)
                {
                    base.PositionLeftBottomCorner = value;
                    if (this.m_physicsShapes != null)
                    {
                        foreach (KeyValuePair<Vector3I, MyVoxelPhysics> pair in this.m_physicsShapes)
                        {
                            if (pair.Value != null)
                            {
                                Vector3D vectord = this.PositionLeftBottomCorner + ((pair.Key * 0x400) * 1f);
                                pair.Value.PositionLeftBottomCorner = vectord;
                                pair.Value.PositionComp.SetPosition(vectord + (pair.Value.Size * 0.5f), null, false, true);
                            }
                        }
                    }
                }
            }
        }

        public MyPlanetInitArguments GetInitArguments =>
            this.m_planetInitValues;

        public Vector3 AtmosphereWavelengths =>
            this.m_planetInitValues.AtmosphereWavelengths;

        public MyAtmosphereSettings AtmosphereSettings
        {
            get => 
                this.m_planetInitValues.AtmosphereSettings;
            set
            {
                this.m_planetInitValues.AtmosphereSettings = value;
                (base.Render as MyRenderComponentPlanet).UpdateAtmosphereSettings(value);
            }
        }

        public float MinimumRadius =>
            ((this.Provider != null) ? this.Provider.Shape.InnerRadius : 0f);

        public float AverageRadius =>
            ((this.Provider != null) ? this.Provider.Shape.Radius : 0f);

        public float MaximumRadius =>
            ((this.Provider != null) ? this.Provider.Shape.OuterRadius : 0f);

        public float AtmosphereRadius =>
            this.m_planetInitValues.AtmosphereRadius;

        public bool HasAtmosphere =>
            this.m_planetInitValues.HasAtmosphere;

        public bool SpherizeWithDistance =>
            this.m_planetInitValues.SpherizeWithDistance;

        public override MyClipmapScaleEnum ScaleGroup =>
            MyClipmapScaleEnum.Massive;
    }
}

