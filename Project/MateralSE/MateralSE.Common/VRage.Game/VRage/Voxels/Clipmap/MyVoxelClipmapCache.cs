namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.Sewing;
    using VRageMath;

    public class MyVoxelClipmapCache
    {
        public static int DefaultCacheSize = 0x400;
        private static MyVoxelClipmapCache m_instance;
        private readonly LRUCache<CellKey, CellData> m_cells;
        private readonly ConcurrentDictionary<uint, MyVoxelClipmap> m_evictionHandlers = new ConcurrentDictionary<uint, MyVoxelClipmap>();
        private int m_lodThreshold;
        private MyDebugHitCounter m_hitCounter = new MyDebugHitCounter(0x186a0);
        private long m_hits;
        private long m_tries;

        public MyVoxelClipmapCache(int maxCachedCells, int lodThreshold = 6)
        {
            this.m_lodThreshold = lodThreshold;
            this.m_cells = new LRUCache<CellKey, CellData>(maxCachedCells, CellKey.Comparer);
            this.m_cells.OnItemDiscarded = (Action<CellKey, CellData>) Delegate.Combine(this.m_cells.OnItemDiscarded, delegate (CellKey key, CellData cell) {
                if (cell.Guide != null)
                {
                    cell.Guide.RemoveReference(this);
                    this.m_evictionHandlers[key.ClipmapId].HandleCacheEviction(key.Coord, cell.Guide);
                }
            });
        }

        [Conditional("DEBUG")]
        internal void CycleDebugCounters()
        {
            this.m_hitCounter.Cycle();
        }

        public void EvictAll(uint clipmapId)
        {
            if (!this.m_evictionHandlers.ContainsKey(clipmapId))
            {
                throw new ArgumentException("The provided clipmap id does not correspond to any registered handler.");
            }
            this.m_cells.RemoveWhere((k, v) => k.ClipmapId == clipmapId);
        }

        public unsafe void EvictAll(uint clipmapId, BoundingBoxI range)
        {
            if (!this.m_evictionHandlers.ContainsKey(clipmapId))
            {
                throw new ArgumentException("The provided clipmap id does not correspond to any registered handler.");
            }
            if (range.Size.Size > this.m_cells.Count)
            {
                EvictionRanges ranges;
                BoundingBoxI* ranges = ranges.Ranges;
                for (int i = 0; i <= this.LodThreshold; i++)
                {
                    ranges[i] = new BoundingBoxI(range.Min >> i, (Vector3I) ((range.Max + ((1 << (i & 0x1f)) - 1)) >> i));
                }
                this.m_cells.RemoveWhere((k, v) => (k.ClipmapId == clipmapId) ? ((ranges + k.Coord.Lod).Contains(k.Coord.CoordInLod) == ContainmentType.Contains) : false);
            }
            else
            {
                for (int i = 0; i <= this.m_lodThreshold; i++)
                {
                    foreach (Vector3I vectori2 in BoundingBoxI.EnumeratePoints(new BoundingBoxI(range.Min >> i, (Vector3I) ((range.Max + ((1 << (i & 0x1f)) - 1)) >> i))))
                    {
                        CellKey key = new CellKey(clipmapId, new MyCellCoord(i, vectori2));
                        this.m_cells.Remove(key);
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void Hit()
        {
            Interlocked.Increment(ref this.m_hits);
            Interlocked.Increment(ref this.m_tries);
        }

        public bool IsCached(uint clipmapId, MyCellCoord cell, VrSewGuide dataGuide)
        {
            CellData data;
            if (!this.m_evictionHandlers.ContainsKey(clipmapId))
            {
                throw new ArgumentException("The provided clipmap id does not correspond to any registered handler.");
            }
            if (cell.Lod > this.m_lodThreshold)
            {
                return false;
            }
            CellKey key = new CellKey(clipmapId, cell);
            return (this.m_cells.TryPeek(key, out data) && ReferenceEquals(data.Guide, dataGuide));
        }

        [Conditional("DEBUG")]
        private void Miss()
        {
            Interlocked.Increment(ref this.m_tries);
        }

        public void Register(uint clipmapId, MyVoxelClipmap clipmap)
        {
            this.m_evictionHandlers.TryAdd(clipmapId, clipmap);
        }

        public bool TryRead(uint clipmapId, MyCellCoord cell, out VrSewGuide data)
        {
            CellData data2;
            if (!this.m_evictionHandlers.ContainsKey(clipmapId))
            {
                throw new ArgumentException("The provided clipmap id does not correspond to any registered handler.");
            }
            if (cell.Lod > this.m_lodThreshold)
            {
                data = null;
                return false;
            }
            CellKey key = new CellKey(clipmapId, cell);
            if (this.m_cells.TryRead(key, out data2))
            {
                data = data2.Guide;
                return true;
            }
            data = null;
            return false;
        }

        public void Unregister(uint clipmapId)
        {
            this.EvictAll(clipmapId);
            this.m_evictionHandlers.Remove<uint, MyVoxelClipmap>(clipmapId);
        }

        public void Write(uint clipmapId, MyCellCoord cell, VrSewGuide guide)
        {
            if (!this.m_evictionHandlers.ContainsKey(clipmapId))
            {
                throw new ArgumentException("The provided clipmap id does not correspond to any registered handler.");
            }
            if (cell.Lod <= this.m_lodThreshold)
            {
                guide.AddReference(this);
                CellKey key = new CellKey(clipmapId, cell);
                CellData data = new CellData(guide);
                this.m_cells.Write(key, data);
            }
        }

        public static MyVoxelClipmapCache Instance =>
            (m_instance ?? (m_instance = new MyVoxelClipmapCache(DefaultCacheSize, 6)));

        public int LodThreshold
        {
            get => 
                this.m_lodThreshold;
            set => 
                (this.m_lodThreshold = MathHelper.Clamp(value, 0, 15));
        }

        public float CacheUtilization =>
            this.m_cells.Usage;

        public float HitRate
        {
            get
            {
                float[] source = (from x in this.m_hitCounter
                    select x.Value into x
                    where !float.IsNaN(x)
                    select x).ToArray<float>();
                return ((source.Length != 0) ? source.Average() : 0f);
            }
        }

        public MyDebugHitCounter DebugHitCounter =>
            this.m_hitCounter;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoxelClipmapCache.<>c <>9 = new MyVoxelClipmapCache.<>c();
            public static Func<MyDebugHitCounter.Sample, float> <>9__16_0;
            public static Func<float, bool> <>9__16_1;

            internal float <get_HitRate>b__16_0(MyDebugHitCounter.Sample x) => 
                x.Value;

            internal bool <get_HitRate>b__16_1(float x) => 
                !float.IsNaN(x);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CellData
        {
            public VrSewGuide Guide;
            public CellData(VrSewGuide guide)
            {
                this.Guide = guide;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CellKey
        {
            public readonly uint ClipmapId;
            public readonly MyCellCoord Coord;
            private static readonly IEqualityComparer<MyVoxelClipmapCache.CellKey> ComparerInstance;
            public CellKey(uint clipmapId, MyCellCoord cell)
            {
                this.ClipmapId = clipmapId;
                this.Coord = cell;
            }

            public static IEqualityComparer<MyVoxelClipmapCache.CellKey> Comparer =>
                ComparerInstance;
            static CellKey()
            {
                ComparerInstance = new ClipmapIdCoordEqualityComparer();
            }
            private sealed class ClipmapIdCoordEqualityComparer : IEqualityComparer<MyVoxelClipmapCache.CellKey>
            {
                public bool Equals(MyVoxelClipmapCache.CellKey x, MyVoxelClipmapCache.CellKey y) => 
                    ((x.ClipmapId == y.ClipmapId) && x.Coord.Equals(y.Coord));

                public int GetHashCode(MyVoxelClipmapCache.CellKey obj) => 
                    (((int) (obj.ClipmapId * 0x18d)) ^ obj.Coord.GetHashCode());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EvictionRanges
        {
            private const int StructSize = 0x18;
            [FixedBuffer(typeof(byte), 0x180)]
            private <m_data>e__FixedBuffer m_data;
            public BoundingBoxI* Ranges =>
                ((BoundingBoxI*) &this.m_data.FixedElementField);
            [StructLayout(LayoutKind.Sequential, Size=0x180), CompilerGenerated, UnsafeValueType]
            public struct <m_data>e__FixedBuffer
            {
                public byte FixedElementField;
            }
        }
    }
}

