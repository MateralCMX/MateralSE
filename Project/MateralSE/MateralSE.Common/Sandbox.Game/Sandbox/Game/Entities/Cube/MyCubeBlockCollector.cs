namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyCubeBlockCollector : IDisposable
    {
        public const bool SHRINK_CONVEX_SHAPE = false;
        public const float BOX_SHRINK = 0f;
        private const bool ADD_INNER_BONES_TO_CONVEX = true;
        private const float MAX_BOX_EXTENT = 40f;
        public List<ShapeInfo> ShapeInfos = new List<ShapeInfo>();
        public List<HkShape> Shapes = new List<HkShape>();
        private HashSet<MySlimBlock> m_tmpRefreshSet = new HashSet<MySlimBlock>();
        private List<Vector3> m_tmpHelperVerts = new List<Vector3>();
        private List<Vector3I> m_tmpCubes = new List<Vector3I>();
        private HashSet<Vector3I> m_tmpCheck;

        private void AddBox(Vector3I minPos, Vector3I maxPos, ref Vector3 min, ref Vector3 max)
        {
            Vector3 translation = (min + max) * 0.5f;
            HkBoxShape shape = new HkBoxShape((max - translation) - 0f, MyPerGameSettings.PhysicsConvexRadius);
            HkConvexTranslateShape shape2 = new HkConvexTranslateShape((HkConvexShape) shape, translation, HkReferencePolicy.TakeOwnership);
            this.Shapes.Add((HkShape) shape2);
            ShapeInfo item = new ShapeInfo {
                Count = 1,
                Min = minPos,
                Max = maxPos
            };
            this.ShapeInfos.Add(item);
        }

        private void AddBoxes(MySlimBlock block)
        {
            int x = block.Min.X;
            while (x <= block.Max.X)
            {
                int y = block.Min.Y;
                while (true)
                {
                    if (y > block.Max.Y)
                    {
                        x++;
                        break;
                    }
                    int z = block.Min.Z;
                    while (true)
                    {
                        if (z > block.Max.Z)
                        {
                            y++;
                            break;
                        }
                        Vector3I item = new Vector3I(x, y, z);
                        this.m_tmpCubes.Add(item);
                        z++;
                    }
                }
            }
        }

        private void AddConvexShape(MySlimBlock block, bool applySkeleton)
        {
            this.m_tmpHelperVerts.Clear();
            Vector3 vector = (Vector3) (block.Min * block.CubeGrid.GridSize);
            Vector3I vectori = (Vector3I) ((block.Min * 2) + 1);
            MyGridSkeleton skeleton = block.CubeGrid.Skeleton;
            foreach (Vector3 vector3 in MyBlockVerticesCache.GetBlockVertices(block.BlockDefinition.CubeDefinition.CubeTopology, block.Orientation))
            {
                Vector3 vector2;
                Vector3I pos = (Vector3I) (vectori + Vector3I.Round(vector3));
                Vector3 vector4 = vector3 * block.CubeGrid.GridSizeHalf;
                if (applySkeleton && skeleton.TryGetBone(ref pos, out vector2))
                {
                    vector4.Add(vector2);
                }
                this.m_tmpHelperVerts.Add(vector4 + vector);
            }
            this.Shapes.Add((HkShape) new HkConvexVerticesShape(this.m_tmpHelperVerts.GetInternalArray<Vector3>(), this.m_tmpHelperVerts.Count, false, MyPerGameSettings.PhysicsConvexRadius));
            ShapeInfo item = new ShapeInfo {
                Count = 1,
                Min = block.Min,
                Max = block.Max
            };
            this.ShapeInfos.Add(item);
        }

        private void AddMass(MySlimBlock block, IDictionary<Vector3I, HkMassElement> massResults)
        {
            float mass = block.BlockDefinition.Mass;
            if (MyFakes.ENABLE_COMPOUND_BLOCKS && (block.FatBlock is MyCompoundCubeBlock))
            {
                mass = 0f;
                foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                {
                    mass += block2.GetMass();
                }
            }
            HkMassProperties properties = new HkMassProperties();
            HkMassElement element = new HkMassElement {
                Properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties((((block.Max - block.Min) + Vector3I.One) * block.CubeGrid.GridSize) / 2f, mass),
                Tranform = Matrix.CreateTranslation(((block.Min + block.Max) * 0.5f) * block.CubeGrid.GridSize)
            };
            massResults[block.Position] = element;
        }

        private unsafe void AddSegmentedParts(float gridSize, MyVoxelSegmentation segmenter, MyVoxelSegmentationType segmentationType)
        {
            int num = (int) Math.Floor((double) (40f / gridSize));
            Vector3 vector = new Vector3(gridSize * 0.5f);
            if (segmenter != null)
            {
                int mergeIterations = (segmentationType == MyVoxelSegmentationType.Optimized) ? 1 : 0;
                segmenter.ClearInput();
                foreach (Vector3I vectori3 in this.m_tmpCubes)
                {
                    segmenter.AddInput(vectori3);
                }
                foreach (MyVoxelSegmentation.Segment segment in segmenter.FindSegments(segmentationType, mergeIterations))
                {
                    Vector3I vectori;
                    vectori.X = segment.Min.X;
                    while (vectori.X <= segment.Max.X)
                    {
                        vectori.Y = segment.Min.Y;
                        while (true)
                        {
                            if (vectori.Y > segment.Max.Y)
                            {
                                int* numPtr3 = (int*) ref vectori.X;
                                numPtr3[0] += num;
                                break;
                            }
                            vectori.Z = segment.Min.Z;
                            while (true)
                            {
                                if (vectori.Z > segment.Max.Z)
                                {
                                    int* numPtr2 = (int*) ref vectori.Y;
                                    numPtr2[0] += num;
                                    break;
                                }
                                Vector3I maxPos = Vector3I.Min((Vector3I) ((vectori + num) - 1), segment.Max);
                                Vector3 min = ((Vector3) (vectori * gridSize)) - vector;
                                Vector3 max = (maxPos * gridSize) + vector;
                                this.AddBox(vectori, maxPos, ref min, ref max);
                                int* numPtr1 = (int*) ref vectori.Z;
                                numPtr1[0] += num;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Vector3I vectori4 in this.m_tmpCubes)
                {
                    Vector3 min = ((Vector3) (vectori4 * gridSize)) - vector;
                    Vector3 max = (vectori4 * gridSize) + vector;
                    this.AddBox(vectori4, vectori4, ref min, ref max);
                }
            }
        }

        public void Clear()
        {
            this.ShapeInfos.Clear();
            foreach (HkShape shape in this.Shapes)
            {
                shape.RemoveReference();
            }
            this.Shapes.Clear();
        }

        public void Collect(MyCubeGrid grid, MyVoxelSegmentation segmenter, MyVoxelSegmentationType segmentationType, IDictionary<Vector3I, HkMassElement> massResults)
        {
            foreach (MySlimBlock block in grid.GetBlocks())
            {
                if (block.FatBlock is MyCompoundCubeBlock)
                {
                    this.CollectCompoundBlock((MyCompoundCubeBlock) block.FatBlock, massResults);
                    continue;
                }
                this.CollectBlock(block, block.BlockDefinition.PhysicsOption, massResults, true);
            }
            this.AddSegmentedParts(grid.GridSize, segmenter, segmentationType);
            this.m_tmpCubes.Clear();
        }

        public void CollectArea(MyCubeGrid grid, HashSet<Vector3I> dirtyBlocks, MyVoxelSegmentation segmenter, MyVoxelSegmentationType segmentationType, IDictionary<Vector3I, HkMassElement> massResults)
        {
            using (MyUtils.ReuseCollection<MySlimBlock>(ref this.m_tmpRefreshSet))
            {
                foreach (Vector3I vectori in dirtyBlocks)
                {
                    if (massResults != null)
                    {
                        massResults.Remove(vectori);
                    }
                    MySlimBlock cubeBlock = grid.GetCubeBlock(vectori);
                    if (cubeBlock != null)
                    {
                        this.m_tmpRefreshSet.Add(cubeBlock);
                    }
                }
                foreach (MySlimBlock block2 in this.m_tmpRefreshSet)
                {
                    this.CollectBlock(block2, block2.BlockDefinition.PhysicsOption, massResults, true);
                }
                this.AddSegmentedParts(grid.GridSize, segmenter, segmentationType);
                this.m_tmpCubes.Clear();
            }
        }

        private void CollectBlock(MySlimBlock block, MyPhysicsOption physicsOption, IDictionary<Vector3I, HkMassElement> massResults, bool allowSegmentation = true)
        {
            if (block.BlockDefinition.HasPhysics && (block.CubeGrid != null))
            {
                if (massResults != null)
                {
                    this.AddMass(block, massResults);
                }
                if (block.BlockDefinition.BlockTopology == MyBlockTopology.Cube)
                {
                    MyCubeTopology topology = (block.BlockDefinition.CubeDefinition != null) ? block.BlockDefinition.CubeDefinition.CubeTopology : MyCubeTopology.Box;
                    if (MyFakes.ENABLE_SIMPLE_GRID_PHYSICS)
                    {
                        physicsOption = MyPhysicsOption.Box;
                    }
                    else if (topology == MyCubeTopology.Box)
                    {
                        if (!block.ShowParts)
                        {
                            physicsOption = MyPhysicsOption.Box;
                        }
                        else if ((block.BlockDefinition.CubeDefinition != null) && block.CubeGrid.Skeleton.IsDeformed(block.Min, 0.05f, block.CubeGrid, false))
                        {
                            physicsOption = MyPhysicsOption.Convex;
                        }
                    }
                    if (physicsOption == MyPhysicsOption.Box)
                    {
                        this.AddBoxes(block);
                    }
                    else if (physicsOption == MyPhysicsOption.Convex)
                    {
                        this.AddConvexShape(block, block.ShowParts);
                    }
                }
                else if (physicsOption != MyPhysicsOption.None)
                {
                    HkShape[] havokCollisionShapes = null;
                    if (block.FatBlock != null)
                    {
                        havokCollisionShapes = block.FatBlock.ModelCollision.HavokCollisionShapes;
                    }
                    if (((havokCollisionShapes != null) && (havokCollisionShapes.Length != 0)) && !MyFakes.ENABLE_SIMPLE_GRID_PHYSICS)
                    {
                        Vector3 translation;
                        Quaternion quaternion;
                        if (block.FatBlock.ModelCollision.ExportedWrong)
                        {
                            translation = (Vector3) (block.Position * block.CubeGrid.GridSize);
                        }
                        else
                        {
                            translation = block.FatBlock.PositionComp.LocalMatrix.Translation;
                        }
                        HkShape[] havokCollisionShapes = block.FatBlock.ModelCollision.HavokCollisionShapes;
                        block.Orientation.GetQuaternion(out quaternion);
                        Vector3 scale = Vector3.One * block.FatBlock.ModelCollision.ScaleFactor;
                        if ((havokCollisionShapes.Length == 1) && (havokCollisionShapes[0].ShapeType == HkShapeType.List))
                        {
                            HkListShape shape = (HkListShape) havokCollisionShapes[0];
                            for (int i = 0; i < shape.TotalChildrenCount; i++)
                            {
                                HkShape childByIndex = shape.GetChildByIndex(i);
                                this.Shapes.Add((HkShape) new HkConvexTransformShape((HkConvexShape) childByIndex, ref translation, ref quaternion, ref scale, HkReferencePolicy.None));
                            }
                        }
                        else if ((havokCollisionShapes.Length != 1) || (havokCollisionShapes[0].ShapeType != HkShapeType.Mopp))
                        {
                            for (int i = 0; i < havokCollisionShapes.Length; i++)
                            {
                                this.Shapes.Add((HkShape) new HkConvexTransformShape((HkConvexShape) havokCollisionShapes[i], ref translation, ref quaternion, ref scale, HkReferencePolicy.None));
                            }
                        }
                        else
                        {
                            HkMoppBvTreeShape shape3 = (HkMoppBvTreeShape) havokCollisionShapes[0];
                            int num2 = 0;
                            while (true)
                            {
                                HkShapeCollection shapeCollection = shape3.ShapeCollection;
                                if (num2 >= shapeCollection.ShapeCount)
                                {
                                    break;
                                }
                                HkShape shape = shape3.ShapeCollection.GetShape((uint) num2, null);
                                this.Shapes.Add((HkShape) new HkConvexTransformShape((HkConvexShape) shape, ref translation, ref quaternion, ref scale, HkReferencePolicy.None));
                                num2++;
                            }
                        }
                        ShapeInfo item = new ShapeInfo {
                            Count = havokCollisionShapes.Length,
                            Min = block.Min,
                            Max = block.Max
                        };
                        this.ShapeInfos.Add(item);
                    }
                    else
                    {
                        int x = block.Min.X;
                        while (x <= block.Max.X)
                        {
                            int y = block.Min.Y;
                            while (true)
                            {
                                if (y > block.Max.Y)
                                {
                                    x++;
                                    break;
                                }
                                int z = block.Min.Z;
                                while (true)
                                {
                                    if (z > block.Max.Z)
                                    {
                                        y++;
                                        break;
                                    }
                                    Vector3I item = new Vector3I(x, y, z);
                                    if (allowSegmentation)
                                    {
                                        this.m_tmpCubes.Add(item);
                                    }
                                    else
                                    {
                                        Vector3 min = ((Vector3) (item * block.CubeGrid.GridSize)) - new Vector3(block.CubeGrid.GridSize / 2f);
                                        Vector3 max = (item * block.CubeGrid.GridSize) + new Vector3(block.CubeGrid.GridSize / 2f);
                                        this.AddBox(item, item, ref min, ref max);
                                    }
                                    z++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void CollectCompoundBlock(MyCompoundCubeBlock compoundBlock, IDictionary<Vector3I, HkMassElement> massResults)
        {
            int count = this.ShapeInfos.Count;
            foreach (MySlimBlock block in compoundBlock.GetBlocks())
            {
                if (block.BlockDefinition.BlockTopology == MyBlockTopology.TriangleMesh)
                {
                    this.CollectBlock(block, block.BlockDefinition.PhysicsOption, massResults, false);
                }
            }
            if (this.ShapeInfos.Count > (count + 1))
            {
                ShapeInfo info = this.ShapeInfos[count];
                while (true)
                {
                    if (this.ShapeInfos.Count <= (count + 1))
                    {
                        this.ShapeInfos[count] = info;
                        break;
                    }
                    int index = this.ShapeInfos.Count - 1;
                    int* numPtr1 = (int*) ref info.Count;
                    numPtr1[0] += this.ShapeInfos[index].Count;
                    this.ShapeInfos.RemoveAt(index);
                }
            }
        }

        public void CollectMassElements(MyCubeGrid grid, IDictionary<Vector3I, HkMassElement> massResults)
        {
            if (massResults != null)
            {
                foreach (MySlimBlock block in grid.GetBlocks())
                {
                    if (block.FatBlock is MyCompoundCubeBlock)
                    {
                        foreach (MySlimBlock block2 in ((MyCompoundCubeBlock) block.FatBlock).GetBlocks())
                        {
                            if (block2.BlockDefinition.BlockTopology == MyBlockTopology.TriangleMesh)
                            {
                                this.AddMass(block2, massResults);
                            }
                        }
                        continue;
                    }
                    this.AddMass(block, massResults);
                }
            }
        }

        public void Dispose()
        {
            this.Clear();
        }

        private unsafe bool IsValid()
        {
            bool flag;
            if (this.m_tmpCheck == null)
            {
                this.m_tmpCheck = new HashSet<Vector3I>();
            }
            try
            {
                using (List<ShapeInfo>.Enumerator enumerator = this.ShapeInfos.GetEnumerator())
                {
                    Vector3I vectori;
                    ShapeInfo current;
                    goto TR_0013;
                TR_000D:
                    while (true)
                    {
                        if (vectori.Y <= current.Max.Y)
                        {
                            vectori.Z = current.Min.Z;
                            while (true)
                            {
                                if (vectori.Z <= current.Max.Z)
                                {
                                    if (this.m_tmpCheck.Add(vectori))
                                    {
                                        int* numPtr1 = (int*) ref vectori.Z;
                                        numPtr1[0]++;
                                        continue;
                                    }
                                    flag = false;
                                }
                                else
                                {
                                    int* numPtr2 = (int*) ref vectori.Y;
                                    numPtr2[0]++;
                                    continue;
                                }
                                break;
                            }
                        }
                        else
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            goto TR_0010;
                        }
                        break;
                    }
                    return flag;
                TR_0010:
                    while (true)
                    {
                        if (vectori.X > current.Max.X)
                        {
                            break;
                        }
                        vectori.Y = current.Min.Y;
                        goto TR_000D;
                    }
                TR_0013:
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            vectori.X = current.Min.X;
                        }
                        else
                        {
                            goto TR_0002;
                        }
                        break;
                    }
                    goto TR_0010;
                }
            TR_0002:
                flag = true;
            }
            finally
            {
                this.m_tmpCheck.Clear();
            }
            return flag;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShapeInfo
        {
            public int Count;
            public Vector3I Min;
            public Vector3I Max;
        }
    }
}

