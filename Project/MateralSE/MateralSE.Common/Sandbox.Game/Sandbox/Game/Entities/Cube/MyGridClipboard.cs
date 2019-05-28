namespace Sandbox.Game.Entities.Cube
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.CoordinateSystem;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridClipboard
    {
        protected static readonly MyStringId ID_GIZMO_DRAW_LINE = MyStringId.GetOrCompute("GizmoDrawLine");
        protected static readonly MyStringId ID_GIZMO_DRAW_LINE_RED = MyStringId.GetOrCompute("GizmoDrawLineRed");
        [ThreadStatic]
        private static HashSet<IMyEntity> m_cacheEntitySet = new HashSet<IMyEntity>();
        protected readonly List<MyObjectBuilder_CubeGrid> m_copiedGrids = new List<MyObjectBuilder_CubeGrid>();
        protected readonly List<Vector3> m_copiedGridOffsets = new List<Vector3>();
        protected List<MyCubeGrid> m_previewGrids = new List<MyCubeGrid>();
        private List<MyCubeGrid> m_previewGridsParallel = new List<MyCubeGrid>();
        private List<MyObjectBuilder_CubeGrid> m_copiedGridsParallel = new List<MyObjectBuilder_CubeGrid>();
        private readonly MyComponentList m_buildComponents = new MyComponentList();
        protected Vector3D m_pastePosition;
        protected Vector3D m_pastePositionPrevious;
        protected bool m_calculateVelocity = true;
        protected Vector3 m_objectVelocity = Vector3.Zero;
        protected float m_pasteOrientationAngle;
        protected Vector3 m_pasteDirUp = new Vector3(1f, 0f, 0f);
        protected Vector3 m_pasteDirForward = new Vector3(0f, 1f, 0f);
        protected float m_dragDistance;
        protected const float m_maxDragDistance = 20000f;
        protected Vector3 m_dragPointToPositionLocal;
        protected bool m_canBePlaced;
        private bool m_canBePlacedNeedsRefresh = true;
        protected bool m_characterHasEnoughMaterials;
        protected MyPlacementSettings m_settings;
        private long? m_spawnerId;
        private Vector3D m_originalSpawnerPosition = Vector3D.Zero;
        private readonly List<MyPhysics.HitInfo> m_raycastCollisionResults = new List<MyPhysics.HitInfo>();
        protected float m_closestHitDistSq = float.MaxValue;
        protected Vector3D m_hitPos = new Vector3(0f, 0f, 0f);
        protected Vector3 m_hitNormal = new Vector3(1f, 0f, 0f);
        protected IMyEntity m_hitEntity;
        protected bool m_visible = true;
        private bool m_allowSwitchCameraMode = true;
        protected bool m_useDynamicPreviews;
        protected Dictionary<string, int> m_blocksPerType = new Dictionary<string, int>();
        protected List<MyCubeGrid> m_touchingGrids = new List<MyCubeGrid>();
        private Task ActivationTask;
        private readonly List<IMyEntity> m_resultIDs = new List<IMyEntity>();
        private bool m_isBeingAdded;
        [CompilerGenerated]
        private Action<MyGridClipboard, bool> Deactivated;
        private bool m_enableStationRotation;
        public bool ShowModdedBlocksWarning = true;
        private bool m_isAligning;
        private int m_lastFrameAligned;

        public event Action<MyGridClipboard, bool> Deactivated
        {
            [CompilerGenerated] add
            {
                Action<MyGridClipboard, bool> deactivated = this.Deactivated;
                while (true)
                {
                    Action<MyGridClipboard, bool> a = deactivated;
                    Action<MyGridClipboard, bool> action3 = (Action<MyGridClipboard, bool>) Delegate.Combine(a, value);
                    deactivated = Interlocked.CompareExchange<Action<MyGridClipboard, bool>>(ref this.Deactivated, action3, a);
                    if (ReferenceEquals(deactivated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGridClipboard, bool> deactivated = this.Deactivated;
                while (true)
                {
                    Action<MyGridClipboard, bool> source = deactivated;
                    Action<MyGridClipboard, bool> action3 = (Action<MyGridClipboard, bool>) Delegate.Remove(source, value);
                    deactivated = Interlocked.CompareExchange<Action<MyGridClipboard, bool>>(ref this.Deactivated, action3, source);
                    if (ReferenceEquals(deactivated, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGridClipboard(MyPlacementSettings settings, bool calculateVelocity = true)
        {
            this.m_calculateVelocity = calculateVelocity;
            this.m_settings = settings;
        }

        public virtual void Activate(Action callback = null)
        {
            if (this.ActivationTask.IsComplete && !this.m_isBeingAdded)
            {
                MyHud.PushRotatingWheelVisible();
                this.m_isBeingAdded = true;
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.PrepareSwapData();
                    MyEntityIdentifier.SwapPerThreadData();
                }
                this.m_copiedGridsParallel.Clear();
                this.m_copiedGridsParallel.AddRange(this.m_copiedGrids);
                this.ActivationTask = Parallel.Start(new Action(this.ActivateInternal), delegate {
                    if (this.m_visible)
                    {
                        foreach (IMyEntity entity in this.m_resultIDs)
                        {
                            IMyEntity entity2;
                            MyEntityIdentifier.TryGetEntity(entity.EntityId, out entity2, false);
                            if (entity2 == null)
                            {
                                MyEntityIdentifier.AddEntityWithId(entity);
                            }
                        }
                        this.m_resultIDs.Clear();
                        foreach (MyCubeGrid grid in this.m_previewGridsParallel)
                        {
                            Sandbox.Game.Entities.MyEntities.Add(grid, true);
                            this.DisablePhysicsRecursively(grid);
                        }
                        if (callback != null)
                        {
                            callback();
                        }
                        this.IsActive = true;
                    }
                    List<MyCubeGrid> previewGridsParallel = this.m_previewGridsParallel;
                    this.m_previewGridsParallel = this.m_previewGrids;
                    this.m_previewGrids = previewGridsParallel;
                    this.m_isBeingAdded = false;
                    MyHud.PopRotatingWheelVisible();
                });
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.ClearSwapDataAndRestore();
                }
            }
        }

        private void ActivateInternal()
        {
            this.ChangeClipboardPreview(true, this.m_previewGridsParallel, this.m_copiedGridsParallel);
            if (this.EnableStationRotation)
            {
                this.AlignClipboardToGravity();
            }
            if (!this.EnableStationRotation)
            {
                MyCoordinateSystem.Static.Visible = true;
                this.AlignRotationToCoordSys();
            }
            MyCoordinateSystem.OnCoordinateChange += new Action(this.OnCoordinateChange);
        }

        public virtual void ActivateNoAlign(Action callback = null)
        {
            if (this.ActivationTask.IsComplete && !this.m_isBeingAdded)
            {
                this.m_isBeingAdded = true;
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.PrepareSwapData();
                    MyEntityIdentifier.SwapPerThreadData();
                }
                this.m_copiedGridsParallel.Clear();
                this.m_copiedGridsParallel.AddRange(this.m_copiedGrids);
                this.ActivationTask = Parallel.Start(() => this.ChangeClipboardPreview(true, this.m_previewGridsParallel, this.m_copiedGridsParallel), delegate {
                    if (this.m_visible)
                    {
                        foreach (MyCubeGrid grid in this.m_previewGridsParallel)
                        {
                            Sandbox.Game.Entities.MyEntities.Add(grid, true);
                            this.DisablePhysicsRecursively(grid);
                        }
                    }
                    List<MyCubeGrid> previewGridsParallel = this.m_previewGridsParallel;
                    this.m_previewGridsParallel = this.m_previewGrids;
                    this.m_previewGrids = previewGridsParallel;
                    if (this.m_visible)
                    {
                        if (callback != null)
                        {
                            callback();
                        }
                        this.IsActive = true;
                    }
                    this.m_isBeingAdded = false;
                });
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.ClearSwapDataAndRestore();
                }
            }
        }

        private static void AddSingleBlockRequirements(MyObjectBuilder_CubeBlock block, MyComponentList buildComponents)
        {
            MyComponentStack.GetMountedComponents(buildComponents, block);
            if (block.ConstructionStockpile != null)
            {
                foreach (MyObjectBuilder_StockpileItem item in block.ConstructionStockpile.Items)
                {
                    if (item.PhysicalContent != null)
                    {
                        buildComponents.AddMaterial(item.PhysicalContent.GetId(), item.Amount, 0, false);
                    }
                }
            }
        }

        public void AlignClipboardToGravity()
        {
            if (this.PreviewGrids.Count > 0)
            {
                Vector3 gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.PreviewGrids[0].WorldMatrix.Translation);
                if ((gravity.LengthSquared() < double.Epsilon) && (MyPerGameSettings.Game == GameEnum.ME_GAME))
                {
                    gravity = Vector3.Down;
                }
                this.AlignClipboardToGravity(gravity);
            }
        }

        public void AlignClipboardToGravity(Vector3 gravity)
        {
            if ((this.PreviewGrids.Count > 0) && (gravity.LengthSquared() > 0.0001f))
            {
                gravity.Normalize();
                Vector3 vector = (Vector3) Vector3D.Reject(this.m_pasteDirForward, gravity);
                this.m_pasteDirForward = vector;
                this.m_pasteDirUp = -gravity;
            }
        }

        protected void AlignRotationToCoordSys()
        {
            if (this.m_previewGrids.Count > 0)
            {
                double gridSize = this.m_previewGrids[0].GridSize;
                long? id = null;
                MyCoordinateSystem.CoordSystemData data = MyCoordinateSystem.Static.SnapWorldPosToClosestGrid(ref this.m_pastePosition, gridSize, this.m_settings.StaticGridAlignToCenter, id);
                this.m_pastePosition = data.SnappedTransform.Position;
                this.m_pasteDirForward = data.SnappedTransform.Rotation.Forward;
                this.m_pasteDirUp = data.SnappedTransform.Rotation.Up;
                this.m_pasteOrientationAngle = 0f;
            }
        }

        private void AngleMinus(float angle)
        {
            this.m_pasteOrientationAngle -= angle;
            if (this.m_pasteOrientationAngle < 0f)
            {
                this.m_pasteOrientationAngle += 6.283185f;
            }
        }

        private void AnglePlus(float angle)
        {
            this.m_pasteOrientationAngle += angle;
            if (this.m_pasteOrientationAngle >= 6.283185f)
            {
                this.m_pasteOrientationAngle -= 6.283185f;
            }
        }

        private void ApplyOrientationAngle()
        {
            this.m_pasteDirForward = Vector3.Normalize(this.m_pasteDirForward);
            this.m_pasteDirUp = Vector3.Normalize(this.m_pasteDirUp);
            Vector3 vector = Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
            float num = (float) Math.Cos((double) this.m_pasteOrientationAngle);
            this.m_pasteDirForward = (this.m_pasteDirForward * num) - (vector * ((float) Math.Sin((double) this.m_pasteOrientationAngle)));
            this.m_pasteOrientationAngle = 0f;
        }

        protected void BeforeCreateGrid(MyObjectBuilder_CubeGrid grid)
        {
            foreach (MyObjectBuilder_CubeBlock block in grid.CubeBlocks)
            {
                MyDefinitionId defId = block.GetId();
                MyCubeBlockDefinition blockDefinition = null;
                MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out blockDefinition);
                if (blockDefinition != null)
                {
                    MyCubeBuilder.BuildComponent.BeforeCreateBlock(blockDefinition, this.GetClipboardBuilder(), block, MySession.Static.CreativeToolsEnabled(Sync.MyId));
                }
            }
        }

        public static void CalculateItemRequirements(List<MyObjectBuilder_CubeGrid> blocksToBuild, MyComponentList buildComponents)
        {
            buildComponents.Clear();
            using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator = blocksToBuild.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (MyObjectBuilder_CubeBlock block in enumerator.Current.CubeBlocks)
                    {
                        MyObjectBuilder_CompoundCubeBlock block2 = block as MyObjectBuilder_CompoundCubeBlock;
                        if (block2 == null)
                        {
                            AddSingleBlockRequirements(block, buildComponents);
                            continue;
                        }
                        MyObjectBuilder_CubeBlock[] blocks = block2.Blocks;
                        for (int i = 0; i < blocks.Length; i++)
                        {
                            AddSingleBlockRequirements(blocks[i], buildComponents);
                        }
                    }
                }
            }
        }

        public static void CalculateItemRequirements(MyObjectBuilder_CubeGrid[] blocksToBuild, MyComponentList buildComponents)
        {
            buildComponents.Clear();
            MyObjectBuilder_CubeGrid[] gridArray = blocksToBuild;
            for (int i = 0; i < gridArray.Length; i++)
            {
                foreach (MyObjectBuilder_CubeBlock block in gridArray[i].CubeBlocks)
                {
                    MyObjectBuilder_CompoundCubeBlock block2 = block as MyObjectBuilder_CompoundCubeBlock;
                    if (block2 == null)
                    {
                        AddSingleBlockRequirements(block, buildComponents);
                        continue;
                    }
                    MyObjectBuilder_CubeBlock[] blocks = block2.Blocks;
                    for (int j = 0; j < blocks.Length; j++)
                    {
                        AddSingleBlockRequirements(blocks[j], buildComponents);
                    }
                }
            }
        }

        public unsafe void CalculateRotationHints(MyBlockBuilderRotationHints hints, bool isRotating)
        {
            MyCubeGrid grid = (this.PreviewGrids.Count > 0) ? this.PreviewGrids[0] : null;
            if (grid != null)
            {
                int rotationHints;
                MatrixD worldMatrix = grid.WorldMatrix;
                Vector3D vectord = Vector3D.TransformNormal(-this.m_dragPointToPositionLocal, worldMatrix);
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation = worldMatrix.Translation + vectord;
                if (MyHud.MinimalHud || MyHud.CutsceneHud)
                {
                    rotationHints = 0;
                }
                else
                {
                    rotationHints = (int) MySandboxGame.Config.RotationHints;
                }
                hints.CalculateRotationHints(worldMatrix, (bool) rotationHints, isRotating, this.OneAxisRotationMode);
            }
        }

        protected virtual unsafe void ChangeClipboardPreview(bool visible, List<MyCubeGrid> previewGrids, List<MyObjectBuilder_CubeGrid> copiedGrids)
        {
            foreach (MyCubeGrid local1 in previewGrids)
            {
                VRageMath.Vector4? color = null;
                Vector3? inflateAmount = null;
                MyStringId? lineMaterial = null;
                Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(local1, false, color, 0.01f, inflateAmount, lineMaterial);
                local1.SetFadeOut(false);
                local1.Close();
            }
            this.m_visible = false;
            previewGrids.Clear();
            this.m_buildComponents.Clear();
            if ((copiedGrids.Count != 0) && visible)
            {
                CalculateItemRequirements(copiedGrids, this.m_buildComponents);
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection((IEnumerable<MyObjectBuilder_EntityBase>) copiedGrids);
                Vector3D zero = Vector3D.Zero;
                bool flag = true;
                this.m_blocksPerType.Clear();
                MyEntityIdentifier.InEntityCreationBlock = true;
                MyEntityIdentifier.LazyInitPerThreadStorage(0x800);
                using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator2 = copiedGrids.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator2.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_CubeGrid current = enumerator2.Current;
                        bool isStatic = current.IsStatic;
                        if (this.m_useDynamicPreviews)
                        {
                            current.IsStatic = false;
                        }
                        current.CreatePhysics = false;
                        current.EnableSmallToLargeConnections = false;
                        foreach (MyObjectBuilder_CubeBlock block in current.CubeBlocks)
                        {
                            MyCubeBlockDefinition definition;
                            block.BuiltBy = 0L;
                            MyDefinitionId defId = new MyDefinitionId(block.TypeId, block.SubtypeId);
                            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition))
                            {
                                string blockPairName = definition.BlockPairName;
                                if (!this.m_blocksPerType.ContainsKey(blockPairName))
                                {
                                    this.m_blocksPerType.Add(blockPairName, 1);
                                    continue;
                                }
                                string str2 = blockPairName;
                                this.m_blocksPerType[str2] += 1;
                            }
                        }
                        if (current.PositionAndOrientation != null)
                        {
                            MyPositionAndOrientation orientation = current.PositionAndOrientation.Value;
                            if (flag)
                            {
                                flag = false;
                                zero = (Vector3D) orientation.Position;
                            }
                            SerializableVector3D* vectordPtr1 = (SerializableVector3D*) ref orientation.Position;
                            vectordPtr1[0] -= zero;
                            current.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                        }
                        MyCubeGrid grid = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(current, false) as MyCubeGrid;
                        current.IsStatic = isStatic;
                        if (grid == null)
                        {
                            this.ChangeClipboardPreview(false, previewGrids, copiedGrids);
                        }
                        else
                        {
                            this.MakeTransparent(grid);
                            if (grid.CubeBlocks.Count != 0)
                            {
                                grid.IsPreview = true;
                                grid.Save = false;
                                previewGrids.Add(grid);
                                grid.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.previewGrid_OnClose);
                                continue;
                            }
                            copiedGrids.Remove(current);
                            this.ChangeClipboardPreview(false, previewGrids, copiedGrids);
                        }
                        return;
                    }
                }
                this.m_resultIDs.Clear();
                MyEntityIdentifier.GetPerThreadEntities(this.m_resultIDs);
                MyEntityIdentifier.ClearPerThreadEntities();
                MyEntityIdentifier.InEntityCreationBlock = false;
                this.m_visible = visible;
            }
        }

        private bool CheckLimitsAndNotify()
        {
            int blocksToBuild = 0;
            int pcuToBuild = 0;
            bool flag = true;
            foreach (MyCubeGrid grid in this.PreviewGrids)
            {
                if (MySession.Static.MaxGridSize != 0)
                {
                    flag &= grid.BlocksCount <= MySession.Static.MaxGridSize;
                }
                blocksToBuild += grid.BlocksCount;
                pcuToBuild += grid.BlocksPCU;
            }
            return MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, null, pcuToBuild, blocksToBuild, flag ? 0 : (MySession.Static.MaxGridSize + 1), this.m_blocksPerType);
        }

        protected bool CheckPastedBlocks() => 
            CheckPastedBlocks(this.m_copiedGrids);

        public static bool CheckPastedBlocks(IEnumerable<MyObjectBuilder_CubeGrid> pastedGrids)
        {
            using (IEnumerator<MyObjectBuilder_CubeGrid> enumerator = pastedGrids.GetEnumerator())
            {
                while (true)
                {
                    bool flag2;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyObjectBuilder_CubeGrid current = enumerator.Current;
                    bool flag = !MySession.Static.Settings.EnableSupergridding;
                    List<MyObjectBuilder_CubeBlock>.Enumerator enumerator2 = current.CubeBlocks.GetEnumerator();
                    try
                    {
                        while (true)
                        {
                            MyCubeBlockDefinition definition;
                            if (!enumerator2.MoveNext())
                            {
                                break;
                            }
                            MyObjectBuilder_CubeBlock block = enumerator2.Current;
                            MyDefinitionId defId = new MyDefinitionId(block.TypeId, block.SubtypeId);
                            if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition))
                            {
                                flag2 = false;
                            }
                            else
                            {
                                if (!flag)
                                {
                                    continue;
                                }
                                if (definition.CubeSize == current.GridSizeEnum)
                                {
                                    continue;
                                }
                                flag2 = false;
                            }
                            return flag2;
                        }
                        continue;
                    }
                    finally
                    {
                        enumerator2.Dispose();
                        continue;
                    }
                    return flag2;
                }
            }
            return true;
        }

        protected MyDLCs.MyDLC CheckPastedDLCBlocks()
        {
            MySessionComponentDLC component = MySession.Static.GetComponent<MySessionComponentDLC>();
            using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator = this.m_copiedGrids.GetEnumerator())
            {
                MyDLCs.MyDLC ydlc2;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        List<MyObjectBuilder_CubeBlock>.Enumerator enumerator2 = enumerator.Current.CubeBlocks.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                MyObjectBuilder_CubeBlock current = enumerator2.Current;
                                MyDefinitionId id = new MyDefinitionId(current.TypeId, current.SubtypeId);
                                MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(id);
                                MyDLCs.MyDLC firstMissingDefinitionDLC = component.GetFirstMissingDefinitionDLC(definition, Sync.MyId);
                                if (firstMissingDefinitionDLC != null)
                                {
                                    return firstMissingDefinitionDLC;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return ydlc2;
            }
        TR_0000:
            return null;
        }

        protected bool CheckPastedScripts()
        {
            if (MySession.Static.IsUserScripter(Sync.MyId))
            {
                return true;
            }
            using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator = this.m_copiedGrids.GetEnumerator())
            {
                bool flag;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        List<MyObjectBuilder_CubeBlock>.Enumerator enumerator2 = enumerator.Current.CubeBlocks.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                MyObjectBuilder_MyProgrammableBlock current = enumerator2.Current as MyObjectBuilder_MyProgrammableBlock;
                                if ((current != null) && (current.Program != null))
                                {
                                    return false;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                    }
                    else
                    {
                        goto TR_0001;
                    }
                    break;
                }
                return flag;
            }
        TR_0001:
            return true;
        }

        public void ClearClipboard()
        {
            if (this.IsActive)
            {
                this.Deactivate(false);
            }
            this.m_copiedGrids.Clear();
            this.m_copiedGridOffsets.Clear();
        }

        public void CopyGrid(MyCubeGrid grid)
        {
            if (grid != null)
            {
                this.m_copiedGrids.Clear();
                this.m_copiedGridOffsets.Clear();
                this.CopyGridInternal(grid);
            }
        }

        private void CopyGridInternal(MyCubeGrid toCopy)
        {
            if (MySession.Static.CameraController.Equals(toCopy))
            {
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(toCopy.PositionComp.GetPosition()));
            }
            MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) toCopy.GetObjectBuilder(true);
            this.m_copiedGrids.Add(objectBuilder);
            this.RemovePilots(objectBuilder);
            if (this.m_copiedGrids.Count == 1)
            {
                Vector3D translation;
                MatrixD pasteMatrix = GetPasteMatrix();
                Vector3I? nullable = toCopy.RayCastBlocks(pasteMatrix.Translation, pasteMatrix.Translation + (pasteMatrix.Forward * 1000.0));
                if (nullable == null)
                {
                    translation = toCopy.WorldMatrix.Translation;
                }
                else
                {
                    translation = toCopy.GridIntegerToWorld(nullable.Value);
                }
                Vector3D vectord = translation;
                this.m_dragPointToPositionLocal = (Vector3) Vector3D.TransformNormal(toCopy.PositionComp.GetPosition() - vectord, toCopy.PositionComp.WorldMatrixNormalizedInv);
                this.m_dragDistance = (float) (vectord - pasteMatrix.Translation).Length();
                this.m_pasteDirUp = (Vector3) toCopy.WorldMatrix.Up;
                this.m_pasteDirForward = (Vector3) toCopy.WorldMatrix.Forward;
                this.m_pasteOrientationAngle = 0f;
            }
            this.m_copiedGridOffsets.Add((Vector3) (toCopy.WorldMatrix.Translation - this.m_copiedGrids[0].PositionAndOrientation.Value.Position));
        }

        public void CopyGroup(MyCubeGrid gridInGroup, GridLinkTypeEnum groupType)
        {
            if (gridInGroup != null)
            {
                this.m_copiedGrids.Clear();
                this.m_copiedGridOffsets.Clear();
                if (MyFakes.ENABLE_COPY_GROUP && MyFakes.ENABLE_LARGE_STATIC_GROUP_COPY_FIRST)
                {
                    MyCubeGrid grid = null;
                    MyCubeGrid grid2 = null;
                    MyCubeGrid grid3 = null;
                    if (gridInGroup.GridSizeEnum != MyCubeSize.Large)
                    {
                        if ((gridInGroup.GridSizeEnum == MyCubeSize.Small) && gridInGroup.IsStatic)
                        {
                            grid3 = gridInGroup;
                        }
                    }
                    else
                    {
                        grid2 = gridInGroup;
                        if (gridInGroup.IsStatic)
                        {
                            grid = gridInGroup;
                        }
                    }
                    foreach (MyCubeGrid grid5 in MyCubeGridGroups.Static.GetGroups(groupType).GetGroupNodes(gridInGroup))
                    {
                        if ((grid2 == null) && (grid5.GridSizeEnum == MyCubeSize.Large))
                        {
                            grid2 = grid5;
                        }
                        if (((grid == null) && (grid5.GridSizeEnum == MyCubeSize.Large)) && grid5.IsStatic)
                        {
                            grid = grid5;
                        }
                        if (((grid3 == null) && (grid5.GridSizeEnum == MyCubeSize.Small)) && grid5.IsStatic)
                        {
                            grid3 = grid5;
                        }
                    }
                    MyCubeGrid toCopy = (grid != null) ? grid : null;
                    toCopy = (toCopy != null) ? toCopy : ((grid2 != null) ? grid2 : null);
                    toCopy = (toCopy != null) ? toCopy : ((grid3 != null) ? grid3 : null);
                    toCopy = (toCopy != null) ? toCopy : gridInGroup;
                    this.CopyGridInternal(toCopy);
                    foreach (MyCubeGrid grid6 in MyCubeGridGroups.Static.GetGroups(groupType).GetGroupNodes(toCopy))
                    {
                        if (!ReferenceEquals(grid6, toCopy))
                        {
                            this.CopyGridInternal(grid6);
                        }
                    }
                }
                else
                {
                    this.CopyGridInternal(gridInGroup);
                    if (MyFakes.ENABLE_COPY_GROUP)
                    {
                        foreach (MyCubeGrid grid7 in MyCubeGridGroups.Static.GetGroups(groupType).GetGroupNodes(gridInGroup))
                        {
                            if (!ReferenceEquals(grid7, gridInGroup))
                            {
                                this.CopyGridInternal(grid7);
                            }
                        }
                    }
                }
            }
        }

        public void CutGrid(MyCubeGrid grid)
        {
            if (grid != null)
            {
                this.CopyGrid(grid);
                this.DeleteGrid(grid);
            }
        }

        public void CutGroup(MyCubeGrid grid, GridLinkTypeEnum groupType)
        {
            if (grid != null)
            {
                this.CopyGroup(grid, groupType);
                this.DeleteGroup(grid, groupType);
            }
        }

        public virtual void Deactivate(bool afterPaste = false)
        {
            this.CreationMode = false;
            this.ChangeClipboardPreview(false, this.m_previewGrids, this.m_copiedGrids);
            this.IsActive = false;
            Action<MyGridClipboard, bool> deactivated = this.Deactivated;
            if (this.IsActive && (deactivated != null))
            {
                deactivated(this, afterPaste);
            }
            MyCoordinateSystem.Static.Visible = false;
            MyCoordinateSystem.Static.ResetSelection();
            MyCoordinateSystem.OnCoordinateChange -= new Action(this.OnCoordinateChange);
        }

        public void DeleteGrid(MyCubeGrid grid)
        {
            if (grid != null)
            {
                grid.SendGridCloseRequest();
            }
        }

        public void DeleteGroup(MyCubeGrid grid, GridLinkTypeEnum groupType)
        {
            if (grid != null)
            {
                if (MyFakes.ENABLE_COPY_GROUP)
                {
                    foreach (MyCubeGrid grid2 in MyCubeGridGroups.Static.GetGroups(groupType).GetGroupNodes(grid))
                    {
                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid2.GetBlocks().GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyCockpit fatBlock = enumerator2.Current.FatBlock as MyCockpit;
                                if ((fatBlock != null) && (fatBlock.Pilot != null))
                                {
                                    fatBlock.RequestRemovePilot();
                                }
                            }
                        }
                        grid2.SendGridCloseRequest();
                    }
                }
                else
                {
                    grid.SendGridCloseRequest();
                }
            }
        }

        private void DisablePhysicsRecursively(VRage.Game.Entity.MyEntity entity)
        {
            if ((entity.Physics != null) && entity.Physics.Enabled)
            {
                entity.Physics.Enabled = false;
            }
            MyCubeBlock block = entity as MyCubeBlock;
            if (((block != null) && (block.UseObjectsComponent.DetectorPhysics != null)) && block.UseObjectsComponent.DetectorPhysics.Enabled)
            {
                block.UseObjectsComponent.DetectorPhysics.Enabled = false;
            }
            if (block != null)
            {
                block.NeedsUpdate &= ~(MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME);
            }
            foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
            {
                this.DisablePhysicsRecursively(base2.Container.Entity as VRage.Game.Entity.MyEntity);
            }
        }

        public void DrawHud()
        {
            if (this.m_previewGrids != null)
            {
                MyCubeBlockDefinition firstBlockDefinition = this.GetFirstBlockDefinition(this.m_copiedGrids[0]);
                MyHud.BlockInfo.LoadDefinition(firstBlockDefinition, true);
                MyHud.BlockInfo.Visible = true;
            }
        }

        public virtual bool EntityCanPaste(VRage.Game.Entity.MyEntity pastingEntity)
        {
            if (this.m_copiedGrids.Count < 1)
            {
                return false;
            }
            if (MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                return true;
            }
            MyCubeBuilder.BuildComponent.GetGridSpawnMaterials(this.m_copiedGrids[0]);
            return MyCubeBuilder.BuildComponent.HasBuildingMaterials(pastingEntity, false);
        }

        protected unsafe void FixSnapTransformationBase6()
        {
            if (this.m_copiedGrids.Count != 0)
            {
                GetPasteMatrix();
                MyCubeGrid hitEntity = this.m_hitEntity as MyCubeGrid;
                if (hitEntity != null)
                {
                    MatrixD worldMatrix = this.m_previewGrids[0].WorldMatrix;
                    Matrix axisDefinitionMatrix = Matrix.Normalize((Matrix) hitEntity.WorldMatrix.GetOrientation());
                    Matrix toAlign = Matrix.Normalize((Matrix) worldMatrix.GetOrientation());
                    Matrix matrix3 = Matrix.AlignRotationToAxes(ref toAlign, ref axisDefinitionMatrix);
                    Matrix matrix = Matrix.Invert(toAlign) * matrix3;
                    int num = 0;
                    foreach (MyCubeGrid local1 in this.m_previewGrids)
                    {
                        worldMatrix = local1.WorldMatrix;
                        Matrix matrix5 = (Matrix) (worldMatrix.GetOrientation() * matrix);
                        Matrix.Invert(matrix5);
                        MatrixD xd2 = MatrixD.CreateWorld(this.m_pastePosition, matrix5.Forward, matrix5.Up);
                        local1.PositionComp.SetWorldMatrix(xd2, null, false, true, true, false, false, false);
                    }
                    if ((hitEntity.GridSizeEnum == MyCubeSize.Large) && (this.m_previewGrids[0].GridSizeEnum == MyCubeSize.Small))
                    {
                        Vector3 gridCoords = MyCubeBuilder.TransformLargeGridHitCoordToSmallGrid(this.m_pastePosition, hitEntity.PositionComp.WorldMatrixNormalizedInv, hitEntity.GridSize);
                        this.m_pastePosition = hitEntity.GridIntegerToWorld(gridCoords);
                        if (MyFakes.ENABLE_VR_BUILDING)
                        {
                            Vector3 vector2 = (Vector3) (Vector3I.Round(Vector3.TransformNormal(this.m_hitNormal, hitEntity.PositionComp.WorldMatrixNormalizedInv)) * (this.m_previewGrids[0].GridSize / hitEntity.GridSize));
                            Vector3 normal = (Vector3) Vector3I.Round(Vector3.TransformNormal(this.m_hitNormal, hitEntity.PositionComp.WorldMatrixNormalizedInv));
                            Vector3 vector3 = (Vector3) Vector3I.Round(Vector3D.TransformNormal(Vector3D.TransformNormal(normal, hitEntity.WorldMatrix), this.m_previewGrids[0].PositionComp.WorldMatrixNormalizedInv));
                            BoundingBox localAABB = this.m_previewGrids[0].PositionComp.LocalAABB;
                            Vector3* vectorPtr1 = (Vector3*) ref localAABB.Min;
                            vectorPtr1[0] /= this.m_previewGrids[0].GridSize;
                            Vector3* vectorPtr2 = (Vector3*) ref localAABB.Max;
                            vectorPtr2[0] /= this.m_previewGrids[0].GridSize;
                            Vector3 zero = Vector3.Zero;
                            Vector3 vector6 = Vector3.Zero;
                            BoundingBox box = new BoundingBox(-Vector3.Half, Vector3.Half);
                            box.Inflate((float) -0.05f);
                            box.Translate((-(this.m_dragPointToPositionLocal / this.m_previewGrids[0].GridSize) + zero) - vector3);
                            while (true)
                            {
                                if (localAABB.Contains(box) == ContainmentType.Disjoint)
                                {
                                    this.m_pastePosition = hitEntity.GridIntegerToWorld(gridCoords - vector6);
                                    break;
                                }
                                zero -= vector3;
                                vector6 -= vector2;
                                box.Translate(-vector3);
                            }
                        }
                    }
                    else
                    {
                        Vector3I vectori = Vector3I.Round(Vector3.TransformNormal(this.m_hitNormal, hitEntity.PositionComp.WorldMatrixNormalizedInv));
                        Vector3I gridOffset = hitEntity.WorldToGridInteger(this.m_pastePosition);
                        int num2 = 0;
                        while (true)
                        {
                            if ((num2 >= 100) || hitEntity.CanMergeCubes(this.m_previewGrids[0], gridOffset))
                            {
                                if (num2 == 0)
                                {
                                    num2 = 0;
                                    while (true)
                                    {
                                        if (num2 < 100)
                                        {
                                            gridOffset -= vectori;
                                            if (hitEntity.CanMergeCubes(this.m_previewGrids[0], gridOffset))
                                            {
                                                num2++;
                                                continue;
                                            }
                                        }
                                        gridOffset = (Vector3I) (gridOffset + vectori);
                                        break;
                                    }
                                }
                                if (num2 == 100)
                                {
                                    gridOffset = hitEntity.WorldToGridInteger(this.m_pastePosition);
                                }
                                this.m_pastePosition = hitEntity.GridIntegerToWorld(gridOffset);
                                break;
                            }
                            gridOffset = (Vector3I) (gridOffset + vectori);
                            num2++;
                        }
                    }
                    if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                    {
                        MyRenderProxy.DebugDrawLine3D(this.m_hitPos, this.m_hitPos + this.m_hitNormal, Color.Red, Color.Green, false, false);
                    }
                    num = 0;
                    foreach (MyCubeGrid local2 in this.m_previewGrids)
                    {
                        MatrixD xd3 = local2.WorldMatrix;
                        num++;
                        xd3.Translation = this.m_pastePosition + Vector3.Transform(this.m_copiedGridOffsets[num], matrix);
                        local2.PositionComp.SetWorldMatrix(xd3, null, false, true, true, false, false, false);
                    }
                }
            }
        }

        protected virtual VRage.Game.Entity.MyEntity GetClipboardBuilder() => 
            MySession.Static.LocalCharacter;

        public MyCubeBlockDefinition GetFirstBlockDefinition(MyObjectBuilder_CubeGrid grid = null)
        {
            if (grid == null)
            {
                if (this.m_copiedGrids.Count <= 0)
                {
                    return null;
                }
                grid = this.m_copiedGrids[0];
            }
            if (grid.CubeBlocks.Count <= 0)
            {
                return null;
            }
            MyDefinitionId id = grid.CubeBlocks[0].GetId();
            return MyDefinitionManager.Static.GetCubeBlockDefinition(id);
        }

        public virtual Matrix GetFirstGridOrientationMatrix() => 
            Matrix.CreateWorld(Vector3.Zero, this.m_pasteDirForward, this.m_pasteDirUp);

        protected static MatrixD GetPasteMatrix()
        {
            if ((MySession.Static.ControlledEntity == null) || ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return MySector.MainCamera.WorldMatrix;
            }
            return MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
        }

        protected static long? GetPasteSpawnerId()
        {
            if ((MySession.Static.ControlledEntity != null) && ((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Entity) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return new long?(MySession.Static.ControlledEntity.Entity.EntityId);
            }
            return null;
        }

        protected static Vector3D GetPasteSpawnerPosition()
        {
            if ((MySession.Static.ControlledEntity == null) || ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return MySector.MainCamera.WorldMatrix.Translation;
            }
            return MySession.Static.ControlledEntity.Entity.WorldMatrix.Translation;
        }

        private void GetTouchingGrids(MyCubeGrid grid, MyGridPlacementSettings settings)
        {
            this.m_touchingGrids.Clear();
            foreach (MySlimBlock block in grid.CubeBlocks)
            {
                if (block.FatBlock is MyCompoundCubeBlock)
                {
                    bool flag = false;
                    foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        MyCubeGrid touchingGrid = null;
                        MyCubeGrid.TestPlacementAreaCubeNoAABBInflate(block2.CubeGrid, ref settings, block2.Min, block2.Max, block2.Orientation, block2.BlockDefinition, out touchingGrid, block2.CubeGrid);
                        if (touchingGrid != null)
                        {
                            this.m_touchingGrids.Add(touchingGrid);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        continue;
                    }
                }
                else
                {
                    MyCubeGrid touchingGrid = null;
                    MyCubeGrid.TestPlacementAreaCubeNoAABBInflate(block.CubeGrid, ref settings, block.Min, block.Max, block.Orientation, block.BlockDefinition, out touchingGrid, block.CubeGrid);
                    if (touchingGrid == null)
                    {
                        continue;
                    }
                    this.m_touchingGrids.Add(touchingGrid);
                }
                break;
            }
        }

        public bool HasCopiedGrids() => 
            (this.m_copiedGrids.Count > 0);

        public void Hide()
        {
            if (MyFakes.ENABLE_VR_BUILDING)
            {
                this.ShowPreview(false);
            }
            else
            {
                this.ChangeClipboardPreview(false, this.m_previewGrids, this.m_copiedGrids);
            }
        }

        public void HideGridWhenColliding(List<Vector3D> collisionTestPoints)
        {
            if (this.m_previewGrids.Count != 0)
            {
                bool flag = true;
                foreach (Vector3D vectord in collisionTestPoints)
                {
                    foreach (MyCubeGrid grid in this.m_previewGrids)
                    {
                        Vector3D point = Vector3.Transform((Vector3) vectord, grid.PositionComp.WorldMatrixNormalizedInv);
                        BoundingBox localAABB = grid.PositionComp.LocalAABB;
                        if (localAABB.Contains(point) == ContainmentType.Contains)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
                using (List<MyCubeGrid>.Enumerator enumerator2 = this.m_previewGrids.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.Render.Visible = flag;
                    }
                }
            }
        }

        private void MakeTransparent(MyCubeGrid grid)
        {
            grid.Render.Transparency = this.Transparency;
            grid.Render.CastShadows = false;
            if (m_cacheEntitySet == null)
            {
                m_cacheEntitySet = new HashSet<IMyEntity>();
            }
            grid.Hierarchy.GetChildrenRecursive(m_cacheEntitySet);
            foreach (IMyEntity local1 in m_cacheEntitySet)
            {
                local1.Render.Transparency = this.Transparency;
                local1.Render.CastShadows = false;
            }
            m_cacheEntitySet.Clear();
        }

        public virtual void MoveEntityCloser()
        {
            this.m_dragDistance /= 1.1f;
        }

        public virtual void MoveEntityFurther()
        {
            float num = this.m_dragDistance * 1.1f;
            this.m_dragDistance = MathHelper.Clamp(num, this.m_dragDistance, 20000f);
        }

        private void OnCoordinateChange()
        {
            if (MyCoordinateSystem.Static.LocalCoordExist && this.AnyCopiedGridIsStatic)
            {
                this.EnableStationRotation = false;
                MyCoordinateSystem.Static.Visible = true;
            }
            if (!MyCoordinateSystem.Static.LocalCoordExist)
            {
                this.EnableStationRotation = true;
                MyCoordinateSystem.Static.Visible = false;
            }
            else
            {
                this.EnableStationRotation = false;
                MyCoordinateSystem.Static.Visible = true;
            }
            if ((!this.m_enableStationRotation && this.IsActive) && !this.m_isAligning)
            {
                this.m_isAligning = true;
                int sessionTotalFrames = MyFpsManager.GetSessionTotalFrames();
                if ((sessionTotalFrames - this.m_lastFrameAligned) >= 12)
                {
                    this.AlignRotationToCoordSys();
                    this.m_lastFrameAligned = sessionTotalFrames;
                }
                this.m_isAligning = false;
            }
        }

        public virtual bool PasteGrid(bool deactivate = true, bool showWarning = true)
        {
            try
            {
                this.UpdateTouchingGrids();
                return this.PasteGridInternal(deactivate, null, this.m_touchingGrids, null, false, showWarning);
            }
            catch
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                MyHud.Notifications.Add(MyNotificationSingletons.PasteFailed);
                return false;
            }
        }

        protected bool PasteGridInternal(bool deactivate, List<MyObjectBuilder_CubeGrid> pastedBuilders = null, List<MyCubeGrid> touchingGrids = null, UpdateAfterPasteCallback updateAfterPasteCallback = null, bool multiBlock = false, bool showWarning = true)
        {
            StringBuilder builder;
            MyStringId? nullable;
            Vector2? nullable2;
            if ((this.m_copiedGrids.Count == 0) & showWarning)
            {
                builder = MyTexts.Get(MyCommonTexts.Blueprints_EmptyClipboardMessageHeader);
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.Blueprints_EmptyClipboardMessage), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                return false;
            }
            if ((this.m_copiedGrids.Count > 0) && !this.IsActive)
            {
                this.Activate(null);
                return true;
            }
            if (!this.CanBePlaced)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                return false;
            }
            if (this.CheckLimitsAndNotify())
            {
                List<MyCubeGrid>.Enumerator enumerator;
                if (this.m_previewGrids.Count == 0)
                {
                    return false;
                }
                using (enumerator = this.m_previewGrids.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (!MySessionComponentSafeZones.IsActionAllowed(enumerator.Current, MySafeZoneAction.Building, MySession.Static.LocalCharacterEntityId))
                        {
                            return false;
                        }
                    }
                }
                if (!MySession.Static.IsSettingsExperimental())
                {
                    bool flag2 = false;
                    using (enumerator = this.m_previewGrids.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            HashSetReader<MyCubeBlock> unsafeBlocks = enumerator.Current.UnsafeBlocks;
                            if (unsafeBlocks.Count > 0)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                    }
                    if (flag2)
                    {
                        builder = MyTexts.Get(MyCommonTexts.Blueprints_UnsafeClipboardMessageHeader);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.Blueprints_UnsafeClipboardMessage), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        return false;
                    }
                }
                if (!MySession.Static.IsUserAdmin(Sync.MyId) || !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.KeepOriginalOwnershipOnPaste))
                {
                    using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator2 = this.m_copiedGrids.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            foreach (MyObjectBuilder_CubeBlock block in enumerator2.Current.CubeBlocks)
                            {
                                block.BuiltBy = MySession.Static.LocalPlayerId;
                                if ((block.Owner != 0) && Sync.Players.IdentityIsNpc(block.Owner))
                                {
                                    block.Owner = MySession.Static.LocalPlayerId;
                                }
                            }
                        }
                    }
                }
                bool missingBlockDefinitions = false;
                if (this.ShowModdedBlocksWarning)
                {
                    missingBlockDefinitions = !this.CheckPastedBlocks();
                }
                if (missingBlockDefinitions)
                {
                    this.AllowSwitchCameraMode = false;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextDoYouWantToPasteGridWithMissingBlocks), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            this.PasteInternal(missingBlockDefinitions, deactivate, pastedBuilders, null, updateAfterPasteCallback, multiBlock, false);
                        }
                        this.AllowSwitchCameraMode = true;
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    return false;
                }
                MyDLCs.MyDLC missingDLC = this.CheckPastedDLCBlocks();
                if (missingDLC == null)
                {
                    if (!MySession.Static.IsUserScripter(Sync.MyId) && !this.CheckPastedScripts())
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.BlueprintScriptsRemoved);
                    }
                    return this.PasteInternal(missingBlockDefinitions, deactivate, pastedBuilders, touchingGrids, updateAfterPasteCallback, multiBlock, true);
                }
                this.AllowSwitchCameraMode = false;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.MessageBoxTextMissingDLCWhenPasting), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, new MyStringId?(MyCommonTexts.VisitStore), new MyStringId?(MyCommonTexts.PasteAnyway), delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        MyGameService.OpenOverlayUrl(missingDLC.URL);
                    }
                    else if (result == MyGuiScreenMessageBox.ResultEnum.NO)
                    {
                        this.PasteInternal(missingBlockDefinitions, deactivate, pastedBuilders, null, updateAfterPasteCallback, multiBlock, false);
                    }
                    this.AllowSwitchCameraMode = true;
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            return false;
        }

        private bool PasteInternal(bool missingDefinitions, bool deactivate, List<MyObjectBuilder_CubeGrid> pastedBuilders = null, List<MyCubeGrid> touchingGrids = null, UpdateAfterPasteCallback updateAfterPasteCallback = null, bool multiBlock = false, bool keepRelativeOffset = false)
        {
            int num1;
            List<MyObjectBuilder_CubeGrid> gridsToMerge = new List<MyObjectBuilder_CubeGrid>();
            foreach (MyObjectBuilder_CubeGrid grid3 in this.m_copiedGrids)
            {
                gridsToMerge.Add((MyObjectBuilder_CubeGrid) grid3.Clone());
            }
            MyObjectBuilder_CubeGrid grid = gridsToMerge[0];
            if ((!this.IsSnapped || ((this.SnapMode != VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode.Base6Directions) || !(this.m_hitEntity is MyCubeGrid))) || (grid == null))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (((MyCubeGrid) this.m_hitEntity).GridSizeEnum == grid.GridSizeEnum);
            }
            bool flag = (bool) num1;
            if (flag && !this.CheckLimitsAndNotify())
            {
                return false;
            }
            MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
            MyCubeGrid hitEntity = null;
            if (flag)
            {
                hitEntity = this.m_hitEntity as MyCubeGrid;
            }
            flag |= (touchingGrids != null) && (touchingGrids.Count > 0);
            if (((hitEntity == null) && (touchingGrids != null)) && (touchingGrids.Count > 0))
            {
                hitEntity = touchingGrids[0];
            }
            int num = 0;
            foreach (MyObjectBuilder_CubeGrid local1 in gridsToMerge)
            {
                local1.CreatePhysics = true;
                local1.EnableSmallToLargeConnections = true;
                local1.PositionAndOrientation = new MyPositionAndOrientation(this.m_previewGrids[num].WorldMatrix);
                MyPositionAndOrientation orientation = local1.PositionAndOrientation.Value;
                orientation.Orientation.Normalize();
                num++;
            }
            long inventoryEntityId = 0L;
            bool instantBuild = MySession.Static.CreativeToolsEnabled(Sync.MyId);
            if (flag && (hitEntity != null))
            {
                hitEntity.PasteBlocksToGrid(gridsToMerge, inventoryEntityId, multiBlock, instantBuild);
            }
            else
            {
                EndpointId id;
                Vector3D? nullable;
                if (this.CreationMode)
                {
                    id = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<MyCubeSize, bool, MyPositionAndOrientation, long, bool>(s => new Action<MyCubeSize, bool, MyPositionAndOrientation, long, bool>(MyCubeGrid.TryCreateGrid_Implementation), this.CubeSize, this.IsStatic, gridsToMerge[0].PositionAndOrientation.Value, inventoryEntityId, instantBuild, id, nullable);
                    this.CreationMode = false;
                }
                else if (MySession.Static.CreativeMode || MySession.Static.HasCreativeRights)
                {
                    bool flag3 = false;
                    bool flag4 = false;
                    foreach (MyCubeGrid grid4 in this.m_previewGrids)
                    {
                        flag4 |= grid4.GridSizeEnum == MyCubeSize.Small;
                        MyGridPlacementSettings gridPlacementSettings = this.m_settings.GetGridPlacementSettings(grid4.GridSizeEnum, grid4.IsStatic);
                        flag3 |= MyCubeGrid.IsAabbInsideVoxel(grid4.PositionComp.WorldMatrix, grid4.PositionComp.LocalAABB, gridPlacementSettings);
                    }
                    bool flag5 = false;
                    if (MyPerGameSettings.Game == GameEnum.ME_GAME)
                    {
                        MyCubeGrid hitEntity = this.m_hitEntity as MyCubeGrid;
                        if (hitEntity != null)
                        {
                            flag5 = flag4 && (hitEntity.GridSizeEnum == MyCubeSize.Large);
                        }
                    }
                    foreach (MyObjectBuilder_CubeGrid grid6 in gridsToMerge)
                    {
                        grid6.IsStatic = (flag5 | flag3) || (MySession.Static.EnableConvertToStation && grid6.IsStatic);
                    }
                    if (!Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        MyHud.PushRotatingWheelVisible();
                    }
                    MyCubeGrid.RelativeOffset offset = new MyCubeGrid.RelativeOffset();
                    if (!keepRelativeOffset || Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        offset.Use = false;
                    }
                    else
                    {
                        offset.Use = true;
                        if (this.m_spawnerId != null)
                        {
                            offset.RelativeToEntity = true;
                            offset.SpawnerId = this.m_spawnerId.Value;
                        }
                        else
                        {
                            offset.RelativeToEntity = false;
                            offset.SpawnerId = 0L;
                        }
                        offset.OriginalSpawnPoint = this.m_originalSpawnerPosition;
                    }
                    id = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(s => new Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(MyCubeGrid.TryPasteGrid_Implementation), gridsToMerge, missingDefinitions, this.m_objectVelocity, multiBlock, instantBuild, offset, id, nullable);
                }
            }
            if (deactivate)
            {
                this.Deactivate(true);
            }
            if (updateAfterPasteCallback != null)
            {
                updateAfterPasteCallback(pastedBuilders);
            }
            return true;
        }

        private void previewGrid_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            this.m_previewGrids.Remove(obj as MyCubeGrid);
            int count = this.m_previewGrids.Count;
        }

        private void RemovePilots(MyObjectBuilder_CubeGrid grid)
        {
            foreach (MyObjectBuilder_CubeBlock block in grid.CubeBlocks)
            {
                MyObjectBuilder_Cockpit cockpit = block as MyObjectBuilder_Cockpit;
                if (cockpit == null)
                {
                    MyObjectBuilder_LandingGear gear = block as MyObjectBuilder_LandingGear;
                    if (gear == null)
                    {
                        continue;
                    }
                    gear.IsLocked = false;
                    gear.MasterToSlave = null;
                    gear.AttachedEntityId = null;
                    gear.LockMode = LandingGearMode.Unlocked;
                    continue;
                }
                cockpit.ClearPilotAndAutopilot();
                if ((cockpit.ComponentContainer != null) && (cockpit.ComponentContainer.Components != null))
                {
                    foreach (MyObjectBuilder_ComponentContainer.ComponentData data in cockpit.ComponentContainer.Components)
                    {
                        if (data.TypeId == typeof(MyHierarchyComponentBase).Name)
                        {
                            ((MyObjectBuilder_HierarchyComponentBase) data.Component).Children.RemoveAll(x => x is MyObjectBuilder_Character);
                            break;
                        }
                    }
                }
                MyObjectBuilder_CryoChamber chamber = cockpit as MyObjectBuilder_CryoChamber;
                if (chamber != null)
                {
                    chamber.Clear();
                }
            }
        }

        private void RightMinus(float angle)
        {
            this.RightPlus(-angle);
        }

        private void RightPlus(float angle)
        {
            if (!this.OneAxisRotationMode)
            {
                this.ApplyOrientationAngle();
                Vector3 vector = Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
                float num = (float) Math.Cos((double) angle);
                this.m_pasteDirUp = (this.m_pasteDirUp * num) + (vector * ((float) Math.Sin((double) angle)));
            }
        }

        public void RotateAroundAxis(int axisIndex, int sign, bool newlyPressed, float angleDelta)
        {
            if ((!this.EnableStationRotation || this.IsSnapped) && !this.EnablePreciseRotationWhenSnapped)
            {
                if (!newlyPressed)
                {
                    return;
                }
                angleDelta = 1.570796f;
            }
            switch (axisIndex)
            {
                case 0:
                    if (sign < 0)
                    {
                        this.UpMinus(angleDelta);
                    }
                    else
                    {
                        this.UpPlus(angleDelta);
                    }
                    break;

                case 1:
                    if (sign < 0)
                    {
                        this.AngleMinus(angleDelta);
                    }
                    else
                    {
                        this.AnglePlus(angleDelta);
                    }
                    break;

                case 2:
                    if (sign < 0)
                    {
                        this.RightPlus(angleDelta);
                    }
                    else
                    {
                        this.RightMinus(angleDelta);
                    }
                    break;

                default:
                    break;
            }
            this.ApplyOrientationAngle();
        }

        public void SaveClipboardAsPrefab(string name = null, string path = null)
        {
            if (this.m_copiedGrids.Count != 0)
            {
                name = name ?? (MyWorldGenerator.GetPrefabTypeName(this.m_copiedGrids[0]) + "_" + MyUtils.GetRandomInt(0xf4240, 0x98967f));
                if (path == null)
                {
                    MyPrefabManager.SavePrefab(name, this.m_copiedGrids);
                }
                else
                {
                    MyPrefabManager.SavePrefabToPath(name, path, this.m_copiedGrids);
                }
                MyHud.Notifications.Add(new MyHudNotificationDebug("Prefab saved: " + path, 0x2710, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
            }
        }

        public void SetGridFromBuilder(MyObjectBuilder_CubeGrid grid, Vector3 dragPointDelta, float dragVectorLength)
        {
            this.m_copiedGrids.Clear();
            this.m_copiedGridOffsets.Clear();
            this.m_dragPointToPositionLocal = dragPointDelta;
            this.m_dragDistance = dragVectorLength;
            MyPositionAndOrientation? positionAndOrientation = grid.PositionAndOrientation;
            MyPositionAndOrientation orientation = (positionAndOrientation != null) ? positionAndOrientation.GetValueOrDefault() : MyPositionAndOrientation.Default;
            this.m_pasteDirUp = (Vector3) orientation.Up;
            this.m_pasteDirForward = (Vector3) orientation.Forward;
            this.SetGridFromBuilderInternal(grid, Vector3.Zero);
        }

        private void SetGridFromBuilderInternal(MyObjectBuilder_CubeGrid grid, Vector3 offset)
        {
            this.BeforeCreateGrid(grid);
            this.m_copiedGrids.Add(grid);
            this.m_copiedGridOffsets.Add(offset);
            this.RemovePilots(grid);
        }

        public void SetGridFromBuilders(MyObjectBuilder_CubeGrid[] grids, Vector3 dragPointDelta, float dragVectorLength)
        {
            this.ShowModdedBlocksWarning = true;
            if (this.IsActive)
            {
                this.Deactivate(false);
            }
            this.m_copiedGrids.Clear();
            this.m_copiedGridOffsets.Clear();
            if (grids.Length != 0)
            {
                MatrixD identity;
                this.m_dragPointToPositionLocal = dragPointDelta;
                this.m_dragDistance = dragVectorLength;
                MyPositionAndOrientation? positionAndOrientation = grids[0].PositionAndOrientation;
                MyPositionAndOrientation orientation = (positionAndOrientation != null) ? positionAndOrientation.GetValueOrDefault() : MyPositionAndOrientation.Default;
                this.m_pasteDirUp = (Vector3) orientation.Up;
                this.m_pasteDirForward = (Vector3) orientation.Forward;
                this.SetGridFromBuilderInternal(grids[0], Vector3.Zero);
                if (grids[0].PositionAndOrientation == null)
                {
                    identity = MatrixD.Identity;
                }
                else
                {
                    identity = grids[0].PositionAndOrientation.Value.GetMatrix();
                }
                MatrixD matrix = MatrixD.Invert(identity);
                for (int i = 1; i < grids.Length; i++)
                {
                    this.SetGridFromBuilderInternal(grids[i], (Vector3) Vector3D.Transform((grids[i].PositionAndOrientation != null) ? ((Vector3D) grids[i].PositionAndOrientation.Value.Position) : Vector3D.Zero, matrix));
                }
            }
        }

        public void Show()
        {
            if (this.IsActive && !this.m_isBeingAdded)
            {
                if (this.m_previewGrids.Count == 0)
                {
                    this.ChangeClipboardPreview(true, this.m_previewGrids, this.m_copiedGrids);
                }
                if (MyFakes.ENABLE_VR_BUILDING)
                {
                    this.ShowPreview(true);
                }
            }
        }

        protected void ShowPreview(bool show)
        {
            if ((this.PreviewGrids.Count != 0) && (this.PreviewGrids[0].Render.Visible != show))
            {
                foreach (MyCubeGrid local1 in this.PreviewGrids)
                {
                    local1.Render.Visible = show;
                    foreach (MySlimBlock block in local1.GetBlocks())
                    {
                        MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                        if (fatBlock != null)
                        {
                            fatBlock.Render.UpdateRenderObject(show, true);
                            foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                            {
                                if (block3.FatBlock != null)
                                {
                                    block3.FatBlock.Render.UpdateRenderObject(show, true);
                                }
                            }
                            continue;
                        }
                        if (block.FatBlock != null)
                        {
                            block.FatBlock.Render.UpdateRenderObject(show, true);
                        }
                    }
                }
            }
        }

        protected virtual void TestBuildingMaterials()
        {
            this.m_characterHasEnoughMaterials = this.EntityCanPaste(this.GetClipboardBuilder());
        }

        protected virtual bool TestPlacement()
        {
            this.m_canBePlacedNeedsRefresh = false;
            if (MyFakes.DISABLE_CLIPBOARD_PLACEMENT_TEST)
            {
                return true;
            }
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(this.m_pastePosition))
            {
                return false;
            }
            bool flag = true;
            for (int i = 0; i < this.m_previewGrids.Count; i++)
            {
                MyCubeGrid targetGrid = this.m_previewGrids[i];
                if (!MySessionComponentSafeZones.IsActionAllowed(targetGrid.PositionComp.WorldAABB, MySafeZoneAction.Building, 0L))
                {
                    flag = false;
                }
                if (flag)
                {
                    if (((i != 0) || (!(this.m_hitEntity is MyCubeGrid) || !this.IsSnapped)) || (this.SnapMode != VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode.Base6Directions))
                    {
                        MyGridPlacementSettings gridPlacementSettings = this.m_settings.GetGridPlacementSettings(targetGrid.GridSizeEnum, targetGrid.IsStatic);
                        flag &= MyCubeGrid.TestPlacementArea(targetGrid, targetGrid.IsStatic, ref gridPlacementSettings, targetGrid.PositionComp.LocalAABB, false, null, true, true);
                    }
                    else
                    {
                        MyCubeGrid hitEntity = this.m_hitEntity as MyCubeGrid;
                        bool flag2 = (hitEntity.GridSizeEnum == MyCubeSize.Large) && (targetGrid.GridSizeEnum == MyCubeSize.Small);
                        MyGridPlacementSettings gridPlacementSettings = this.m_settings.GetGridPlacementSettings(targetGrid.GridSizeEnum, targetGrid.IsStatic);
                        if ((MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE && targetGrid.IsStatic) & flag2)
                        {
                            flag &= MyCubeGrid.TestPlacementArea(targetGrid, targetGrid.IsStatic, ref gridPlacementSettings, targetGrid.PositionComp.LocalAABB, false, hitEntity, true, true);
                        }
                        else
                        {
                            Vector3I gridOffset = hitEntity.WorldToGridInteger(this.m_pastePosition);
                            if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                            {
                                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 60f), "First grid offset: " + gridOffset.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            }
                            flag = ((flag & ((hitEntity.GridSizeEnum == targetGrid.GridSizeEnum) && hitEntity.CanMergeCubes(targetGrid, gridOffset))) & MyCubeGrid.CheckMergeConnectivity(hitEntity, targetGrid, gridOffset)) & MyCubeGrid.TestPlacementArea(targetGrid, targetGrid.IsStatic, ref gridPlacementSettings, targetGrid.PositionComp.LocalAABB, false, hitEntity, true, true);
                        }
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return flag;
        }

        protected bool TrySnapToSurface(VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode snapMode)
        {
            if (this.m_closestHitDistSq >= float.MaxValue)
            {
                this.IsSnapped = false;
                return false;
            }
            Vector3D hitPos = this.m_hitPos;
            if (this.m_hitNormal.Length() > 0.5)
            {
                MyCubeGrid hitEntity = this.m_hitEntity as MyCubeGrid;
                if (hitEntity != null)
                {
                    Matrix orientation = (Matrix) hitEntity.WorldMatrix.GetOrientation();
                    Matrix toAlign = this.GetFirstGridOrientationMatrix();
                    Matrix matrix4 = Matrix.AlignRotationToAxes(ref toAlign, ref orientation);
                    Matrix matrix1 = Matrix.Invert(toAlign) * matrix4;
                    this.m_pasteDirForward = matrix4.Forward;
                    this.m_pasteDirUp = matrix4.Up;
                    this.m_pasteOrientationAngle = 0f;
                }
            }
            Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
            this.m_pastePosition = hitPos + Vector3.TransformNormal(this.m_dragPointToPositionLocal, firstGridOrientationMatrix);
            if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
            {
                MyRenderProxy.DebugDrawSphere(hitPos, 0.08f, Color.Red.ToVector3(), 1f, false, false, true, false);
                MyRenderProxy.DebugDrawSphere(this.m_pastePosition, 0.08f, Color.Red.ToVector3(), 1f, false, false, true, false);
            }
            this.IsSnapped = true;
            return true;
        }

        public virtual void Update()
        {
            if (this.IsActive && this.m_visible)
            {
                this.UpdateHitEntity(true);
                this.UpdatePastePosition();
                this.UpdateGridTransformations();
                if (this.IsSnapped && (this.SnapMode == VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode.Base6Directions))
                {
                    this.FixSnapTransformationBase6();
                }
                if (this.m_calculateVelocity)
                {
                    this.m_objectVelocity = (Vector3) ((this.m_pastePosition - this.m_pastePositionPrevious) / 0.01666666753590107);
                }
                if ((MyFpsManager.GetSessionTotalFrames() % 11) == 0)
                {
                    this.m_canBePlaced = this.TestPlacement();
                }
                else
                {
                    this.m_canBePlacedNeedsRefresh = true;
                }
                this.m_characterHasEnoughMaterials = true;
                this.UpdatePreviewBBox();
                if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "FW: " + this.m_pasteDirForward.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 20f), "UP: " + this.m_pasteDirUp.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 40f), "AN: " + this.m_pasteOrientationAngle.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
            }
        }

        protected virtual void UpdateGridTransformations()
        {
            if (this.m_copiedGrids.Count != 0)
            {
                Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                Matrix matrix = Matrix.Invert((Matrix) this.m_copiedGrids[0].PositionAndOrientation.Value.GetMatrix()).GetOrientation() * firstGridOrientationMatrix;
                for (int i = 0; (i < this.m_previewGrids.Count) && (i <= (this.m_copiedGrids.Count - 1)); i++)
                {
                    if (this.m_copiedGrids[i].PositionAndOrientation != null)
                    {
                        MatrixD rotationMatrix = this.m_copiedGrids[i].PositionAndOrientation.Value.GetMatrix();
                        this.m_copiedGridOffsets[i] = Vector3.TransformNormal(rotationMatrix.Translation - this.m_copiedGrids[0].PositionAndOrientation.Value.Position, matrix);
                        rotationMatrix *= matrix;
                        rotationMatrix.Translation = Vector3.Zero;
                        rotationMatrix = MatrixD.Orthogonalize(rotationMatrix);
                        rotationMatrix.Translation = this.m_pastePosition + this.m_copiedGridOffsets[i];
                        this.m_previewGrids[i].PositionComp.SetWorldMatrix(rotationMatrix, null, false, true, true, true, false, false);
                    }
                }
            }
        }

        protected bool UpdateHitEntity(bool canPasteLargeOnSmall = true)
        {
            this.m_closestHitDistSq = float.MaxValue;
            this.m_hitPos = new Vector3(0f, 0f, 0f);
            this.m_hitNormal = new Vector3(1f, 0f, 0f);
            this.m_hitEntity = null;
            MatrixD pasteMatrix = GetPasteMatrix();
            if (MyFakes.ENABLE_VR_BUILDING && (MyBlockBuilderBase.PlacementProvider != null))
            {
                if (MyBlockBuilderBase.PlacementProvider.HitInfo == null)
                {
                    return false;
                }
                MyCubeGrid closestGrid = MyBlockBuilderBase.PlacementProvider.ClosestGrid;
                this.m_hitEntity = closestGrid ?? MyBlockBuilderBase.PlacementProvider.ClosestVoxelMap;
                this.m_hitPos = MyBlockBuilderBase.PlacementProvider.HitInfo.Value.Position;
                this.m_hitNormal = MyBlockBuilderBase.PlacementProvider.HitInfo.Value.HkHitInfo.Normal;
                this.m_hitNormal = (Vector3) Base6Directions.GetIntVector(Base6Directions.GetClosestDirection(Vector3.TransformNormal(this.m_hitNormal, this.m_hitEntity.PositionComp.WorldMatrixNormalizedInv)));
                this.m_hitNormal = Vector3.TransformNormal(this.m_hitNormal, this.m_hitEntity.PositionComp.WorldMatrix);
                this.m_closestHitDistSq = (float) (this.m_hitPos - pasteMatrix.Translation).LengthSquared();
                return true;
            }
            MyPhysics.CastRay(pasteMatrix.Translation, pasteMatrix.Translation + (pasteMatrix.Forward * this.m_dragDistance), this.m_raycastCollisionResults, 15);
            foreach (MyPhysics.HitInfo info in this.m_raycastCollisionResults)
            {
                if (info.HkHitInfo.Body == null)
                {
                    continue;
                }
                IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                if (hitEntity != null)
                {
                    MyCubeGrid grid = hitEntity as MyCubeGrid;
                    if (((canPasteLargeOnSmall || ((this.m_previewGrids.Count == 0) || ((this.m_previewGrids[0].GridSizeEnum != MyCubeSize.Large) || (grid == null)))) || (grid.GridSizeEnum != MyCubeSize.Small)) && ((hitEntity is MyVoxelBase) || (((grid != null) && (this.m_previewGrids.Count != 0)) && (grid.EntityId != this.m_previewGrids[0].EntityId))))
                    {
                        float num = (float) (info.Position - pasteMatrix.Translation).LengthSquared();
                        if (num < this.m_closestHitDistSq)
                        {
                            this.m_closestHitDistSq = num;
                            this.m_hitPos = info.Position;
                            this.m_hitNormal = info.HkHitInfo.Normal;
                            this.m_hitEntity = hitEntity;
                        }
                    }
                }
            }
            this.m_raycastCollisionResults.Clear();
            return true;
        }

        protected virtual void UpdatePastePosition()
        {
            if (this.m_previewGrids.Count != 0)
            {
                this.m_pastePositionPrevious = this.m_pastePosition;
                MatrixD pasteMatrix = GetPasteMatrix();
                this.m_spawnerId = GetPasteSpawnerId();
                this.m_originalSpawnerPosition = GetPasteSpawnerPosition();
                Vector3 vector = (Vector3) (pasteMatrix.Forward * this.m_dragDistance);
                MyGridPlacementSettings gridPlacementSettings = this.m_settings.GetGridPlacementSettings(this.m_previewGrids[0].GridSizeEnum);
                if (!this.TrySnapToSurface(gridPlacementSettings.SnapMode))
                {
                    this.m_pastePosition = pasteMatrix.Translation + vector;
                    Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                    this.m_pastePosition += Vector3.TransformNormal(this.m_dragPointToPositionLocal, firstGridOrientationMatrix);
                }
                double gridSize = this.m_previewGrids[0].GridSize;
                long? id = null;
                MyCoordinateSystem.CoordSystemData data = MyCoordinateSystem.Static.SnapWorldPosToClosestGrid(ref this.m_pastePosition, gridSize, this.m_settings.StaticGridAlignToCenter, id);
                if (MyCoordinateSystem.Static.LocalCoordExist && !this.EnableStationRotation)
                {
                    this.m_pastePosition = data.SnappedTransform.Position;
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                {
                    MyRenderProxy.DebugDrawSphere(pasteMatrix.Translation + vector, 0.15f, Color.Pink.ToVector3(), 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawSphere(this.m_pastePosition, 0.15f, Color.Pink.ToVector3(), 1f, false, false, true, false);
                }
            }
        }

        private void UpdatePreviewBBox()
        {
            if ((this.m_previewGrids != null) && !this.m_isBeingAdded)
            {
                List<MyCubeGrid>.Enumerator enumerator;
                if (!this.m_visible || !this.HasPreviewBBox)
                {
                    using (enumerator = this.m_previewGrids.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            VRageMath.Vector4? color = null;
                            Vector3? inflateAmount = null;
                            MyStringId? lineMaterial = null;
                            Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(enumerator.Current, false, color, 0.01f, inflateAmount, lineMaterial);
                        }
                    }
                }
                else
                {
                    VRageMath.Vector4 vector = new VRageMath.Vector4(Color.White.ToVector3(), 1f);
                    MyStringId id = ID_GIZMO_DRAW_LINE_RED;
                    bool flag = false;
                    if (!MySession.Static.IsSettingsExperimental())
                    {
                        using (enumerator = this.m_previewGrids.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                HashSetReader<MyCubeBlock> unsafeBlocks = enumerator.Current.UnsafeBlocks;
                                if (unsafeBlocks.Count > 0)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (this.m_canBePlaced && !flag)
                    {
                        if (this.m_characterHasEnoughMaterials)
                        {
                            id = ID_GIZMO_DRAW_LINE;
                        }
                        else
                        {
                            vector = Color.Gray.ToVector4();
                        }
                    }
                    int num = 0;
                    int num2 = 0;
                    Vector3 vector2 = new Vector3(0.1f);
                    foreach (MyCubeGrid grid in this.m_previewGrids)
                    {
                        Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(grid, true, new VRageMath.Vector4?(vector), 0.04f, new Vector3?(vector2), new MyStringId?(id));
                        num += grid.BlocksPCU;
                        num2 += grid.BlocksCount;
                    }
                    if (!Sync.IsDedicated)
                    {
                        StringBuilder builder = MyTexts.Get(MyCommonTexts.Clipboard_TotalPCU);
                        StringBuilder builder2 = MyTexts.Get(MyCommonTexts.Clipboard_TotalBlocks);
                        string[] textArray1 = new string[] { builder.ToString(), num.ToString(), "\n", builder2.ToString(), num2.ToString() };
                        Color? colorMask = null;
                        MyGuiManager.DrawString("White", new StringBuilder(string.Concat(textArray1)), new Vector2(0.51f, 0.51f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                    }
                }
            }
        }

        protected void UpdateTouchingGrids()
        {
            for (int i = 0; i < this.m_previewGrids.Count; i++)
            {
                MyCubeGrid grid = this.m_previewGrids[i];
                MyGridPlacementSettings gridPlacementSettings = this.m_settings.GetGridPlacementSettings(grid.GridSizeEnum, grid.IsStatic);
                this.GetTouchingGrids(grid, gridPlacementSettings);
            }
        }

        private void UpMinus(float angle)
        {
            this.UpPlus(-angle);
        }

        private void UpPlus(float angle)
        {
            if (!this.OneAxisRotationMode)
            {
                this.ApplyOrientationAngle();
                Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
                float num = (float) Math.Cos((double) angle);
                float num2 = (float) Math.Sin((double) angle);
                Vector3 vector = (this.m_pasteDirUp * num) - (this.m_pasteDirForward * num2);
                this.m_pasteDirForward = (this.m_pasteDirUp * num2) + (this.m_pasteDirForward * num);
                this.m_pasteDirUp = vector;
            }
        }

        protected virtual bool CanBePlaced
        {
            get
            {
                if (this.m_canBePlacedNeedsRefresh)
                {
                    this.m_canBePlaced = this.TestPlacement();
                }
                return this.m_canBePlaced;
            }
        }

        public bool CharacterHasEnoughMaterials =>
            this.m_characterHasEnoughMaterials;

        public virtual bool HasPreviewBBox
        {
            get => 
                true;
            set
            {
            }
        }

        public bool IsActive { get; protected set; }

        public bool AllowSwitchCameraMode
        {
            get => 
                this.m_allowSwitchCameraMode;
            private set => 
                (this.m_allowSwitchCameraMode = value);
        }

        public bool IsSnapped { get; protected set; }

        public List<MyObjectBuilder_CubeGrid> CopiedGrids =>
            this.m_copiedGrids;

        public VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode SnapMode =>
            ((this.m_previewGrids.Count != 0) ? this.m_settings.GetGridPlacementSettings(this.m_previewGrids[0].GridSizeEnum).SnapMode : VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode.Base6Directions);

        public bool EnablePreciseRotationWhenSnapped =>
            ((this.m_previewGrids.Count != 0) ? (this.m_settings.GetGridPlacementSettings(this.m_previewGrids[0].GridSizeEnum).EnablePreciseRotationWhenSnapped && this.EnableStationRotation) : false);

        public bool OneAxisRotationMode =>
            (this.IsSnapped && (this.SnapMode == VRage.Game.ObjectBuilders.Definitions.SessionComponents.SnapMode.OneFreeAxis));

        public List<MyCubeGrid> PreviewGrids =>
            this.m_previewGrids;

        protected virtual bool AnyCopiedGridIsStatic
        {
            get
            {
                using (List<MyCubeGrid>.Enumerator enumerator = this.m_previewGrids.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.IsStatic)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool EnableStationRotation
        {
            get => 
                (this.m_enableStationRotation && MyFakes.ENABLE_STATION_ROTATION);
            set
            {
                if (this.m_enableStationRotation != value)
                {
                    this.m_enableStationRotation = value;
                    if (this.IsActive && this.m_enableStationRotation)
                    {
                        this.AlignClipboardToGravity();
                        MyCoordinateSystem.Static.Visible = false;
                    }
                    else if (this.IsActive && !this.m_enableStationRotation)
                    {
                        this.AlignRotationToCoordSys();
                        MyCoordinateSystem.Static.Visible = true;
                    }
                }
            }
        }

        public bool CreationMode { get; set; }

        public MyCubeSize CubeSize { get; set; }

        public bool IsStatic { get; set; }

        public bool IsBeingAdded
        {
            get => 
                this.m_isBeingAdded;
            set => 
                (this.m_isBeingAdded = value);
        }

        protected virtual float Transparency =>
            0.25f;

        public string CopiedGridsName =>
            (!this.HasCopiedGrids() ? null : this.m_copiedGrids[0].DisplayName);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridClipboard.<>c <>9 = new MyGridClipboard.<>c();
            public static Func<IMyEventOwner, Action<MyCubeSize, bool, MyPositionAndOrientation, long, bool>> <>9__115_0;
            public static Func<IMyEventOwner, Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>> <>9__115_1;
            public static Predicate<MyObjectBuilder_EntityBase> <>9__156_0;

            internal Action<MyCubeSize, bool, MyPositionAndOrientation, long, bool> <PasteInternal>b__115_0(IMyEventOwner s) => 
                new Action<MyCubeSize, bool, MyPositionAndOrientation, long, bool>(MyCubeGrid.TryCreateGrid_Implementation);

            internal Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset> <PasteInternal>b__115_1(IMyEventOwner s) => 
                new Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(MyCubeGrid.TryPasteGrid_Implementation);

            internal bool <RemovePilots>b__156_0(MyObjectBuilder_EntityBase x) => 
                (x is MyObjectBuilder_Character);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GridCopy
        {
            private MyObjectBuilder_CubeGrid Grid;
            private Vector3 Offset;
            private Quaternion Rotation;
        }

        protected delegate void UpdateAfterPasteCallback(List<MyObjectBuilder_CubeGrid> pastedBuilders);
    }
}

