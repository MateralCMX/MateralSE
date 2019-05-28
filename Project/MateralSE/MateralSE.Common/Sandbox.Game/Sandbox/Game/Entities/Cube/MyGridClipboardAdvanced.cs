namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridClipboardAdvanced : MyGridClipboard
    {
        private static List<Vector3D> m_tmpCollisionPoints = new List<Vector3D>();
        protected bool m_dynamicBuildAllowed;

        public MyGridClipboardAdvanced(MyPlacementSettings settings, bool calculateVelocity = true) : base(settings, calculateVelocity)
        {
            base.m_useDynamicPreviews = false;
            base.m_dragDistance = 0f;
        }

        public override void Activate(Action callback = null)
        {
            base.Activate(callback);
            this.SetupDragDistance();
        }

        protected static bool CheckConnectivityOnGrid(MySlimBlock block, ref MatrixI transform, ref MyGridPlacementSettings settings, MyCubeGrid hitGrid)
        {
            Vector3I vectori;
            Quaternion quaternion;
            Vector3I.Transform(ref block.Position, ref transform, out vectori);
            new MyBlockOrientation(transform.GetDirection(block.Orientation.Forward), transform.GetDirection(block.Orientation.Up)).GetQuaternion(out quaternion);
            MyCubeBlockDefinition blockDefinition = block.BlockDefinition;
            return MyCubeGrid.CheckConnectivity(hitGrid, blockDefinition, blockDefinition.GetBuildProgressModelMountPoints(block.BuildLevelRatio), ref quaternion, ref vectori);
        }

        private static void ConvertGridBuilderToStatic(MyObjectBuilder_CubeGrid originalGrid, MatrixD worldMatrix)
        {
            originalGrid.IsStatic = true;
            originalGrid.PositionAndOrientation = new MyPositionAndOrientation((Vector3D) originalGrid.PositionAndOrientation.Value.Position, Vector3.Forward, Vector3.Up);
            Vector3 forward = (Vector3) worldMatrix.Forward;
            Base6Directions.Direction closestDirection = Base6Directions.GetClosestDirection(forward);
            Base6Directions.Direction up = Base6Directions.GetClosestDirection((Vector3) worldMatrix.Up);
            if (up == closestDirection)
            {
                up = Base6Directions.GetPerpendicular(closestDirection);
            }
            MatrixI transform = new MatrixI(Vector3I.Zero, closestDirection, up);
            foreach (MyObjectBuilder_CubeBlock block in originalGrid.CubeBlocks)
            {
                if (!(block is MyObjectBuilder_CompoundCubeBlock))
                {
                    ConvertRotatedGridBlockToStatic(ref transform, block);
                    continue;
                }
                MyObjectBuilder_CompoundCubeBlock origBlock = block as MyObjectBuilder_CompoundCubeBlock;
                ConvertRotatedGridCompoundBlockToStatic(ref transform, origBlock);
                for (int i = 0; i < origBlock.Blocks.Length; i++)
                {
                    MyObjectBuilder_CubeBlock block3 = origBlock.Blocks[i];
                    ConvertRotatedGridBlockToStatic(ref transform, block3);
                }
            }
        }

        private static void ConvertRotatedGridBlockToStatic(ref MatrixI transform, MyObjectBuilder_CubeBlock origBlock)
        {
            MyCubeBlockDefinition definition;
            MyDefinitionId defId = new MyDefinitionId(origBlock.TypeId, origBlock.SubtypeName);
            MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition);
            if (definition != null)
            {
                Vector3I vectori2;
                Vector3I vectori3;
                Vector3I vectori4;
                Quaternion quaternion;
                MyBlockOrientation blockOrientation = (MyBlockOrientation) origBlock.BlockOrientation;
                Vector3I min = (Vector3I) origBlock.Min;
                MySlimBlock.ComputeMax(definition, blockOrientation, ref min, out vectori2);
                Vector3I.Transform(ref min, ref transform, out vectori3);
                Vector3I.Transform(ref vectori2, ref transform, out vectori4);
                new MyBlockOrientation(transform.GetDirection(blockOrientation.Forward), transform.GetDirection(blockOrientation.Up)).GetQuaternion(out quaternion);
                origBlock.Orientation = quaternion;
                origBlock.Min = Vector3I.Min(vectori3, vectori4);
            }
        }

        private static void ConvertRotatedGridCompoundBlockToStatic(ref MatrixI transform, MyObjectBuilder_CompoundCubeBlock origBlock)
        {
            MyCubeBlockDefinition definition;
            MyDefinitionId defId = new MyDefinitionId(origBlock.TypeId, origBlock.SubtypeName);
            MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition);
            if (definition != null)
            {
                Vector3I vectori2;
                Vector3I vectori3;
                Vector3I vectori4;
                Vector3I min = (Vector3I) origBlock.Min;
                MySlimBlock.ComputeMax(definition, (MyBlockOrientation) origBlock.BlockOrientation, ref min, out vectori2);
                Vector3I.Transform(ref min, ref transform, out vectori3);
                Vector3I.Transform(ref vectori2, ref transform, out vectori4);
                origBlock.Min = Vector3I.Min(vectori3, vectori4);
            }
        }

        private static double DistanceFromCharacterPlane(ref Vector3D point) => 
            Vector3D.Dot(point - MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionDirection);

        internal void DynamicModeChanged()
        {
            if (MyCubeBuilder.Static.DynamicMode)
            {
                this.SetupDragDistance();
            }
        }

        protected Vector3D? GetFreeSpacePlacementPosition(bool copyPaste, out bool buildAllowed)
        {
            Vector3D? nullable = null;
            buildAllowed = false;
            double maxValue = double.MaxValue;
            double? currentRayIntersection = MyCubeBuilder.GetCurrentRayIntersection();
            if (currentRayIntersection != null)
            {
                maxValue = currentRayIntersection.Value;
            }
            Vector3D zero = Vector3D.Zero;
            if (copyPaste)
            {
                Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                zero = Vector3.TransformNormal(base.m_dragPointToPositionLocal, firstGridOrientationMatrix);
            }
            Vector3D vectord2 = base.PreviewGrids[0].GridIntegerToWorld(Vector3I.Zero);
            foreach (MySlimBlock local1 in base.PreviewGrids[0].GetBlocks())
            {
                Matrix matrix;
                Vector3 halfExtents = (Vector3) ((local1.BlockDefinition.Size * base.PreviewGrids[0].GridSize) * 0.5f);
                Vector3 vector2 = ((Vector3) (local1.Min * base.PreviewGrids[0].GridSize)) - (Vector3.Half * base.PreviewGrids[0].GridSize);
                Vector3 vector3 = (local1.Max * base.PreviewGrids[0].GridSize) + (Vector3.Half * base.PreviewGrids[0].GridSize);
                local1.Orientation.GetMatrix(out matrix);
                matrix.Translation = (Vector3) (0.5f * (vector2 + vector3));
                MatrixD xd = matrix * base.PreviewGrids[0].WorldMatrix;
                Vector3D vectord3 = (xd.Translation + zero) - vectord2;
                HkShape shape = (HkShape) new HkBoxShape(halfExtents);
                Vector3D point = MyBlockBuilderBase.IntersectionStart + vectord3;
                double num4 = DistanceFromCharacterPlane(ref point);
                point -= num4 * MyBlockBuilderBase.IntersectionDirection;
                Vector3D to = (MyBlockBuilderBase.IntersectionStart + ((base.m_dragDistance - num4) * MyBlockBuilderBase.IntersectionDirection)) + vectord3;
                MatrixD transform = xd;
                transform.Translation = point;
                try
                {
                    float? nullable3 = MyPhysics.CastShape(to, shape, ref transform, 30, 0f);
                    if (nullable3 == null)
                    {
                        continue;
                    }
                    if (nullable3.Value == 0f)
                    {
                        continue;
                    }
                    double num5 = DistanceFromCharacterPlane(ref point + (((double) nullable3.Value) * (to - point))) - num4;
                    if (num5 > 0.0)
                    {
                        if (num5 < maxValue)
                        {
                            maxValue = num5;
                        }
                        buildAllowed = true;
                        continue;
                    }
                    num5 = 0.0;
                    maxValue = 0.0;
                }
                finally
                {
                    shape.RemoveReference();
                    continue;
                }
                break;
            }
            float num3 = 1.5f * ((float) base.PreviewGrids[0].PositionComp.WorldAABB.HalfExtents.Length());
            if (maxValue < num3)
            {
                maxValue = num3;
                buildAllowed = false;
            }
            if (maxValue < base.m_dragDistance)
            {
                nullable = new Vector3D?(MyBlockBuilderBase.IntersectionStart + (maxValue * MyBlockBuilderBase.IntersectionDirection));
            }
            return nullable;
        }

        protected Vector3D? GetFreeSpacePlacementPositionGridAabbs(bool copyPaste, out bool buildAllowed)
        {
            Vector3D? nullable = null;
            buildAllowed = true;
            float gridSize = base.PreviewGrids[0].GridSize;
            double maxValue = double.MaxValue;
            double? currentRayIntersection = MyCubeBuilder.GetCurrentRayIntersection();
            if (currentRayIntersection != null)
            {
                maxValue = currentRayIntersection.Value;
            }
            Vector3D zero = Vector3D.Zero;
            if (copyPaste)
            {
                Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                zero = Vector3.TransformNormal(base.m_dragPointToPositionLocal, firstGridOrientationMatrix);
            }
            Vector3D vectord2 = base.PreviewGrids[0].GridIntegerToWorld(Vector3I.Zero);
            foreach (MyCubeGrid grid in base.PreviewGrids)
            {
                BoundingBox localAABB = grid.PositionComp.LocalAABB;
                Vector3 halfExtents = localAABB.HalfExtents;
                Vector3 vector2 = ((Vector3) (grid.Min * grid.GridSize)) - (Vector3.Half * grid.GridSize);
                Vector3 vector3 = (grid.Max * grid.GridSize) + (Vector3.Half * grid.GridSize);
                MatrixD identity = MatrixD.Identity;
                identity.Translation = (Vector3D) (0.5f * (vector2 + vector3));
                identity *= grid.WorldMatrix;
                Vector3.TransformNormal((Vector3) ((Vector3I.Abs(((Vector3I) (((grid.Max - grid.Min) + Vector3I.One) % 2)) - Vector3I.One) * 0.5) * grid.GridSize), grid.WorldMatrix);
                Vector3D vectord3 = (identity.Translation + zero) - vectord2;
                HkShape shape = (HkShape) new HkBoxShape(halfExtents);
                Vector3D point = MyBlockBuilderBase.IntersectionStart + vectord3;
                double num2 = DistanceFromCharacterPlane(ref point);
                point -= num2 * MyBlockBuilderBase.IntersectionDirection;
                Vector3D to = (MyBlockBuilderBase.IntersectionStart + ((base.m_dragDistance - num2) * MyBlockBuilderBase.IntersectionDirection)) + vectord3;
                MatrixD transform = identity;
                transform.Translation = point;
                try
                {
                    float? nullable3 = MyPhysics.CastShape(to, shape, ref transform, 30, 0f);
                    if ((nullable3 == null) || (nullable3.Value == 0f))
                    {
                        buildAllowed = false;
                        continue;
                    }
                    Vector3D vectord6 = point + (((double) nullable3.Value) * (to - point));
                    Color green = Color.Green;
                    BoundingBoxD localbox = new BoundingBoxD(-halfExtents, halfExtents);
                    localbox.Inflate((double) 0.029999999329447746);
                    MatrixD worldMatrix = transform;
                    worldMatrix.Translation = vectord6;
                    MyStringId? faceMaterial = null;
                    faceMaterial = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref green, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                    double num3 = DistanceFromCharacterPlane(ref vectord6) - num2;
                    if (num3 > 0.0)
                    {
                        if (num3 >= maxValue)
                        {
                            continue;
                        }
                        maxValue = num3;
                        continue;
                    }
                    num3 = 0.0;
                    maxValue = 0.0;
                    buildAllowed = false;
                }
                finally
                {
                    shape.RemoveReference();
                    continue;
                }
                break;
            }
            if ((maxValue == 0.0) || (maxValue >= base.m_dragDistance))
            {
                buildAllowed = false;
            }
            else
            {
                nullable = new Vector3D?(MyBlockBuilderBase.IntersectionStart + (maxValue * MyBlockBuilderBase.IntersectionDirection));
            }
            return nullable;
        }

        public override void MoveEntityCloser()
        {
            base.MoveEntityCloser();
            if (base.m_dragDistance < MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance)
            {
                base.m_dragDistance = MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance;
            }
        }

        public override bool PasteGrid(bool deactivate = true, bool showWarning = true)
        {
            int num1;
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
            if (!(base.m_hitEntity is MyCubeGrid) || ((MyCubeGrid) base.m_hitEntity).IsStatic)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !MyCubeBuilder.Static.DynamicMode;
            }
            bool flag2 = (bool) num1;
            return (!MyCubeBuilder.Static.DynamicMode ? (!flag2 ? (!MyFakes.ENABLE_VR_BUILDING ? this.PasteGridsInStaticMode(deactivate) : base.PasteGridInternal(deactivate, null, null, null, false, true)) : base.PasteGridInternal(deactivate, null, null, null, false, true)) : this.PasteGridsInDynamicMode(deactivate));
        }

        private bool PasteGridsInDynamicMode(bool deactivate)
        {
            List<bool> list = new List<bool>();
            foreach (MyObjectBuilder_CubeGrid grid in base.CopiedGrids)
            {
                list.Add(grid.IsStatic);
                grid.IsStatic = false;
            }
            bool flag = base.PasteGridInternal(deactivate, null, null, null, false, true);
            for (int i = 0; i < base.CopiedGrids.Count; i++)
            {
                base.CopiedGrids[i].IsStatic = list[i];
            }
            return flag;
        }

        private bool PasteGridsInStaticMode(bool deactivate)
        {
            MatrixD worldMatrix = base.PreviewGrids[0].WorldMatrix;
            ConvertGridBuilderToStatic(base.CopiedGrids[0], worldMatrix);
            base.PreviewGrids[0].WorldMatrix = MatrixD.CreateTranslation(worldMatrix.Translation);
            for (int i = 1; i < base.CopiedGrids.Count; i++)
            {
                if (base.CopiedGrids[i].IsStatic)
                {
                    MatrixD xd2 = base.PreviewGrids[i].WorldMatrix;
                    ConvertGridBuilderToStatic(base.CopiedGrids[i], xd2);
                    base.PreviewGrids[i].WorldMatrix = MatrixD.CreateTranslation(xd2.Translation);
                }
            }
            List<MyObjectBuilder_CubeGrid> pastedBuilders = new List<MyObjectBuilder_CubeGrid>();
            bool flag1 = base.PasteGridInternal(true, pastedBuilders, base.m_touchingGrids, delegate (List<MyObjectBuilder_CubeGrid> pastedBuildersInCallback) {
                this.UpdateAfterPaste(deactivate, pastedBuildersInCallback);
            }, false, true);
            if (flag1)
            {
                this.UpdateAfterPaste(deactivate, pastedBuilders);
            }
            return flag1;
        }

        public void SetDragDistance(float dragDistance)
        {
            base.m_dragDistance = dragDistance;
        }

        protected virtual void SetupDragDistance()
        {
            if (base.IsActive)
            {
                if (base.PreviewGrids.Count <= 0)
                {
                    base.m_dragDistance = 0f;
                }
                else
                {
                    double? currentRayIntersection = MyCubeBuilder.GetCurrentRayIntersection();
                    if ((currentRayIntersection != null) && (base.m_dragDistance > currentRayIntersection.Value))
                    {
                        base.m_dragDistance = (float) currentRayIntersection.Value;
                    }
                    float num2 = 2.5f * ((float) base.PreviewGrids[0].PositionComp.WorldAABB.HalfExtents.Length());
                    if (base.m_dragDistance < num2)
                    {
                        base.m_dragDistance = num2;
                    }
                }
            }
        }

        protected static bool TestBlockPlacement(MySlimBlock block, ref MyGridPlacementSettings settings) => 
            MyCubeGrid.TestPlacementAreaCube(block.CubeGrid, ref settings, block.Min, block.Max, block.Orientation, block.BlockDefinition, block.CubeGrid, false);

        protected static bool TestBlockPlacement(MySlimBlock block, ref MyGridPlacementSettings settings, out MyCubeGrid touchingGrid) => 
            MyCubeGrid.TestPlacementAreaCube(block.CubeGrid, ref settings, block.Min, block.Max, block.Orientation, block.BlockDefinition, out touchingGrid, block.CubeGrid, false);

        protected static bool TestBlockPlacementArea(MySlimBlock block, ref MyGridPlacementSettings settings, bool dynamicMode, bool testVoxel = true)
        {
            BoundingBoxD localAabb = BoundingBoxD.CreateInvalid();
            localAabb.Include((Vector3D) ((block.Min * block.CubeGrid.GridSize) - (block.CubeGrid.GridSize / 2f)));
            localAabb.Include((block.Max * block.CubeGrid.GridSize) + (block.CubeGrid.GridSize / 2f));
            return MyCubeGrid.TestBlockPlacementArea(block.BlockDefinition, new MyBlockOrientation?(block.Orientation), block.CubeGrid.WorldMatrix, ref settings, localAabb, dynamicMode, block.CubeGrid, testVoxel);
        }

        protected static bool TestBlockPlacementNoAABBInflate(MySlimBlock block, ref MyGridPlacementSettings settings, out MyCubeGrid touchingGrid) => 
            MyCubeGrid.TestPlacementAreaCubeNoAABBInflate(block.CubeGrid, ref settings, block.Min, block.Max, block.Orientation, block.BlockDefinition, out touchingGrid, block.CubeGrid);

        protected static bool TestBlockPlacementOnGrid(MySlimBlock block, ref MatrixI transform, ref MyGridPlacementSettings settings, MyCubeGrid hitGrid)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I.Transform(ref block.Min, ref transform, out vectori);
            Vector3I.Transform(ref block.Max, ref transform, out vectori2);
            Vector3I min = Vector3I.Min(vectori, vectori2);
            MyBlockOrientation orientation = new MyBlockOrientation(transform.GetDirection(block.Orientation.Forward), transform.GetDirection(block.Orientation.Up));
            int? ignoreMultiblockId = null;
            return hitGrid.CanPlaceBlock(min, Vector3I.Max(vectori, vectori2), orientation, block.BlockDefinition, ref settings, ignoreMultiblockId, false);
        }

        protected unsafe bool TestGridPlacementOnGrid(MyCubeGrid previewGrid, ref MyGridPlacementSettings settings, MyCubeGrid hitGrid)
        {
            bool flag2;
            HashSet<MySlimBlock>.Enumerator enumerator;
            int num2;
            bool flag = true;
            Vector3I gridOffset = hitGrid.WorldToGridInteger(previewGrid.PositionComp.WorldMatrix.Translation);
            MatrixI transform = hitGrid.CalculateMergeTransform(previewGrid, gridOffset);
            Matrix floatMatrix = transform.GetFloatMatrix();
            Matrix* matrixPtr1 = (Matrix*) ref floatMatrix;
            matrixPtr1.Translation *= previewGrid.GridSize;
            if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 60f), "First grid offset: " + gridOffset.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            if (!(flag && MyCubeBuilder.CheckValidBlocksRotation(floatMatrix, previewGrid)) || (hitGrid.GridSizeEnum != previewGrid.GridSizeEnum))
            {
                num2 = 0;
            }
            else
            {
                num2 = (int) hitGrid.CanMergeCubes(previewGrid, gridOffset);
            }
            flag = num2 && MyCubeGrid.CheckMergeConnectivity(hitGrid, previewGrid, gridOffset);
            if (!flag)
            {
                goto TR_0014;
            }
            else
            {
                flag2 = false;
                using (enumerator = previewGrid.CubeBlocks.GetEnumerator())
                {
                    do
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MySlimBlock current = enumerator.Current;
                                if (current.FatBlock is MyCompoundCubeBlock)
                                {
                                    foreach (MySlimBlock block2 in (current.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                    {
                                        flag2 |= CheckConnectivityOnGrid(block2, ref transform, ref settings, hitGrid);
                                        if (flag2)
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                }
                                flag2 |= CheckConnectivityOnGrid(current, ref transform, ref settings, hitGrid);
                            }
                            else
                            {
                                goto TR_0015;
                            }
                            break;
                        }
                    }
                    while (!flag2);
                }
            }
            goto TR_0015;
        TR_0014:
            if (flag)
            {
                using (enumerator = previewGrid.CubeBlocks.GetEnumerator())
                {
                    do
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                bool flag1;
                                int num1;
                                MySlimBlock current = enumerator.Current;
                                if (current.FatBlock is MyCompoundCubeBlock)
                                {
                                    using (List<MySlimBlock>.Enumerator enumerator2 = (current.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            flag = flag && flag1;
                                            if (!flag)
                                            {
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    num1 = 0;
                                }
                                else
                                {
                                    MySlimBlock block4;
                                    flag1 = TestBlockPlacementOnGrid(block4, ref transform, ref settings, hitGrid);
                                    num1 = (int) TestBlockPlacementOnGrid(current, ref transform, ref settings, hitGrid);
                                }
                                flag = (bool) num1;
                            }
                            else
                            {
                                return flag;
                            }
                            break;
                        }
                    }
                    while (flag);
                }
            }
            return flag;
        TR_0015:
            flag &= flag2;
            goto TR_0014;
        }

        private bool TestPlacement()
        {
            MyCubeGrid grid;
            BoundingBoxD xd;
            HashSet<MySlimBlock>.Enumerator enumerator;
            MyCubeGrid grid3;
            MyGridPlacementSettings settings5;
            bool flag4;
            bool flag10;
            bool flag11;
            bool flag = true;
            base.m_touchingGrids.Clear();
            int num = 0;
            goto TR_008F;
        TR_000C:
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
            if (flag)
            {
                num++;
                goto TR_008F;
            }
            return flag;
        TR_003D:
            settings5 = (num == 0) ? base.m_settings.GetGridPlacementSettings(grid.GridSizeEnum, false) : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallStaticGrid;
            bool flag3 = true;
            foreach (MySlimBlock local2 in grid.CubeBlocks)
            {
                Vector3 min = ((Vector3) (local2.Min * base.PreviewGrids[num].GridSize)) - (Vector3.Half * base.PreviewGrids[num].GridSize);
                Vector3 max = (local2.Max * base.PreviewGrids[num].GridSize) + (Vector3.Half * base.PreviewGrids[num].GridSize);
                BoundingBoxD localAabb = new BoundingBoxD(min, max);
                flag3 = flag3 && MyCubeGrid.TestPlacementArea(grid, false, ref settings5, localAabb, false, null, true, true);
                if (!flag3)
                {
                    break;
                }
            }
            flag &= !flag3;
            goto TR_000C;
        TR_003E:
            flag &= flag4;
            goto TR_000C;
        TR_0056:
            if (flag && (base.m_touchingGrids[num] != null))
            {
                MyGridPlacementSettings settings = (grid.GridSizeEnum == MyCubeSize.Large) ? MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.LargeStaticGrid : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallStaticGrid;
                flag = flag && this.TestGridPlacementOnGrid(grid, ref settings, base.m_touchingGrids[num]);
            }
            if (!flag)
            {
                goto TR_000C;
            }
            else if (num != 0)
            {
                goto TR_000C;
            }
            else if ((grid.GridSizeEnum == MyCubeSize.Small) && grid.IsStatic)
            {
                goto TR_003D;
            }
            else if (grid.IsStatic)
            {
                if (base.m_touchingGrids[num] != null)
                {
                    goto TR_000C;
                }
                else
                {
                    MyGridPlacementSettings gridPlacementSettings = base.m_settings.GetGridPlacementSettings(grid.GridSizeEnum, (num == 0) || grid.IsStatic);
                    MyCubeGrid touchingGrid = null;
                    flag4 = false;
                    using (enumerator = grid.CubeBlocks.GetEnumerator())
                    {
                        do
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    MySlimBlock current = enumerator.Current;
                                    if (current.FatBlock is MyCompoundCubeBlock)
                                    {
                                        foreach (MySlimBlock block6 in (current.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                        {
                                            flag4 |= TestBlockPlacementNoAABBInflate(block6, ref gridPlacementSettings, out touchingGrid);
                                            if (flag4)
                                            {
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    flag4 |= TestBlockPlacementNoAABBInflate(current, ref gridPlacementSettings, out touchingGrid);
                                }
                                else
                                {
                                    goto TR_003E;
                                }
                                break;
                            }
                        }
                        while (!flag4);
                    }
                }
            }
            else
            {
                goto TR_003D;
            }
            goto TR_003E;
        TR_0059:
            if (flag && (grid3 != null))
            {
                base.m_touchingGrids[num] = grid3;
            }
            goto TR_0056;
        TR_0086:
            grid3 = null;
            MyGridPlacementSettings settings3 = (num == 0) ? ((grid.GridSizeEnum == MyCubeSize.Large) ? MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.LargeStaticGrid : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallStaticGrid) : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(grid.GridSizeEnum);
            if (!grid.IsStatic)
            {
                foreach (MySlimBlock local1 in grid.CubeBlocks)
                {
                    int num3;
                    Vector3 min = ((Vector3) (local1.Min * base.PreviewGrids[num].GridSize)) - (Vector3.Half * base.PreviewGrids[num].GridSize);
                    Vector3 max = (local1.Max * base.PreviewGrids[num].GridSize) + (Vector3.Half * base.PreviewGrids[num].GridSize);
                    BoundingBoxD localAabb = new BoundingBoxD(min, max);
                    if (!flag)
                    {
                        num3 = 0;
                    }
                    else
                    {
                        flag11 = flag10;
                        num3 = (int) MyCubeGrid.TestPlacementArea(grid, grid.IsStatic, ref settings3, localAabb, false, null, true, true);
                    }
                    flag = (bool) num3;
                    if (!flag)
                    {
                        break;
                    }
                }
                base.m_touchingGrids[num] = null;
                goto TR_0056;
            }
            else
            {
                if (num == 0)
                {
                    Matrix orientation = (Matrix) grid.WorldMatrix.GetOrientation();
                    flag = flag && MyCubeBuilder.CheckValidBlocksRotation(orientation, grid);
                }
                using (enumerator = grid.CubeBlocks.GetEnumerator())
                {
                    do
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                bool flag8;
                                bool flag9;
                                int num2;
                                MySlimBlock current = enumerator.Current;
                                if (current.FatBlock is MyCompoundCubeBlock)
                                {
                                    foreach (MySlimBlock block4 in (current.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                    {
                                        MyCubeGrid grid4 = null;
                                        flag = flag && flag9;
                                        if (flag)
                                        {
                                            flag8 = TestBlockPlacementNoAABBInflate(block4, ref settings3, out grid4);
                                            if ((grid4 != null) && (grid3 == null))
                                            {
                                                grid3 = grid4;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                }
                                MyCubeGrid touchingGrid = null;
                                if (!flag)
                                {
                                    num2 = 0;
                                }
                                else
                                {
                                    flag9 = flag8;
                                    num2 = (int) flag11;
                                }
                                flag = (bool) num2;
                                if (flag)
                                {
                                    flag10 = TestBlockPlacementNoAABBInflate(current, ref settings3, out touchingGrid);
                                    if ((touchingGrid != null) && (grid3 == null))
                                    {
                                        grid3 = touchingGrid;
                                    }
                                }
                            }
                            else
                            {
                                goto TR_0059;
                            }
                            break;
                        }
                    }
                    while (flag);
                }
            }
            goto TR_0059;
        TR_008F:
            while (true)
            {
                if (num < base.PreviewGrids.Count)
                {
                    grid = base.PreviewGrids[num];
                    base.m_touchingGrids.Add(null);
                    if (!MyCubeBuilder.Static.DynamicMode)
                    {
                        if (num != 0)
                        {
                            goto TR_0086;
                        }
                        else if (!(base.m_hitEntity is MyCubeGrid))
                        {
                            goto TR_0086;
                        }
                        else if (base.IsSnapped && (base.SnapMode == SnapMode.Base6Directions))
                        {
                            MyGridPlacementSettings settings = (grid.GridSizeEnum == MyCubeSize.Large) ? MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.LargeStaticGrid : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallStaticGrid;
                            MyCubeGrid hitEntity = base.m_hitEntity as MyCubeGrid;
                            if ((hitEntity.GridSizeEnum == MyCubeSize.Small) && (grid.GridSizeEnum == MyCubeSize.Large))
                            {
                                flag = false;
                            }
                            else
                            {
                                MySlimBlock current;
                                bool flag7;
                                bool flag2 = (hitEntity.GridSizeEnum == MyCubeSize.Large) && (grid.GridSizeEnum == MyCubeSize.Small);
                                if (!(MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE & flag2))
                                {
                                    int num5;
                                    if (!flag)
                                    {
                                        num5 = 0;
                                    }
                                    else
                                    {
                                        flag7 = TestBlockPlacement(current, ref settings);
                                        num5 = (int) this.TestGridPlacementOnGrid(grid, ref settings, hitEntity);
                                    }
                                    flag = (bool) num5;
                                    break;
                                }
                                else if (hitEntity.IsStatic)
                                {
                                    using (enumerator = grid.CubeBlocks.GetEnumerator())
                                    {
                                        do
                                        {
                                            while (true)
                                            {
                                                if (enumerator.MoveNext())
                                                {
                                                    bool flag6;
                                                    int num1;
                                                    current = enumerator.Current;
                                                    if (current.FatBlock is MyCompoundCubeBlock)
                                                    {
                                                        using (List<MySlimBlock>.Enumerator enumerator2 = (current.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                                                        {
                                                            while (enumerator2.MoveNext())
                                                            {
                                                                flag = flag && flag6;
                                                                if (!flag)
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    if (!flag)
                                                    {
                                                        num1 = 0;
                                                    }
                                                    else
                                                    {
                                                        MySlimBlock block2;
                                                        flag6 = TestBlockPlacement(block2, ref settings);
                                                        num1 = (int) flag7;
                                                    }
                                                    flag = (bool) num1;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                                break;
                                            }
                                        }
                                        while (flag);
                                        break;
                                    }
                                }
                                else
                                {
                                    flag = false;
                                }
                            }
                        }
                        else
                        {
                            goto TR_0086;
                        }
                        return flag;
                    }
                    else if (!this.m_dynamicBuildAllowed)
                    {
                        bool flag5;
                        int num4;
                        MyGridPlacementSettings gridPlacementSettings = base.m_settings.GetGridPlacementSettings(grid.GridSizeEnum, false);
                        BoundingBoxD localAABB = grid.PositionComp.LocalAABB;
                        MatrixD worldMatrix = grid.WorldMatrix;
                        if (MyFakes.ENABLE_VOXEL_MAP_AABB_CORNER_TEST)
                        {
                            flag = flag && flag5;
                        }
                        if (!flag)
                        {
                            num4 = 0;
                        }
                        else
                        {
                            flag5 = MyCubeGrid.TestPlacementVoxelMapOverlap(null, ref gridPlacementSettings, ref localAABB, ref worldMatrix, false);
                            num4 = (int) MyCubeGrid.TestPlacementArea(grid, false, ref gridPlacementSettings, localAABB, true, null, true, true);
                        }
                        flag = (bool) num4;
                        if (!flag)
                        {
                            return flag;
                        }
                    }
                }
                else
                {
                    return flag;
                }
                break;
            }
            goto TR_000C;
        }

        protected static bool TestVoxelPlacement(MySlimBlock block, ref MyGridPlacementSettings settings, bool dynamicMode)
        {
            BoundingBoxD localAabb = BoundingBoxD.CreateInvalid();
            localAabb.Include((Vector3D) ((block.Min * block.CubeGrid.GridSize) - (block.CubeGrid.GridSize / 2f)));
            localAabb.Include((block.Max * block.CubeGrid.GridSize) + (block.CubeGrid.GridSize / 2f));
            return MyCubeGrid.TestVoxelPlacement(block.BlockDefinition, settings, dynamicMode, block.CubeGrid.WorldMatrix, localAabb);
        }

        public override void Update()
        {
            if (base.IsActive && base.m_visible)
            {
                bool flag = base.UpdateHitEntity(false);
                if (MyFakes.ENABLE_VR_BUILDING && !flag)
                {
                    base.Hide();
                }
                else if (!base.m_visible)
                {
                    base.Hide();
                }
                else
                {
                    base.Show();
                    if (base.m_dragDistance == 0f)
                    {
                        this.SetupDragDistance();
                    }
                    this.UpdatePastePosition();
                    this.UpdateGridTransformations();
                    if (MyCubeBuilder.Static.CubePlacementMode != MyCubeBuilder.CubePlacementModeEnum.FreePlacement)
                    {
                        base.FixSnapTransformationBase6();
                    }
                    if (base.m_calculateVelocity)
                    {
                        base.m_objectVelocity = (Vector3) ((base.m_pastePosition - base.m_pastePositionPrevious) / 0.01666666753590107);
                    }
                    base.m_canBePlaced = this.TestPlacement();
                    this.TestBuildingMaterials();
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

        private void UpdateAfterPaste(bool deactivate, List<MyObjectBuilder_CubeGrid> pastedBuilders)
        {
            if (base.CopiedGrids.Count == pastedBuilders.Count)
            {
                base.m_copiedGridOffsets.Clear();
                int num = 0;
                while (true)
                {
                    if (num >= base.CopiedGrids.Count)
                    {
                        base.m_pasteOrientationAngle = 0f;
                        base.m_pasteDirForward = (Vector3) Vector3I.Forward;
                        base.m_pasteDirUp = (Vector3) Vector3I.Up;
                        if (!deactivate)
                        {
                            this.Activate(null);
                        }
                        break;
                    }
                    base.CopiedGrids[num].PositionAndOrientation = pastedBuilders[num].PositionAndOrientation;
                    base.m_copiedGridOffsets.Add((Vector3) (base.CopiedGrids[num].PositionAndOrientation.Value.Position - base.CopiedGrids[0].PositionAndOrientation.Value.Position));
                    num++;
                }
            }
        }

        protected override void UpdatePastePosition()
        {
            base.m_pastePositionPrevious = base.m_pastePosition;
            if (MyCubeBuilder.Static.DynamicMode)
            {
                base.m_visible = true;
                base.IsSnapped = false;
                base.m_pastePosition = MyBlockBuilderBase.IntersectionStart + (base.m_dragDistance * MyBlockBuilderBase.IntersectionDirection);
                Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                Vector3D vectord = Vector3.TransformNormal(base.m_dragPointToPositionLocal, firstGridOrientationMatrix);
                base.m_pastePosition += vectord;
            }
            else
            {
                base.m_visible = true;
                if (!base.IsSnapped)
                {
                    base.m_pasteOrientationAngle = 0f;
                    base.m_pasteDirForward = (Vector3) Vector3I.Forward;
                    base.m_pasteDirUp = (Vector3) Vector3I.Up;
                }
                base.IsSnapped = true;
                MatrixD pasteMatrix = GetPasteMatrix();
                Vector3 vector = (Vector3) (pasteMatrix.Forward * base.m_dragDistance);
                MyGridPlacementSettings gridPlacementSettings = base.m_settings.GetGridPlacementSettings(base.PreviewGrids[0].GridSizeEnum);
                if (!base.TrySnapToSurface(gridPlacementSettings.SnapMode))
                {
                    base.m_pastePosition = pasteMatrix.Translation + vector;
                    Matrix firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
                    Vector3D vectord2 = Vector3.TransformNormal(base.m_dragPointToPositionLocal, firstGridOrientationMatrix);
                    base.m_pastePosition += vectord2;
                    base.IsSnapped = true;
                }
                if (!MyFakes.ENABLE_VR_BUILDING)
                {
                    double gridSize = base.PreviewGrids[0].GridSize;
                    if (base.m_settings.StaticGridAlignToCenter)
                    {
                        base.m_pastePosition = (Vector3D) (Vector3I.Round(base.m_pastePosition / gridSize) * gridSize);
                    }
                    else
                    {
                        base.m_pastePosition = (Vector3D) ((Vector3I.Round((base.m_pastePosition / gridSize) + 0.5) * gridSize) - (0.5 * gridSize));
                    }
                }
            }
        }

        private void UpdatePreview()
        {
            if ((base.PreviewGrids != null) && (base.m_visible && this.HasPreviewBBox))
            {
                MyStringId id = base.m_canBePlaced ? MyGridClipboard.ID_GIZMO_DRAW_LINE : MyGridClipboard.ID_GIZMO_DRAW_LINE_RED;
                if (!MyFakes.ENABLE_VR_BUILDING || !base.m_canBePlaced)
                {
                    Color white = Color.White;
                    foreach (MyCubeGrid local1 in base.PreviewGrids)
                    {
                        BoundingBoxD localAABB = local1.PositionComp.LocalAABB;
                        MatrixD worldMatrix = local1.PositionComp.WorldMatrix;
                        MyStringId? faceMaterial = null;
                        MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localAABB, ref white, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                    }
                }
            }
        }

        protected override bool AnyCopiedGridIsStatic =>
            false;
    }
}

