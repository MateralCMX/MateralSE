namespace Sandbox.Game.Entities.Planet
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500)]
    public class MyPlanetEnvironmentSessionComponent : MySessionComponentBase
    {
        private const int TIME_TO_UPDATE = 10;
        private const int UPDATES_TO_LAZY_UPDATE = 10;
        private int m_updateInterval;
        private int m_lazyUpdateInterval;
        public static bool EnableUpdate = true;
        public static bool DebugDrawSectors = false;
        public static bool DebugDrawDynamicObjectClusters = false;
        public static bool DebugDrawEnvironmentProviders = false;
        public static bool DebugDrawActiveSectorItems = false;
        public static bool DebugDrawActiveSectorProvider = false;
        public static bool DebugDrawProxies = false;
        public static bool DebugDrawCollisionCheckers = false;
        public static float DebugDrawDistance = 150f;
        private readonly HashSet<IMyEnvironmentDataProvider> m_environmentProviders = new HashSet<IMyEnvironmentDataProvider>();
        private readonly HashSet<MyPlanetEnvironmentComponent> m_planetEnvironments = new HashSet<MyPlanetEnvironmentComponent>();
        public static MyEnvironmentSector ActiveSector;
        private const int NewEnvReleaseVersion = 0x1149ca;
        private MyListDictionary<MyCubeGrid, BoundingBoxD> m_cubeBlocksToWork = new MyListDictionary<MyCubeGrid, BoundingBoxD>();
        private volatile MyListDictionary<MyCubeGrid, BoundingBoxD> m_cubeBlocksPending = new MyListDictionary<MyCubeGrid, BoundingBoxD>();
        private volatile bool m_itemDisableJobRunning;
        private List<MyVoxelBase> m_tmpVoxelList = new List<MyVoxelBase>();
        private List<VRage.Game.Entity.MyEntity> m_tmpEntityList = new List<VRage.Game.Entity.MyEntity>();
        private MyListDictionary<MyEnvironmentSector, int> m_itemsToDisable = new MyListDictionary<MyEnvironmentSector, int>();

        public override void BeforeStart()
        {
            if (MySession.Static.AppVersionFromSave < 0x1149ca)
            {
                using (HashSet<MyPlanetEnvironmentComponent>.Enumerator enumerator = this.m_planetEnvironments.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InitClearAreasManagement();
                    }
                }
            }
        }

        private void CheckCubeGridCreated(VRage.Game.Entity.MyEntity myEntity)
        {
            bool ready = MySession.Static.Ready;
        }

        public void DisableGatheredItems()
        {
            foreach (KeyValuePair<MyEnvironmentSector, List<int>> pair in this.m_itemsToDisable)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    pair.Key.EnableItem(pair.Value[i], false);
                }
            }
            this.m_itemsToDisable.Clear();
            this.m_itemDisableJobRunning = false;
        }

        public override void Draw()
        {
            if (DebugDrawEnvironmentProviders)
            {
                using (HashSet<IMyEnvironmentDataProvider>.Enumerator enumerator = this.m_environmentProviders.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.DebugDraw();
                    }
                }
            }
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(MySector.MainCamera.Position);
            if (DebugDrawSectors && (closestPlanet != null))
            {
                ActiveSector = closestPlanet.Components.Get<MyPlanetEnvironmentComponent>().GetSectorForPosition(MySector.MainCamera.Position);
            }
        }

        private void GatherEnvItemsInBoxes()
        {
            MyListDictionary<MyCubeGrid, BoundingBoxD> dictionary = Interlocked.Exchange<MyListDictionary<MyCubeGrid, BoundingBoxD>>(ref this.m_cubeBlocksPending, this.m_cubeBlocksToWork);
            this.m_cubeBlocksToWork = dictionary;
            int num = 0;
            int num2 = 0;
            using (Dictionary<MyCubeGrid, List<BoundingBoxD>>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    List<BoundingBoxD> current = enumerator.Current;
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 >= current.Count)
                        {
                            break;
                        }
                        BoundingBoxD box = current[num3];
                        MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, this.m_tmpVoxelList);
                        num2++;
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 < this.m_tmpVoxelList.Count)
                            {
                                MyPlanet planet = this.m_tmpVoxelList[num4] as MyPlanet;
                                if (planet != null)
                                {
                                    planet.Hierarchy.QueryAABB(ref box, this.m_tmpEntityList);
                                    int num5 = 0;
                                    while (true)
                                    {
                                        if (num5 < this.m_tmpEntityList.Count)
                                        {
                                            MyEnvironmentSector sector = this.m_tmpEntityList[num5] as MyEnvironmentSector;
                                            if (sector != null)
                                            {
                                                BoundingBoxD aabb = box;
                                                sector.GetItemsInAabb(ref aabb, this.m_itemsToDisable.GetOrAdd(sector));
                                                if ((sector.DataView != null) && (sector.DataView.Items != null))
                                                {
                                                    num += sector.DataView.Items.Count;
                                                }
                                                num5++;
                                                continue;
                                            }
                                            return;
                                        }
                                        else
                                        {
                                            this.m_tmpEntityList.Clear();
                                        }
                                        break;
                                    }
                                }
                                num4++;
                                continue;
                            }
                            else
                            {
                                this.m_tmpVoxelList.Clear();
                                num3++;
                            }
                            break;
                        }
                    }
                }
            }
            dictionary.Clear();
        }

        public override void LoadData()
        {
            base.LoadData();
            MyCubeGrids.BlockBuilt += new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockBuilt);
            Sandbox.Game.Entities.MyEntities.OnEntityAdd += new Action<VRage.Game.Entity.MyEntity>(this.CheckCubeGridCreated);
        }

        private void MyCubeGridsOnBlockBuilt(MyCubeGrid myCubeGrid, MySlimBlock mySlimBlock)
        {
            if ((mySlimBlock != null) && myCubeGrid.IsStatic)
            {
                BoundingBoxD xd;
                MySlimBlock cubeBlock = myCubeGrid.GetCubeBlock(mySlimBlock.Min);
                if (cubeBlock != null)
                {
                    MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                    if ((fatBlock != null) && !ReferenceEquals(mySlimBlock.FatBlock, fatBlock))
                    {
                        return;
                    }
                }
                mySlimBlock.GetWorldBoundingBox(out xd, true);
            }
        }

        public void RegisterPlanetEnvironment(MyPlanetEnvironmentComponent env)
        {
            this.m_planetEnvironments.Add(env);
            foreach (IMyEnvironmentDataProvider provider in env.Providers)
            {
                this.m_environmentProviders.Add(provider);
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyCubeGrids.BlockBuilt -= new Action<MyCubeGrid, MySlimBlock>(this.MyCubeGridsOnBlockBuilt);
            Sandbox.Game.Entities.MyEntities.OnEntityAdd -= new Action<VRage.Game.Entity.MyEntity>(this.CheckCubeGridCreated);
        }

        public void UnregisterPlanetEnvironment(MyPlanetEnvironmentComponent env)
        {
            this.m_planetEnvironments.Remove(env);
            foreach (IMyEnvironmentDataProvider provider in env.Providers)
            {
                this.m_environmentProviders.Remove(provider);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (EnableUpdate)
            {
                this.m_updateInterval++;
                if (this.m_updateInterval > 10)
                {
                    this.m_updateInterval = 0;
                    this.m_lazyUpdateInterval++;
                    bool doLazy = false;
                    if (this.m_lazyUpdateInterval > 10)
                    {
                        doLazy = true;
                        this.m_lazyUpdateInterval = 0;
                    }
                    this.UpdatePlanetEnvironments(doLazy);
                }
            }
        }

        public override bool UpdatedBeforeInit() => 
            true;

        private void UpdatePlanetEnvironments(bool doLazy)
        {
            using (HashSet<MyPlanetEnvironmentComponent>.Enumerator enumerator = this.m_planetEnvironments.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update(doLazy, false);
                }
            }
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyCubeGrids) };
    }
}

