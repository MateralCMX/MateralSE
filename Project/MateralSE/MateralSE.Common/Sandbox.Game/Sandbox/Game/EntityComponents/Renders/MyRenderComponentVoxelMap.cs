namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Entities.Components;
    using VRage.Factory;
    using VRage.Game.Entity;
    using VRage.Voxels.Clipmap;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    [MyDependency(typeof(MyVoxelMesherComponent), Critical=true)]
    public class MyRenderComponentVoxelMap : MyRenderComponent
    {
        public const string DefaultSettingsGroup = "Default";
        public const string PlanetSettingsGroup = "Planet";
        [CompilerGenerated]
        private static Action TerrainQualityChange;
        protected MyVoxelBase m_voxelMap;
        protected MyVoxelMesherComponent Mesher;

        public static  event Action TerrainQualityChange
        {
            [CompilerGenerated] add
            {
                Action terrainQualityChange = TerrainQualityChange;
                while (true)
                {
                    Action a = terrainQualityChange;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    terrainQualityChange = Interlocked.CompareExchange<Action>(ref TerrainQualityChange, action3, a);
                    if (ReferenceEquals(terrainQualityChange, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action terrainQualityChange = TerrainQualityChange;
                while (true)
                {
                    Action source = terrainQualityChange;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    terrainQualityChange = Interlocked.CompareExchange<Action>(ref TerrainQualityChange, action3, source);
                    if (ReferenceEquals(terrainQualityChange, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyRenderComponentVoxelMap()
        {
            MyRenderQualityEnum nORMAL;
            if (MySandboxGame.Config.VoxelQuality == null)
            {
                nORMAL = MyRenderQualityEnum.NORMAL;
            }
            else
            {
                nORMAL = MySandboxGame.Config.VoxelQuality.Value;
            }
            SetLodQuality(nORMAL);
        }

        public override void AddRenderObjects()
        {
            if (this.Mesher != null)
            {
                MatrixD worldMatrix = MatrixD.CreateWorld(this.m_voxelMap.PositionLeftBottomCorner, this.m_voxelMap.Orientation.Forward, this.m_voxelMap.Orientation.Up);
                this.Clipmap = this.CreateLodController();
                VoxelLoadingWaitStep.AddClipmap(this.Clipmap);
                this.SetRenderObjectID(0, MyRenderProxy.RenderVoxelCreate(this.m_voxelMap.StorageName, worldMatrix, this.Clipmap, this.GetRenderFlags(), base.Transparency));
            }
        }

        protected virtual IMyLodController CreateLodController()
        {
            MatrixD worldMatrix = MatrixD.CreateWorld(this.m_voxelMap.PositionLeftBottomCorner, this.m_voxelMap.Orientation.Forward, this.m_voxelMap.Orientation.Up);
            return new MyVoxelClipmap(this.m_voxelMap.Size, worldMatrix, this.Mesher, null, Vector3D.Zero, "Default");
        }

        public void InvalidateRange(Vector3I minVoxelChanged, Vector3I maxVoxelChanged)
        {
            minVoxelChanged -= 1;
            maxVoxelChanged = (Vector3I) (maxVoxelChanged + 1);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref minVoxelChanged, 1);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref maxVoxelChanged, 1);
            minVoxelChanged -= this.m_voxelMap.StorageMin;
            maxVoxelChanged -= this.m_voxelMap.StorageMin;
            if (this.Clipmap != null)
            {
                this.Clipmap.InvalidateRange(minVoxelChanged, maxVoxelChanged);
            }
        }

        public override void InvalidateRenderObjects()
        {
            if (base.Visible && (base.m_renderObjectIDs[0] != uint.MaxValue))
            {
                MatrixD xd = MatrixD.CreateWorld(this.m_voxelMap.PositionLeftBottomCorner, this.m_voxelMap.Orientation.Forward, this.m_voxelMap.Orientation.Up);
                BoundingBox? aabb = null;
                Matrix? localMatrix = null;
                MyRenderProxy.UpdateRenderObject(base.m_renderObjectIDs[0], new MatrixD?(xd), aabb, -1, localMatrix);
            }
        }

        public override void OnAddedToScene()
        {
            this.m_voxelMap = base.Container.Entity as MyVoxelBase;
            this.Mesher = new MyVoxelMesherComponent();
            this.Mesher.SetContainer(base.Entity.Components);
            this.Mesher.OnAddedToScene();
            base.OnAddedToScene();
        }

        public static void RefreshClipmapSettings()
        {
            if (MyEntities.IsLoaded)
            {
                foreach (MyEntity entity in MyEntities.GetEntities())
                {
                    if (entity.MarkedForClose)
                    {
                        continue;
                    }
                    MyRenderComponentVoxelMap render = entity.Render as MyRenderComponentVoxelMap;
                    if (render != null)
                    {
                        MyVoxelClipmap clipmap = render.Clipmap as MyVoxelClipmap;
                        if (clipmap != null)
                        {
                            clipmap.UpdateSettings(MyVoxelClipmapSettings.GetSettings(clipmap.SettingsGroup));
                        }
                    }
                }
            }
        }

        public void ResetLoading()
        {
            VoxelLoadingWaitStep.AddClipmap(this.Clipmap);
        }

        public static void SetLodQuality(MyRenderQualityEnum quality)
        {
            MyVoxelClipmapSettings.SetSettingsForGroup("Default", MyVoxelClipmapSettingsPresets.NormalSettings[(int) quality]);
            MyVoxelClipmapSettings.SetSettingsForGroup("Planet", MyVoxelClipmapSettingsPresets.PlanetSettings[(int) quality]);
            RefreshClipmapSettings();
            if (TerrainQualityChange != null)
            {
                TerrainQualityChange();
            }
        }

        public void UpdateCells()
        {
            if (base.m_renderObjectIDs[0] != uint.MaxValue)
            {
                MatrixD xd = MatrixD.CreateWorld(this.m_voxelMap.PositionLeftBottomCorner, this.m_voxelMap.Orientation.Forward, this.m_voxelMap.Orientation.Up);
                BoundingBox? aabb = null;
                Matrix? localMatrix = null;
                MyRenderProxy.UpdateRenderObject(base.m_renderObjectIDs[0], new MatrixD?(xd), aabb, -1, localMatrix);
            }
        }

        public IMyLodController Clipmap { get; protected set; }

        public uint ClipmapId =>
            base.m_renderObjectIDs[0];

        public static class VoxelLoadingWaitStep
        {
            public static readonly HashSet<IMyLodController> Clipmaps = new HashSet<IMyLodController>();
            public static int Total;

            public static void AddClipmap(IMyLodController controller)
            {
                HashSet<IMyLodController> clipmaps = Clipmaps;
                lock (clipmaps)
                {
                    if (Clipmaps.Add(controller))
                    {
                        Total++;
                        controller.Loaded += new Action<IMyLodController>(MyRenderComponentVoxelMap.VoxelLoadingWaitStep.RemoveClipmap);
                    }
                }
            }

            public static void RemoveClipmap(IMyLodController clipmap)
            {
                HashSet<IMyLodController> clipmaps = Clipmaps;
                lock (clipmaps)
                {
                    if (Clipmaps.Remove(clipmap))
                    {
                        clipmap.Loaded -= new Action<IMyLodController>(MyRenderComponentVoxelMap.VoxelLoadingWaitStep.RemoveClipmap);
                    }
                }
                if (Complete)
                {
                    MyRenderProxy.SendClipmapsReady();
                }
            }

            public static bool Complete =>
                (Clipmaps.Count == 0);

            public static float Progress =>
                (((float) Clipmaps.Count) / ((float) Total));
        }
    }
}

