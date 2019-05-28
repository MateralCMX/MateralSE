namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyProceduralEnvironmentProvider : IMyEnvironmentDataProvider
    {
        private readonly FastResourceLock m_sectorsLock = new FastResourceLock();
        private readonly Dictionary<long, MyProceduralLogicalSector> m_sectors = new Dictionary<long, MyProceduralLogicalSector>();
        private readonly Dictionary<long, MyObjectBuilder_ProceduralEnvironmentSector> m_savedSectors = new Dictionary<long, MyObjectBuilder_ProceduralEnvironmentSector>();
        private volatile bool m_sectorsQueued;
        private readonly MyConcurrentQueue<MyProceduralLogicalSector> m_sectorsToRaise = new MyConcurrentQueue<MyProceduralLogicalSector>();
        private readonly MyConcurrentQueue<MyProceduralLogicalSector> m_sectorsToDestroy = new MyConcurrentQueue<MyProceduralLogicalSector>();
        private readonly MyConcurrentHashSet<MyProceduralLogicalSector> m_sectorsForReplication = new MyConcurrentHashSet<MyProceduralLogicalSector>();
        public int LodFactor = 3;
        private Vector3D m_origin;
        private Vector3D m_basisX;
        private Vector3D m_basisY;
        private double m_sectorSize;
        private readonly Action m_raiseCallback;

        public MyProceduralEnvironmentProvider()
        {
            this.m_raiseCallback = new Action(this.RaiseLogicalSectors);
        }

        private void CloseSector(MyProceduralLogicalSector sector)
        {
            if (!sector.ServerOwned)
            {
                sector.OnViewerEmpty -= new Action<MyProceduralLogicalSector>(this.CloseSector);
                this.SaveLogicalSector(sector);
                using (this.m_sectorsLock.AcquireExclusiveUsing())
                {
                    this.m_sectors.Remove(sector.Id);
                }
                if (sector.Replicable)
                {
                    this.UnmarkReplicable(sector);
                }
                this.QueueDestroyLogicalSector(sector);
            }
        }

        public void CloseView(MyProceduralDataView view)
        {
            int lod = view.Lod / this.LodFactor;
            int y = view.Start.Y;
            while (y <= view.End.Y)
            {
                int x = view.Start.X;
                while (true)
                {
                    MyProceduralLogicalSector sector;
                    if (x > view.End.X)
                    {
                        y++;
                        break;
                    }
                    long num4 = MyPlanetSectorId.MakeSectorId(x, y, this.ProviderId, lod);
                    using (this.m_sectorsLock.AcquireSharedUsing())
                    {
                        sector = this.m_sectors[num4];
                    }
                    sector.RemoveView(view);
                    x++;
                }
            }
        }

        public void DebugDraw()
        {
            float num = MyPlanetEnvironmentSessionComponent.DebugDrawDistance * MyPlanetEnvironmentSessionComponent.DebugDrawDistance;
            using (this.m_sectorsLock.AcquireSharedUsing())
            {
                foreach (MyProceduralLogicalSector sector in this.m_sectors.Values)
                {
                    MyRenderProxy.DebugDraw6FaceConvex(sector.Bounds, Color.Violet, 0.5f, true, false, false);
                    Vector3D vectord = (sector.Bounds[4] + sector.Bounds[7]) / 2.0;
                    if (Vector3D.DistanceSquared(vectord, MySector.MainCamera.Position) < num)
                    {
                        MyRenderProxy.DebugDrawText3D(vectord + (-MySector.MainCamera.UpVector * 3f), sector.ToString(), Color.Violet, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                }
            }
        }

        public void GeSectorWorldParameters(int x, int y, int localLod, out Vector3D worldPos, out Vector3 scanBasisA, out Vector3 scanBasisB)
        {
            double num = (1 << (localLod & 0x1f)) * this.m_sectorSize;
            worldPos = (this.m_origin + (this.m_basisX * ((x + 0.5) * num))) + (this.m_basisY * ((y + 0.5) * num));
            scanBasisA = (Vector3) (this.m_basisX * (num * 0.5));
            scanBasisB = (Vector3) (this.m_basisY * (num * 0.5));
        }

        public MyEnvironmentDataView GetItemView(int lod, ref Vector2I start, ref Vector2I end, ref Vector3D localOrigin)
        {
            int localLod = lod / this.LodFactor;
            int logicalLod = lod % this.LodFactor;
            start = start >> (localLod * this.LodFactor);
            end = end >> (localLod * this.LodFactor);
            MyProceduralDataView view = new MyProceduralDataView(this, lod, ref start, ref end);
            int y = start.Y;
            while (y <= end.Y)
            {
                int x = start.X;
                while (true)
                {
                    if (x > end.X)
                    {
                        y++;
                        break;
                    }
                    this.GetLogicalSector(x, y, localLod).AddView(view, localOrigin, logicalLod);
                    x++;
                }
            }
            if (((this.m_sectorsToRaise.Count > 0) || (this.m_sectorsToRaise.Count > 0)) && !this.m_sectorsQueued)
            {
                this.m_sectorsQueued = true;
                MySandboxGame.Static.Invoke(this.m_raiseCallback, "RaiseLogicalSectors");
            }
            return view;
        }

        public MyLogicalEnvironmentSectorBase GetLogicalSector(long sectorId)
        {
            using (this.m_sectorsLock.AcquireSharedUsing())
            {
                MyProceduralLogicalSector sector;
                this.m_sectors.TryGetValue(sectorId, out sector);
                return sector;
            }
        }

        private MyProceduralLogicalSector GetLogicalSector(int x, int y, int localLod)
        {
            MyProceduralLogicalSector sector;
            bool flag;
            long key = MyPlanetSectorId.MakeSectorId(x, y, this.ProviderId, localLod);
            using (this.m_sectorsLock.AcquireSharedUsing())
            {
                flag = this.m_sectors.TryGetValue(key, out sector);
            }
            if (!flag)
            {
                using (this.m_sectorsLock.AcquireExclusiveUsing())
                {
                    if (!this.m_sectors.TryGetValue(key, out sector))
                    {
                        MyObjectBuilder_ProceduralEnvironmentSector sector2;
                        this.m_savedSectors.TryGetValue(key, out sector2);
                        MyProceduralLogicalSector sector1 = new MyProceduralLogicalSector(this, x, y, localLod, sector2);
                        sector1.Id = key;
                        sector = sector1;
                        sector.OnViewerEmpty += new Action<MyProceduralLogicalSector>(this.CloseSector);
                        this.m_sectors[key] = sector;
                    }
                }
            }
            return sector;
        }

        public MyObjectBuilder_EnvironmentDataProvider GetObjectBuilder()
        {
            MyObjectBuilder_ProceduralEnvironmentProvider provider = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ProceduralEnvironmentProvider>();
            using (this.m_sectorsLock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<long, MyProceduralLogicalSector> pair in this.m_sectors)
                {
                    MyObjectBuilder_EnvironmentSector objectBuilder = pair.Value.GetObjectBuilder();
                    if (objectBuilder != null)
                    {
                        provider.Sectors.Add((MyObjectBuilder_ProceduralEnvironmentSector) objectBuilder);
                    }
                }
                foreach (KeyValuePair<long, MyObjectBuilder_ProceduralEnvironmentSector> pair2 in this.m_savedSectors)
                {
                    if (!this.m_sectors.ContainsKey(pair2.Key))
                    {
                        provider.Sectors.Add(pair2.Value);
                    }
                }
            }
            return provider;
        }

        public int GetSeed() => 
            this.Owner.GetSeed();

        public void Init(IMyEnvironmentOwner owner, ref Vector3D origin, ref Vector3D basisA, ref Vector3D basisB, double sectorSize, MyObjectBuilder_Base ob)
        {
            this.Owner = owner;
            this.m_sectorSize = sectorSize;
            this.m_origin = origin;
            this.m_basisX = basisA;
            this.m_basisY = basisB;
            MyObjectBuilder_ProceduralEnvironmentProvider provider = ob as MyObjectBuilder_ProceduralEnvironmentProvider;
            if (provider != null)
            {
                for (int i = 0; i < provider.Sectors.Count; i++)
                {
                    MyObjectBuilder_ProceduralEnvironmentSector sector = provider.Sectors[i];
                    this.m_savedSectors.Add(sector.SectorId, sector);
                }
            }
        }

        internal void MarkReplicable(MyProceduralLogicalSector sector)
        {
            this.m_sectorsForReplication.Add(sector);
            this.QueueRaiseLogicalSector(sector);
            sector.Replicable = true;
            if (!Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyClientState.AddKnownSector), this.Owner.Entity.EntityId, sector.Id, targetEndpoint, position);
            }
        }

        private void QueueDestroyLogicalSector(MyProceduralLogicalSector sector)
        {
            if (!Sync.IsServer || !Sync.MultiplayerActive)
            {
                sector.Close();
            }
            else
            {
                this.m_sectorsToDestroy.Enqueue(sector);
            }
        }

        private void QueueRaiseLogicalSector(MyProceduralLogicalSector sector)
        {
            if (Sync.IsServer && Sync.MultiplayerActive)
            {
                this.m_sectorsToRaise.Enqueue(sector);
            }
        }

        private void RaiseLogicalSectors()
        {
            MyProceduralLogicalSector sector;
            MyMultiplayerServerBase @static = (MyMultiplayerServerBase) MyMultiplayer.Static;
            this.m_sectorsQueued = false;
            while (this.m_sectorsToDestroy.TryDequeue(out sector))
            {
                sector.Close();
            }
            while (this.m_sectorsToRaise.TryDequeue(out sector))
            {
                if (@static == null)
                {
                    continue;
                }
                @static.RaiseReplicableCreated(sector);
            }
        }

        private void SaveLogicalSector(MyProceduralLogicalSector sector)
        {
            MyObjectBuilder_EnvironmentSector objectBuilder = sector.GetObjectBuilder();
            if (objectBuilder == null)
            {
                this.m_savedSectors.Remove(sector.Id);
            }
            else
            {
                this.m_savedSectors[sector.Id] = (MyObjectBuilder_ProceduralEnvironmentSector) objectBuilder;
            }
        }

        public MyProceduralLogicalSector TryGetLogicalSector(int lod, int logicalx, int logicaly)
        {
            using (this.m_sectorsLock.AcquireSharedUsing())
            {
                MyProceduralLogicalSector sector;
                this.m_sectors.TryGetValue(MyPlanetSectorId.MakeSectorId(logicalx, logicaly, this.ProviderId, lod), out sector);
                return sector;
            }
        }

        internal void UnmarkReplicable(MyProceduralLogicalSector sector)
        {
            this.m_sectorsForReplication.Remove(sector);
            sector.Replicable = false;
            if (!Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyClientState.RemoveKnownSector), this.Owner.Entity.EntityId, sector.Id, targetEndpoint, position);
            }
        }

        public IMyEnvironmentOwner Owner { get; private set; }

        public int ProviderId { get; set; }

        internal int SyncLod =>
            this.Owner.EnvironmentDefinition.SyncLod;

        public IEnumerable<MyLogicalEnvironmentSectorBase> LogicalSectors =>
            ((IEnumerable<MyLogicalEnvironmentSectorBase>) this.m_sectorsForReplication);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyProceduralEnvironmentProvider.<>c <>9 = new MyProceduralEnvironmentProvider.<>c();
            public static Func<IMyEventOwner, Action<long, long>> <>9__41_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__42_0;

            internal Action<long, long> <MarkReplicable>b__41_0(IMyEventOwner s) => 
                new Action<long, long>(MyClientState.AddKnownSector);

            internal Action<long, long> <UnmarkReplicable>b__42_0(IMyEventOwner s) => 
                new Action<long, long>(MyClientState.RemoveKnownSector);
        }
    }
}

