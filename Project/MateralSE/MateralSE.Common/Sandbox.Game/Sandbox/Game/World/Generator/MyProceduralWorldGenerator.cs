namespace Sandbox.Game.World.Generator
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500, typeof(MyObjectBuilder_WorldGenerator), (Type) null), StaticEventOwner]
    public class MyProceduralWorldGenerator : MySessionComponentBase
    {
        public static MyProceduralWorldGenerator Static;
        private int m_seed;
        private double m_objectDensity = -1.0;
        private List<MyProceduralWorldModule> m_modules = new List<MyProceduralWorldModule>();
        private Dictionary<MyEntity, MyEntityTracker> m_trackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private Dictionary<MyEntity, MyEntityTracker> m_toAddTrackedEntities = new Dictionary<MyEntity, MyEntityTracker>();
        private HashSet<EmptyArea> m_markedAreas = new HashSet<EmptyArea>();
        private HashSet<EmptyArea> m_deletedAreas = new HashSet<EmptyArea>();
        private HashSet<MyObjectSeedParams> m_existingObjectsSeeds = new HashSet<MyObjectSeedParams>();
        private List<MyProceduralCell> m_tempProceduralCellsList = new List<MyProceduralCell>();
        private List<MyObjectSeed> m_tempObjectSeedList = new List<MyObjectSeed>();
        private MyProceduralPlanetCellGenerator m_planetsModule;
        private MyProceduralAsteroidCellGenerator m_asteroidsModule;

        [Event(null, 0x1f0), Reliable, ServerInvoked, Broadcast]
        public static void AddExistingObjectsSeed(MyObjectSeedParams seed)
        {
            Static.m_existingObjectsSeeds.Add(seed);
        }

        public void GetAllExisting(List<MyObjectSeed> list)
        {
            list.Clear();
            this.GetAllExistingCells(this.m_tempProceduralCellsList);
            using (List<MyProceduralCell>.Enumerator enumerator = this.m_tempProceduralCellsList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.GetAll(list, false);
                }
            }
            this.m_tempProceduralCellsList.Clear();
        }

        public void GetAllExistingCells(List<MyProceduralCell> list)
        {
            list.Clear();
            using (List<MyProceduralWorldModule>.Enumerator enumerator = this.m_modules.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.GetAllCells(list);
                }
            }
        }

        public void GetAllInSphere(BoundingSphereD area, List<MyObjectSeed> list)
        {
            foreach (MyProceduralWorldModule local1 in this.m_modules)
            {
                local1.GetObjectSeeds(area, list, false);
                BoundingSphereD? toExclude = null;
                local1.MarkCellsDirty(area, toExclude, false);
            }
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_WorldGenerator objectBuilder = (MyObjectBuilder_WorldGenerator) base.GetObjectBuilder();
            objectBuilder.MarkedAreas = this.m_markedAreas;
            objectBuilder.DeletedAreas = this.m_deletedAreas;
            objectBuilder.ExistingObjectsSeeds = this.m_existingObjectsSeeds;
            return objectBuilder;
        }

        public static Vector3D GetRandomDirection(MyRandom random)
        {
            double d = (random.NextDouble() * 2.0) * 3.1415926535897931;
            double z = (random.NextDouble() * 2.0) - 1.0;
            double num3 = Math.Sqrt(1.0 - (z * z));
            return new Vector3D(num3 * Math.Cos(d), num3 * Math.Sin(d), z);
        }

        public DictionaryReader<MyEntity, MyEntityTracker> GetTrackedEntities() => 
            new DictionaryReader<MyEntity, MyEntityTracker>(this.m_trackedEntities);

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_WorldGenerator generator = (MyObjectBuilder_WorldGenerator) sessionComponent;
            if (!Sync.IsServer)
            {
                this.m_markedAreas = generator.MarkedAreas;
            }
            this.m_deletedAreas = generator.DeletedAreas;
            this.m_existingObjectsSeeds = generator.ExistingObjectsSeeds;
            if (this.m_markedAreas == null)
            {
                this.m_markedAreas = new HashSet<EmptyArea>();
            }
            foreach (EmptyArea area in this.m_markedAreas)
            {
                this.MarkModules(area.Position, area.Radius, true);
            }
            foreach (EmptyArea area2 in this.m_deletedAreas)
            {
                this.MarkModules(area2.Position, area2.Radius, false);
            }
        }

        public override void LoadData()
        {
            Static = this;
            if (MyFakes.ENABLE_ASTEROID_FIELDS)
            {
                MyObjectBuilder_SessionSettings settings = MySession.Static.Settings;
                if (settings.ProceduralDensity == 0f)
                {
                    this.Enabled = false;
                    MySandboxGame.Log.WriteLine("Skip Procedural World Generator");
                }
                else
                {
                    this.m_seed = settings.ProceduralSeed;
                    this.m_objectDensity = MathHelper.Clamp((float) ((settings.ProceduralDensity * 2f) - 1f), (float) -1f, (float) 1f);
                    MySandboxGame.Log.WriteLine($"Loading Procedural World Generator: Seed = '{settings.ProceduralSeed}' = {this.m_seed}, Density = {settings.ProceduralDensity}");
                    using (MyRandom.Instance.PushSeed(this.m_seed))
                    {
                        this.m_asteroidsModule = new MyProceduralAsteroidCellGenerator(this.m_seed, this.m_objectDensity, null);
                        this.m_modules.Add(this.m_asteroidsModule);
                    }
                    this.Enabled = true;
                }
            }
        }

        public void MarkDeletedArea(Vector3D pos, float radius)
        {
            this.MarkModules(pos, radius, false);
            HashSet<EmptyArea> deletedAreas = this.m_deletedAreas;
            lock (deletedAreas)
            {
                EmptyArea item = new EmptyArea {
                    Position = pos,
                    Radius = radius
                };
                this.m_deletedAreas.Add(item);
            }
        }

        public void MarkEmptyArea(Vector3D pos, float radius)
        {
            this.MarkModules(pos, radius, true);
            HashSet<EmptyArea> deletedAreas = this.m_deletedAreas;
            lock (deletedAreas)
            {
                EmptyArea item = new EmptyArea {
                    Position = pos,
                    Radius = radius
                };
                this.m_markedAreas.Add(item);
            }
        }

        private void MarkModules(Vector3D pos, float radius, bool planet)
        {
            MySphereDensityFunction func = !planet ? new MySphereDensityFunction(pos, (double) radius, 0.0) : new MySphereDensityFunction(pos, (radius * 1.1) + 16000.0, 16000.0);
            using (List<MyProceduralWorldModule>.Enumerator enumerator = this.m_modules.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.AddDensityFunctionRemoved(func);
                }
            }
        }

        public void OverlapAllAsteroidSeedsInSphere(BoundingSphereD area, List<MyObjectSeed> list)
        {
            if (this.m_asteroidsModule != null)
            {
                this.m_asteroidsModule.GetObjectSeeds(area, list, false);
                BoundingSphereD? toExclude = null;
                this.m_asteroidsModule.MarkCellsDirty(area, toExclude, false);
            }
        }

        public void OverlapAllPlanetSeedsInSphere(BoundingSphereD area, List<MyObjectSeed> list)
        {
            if (this.m_planetsModule != null)
            {
                this.m_planetsModule.GetObjectSeeds(area, list, false);
                BoundingSphereD? toExclude = null;
                this.m_planetsModule.MarkCellsDirty(area, toExclude, false);
            }
        }

        public void TrackEntity(MyEntity entity)
        {
            if (this.Enabled && (entity is MyCharacter))
            {
                int viewDistance = MySession.Static.Settings.ViewDistance;
                if (MyFakes.USE_GPS_AS_FRIENDLY_SPAWN_LOCATIONS)
                {
                    viewDistance = 0xc350;
                }
                this.TrackEntity(entity, (double) viewDistance);
            }
        }

        private void TrackEntity(MyEntity entity, double range)
        {
            MyEntityTracker tracker;
            if (this.m_trackedEntities.TryGetValue(entity, out tracker) || this.m_toAddTrackedEntities.TryGetValue(entity, out tracker))
            {
                tracker.Radius = range;
            }
            else
            {
                tracker = new MyEntityTracker(entity, range);
                this.m_toAddTrackedEntities.Add(entity, tracker);
                entity.OnClose += delegate (MyEntity e) {
                    this.m_trackedEntities.Remove(e);
                    this.m_toAddTrackedEntities.Remove(e);
                    using (List<MyProceduralWorldModule>.Enumerator enumerator = this.m_modules.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            BoundingSphereD? toExclude = null;
                            enumerator.Current.MarkCellsDirty(tracker.BoundingVolume, toExclude, true);
                        }
                    }
                };
            }
        }

        protected override void UnloadData()
        {
            this.Enabled = false;
            if (MyFakes.ENABLE_ASTEROID_FIELDS)
            {
                MySandboxGame.Log.WriteLine("Unloading Procedural World Generator");
                this.m_modules.Clear();
                this.m_trackedEntities.Clear();
                this.m_tempObjectSeedList.Clear();
                Static = null;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (this.Enabled)
            {
                if (this.m_toAddTrackedEntities.Count != 0)
                {
                    foreach (KeyValuePair<MyEntity, MyEntityTracker> pair in this.m_toAddTrackedEntities)
                    {
                        this.m_trackedEntities.Add(pair.Key, pair.Value);
                    }
                    this.m_toAddTrackedEntities.Clear();
                }
                foreach (MyEntityTracker tracker in this.m_trackedEntities.Values)
                {
                    if (tracker.ShouldGenerate())
                    {
                        BoundingSphereD boundingVolume = tracker.BoundingVolume;
                        tracker.UpdateLastPosition();
                        foreach (MyProceduralWorldModule local1 in this.m_modules)
                        {
                            local1.GetObjectSeeds(tracker.BoundingVolume, this.m_tempObjectSeedList, true);
                            local1.GenerateObjects(this.m_tempObjectSeedList, this.m_existingObjectsSeeds);
                            this.m_tempObjectSeedList.Clear();
                            local1.MarkCellsDirty(boundingVolume, new BoundingSphereD?(tracker.BoundingVolume), true);
                        }
                    }
                }
                using (List<MyProceduralWorldModule>.Enumerator enumerator3 = this.m_modules.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        enumerator3.Current.ProcessDirtyCells(this.m_trackedEntities);
                    }
                }
            }
            if ((!MySandboxGame.AreClipmapsReady && (MySession.Static.VoxelMaps.Instances.Count == 0)) && Sync.IsServer)
            {
                MySandboxGame.AreClipmapsReady = true;
            }
        }

        public override bool UpdatedBeforeInit() => 
            true;

        public bool Enabled { get; private set; }

        public override Type[] Dependencies =>
            new Type[] { typeof(MySector), typeof(MyEncounterGenerator) };
    }
}

