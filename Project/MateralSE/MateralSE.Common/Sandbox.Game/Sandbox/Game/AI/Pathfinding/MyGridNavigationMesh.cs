namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    public class MyGridNavigationMesh : MyNavigationMesh
    {
        private MyCubeGrid m_grid;
        private Dictionary<Vector3I, List<int>> m_smallTriangleRegistry;
        private MyVector3ISet m_cubeSet;
        private Dictionary<EdgeIndex, int> m_connectionHelper;
        private MyNavmeshCoordinator m_coordinator;
        private MyHighLevelGroup m_higherLevel;
        private MyGridHighLevelHelper m_higherLevelHelper;
        private Component m_component;
        private static HashSet<Vector3I> m_mergeHelper = new HashSet<Vector3I>();
        private static List<KeyValuePair<MyNavigationTriangle, Vector3I>> m_tmpTriangleList = new List<KeyValuePair<MyNavigationTriangle, Vector3I>>();
        private bool m_static;

        public MyGridNavigationMesh(MyCubeGrid grid, MyNavmeshCoordinator coordinator, int triPrealloc = 0x20, Func<long> timestampFunction = null) : this(coordinator?.Links, triPrealloc, timestampFunction)
        {
            this.m_connectionHelper = new Dictionary<EdgeIndex, int>();
            this.m_smallTriangleRegistry = new Dictionary<Vector3I, List<int>>();
            this.m_cubeSet = new MyVector3ISet();
            this.m_coordinator = coordinator;
            this.m_static = false;
            if (grid != null)
            {
                this.m_higherLevel = new MyHighLevelGroup(this, coordinator.HighLevelLinks, timestampFunction);
                this.m_higherLevelHelper = new MyGridHighLevelHelper(this, this.m_smallTriangleRegistry, new Vector3I(8, 8, 8));
                this.m_grid = grid;
                grid.OnBlockAdded += new Action<MySlimBlock>(this.grid_OnBlockAdded);
                grid.OnBlockRemoved += new Action<MySlimBlock>(this.grid_OnBlockRemoved);
                float num = 1f / ((float) grid.CubeBlocks.Count);
                Vector3 zero = Vector3.Zero;
                foreach (MySlimBlock block in grid.CubeBlocks)
                {
                    this.OnBlockAddedInternal(block);
                    zero += (block.Position * grid.GridSize) * num;
                }
            }
        }

        private void AddBlock(MySlimBlock block)
        {
            Vector3I min = block.Min;
            Vector3I max = block.Max;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref min, ref max);
            while (iterator.IsValid())
            {
                this.m_cubeSet.Add(ref min);
                iterator.GetNext(out min);
            }
            MatrixI transform = new MatrixI(block.Position, block.Orientation.Forward, block.Orientation.Up);
            this.MergeFromAnotherMesh(block.BlockDefinition.NavigationDefinition.Mesh, ref transform);
        }

        public MyNavigationTriangle AddTriangle(ref Vector3 a, ref Vector3 b, ref Vector3 c) => 
            ((this.m_grid == null) ? this.AddTriangleInternal(a, b, c) : null);

        private MyNavigationTriangle AddTriangleInternal(Vector3 a, Vector3 b, Vector3 c)
        {
            int num;
            int num2;
            int num3;
            Vector3I pointB = Vector3I.Round(a * 256f);
            Vector3I pointA = Vector3I.Round(b * 256f);
            Vector3I vectori3 = Vector3I.Round(c * 256f);
            Vector3 vector = pointB / 256f;
            Vector3 vector2 = pointA / 256f;
            Vector3 vector3 = vectori3 / 256f;
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref pointA, ref pointB), out num))
            {
                num = -1;
            }
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref vectori3, ref pointA), out num2))
            {
                num2 = -1;
            }
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref pointB, ref vectori3), out num3))
            {
                num3 = -1;
            }
            int num4 = num2;
            int num5 = num3;
            MyNavigationTriangle triangle = base.AddTriangle(ref vector, ref vector3, ref vector2, ref num3, ref num2, ref num);
            if (num == -1)
            {
                this.m_connectionHelper.Add(new EdgeIndex(ref pointB, ref pointA), num);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref pointA, ref pointB));
            }
            if (num4 == -1)
            {
                this.m_connectionHelper.Add(new EdgeIndex(ref pointA, ref vectori3), num2);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref vectori3, ref pointA));
            }
            if (num5 == -1)
            {
                this.m_connectionHelper.Add(new EdgeIndex(ref vectori3, ref pointB), num3);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref pointB, ref vectori3));
            }
            return triangle;
        }

        private void CopyTriangle(MyNavigationTriangle otherTri, Vector3I triPosition, ref MatrixI transform)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            otherTri.GetTransformed(ref transform, out vector, out vector2, out vector3);
            if (MyPerGameSettings.NavmeshPresumesDownwardGravity)
            {
                Vector3 vector4 = Vector3.Cross(vector3 - vector, vector2 - vector);
                vector4.Normalize();
                if (Vector3.Dot(vector4, Base6Directions.GetVector(Base6Directions.Direction.Up)) < 0.7f)
                {
                    return;
                }
            }
            Vector3I.Transform(ref triPosition, ref transform, out triPosition);
            MyNavigationTriangle tri = this.AddTriangleInternal(vector, vector3, vector2);
            this.RegisterTriangleInternal(tri, ref triPosition);
        }

        public override void DebugDraw(ref Matrix drawMatrix)
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.EDGES) != MyWEMDebugDrawMode.NONE) && (this.m_connectionHelper != null))
                {
                    foreach (KeyValuePair<EdgeIndex, int> pair in this.m_connectionHelper)
                    {
                        Vector3 pointTo = Vector3.Transform(pair.Key.B / 256f, (Matrix) drawMatrix);
                        MyRenderProxy.DebugDrawLine3D(Vector3.Transform(pair.Key.A / 256f, (Matrix) drawMatrix), pointTo, Color.Red, Color.Yellow, false, false);
                    }
                }
                if ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.NORMALS) != MyWEMDebugDrawMode.NONE)
                {
                    foreach (KeyValuePair<Vector3I, List<int>> pair2 in this.m_smallTriangleRegistry)
                    {
                        foreach (int num in pair2.Value)
                        {
                            MyNavigationTriangle triangle = base.GetTriangle(num);
                            Vector3 pointTo = Vector3.Transform(triangle.Center + (triangle.Normal * 0.2f), (Matrix) drawMatrix);
                            MyRenderProxy.DebugDrawLine3D(Vector3.Transform(triangle.Center, (Matrix) drawMatrix), pointTo, Color.Blue, Color.Blue, true, false);
                        }
                    }
                }
                if (MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY && (this.m_higherLevel != null))
                {
                    this.m_higherLevel.DebugDraw(MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY_LITE);
                }
            }
        }

        private void EraseCubeTriangles(Vector3I pos)
        {
            List<int> list;
            if (this.m_smallTriangleRegistry.TryGetValue(pos, out list))
            {
                m_tmpTriangleList.Clear();
                foreach (int num in list)
                {
                    MyNavigationTriangle key = base.GetTriangle(num);
                    m_tmpTriangleList.Add(new KeyValuePair<MyNavigationTriangle, Vector3I>(key, pos));
                }
                foreach (KeyValuePair<MyNavigationTriangle, Vector3I> pair in m_tmpTriangleList)
                {
                    this.RemoveTriangle(pair.Key, pair.Value);
                }
                m_tmpTriangleList.Clear();
                this.m_smallTriangleRegistry.Remove(pos);
            }
        }

        private void EraseFaceTriangles(Vector3I pos, Base6Directions.Direction direction)
        {
            m_tmpTriangleList.Clear();
            Vector3I intVector = Base6Directions.GetIntVector((int) direction);
            List<int> list = null;
            if (this.m_smallTriangleRegistry.TryGetValue(pos, out list))
            {
                foreach (int num in list)
                {
                    MyNavigationTriangle triangle = base.GetTriangle(num);
                    if (this.IsFaceTriangle(triangle, pos, intVector))
                    {
                        m_tmpTriangleList.Add(new KeyValuePair<MyNavigationTriangle, Vector3I>(triangle, pos));
                    }
                }
            }
            foreach (KeyValuePair<MyNavigationTriangle, Vector3I> pair in m_tmpTriangleList)
            {
                this.RemoveTriangle(pair.Key, pair.Value);
            }
            m_tmpTriangleList.Clear();
        }

        public override MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq)
        {
            if (highLevel)
            {
                return null;
            }
            Vector3 vector = (Vector3) (Vector3D.Transform(point, this.m_grid.PositionComp.WorldMatrixNormalizedInv) / this.m_grid.GridSize);
            float closestDistSq = ((float) closestDistanceSq) / this.m_grid.GridSize;
            MyNavigationTriangle closestNavigationTriangle = this.GetClosestNavigationTriangle(ref vector, ref closestDistSq);
            if (closestNavigationTriangle != null)
            {
                closestDistanceSq = closestDistSq * this.m_grid.GridSize;
            }
            return closestNavigationTriangle;
        }

        public List<Vector4D> FindPath(Vector3 start, Vector3 end)
        {
            start /= this.m_grid.GridSize;
            end /= this.m_grid.GridSize;
            float positiveInfinity = float.PositiveInfinity;
            MyNavigationTriangle closestNavigationTriangle = this.GetClosestNavigationTriangle(ref start, ref positiveInfinity);
            if (closestNavigationTriangle == null)
            {
                return null;
            }
            positiveInfinity = float.PositiveInfinity;
            MyNavigationTriangle triangle2 = this.GetClosestNavigationTriangle(ref end, ref positiveInfinity);
            if (triangle2 == null)
            {
                return null;
            }
            List<Vector4D> list = base.FindRefinedPath(closestNavigationTriangle, triangle2, ref start, ref end);
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Vector4D vectord = list[i] * this.m_grid.GridSize;
                    list[i] = vectord;
                }
            }
            return list;
        }

        private Vector3I FindTriangleCube(int triIndex, ref Vector3I edgePositionA, ref Vector3I edgePositionB)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I.Min(ref edgePositionA, ref edgePositionB, out vectori);
            Vector3I.Max(ref edgePositionA, ref edgePositionB, out vectori2);
            vectori = Vector3I.Round((new Vector3(vectori) / 256f) - Vector3.Half);
            vectori2 = Vector3I.Round((new Vector3(vectori2) / 256f) + Vector3.Half);
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                List<int> list;
                this.m_smallTriangleRegistry.TryGetValue(vectori, out list);
                if ((list != null) && list.Contains(triIndex))
                {
                    return vectori;
                }
                iterator.GetNext(out vectori);
            }
            return Vector3I.Zero;
        }

        private unsafe void FixBlockFaces(MySlimBlock block)
        {
            Vector3I vectori;
            Vector3I backward;
            vectori.X = block.Min.X;
            while (vectori.X <= block.Max.X)
            {
                vectori.Y = block.Min.Y;
                while (true)
                {
                    if (vectori.Y > block.Max.Y)
                    {
                        int* numPtr2 = (int*) ref vectori.X;
                        numPtr2[0]++;
                        break;
                    }
                    backward = Vector3I.Backward;
                    vectori.Z = block.Min.Z - 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    backward = Vector3I.Forward;
                    vectori.Z = block.Max.Z + 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    int* numPtr1 = (int*) ref vectori.Y;
                    numPtr1[0]++;
                }
            }
            vectori.X = block.Min.X;
            while (vectori.X <= block.Max.X)
            {
                vectori.Z = block.Min.Z;
                while (true)
                {
                    if (vectori.Z > block.Max.Z)
                    {
                        int* numPtr4 = (int*) ref vectori.X;
                        numPtr4[0]++;
                        break;
                    }
                    backward = Vector3I.Up;
                    vectori.Y = block.Min.Y - 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    backward = Vector3I.Down;
                    vectori.Y = block.Max.Y + 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    int* numPtr3 = (int*) ref vectori.Z;
                    numPtr3[0]++;
                }
            }
            vectori.Y = block.Min.Y;
            while (vectori.Y <= block.Max.Y)
            {
                vectori.Z = block.Min.Z;
                while (true)
                {
                    if (vectori.Z > block.Max.Z)
                    {
                        int* numPtr6 = (int*) ref vectori.Y;
                        numPtr6[0]++;
                        break;
                    }
                    backward = Vector3I.Right;
                    vectori.X = block.Min.X - 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    backward = Vector3I.Left;
                    vectori.X = block.Max.X + 1;
                    this.FixCubeFace(ref vectori, ref backward);
                    int* numPtr5 = (int*) ref vectori.Z;
                    numPtr5[0]++;
                }
            }
        }

        private void FixCubeFace(ref Vector3I pos, ref Vector3I dir)
        {
            if (this.m_cubeSet.Contains(ref pos))
            {
                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(pos);
                MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                if (fatBlock != null)
                {
                    ListReader<MySlimBlock> blocks = fatBlock.GetBlocks();
                    MySlimBlock block3 = null;
                    foreach (MySlimBlock block4 in blocks)
                    {
                        if (block4.BlockDefinition.NavigationDefinition != null)
                        {
                            block3 = block4;
                            break;
                        }
                    }
                    if (block3 != null)
                    {
                        cubeBlock = block3;
                    }
                }
                if (cubeBlock.BlockDefinition.NavigationDefinition != null)
                {
                    MatrixI xi2;
                    Vector3I vectori;
                    Vector3I vectori2;
                    List<int> list;
                    MatrixI matrix = new MatrixI(cubeBlock.Position, cubeBlock.Orientation.Forward, cubeBlock.Orientation.Up);
                    MatrixI.Invert(ref matrix, out xi2);
                    Vector3I.Transform(ref pos, ref xi2, out vectori);
                    Vector3I.TransformNormal(ref dir, ref xi2, out vectori2);
                    MyGridNavigationMesh mesh = cubeBlock.BlockDefinition.NavigationDefinition.Mesh;
                    if ((mesh != null) && mesh.m_smallTriangleRegistry.TryGetValue(vectori, out list))
                    {
                        foreach (int num in list)
                        {
                            MyNavigationTriangle triangle = mesh.GetTriangle(num);
                            if (this.IsFaceTriangle(triangle, vectori, vectori2))
                            {
                                this.CopyTriangle(triangle, vectori, ref matrix);
                            }
                        }
                    }
                }
            }
        }

        private MyNavigationTriangle GetClosestNavigationTriangle(ref Vector3 point, ref float closestDistSq)
        {
            Vector3I vectori;
            Vector3I.Round(ref point, out vectori);
            MyNavigationTriangle triangle = null;
            Vector3I start = vectori - new Vector3I(4, 4, 4);
            Vector3I end = (Vector3I) (vectori + new Vector3I(4, 4, 4));
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
            while (iterator.IsValid())
            {
                List<int> list;
                this.m_smallTriangleRegistry.TryGetValue(start, out list);
                if (list != null)
                {
                    foreach (int num in list)
                    {
                        MyNavigationTriangle triangle2 = base.GetTriangle(num);
                        MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = triangle2.GetVertexEnumerator();
                        vertexEnumerator.MoveNext();
                        Vector3 current = vertexEnumerator.Current;
                        vertexEnumerator.MoveNext();
                        Vector3 vector2 = vertexEnumerator.Current;
                        vertexEnumerator.MoveNext();
                        Vector3 vector3 = vertexEnumerator.Current;
                        Vector3 vector4 = ((current + vector2) + vector3) / 3f;
                        Vector3 vector5 = vector2 - current;
                        Vector3 vector6 = vector3 - vector2;
                        float num2 = Vector3.DistanceSquared(vector4, point);
                        if (num2 < (vector5.LengthSquared() + vector6.LengthSquared()))
                        {
                            Vector3 vector7 = Vector3.Cross(vector5, vector6);
                            vector7.Normalize();
                            vector5 = Vector3.Cross(vector5, vector7);
                            vector6 = Vector3.Cross(vector6, vector7);
                            float num3 = -Vector3.Dot(vector5, current);
                            float num4 = -Vector3.Dot(vector6, vector2);
                            Vector3 vector1 = Vector3.Cross(current - vector3, vector7);
                            float num5 = -Vector3.Dot(vector1, vector3);
                            float num6 = Vector3.Dot(vector5, point) + num3;
                            float num7 = Vector3.Dot(vector6, point) + num4;
                            float num8 = Vector3.Dot(vector1, point) + num5;
                            num2 = Vector3.Dot(vector7, point) - Vector3.Dot(vector7, vector4);
                            num2 *= num2;
                            if (num6 > 0f)
                            {
                                if (num7 <= 0f)
                                {
                                    num2 = (num8 <= 0f) ? (num2 + Vector3.DistanceSquared(vector3, point)) : (num2 + (num7 * num7));
                                }
                                else if (num8 < 0f)
                                {
                                    num2 += num8 * num8;
                                }
                            }
                            else if (num7 > 0f)
                            {
                                num2 = (num8 <= 0f) ? (num2 + Vector3.DistanceSquared(current, point)) : (num2 + (num6 * num6));
                            }
                            else if (num8 > 0f)
                            {
                                num2 += Vector3.DistanceSquared(vector2, point);
                            }
                        }
                        if (num2 < closestDistSq)
                        {
                            triangle = triangle2;
                            closestDistSq = num2;
                        }
                    }
                }
                iterator.GetNext(out start);
            }
            return triangle;
        }

        public override IMyHighLevelComponent GetComponent(MyHighLevelPrimitive highLevelPrimitive) => 
            new Component(this, highLevelPrimitive.Index);

        public MyVector3ISet.Enumerator GetCubes() => 
            this.m_cubeSet.GetEnumerator();

        public void GetCubeTriangles(Vector3I gridPos, List<MyNavigationTriangle> trianglesOut)
        {
            List<int> list = null;
            if (this.m_smallTriangleRegistry.TryGetValue(gridPos, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    trianglesOut.Add(base.GetTriangle(list[i]));
                }
            }
        }

        public override MyHighLevelPrimitive GetHighLevelPrimitive(MyNavigationPrimitive myNavigationTriangle) => 
            this.m_higherLevelHelper.GetHighLevelNavigationPrimitive(myNavigationTriangle as MyNavigationTriangle);

        public override MatrixD GetWorldMatrix()
        {
            MatrixD worldMatrix = this.m_grid.WorldMatrix;
            MatrixD.Rescale(ref worldMatrix, this.m_grid.GridSize);
            return worldMatrix;
        }

        public override Vector3 GlobalToLocal(Vector3D globalPos) => 
            ((Vector3) (Vector3D.Transform(globalPos, this.m_grid.PositionComp.WorldMatrixNormalizedInv) / this.m_grid.GridSize));

        private void grid_OnBlockAdded(MySlimBlock block)
        {
            this.OnBlockAddedInternal(block);
        }

        private void grid_OnBlockRemoved(MySlimBlock block)
        {
            bool flag = true;
            bool flag2 = false;
            bool flag3 = false;
            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(block.Position);
            MyCompoundCubeBlock block3 = (cubeBlock == null) ? null : (cubeBlock.FatBlock as MyCompoundCubeBlock);
            if ((block.FatBlock is MyCompoundCubeBlock) || (block.BlockDefinition.NavigationDefinition != null))
            {
                BoundingBoxD xd;
                if (block3 == null)
                {
                    flag = false;
                    if (cubeBlock != null)
                    {
                        if (block.BlockDefinition.NavigationDefinition.NoEntry)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag3 = true;
                        }
                    }
                }
                else
                {
                    ListReader<MySlimBlock> blocks = block3.GetBlocks();
                    if (blocks.Count != 0)
                    {
                        foreach (MySlimBlock block4 in blocks)
                        {
                            if (block4.BlockDefinition.NavigationDefinition != null)
                            {
                                if (block4.BlockDefinition.NavigationDefinition.NoEntry | flag3)
                                {
                                    flag = false;
                                    flag2 = true;
                                    break;
                                }
                                flag = false;
                                flag3 = true;
                                block = block4;
                            }
                        }
                    }
                }
                block.GetWorldBoundingBox(out xd, false);
                xd.Inflate((double) 5.0999999046325684);
                this.m_coordinator.InvalidateVoxelsBBox(ref xd);
                this.MarkBlockChanged(block);
                MyCestmirPathfindingShorts.Pathfinding.GridPathfinding.MarkHighLevelDirty();
                if (flag)
                {
                    this.RemoveBlock(block.Min, block.Max, true);
                    this.FixBlockFaces(block);
                }
                else if (flag2)
                {
                    this.RemoveBlock(block.Min, block.Max, false);
                }
                else if (flag3)
                {
                    this.RemoveBlock(block.Min, block.Max, true);
                    this.AddBlock(block);
                }
                else if (this.m_cubeSet.Contains(block.Position))
                {
                    this.RemoveBlock(block.Min, block.Max, true);
                    this.FixBlockFaces(block);
                }
            }
        }

        private bool IsFaceTriangle(MyNavigationTriangle triangle, Vector3I cubePosition, Vector3I direction)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = triangle.GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            vertexEnumerator.MoveNext();
            vertexEnumerator.MoveNext();
            cubePosition *= 0x100;
            Vector3I vectori4 = (Vector3I) (cubePosition + (direction * 0x80));
            Vector3I vectori = Vector3I.Round(vertexEnumerator.Current * 256f) - vectori4;
            Vector3I vectori2 = Vector3I.Round(vertexEnumerator.Current * 256f) - vectori4;
            Vector3I vectori3 = Vector3I.Round(vertexEnumerator.Current * 256f) - vectori4;
            return (!((vectori * direction) != Vector3I.Zero) ? (!((vectori2 * direction) != Vector3I.Zero) ? (!((vectori3 * direction) != Vector3I.Zero) ? ((vectori.AbsMax() <= 0x80) && ((vectori2.AbsMax() <= 0x80) && (vectori3.AbsMax() <= 0x80))) : false) : false) : false);
        }

        public override Vector3D LocalToGlobal(Vector3 localPos)
        {
            localPos *= this.m_grid.GridSize;
            return Vector3D.Transform(localPos, this.m_grid.WorldMatrix);
        }

        public void MakeStatic()
        {
            if (!this.m_static)
            {
                this.m_static = true;
                this.m_connectionHelper = null;
                this.m_cubeSet = null;
            }
        }

        private void MarkBlockChanged(MySlimBlock block)
        {
            this.m_higherLevelHelper.MarkBlockChanged(block);
            MyCestmirPathfindingShorts.Pathfinding.GridPathfinding.MarkHighLevelDirty();
        }

        private void MergeFromAnotherMesh(MyGridNavigationMesh otherMesh, ref MatrixI transform)
        {
            int num;
            m_mergeHelper.Clear();
            foreach (Vector3I vectori in otherMesh.m_smallTriangleRegistry.Keys)
            {
                bool flag = false;
                Vector3I[] intDirections = Base6Directions.IntDirections;
                num = 0;
                while (true)
                {
                    if (num >= intDirections.Length)
                    {
                        if (flag)
                        {
                            m_mergeHelper.Add(vectori);
                        }
                        break;
                    }
                    Vector3I vectori2 = intDirections[num];
                    Vector3I position = Vector3I.Transform((Vector3I) (vectori + vectori2), (MatrixI) transform);
                    if (this.m_cubeSet.Contains(ref position))
                    {
                        m_mergeHelper.Add(vectori + vectori2);
                        flag = true;
                    }
                    num++;
                }
            }
            using (Dictionary<Vector3I, List<int>>.Enumerator enumerator2 = otherMesh.m_smallTriangleRegistry.GetEnumerator())
            {
                while (true)
                {
                    while (true)
                    {
                        if (enumerator2.MoveNext())
                        {
                            Vector3I vectori5;
                            KeyValuePair<Vector3I, List<int>> current = enumerator2.Current;
                            Vector3I key = current.Key;
                            Vector3I.Transform(ref key, ref transform, out vectori5);
                            if (m_mergeHelper.Contains(key))
                            {
                                m_tmpTriangleList.Clear();
                                Base6Directions.Direction[] enumDirections = Base6Directions.EnumDirections;
                                num = 0;
                                while (true)
                                {
                                    if (num < enumDirections.Length)
                                    {
                                        Base6Directions.Direction direction = enumDirections[num];
                                        Vector3I intVector = Base6Directions.GetIntVector((int) direction);
                                        Vector3I vectori7 = Base6Directions.GetIntVector((int) Base6Directions.GetFlippedDirection(transform.GetDirection(direction)));
                                        if (m_mergeHelper.Contains(key + intVector))
                                        {
                                            List<int> list = null;
                                            if (this.m_smallTriangleRegistry.TryGetValue(vectori5 - vectori7, out list))
                                            {
                                                foreach (int num3 in list)
                                                {
                                                    MyNavigationTriangle triangle = base.GetTriangle(num3);
                                                    if (this.IsFaceTriangle(triangle, vectori5 - vectori7, vectori7))
                                                    {
                                                        m_tmpTriangleList.Add(new KeyValuePair<MyNavigationTriangle, Vector3I>(triangle, vectori5 - vectori7));
                                                    }
                                                }
                                            }
                                        }
                                        num++;
                                        continue;
                                    }
                                    foreach (KeyValuePair<MyNavigationTriangle, Vector3I> pair2 in m_tmpTriangleList)
                                    {
                                        this.RemoveTriangle(pair2.Key, pair2.Value);
                                    }
                                    m_tmpTriangleList.Clear();
                                    int num2 = 0;
                                    foreach (int num4 in current.Value)
                                    {
                                        MyNavigationTriangle triangle = otherMesh.GetTriangle(num4);
                                        Vector3I cubePosition = current.Key;
                                        bool flag2 = true;
                                        enumDirections = Base6Directions.EnumDirections;
                                        num = 0;
                                        while (true)
                                        {
                                            if (num < enumDirections.Length)
                                            {
                                                Vector3I intVector = Base6Directions.GetIntVector((int) enumDirections[num]);
                                                if (!m_mergeHelper.Contains(cubePosition + intVector) || !this.IsFaceTriangle(triangle, cubePosition, intVector))
                                                {
                                                    num++;
                                                    continue;
                                                }
                                                flag2 = false;
                                            }
                                            if (flag2)
                                            {
                                                int num6 = num2;
                                                this.CopyTriangle(triangle, cubePosition, ref transform);
                                                num2++;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            foreach (int num5 in current.Value)
                            {
                                MyNavigationTriangle triangle = otherMesh.GetTriangle(num5);
                                this.CopyTriangle(triangle, current.Key, ref transform);
                            }
                        }
                        else
                        {
                            goto TR_0000;
                        }
                    }
                }
            }
        TR_0000:
            m_mergeHelper.Clear();
        }

        private unsafe void OnBlockAddedInternal(MySlimBlock block)
        {
            MyCompoundCubeBlock fatBlock = this.m_grid.GetCubeBlock(block.Position).FatBlock as MyCompoundCubeBlock;
            if ((block.FatBlock is MyCompoundCubeBlock) || (block.BlockDefinition.NavigationDefinition != null))
            {
                bool flag = false;
                bool flag2 = false;
                if (fatBlock != null)
                {
                    ListReader<MySlimBlock> blocks = fatBlock.GetBlocks();
                    if (blocks.Count == 0)
                    {
                        return;
                    }
                    foreach (MySlimBlock block3 in blocks)
                    {
                        if (block3.BlockDefinition.NavigationDefinition != null)
                        {
                            if (block3.BlockDefinition.NavigationDefinition.NoEntry | flag2)
                            {
                                flag2 = false;
                                flag = true;
                                break;
                            }
                            block = block3;
                            flag2 = true;
                        }
                    }
                }
                else if (block.BlockDefinition.NavigationDefinition != null)
                {
                    if (!block.BlockDefinition.NavigationDefinition.NoEntry)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                        flag = true;
                    }
                }
                if (flag || flag2)
                {
                    BoundingBoxD xd;
                    if (!flag)
                    {
                        if (this.m_cubeSet.Contains(block.Position))
                        {
                            this.RemoveBlock(block.Min, block.Max, true);
                        }
                        this.AddBlock(block);
                    }
                    else
                    {
                        if (this.m_cubeSet.Contains(block.Position))
                        {
                            this.RemoveBlock(block.Min, block.Max, true);
                        }
                        Vector3I start = new Vector3I {
                            X = block.Min.X
                        };
                        while (true)
                        {
                            if (start.X > block.Max.X)
                            {
                                start.Y = block.Min.Y;
                                while (true)
                                {
                                    if (start.Y > block.Max.Y)
                                    {
                                        start = block.Min;
                                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref block.Max);
                                        while (iterator.IsValid())
                                        {
                                            this.m_cubeSet.Add(start);
                                            iterator.GetNext(out start);
                                        }
                                        break;
                                    }
                                    start.Z = block.Min.Z;
                                    while (true)
                                    {
                                        if (start.Z > block.Max.Z)
                                        {
                                            int* numPtr5 = (int*) ref start.Y;
                                            numPtr5[0]++;
                                            break;
                                        }
                                        start.X = block.Min.X - 1;
                                        if (this.m_cubeSet.Contains(ref start))
                                        {
                                            this.EraseFaceTriangles(start, Base6Directions.Direction.Right);
                                        }
                                        start.X = block.Max.X + 1;
                                        if (this.m_cubeSet.Contains(ref start))
                                        {
                                            this.EraseFaceTriangles(start, Base6Directions.Direction.Left);
                                        }
                                        int* numPtr4 = (int*) ref start.Z;
                                        numPtr4[0]++;
                                    }
                                }
                                break;
                            }
                            start.Y = block.Min.Y;
                            while (true)
                            {
                                if (start.Y > block.Max.Y)
                                {
                                    start.Z = block.Min.Z;
                                    while (true)
                                    {
                                        if (start.Z > block.Max.Z)
                                        {
                                            int* numPtr3 = (int*) ref start.X;
                                            numPtr3[0]++;
                                            break;
                                        }
                                        start.Y = block.Min.Y - 1;
                                        if (this.m_cubeSet.Contains(ref start))
                                        {
                                            this.EraseFaceTriangles(start, Base6Directions.Direction.Up);
                                        }
                                        start.Y = block.Max.Y + 1;
                                        if (this.m_cubeSet.Contains(ref start))
                                        {
                                            this.EraseFaceTriangles(start, Base6Directions.Direction.Down);
                                        }
                                        int* numPtr2 = (int*) ref start.Z;
                                        numPtr2[0]++;
                                    }
                                    break;
                                }
                                start.Z = block.Min.Z - 1;
                                if (this.m_cubeSet.Contains(ref start))
                                {
                                    this.EraseFaceTriangles(start, Base6Directions.Direction.Backward);
                                }
                                start.Z = block.Max.Z + 1;
                                if (this.m_cubeSet.Contains(ref start))
                                {
                                    this.EraseFaceTriangles(start, Base6Directions.Direction.Forward);
                                }
                                int* numPtr1 = (int*) ref start.Y;
                                numPtr1[0]++;
                            }
                        }
                    }
                    block.GetWorldBoundingBox(out xd, false);
                    xd.Inflate((double) 5.0999999046325684);
                    this.m_coordinator.InvalidateVoxelsBBox(ref xd);
                    this.MarkBlockChanged(block);
                }
            }
        }

        public void RegisterTriangle(MyNavigationTriangle tri, ref Vector3I gridPos)
        {
            if (this.m_grid == null)
            {
                this.RegisterTriangleInternal(tri, ref gridPos);
            }
        }

        private void RegisterTriangleInternal(MyNavigationTriangle tri, ref Vector3I gridPos)
        {
            List<int> list = null;
            if (!this.m_smallTriangleRegistry.TryGetValue(gridPos, out list))
            {
                list = new List<int>();
                this.m_smallTriangleRegistry.Add(gridPos, list);
            }
            list.Add(tri.Index);
            tri.Registered = true;
        }

        private void RemoveAndAddTriangle(ref Vector3I positionA, ref Vector3I positionB, int registeredEdgeIndex)
        {
            MyNavigationTriangle edgeTriangle = base.GetEdgeTriangle(registeredEdgeIndex);
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = edgeTriangle.GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            Vector3 current = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            vertexEnumerator.MoveNext();
            Vector3I cube = this.FindTriangleCube(edgeTriangle.Index, ref positionA, ref positionB);
            this.RemoveTriangle(edgeTriangle, cube);
            MyNavigationTriangle tri = this.AddTriangleInternal(current, vertexEnumerator.Current, vertexEnumerator.Current);
            this.RegisterTriangleInternal(tri, ref cube);
        }

        private void RemoveBlock(Vector3I min, Vector3I max, bool eraseCubeSet)
        {
            Vector3I start = min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref max);
            while (iterator.IsValid())
            {
                if (eraseCubeSet)
                {
                    this.m_cubeSet.Remove(ref start);
                }
                this.EraseCubeTriangles(start);
                iterator.GetNext(out start);
            }
        }

        private void RemoveTriangle(MyNavigationTriangle triangle, Vector3I cube)
        {
            int num4;
            int num5;
            int num6;
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = triangle.GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            Vector3I pointA = Vector3I.Round(vertexEnumerator.Current * 256f);
            vertexEnumerator.MoveNext();
            Vector3I pointB = Vector3I.Round(vertexEnumerator.Current * 256f);
            vertexEnumerator.MoveNext();
            Vector3I vectori3 = Vector3I.Round(vertexEnumerator.Current * 256f);
            int edgeIndex = triangle.GetEdgeIndex(0);
            int num2 = triangle.GetEdgeIndex(1);
            int num3 = triangle.GetEdgeIndex(2);
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref pointA, ref vectori3), out num4))
            {
                num4 = -1;
            }
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref vectori3, ref pointB), out num5))
            {
                num5 = -1;
            }
            if (!this.m_connectionHelper.TryGetValue(new EdgeIndex(ref pointB, ref pointA), out num6))
            {
                num6 = -1;
            }
            if ((num4 == -1) || (num3 != num4))
            {
                this.m_connectionHelper.Add(new EdgeIndex(vectori3, pointA), num3);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref pointA, ref vectori3));
            }
            if ((num5 == -1) || (num2 != num5))
            {
                this.m_connectionHelper.Add(new EdgeIndex(pointB, vectori3), num2);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref vectori3, ref pointB));
            }
            if ((num6 == -1) || (edgeIndex != num6))
            {
                this.m_connectionHelper.Add(new EdgeIndex(pointA, pointB), edgeIndex);
            }
            else
            {
                this.m_connectionHelper.Remove(new EdgeIndex(ref pointB, ref pointA));
            }
            List<int> list = null;
            this.m_smallTriangleRegistry.TryGetValue(cube, out list);
            int index = 0;
            while (true)
            {
                if (index < list.Count)
                {
                    if (list[index] != triangle.Index)
                    {
                        index++;
                        continue;
                    }
                    list.RemoveAtFast<int>(index);
                }
                if (list.Count == 0)
                {
                    this.m_smallTriangleRegistry.Remove(cube);
                }
                base.RemoveTriangle(triangle);
                if ((num4 != -1) && (num3 != num4))
                {
                    this.RemoveAndAddTriangle(ref pointA, ref vectori3, num4);
                }
                if ((num5 != -1) && (num2 != num5))
                {
                    this.RemoveAndAddTriangle(ref vectori3, ref pointB, num5);
                }
                if ((num6 != -1) && (edgeIndex != num6))
                {
                    this.RemoveAndAddTriangle(ref pointB, ref pointA, num6);
                }
                return;
            }
        }

        public override string ToString() => 
            ("Grid NavMesh: " + this.m_grid.DisplayName);

        public void UpdateHighLevel()
        {
            this.m_higherLevelHelper.ProcessChangedCellComponents();
        }

        public bool HighLevelDirty =>
            this.m_higherLevelHelper.IsDirty;

        public override MyHighLevelGroup HighLevelGroup =>
            this.m_higherLevel;

        public class Component : IMyHighLevelComponent
        {
            private MyGridNavigationMesh m_parent;
            private int m_componentIndex;

            public Component(MyGridNavigationMesh parent, int componentIndex)
            {
                this.m_parent = parent;
                this.m_componentIndex = componentIndex;
            }

            public bool Contains(MyNavigationPrimitive primitive)
            {
                if (!ReferenceEquals(primitive.Group, this.m_parent))
                {
                    return false;
                }
                MyNavigationTriangle triangle = primitive as MyNavigationTriangle;
                return ((triangle != null) ? (triangle.ComponentIndex == this.m_componentIndex) : false);
            }

            public bool FullyExplored =>
                true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EdgeIndex : IEquatable<MyGridNavigationMesh.EdgeIndex>
        {
            public Vector3I A;
            public Vector3I B;
            public EdgeIndex(Vector3I PointA, Vector3I PointB)
            {
                this.A = PointA;
                this.B = PointB;
            }

            public EdgeIndex(ref Vector3I PointA, ref Vector3I PointB)
            {
                this.A = PointA;
                this.B = PointB;
            }

            public override int GetHashCode() => 
                ((this.A.GetHashCode() * 0x60000005) + this.B.GetHashCode());

            public override bool Equals(object obj) => 
                ((obj is MyGridNavigationMesh.EdgeIndex) ? this.Equals((MyGridNavigationMesh.EdgeIndex) obj) : false);

            public override string ToString()
            {
                string[] textArray1 = new string[] { "(", this.A.ToString(), ", ", this.B.ToString(), ")" };
                return string.Concat(textArray1);
            }

            public bool Equals(MyGridNavigationMesh.EdgeIndex other) => 
                ((this.A == other.A) && (this.B == other.B));
        }
    }
}

