namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems.CoordinateSystem;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyMultiBlockClipboard : MyGridClipboardAdvanced
    {
        private static List<Vector3D> m_tmpCollisionPoints = new List<Vector3D>();
        private static List<VRage.Game.Entity.MyEntity> m_tmpNearEntities = new List<VRage.Game.Entity.MyEntity>();
        private MyMultiBlockDefinition m_multiBlockDefinition;
        public MySlimBlock RemoveBlock;
        public ushort? BlockIdInCompound;
        private Vector3I m_addPos;
        public HashSet<Tuple<MySlimBlock, ushort?>> RemoveBlocksInMultiBlock;
        private HashSet<Vector3I> m_tmpBlockPositionsSet;
        private bool m_lastVoxelState;

        public MyMultiBlockClipboard(MyPlacementSettings settings, bool calculateVelocity = true) : base(settings, calculateVelocity)
        {
            this.RemoveBlocksInMultiBlock = new HashSet<Tuple<MySlimBlock, ushort?>>();
            this.m_tmpBlockPositionsSet = new HashSet<Vector3I>();
            base.m_useDynamicPreviews = false;
        }

        protected override void ChangeClipboardPreview(bool visible, List<MyCubeGrid> previewGrids, List<MyObjectBuilder_CubeGrid> copiedGrids)
        {
            base.ChangeClipboardPreview(visible, previewGrids, copiedGrids);
            if (visible && MySession.Static.SurvivalMode)
            {
                using (List<MyCubeGrid>.Enumerator enumerator = base.PreviewGrids.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MySlimBlock block in enumerator.Current.GetBlocks())
                        {
                            MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                            if (fatBlock != null)
                            {
                                using (List<MySlimBlock>.Enumerator enumerator3 = fatBlock.GetBlocks().GetEnumerator())
                                {
                                    while (enumerator3.MoveNext())
                                    {
                                        SetBlockToFullIntegrity(enumerator3.Current);
                                    }
                                    continue;
                                }
                            }
                            SetBlockToFullIntegrity(block);
                        }
                    }
                }
            }
        }

        public override void Deactivate(bool afterPaste = false)
        {
            this.m_multiBlockDefinition = null;
            base.Deactivate(afterPaste);
        }

        private MyCubeGrid DetectTouchingGrid()
        {
            if ((base.PreviewGrids != null) && (base.PreviewGrids.Count != 0))
            {
                using (HashSet<MySlimBlock>.Enumerator enumerator = base.PreviewGrids[0].CubeBlocks.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MySlimBlock current = enumerator.Current;
                        MyCubeGrid grid = this.DetectTouchingGrid(current);
                        if (grid != null)
                        {
                            return grid;
                        }
                    }
                }
            }
            return null;
        }

        private MyCubeGrid DetectTouchingGrid(MySlimBlock block)
        {
            MyCubeGrid grid2;
            if (MyCubeBuilder.Static.DynamicMode)
            {
                return null;
            }
            if (block == null)
            {
                return null;
            }
            if (block.FatBlock is MyCompoundCubeBlock)
            {
                using (List<MySlimBlock>.Enumerator enumerator = (block.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            MySlimBlock current = enumerator.Current;
                            MyCubeGrid grid = this.DetectTouchingGrid(current);
                            if (grid == null)
                            {
                                continue;
                            }
                            grid2 = grid;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    }
                    return grid2;
                }
            }
            else
            {
                BoundingBoxD xd;
                block.GetWorldBoundingBox(out xd, false);
                xd.Inflate((double) (block.CubeGrid.GridSize / 2f));
                m_tmpNearEntities.Clear();
                Sandbox.Game.Entities.MyEntities.GetElementsInBox(ref xd, m_tmpNearEntities);
                MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = block.BlockDefinition.GetBuildProgressModelMountPoints(block.BuildLevelRatio);
                try
                {
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= m_tmpNearEntities.Count)
                        {
                            break;
                        }
                        MyCubeGrid objA = m_tmpNearEntities[num2] as MyCubeGrid;
                        if (((objA != null) && (!ReferenceEquals(objA, block.CubeGrid) && ((objA.Physics != null) && (objA.Physics.Enabled && objA.IsStatic)))) && (objA.GridSizeEnum == block.CubeGrid.GridSizeEnum))
                        {
                            Vector3I gridOffset = objA.WorldToGridInteger(base.m_pastePosition);
                            if (objA.CanMergeCubes(block.CubeGrid, gridOffset))
                            {
                                Quaternion quaternion;
                                MatrixI transformation = objA.CalculateMergeTransform(block.CubeGrid, gridOffset);
                                new MyBlockOrientation(transformation.GetDirection(block.Orientation.Forward), transformation.GetDirection(block.Orientation.Up)).GetQuaternion(out quaternion);
                                Vector3I position = Vector3I.Transform(block.Position, transformation);
                                if (MyCubeGrid.CheckConnectivity(objA, block.BlockDefinition, buildProgressModelMountPoints, ref quaternion, ref position))
                                {
                                    return objA;
                                }
                            }
                        }
                        num2++;
                    }
                }
                finally
                {
                    m_tmpNearEntities.Clear();
                }
                return null;
            }
            return grid2;
        }

        public override bool EntityCanPaste(VRage.Game.Entity.MyEntity pastingEntity)
        {
            if (base.CopiedGrids.Count < 1)
            {
                return false;
            }
            if (MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                return true;
            }
            MyCubeBuilder.BuildComponent.GetMultiBlockPlacementMaterials(this.m_multiBlockDefinition);
            return MyCubeBuilder.BuildComponent.HasBuildingMaterials(pastingEntity, false);
        }

        private void FixSnapTransformationBase6()
        {
            if (base.CopiedGrids.Count != 0)
            {
                MyCubeGrid hitEntity = base.m_hitEntity as MyCubeGrid;
                if (hitEntity != null)
                {
                    Matrix rotationDeltaMatrixToHitGrid = this.GetRotationDeltaMatrixToHitGrid(hitEntity);
                    foreach (MyCubeGrid local1 in base.PreviewGrids)
                    {
                        MatrixD worldMatrix = local1.WorldMatrix;
                        Matrix matrix2 = (Matrix) (worldMatrix.GetOrientation() * rotationDeltaMatrixToHitGrid);
                        MatrixD xd = MatrixD.CreateWorld(base.m_pastePosition, matrix2.Forward, matrix2.Up);
                        local1.PositionComp.SetWorldMatrix(xd, null, false, true, true, false, false, false);
                    }
                    if ((hitEntity.GridSizeEnum == MyCubeSize.Large) && (base.PreviewGrids[0].GridSizeEnum == MyCubeSize.Small))
                    {
                        base.m_pastePosition = hitEntity.GridIntegerToWorld(MyCubeBuilder.TransformLargeGridHitCoordToSmallGrid(base.m_hitPos, hitEntity.PositionComp.WorldMatrixNormalizedInv, hitEntity.GridSize));
                    }
                    else
                    {
                        Vector3I vectori = Vector3I.Round(base.m_hitNormal);
                        Vector3I gridOffset = hitEntity.WorldToGridInteger(base.m_pastePosition);
                        Vector3I min = base.PreviewGrids[0].Min;
                        Vector3I vectori4 = Vector3I.Abs(Vector3I.Round(Vector3D.TransformNormal(Vector3D.TransformNormal((Vector3D) ((base.PreviewGrids[0].Max - min) + Vector3I.One), base.PreviewGrids[0].WorldMatrix), hitEntity.PositionComp.WorldMatrixNormalizedInv)));
                        int num = Math.Abs(Vector3I.Dot(ref vectori, ref vectori4));
                        int num2 = 0;
                        while (true)
                        {
                            if ((num2 >= num) || hitEntity.CanMergeCubes(base.PreviewGrids[0], gridOffset))
                            {
                                if (num2 == num)
                                {
                                    gridOffset = hitEntity.WorldToGridInteger(base.m_pastePosition);
                                }
                                base.m_pastePosition = hitEntity.GridIntegerToWorld(gridOffset);
                                break;
                            }
                            gridOffset = (Vector3I) (gridOffset + vectori);
                            num2++;
                        }
                    }
                    for (int i = 0; i < base.PreviewGrids.Count; i++)
                    {
                        MyCubeGrid local2 = base.PreviewGrids[i];
                        MatrixD worldMatrix = local2.WorldMatrix;
                        worldMatrix.Translation = base.m_pastePosition + Vector3.Transform(base.m_copiedGridOffsets[i], rotationDeltaMatrixToHitGrid);
                        local2.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                    }
                    if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                    {
                        MyRenderProxy.DebugDrawLine3D(base.m_hitPos, base.m_hitPos + base.m_hitNormal, Color.Red, Color.Green, false, false);
                    }
                }
            }
        }

        public Matrix GetRotationDeltaMatrixToHitGrid(MyCubeGrid hitGrid)
        {
            Matrix orientation = (Matrix) hitGrid.WorldMatrix.GetOrientation();
            Matrix toAlign = (Matrix) base.PreviewGrids[0].WorldMatrix.GetOrientation();
            Matrix matrix3 = Matrix.AlignRotationToAxes(ref toAlign, ref orientation);
            return (Matrix.Invert(toAlign) * matrix3);
        }

        public override void MoveEntityCloser()
        {
            if (MyCubeBuilder.Static.DynamicMode)
            {
                base.MoveEntityCloser();
                if (base.m_dragDistance < MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance)
                {
                    base.m_dragDistance = MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance;
                }
            }
        }

        public override void MoveEntityFurther()
        {
            if (MyCubeBuilder.Static.DynamicMode)
            {
                base.MoveEntityFurther();
                if (base.m_dragDistance > MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance)
                {
                    base.m_dragDistance = MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance;
                }
            }
        }

        public override bool PasteGrid(bool deactivate = true, bool showWarning = true)
        {
            if ((base.CopiedGrids.Count > 0) && !base.IsActive)
            {
                this.Activate(null);
                return true;
            }
            if (!base.m_canBePlaced)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                return false;
            }
            if (base.PreviewGrids.Count == 0)
            {
                return false;
            }
            bool flag2 = (this.RemoveBlock != null) && !this.RemoveBlock.CubeGrid.IsStatic;
            return (!(MyCubeBuilder.Static.DynamicMode | flag2) ? this.PasteGridsInStaticMode(deactivate) : this.PasteGridsInDynamicMode(deactivate));
        }

        private bool PasteGridsInDynamicMode(bool deactivate)
        {
            List<bool> list = new List<bool>();
            foreach (MyObjectBuilder_CubeGrid grid in base.CopiedGrids)
            {
                list.Add(grid.IsStatic);
                grid.IsStatic = false;
                base.BeforeCreateGrid(grid);
            }
            bool flag = base.PasteGridInternal(deactivate, null, null, null, true, true);
            for (int i = 0; i < base.CopiedGrids.Count; i++)
            {
                base.CopiedGrids[i].IsStatic = list[i];
            }
            return flag;
        }

        private bool PasteGridsInStaticMode(bool deactivate)
        {
            List<MyObjectBuilder_CubeGrid> itemsToAdd = new List<MyObjectBuilder_CubeGrid>();
            List<MatrixD> list2 = new List<MatrixD>();
            MyObjectBuilder_CubeGrid grid = base.CopiedGrids[0];
            base.BeforeCreateGrid(grid);
            itemsToAdd.Add(grid);
            MatrixD worldMatrix = base.PreviewGrids[0].WorldMatrix;
            MyObjectBuilder_CubeGrid grid2 = MyCubeBuilder.ConvertGridBuilderToStatic(grid, worldMatrix);
            base.CopiedGrids[0] = grid2;
            list2.Add(worldMatrix);
            for (int i = 1; i < base.CopiedGrids.Count; i++)
            {
                MyObjectBuilder_CubeGrid grid3 = base.CopiedGrids[i];
                base.BeforeCreateGrid(grid3);
                itemsToAdd.Add(grid3);
                MatrixD item = base.PreviewGrids[i].WorldMatrix;
                list2.Add(item);
                if (base.CopiedGrids[i].IsStatic)
                {
                    base.CopiedGrids[i] = MyCubeBuilder.ConvertGridBuilderToStatic(grid3, item);
                }
            }
            bool flag = base.PasteGridInternal(deactivate, null, base.m_touchingGrids, null, true, true);
            base.CopiedGrids.Clear();
            base.CopiedGrids.AddList<MyObjectBuilder_CubeGrid>(itemsToAdd);
            for (int j = 0; j < base.PreviewGrids.Count; j++)
            {
                base.PreviewGrids[j].WorldMatrix = list2[j];
            }
            return flag;
        }

        private static void SetBlockToFullIntegrity(MySlimBlock block)
        {
            float buildRatio = block.ComponentStack.BuildRatio;
            block.ComponentStack.SetIntegrity(block.ComponentStack.MaxIntegrity, block.ComponentStack.MaxIntegrity);
            if (block.BlockDefinition.ModelChangeIsNeeded(buildRatio, block.ComponentStack.BuildRatio))
            {
                block.UpdateVisual(true);
            }
        }

        public void SetGridFromBuilder(MyMultiBlockDefinition multiBlockDefinition, MyObjectBuilder_CubeGrid grid, Vector3 dragPointDelta, float dragVectorLength)
        {
            this.ChangeClipboardPreview(false, base.m_previewGrids, base.m_copiedGrids);
            this.m_multiBlockDefinition = multiBlockDefinition;
            base.SetGridFromBuilder(grid, dragPointDelta, dragVectorLength);
            this.ChangeClipboardPreview(true, base.m_previewGrids, base.m_copiedGrids);
        }

        protected override void SetupDragDistance()
        {
            if (base.IsActive)
            {
                base.SetupDragDistance();
                if (MySession.Static.SurvivalMode && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    base.m_dragDistance = MyBlockBuilderBase.IntersectionDistance;
                }
            }
        }

        public static void TakeMaterialsFromBuilder(List<MyObjectBuilder_CubeGrid> blocksToBuild, VRage.Game.Entity.MyEntity builder)
        {
            if (blocksToBuild.Count != 0)
            {
                MyObjectBuilder_CubeBlock block = blocksToBuild[0].CubeBlocks.FirstOrDefault<MyObjectBuilder_CubeBlock>();
                if (block != null)
                {
                    MyDefinitionId id;
                    MyObjectBuilder_CompoundCubeBlock block2 = block as MyObjectBuilder_CompoundCubeBlock;
                    if (block2 == null)
                    {
                        if (block.MultiBlockDefinition == null)
                        {
                            return;
                        }
                        id = block.MultiBlockDefinition.Value;
                    }
                    else if (block2.Blocks == null)
                    {
                        return;
                    }
                    else if ((block2.Blocks.Length != 0) && (block2.Blocks[0].MultiBlockDefinition != null))
                    {
                        id = block2.Blocks[0].MultiBlockDefinition.Value;
                    }
                    else
                    {
                        return;
                    }
                    MyMultiBlockDefinition multiBlockDefinition = MyDefinitionManager.Static.TryGetMultiBlockDefinition(id);
                    if (multiBlockDefinition != null)
                    {
                        MyCubeBuilder.BuildComponent.GetMultiBlockPlacementMaterials(multiBlockDefinition);
                        MyCubeBuilder.BuildComponent.AfterSuccessfulBuild(builder, false);
                    }
                }
            }
        }

        private bool TestPlacement()
        {
            MyCubeGrid grid;
            MyGridPlacementSettings gridPlacementSettings;
            BoundingBoxD xd;
            bool flag3;
            MyGridPlacementSettings settings4;
            bool flag1;
            bool flag = true;
            base.m_touchingGrids.Clear();
            int num = 0;
            goto TR_0047;
        TR_000F:
            xd = grid.PositionComp.LocalAABB;
            MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
            if (MySector.MainCamera != null)
            {
                Vector3D point = Vector3D.Transform(MySector.MainCamera.Position, worldMatrixNormalizedInv);
                flag = flag && (xd.Contains(point) != ContainmentType.Contains);
            }
            if (flag)
            {
                m_tmpCollisionPoints.Clear();
                MyCubeBuilder.PrepareCharacterCollisionPoints(m_tmpCollisionPoints);
                using (List<Vector3D>.Enumerator enumerator3 = m_tmpCollisionPoints.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        Vector3D point = Vector3D.Transform(enumerator3.Current, worldMatrixNormalizedInv);
                        flag = flag && (xd.Contains(point) != ContainmentType.Contains);
                        if (!flag)
                        {
                            break;
                        }
                    }
                }
            }
            num++;
            goto TR_0047;
        TR_001D:
            flag &= flag3;
            base.m_touchingGrids[num] = this.DetectTouchingGrid();
            goto TR_000F;
        TR_0033:
            settings4 = base.m_settings.GetGridPlacementSettings(grid.GridSizeEnum, grid.IsStatic && !MyCubeBuilder.Static.DynamicMode);
            flag = flag && MyCubeGrid.TestPlacementArea(grid, grid.IsStatic, ref settings4, grid.PositionComp.LocalAABB, false, null, true, true);
            goto TR_000F;
        TR_0047:
            while (true)
            {
                if (num >= base.PreviewGrids.Count)
                {
                    return flag;
                }
                grid = base.PreviewGrids[num];
                if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(grid.PositionComp.GetPosition()))
                {
                    return false;
                }
                gridPlacementSettings = base.m_settings.GetGridPlacementSettings(grid.GridSizeEnum);
                base.m_touchingGrids.Add(null);
                if ((MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    if ((num == 0) && MyBlockBuilderBase.CameraControllerSpectator)
                    {
                        base.m_visible = false;
                        return false;
                    }
                    if (((num == 0) && !MyCubeBuilder.Static.DynamicMode) && !MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref grid.PositionComp.WorldMatrixNormalizedInv, grid.PositionComp.LocalAABB, grid.GridSize, MyBlockBuilderBase.IntersectionDistance))
                    {
                        base.m_visible = false;
                        return false;
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                if (!MyCubeBuilder.Static.DynamicMode)
                {
                    if (num != 0)
                    {
                        break;
                    }
                    if (!(base.m_hitEntity is MyCubeGrid))
                    {
                        break;
                    }
                    if (!base.IsSnapped)
                    {
                        break;
                    }
                    MyCubeGrid hitEntity = base.m_hitEntity as MyCubeGrid;
                    MyGridPlacementSettings gridPlacementSettings = base.m_settings.GetGridPlacementSettings(hitEntity.GridSizeEnum, hitEntity.IsStatic);
                    flag = ((hitEntity.GridSizeEnum != MyCubeSize.Large) || (grid.GridSizeEnum != MyCubeSize.Small)) ? (flag && base.TestGridPlacementOnGrid(grid, ref gridPlacementSettings, hitEntity)) : (flag && flag1);
                    base.m_touchingGrids.Clear();
                    base.m_touchingGrids.Add(hitEntity);
                }
                else
                {
                    MyGridPlacementSettings settings2 = (grid.GridSizeEnum == MyCubeSize.Large) ? base.m_settings.LargeGrid : base.m_settings.SmallGrid;
                    bool flag2 = false;
                    foreach (MySlimBlock block in grid.GetBlocks())
                    {
                        Vector3 min = ((Vector3) (block.Min * base.PreviewGrids[num].GridSize)) - (Vector3.Half * base.PreviewGrids[num].GridSize);
                        Vector3 max = (block.Max * base.PreviewGrids[num].GridSize) + (Vector3.Half * base.PreviewGrids[num].GridSize);
                        BoundingBoxD localAabb = new BoundingBoxD(min, max);
                        if (!flag2)
                        {
                            flag2 = TestVoxelPlacement(block, ref gridPlacementSettings, true);
                        }
                        flag = flag && MyCubeGrid.TestPlacementArea(grid, grid.IsStatic, ref settings2, localAabb, true, null, false, true);
                        if (!flag)
                        {
                            break;
                        }
                    }
                    flag &= flag2;
                }
                goto TR_000F;
            }
            if (num != 0)
            {
                goto TR_0033;
            }
            else if (!(base.m_hitEntity is MyVoxelMap))
            {
                goto TR_0033;
            }
            else
            {
                flag3 = false;
                using (HashSet<MySlimBlock>.Enumerator enumerator = grid.CubeBlocks.GetEnumerator())
                {
                    do
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MySlimBlock block3;
                                bool flag4;
                                MySlimBlock current = enumerator.Current;
                                if (current.FatBlock is MyCompoundCubeBlock)
                                {
                                    using (List<MySlimBlock>.Enumerator enumerator2 = (current.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            if (!flag3)
                                            {
                                                flag1 = MyCubeGrid.TestPlacementArea(grid, ref gridPlacementSettings, grid.PositionComp.LocalAABB, false, null);
                                                flag3 = TestVoxelPlacement(block3, ref gridPlacementSettings, false);
                                            }
                                            flag = flag && flag4;
                                            if (!flag)
                                            {
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (!flag3)
                                {
                                    flag4 = TestBlockPlacementArea(block3, ref gridPlacementSettings, false, false);
                                    flag3 = TestVoxelPlacement(current, ref gridPlacementSettings, false);
                                }
                                flag = flag && TestBlockPlacementArea(current, ref gridPlacementSettings, false, false);
                            }
                            else
                            {
                                goto TR_001D;
                            }
                            break;
                        }
                    }
                    while (flag);
                }
            }
            goto TR_001D;
        }

        public override void Update()
        {
            if (base.IsActive)
            {
                this.UpdateHitEntity();
                if (!base.m_visible)
                {
                    base.ShowPreview(false);
                }
                else if (base.PreviewGrids.Count != 0)
                {
                    if (base.m_dragDistance == 0f)
                    {
                        this.SetupDragDistance();
                    }
                    if (base.m_dragDistance > MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance)
                    {
                        base.m_dragDistance = MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance;
                    }
                    this.UpdatePastePosition();
                    this.UpdateGridTransformations();
                    this.FixSnapTransformationBase6();
                    if (base.m_calculateVelocity)
                    {
                        base.m_objectVelocity = (Vector3) ((base.m_pastePosition - base.m_pastePositionPrevious) / 0.01666666753590107);
                    }
                    base.m_canBePlaced = this.TestPlacement();
                    if (!base.m_visible)
                    {
                        base.ShowPreview(false);
                    }
                    else
                    {
                        base.ShowPreview(true);
                        this.TestBuildingMaterials();
                        base.m_canBePlaced &= base.CharacterHasEnoughMaterials;
                        this.UpdatePreview();
                        if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                        {
                            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "FW: " + base.m_pasteDirForward.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 20f), "UP: " + base.m_pasteDirUp.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 40f), "AN: " + base.m_pasteOrientationAngle.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        }
                    }
                }
            }
        }

        private void UpdateHitEntity()
        {
            base.m_closestHitDistSq = float.MaxValue;
            base.m_hitPos = new Vector3(0f, 0f, 0f);
            base.m_hitNormal = new Vector3(1f, 0f, 0f);
            base.m_hitEntity = null;
            this.m_addPos = Vector3I.Zero;
            this.RemoveBlock = null;
            this.BlockIdInCompound = null;
            this.RemoveBlocksInMultiBlock.Clear();
            base.m_dynamicBuildAllowed = false;
            base.m_visible = false;
            base.m_canBePlaced = false;
            if (MyCubeBuilder.Static.DynamicMode)
            {
                base.m_visible = true;
            }
            else
            {
                MatrixD pasteMatrix = GetPasteMatrix();
                if ((MyCubeBuilder.Static.CurrentGrid == null) && (MyCubeBuilder.Static.CurrentVoxelBase == null))
                {
                    MyCubeBuilder.Static.ChooseHitObject();
                }
                if (MyCubeBuilder.Static.HitInfo == null)
                {
                    base.m_visible = false;
                }
                else
                {
                    Vector3? nullable;
                    Vector3I vectori;
                    Vector3I vectori2;
                    int num1;
                    float cubeSize = MyDefinitionManager.Static.GetCubeSize(base.CopiedGrids[0].GridSizeEnum);
                    MyCubeGrid hitEntity = MyCubeBuilder.Static.HitInfo.Value.HkHitInfo.GetHitEntity() as MyCubeGrid;
                    if (((hitEntity == null) || (!hitEntity.IsStatic || (hitEntity.GridSizeEnum != MyCubeSize.Large))) || (base.CopiedGrids[0].GridSizeEnum != MyCubeSize.Small))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE;
                    }
                    if (!MyCubeBuilder.Static.GetAddAndRemovePositions(cubeSize, (bool) num1, out this.m_addPos, out nullable, out vectori, out vectori2, out this.RemoveBlock, out this.BlockIdInCompound, this.RemoveBlocksInMultiBlock))
                    {
                        base.m_visible = false;
                    }
                    else if (this.RemoveBlock != null)
                    {
                        base.m_hitPos = MyCubeBuilder.Static.HitInfo.Value.Position;
                        base.m_closestHitDistSq = (float) (base.m_hitPos - pasteMatrix.Translation).LengthSquared();
                        base.m_hitNormal = (Vector3) vectori;
                        base.m_hitEntity = this.RemoveBlock.CubeGrid;
                        if ((((double) MyDefinitionManager.Static.GetCubeSize(this.RemoveBlock.CubeGrid.GridSizeEnum)) / ((double) MyDefinitionManager.Static.GetCubeSize(base.CopiedGrids[0].GridSizeEnum))) < 1.0)
                        {
                            this.RemoveBlock = null;
                        }
                        base.m_visible = this.RemoveBlock != null;
                    }
                    else if (!MyFakes.ENABLE_BLOCK_PLACEMENT_ON_VOXEL || !(MyCubeBuilder.Static.HitInfo.Value.HkHitInfo.GetHitEntity() is MyVoxelBase))
                    {
                        base.m_visible = false;
                    }
                    else
                    {
                        base.m_hitPos = MyCubeBuilder.Static.HitInfo.Value.Position;
                        base.m_closestHitDistSq = (float) (base.m_hitPos - pasteMatrix.Translation).LengthSquared();
                        base.m_hitNormal = (Vector3) vectori;
                        base.m_hitEntity = MyCubeBuilder.Static.HitInfo.Value.HkHitInfo.GetHitEntity() as MyVoxelBase;
                        base.m_visible = true;
                    }
                }
            }
        }

        protected override void UpdatePastePosition()
        {
            base.m_pastePositionPrevious = base.m_pastePosition;
            if (MyCubeBuilder.Static.HitInfo != null)
            {
                base.m_pastePosition = MyCubeBuilder.Static.HitInfo.Value.Position;
            }
            else
            {
                base.m_pastePosition = MyCubeBuilder.Static.FreePlacementTarget;
            }
            double cubeSize = MyDefinitionManager.Static.GetCubeSize(base.CopiedGrids[0].GridSizeEnum);
            long? id = null;
            MyCoordinateSystem.CoordSystemData data = MyCoordinateSystem.Static.SnapWorldPosToClosestGrid(ref base.m_pastePosition, cubeSize, base.m_settings.StaticGridAlignToCenter, id);
            base.EnableStationRotation = MyCubeBuilder.Static.DynamicMode;
            if (MyCubeBuilder.Static.DynamicMode)
            {
                base.AlignClipboardToGravity();
                base.m_visible = true;
                base.IsSnapped = false;
                this.m_lastVoxelState = false;
            }
            else if (this.RemoveBlock != null)
            {
                base.m_pastePosition = Vector3D.Transform((Vector3) (this.m_addPos * this.RemoveBlock.CubeGrid.GridSize), this.RemoveBlock.CubeGrid.WorldMatrix);
                if (!base.IsSnapped && this.RemoveBlock.CubeGrid.IsStatic)
                {
                    base.m_pasteOrientationAngle = 0f;
                    base.m_pasteDirForward = (Vector3) this.RemoveBlock.CubeGrid.WorldMatrix.Forward;
                    base.m_pasteDirUp = (Vector3) this.RemoveBlock.CubeGrid.WorldMatrix.Up;
                }
                base.IsSnapped = true;
                this.m_lastVoxelState = false;
            }
            else if (MyFakes.ENABLE_BLOCK_PLACEMENT_ON_VOXEL && (base.m_hitEntity is MyVoxelBase))
            {
                if (MyCoordinateSystem.Static.LocalCoordExist)
                {
                    base.m_pastePosition = data.SnappedTransform.Position;
                    if (!this.m_lastVoxelState)
                    {
                        base.AlignRotationToCoordSys();
                    }
                }
                base.IsSnapped = true;
                this.m_lastVoxelState = true;
            }
        }

        private void UpdatePreview()
        {
            if ((base.PreviewGrids != null) && (base.m_visible && this.HasPreviewBBox))
            {
                Vector4? nullable2;
                MyStringId id = base.m_canBePlaced ? MyGridClipboard.ID_GIZMO_DRAW_LINE : MyGridClipboard.ID_GIZMO_DRAW_LINE_RED;
                Color white = Color.White;
                foreach (MyCubeGrid local1 in base.PreviewGrids)
                {
                    BoundingBoxD localAABB = local1.PositionComp.LocalAABB;
                    MatrixD worldMatrix = local1.PositionComp.WorldMatrix;
                    MyStringId? faceMaterial = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localAABB, ref white, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                }
                Vector4 color = new Vector4(Color.Red.ToVector3() * 0.8f, 1f);
                if (this.RemoveBlocksInMultiBlock.Count <= 0)
                {
                    if (this.RemoveBlock != null)
                    {
                        nullable2 = null;
                        MyCubeBuilder.DrawSemiTransparentBox(this.RemoveBlock.CubeGrid, this.RemoveBlock, color, false, new MyStringId?(MyGridClipboard.ID_GIZMO_DRAW_LINE_RED), nullable2);
                    }
                }
                else
                {
                    this.m_tmpBlockPositionsSet.Clear();
                    MyCubeBuilder.GetAllBlocksPositions(this.RemoveBlocksInMultiBlock, this.m_tmpBlockPositionsSet);
                    foreach (Vector3I local2 in this.m_tmpBlockPositionsSet)
                    {
                        nullable2 = null;
                        MyCubeBuilder.DrawSemiTransparentBox(local2, local2, this.RemoveBlock.CubeGrid, color, false, new MyStringId?(MyGridClipboard.ID_GIZMO_DRAW_LINE_RED), nullable2);
                    }
                    this.m_tmpBlockPositionsSet.Clear();
                }
            }
        }

        protected override bool AnyCopiedGridIsStatic =>
            false;
    }
}

