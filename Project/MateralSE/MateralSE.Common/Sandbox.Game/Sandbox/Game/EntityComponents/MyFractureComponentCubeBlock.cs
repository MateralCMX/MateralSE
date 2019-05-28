namespace Sandbox.Game.EntityComponents
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_FractureComponentCubeBlock), true)]
    public class MyFractureComponentCubeBlock : MyFractureComponentBase
    {
        private readonly List<MyObjectBuilder_FractureComponentBase.FracturedShape> m_tmpShapeListInit = new List<MyObjectBuilder_FractureComponentBase.FracturedShape>();
        private MyObjectBuilder_ComponentBase m_obFracture;

        public static unsafe HkdBreakableShape AddMountForShape(HkdBreakableShape shape, Matrix transform, ref BoundingBox blockBB, float gridSize, List<MyCubeBlockDefinition.MountPoint> outMountPoints)
        {
            Vector4 vector;
            Vector4 vector2;
            shape.GetShape().GetLocalAABB(0.01f, out vector, out vector2);
            BoundingBox box = new BoundingBox(new Vector3(vector), new Vector3(vector2));
            box = box.Transform(transform);
            Vector3* vectorPtr1 = (Vector3*) ref box.Min;
            vectorPtr1[0] /= gridSize;
            Vector3* vectorPtr2 = (Vector3*) ref box.Max;
            vectorPtr2[0] /= gridSize;
            box.Inflate((float) 0.04f);
            Vector3* vectorPtr3 = (Vector3*) ref box.Min;
            vectorPtr3[0] += blockBB.HalfExtents;
            Vector3* vectorPtr4 = (Vector3*) ref box.Max;
            vectorPtr4[0] += blockBB.HalfExtents;
            if (blockBB.Contains(box) == ContainmentType.Intersects)
            {
                box.Inflate((float) -0.04f);
                foreach (int num2 in Base6Directions.EnumDirections)
                {
                    Vector3 vector3 = Base6Directions.Directions[num2];
                    Vector3 vector4 = Vector3.Abs(vector3);
                    MyCubeBlockDefinition.MountPoint item = new MyCubeBlockDefinition.MountPoint {
                        Start = box.Min,
                        End = box.Max,
                        Enabled = true
                    };
                    Vector3 vector5 = ((item.Start * vector4) / (blockBB.HalfExtents * 2f)) - (vector4 * 0.04f);
                    Vector3 vector6 = ((item.End * vector4) / (blockBB.HalfExtents * 2f)) + (vector4 * 0.04f);
                    bool flag = false;
                    bool flag2 = false;
                    if (((vector5.Max() < 1f) && (vector6.Max() > 1f)) && (vector3.Max() > 0f))
                    {
                        flag = true;
                        flag2 = true;
                    }
                    else if (((vector5.Min() < 0f) && (vector6.Max() > 0f)) && (vector3.Min() < 0f))
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        Vector3* vectorPtr5 = (Vector3*) ref item.Start;
                        vectorPtr5[0] -= (item.Start * vector4) - (vector4 * 0.04f);
                        Vector3* vectorPtr6 = (Vector3*) ref item.End;
                        vectorPtr6[0] -= (item.End * vector4) + (vector4 * 0.04f);
                        if (flag2)
                        {
                            Vector3* vectorPtr7 = (Vector3*) ref item.Start;
                            vectorPtr7[0] += (vector4 * blockBB.HalfExtents) * 2f;
                            Vector3* vectorPtr8 = (Vector3*) ref item.End;
                            vectorPtr8[0] += (vector4 * blockBB.HalfExtents) * 2f;
                        }
                        Vector3* vectorPtr9 = (Vector3*) ref item.Start;
                        vectorPtr9[0] -= blockBB.HalfExtents - (Vector3.One / 2f);
                        Vector3* vectorPtr10 = (Vector3*) ref item.End;
                        vectorPtr10[0] -= blockBB.HalfExtents - (Vector3.One / 2f);
                        item.Normal = new Vector3I(vector3);
                        outMountPoints.Add(item);
                    }
                }
            }
            return shape;
        }

        private unsafe void CreateMountPoints()
        {
            if (!MyFakes.FRACTURED_BLOCK_AABB_MOUNT_POINTS)
            {
                HkShape[] shapes = new HkShape[] { base.Shape.GetShape() };
                this.MountPoints = MyCubeBuilder.AutogenerateMountpoints(shapes, this.Block.CubeGrid.GridSize);
            }
            else
            {
                if (this.MountPoints == null)
                {
                    this.MountPoints = new List<MyCubeBlockDefinition.MountPoint>();
                }
                else
                {
                    this.MountPoints.Clear();
                }
                Vector3 vector = new Vector3(this.Block.BlockDefinition.Size);
                BoundingBox blockBB = new BoundingBox(-vector / 2f, vector / 2f);
                Vector3 halfExtents = blockBB.HalfExtents;
                Vector3* vectorPtr1 = (Vector3*) ref blockBB.Min;
                vectorPtr1[0] += halfExtents;
                Vector3* vectorPtr2 = (Vector3*) ref blockBB.Max;
                vectorPtr2[0] += halfExtents;
                base.Shape.GetChildren(base.m_tmpChildren);
                if (base.m_tmpChildren.Count > 0)
                {
                    foreach (HkdShapeInstanceInfo info in base.m_tmpChildren)
                    {
                        AddMountForShape(info.Shape, Matrix.Identity, ref blockBB, this.Block.CubeGrid.GridSize, this.MountPoints);
                    }
                }
                else
                {
                    AddMountForShape(base.Shape, Matrix.Identity, ref blockBB, this.Block.CubeGrid.GridSize, this.MountPoints);
                }
                base.m_tmpChildren.Clear();
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            if (this.Block != null)
            {
                this.Init(builder);
            }
            else
            {
                this.m_obFracture = builder;
            }
        }

        public float GetIntegrityRatioFromFracturedPieceCounts()
        {
            if (base.Shape.IsValid() && (this.Block != null))
            {
                int totalBreakableShapeChildrenCount = this.Block.GetTotalBreakableShapeChildrenCount();
                if (totalBreakableShapeChildrenCount > 0)
                {
                    int totalChildrenCount = base.Shape.GetTotalChildrenCount();
                    if (totalChildrenCount <= totalBreakableShapeChildrenCount)
                    {
                        return (((float) totalChildrenCount) / ((float) totalBreakableShapeChildrenCount));
                    }
                }
            }
            return 0f;
        }

        private void Init(MyObjectBuilder_ComponentBase builder)
        {
            MyObjectBuilder_FractureComponentCubeBlock block = builder as MyObjectBuilder_FractureComponentCubeBlock;
            if (block.Shapes.Count == 0)
            {
                throw new Exception("No relevant shape was found for fractured block. It was probably reexported and names changed. Block definition: " + this.Block.BlockDefinition.Id.ToString());
            }
            this.RecreateShape(block.Shapes);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.Block = (base.Entity as MyCubeBlock).SlimBlock;
            this.Block.FatBlock.CheckConnectionAllowed = true;
            MySlimBlock cubeBlock = this.Block.CubeGrid.GetCubeBlock(this.Block.Position);
            if (cubeBlock != null)
            {
                cubeBlock.FatBlock.CheckConnectionAllowed = true;
            }
            if (this.m_obFracture != null)
            {
                this.Init(this.m_obFracture);
                this.m_obFracture = null;
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.Block.FatBlock.CheckConnectionAllowed = false;
            MySlimBlock cubeBlock = this.Block.CubeGrid.GetCubeBlock(this.Block.Position);
            if ((cubeBlock != null) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
            {
                bool flag = false;
                foreach (MySlimBlock block2 in (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks())
                {
                    flag |= block2.FatBlock.CheckConnectionAllowed;
                }
                if (!flag)
                {
                    cubeBlock.FatBlock.CheckConnectionAllowed = false;
                }
            }
        }

        public void OnCubeGridChanged()
        {
            base.m_tmpShapeList.Clear();
            base.GetCurrentFracturedShapeList(base.m_tmpShapeList, null);
            this.RecreateShape(base.m_tmpShapeList);
            base.m_tmpShapeList.Clear();
        }

        protected override void RecreateShape(List<MyObjectBuilder_FractureComponentBase.FracturedShape> shapeList)
        {
            if (base.Shape.IsValid())
            {
                base.Shape.RemoveReference();
                base.Shape = new HkdBreakableShape();
            }
            MyRenderComponentFracturedPiece render = this.Block.FatBlock.Render as MyRenderComponentFracturedPiece;
            if (render != null)
            {
                render.ClearModels();
                render.UpdateRenderObject(false, true);
            }
            if (shapeList.Count != 0)
            {
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                MyCubeBlockDefinition blockDefinition = this.Block.BlockDefinition;
                string model = blockDefinition.Model;
                if (MyModels.GetModelOnlyData(model).HavokBreakableShapes == null)
                {
                    MyDestructionData.Static.LoadModelDestruction(model, blockDefinition, Vector3.One, true, false);
                }
                HkdBreakableShape shape = MyModels.GetModelOnlyData(model).HavokBreakableShapes[0];
                Quaternion? rotation = null;
                Vector3? translation = null;
                HkdShapeInstanceInfo item = new HkdShapeInstanceInfo(shape, rotation, translation);
                list.Add(item);
                base.m_tmpChildren.Add(item);
                shape.GetChildren(base.m_tmpChildren);
                if (blockDefinition.BuildProgressModels != null)
                {
                    MyCubeBlockDefinition.BuildProgressModel[] buildProgressModels = blockDefinition.BuildProgressModels;
                    for (int j = 0; j < buildProgressModels.Length; j++)
                    {
                        model = buildProgressModels[j].File;
                        if (MyModels.GetModelOnlyData(model).HavokBreakableShapes == null)
                        {
                            MyDestructionData.Static.LoadModelDestruction(model, blockDefinition, Vector3.One, true, false);
                        }
                        shape = MyModels.GetModelOnlyData(model).HavokBreakableShapes[0];
                        rotation = null;
                        translation = null;
                        item = new HkdShapeInstanceInfo(shape, rotation, translation);
                        list.Add(item);
                        base.m_tmpChildren.Add(item);
                        shape.GetChildren(base.m_tmpChildren);
                    }
                }
                this.m_tmpShapeListInit.Clear();
                this.m_tmpShapeListInit.AddList<MyObjectBuilder_FractureComponentBase.FracturedShape>(shapeList);
                for (int i = 0; i < base.m_tmpChildren.Count; i++)
                {
                    HkdShapeInstanceInfo child = base.m_tmpChildren[i];
                    IEnumerable<MyObjectBuilder_FractureComponentBase.FracturedShape> source = from s in this.m_tmpShapeListInit
                        where s.Name == child.ShapeName
                        select s;
                    if (source.Count<MyObjectBuilder_FractureComponentBase.FracturedShape>() <= 0)
                    {
                        child.GetChildren(base.m_tmpChildren);
                    }
                    else
                    {
                        MyObjectBuilder_FractureComponentBase.FracturedShape shape2 = source.First<MyObjectBuilder_FractureComponentBase.FracturedShape>();
                        HkdShapeInstanceInfo info2 = new HkdShapeInstanceInfo(child.Shape.Clone(), Matrix.Identity);
                        if (shape2.Fixed)
                        {
                            info2.Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                        }
                        list.Add(info2);
                        base.m_tmpShapeInfos.Add(info2);
                        this.m_tmpShapeListInit.Remove(shape2);
                    }
                }
                this.m_tmpShapeListInit.Clear();
                if ((shapeList.Count > 0) && (base.m_tmpShapeInfos.Count == 0))
                {
                    base.m_tmpChildren.Clear();
                    throw new Exception("No relevant shape was found for fractured block. It was probably reexported and names changed. Block definition: " + this.Block.BlockDefinition.Id.ToString());
                }
                if (render != null)
                {
                    foreach (HkdShapeInstanceInfo info3 in base.m_tmpShapeInfos)
                    {
                        HkdBreakableShape shape3 = info3.Shape;
                        if (!string.IsNullOrEmpty(shape3.Name))
                        {
                            render.AddPiece(info3.Shape.Name, Matrix.Identity);
                        }
                    }
                    render.UpdateRenderObject(true, true);
                }
                base.m_tmpChildren.Clear();
                if (this.Block.CubeGrid.CreatePhysics)
                {
                    HkdBreakableShape? oldParent = null;
                    HkdBreakableShape shape4 = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, base.m_tmpShapeInfos);
                    shape4.RecalcMassPropsFromChildren();
                    HkMassProperties massProperties = new HkMassProperties();
                    shape4.BuildMassProperties(ref massProperties);
                    base.Shape = new HkdBreakableShape(shape4.GetShape(), ref massProperties);
                    shape4.RemoveReference();
                    foreach (HkdShapeInstanceInfo info4 in base.m_tmpShapeInfos)
                    {
                        base.Shape.AddShape(ref info4);
                    }
                    base.Shape.SetStrenght(MyDestructionConstants.STRENGTH);
                    this.CreateMountPoints();
                    MySlimBlock cubeBlock = this.Block.CubeGrid.GetCubeBlock(this.Block.Position);
                    if (cubeBlock != null)
                    {
                        cubeBlock.CubeGrid.UpdateBlockNeighbours(cubeBlock);
                    }
                    if (this.Block.CubeGrid.Physics != null)
                    {
                        this.Block.CubeGrid.Physics.AddDirtyBlock(this.Block);
                    }
                }
                foreach (HkdShapeInstanceInfo info5 in base.m_tmpShapeInfos)
                {
                    info5.Shape.RemoveReference();
                }
                base.m_tmpShapeInfos.Clear();
                foreach (HkdShapeInstanceInfo info6 in list)
                {
                    info6.RemoveReference();
                }
            }
        }

        public override bool RemoveChildShapes(string[] shapeNames)
        {
            base.RemoveChildShapes(shapeNames);
            if (!base.Shape.IsValid() || (base.Shape.GetChildrenCount() == 0))
            {
                this.MountPoints.Clear();
                if (Sync.IsServer)
                {
                    return true;
                }
                this.Block.FatBlock.Components.Remove<MyFractureComponentBase>();
            }
            return false;
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_FractureComponentCubeBlock ob = base.Serialize(false) as MyObjectBuilder_FractureComponentCubeBlock;
            base.SerializeInternal(ob);
            return ob;
        }

        public override void SetShape(HkdBreakableShape shape, bool compound)
        {
            base.SetShape(shape, compound);
            this.CreateMountPoints();
            MySlimBlock cubeBlock = this.Block.CubeGrid.GetCubeBlock(this.Block.Position);
            if (cubeBlock != null)
            {
                cubeBlock.CubeGrid.UpdateBlockNeighbours(cubeBlock);
            }
            if (this.Block.CubeGrid.Physics != null)
            {
                this.Block.CubeGrid.Physics.AddDirtyBlock(this.Block);
            }
        }

        public MySlimBlock Block { get; private set; }

        public List<MyCubeBlockDefinition.MountPoint> MountPoints { get; private set; }

        public override MyPhysicalModelDefinition PhysicalModelDefinition =>
            this.Block.BlockDefinition;

        private class MyFractureComponentBlockDebugRender : MyDebugRenderComponentBase
        {
            private MyCubeBlock m_block;

            public MyFractureComponentBlockDebugRender(MyCubeBlock b)
            {
                this.m_block = b;
            }

            public override void DebugDraw()
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS && this.m_block.Components.Has<MyFractureComponentBase>())
                {
                    MyFractureComponentCubeBlock fractureComponent = this.m_block.GetFractureComponent();
                    if (fractureComponent != null)
                    {
                        MatrixD worldMatrix = this.m_block.CubeGrid.PositionComp.WorldMatrix;
                        worldMatrix.Translation = this.m_block.CubeGrid.GridIntegerToWorld(this.m_block.Position);
                        MyCubeBuilder.DrawMountPoints(this.m_block.CubeGrid.GridSize, this.m_block.BlockDefinition, worldMatrix, fractureComponent.MountPoints.ToArray());
                    }
                }
            }

            public override void DebugDrawInvalidTriangles()
            {
            }
        }
    }
}

