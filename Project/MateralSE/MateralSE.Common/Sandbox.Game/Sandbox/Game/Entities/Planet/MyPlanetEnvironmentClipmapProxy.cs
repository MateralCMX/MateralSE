namespace Sandbox.Game.Entities.Planet
{
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyPlanetEnvironmentClipmapProxy : IMy2DClipmapNodeHandler
    {
        public long Id;
        public int Face;
        public int Lod;
        public Vector2I Coords;
        private int m_lodSet = -1;
        public MyEnvironmentSector EnvironmentSector;
        private MyPlanetEnvironmentComponent m_manager;
        private bool m_split;
        private bool m_closed;
        private bool m_stateCommited;
        private MyPlanetEnvironmentClipmapProxy m_parent;
        private readonly MyPlanetEnvironmentClipmapProxy[] m_children = new MyPlanetEnvironmentClipmapProxy[4];
        private readonly HashSet<MyPlanetEnvironmentClipmapProxy> m_dependencies = new HashSet<MyPlanetEnvironmentClipmapProxy>();
        private readonly HashSet<MyPlanetEnvironmentClipmapProxy> m_dependants = new HashSet<MyPlanetEnvironmentClipmapProxy>();

        private void ClearDependencies()
        {
            using (HashSet<MyPlanetEnvironmentClipmapProxy>.Enumerator enumerator = this.m_dependencies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.m_dependants.Remove(this);
                }
            }
            this.m_dependencies.Clear();
        }

        public void Close()
        {
            if (!this.m_closed)
            {
                this.m_closed = true;
                if (this.EnvironmentSector == null)
                {
                    this.m_manager.UnregisterProxy(this);
                }
                else
                {
                    this.m_manager.MarkProxyOutgoingProxy(this);
                    this.NotifyDependants(true);
                    if (!this.m_split)
                    {
                        if (this.m_parent != null)
                        {
                            this.WaitFor(this.m_parent);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            this.WaitFor(this.m_children[i]);
                        }
                    }
                    if (this.m_manager.IsQueued(this) || (this.m_dependencies.Count == 0))
                    {
                        this.EnqueueClose(true);
                    }
                    if (this.m_dependencies.Count == 0)
                    {
                        this.CloseCommit(true);
                    }
                }
            }
        }

        private void CloseCommit(bool clipmapUpdate)
        {
            if (!this.m_split)
            {
                this.m_manager.UnregisterOutgoingProxy(this);
                this.EnvironmentSector.OnLodCommit -= new Action<MyEnvironmentSector, int>(this.sector_OnMyLodCommit);
            }
            this.NotifyDependants(clipmapUpdate);
        }

        internal void DebugDraw(bool outgoing = false)
        {
            if (this.EnvironmentSector != null)
            {
                MyRenderProxy.DebugDrawText3D(((this.EnvironmentSector.Bounds[4] + this.EnvironmentSector.Bounds[7]) / 2.0) + ((MySector.MainCamera.UpVector * 2f) * (1 << (this.Lod & 0x1f))), string.Format("Lod: {4}; Dependants: {0}; Dependencies: {1}\nSplit: {2}; Closed:{3}", new object[] { this.m_dependants.Count, this.m_dependencies.Count, this.m_split, this.m_closed, this.Lod }), outgoing ? Color.Yellow : Color.White, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                this.EnvironmentSector.DebugDraw();
            }
        }

        private void EnqueueClose(bool clipmapUpdate)
        {
            if (!this.EnvironmentSector.IsClosed)
            {
                if (clipmapUpdate)
                {
                    this.m_manager.EnqueueOperation(this, -1, !this.m_split);
                    this.LodSet = -1;
                }
                else
                {
                    this.EnvironmentSector.SetLod(-1);
                    this.LodSet = -1;
                    if (!this.m_split)
                    {
                        this.m_manager.CheckOnGraphicsClose(this.EnvironmentSector);
                    }
                }
            }
        }

        public unsafe void Init(IMy2DClipmapManager parent, int x, int y, int lod, ref BoundingBox2D bounds)
        {
            MyEnvironmentSectorParameters parameters;
            this.m_manager = (MyPlanetEnvironmentComponent) parent;
            BoundingBoxD xd = new BoundingBoxD(new Vector3D(bounds.Min, 0.0), new Vector3D(bounds.Max, 0.0));
            this.Lod = lod;
            this.Face = this.m_manager.ActiveFace;
            MatrixD worldMatrix = this.m_manager.ActiveClipmap.WorldMatrix;
            xd = xd.TransformFast(worldMatrix);
            this.Coords = new Vector2I(x, y);
            this.Id = MyPlanetSectorId.MakeSectorId(x, y, this.m_manager.ActiveFace, lod);
            this.m_manager.RegisterProxy(this);
            worldMatrix.Translation = Vector3D.Zero;
            parameters.SurfaceBasisX = (Vector3) Vector3.Transform(new Vector3(bounds.Width / 2.0, 0.0, 0.0), worldMatrix);
            parameters.SurfaceBasisY = (Vector3) Vector3.Transform(new Vector3(0.0, bounds.Height / 2.0, 0.0), worldMatrix);
            parameters.Center = xd.Center;
            if (lod <= this.m_manager.MaxLod)
            {
                if (!this.m_manager.TryGetSector(this.Id, out this.EnvironmentSector))
                {
                    parameters.SectorId = this.Id;
                    parameters.EntityId = MyPlanetSectorId.MakeSectorId(x, y, this.m_manager.ActiveFace, lod);
                    MyEnvironmentSectorParameters* parametersPtr1 = (MyEnvironmentSectorParameters*) ref parameters;
                    parametersPtr1->Bounds = this.m_manager.GetBoundingShape(ref parameters.Center, ref parameters.SurfaceBasisX, ref parameters.SurfaceBasisY);
                    parameters.Environment = this.m_manager.EnvironmentDefinition;
                    parameters.DataRange = new BoundingBox2I(this.Coords << lod, ((this.Coords + 1) << lod) - 1);
                    parameters.Provider = this.m_manager.Providers[this.m_manager.ActiveFace];
                    this.EnvironmentSector = this.m_manager.EnvironmentDefinition.CreateSector();
                    this.EnvironmentSector.Init(this.m_manager, ref parameters);
                    this.m_manager.Planet.AddChildEntity(this.EnvironmentSector);
                }
                this.m_manager.EnqueueOperation(this, lod, false);
                this.LodSet = lod;
                this.EnvironmentSector.OnLodCommit += new Action<MyEnvironmentSector, int>(this.sector_OnMyLodCommit);
            }
        }

        public void InitJoin(IMy2DClipmapNodeHandler[] children)
        {
            this.m_split = false;
            this.m_closed = false;
            if (this.EnvironmentSector == null)
            {
                this.m_manager.RegisterProxy(this);
            }
            else
            {
                this.m_manager.UnmarkProxyOutgoingProxy(this);
                this.m_manager.EnqueueOperation(this, this.Lod, false);
                this.LodSet = this.Lod;
                for (int i = 0; i < 4; i++)
                {
                    this.m_children[i] = null;
                }
            }
        }

        private void Notify(MyPlanetEnvironmentClipmapProxy proxy, bool clipmapUpdate)
        {
            if (this.m_dependencies.Count != 0)
            {
                this.m_dependencies.Remove(proxy);
                if ((this.m_dependencies.Count == 0) && this.m_closed)
                {
                    this.EnqueueClose(clipmapUpdate);
                    if (this.EnvironmentSector.IsClosed || (this.EnvironmentSector.LodLevel == -1))
                    {
                        this.CloseCommit(clipmapUpdate);
                    }
                }
            }
        }

        private void NotifyDependants(bool clipmapUpdate)
        {
            using (HashSet<MyPlanetEnvironmentClipmapProxy>.Enumerator enumerator = this.m_dependants.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Notify(this, clipmapUpdate);
                }
            }
            this.m_dependants.Clear();
        }

        private void sector_OnMyLodCommit(MyEnvironmentSector sector, int lod)
        {
            if (lod == this.LodSet)
            {
                this.m_stateCommited = true;
                if (this.m_dependencies.Count == 0)
                {
                    if ((lod == -1) && this.m_closed)
                    {
                        this.CloseCommit(false);
                    }
                    else
                    {
                        this.NotifyDependants(false);
                    }
                }
            }
        }

        public unsafe void Split(BoundingBox2D* childBoxes, ref IMy2DClipmapNodeHandler[] children)
        {
            this.m_split = true;
            for (int i = 0; i < 4; i++)
            {
                children[i].Init(this.m_manager, (this.Coords.X << 1) + (i & 1), (this.Coords.Y << 1) + ((i >> 1) & 1), this.Lod - 1, ref (BoundingBox2D) ref (childBoxes + i));
            }
            if (this.EnvironmentSector != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    this.m_children[j] = (MyPlanetEnvironmentClipmapProxy) children[j];
                    this.m_children[j].m_parent = this;
                }
            }
        }

        private void WaitFor(MyPlanetEnvironmentClipmapProxy proxy)
        {
            if (proxy.LodSet != -1)
            {
                this.m_dependencies.Add(proxy);
                proxy.m_dependants.Add(this);
            }
        }

        public int LodSet
        {
            get => 
                this.m_lodSet;
            protected set
            {
                this.m_lodSet = value;
                this.m_stateCommited = false;
            }
        }
    }
}

