namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_FracturedBlock))]
    public class MyFracturedBlock : MyCubeBlock
    {
        private static List<HkdShapeInstanceInfo> m_children = new List<HkdShapeInstanceInfo>();
        private static List<HkdShapeInstanceInfo> m_shapeInfos = new List<HkdShapeInstanceInfo>();
        private static HashSet<Tuple<string, float>> m_tmpNamesAndBuildProgress = new HashSet<Tuple<string, float>>();
        public HkdBreakableShape Shape;
        public List<MyDefinitionId> OriginalBlocks;
        public List<MyBlockOrientation> Orientations;
        public List<MultiBlockPartInfo> MultiBlocks;
        private List<MyObjectBuilder_FracturedBlock.ShapeB> m_shapes = new List<MyObjectBuilder_FracturedBlock.ShapeB>();
        private List<MyCubeBlockDefinition.MountPoint> m_mpCache = new List<MyCubeBlockDefinition.MountPoint>();

        public MyFracturedBlock()
        {
            base.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            this.Render = new MyRenderComponentFracturedPiece();
            this.Render.NeedsDraw = true;
            this.Render.PersistentFlags = MyPersistentEntityFlags2.Enabled;
            base.CheckConnectionAllowed = true;
            base.AddDebugRenderComponent(new MyFBDebugRender(this));
        }

        private void AddMeshBuilderRecursively(List<HkdShapeInstanceInfo> children)
        {
            MyRenderComponentFracturedPiece render = this.Render;
            foreach (HkdShapeInstanceInfo info in children)
            {
                render.AddPiece(info.ShapeName, Matrix.Identity);
            }
        }

        protected override void Closing()
        {
            if (this.Shape.IsValid())
            {
                this.Shape.RemoveReference();
            }
            base.Closing();
        }

        public override bool ConnectionAllowed(ref Vector3I otherBlockPos, ref Vector3I faceNormal, MyCubeBlockDefinition def)
        {
            if (this.MountPoints == null)
            {
                return true;
            }
            Vector3I pos = (Vector3I) (base.Position + faceNormal);
            MySlimBlock cubeBlock = base.CubeGrid.GetCubeBlock(pos);
            MyBlockOrientation orientation = (cubeBlock == null) ? MyBlockOrientation.Identity : cubeBlock.Orientation;
            Vector3I position = base.Position;
            this.m_mpCache.Clear();
            if ((cubeBlock != null) && (cubeBlock.FatBlock is MyFracturedBlock))
            {
                this.m_mpCache.AddRange((cubeBlock.FatBlock as MyFracturedBlock).MountPoints);
            }
            else if ((cubeBlock != null) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
            {
                List<MyCubeBlockDefinition.MountPoint> outMountPoints = new List<MyCubeBlockDefinition.MountPoint>();
                foreach (MySlimBlock block2 in (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks())
                {
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = block2.BlockDefinition.GetBuildProgressModelMountPoints(block2.BuildLevelRatio);
                    MyCubeGrid.TransformMountPoints(outMountPoints, block2.BlockDefinition, buildProgressModelMountPoints, ref block2.Orientation);
                    this.m_mpCache.AddRange(outMountPoints);
                }
            }
            else if (cubeBlock != null)
            {
                MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = def.GetBuildProgressModelMountPoints(cubeBlock.BuildLevelRatio);
                MyCubeGrid.TransformMountPoints(this.m_mpCache, def, buildProgressModelMountPoints, ref orientation);
            }
            return MyCubeGrid.CheckMountPointsForSide(this.MountPoints, ref base.SlimBlock.Orientation, ref position, base.BlockDefinition.Id, ref faceNormal, this.m_mpCache, ref orientation, ref pos, def.Id);
        }

        private static void ConvertAllShapesToFractureComponentShapeBuilder(HkdBreakableShape shape, ref Matrix shapeRotation, MyBlockOrientation blockOrientation, HashSet<Tuple<string, float>> namesAndBuildProgress, MyObjectBuilder_FractureComponentCubeBlock fractureComponentBuilder, out float buildProgress)
        {
            buildProgress = 1f;
            string name = shape.Name;
            Tuple<string, float> tuple = null;
            foreach (Tuple<string, float> tuple2 in namesAndBuildProgress)
            {
                if (tuple2.Item1 == name)
                {
                    tuple = tuple2;
                    break;
                }
            }
            if ((tuple != null) && (new MyBlockOrientation(ref shapeRotation) == blockOrientation))
            {
                MyObjectBuilder_FractureComponentBase.FracturedShape item = new MyObjectBuilder_FractureComponentBase.FracturedShape {
                    Name = name,
                    Fixed = MyDestructionHelper.IsFixed(shape)
                };
                fractureComponentBuilder.Shapes.Add(item);
                buildProgress = tuple.Item2;
            }
            if (shape.GetChildrenCount() > 0)
            {
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                shape.GetChildren(list);
                foreach (HkdShapeInstanceInfo info in list)
                {
                    float num;
                    Matrix transform = info.GetTransform();
                    ConvertAllShapesToFractureComponentShapeBuilder(info.Shape, ref transform, blockOrientation, namesAndBuildProgress, fractureComponentBuilder, out num);
                    if (tuple == null)
                    {
                        buildProgress = num;
                    }
                }
            }
        }

        public static MyObjectBuilder_CubeGrid ConvertFracturedBlocksToComponents(MyObjectBuilder_CubeGrid gridBuilder)
        {
            bool flag = false;
            using (List<MyObjectBuilder_CubeBlock>.Enumerator enumerator = gridBuilder.CubeBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is MyObjectBuilder_FracturedBlock)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                return gridBuilder;
            }
            bool enableSmallToLargeConnections = gridBuilder.EnableSmallToLargeConnections;
            gridBuilder.EnableSmallToLargeConnections = false;
            bool createPhysics = gridBuilder.CreatePhysics;
            gridBuilder.CreatePhysics = true;
            MyCubeGrid grid = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(gridBuilder, false) as MyCubeGrid;
            if (grid == null)
            {
                return gridBuilder;
            }
            grid.ConvertFracturedBlocksToComponents();
            gridBuilder.EnableSmallToLargeConnections = enableSmallToLargeConnections;
            MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) grid.GetObjectBuilder(false);
            objectBuilder.EnableSmallToLargeConnections = enableSmallToLargeConnections;
            gridBuilder.CreatePhysics = createPhysics;
            objectBuilder.CreatePhysics = createPhysics;
            grid.Close();
            Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(objectBuilder);
            return objectBuilder;
        }

        public MyObjectBuilder_CubeBlock ConvertToOriginalBlocksWithFractureComponent()
        {
            List<MyObjectBuilder_CubeBlock> cubeBlockBuilders = new List<MyObjectBuilder_CubeBlock>();
            for (int i = 0; i < this.OriginalBlocks.Count; i++)
            {
                MyCubeBlockDefinition definition;
                MyDefinitionId defId = this.OriginalBlocks[i];
                MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition);
                if (definition != null)
                {
                    Quaternion quaternion;
                    float num2;
                    MultiBlockPartInfo local1;
                    MyBlockOrientation blockOrientation = this.Orientations[i];
                    if ((this.MultiBlocks == null) || (this.MultiBlocks.Count <= i))
                    {
                        local1 = null;
                    }
                    else
                    {
                        local1 = this.MultiBlocks[i];
                    }
                    MultiBlockPartInfo info = local1;
                    MyObjectBuilder_CubeBlock item = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) defId) as MyObjectBuilder_CubeBlock;
                    blockOrientation.GetQuaternion(out quaternion);
                    item.Orientation = quaternion;
                    item.Min = base.Position;
                    item.MultiBlockId = (info != null) ? info.MultiBlockId : 0;
                    item.MultiBlockDefinition = null;
                    if (info != null)
                    {
                        item.MultiBlockDefinition = new SerializableDefinitionId?((SerializableDefinitionId) info.MultiBlockDefinition);
                    }
                    item.ComponentContainer = new MyObjectBuilder_ComponentContainer();
                    MyObjectBuilder_FractureComponentCubeBlock fractureComponentBuilder = new MyObjectBuilder_FractureComponentCubeBlock();
                    m_tmpNamesAndBuildProgress.Clear();
                    GetAllBlockBreakableShapeNames(definition, m_tmpNamesAndBuildProgress);
                    ConvertAllShapesToFractureComponentShapeBuilder(this.Shape, ref Matrix.Identity, blockOrientation, m_tmpNamesAndBuildProgress, fractureComponentBuilder, out num2);
                    m_tmpNamesAndBuildProgress.Clear();
                    if (fractureComponentBuilder.Shapes.Count != 0)
                    {
                        if (definition.BuildProgressModels != null)
                        {
                            foreach (MyCubeBlockDefinition.BuildProgressModel model in definition.BuildProgressModels)
                            {
                                if (model.BuildRatioUpperBound >= num2)
                                {
                                    break;
                                }
                                float buildRatioUpperBound = model.BuildRatioUpperBound;
                            }
                        }
                        MyObjectBuilder_ComponentContainer.ComponentData data = new MyObjectBuilder_ComponentContainer.ComponentData {
                            TypeId = typeof(MyFractureComponentBase).Name,
                            Component = fractureComponentBuilder
                        };
                        item.ComponentContainer.Components.Add(data);
                        item.BuildPercent = num2;
                        item.IntegrityPercent = MyDefinitionManager.Static.DestructionDefinition.ConvertedFractureIntegrityRatio * num2;
                        if ((i == 0) && (base.CubeGrid.GridSizeEnum == MyCubeSize.Small))
                        {
                            return item;
                        }
                        cubeBlockBuilders.Add(item);
                    }
                }
            }
            return ((cubeBlockBuilders.Count <= 0) ? null : MyCompoundCubeBlock.CreateBuilder(cubeBlockBuilders));
        }

        private unsafe void CreateMountPoints()
        {
            if (!MyFakes.FRACTURED_BLOCK_AABB_MOUNT_POINTS)
            {
                HkShape[] shapes = new HkShape[] { this.Shape.GetShape() };
                this.MountPoints = MyCubeBuilder.AutogenerateMountpoints(shapes, base.CubeGrid.GridSize);
            }
            else
            {
                this.MountPoints = new List<MyCubeBlockDefinition.MountPoint>();
                BoundingBox blockBB = BoundingBox.CreateInvalid();
                for (int i = 0; i < this.OriginalBlocks.Count; i++)
                {
                    Matrix matrix;
                    MyDefinitionId id = this.OriginalBlocks[i];
                    this.Orientations[i].GetMatrix(out matrix);
                    Vector3 vector2 = new Vector3(MyDefinitionManager.Static.GetCubeBlockDefinition(id).Size);
                    BoundingBox box2 = new BoundingBox(-vector2 / 2f, vector2 / 2f);
                    blockBB = blockBB.Include(box2.Transform(matrix));
                }
                Vector3 halfExtents = blockBB.HalfExtents;
                Vector3* vectorPtr1 = (Vector3*) ref blockBB.Min;
                vectorPtr1[0] += halfExtents;
                Vector3* vectorPtr2 = (Vector3*) ref blockBB.Max;
                vectorPtr2[0] += halfExtents;
                this.Shape.GetChildren(m_children);
                foreach (HkdShapeInstanceInfo info in m_children)
                {
                    MyFractureComponentCubeBlock.AddMountForShape(info.Shape, info.GetTransform(), ref blockBB, base.CubeGrid.GridSize, this.MountPoints);
                }
                if (m_children.Count == 0)
                {
                    MyFractureComponentCubeBlock.AddMountForShape(this.Shape, Matrix.Identity, ref blockBB, base.CubeGrid.GridSize, this.MountPoints);
                }
                m_children.Clear();
            }
        }

        public static void GetAllBlockBreakableShapeNames(MyCubeBlockDefinition blockDef, HashSet<Tuple<string, float>> outNamesAndBuildProgress)
        {
            string modelAsset = blockDef.Model;
            if (MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes == null)
            {
                MyDestructionData.Static.LoadModelDestruction(modelAsset, blockDef, Vector3.One, true, false);
            }
            GetAllBlockBreakableShapeNames(MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes[0], outNamesAndBuildProgress, 1f);
            if (blockDef.BuildProgressModels != null)
            {
                float buildRatioUpperBound = 0f;
                foreach (MyCubeBlockDefinition.BuildProgressModel model in blockDef.BuildProgressModels)
                {
                    modelAsset = model.File;
                    if (MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes == null)
                    {
                        MyDestructionData.Static.LoadModelDestruction(modelAsset, blockDef, Vector3.One, true, false);
                    }
                    float buildProgress = 0.5f * (model.BuildRatioUpperBound + buildRatioUpperBound);
                    GetAllBlockBreakableShapeNames(MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes[0], outNamesAndBuildProgress, buildProgress);
                    buildRatioUpperBound = model.BuildRatioUpperBound;
                }
            }
        }

        public static void GetAllBlockBreakableShapeNames(HkdBreakableShape shape, HashSet<Tuple<string, float>> outNamesAndBuildProgress, float buildProgress)
        {
            string name = shape.Name;
            if (!string.IsNullOrEmpty(name))
            {
                outNamesAndBuildProgress.Add(new Tuple<string, float>(name, buildProgress));
            }
            if (shape.GetChildrenCount() > 0)
            {
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                shape.GetChildren(list);
                foreach (HkdShapeInstanceInfo info in list)
                {
                    GetAllBlockBreakableShapeNames(info.Shape, outNamesAndBuildProgress, buildProgress);
                }
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_FracturedBlock.ShapeB eb2;
            MyObjectBuilder_FracturedBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_FracturedBlock;
            if (!string.IsNullOrEmpty(this.Shape.Name) && !this.Shape.IsCompound())
            {
                eb2 = new MyObjectBuilder_FracturedBlock.ShapeB {
                    Name = this.Shape.Name
                };
                objectBuilderCubeBlock.Shapes.Add(eb2);
            }
            else
            {
                this.Shape.GetChildren(m_children);
                foreach (HkdShapeInstanceInfo info in m_children)
                {
                    eb2 = new MyObjectBuilder_FracturedBlock.ShapeB {
                        Name = info.ShapeName
                    };
                    eb2.Orientation = Quaternion.CreateFromRotationMatrix(info.GetTransform().GetOrientation());
                    eb2.Fixed = MyDestructionHelper.IsFixed(info.Shape);
                    MyObjectBuilder_FracturedBlock.ShapeB item = eb2;
                    objectBuilderCubeBlock.Shapes.Add(item);
                }
                m_children.Clear();
            }
            foreach (MyDefinitionId id in this.OriginalBlocks)
            {
                objectBuilderCubeBlock.BlockDefinitions.Add((SerializableDefinitionId) id);
            }
            foreach (MyBlockOrientation orientation in this.Orientations)
            {
                objectBuilderCubeBlock.BlockOrientations.Add(orientation);
            }
            if (this.MultiBlocks != null)
            {
                foreach (MultiBlockPartInfo info2 in this.MultiBlocks)
                {
                    if (info2 == null)
                    {
                        objectBuilderCubeBlock.MultiBlocks.Add(null);
                        continue;
                    }
                    MyObjectBuilder_FracturedBlock.MyMultiBlockPart item = new MyObjectBuilder_FracturedBlock.MyMultiBlockPart();
                    item.MultiBlockDefinition = (SerializableDefinitionId) info2.MultiBlockDefinition;
                    item.MultiBlockId = info2.MultiBlockId;
                    objectBuilderCubeBlock.MultiBlocks.Add(item);
                }
            }
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            base.CheckConnectionAllowed = true;
            MyObjectBuilder_FracturedBlock block = builder as MyObjectBuilder_FracturedBlock;
            if (block.Shapes.Count == 0)
            {
                if (!block.CreatingFracturedBlock)
                {
                    throw new Exception("No relevant shape was found for fractured block. It was probably reexported and names changed.");
                }
            }
            else
            {
                this.OriginalBlocks = new List<MyDefinitionId>();
                this.Orientations = new List<MyBlockOrientation>();
                List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
                foreach (SerializableDefinitionId id in block.BlockDefinitions)
                {
                    MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(id);
                    string model = cubeBlockDefinition.Model;
                    if (MyModels.GetModelOnlyData(model).HavokBreakableShapes == null)
                    {
                        MyDestructionData.Static.LoadModelDestruction(model, cubeBlockDefinition, Vector3.One, true, false);
                    }
                    HkdBreakableShape shape = MyModels.GetModelOnlyData(model).HavokBreakableShapes[0];
                    Quaternion? rotation = null;
                    Vector3? translation = null;
                    HkdShapeInstanceInfo item = new HkdShapeInstanceInfo(shape, rotation, translation);
                    list.Add(item);
                    m_children.Add(item);
                    shape.GetChildren(m_children);
                    if (cubeBlockDefinition.BuildProgressModels != null)
                    {
                        MyCubeBlockDefinition.BuildProgressModel[] buildProgressModels = cubeBlockDefinition.BuildProgressModels;
                        for (int j = 0; j < buildProgressModels.Length; j++)
                        {
                            model = buildProgressModels[j].File;
                            if (MyModels.GetModelOnlyData(model).HavokBreakableShapes == null)
                            {
                                MyDestructionData.Static.LoadModelDestruction(model, cubeBlockDefinition, Vector3.One, true, false);
                            }
                            shape = MyModels.GetModelOnlyData(model).HavokBreakableShapes[0];
                            rotation = null;
                            translation = null;
                            item = new HkdShapeInstanceInfo(shape, rotation, translation);
                            list.Add(item);
                            m_children.Add(item);
                            shape.GetChildren(m_children);
                        }
                    }
                    this.OriginalBlocks.Add(id);
                }
                foreach (SerializableBlockOrientation orientation in block.BlockOrientations)
                {
                    this.Orientations.Add((MyBlockOrientation) orientation);
                }
                if (block.MultiBlocks.Count > 0)
                {
                    this.MultiBlocks = new List<MultiBlockPartInfo>();
                    foreach (MyObjectBuilder_FracturedBlock.MyMultiBlockPart part in block.MultiBlocks)
                    {
                        if (part == null)
                        {
                            this.MultiBlocks.Add(null);
                            continue;
                        }
                        MultiBlockPartInfo item = new MultiBlockPartInfo();
                        item.MultiBlockDefinition = part.MultiBlockDefinition;
                        item.MultiBlockId = part.MultiBlockId;
                        this.MultiBlocks.Add(item);
                    }
                }
                this.m_shapes.AddRange(block.Shapes);
                for (int i = 0; i < m_children.Count; i++)
                {
                    HkdShapeInstanceInfo child = m_children[i];
                    Func<MyObjectBuilder_FracturedBlock.ShapeB, bool> predicate = s => s.Name == child.ShapeName;
                    IEnumerable<MyObjectBuilder_FracturedBlock.ShapeB> source = this.m_shapes.Where<MyObjectBuilder_FracturedBlock.ShapeB>(predicate);
                    if (source.Count<MyObjectBuilder_FracturedBlock.ShapeB>() <= 0)
                    {
                        child.GetChildren(m_children);
                    }
                    else
                    {
                        MyObjectBuilder_FracturedBlock.ShapeB item = source.First<MyObjectBuilder_FracturedBlock.ShapeB>();
                        Matrix transform = Matrix.CreateFromQuaternion((Quaternion) item.Orientation);
                        transform.Translation = child.GetTransform().Translation;
                        HkdShapeInstanceInfo info2 = new HkdShapeInstanceInfo(child.Shape.Clone(), transform);
                        if (item.Fixed)
                        {
                            info2.Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                        }
                        list.Add(info2);
                        m_shapeInfos.Add(info2);
                        this.m_shapes.Remove(item);
                    }
                }
                if (m_shapeInfos.Count == 0)
                {
                    m_children.Clear();
                    throw new Exception("No relevant shape was found for fractured block. It was probably reexported and names changed.");
                }
                foreach (HkdShapeInstanceInfo info3 in m_shapeInfos)
                {
                    HkdBreakableShape shape = info3.Shape;
                    if (!string.IsNullOrEmpty(shape.Name))
                    {
                        this.Render.AddPiece(info3.Shape.Name, Matrix.CreateFromQuaternion(Quaternion.CreateFromRotationMatrix(info3.GetTransform().GetOrientation())));
                    }
                }
                if (base.CubeGrid.CreatePhysics)
                {
                    HkdBreakableShape? oldParent = null;
                    HkdBreakableShape shape3 = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, m_shapeInfos);
                    shape3.RecalcMassPropsFromChildren();
                    this.Shape = shape3;
                    HkMassProperties massProperties = new HkMassProperties();
                    shape3.BuildMassProperties(ref massProperties);
                    this.Shape = new HkdBreakableShape(shape3.GetShape(), ref massProperties);
                    shape3.RemoveReference();
                    foreach (HkdShapeInstanceInfo info4 in m_shapeInfos)
                    {
                        this.Shape.AddShape(ref info4);
                    }
                    this.Shape.SetStrenght(MyDestructionConstants.STRENGTH);
                    this.CreateMountPoints();
                }
                m_children.Clear();
                foreach (HkdShapeInstanceInfo info5 in m_shapeInfos)
                {
                    info5.Shape.RemoveReference();
                }
                foreach (HkdShapeInstanceInfo info6 in list)
                {
                    info6.RemoveReference();
                }
                m_shapeInfos.Clear();
            }
        }

        public bool IsMultiBlockPart(MyDefinitionId multiBlockDefinition, int multiblockId)
        {
            if (this.MultiBlocks != null)
            {
                using (List<MultiBlockPartInfo>.Enumerator enumerator = this.MultiBlocks.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MultiBlockPartInfo current = enumerator.Current;
                        if ((current != null) && ((current.MultiBlockDefinition == multiBlockDefinition) && (current.MultiBlockId == multiblockId)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void SetDataFromCompound(HkdBreakableShape compound)
        {
            MyRenderComponentFracturedPiece render = this.Render;
            if (render != null)
            {
                compound.GetChildren(m_shapeInfos);
                foreach (HkdShapeInstanceInfo info in m_shapeInfos)
                {
                    if (info.IsValid())
                    {
                        render.AddPiece(info.ShapeName, info.GetTransform());
                    }
                }
                m_shapeInfos.Clear();
            }
        }

        internal void SetDataFromHavok(HkdBreakableShape shape, bool compound)
        {
            this.Shape = shape;
            if (compound)
            {
                this.SetDataFromCompound(shape);
            }
            else
            {
                this.Render.AddPiece(shape.Name, Matrix.Identity);
            }
            this.CreateMountPoints();
        }

        private MyRenderComponentFracturedPiece Render
        {
            get => 
                ((MyRenderComponentFracturedPiece) base.Render);
            set => 
                (base.Render = value);
        }

        public List<MyCubeBlockDefinition.MountPoint> MountPoints { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct Info
        {
            public HkdBreakableShape Shape;
            public Vector3I Position;
            public bool Compound;
            public List<MyDefinitionId> OriginalBlocks;
            public List<MyBlockOrientation> Orientations;
            public List<MyFracturedBlock.MultiBlockPartInfo> MultiBlocks;
        }

        public class MultiBlockPartInfo
        {
            public MyDefinitionId MultiBlockDefinition;
            public int MultiBlockId;
        }

        private class MyFBDebugRender : MyDebugRenderComponentBase
        {
            private MyFracturedBlock m_block;

            public MyFBDebugRender(MyFracturedBlock b)
            {
                this.m_block = b;
            }

            public override void DebugDraw()
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS && (this.m_block.MountPoints != null))
                {
                    MatrixD worldMatrix = this.m_block.CubeGrid.PositionComp.WorldMatrix;
                    worldMatrix.Translation = this.m_block.CubeGrid.GridIntegerToWorld(this.m_block.Position);
                    MyCubeBuilder.DrawMountPoints(this.m_block.CubeGrid.GridSize, this.m_block.BlockDefinition, worldMatrix, this.m_block.MountPoints.ToArray());
                }
            }

            public override void DebugDrawInvalidTriangles()
            {
            }
        }
    }
}

