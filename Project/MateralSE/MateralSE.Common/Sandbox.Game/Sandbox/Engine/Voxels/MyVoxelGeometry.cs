namespace Sandbox.Engine.Voxels
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.Game.Voxels;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyVoxelGeometry
    {
        private static List<Vector3I> m_sweepResultCache = new List<Vector3I>();
        private static List<int> m_overlapElementCache = new List<int>();
        private MyStorageBase m_storage;
        private Vector3I m_cellsCount;
        private readonly Dictionary<ulong, CellData> m_cellsByCoordinate = new Dictionary<ulong, CellData>();
        private readonly Dictionary<ulong, MyIsoMesh> m_coordinateToMesh = new Dictionary<ulong, MyIsoMesh>();
        private readonly FastResourceLock m_lock = new FastResourceLock();
        private readonly LRUCache<ulong, ulong> m_isEmptyCache = new LRUCache<ulong, ulong>(0x80, null);

        private void ClampCellCoord(ref Vector3I cellCoord)
        {
            Vector3I max = this.m_cellsCount - 1;
            Vector3I.Clamp(ref cellCoord, ref Vector3I.Zero, ref max, out cellCoord);
        }

        private unsafe void ComputeIsEmptyLookup(MyCellCoord cell, out ulong outCacheKey, out int outBit)
        {
            Vector3I vectori = cell.CoordInLod % 4;
            Vector3I* vectoriPtr1 = (Vector3I*) ref cell.CoordInLod;
            vectoriPtr1[0] = vectoriPtr1[0] >> 2;
            outCacheKey = cell.PackId64();
            outBit = vectori.X + (4 * (vectori.Y + (4 * vectori.Z)));
        }

        internal CellData GetCell(ref MyCellCoord cell)
        {
            bool flag;
            CellData data;
            if (!this.TryGetCell(cell, out flag, out data))
            {
                MyIsoMesh mesh;
                if (!this.TryGetMesh(cell, out flag, out mesh))
                {
                    Vector3I lodVoxelMin = cell.CoordInLod << 3;
                    lodVoxelMin -= 1;
                    mesh = MyPrecalcComponent.IsoMesher.Precalc(this.m_storage, 0, lodVoxelMin, (Vector3I) ((lodVoxelMin + 8) + 2), MyStorageDataTypeFlags.Content, 0);
                }
                if (mesh != null)
                {
                    data = new CellData();
                    data.Init((Vector3) mesh.PositionOffset, mesh.PositionScale, mesh.Positions.GetInternalArray<Vector3>(), mesh.VerticesCount, mesh.Triangles.GetInternalArray<MyVoxelTriangle>(), mesh.TrianglesCount);
                }
                if (cell.Lod == 0)
                {
                    using (this.m_lock.AcquireExclusiveUsing())
                    {
                        if (data == null)
                        {
                            this.SetEmpty(ref cell, true);
                        }
                        else
                        {
                            ulong num = cell.PackId64();
                            this.m_cellsByCoordinate[num] = data;
                        }
                    }
                }
            }
            return data;
        }

        private void GetCellLineIntersectionOctree(ref MyIntersectionResultLineTriangle? result, ref Line modelSpaceLine, ref float? minDistanceUntilNow, CellData cachedDataCell, IntersectionFlags flags)
        {
            m_overlapElementCache.Clear();
            if (cachedDataCell.Octree != null)
            {
                Vector3 vector;
                Vector3 vector2;
                cachedDataCell.GetPackedPosition(ref modelSpaceLine.From, out vector);
                cachedDataCell.GetPackedPosition(ref modelSpaceLine.To, out vector2);
                Ray ray = new Ray(vector, vector2 - vector);
                cachedDataCell.Octree.GetIntersectionWithLine(ref ray, m_overlapElementCache);
            }
            for (int i = 0; i < m_overlapElementCache.Count; i++)
            {
                int index = m_overlapElementCache[i];
                if ((cachedDataCell.VoxelTriangles != null) && (index < cachedDataCell.VoxelTriangles.Length))
                {
                    MyTriangle_Vertices vertices;
                    MyVoxelTriangle triangle = cachedDataCell.VoxelTriangles[index];
                    cachedDataCell.GetUnpackedPosition(triangle.V0, out vertices.Vertex0);
                    cachedDataCell.GetUnpackedPosition(triangle.V1, out vertices.Vertex1);
                    cachedDataCell.GetUnpackedPosition(triangle.V2, out vertices.Vertex2);
                    Vector3 normalVectorFromTriangle = MyUtils.GetNormalVectorFromTriangle(ref vertices);
                    if (normalVectorFromTriangle.IsValid() && (((flags & IntersectionFlags.FLIPPED_TRIANGLES) != ((IntersectionFlags) 0)) || (Vector3.Dot(modelSpaceLine.Direction, normalVectorFromTriangle) <= 0f)))
                    {
                        float? lineTriangleIntersection = MyUtils.GetLineTriangleIntersection(ref modelSpaceLine, ref vertices);
                        if ((lineTriangleIntersection != null) && ((result == 0) || (lineTriangleIntersection.Value < result.Value.Distance)))
                        {
                            minDistanceUntilNow = new float?(lineTriangleIntersection.Value);
                            result = new MyIntersectionResultLineTriangle(0, ref vertices, ref normalVectorFromTriangle, lineTriangleIntersection.Value);
                        }
                    }
                }
            }
        }

        public void Init(MyStorageBase storage)
        {
            this.m_storage = storage;
            this.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChanged);
            Vector3I size = this.m_storage.Size;
            this.m_cellsCount.X = size.X >> 3;
            this.m_cellsCount.Y = size.Y >> 3;
            this.m_cellsCount.Z = size.Z >> 3;
        }

        public bool Intersect(ref Line localLine, out MyIntersectionResultLineTriangle result, IntersectionFlags flags)
        {
            MyIntersectionResultLineTriangle valueOrDefault;
            m_sweepResultCache.Clear();
            MyGridIntersection.Calculate(m_sweepResultCache, 8f, localLine.From, localLine.To, new Vector3I(0, 0, 0), this.m_cellsCount - 1);
            float? minDistanceUntilNow = null;
            MyCellCoord cell = new MyCellCoord();
            MyIntersectionResultLineTriangle? nullable2 = null;
            for (int i = 0; i < m_sweepResultCache.Count; i++)
            {
                BoundingBox box;
                cell.CoordInLod = m_sweepResultCache[i];
                MyVoxelCoordSystems.GeometryCellCoordToLocalAABB(ref cell.CoordInLod, out box);
                float? lineBoundingBoxIntersection = MyUtils.GetLineBoundingBoxIntersection(ref localLine, ref box);
                if ((minDistanceUntilNow != null) && (lineBoundingBoxIntersection != null))
                {
                    float? nullable1;
                    float? nullable5 = minDistanceUntilNow;
                    float num3 = 15.58846f;
                    if (nullable5 != null)
                    {
                        nullable1 = new float?(nullable5.GetValueOrDefault() + num3);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    float? nullable4 = nullable1;
                    float num2 = lineBoundingBoxIntersection.Value;
                    if ((nullable4.GetValueOrDefault() < num2) & (nullable4 != null))
                    {
                        break;
                    }
                }
                CellData cachedDataCell = this.GetCell(ref cell);
                if ((cachedDataCell != null) && (cachedDataCell.VoxelTrianglesCount != 0))
                {
                    this.GetCellLineIntersectionOctree(ref nullable2, ref localLine, ref minDistanceUntilNow, cachedDataCell, flags);
                }
            }
            MyIntersectionResultLineTriangle? nullable7 = nullable2;
            if (nullable7 != null)
            {
                valueOrDefault = nullable7.GetValueOrDefault();
            }
            else
            {
                valueOrDefault = new MyIntersectionResultLineTriangle();
            }
            result = valueOrDefault;
            return (nullable2 != null);
        }

        public unsafe bool Intersects(ref BoundingSphere localSphere)
        {
            Vector3I vectori;
            Vector3I vectori2;
            BoundingBox box = BoundingBox.CreateInvalid();
            box.Include(ref localSphere);
            Vector3 min = box.Min;
            Vector3 max = box.Max;
            MyVoxelCoordSystems.LocalPositionToGeometryCellCoord(ref min, out vectori);
            MyVoxelCoordSystems.LocalPositionToGeometryCellCoord(ref max, out vectori2);
            this.ClampCellCoord(ref vectori);
            this.ClampCellCoord(ref vectori2);
            MyCellCoord cell = new MyCellCoord {
                CoordInLod = { X = vectori.X }
            };
            while (cell.CoordInLod.X <= vectori2.X)
            {
                cell.CoordInLod.Y = vectori.Y;
                while (true)
                {
                    if (cell.CoordInLod.Y > vectori2.Y)
                    {
                        int* numPtr3 = (int*) ref cell.CoordInLod.X;
                        numPtr3[0]++;
                        break;
                    }
                    cell.CoordInLod.Z = vectori.Z;
                    while (true)
                    {
                        BoundingBox box2;
                        if (cell.CoordInLod.Z > vectori2.Z)
                        {
                            int* numPtr2 = (int*) ref cell.CoordInLod.Y;
                            numPtr2[0]++;
                            break;
                        }
                        MyVoxelCoordSystems.GeometryCellCoordToLocalAABB(ref cell.CoordInLod, out box2);
                        if (box2.Intersects(ref localSphere))
                        {
                            CellData data = this.GetCell(ref cell);
                            if (data != null)
                            {
                                for (int i = 0; i < data.VoxelTrianglesCount; i++)
                                {
                                    MyTriangle_Vertices vertices;
                                    MyVoxelTriangle triangle = data.VoxelTriangles[i];
                                    data.GetUnpackedPosition(triangle.V0, out vertices.Vertex0);
                                    data.GetUnpackedPosition(triangle.V1, out vertices.Vertex1);
                                    data.GetUnpackedPosition(triangle.V2, out vertices.Vertex2);
                                    BoundingBox box3 = BoundingBox.CreateInvalid();
                                    box3.Include(ref vertices.Vertex0);
                                    box3.Include(ref vertices.Vertex1);
                                    box3.Include(ref vertices.Vertex2);
                                    if (box3.Intersects(ref localSphere))
                                    {
                                        Plane trianglePlane = new Plane(vertices.Vertex0, vertices.Vertex1, vertices.Vertex2);
                                        if (MyUtils.GetSphereTriangleIntersection(ref localSphere, ref trianglePlane, ref vertices) != null)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        int* numPtr1 = (int*) ref cell.CoordInLod.Z;
                        numPtr1[0]++;
                    }
                }
            }
            return false;
        }

        private bool IsEmpty(ref MyCellCoord cell)
        {
            ulong num;
            int num2;
            this.ComputeIsEmptyLookup(cell, out num, out num2);
            return ((this.m_isEmptyCache.Read(num) & (1L << (num2 & 0x3f))) != 0L);
        }

        private void RemoveEmpty(ref MyCellCoord cell)
        {
            ulong num;
            int num2;
            this.ComputeIsEmptyLookup(cell, out num, out num2);
            this.m_isEmptyCache.Remove(num);
        }

        private void SetEmpty(ref MyCellCoord cell, bool value)
        {
            ulong num;
            int num2;
            this.ComputeIsEmptyLookup(cell, out num, out num2);
            ulong num3 = this.m_isEmptyCache.Read(num);
            num3 = !value ? (num3 & ((ulong) ~(1L << (num2 & 0x3f)))) : (num3 | ((ulong) (1L << (num2 & 0x3f))));
            this.m_isEmptyCache.Write(num, num3);
        }

        public void SetMesh(MyCellCoord cell, MyIsoMesh mesh)
        {
            if (cell.Lod == 0)
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    if (mesh == null)
                    {
                        this.SetEmpty(ref cell, true);
                    }
                    else
                    {
                        ulong num = cell.PackId64();
                        this.m_coordinateToMesh[num] = mesh;
                    }
                }
            }
        }

        private unsafe void storage_RangeChanged(Vector3I minChanged, Vector3I maxChanged, MyStorageDataTypeFlags changedData)
        {
            Vector3I voxelCoord = minChanged - MyPrecalcComponent.InvalidatedRangeInflate;
            Vector3I vectori2 = (Vector3I) (maxChanged + MyPrecalcComponent.InvalidatedRangeInflate);
            this.m_storage.ClampVoxelCoord(ref voxelCoord, 1);
            this.m_storage.ClampVoxelCoord(ref vectori2, 1);
            Vector3I start = voxelCoord >> 3;
            Vector3I end = vectori2 >> 3;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if ((start == Vector3I.Zero) && (end == (this.m_cellsCount - 1)))
                {
                    this.m_cellsByCoordinate.Clear();
                    this.m_coordinateToMesh.Clear();
                    this.m_isEmptyCache.Reset();
                }
                else
                {
                    MyCellCoord cell = new MyCellCoord();
                    if ((this.m_cellsByCoordinate.Count > 0) || (this.m_coordinateToMesh.Count > 0))
                    {
                        cell.CoordInLod = start;
                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                        while (iterator.IsValid())
                        {
                            ulong key = cell.PackId64();
                            this.m_cellsByCoordinate.Remove(key);
                            this.m_coordinateToMesh.Remove(key);
                            iterator.GetNext(out cell.CoordInLod);
                        }
                    }
                    if ((end - start).Volume() <= 0x186a0)
                    {
                        Vector3I_RangeIterator iterator3 = new Vector3I_RangeIterator(ref start, ref end);
                        while (iterator3.IsValid())
                        {
                            this.SetEmpty(ref cell, false);
                            iterator3.GetNext(out cell.CoordInLod);
                        }
                    }
                    else
                    {
                        Vector3I vectori6 = start >> 2;
                        Vector3I vectori7 = (Vector3I) ((end >> 2) + 1);
                        cell.CoordInLod = vectori6;
                        Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref vectori6, ref vectori7);
                        while (iterator2.IsValid())
                        {
                            Vector3I* vectoriPtr1 = (Vector3I*) ref cell.CoordInLod;
                            vectoriPtr1[0] = vectoriPtr1[0] << 2;
                            this.RemoveEmpty(ref cell);
                            iterator2.GetNext(out cell.CoordInLod);
                        }
                    }
                }
            }
        }

        private bool TryGetCell(MyCellCoord cell, out bool isEmpty, out CellData nonEmptyCell)
        {
            bool flag;
            using (this.m_lock.AcquireSharedUsing())
            {
                if (this.IsEmpty(ref cell))
                {
                    isEmpty = true;
                    nonEmptyCell = null;
                    flag = true;
                }
                else
                {
                    ulong key = cell.PackId64();
                    if (this.m_cellsByCoordinate.TryGetValue(key, out nonEmptyCell))
                    {
                        isEmpty = false;
                        flag = true;
                    }
                    else
                    {
                        isEmpty = false;
                        nonEmptyCell = null;
                        flag = false;
                    }
                }
            }
            return flag;
        }

        public bool TryGetMesh(MyCellCoord cell, out bool isEmpty, out MyIsoMesh nonEmptyMesh)
        {
            bool flag;
            using (this.m_lock.AcquireSharedUsing())
            {
                if (this.IsEmpty(ref cell))
                {
                    isEmpty = true;
                    nonEmptyMesh = null;
                    flag = true;
                }
                else
                {
                    ulong key = cell.PackId64();
                    if (this.m_coordinateToMesh.TryGetValue(key, out nonEmptyMesh))
                    {
                        isEmpty = false;
                        flag = true;
                    }
                    else
                    {
                        isEmpty = false;
                        nonEmptyMesh = null;
                        flag = false;
                    }
                }
            }
            return flag;
        }

        public Vector3I CellsCount =>
            this.m_cellsCount;

        public class CellData
        {
            public int VoxelTrianglesCount;
            public int VoxelVerticesCount;
            public MyVoxelTriangle[] VoxelTriangles;
            private Vector3 m_positionOffset;
            private Vector3 m_positionScale;
            private Vector3[] m_positions;
            private MyOctree m_octree;

            public void GetPackedPosition(ref Vector3 unpacked, out Vector3 packed)
            {
                packed = (unpacked - this.m_positionOffset) / this.m_positionScale;
            }

            public void GetUnpackedPosition(int index, out Vector3 unpacked)
            {
                unpacked = (this.m_positions[index] * this.m_positionScale) + this.m_positionOffset;
            }

            public void Init(Vector3 positionOffset, Vector3 positionScale, Vector3[] positions, int vertexCount, MyVoxelTriangle[] triangles, int triangleCount)
            {
                if (vertexCount == 0)
                {
                    this.VoxelVerticesCount = 0;
                    this.VoxelTrianglesCount = 0;
                    this.m_octree = null;
                    this.m_positions = null;
                }
                else
                {
                    this.m_positionOffset = positionOffset;
                    this.m_positionScale = positionScale;
                    this.m_positions = new Vector3[vertexCount];
                    Array.Copy(positions, this.m_positions, vertexCount);
                    if (this.m_octree == null)
                    {
                        this.m_octree = new MyOctree();
                    }
                    this.m_octree.Init(this.m_positions, vertexCount, triangles, triangleCount, out this.VoxelTriangles);
                    this.VoxelVerticesCount = vertexCount;
                    this.VoxelTrianglesCount = triangleCount;
                }
            }

            internal MyOctree Octree
            {
                get
                {
                    if ((this.m_octree == null) && (this.VoxelTrianglesCount > 0))
                    {
                        this.m_octree = new MyOctree();
                        this.m_octree.Init(this.m_positions, this.VoxelVerticesCount, this.VoxelTriangles, this.VoxelTrianglesCount, out this.VoxelTriangles);
                    }
                    return this.m_octree;
                }
            }
        }
    }
}

