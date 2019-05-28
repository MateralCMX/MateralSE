namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.Sewing;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    internal class MyVoxelClipmapRing
    {
        private readonly MyVoxelClipmap m_clipmap;
        private readonly int m_lod;
        private Vector3L m_max;
        private readonly Dictionary<Vector3I, CellData> m_cells = new Dictionary<Vector3I, CellData>(Vector3I.Comparer);
        private readonly HashSet<Vector3I> m_cellsRemove = new HashSet<Vector3I>(Vector3I.Comparer);
        private readonly HashSet<Vector3I> m_cellsAdd = new HashSet<Vector3I>(Vector3I.Comparer);
        private readonly HashSet<Vector3I> m_cellsReStitch = new HashSet<Vector3I>(Vector3I.Comparer);
        internal BoundingBoxI Bounds;
        internal BoundingBoxI InnerBounds;
        internal bool BoundsChanged;

        public MyVoxelClipmapRing(MyVoxelClipmap clipmap, int lod)
        {
            this.m_clipmap = clipmap;
            this.m_lod = lod;
        }

        private void AddCell(Vector3I cell)
        {
            CellData data = new CellData();
            this.m_cells.Add(cell, data);
            this.m_clipmap.RequestCell(cell, this.m_lod, null);
        }

        public void DebugDraw()
        {
            Vector3D translation = MyTransparentGeometry.Camera.Translation;
            int num = this.m_clipmap.CellSize * (2 + (1 << ((this.m_lod + 1) & 0x1f)));
            num *= num;
            Vector4 color = MyClipmap.LodColors[this.m_lod];
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(this.m_clipmap.LocalToWorld, new Color(color - new Vector4(0.2f), 0.07f), true, true))
            {
                using (IMyDebugDrawBatchAabb aabb2 = MyRenderProxy.DebugDrawBatchAABB(this.m_clipmap.LocalToWorld, new Color(color, 0.4f), true, false))
                {
                    foreach (KeyValuePair<Vector3I, CellData> pair in this.m_cells)
                    {
                        if (pair.Value.Guide == null)
                        {
                            continue;
                        }
                        if (pair.Value.Guide.Mesh != null)
                        {
                            Vector3I vectori = (pair.Key << this.m_clipmap.CellBits) << this.m_lod;
                            Vector3I vectori2 = (Vector3I) (vectori + (this.m_clipmap.CellSize << (this.m_lod & 0x1f)));
                            BoundingBoxD xd2 = new BoundingBoxD((Vector3D) vectori, (Vector3D) vectori2);
                            Color? nullable = null;
                            aabb.Add(ref xd2, nullable);
                            nullable = null;
                            aabb2.Add(ref xd2, nullable);
                            Vector3D vectord2 = Vector3D.Transform((Vector3D) (vectori + ((this.m_clipmap.CellSize << (this.m_lod & 0x1f)) >> 1)), this.m_clipmap.LocalToWorld);
                            double num2 = Vector3D.DistanceSquared(vectord2, translation);
                            if (num2 < num)
                            {
                                float a = 1f - (((float) num2) / ((float) num));
                                MyRenderProxy.DebugDrawText3D(vectord2, $"{this.m_lod}:{pair.Key}", new Color(color, a), 0.8f * a, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, -1, false);
                            }
                        }
                    }
                }
            }
        }

        public void DispatchStitchingRefreshes()
        {
            foreach (Vector3I vectori in this.m_cellsReStitch)
            {
                this.m_clipmap.Stitch(new MyCellCoord(this.m_lod, vectori));
            }
            this.m_cellsReStitch.Clear();
        }

        private void DisposeCell(Vector3I coord, CellData data)
        {
            data.Dispose(this.m_clipmap);
            if (data.Cell != null)
            {
                this.m_clipmap.Actor.DeleteCell(data.Cell, false);
                data.Cell = null;
            }
            if (data.Guide != null)
            {
                data.Guide.RemoveReference(this.m_clipmap);
                data.Guide = null;
            }
        }

        public void FinishAdd(Vector3I cell)
        {
            CellData data;
            if (this.m_cells.TryGetValue(cell, out data) && (data.Cell != null))
            {
                data.Cell.SetVisible(true, true);
            }
        }

        public void FinishRemove(Vector3I cell)
        {
            CellData data;
            if (this.m_cells.TryGetValue(cell, out data))
            {
                this.RemoveImmediately(cell, data);
            }
        }

        internal void InvalidateAll()
        {
            foreach (KeyValuePair<Vector3I, CellData> pair in this.m_cells)
            {
                this.DisposeCell(pair.Key, pair.Value);
            }
            this.m_cells.Clear();
            this.Bounds = new BoundingBoxI();
            this.InnerBounds = new BoundingBoxI();
        }

        internal unsafe void InvalidateRange(BoundingBoxI range)
        {
            Vector3I minRange = range.Min >> this.m_lod;
            Vector3I* vectoriPtr1 = (Vector3I*) ref range.Min;
            vectoriPtr1[0] = vectoriPtr1[0] >> (this.m_lod + this.m_clipmap.CellBits);
            Vector3I* vectoriPtr2 = (Vector3I*) ref range.Max;
            vectoriPtr2[0] = (Vector3I) (vectoriPtr2[0] + ((1 << ((this.m_lod + this.m_clipmap.CellBits) & 0x1f)) - 1));
            Vector3I* vectoriPtr3 = (Vector3I*) ref range.Max;
            vectoriPtr3[0] = vectoriPtr3[0] >> (this.m_lod + this.m_clipmap.CellBits);
            range = range.Intersect(this.Bounds);
            foreach (Vector3I vectori2 in BoundingBoxI.EnumeratePoints(range))
            {
                CellData data;
                if (!this.m_cells.TryGetValue(vectori2, out data))
                {
                    continue;
                }
                if (data.Status != CellStatus.MarkedForRemoval)
                {
                    if (data.Guide != null)
                    {
                        data.Guide.InvalidateGenerated(minRange);
                    }
                    if ((this.m_clipmap.ApproximateCellConstitution(vectori2, this.m_lod) != data.Constitution) || (data.Status != CellStatus.Empty))
                    {
                        data.Status = CellStatus.Pending;
                        this.m_clipmap.RequestCell(vectori2, this.m_lod, data.Guide);
                    }
                }
            }
        }

        public bool IsForwardEdge(Vector3I cell) => 
            ((cell.X == (this.Bounds.Max.X - 1)) || ((cell.Y == (this.Bounds.Max.Y - 1)) || (cell.Z == (this.Bounds.Max.Z - 1))));

        internal bool IsInBounds(Vector3I cell) => 
            (this.Bounds.Contains(cell) == ContainmentType.Contains);

        internal bool IsInnerLodEdge(Vector3I cell) => 
            ((cell.X == (this.InnerBounds.Min.X - 1)) || ((cell.Y == (this.InnerBounds.Min.Y - 1)) || (cell.Z == (this.InnerBounds.Min.Z - 1))));

        internal bool IsInnerLodEdge(Vector3I cell, out int innerCornerIndex)
        {
            bool flag = cell.Y == (this.InnerBounds.Min.Y - 1);
            bool flag2 = cell.Z == (this.InnerBounds.Min.Z - 1);
            innerCornerIndex = 0;
            if (cell.X == (this.InnerBounds.Min.X - 1))
            {
                innerCornerIndex |= 1;
            }
            if (flag)
            {
                innerCornerIndex |= 2;
            }
            if (flag2)
            {
                innerCornerIndex |= 4;
            }
            return (innerCornerIndex != 0);
        }

        internal bool IsInsideInnerLod(Vector3I cell) => 
            (this.InnerBounds.Contains(cell) == ContainmentType.Contains);

        public void ProcessChanges()
        {
            foreach (Vector3I vectori in this.m_cellsAdd)
            {
                this.AddCell(vectori);
            }
            this.m_cellsAdd.Clear();
            foreach (Vector3I vectori2 in this.m_cellsRemove)
            {
                this.RemoveCell(vectori2);
            }
            this.m_cellsRemove.Clear();
        }

        private void RemoveCell(Vector3I cell)
        {
            CellData cellData = this.m_cells[cell];
            this.RemoveImmediately(cell, cellData);
        }

        private void RemoveImmediately(Vector3I cell, CellData cellData)
        {
            this.DisposeCell(cell, cellData);
            this.m_cells.Remove(cell);
        }

        internal bool TryGetCell(Vector3I cell, out CellData data) => 
            this.m_cells.TryGetValue(cell, out data);

        public unsafe void Update(Vector3L relativePosition)
        {
            BoundingBoxI innerBounds;
            bool boundsChanged;
            this.BoundsChanged = false;
            Vector3L result = ((relativePosition - this.m_clipmap.Ranges[this.m_lod]) >> this.m_lod) >> this.m_clipmap.CellBits;
            Vector3L vectorl2 = (Vector3L) (((relativePosition + this.m_clipmap.Ranges[this.m_lod]) >> this.m_lod) >> this.m_clipmap.CellBits);
            Vector3L* vectorlPtr1 = (Vector3L*) ref result;
            Vector3L.Clamp(ref (Vector3L) ref vectorlPtr1, ref Vector3L.Zero, ref this.m_max, out result);
            Vector3L* vectorlPtr2 = (Vector3L*) ref vectorl2;
            Vector3L.Clamp(ref (Vector3L) ref vectorlPtr2, ref Vector3L.Zero, ref this.m_max, out vectorl2);
            BoundingBoxI right = new BoundingBoxI((Vector3I) result, (Vector3I) vectorl2);
            BoundingBoxI* xiPtr1 = (BoundingBoxI*) ref right;
            xiPtr1->Min = (right.Min >> 1) << 1;
            BoundingBoxI* xiPtr2 = (BoundingBoxI*) ref right;
            xiPtr2->Max = (Vector3I) (((right.Max + 1) >> 1) << 1);
            if (this.m_lod <= 0)
            {
                innerBounds = this.InnerBounds;
                boundsChanged = false;
            }
            else
            {
                MyVoxelClipmapRing ring = this.m_clipmap.Rings[this.m_lod - 1];
                innerBounds.Min = ring.Bounds.Min >> 1;
                innerBounds.Max = ring.Bounds.Max >> 1;
                boundsChanged = ring.BoundsChanged;
            }
            if (right != this.Bounds)
            {
                foreach (Vector3I vectori in BoundingBoxI.IterateDifference(this.Bounds, right))
                {
                    if (this.m_cells.ContainsKey(vectori))
                    {
                        this.m_cellsRemove.Add(vectori);
                    }
                }
                foreach (Vector3I vectori2 in BoundingBoxI.IterateDifference(right, this.Bounds))
                {
                    if (((this.m_lod <= 0) || !vectori2.IsInside(ref innerBounds.Min, ref innerBounds.Max)) && !this.m_cells.ContainsKey(vectori2))
                    {
                        MyVoxelContentConstitution constitution = this.m_clipmap.ApproximateCellConstitution(vectori2, this.m_lod);
                        if (constitution == MyVoxelContentConstitution.Mixed)
                        {
                            this.m_cellsAdd.Add(vectori2);
                        }
                        else
                        {
                            this.m_cells.Add(vectori2, new CellData(CellStatus.Empty, constitution));
                        }
                    }
                }
                BoundingBoxI left = right.Intersect(this.Bounds);
                BoundingBoxI xi4 = new BoundingBoxI(left.Min, left.Max - 1);
                foreach (Vector3I vectori3 in BoundingBoxI.IterateDifference(left, xi4))
                {
                    CellData data;
                    if (!this.m_cells.TryGetValue(vectori3, out data))
                    {
                        continue;
                    }
                    if (data.Guide != null)
                    {
                        this.m_cellsReStitch.Add(vectori3);
                    }
                }
                this.BoundsChanged = true;
                this.Bounds = right;
            }
            if (boundsChanged)
            {
                BoundingBoxI left = this.Bounds.Intersect(this.InnerBounds);
                foreach (Vector3I vectori4 in BoundingBoxI.IterateDifference(left, innerBounds))
                {
                    MyVoxelContentConstitution constitution = this.m_clipmap.ApproximateCellConstitution(vectori4, this.m_lod);
                    if (constitution == MyVoxelContentConstitution.Mixed)
                    {
                        this.m_cellsAdd.Add(vectori4);
                        continue;
                    }
                    this.m_cells.Add(vectori4, new CellData(CellStatus.Empty, constitution));
                }
                foreach (Vector3I vectori5 in BoundingBoxI.IterateDifference(innerBounds, left))
                {
                    if (this.m_cells.ContainsKey(vectori5))
                    {
                        this.m_cellsRemove.Add(vectori5);
                        this.m_cellsReStitch.Remove(vectori5);
                    }
                }
                BoundingBoxI xi6 = this.Bounds.Intersect(new BoundingBoxI(innerBounds.Min - 1, innerBounds.Max));
                foreach (Vector3I vectori6 in BoundingBoxI.IterateDifference(this.Bounds.Intersect(new BoundingBoxI(this.InnerBounds.Min - 1, this.InnerBounds.Max)), this.InnerBounds).Concat<Vector3I>(BoundingBoxI.IterateDifference(xi6, innerBounds)))
                {
                    CellData data2;
                    if (this.m_cellsRemove.Contains(vectori6))
                    {
                        continue;
                    }
                    if (this.m_cells.TryGetValue(vectori6, out data2) && (data2.Guide != null))
                    {
                        this.m_cellsReStitch.Add(vectori6);
                    }
                }
                this.InnerBounds = innerBounds;
            }
        }

        public void UpdateCellData(Vector3I cell, VrSewGuide guide, MyVoxelContentConstitution constitution)
        {
            CellData data;
            if (this.m_cells.TryGetValue(cell, out data))
            {
                if (((data.Guide != null) && !ReferenceEquals(data.Guide, guide)) && (data.Guide != null))
                {
                    data.Guide.RemoveReference(this.m_clipmap);
                }
                data.Guide = guide;
                data.Constitution = constitution;
                if ((guide != null) && (guide.Mesh != null))
                {
                    data.Status = CellStatus.Calculated;
                }
                else
                {
                    data.Status = CellStatus.Empty;
                    if (data.Cell != null)
                    {
                        this.m_clipmap.Actor.DeleteCell(data.Cell, false);
                        data.Cell = null;
                    }
                }
            }
        }

        public bool UpdateCellRender(Vector3I cell, ref MyVoxelRenderCellData updateData)
        {
            CellData data;
            bool flag = false;
            if (this.m_cells.TryGetValue(cell, out data))
            {
                if ((updateData.Parts == null) || (updateData.Parts.Length == 0))
                {
                    if (data.Cell != null)
                    {
                        this.m_clipmap.Actor.DeleteCell(data.Cell, false);
                        data.Cell = null;
                    }
                }
                else
                {
                    if (data.Cell == null)
                    {
                        data.Cell = this.m_clipmap.Actor.CreateCell((Vector3D) (cell << (this.m_lod + this.m_clipmap.CellBits)), this.m_lod, false);
                        flag = true;
                    }
                    data.Cell.UpdateMesh(ref updateData);
                    data.Status = CellStatus.Ready;
                    data.Cell.SetVisible(true, true);
                }
            }
            return flag;
        }

        internal void UpdateSize(Vector3I size)
        {
            this.m_max = (Vector3L) (((size + 1) >> 1) << 1);
        }

        public int Lod =>
            this.m_lod;

        public DictionaryReader<Vector3I, CellData> Cells =>
            this.m_cells;

        public class CellData
        {
            public IMyVoxelActorCell Cell;
            public MyVoxelClipmapRing.CellStatus Status;
            public MyVoxelContentConstitution Constitution;
            public VrSewGuide Guide;
            public bool Visible;
            public VRage.Voxels.Clipmap.MyVoxelClipmapRing.Vicinity Vicinity;

            public CellData()
            {
                this.Vicinity = VRage.Voxels.Clipmap.MyVoxelClipmapRing.Vicinity.Invalid;
                this.Status = MyVoxelClipmapRing.CellStatus.Pending;
                this.Constitution = MyVoxelContentConstitution.Mixed;
                this.Cell = null;
            }

            public CellData(MyVoxelClipmapRing.CellStatus status, MyVoxelContentConstitution constitution)
            {
                this.Vicinity = VRage.Voxels.Clipmap.MyVoxelClipmapRing.Vicinity.Invalid;
                this.Status = status;
                this.Constitution = constitution;
                this.Cell = null;
            }

            public void Dispose(MyVoxelClipmap clipmap)
            {
            }
        }

        public enum CellStatus : byte
        {
            Pending = 0,
            Calculated = 1,
            Empty = 2,
            Ready = 3,
            MarkedForRemoval = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vicinity
        {
            public readonly uint Lods;
            [FixedBuffer(typeof(sbyte), 8)]
            public <Versions>e__FixedBuffer Versions;
            public static readonly MyVoxelClipmapRing.Vicinity Invalid;
            public unsafe Vicinity(VrSewGuide[] guides, MyCellCoord[] coords)
            {
                this.Lods = 0;
                MyVoxelClipmapRing.Vicinity* vicinityPtr = (MyVoxelClipmapRing.Vicinity*) this;
                for (int i = 0; i < 8; i++)
                {
                    int lod = MyVoxelClipmap.MakeFulfilled(coords[i]).Lod;
                    if (guides[i] == null)
                    {
                        &vicinityPtr->Versions.FixedElementField[i] = -1;
                    }
                    else
                    {
                        this.Lods |= (uint) (lod << ((i * 4) & 0x1f));
                        &vicinityPtr->Versions.FixedElementField[i] = (sbyte) (guides[i].Version & 0x7f);
                    }
                }
                fixed (MyVoxelClipmapRing.Vicinity* vicinityRef = null)
                {
                    return;
                }
            }

            private unsafe Vicinity(bool dummySelector)
            {
                this.Lods = 0;
                MyVoxelClipmapRing.Vicinity* vicinityPtr = (MyVoxelClipmapRing.Vicinity*) this;
                for (int i = 0; i < 8; i++)
                {
                    &vicinityPtr->Versions.FixedElementField[i] = -1;
                }
                fixed (MyVoxelClipmapRing.Vicinity* vicinityRef = null)
                {
                    return;
                }
            }

            public unsafe bool Equals(MyVoxelClipmapRing.Vicinity other)
            {
                MyVoxelClipmapRing.Vicinity* vicinityPtr = (MyVoxelClipmapRing.Vicinity*) this;
                return ((this.Lods == other.Lods) && (*(((long*) &vicinityPtr->Versions.FixedElementField)) == *(((long*) &other.Versions.FixedElementField))));
            }

            public override bool Equals(object obj) => 
                ((obj != null) ? ((obj is MyVoxelClipmapRing.Vicinity) && this.Equals((MyVoxelClipmapRing.Vicinity) obj)) : false);

            public override unsafe int GetHashCode()
            {
                MyVoxelClipmapRing.Vicinity* vicinityPtr = (MyVoxelClipmapRing.Vicinity*) this;
                return (((int) (this.Lods * 0x18d)) ^ ((int) ((ulong) ((IntPtr) &vicinityPtr->Versions.FixedElementField))));
            }

            public static bool operator ==(MyVoxelClipmapRing.Vicinity left, MyVoxelClipmapRing.Vicinity right) => 
                left.Equals(right);

            public static bool operator !=(MyVoxelClipmapRing.Vicinity left, MyVoxelClipmapRing.Vicinity right) => 
                !(left == right);

            static Vicinity()
            {
                Invalid = new MyVoxelClipmapRing.Vicinity(false);
            }
            [StructLayout(LayoutKind.Sequential, Size=8), CompilerGenerated, UnsafeValueType]
            public struct <Versions>e__FixedBuffer
            {
                public sbyte FixedElementField;
            }
        }
    }
}

