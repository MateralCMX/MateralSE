namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentCubeGrid : MyDebugRenderComponent
    {
        private MyCubeGrid m_cubeGrid;
        private Dictionary<Vector3I, MyTimeSpan> m_dirtyBlocks;
        private List<Vector3I> m_tmpRemoveList;
        private List<HkBodyCollision> m_penetrations;

        public MyDebugRenderComponentCubeGrid(MyCubeGrid cubeGrid) : base(cubeGrid)
        {
            this.m_dirtyBlocks = new Dictionary<Vector3I, MyTimeSpan>();
            this.m_tmpRemoveList = new List<Vector3I>();
            this.m_penetrations = new List<HkBodyCollision>();
            this.m_cubeGrid = cubeGrid;
        }

        public override void DebugDraw()
        {
            MatrixD worldMatrix;
            Vector3D vectord4;
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_AABB)
            {
                MatrixD worldMatrix = this.m_cubeGrid.PositionComp.WorldMatrix;
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(this.m_cubeGrid.PositionComp.LocalAABB, worldMatrix), Color.Yellow, 0.2f, false, true, false);
                MyRenderProxy.DebugDrawAxis(worldMatrix, 1f, false, false, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_FIXED_BLOCK_QUERIES)
            {
                foreach (MySlimBlock local1 in this.m_cubeGrid.GetBlocks())
                {
                    Vector3D vectord;
                    Matrix matrix;
                    BoundingBox geometryLocalBox = local1.FatBlock.GetGeometryLocalBox();
                    Vector3 halfExtents = geometryLocalBox.Size / 2f;
                    local1.ComputeScaledCenter(out vectord);
                    vectord = Vector3D.Transform(vectord + geometryLocalBox.Center, this.m_cubeGrid.WorldMatrix);
                    local1.Orientation.GetMatrix(out matrix);
                    worldMatrix = this.m_cubeGrid.WorldMatrix;
                    Quaternion rotation = Quaternion.CreateFromRotationMatrix((MatrixD) (matrix * worldMatrix.GetOrientation()));
                    MyPhysics.GetPenetrationsBox(ref halfExtents, ref vectord, ref rotation, this.m_penetrations, 14);
                    bool flag = false;
                    using (List<HkBodyCollision>.Enumerator enumerator2 = this.m_penetrations.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            IMyEntity collisionEntity = enumerator2.Current.GetCollisionEntity();
                            if ((collisionEntity != null) && (collisionEntity is MyVoxelMap))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    this.m_penetrations.Clear();
                    MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(vectord, halfExtents, rotation), flag ? Color.Green : Color.Red, 0.1f, false, false, false);
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_NAMES || MyDebugDrawSettings.DEBUG_DRAW_GRID_CONTROL)
            {
                string text = "";
                Color white = Color.White;
                if (MyDebugDrawSettings.DEBUG_DRAW_GRID_NAMES)
                {
                    text = text + this.m_cubeGrid.ToString() + " ";
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_GRID_CONTROL)
                {
                    MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(this.m_cubeGrid);
                    if (controllingPlayer != null)
                    {
                        text = text + "Controlled by: " + controllingPlayer.DisplayName;
                        white = Color.LightGreen;
                    }
                }
                MyRenderProxy.DebugDrawText3D(this.m_cubeGrid.PositionComp.WorldAABB.Center, text, white, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            }
            MyRenderComponentCubeGrid render = this.m_cubeGrid.Render;
            if (MyDebugDrawSettings.DEBUG_DRAW_BLOCK_GROUPS)
            {
                worldMatrix = this.m_cubeGrid.PositionComp.WorldMatrix;
                Vector3D translation = worldMatrix.Translation;
                foreach (MyBlockGroup group in this.m_cubeGrid.BlockGroups)
                {
                    MyRenderProxy.DebugDrawText3D(translation, group.Name.ToString(), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    worldMatrix = this.m_cubeGrid.PositionComp.WorldMatrix;
                    translation += (worldMatrix.Right * group.Name.Length) * 0.10000000149011612;
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_DIRTY_BLOCKS)
            {
                foreach (KeyValuePair<Vector3I, MyTimeSpan> pair in this.m_dirtyBlocks)
                {
                    Vector3 vector1;
                    if (this.m_cubeGrid.GetCubeBlock(pair.Key) == null)
                    {
                        vector1 = Color.Yellow.ToVector3();
                    }
                    else
                    {
                        vector1 = Color.Red.ToVector3();
                    }
                    Vector3 color = vector1;
                    MyRenderProxy.DebugDrawOBB((Matrix.CreateScale(this.m_cubeGrid.GridSize) * Matrix.CreateTranslation(pair.Key * this.m_cubeGrid.GridSize)) * this.m_cubeGrid.WorldMatrix, color, 0.15f, false, true, true, false);
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES)
            {
                Vector3D position = MySector.MainCamera.Position;
                foreach (MySlimBlock block in this.m_cubeGrid.CubeBlocks)
                {
                    MyCube cube;
                    if (this.m_cubeGrid.TryGetCube(block.Position, out cube))
                    {
                        int num = 0;
                        foreach (MyCubePart part in cube.Parts)
                        {
                            if ((part.Model.BoneMapping != null) && (num == MyPetaInputComponent.DEBUG_INDEX))
                            {
                                for (int i = 0; i < Math.Min(part.Model.BoneMapping.Length, 9); i++)
                                {
                                    Matrix orientation = part.InstanceData.LocalMatrix.GetOrientation();
                                    Vector3I vectori = Vector3I.Round(Vector3.Transform((Vector3) (((part.Model.BoneMapping[i] * 1f) - Vector3.One) * 1f), orientation));
                                    Vector3I bonePos = Vector3I.Round(Vector3.Transform((Vector3) (((part.Model.BoneMapping[i] * 1f) - Vector3.One) * 1f), orientation) + Vector3.One);
                                    Vector3 bone = this.m_cubeGrid.Skeleton.GetBone(block.Position, bonePos);
                                    Vector3D vectord3 = Vector3D.TransformNormal(bone, this.m_cubeGrid.PositionComp.WorldMatrix);
                                    Vector3D vectord6 = Vector3D.Transform((Vector3) (this.m_cubeGrid.GridSize * (block.Position + (vectori / 2f))), this.m_cubeGrid.PositionComp.WorldMatrix);
                                    MyRenderProxy.DebugDrawSphere(vectord6, 0.025f, Color.Green, 0.5f, false, true, true, false);
                                    MyRenderProxy.DebugDrawText3D(vectord6, i.ToString(), Color.Green, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                    Color? colorTo = null;
                                    MyRenderProxy.DebugDrawArrow3D(vectord6, vectord6 + vectord3, Color.Red, colorTo, false, 0.1, null, 0.5f, false);
                                }
                            }
                            num++;
                        }
                    }
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY && (this.m_cubeGrid.StructuralIntegrity != null))
            {
                this.m_cubeGrid.StructuralIntegrity.DebugDraw();
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_CUBES)
            {
                foreach (MySlimBlock local2 in this.m_cubeGrid.CubeBlocks)
                {
                    Matrix matrix4;
                    local2.GetLocalMatrix(out matrix4);
                    MyRenderProxy.DebugDrawAxis(matrix4 * this.m_cubeGrid.WorldMatrix, 1f, false, false, false);
                    MyCubeBlock fatBlock = local2.FatBlock;
                    if (fatBlock != null)
                    {
                        fatBlock.DebugDraw();
                    }
                }
            }
            this.m_cubeGrid.GridSystems.DebugDraw();
            bool flag1 = MyDebugDrawSettings.DEBUG_DRAW_GRID_TERMINAL_SYSTEMS;
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_ORIGINS)
            {
                MyRenderProxy.DebugDrawAxis(this.m_cubeGrid.PositionComp.WorldMatrix, 1f, false, false, false);
            }
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_ALL)
            {
                foreach (MySlimBlock block3 in this.m_cubeGrid.GetBlocks())
                {
                    vectord4 = this.m_cubeGrid.GridIntegerToWorld(block3.Position) - MySector.MainCamera.Position;
                    if (vectord4.LengthSquared() < 200.0)
                    {
                        this.DebugDrawMountPoints(block3);
                    }
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_BLOCK_INTEGRITY && (MySector.MainCamera != null))
            {
                vectord4 = MySector.MainCamera.Position - this.m_cubeGrid.PositionComp.WorldVolume.Center;
                if (vectord4.Length() < (16.0 + this.m_cubeGrid.PositionComp.WorldVolume.Radius))
                {
                    using (HashSet<MySlimBlock>.Enumerator enumerator = this.m_cubeGrid.CubeBlocks.GetEnumerator())
                    {
                        MySlimBlock current;
                        float num4;
                        goto TR_0011;
                    TR_0002:
                        MyRenderProxy.DebugDrawText3D(this.m_cubeGrid.GridIntegerToWorld(current.Position), ((int) num4).ToString(), Color.White, (this.m_cubeGrid.GridSizeEnum == MyCubeSize.Large) ? 0.65f : 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    TR_0011:
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            current = enumerator.Current;
                            Vector3D vectord5 = this.m_cubeGrid.GridIntegerToWorld(current.Position);
                            if (this.m_cubeGrid.GridSizeEnum != MyCubeSize.Large)
                            {
                                if (MySector.MainCamera == null)
                                {
                                    continue;
                                }
                                if ((MySector.MainCamera.Position - vectord5).LengthSquared() >= 9.0)
                                {
                                    continue;
                                }
                            }
                            num4 = 0f;
                            if (current.FatBlock is MyCompoundCubeBlock)
                            {
                                foreach (MySlimBlock block5 in (current.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                {
                                    num4 += block5.Integrity * block5.BlockDefinition.MaxIntegrityRatio;
                                }
                            }
                            else
                            {
                                num4 = current.Integrity * current.BlockDefinition.MaxIntegrityRatio;
                            }
                            goto TR_0002;
                        }
                    }
                }
            }
            base.DebugDraw();
        }

        public override void DebugDrawInvalidTriangles()
        {
            base.DebugDrawInvalidTriangles();
            foreach (KeyValuePair<Vector3I, MyCubeGridRenderCell> pair in this.m_cubeGrid.Render.RenderData.Cells)
            {
                IEnumerator<KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>>> enumerator = pair.Value.CubeParts.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>> current = enumerator.Current;
                        MyModel model = current.Key.Model;
                        if (model != null)
                        {
                            int trianglesCount = model.GetTrianglesCount();
                            for (int i = 0; i < trianglesCount; i++)
                            {
                                MyTriangleVertexIndices triangle = model.GetTriangle(i);
                                if (MyUtils.IsWrongTriangle(model.GetVertex(triangle.I0), model.GetVertex(triangle.I1), model.GetVertex(triangle.I2)))
                                {
                                    Vector3 pointFrom = Vector3.Transform(model.GetVertex(triangle.I0), (Matrix) this.m_cubeGrid.PositionComp.WorldMatrix);
                                    Vector3 pointTo = Vector3.Transform(model.GetVertex(triangle.I1), (Matrix) this.m_cubeGrid.PositionComp.WorldMatrix);
                                    Vector3 vector3 = Vector3.Transform(model.GetVertex(triangle.I2), (Matrix) this.m_cubeGrid.PositionComp.WorldMatrix);
                                    MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, Color.Purple, Color.Purple, false, false);
                                    MyRenderProxy.DebugDrawLine3D(pointTo, vector3, Color.Purple, Color.Purple, false, false);
                                    MyRenderProxy.DebugDrawLine3D(vector3, pointFrom, Color.Purple, Color.Purple, false, false);
                                    Vector3 vector4 = ((pointFrom + pointTo) + vector3) / 3f;
                                    MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitX, Color.Yellow, Color.Yellow, false, false);
                                    MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitY, Color.Yellow, Color.Yellow, false, false);
                                    MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitZ, Color.Yellow, Color.Yellow, false, false);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
        }

        private void DebugDrawMountPoints(MySlimBlock block)
        {
            if (block.FatBlock is MyCompoundCubeBlock)
            {
                foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                {
                    this.DebugDrawMountPoints(block2);
                }
            }
            else
            {
                Matrix matrix;
                block.GetLocalMatrix(out matrix);
                MatrixD drawMatrix = matrix * this.m_cubeGrid.WorldMatrix;
                if ((!MyFakes.ENABLE_FRACTURE_COMPONENT || (block.FatBlock == null)) || !block.FatBlock.Components.Has<MyFractureComponentBase>())
                {
                    MyCubeBuilder.DrawMountPoints(this.m_cubeGrid.GridSize, block.BlockDefinition, ref drawMatrix);
                }
                else
                {
                    MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
                    if (fractureComponent != null)
                    {
                        MyCubeBuilder.DrawMountPoints(this.m_cubeGrid.GridSize, block.BlockDefinition, drawMatrix, fractureComponent.MountPoints.GetInternalArray<MyCubeBlockDefinition.MountPoint>());
                    }
                }
            }
        }

        public override void PrepareForDraw()
        {
            base.PrepareForDraw();
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_DIRTY_BLOCKS)
            {
                MyTimeSpan span = MyTimeSpan.FromMilliseconds(1500.0);
                using (this.m_tmpRemoveList.GetClearToken<Vector3I>())
                {
                    foreach (KeyValuePair<Vector3I, MyTimeSpan> pair in this.m_dirtyBlocks)
                    {
                        if ((MySandboxGame.Static.TotalTime - pair.Value) > span)
                        {
                            this.m_tmpRemoveList.Add(pair.Key);
                        }
                    }
                    foreach (Vector3I vectori in this.m_tmpRemoveList)
                    {
                        this.m_dirtyBlocks.Remove(vectori);
                    }
                }
                foreach (Vector3I vectori2 in this.m_cubeGrid.DirtyBlocks)
                {
                    this.m_dirtyBlocks[vectori2] = MySandboxGame.Static.TotalTime;
                }
            }
        }
    }
}

