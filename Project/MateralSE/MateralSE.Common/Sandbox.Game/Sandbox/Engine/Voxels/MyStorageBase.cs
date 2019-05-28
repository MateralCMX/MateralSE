namespace Sandbox.Engine.Voxels
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Voxels.Storage;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public abstract class MyStorageBase : VRage.Game.Voxels.IMyStorage, VRage.ModAPI.IMyStorage
    {
        private const int ACCESS_GRID_LOD_DEFAULT = 10;
        private int m_accessGridLod;
        private int m_streamGridLod = 0xffff;
        private readonly ConcurrentDictionary<Vector3I, MyTimeSpan> m_access = new ConcurrentDictionary<Vector3I, MyTimeSpan>();
        private const int MaxChunksToDiscard = 10;
        private const int MaximumHitsForDiscard = 100;
        private MyConcurrentQueue<Vector3I> m_pendingChunksToWrite;
        private MyQueue<Vector3I> m_chunksbyAge;
        private MyConcurrentDictionary<Vector3I, VoxelChunk> m_cachedChunks;
        private MyDynamicAABBTree m_cacheMap;
        private FastResourceLock m_cacheLock;
        [ThreadStatic]
        private static List<VoxelChunk> m_tmpChunks;
        private bool m_writeCacheThrough;
        private const int WriteCacheCap = 0x400;
        private const int MaxWriteJobWorkMillis = 6;
        private bool m_cachedWrites;
        public const int STORAGE_TYPE_VERSION_CELL = 2;
        private const string STORAGE_TYPE_NAME_CELL = "Cell";
        private const string STORAGE_TYPE_NAME_OCTREE = "Octree";
        protected const int STORAGE_TYPE_VERSION_OCTREE = 1;
        protected const int STORAGE_TYPE_VERSION_OCTREE_ACCESS = 2;
        private readonly object m_compressedDataLock = new object();
        private byte[] m_compressedData;
        private bool m_setCompressedDataCacheAllowed;
        private readonly MyVoxelGeometry m_geometry = new MyVoxelGeometry();
        protected readonly FastResourceLock StorageLock = new FastResourceLock();
        public static bool UseStorageCache = true;
        private static readonly LRUCache<int, MyStorageBase> m_storageCache = new LRUCache<int, MyStorageBase>(0x200, null);
        private static int m_runningStorageId = -1;
        [CompilerGenerated]
        private Action<Vector3I, Vector3I, MyStorageDataTypeFlags> RangeChanged;
        private const int CLOSE_MASK = -2147483648;
        private const int PIN_MASK = 0x7fffffff;
        private int m_pinAndCloseMark;
        private int m_closed;

        public event Action<Vector3I, Vector3I, MyStorageDataTypeFlags> RangeChanged
        {
            [CompilerGenerated] add
            {
                Action<Vector3I, Vector3I, MyStorageDataTypeFlags> rangeChanged = this.RangeChanged;
                while (true)
                {
                    Action<Vector3I, Vector3I, MyStorageDataTypeFlags> a = rangeChanged;
                    Action<Vector3I, Vector3I, MyStorageDataTypeFlags> action3 = (Action<Vector3I, Vector3I, MyStorageDataTypeFlags>) Delegate.Combine(a, value);
                    rangeChanged = Interlocked.CompareExchange<Action<Vector3I, Vector3I, MyStorageDataTypeFlags>>(ref this.RangeChanged, action3, a);
                    if (ReferenceEquals(rangeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Vector3I, Vector3I, MyStorageDataTypeFlags> rangeChanged = this.RangeChanged;
                while (true)
                {
                    Action<Vector3I, Vector3I, MyStorageDataTypeFlags> source = rangeChanged;
                    Action<Vector3I, Vector3I, MyStorageDataTypeFlags> action3 = (Action<Vector3I, Vector3I, MyStorageDataTypeFlags>) Delegate.Remove(source, value);
                    rangeChanged = Interlocked.CompareExchange<Action<Vector3I, Vector3I, MyStorageDataTypeFlags>>(ref this.RangeChanged, action3, source);
                    if (ReferenceEquals(rangeChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyStorageBase()
        {
            this.StorageId = (uint) Interlocked.Increment(ref m_runningStorageId);
        }

        public void AccessDelete(ref Vector3I coord, MyStorageDataTypeFlags dataType, bool notify = true)
        {
            Vector3I voxelRangeMin = coord << this.m_accessGridLod;
            this.DeleteRange(dataType, voxelRangeMin, (Vector3I) (voxelRangeMin + (1 << (this.m_accessGridLod & 0x1f))), notify);
        }

        public void AccessDeleteFirst()
        {
            if (!this.m_access.IsEmpty)
            {
                Vector3I key = this.m_access.First<KeyValuePair<Vector3I, MyTimeSpan>>().Key;
                this.AccessDelete(ref key, MyStorageDataTypeFlags.All, true);
            }
        }

        public void AccessRange(MyAccessType accessType, MyStorageDataTypeEnum dataType, ref MyCellCoord coord)
        {
            if ((coord.Lod == this.m_accessGridLod) && (dataType == MyStorageDataTypeEnum.Content))
            {
                if (accessType == MyAccessType.Write)
                {
                    MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
                    this.m_access[coord.CoordInLod] = span;
                }
                else if (accessType == MyAccessType.Delete)
                {
                    this.m_access.Remove<Vector3I, MyTimeSpan>(coord.CoordInLod);
                }
            }
        }

        protected void AccessReset()
        {
            this.m_access.Clear();
            int num = 0;
            Vector3I size = this.Size;
            while (size != Vector3I.Zero)
            {
                size = size >> 1;
                num++;
            }
            this.m_accessGridLod = Math.Min(num - 1, 10);
        }

        private unsafe void ChangeMaterials(Dictionary<byte, byte> map)
        {
            int num = 0;
            if ((this.Size + 1).Size > 0x400000)
            {
                MyLog.Default.Error("Cannot overwrite materials for a storage 4 MB or larger.", Array.Empty<object>());
            }
            else
            {
                byte* numPtr;
                byte[] pinned buffer;
                Vector3I zero = Vector3I.Zero;
                Vector3I lodVoxelRangeMax = this.Size - 1;
                MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
                target.Resize(this.Size);
                this.ReadRange(target, MyStorageDataTypeFlags.Material, 0, zero, lodVoxelRangeMax);
                int sizeLinear = target.SizeLinear;
                if (((buffer = target[MyStorageDataTypeEnum.Material]) == null) || (buffer.Length == 0))
                {
                    numPtr = null;
                }
                else
                {
                    numPtr = buffer;
                }
                for (int i = 0; i < sizeLinear; i++)
                {
                    byte num4;
                    if (map.TryGetValue(numPtr[i], out num4))
                    {
                        numPtr[i] = num4;
                        num++;
                    }
                }
                buffer = null;
                if (num > 0)
                {
                    this.WriteRange(target, MyStorageDataTypeFlags.Material, zero, lodVoxelRangeMax, true, false);
                }
            }
        }

        public void CleanCachedChunks()
        {
            int num = 0;
            int count = this.m_chunksbyAge.Count;
            int num3 = 0;
            while (true)
            {
                while (true)
                {
                    if (num3 >= count)
                    {
                        return;
                    }
                    else if (num < 10)
                    {
                        bool flag;
                        VoxelChunk chunk;
                        Vector3I vectori;
                        FastResourceLockExtensions.MyExclusiveLock @lock;
                        using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                        {
                            vectori = this.m_chunksbyAge.Dequeue();
                            flag = this.m_cachedChunks.TryGetValue(vectori, out chunk);
                        }
                        if (flag)
                        {
                            if ((chunk.Dirty == MyStorageDataTypeFlags.None) && (chunk.HitCount <= 100))
                            {
                                using (chunk.Lock.AcquireSharedUsing())
                                {
                                    if ((chunk.Dirty == MyStorageDataTypeFlags.None) && (chunk.HitCount <= 100))
                                    {
                                        using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                                        {
                                            this.m_cachedChunks.Remove(vectori);
                                            this.m_cacheMap.RemoveProxy(chunk.TreeProxy);
                                            break;
                                        }
                                    }
                                    using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                                    {
                                        this.m_chunksbyAge.Enqueue(vectori);
                                        chunk.HitCount = 0;
                                        break;
                                    }
                                }
                            }
                            using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                            {
                                this.m_chunksbyAge.Enqueue(vectori);
                                chunk.HitCount = 0;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;
                }
                num3++;
            }
        }

        public void Close()
        {
            int num2;
            for (int i = this.m_pinAndCloseMark; (i & -2147483648) == 0; i = num2)
            {
                num2 = Interlocked.CompareExchange(ref this.m_pinAndCloseMark, i | -2147483648, i);
                if (i == num2)
                {
                    if ((i & 0x7fffffff) == 0)
                    {
                        this.CloseInternal();
                    }
                    return;
                }
            }
        }

        public virtual void CloseInternal()
        {
            if (Interlocked.CompareExchange(ref this.m_closed, 1, 0) == 0)
            {
                using (this.StorageLock.AcquireExclusiveUsing())
                {
                    this.RangeChanged = null;
                    if (this.DataProvider != null)
                    {
                        this.DataProvider.Close();
                    }
                }
                if (this.CachedWrites && (OperationsComponent != null))
                {
                    OperationsComponent.Remove(this);
                }
            }
        }

        public void ConvertAccessCoordinates(ref Vector3I coord, out BoundingBoxD bb)
        {
            MyCellCoord coord2 = new MyCellCoord(this.m_accessGridLod, ref coord);
            float num = 1f * (1 << (coord2.Lod & 0x1f));
            Vector3 vector = ((coord2.CoordInLod << coord2.Lod) * 1f) + (0.5f * num);
            bb = new BoundingBoxD(vector - (0.5f * num), vector + (0.5f * num));
        }

        public virtual VRage.Game.Voxels.IMyStorage Copy() => 
            null;

        public virtual void DebugDraw(ref MatrixD worldMatrix, MyVoxelDebugDrawMode mode)
        {
            if (mode == MyVoxelDebugDrawMode.Content_Access)
            {
                this.DebugDrawAccess(ref worldMatrix);
            }
        }

        private void DebugDrawAccess(ref MatrixD worldMatrix)
        {
            Color green = Color.Green;
            green.A = 0x40;
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(worldMatrix, green, true, true))
            {
                foreach (KeyValuePair<Vector3I, MyTimeSpan> pair in this.m_access)
                {
                    BoundingBoxD xd;
                    Vector3I key = pair.Key;
                    this.ConvertAccessCoordinates(ref key, out xd);
                    Color? color = null;
                    aabb.Add(ref xd, color);
                }
            }
        }

        private void DeleteChunk(ref Vector3I coord, MyStorageDataTypeFlags dataToDelete)
        {
            VoxelChunk chunk;
            if (this.m_cachedChunks.TryGetValue(coord, out chunk))
            {
                if ((dataToDelete & chunk.Cached) == chunk.Cached)
                {
                    this.m_cachedChunks.Remove(coord);
                    this.m_cacheMap.RemoveProxy(chunk.TreeProxy);
                }
                else
                {
                    chunk.Cached &= (byte) ~dataToDelete;
                    chunk.Dirty &= (byte) ~dataToDelete;
                }
            }
        }

        private unsafe void DeleteRange(MyStorageDataTypeFlags dataToDelete, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify)
        {
            bool flag1 = notify;
            try
            {
                FastResourceLockExtensions.MyExclusiveLock @lock;
                this.ResetCompressedDataCache();
                if (this.CachedWrites && this.OverlapsAnyCachedCell(voxelRangeMin, voxelRangeMax))
                {
                    using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                    {
                        int num = 3;
                        Vector3I vectori = voxelRangeMin >> num;
                        Vector3I vectori2 = voxelRangeMax >> num;
                        Vector3I zero = Vector3I.Zero;
                        zero.Z = vectori.Z;
                        while (zero.Z <= vectori2.Z)
                        {
                            zero.Y = vectori.Y;
                            while (true)
                            {
                                if (zero.Y > vectori2.Y)
                                {
                                    int* numPtr3 = (int*) ref zero.Z;
                                    numPtr3[0]++;
                                    break;
                                }
                                zero.X = vectori.X;
                                while (true)
                                {
                                    if (zero.X > vectori2.X)
                                    {
                                        int* numPtr2 = (int*) ref zero.Y;
                                        numPtr2[0]++;
                                        break;
                                    }
                                    this.DeleteChunk(ref zero, dataToDelete);
                                    int* numPtr1 = (int*) ref zero.X;
                                    numPtr1[0]++;
                                }
                            }
                        }
                    }
                }
                using (@lock = this.StorageLock.AcquireExclusiveUsing())
                {
                    this.DeleteRangeInternal(dataToDelete, ref voxelRangeMin, ref voxelRangeMax);
                }
            }
            finally
            {
                if (notify)
                {
                    this.OnRangeChanged(voxelRangeMin, voxelRangeMax, dataToDelete);
                }
            }
        }

        protected abstract void DeleteRangeInternal(MyStorageDataTypeFlags dataToDelete, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax);
        private void DequeueDirtyChunk(out VoxelChunk chunk, out Vector3I coords)
        {
            coords = this.m_pendingChunksToWrite.Dequeue();
            this.m_cachedChunks.TryGetValue(coords, out chunk);
        }

        private unsafe void ExecuteOperationFast<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, bool notifyRangeChanged, bool skipCache) where TVoxelOperator: struct, IVoxelOperator
        {
            bool flag1 = notifyRangeChanged;
            try
            {
                FastResourceLockExtensions.MyExclusiveLock @lock;
                this.ResetCompressedDataCache();
                if ((!this.CachedWrites || skipCache) || ((this.m_pendingChunksToWrite.Count >= 0x400) && !this.OverlapsAnyCachedCell(voxelRangeMin, voxelRangeMax)))
                {
                    if (skipCache)
                    {
                        BoundingBoxI box = new BoundingBoxI(voxelRangeMin, voxelRangeMax);
                        this.FlushCache(ref box, 0);
                    }
                    using (@lock = this.StorageLock.AcquireExclusiveUsing())
                    {
                        this.WriteRangeInternal<TVoxelOperator>(ref voxelOperator, dataToWrite, ref voxelRangeMin, ref voxelRangeMax);
                    }
                }
                else
                {
                    int num = 3;
                    Vector3I vectori = voxelRangeMin >> num;
                    Vector3I vectori2 = voxelRangeMax >> num;
                    Vector3I zero = Vector3I.Zero;
                    zero.Z = vectori.Z;
                    while (zero.Z <= vectori2.Z)
                    {
                        zero.Y = vectori.Y;
                        while (true)
                        {
                            if (zero.Y > vectori2.Y)
                            {
                                int* numPtr3 = (int*) ref zero.Z;
                                numPtr3[0]++;
                                break;
                            }
                            zero.X = vectori.X;
                            while (true)
                            {
                                VoxelChunk chunk;
                                if (zero.X > vectori2.X)
                                {
                                    int* numPtr2 = (int*) ref zero.Y;
                                    numPtr2[0]++;
                                    break;
                                }
                                Vector3I vectori4 = zero << num;
                                Vector3I min = Vector3I.Max(zero << num, voxelRangeMin);
                                Vector3I max = Vector3I.Min((Vector3I) (((zero + 1) << num) - 1), voxelRangeMax);
                                Vector3I targetOffset = min - voxelRangeMin;
                                MyStorageDataTypeFlags required = dataToWrite;
                                Vector3I vectori8 = (Vector3I) ((max - min) + 1);
                                if ((vectori8.Size == 0x200) && (voxelOperator.Flags == VoxelOperatorFlags.WriteAll))
                                {
                                    required = MyStorageDataTypeFlags.None;
                                }
                                this.GetChunk(ref zero, out chunk, required);
                                min -= vectori4;
                                max -= vectori4;
                                using (@lock = chunk.Lock.AcquireExclusiveUsing())
                                {
                                    chunk.ExecuteOperator<TVoxelOperator>(ref voxelOperator, dataToWrite, ref targetOffset, ref min, ref max);
                                    if (chunk.Dirty == MyStorageDataTypeFlags.None)
                                    {
                                        this.m_pendingChunksToWrite.Enqueue(zero);
                                    }
                                }
                                int* numPtr1 = (int*) ref zero.X;
                                numPtr1[0]++;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (notifyRangeChanged)
                {
                    this.OnRangeChanged(voxelRangeMin, voxelRangeMax, dataToWrite);
                }
            }
        }

        protected void FlushCache(ref BoundingBoxI box, int lodIndex)
        {
            if (this.CachedWrites && (this.m_cacheMap.GetLeafCount() != 0))
            {
                if (m_tmpChunks == null)
                {
                    m_tmpChunks = new List<VoxelChunk>();
                }
                BoundingBox bbox = new BoundingBox((Vector3) (box.Min << lodIndex), (Vector3) (box.Max << lodIndex));
                using (this.m_cacheLock.AcquireSharedUsing())
                {
                    this.m_cacheMap.OverlapAllBoundingBox<VoxelChunk>(ref bbox, m_tmpChunks, 0, false);
                }
                foreach (VoxelChunk chunk in m_tmpChunks)
                {
                    if (chunk.Dirty != MyStorageDataTypeFlags.None)
                    {
                        this.WriteChunk(chunk);
                    }
                }
                m_tmpChunks.Clear();
            }
        }

        private void GetChunk(ref Vector3I coord, out VoxelChunk chunk, MyStorageDataTypeFlags required)
        {
            using (this.m_cacheLock.AcquireExclusiveUsing())
            {
                FastResourceLockExtensions.MySharedLock lock2;
                if (this.m_cachedChunks.TryGetValue(coord, out chunk))
                {
                    if ((chunk.Cached & required) != required)
                    {
                        using (lock2 = this.StorageLock.AcquireSharedUsing())
                        {
                            this.ReadDatForChunk(chunk, ((MyStorageDataTypeFlags) ((int) required)) & (((MyStorageDataTypeFlags) ((byte) ~chunk.Cached)) & MyStorageDataTypeFlags.All));
                        }
                    }
                }
                else
                {
                    chunk = new VoxelChunk(coord);
                    Vector3I vectori = coord << 3;
                    Vector3I vectori2 = (Vector3I) (((coord + 1) << 3) - 1);
                    if (required != MyStorageDataTypeFlags.None)
                    {
                        using (lock2 = this.StorageLock.AcquireSharedUsing())
                        {
                            this.ReadDatForChunk(chunk, required);
                        }
                    }
                    this.m_chunksbyAge.Enqueue(coord);
                    this.m_cachedChunks.Add(coord, chunk);
                    BoundingBox aabb = new BoundingBox((Vector3) vectori, (Vector3) vectori2);
                    chunk.TreeProxy = this.m_cacheMap.AddProxy(ref aabb, chunk, 0, true);
                }
            }
        }

        private byte[] GetData(bool compressed)
        {
            MemoryStream stream;
            using (stream = new MemoryStream(0x4000))
            {
                Stream stream3 = null;
                stream3 = !compressed ? ((Stream) stream) : ((Stream) new GZipStream(stream, CompressionMode.Compress));
                using (BufferedStream stream4 = new BufferedStream(stream3, 0x4000))
                {
                    if (!(base.GetType() == typeof(MyOctreeStorage)))
                    {
                        throw new InvalidBranchException();
                    }
                    stream4.WriteNoAlloc("Octree", null);
                    stream4.Write7BitEncodedInt(2);
                    this.SaveInternal(stream4);
                }
                if (compressed)
                {
                    stream3.Dispose();
                }
            }
            return stream.ToArray();
        }

        public void GetStats(out WriteCacheStats stats)
        {
            stats.CachedChunks = this.m_cachedChunks.Count;
            stats.QueuedWrites = this.m_pendingChunksToWrite.Count;
            stats.Chunks = this.m_cachedChunks;
        }

        public static string GetStoragePath(string storageName) => 
            Path.Combine(MySession.Static.CurrentPath, storageName + ".vx2");

        public byte[] GetVoxelData()
        {
            if (this.CachedWrites)
            {
                this.WritePending(true);
            }
            byte[] data = null;
            try
            {
                using (this.StorageLock.AcquireSharedUsing())
                {
                    data = this.GetData(false);
                    this.m_setCompressedDataCacheAllowed = true;
                }
            }
            finally
            {
            }
            return data;
        }

        public void InitWriteCache(int prealloc = 0x80)
        {
            if ((this.m_cachedChunks == null) && (OperationsComponent != null))
            {
                this.CachedWrites = true;
                this.m_cachedChunks = new MyConcurrentDictionary<Vector3I, VoxelChunk>(prealloc, Vector3I.Comparer);
                this.m_pendingChunksToWrite = new MyConcurrentQueue<Vector3I>(prealloc / 10);
                this.m_chunksbyAge = new MyQueue<Vector3I>(prealloc);
                this.m_cacheMap = new MyDynamicAABBTree(Vector3.Zero, 1f);
                this.m_cacheLock = new FastResourceLock();
                OperationsComponent.Add(this);
            }
        }

        public bool Intersect(ref LineD line)
        {
            using (this.StorageLock.AcquireSharedUsing())
            {
                return (!this.Closed ? this.IntersectInternal(ref line) : false);
            }
        }

        public abstract ContainmentType Intersect(ref BoundingBox box, bool lazy);
        public ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true)
        {
            this.FlushCache(ref box, lod);
            using (this.StorageLock.AcquireSharedUsing())
            {
                return (!this.Closed ? this.IntersectInternal(ref box, lod, exhaustiveContainmentCheck) : ContainmentType.Disjoint);
            }
        }

        public abstract bool IntersectInternal(ref LineD line);
        protected abstract ContainmentType IntersectInternal(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck);
        protected abstract bool IsModifiedInternal(ref BoundingBoxI range);
        public bool IsRangeModified(ref BoundingBoxI box)
        {
            if (this.DataProvider == null)
            {
                return true;
            }
            using (this.StorageLock.AcquireSharedUsing())
            {
                return this.IsModifiedInternal(ref box);
            }
        }

        public static MyStorageBase Load(byte[] memoryBuffer)
        {
            MyStorageBase base2;
            bool flag;
            using (MemoryStream stream = new MemoryStream(memoryBuffer, false))
            {
                using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress))
                {
                    Load(stream2, out base2, out flag);
                }
            }
            if (!flag)
            {
                base2.SetCompressedDataCache(memoryBuffer);
            }
            else
            {
                MySandboxGame.Log.WriteLine("Voxel storage was in old format. It is updated but needs to be saved.");
                base2.ResetCompressedDataCache();
            }
            return base2;
        }

        public static MyStorageBase Load(string name, bool cache = true)
        {
            MyStorageBase base2 = null;
            if ((MyMultiplayer.Static == null) || MyMultiplayer.Static.IsServer)
            {
                base2 = LoadFromFile(Path.IsPathRooted(name) ? name : Path.Combine(MySession.Static.CurrentPath, name + ".vx2"), null, cache);
            }
            else
            {
                byte[] memoryBuffer = MyMultiplayer.Static.VoxelMapData.Read(name);
                if (memoryBuffer == null)
                {
                    MyAnalyticsHelper.ReportActivityStart(null, "Missing voxel map data!", name, "DevNote", "", false);
                    throw new Exception($"Missing voxel map data! : {name}");
                }
                base2 = Load(memoryBuffer);
            }
            return base2;
        }

        private static void Load(Stream stream, out MyStorageBase storage, out bool isOldFormat)
        {
            try
            {
                isOldFormat = false;
                string str = stream.ReadString(null);
                int fileVersion = stream.Read7BitEncodedInt();
                if (str == "Cell")
                {
                    storage = new MyStorageBaseCompatibility().Compatibility_LoadCellStorage(fileVersion, stream);
                    isOldFormat = true;
                }
                else
                {
                    if (str != "Octree")
                    {
                        throw new InvalidBranchException();
                    }
                    storage = new MyOctreeStorage();
                    storage.LoadInternal(fileVersion, stream, ref isOldFormat);
                    storage.m_geometry.Init(storage);
                }
            }
            finally
            {
            }
        }

        protected void LoadAccess(Stream stream, MyCellCoord coord)
        {
            if ((coord.Lod == this.m_streamGridLod) || (coord.Lod == this.m_accessGridLod))
            {
                MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
                MyTimeSpan span2 = span;
                if (coord.Lod == this.m_streamGridLod)
                {
                    long num = stream.ReadUInt16();
                    span2 = MyTimeSpan.FromSeconds(span.Seconds - (num * 60));
                }
                if (coord.Lod == this.m_accessGridLod)
                {
                    this.m_access[coord.CoordInLod] = span2;
                }
            }
        }

        public static MyStorageBase LoadFromFile(string absoluteFilePath, Dictionary<byte, byte> modifiers = null, bool cache = true)
        {
            if (absoluteFilePath.Contains(".vox") && absoluteFilePath.Contains(".vx2"))
            {
                int startIndex = absoluteFilePath.LastIndexOf(".vx2");
                if (startIndex != -1)
                {
                    absoluteFilePath = absoluteFilePath.Remove(startIndex);
                    string text1 = Path.ChangeExtension(absoluteFilePath, "vx2");
                    absoluteFilePath = text1;
                }
            }
            MyStorageBase base2 = null;
            MyVoxelObjectDefinition definition = new MyVoxelObjectDefinition(absoluteFilePath, modifiers);
            int hashCode = definition.GetHashCode();
            if (cache && UseStorageCache)
            {
                base2 = m_storageCache.Read(hashCode);
                if (base2 != null)
                {
                    base2.Shared = true;
                    return base2;
                }
            }
            if (MyFileSystem.FileExists(absoluteFilePath))
            {
                MySandboxGame.Log.WriteLine($"Loading voxel storage from file '{absoluteFilePath}'");
            }
            else
            {
                string str = Path.ChangeExtension(absoluteFilePath, "vox");
                MySandboxGame.Log.WriteLine($"Loading voxel storage from file '{str}'");
                if (!MyFileSystem.FileExists(str))
                {
                    int startIndex = absoluteFilePath.LastIndexOf(".vx2");
                    if (startIndex == -1)
                    {
                        return null;
                    }
                    str = absoluteFilePath.Remove(startIndex);
                    if (!MyFileSystem.FileExists(str))
                    {
                        return null;
                    }
                }
                UpdateFileFormat(str);
            }
            byte[] buffer = null;
            using (Stream stream = MyFileSystem.OpenRead(absoluteFilePath))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }
            base2 = Load(buffer);
            if (definition.Changes != null)
            {
                base2.ChangeMaterials(definition.Changes);
            }
            if (UseStorageCache & cache)
            {
                m_storageCache.Write(hashCode, base2);
                base2.Shared = true;
            }
            return base2;
        }

        protected abstract void LoadInternal(int fileVersion, Stream stream, ref bool isOldFormat);
        public void NotifyChanged(Vector3I voxelRangeMin, Vector3I voxelRangeMax, MyStorageDataTypeFlags changedData)
        {
            this.OnRangeChanged(voxelRangeMin, voxelRangeMax, changedData);
        }

        protected void OnRangeChanged(Vector3I voxelRangeMin, Vector3I voxelRangeMax, MyStorageDataTypeFlags changedData)
        {
            if (!this.Closed)
            {
                this.ResetCompressedDataCache();
                if (this.RangeChanged != null)
                {
                    this.ClampVoxelCoord(ref voxelRangeMin, 1);
                    this.ClampVoxelCoord(ref voxelRangeMax, 1);
                    this.RangeChanged.InvokeIfNotNull<Vector3I, Vector3I, MyStorageDataTypeFlags>(voxelRangeMin, voxelRangeMax, changedData);
                }
            }
        }

        private bool OverlapsAnyCachedCell(Vector3I voxelRangeMin, Vector3I voxelRangeMax)
        {
            BoundingBox bbox = new BoundingBox((Vector3) voxelRangeMin, (Vector3) voxelRangeMax);
            using (this.m_cacheLock.AcquireSharedUsing())
            {
                return this.m_cacheMap.OverlapsAnyLeafBoundingBox(ref bbox);
            }
        }

        public void OverwriteAllMaterials(byte materialIndex)
        {
        }

        public void OverwriteAllMaterials(MyVoxelMaterialDefinition material)
        {
            using (this.StorageLock.AcquireExclusiveUsing())
            {
                this.ResetCompressedDataCache();
                this.OverwriteAllMaterialsInternal(material);
            }
            this.OnRangeChanged(Vector3I.Zero, this.Size - 1, MyStorageDataTypeFlags.Material);
        }

        protected abstract void OverwriteAllMaterialsInternal(MyVoxelMaterialDefinition material);
        public StoragePin Pin()
        {
            if ((Interlocked.Increment(ref this.m_pinAndCloseMark) & -2147483648) == 0)
            {
                return new StoragePin(this);
            }
            if ((Interlocked.Decrement(ref this.m_pinAndCloseMark) & 0x7fffffff) == 0)
            {
                this.CloseInternal();
            }
            return new StoragePin(null);
        }

        public void PinAndExecute(Action action)
        {
            using (StoragePin pin = this.Pin())
            {
                if (pin.Valid)
                {
                    action.InvokeIfNotNull();
                }
            }
        }

        public void PinAndExecute(Action<VRage.ModAPI.IMyStorage> action)
        {
            using (StoragePin pin = this.Pin())
            {
                if (pin.Valid)
                {
                    action.InvokeIfNotNull<VRage.ModAPI.IMyStorage>(this);
                }
            }
        }

        private void ReadDatForChunk(VoxelChunk chunk, MyStorageDataTypeFlags data)
        {
            using (chunk.Lock.AcquireExclusiveUsing())
            {
                Vector3I lodVoxelRangeMin = chunk.Coords << 3;
                Vector3I lodVoxelRangeMax = (Vector3I) (((chunk.Coords + 1) << 3) - 1);
                MyStorageData target = chunk.MakeData();
                MyVoxelRequestFlags considerContent = MyVoxelRequestFlags.ConsiderContent;
                this.ReadRangeInternal(target, ref Vector3I.Zero, data, 0, ref lodVoxelRangeMin, ref lodVoxelRangeMax, ref considerContent);
                chunk.Cached |= data;
                chunk.MaxLod = 0;
            }
        }

        public void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax)
        {
            MyVoxelRequestFlags requestFlags = (dataToRead == MyStorageDataTypeFlags.All) ? MyVoxelRequestFlags.ConsiderContent : ((MyVoxelRequestFlags) 0);
            this.ReadRange(target, dataToRead, lodIndex, lodVoxelRangeMin, lodVoxelRangeMax, ref requestFlags);
        }

        public void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, ref MyVoxelRequestFlags requestFlags)
        {
            if (((dataToRead & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None) && ((requestFlags & MyVoxelRequestFlags.SurfaceMaterial) == 0))
            {
                target.ClearMaterials(0xff);
                requestFlags |= MyVoxelRequestFlags.EmptyData;
                if ((dataToRead & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
                {
                    requestFlags |= MyVoxelRequestFlags.ConsiderContent;
                }
            }
            if ((dataToRead & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                target.ClearContent(0);
                requestFlags |= MyVoxelRequestFlags.EmptyData;
            }
            if ((requestFlags.HasFlags(MyVoxelRequestFlags.AdviseCache) && (lodIndex == 0)) && this.CachedWrites)
            {
                this.ReadRangeAdviseCache(target, dataToRead, lodVoxelRangeMin, lodVoxelRangeMax);
            }
            else
            {
                FastResourceLockExtensions.MySharedLock @lock;
                if ((this.CachedWrites && (lodIndex <= 3)) && (this.m_cachedChunks.Count > 0))
                {
                    if (m_tmpChunks == null)
                    {
                        m_tmpChunks = new List<VoxelChunk>();
                    }
                    int num = 3 - lodIndex;
                    BoundingBox bbox = new BoundingBox((Vector3) (lodVoxelRangeMin << lodIndex), (Vector3) (lodVoxelRangeMax << lodIndex));
                    using (@lock = this.m_cacheLock.AcquireSharedUsing())
                    {
                        this.m_cacheMap.OverlapAllBoundingBox<VoxelChunk>(ref bbox, m_tmpChunks, 0, false);
                    }
                    if (m_tmpChunks.Count > 0)
                    {
                        bool flag = false;
                        if ((((lodVoxelRangeMax >> num) - (lodVoxelRangeMin >> num)) + 1).Size > m_tmpChunks.Count)
                        {
                            using (@lock = this.StorageLock.AcquireSharedUsing())
                            {
                                this.ReadRangeInternal(target, ref Vector3I.Zero, dataToRead, lodIndex, ref lodVoxelRangeMin, ref lodVoxelRangeMax, ref requestFlags);
                            }
                            requestFlags &= ~(MyVoxelRequestFlags.OneMaterial | MyVoxelRequestFlags.FullContent | MyVoxelRequestFlags.EmptyData);
                            flag = true;
                        }
                        for (int i = 0; i < m_tmpChunks.Count; i++)
                        {
                            VoxelChunk chunk = m_tmpChunks[i];
                            Vector3I vectori3 = chunk.Coords << num;
                            Vector3I min = Vector3I.Max(chunk.Coords << num, lodVoxelRangeMin);
                            Vector3I targetOffset = min - lodVoxelRangeMin;
                            min -= vectori3;
                            Vector3I max = Vector3I.Min((Vector3I) (((chunk.Coords + 1) << num) - 1), lodVoxelRangeMax) - vectori3;
                            if (((chunk.Cached & dataToRead) != dataToRead) && !flag)
                            {
                                using (@lock = this.StorageLock.AcquireSharedUsing())
                                {
                                    if ((chunk.Cached & dataToRead) != dataToRead)
                                    {
                                        this.ReadDatForChunk(chunk, dataToRead);
                                    }
                                }
                            }
                            using (@lock = chunk.Lock.AcquireSharedUsing())
                            {
                                chunk.ReadLod(target, !flag ? dataToRead : (dataToRead & chunk.Cached), ref targetOffset, lodIndex, ref min, ref max);
                            }
                        }
                        m_tmpChunks.Clear();
                        return;
                    }
                }
                using (@lock = this.StorageLock.AcquireSharedUsing())
                {
                    this.ReadRangeInternal(target, ref Vector3I.Zero, dataToRead, lodIndex, ref lodVoxelRangeMin, ref lodVoxelRangeMax, ref requestFlags);
                }
            }
        }

        private unsafe void ReadRangeAdviseCache(MyStorageData target, MyStorageDataTypeFlags dataToRead, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax)
        {
            if (this.m_pendingChunksToWrite.Count > 0x400)
            {
                this.ReadRange(target, dataToRead, 0, lodVoxelRangeMin, lodVoxelRangeMax);
            }
            else if (this.CachedWrites)
            {
                int num = 3;
                Vector3I vectori = lodVoxelRangeMin >> num;
                Vector3I vectori2 = lodVoxelRangeMax >> num;
                Vector3I zero = Vector3I.Zero;
                zero.Z = vectori.Z;
                while (zero.Z <= vectori2.Z)
                {
                    zero.Y = vectori.Y;
                    while (true)
                    {
                        if (zero.Y > vectori2.Y)
                        {
                            int* numPtr3 = (int*) ref zero.Z;
                            numPtr3[0]++;
                            break;
                        }
                        zero.X = vectori.X;
                        while (true)
                        {
                            VoxelChunk chunk;
                            if (zero.X > vectori2.X)
                            {
                                int* numPtr2 = (int*) ref zero.Y;
                                numPtr2[0]++;
                                break;
                            }
                            Vector3I vectori4 = zero << num;
                            Vector3I min = Vector3I.Max(zero << num, lodVoxelRangeMin);
                            Vector3I targetOffset = min - lodVoxelRangeMin;
                            this.GetChunk(ref zero, out chunk, dataToRead);
                            min -= vectori4;
                            Vector3I max = Vector3I.Min((Vector3I) (((zero + 1) << num) - 1), lodVoxelRangeMax) - vectori4;
                            using (chunk.Lock.AcquireSharedUsing())
                            {
                                chunk.ReadLod(target, dataToRead, ref targetOffset, 0, ref min, ref max);
                            }
                            int* numPtr1 = (int*) ref zero.X;
                            numPtr1[0]++;
                        }
                    }
                }
            }
        }

        protected abstract void ReadRangeInternal(MyStorageData target, ref Vector3I targetWriteRange, MyStorageDataTypeFlags dataToRead, int lodIndex, ref Vector3I lodVoxelRangeMin, ref Vector3I lodVoxelRangeMax, ref MyVoxelRequestFlags requestFlags);
        protected void ReadStorageAccess(Stream stream)
        {
            this.m_streamGridLod = stream.ReadUInt16();
        }

        public void Reset(MyStorageDataTypeFlags dataToReset)
        {
            using (this.StorageLock.AcquireExclusiveUsing())
            {
                this.ResetCompressedDataCache();
                this.ResetInternal(dataToReset);
            }
            this.OnRangeChanged(Vector3I.Zero, this.Size - 1, dataToReset);
        }

        public static void ResetCache()
        {
        }

        private void ResetCompressedDataCache()
        {
            object compressedDataLock = this.m_compressedDataLock;
            lock (compressedDataLock)
            {
                this.m_setCompressedDataCacheAllowed = false;
                this.m_compressedData = null;
            }
        }

        protected abstract void ResetInternal(MyStorageDataTypeFlags dataToReset);
        public void Save(out byte[] outCompressedData)
        {
            if (this.CachedWrites)
            {
                this.WritePending(true);
            }
            try
            {
                using (this.StorageLock.AcquireSharedUsing())
                {
                    object compressedDataLock = this.m_compressedDataLock;
                    lock (compressedDataLock)
                    {
                        if (this.m_compressedData == null)
                        {
                            this.m_compressedData = this.GetData(true);
                        }
                        outCompressedData = this.m_compressedData;
                    }
                }
            }
            finally
            {
            }
        }

        protected void SaveAccess(Stream stream, MyCellCoord coord)
        {
            if (coord.Lod == this.m_accessGridLod)
            {
                MyTimeSpan span2;
                MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
                if (!this.m_access.TryGetValue(coord.CoordInLod, out span2))
                {
                    span2 = span;
                }
                long num = (long) ((span - span2).Seconds / 60.0);
                if (num > 0xffffL)
                {
                    num = 0xffffL;
                }
                stream.WriteNoAlloc((ushort) num);
            }
        }

        protected abstract void SaveInternal(Stream stream);
        public void SetCompressedDataCache(byte[] data)
        {
            object compressedDataLock = this.m_compressedDataLock;
            lock (compressedDataLock)
            {
                if (this.m_setCompressedDataCacheAllowed)
                {
                    this.m_setCompressedDataCacheAllowed = false;
                    this.m_compressedData = data;
                }
            }
        }

        public void Sweep(MyStorageDataTypeFlags dataToSweep)
        {
            try
            {
                FastResourceLockExtensions.MyExclusiveLock @lock;
                this.ResetCompressedDataCache();
                if (this.CachedWrites)
                {
                    using (@lock = this.m_cacheLock.AcquireExclusiveUsing())
                    {
                        while (this.m_cachedChunks.Count > 0)
                        {
                            KeyValuePair<Vector3I, VoxelChunk> pair = this.m_cachedChunks.FirstPair();
                            Vector3I key = pair.Key;
                            this.DeleteChunk(ref key, dataToSweep);
                        }
                    }
                }
                using (@lock = this.StorageLock.AcquireExclusiveUsing())
                {
                    this.SweepInternal(dataToSweep);
                }
            }
            finally
            {
                this.OnRangeChanged(Vector3I.Zero, this.Size, dataToSweep);
            }
        }

        protected abstract void SweepInternal(MyStorageDataTypeFlags dataToSweep);
        public void Unpin()
        {
            if (Interlocked.Decrement(ref this.m_pinAndCloseMark) == -2147483648)
            {
                this.CloseInternal();
            }
        }

        private static void UpdateFileFormat(string originalVoxFile)
        {
            string path = Path.ChangeExtension(originalVoxFile, ".vx2");
            if (!File.Exists(originalVoxFile))
            {
                object[] args = new object[] { originalVoxFile };
                MySandboxGame.Log.Error("Voxel file '{0}' does not exist!", args);
            }
            else
            {
                if (Path.GetExtension(originalVoxFile) != ".vox")
                {
                    object[] args = new object[] { originalVoxFile };
                    MySandboxGame.Log.Warning("Unexpected voxel file extensions in path: '{0}'", args);
                }
                try
                {
                    using (MyCompressionFileLoad load = new MyCompressionFileLoad(originalVoxFile))
                    {
                        using (Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create))
                        {
                            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress))
                            {
                                using (BufferedStream stream3 = new BufferedStream(stream2))
                                {
                                    stream3.WriteNoAlloc("Cell", null);
                                    stream3.Write7BitEncodedInt(load.GetInt32());
                                    byte[] output = new byte[0x4000];
                                    for (int i = load.GetBytes(output.Length, output); i != 0; i = load.GetBytes(output.Length, output))
                                    {
                                        stream3.Write(output, 0, i);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    object[] args = new object[] { originalVoxFile, exception.Message };
                    MySandboxGame.Log.Error("While updating voxel storage '{0}' to new format: {1}", args);
                }
            }
        }

        void VRage.ModAPI.IMyStorage.DeleteRange(MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify)
        {
            this.DeleteRange(dataToWrite, voxelRangeMin, voxelRangeMax, notify);
        }

        void VRage.ModAPI.IMyStorage.ExecuteOperationFast<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, bool notifyRangeChanged) where TVoxelOperator: struct, IVoxelOperator
        {
            this.ExecuteOperationFast<TVoxelOperator>(ref voxelOperator, dataToWrite, ref voxelRangeMin, ref voxelRangeMax, notifyRangeChanged, false);
        }

        void VRage.ModAPI.IMyStorage.NotifyRangeChanged(ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, MyStorageDataTypeFlags dataChanged)
        {
            this.OnRangeChanged(voxelRangeMin, voxelRangeMax, dataChanged);
        }

        void VRage.ModAPI.IMyStorage.ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax)
        {
            if (lodIndex < 0x10)
            {
                this.ReadRange(target, dataToRead, lodIndex, lodVoxelRangeMin, lodVoxelRangeMax);
            }
        }

        void VRage.ModAPI.IMyStorage.WriteRange(MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify, bool skipCache)
        {
            this.WriteRange(source, dataToWrite, voxelRangeMin, voxelRangeMax, notify, skipCache);
        }

        private void WriteChunk(VoxelChunk chunk)
        {
            using (this.StorageLock.AcquireExclusiveUsing())
            {
                using (chunk.Lock.AcquireExclusiveUsing())
                {
                    if (chunk.Dirty != MyStorageDataTypeFlags.None)
                    {
                        Vector3I voxelRangeMin = chunk.Coords << 3;
                        Vector3I voxelRangeMax = (Vector3I) (((chunk.Coords + 1) << 3) - 1);
                        MyStorageDataWriteOperator source = new MyStorageDataWriteOperator(chunk.MakeData());
                        this.WriteRangeInternal<MyStorageDataWriteOperator>(ref source, chunk.Dirty, ref voxelRangeMin, ref voxelRangeMax);
                        chunk.Dirty = MyStorageDataTypeFlags.None;
                    }
                }
            }
        }

        public bool WritePending(bool force = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            using (this.m_cacheLock.AcquireSharedUsing())
            {
                while (((stopwatch.ElapsedMilliseconds < 6L) | force) && (this.m_pendingChunksToWrite.Count > 0))
                {
                    VoxelChunk chunk;
                    Vector3I vectori;
                    this.DequeueDirtyChunk(out chunk, out vectori);
                    if ((chunk != null) && (chunk.Dirty != MyStorageDataTypeFlags.None))
                    {
                        this.WriteChunk(chunk);
                    }
                }
            }
            return (this.m_pendingChunksToWrite.Count == 0);
        }

        public void WriteRange(MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notifyRangeChanged = true, bool skipCache = false)
        {
            MyStorageDataWriteOperator voxelOperator = new MyStorageDataWriteOperator(source);
            this.ExecuteOperationFast<MyStorageDataWriteOperator>(ref voxelOperator, dataToWrite, ref voxelRangeMin, ref voxelRangeMax, notifyRangeChanged, skipCache);
        }

        protected abstract void WriteRangeInternal<TOperator>(ref TOperator source, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax) where TOperator: struct, IVoxelOperator;
        protected void WriteStorageAccess(Stream stream)
        {
            stream.WriteNoAlloc((ushort) this.m_accessGridLod);
        }

        public IEnumerator<KeyValuePair<Vector3I, MyTimeSpan>> AccessEnumerator =>
            this.m_access.GetEnumerator();

        private static MyVoxelOperationsSessionComponent OperationsComponent =>
            MySession.Static.GetComponent<MyVoxelOperationsSessionComponent>();

        public bool CachedWrites
        {
            get => 
                (this.m_cachedWrites && MyVoxelOperationsSessionComponent.EnableCache);
            set => 
                (this.m_cachedWrites = value);
        }

        public bool HasPendingWrites =>
            (this.m_pendingChunksToWrite.Count > 0);

        public bool HasCachedChunks =>
            ((this.m_chunksbyAge.Count - this.m_pendingChunksToWrite.Count) > 0);

        public int CachedChunksCount =>
            this.m_cachedChunks.Count;

        public int PendingCachedChunksCount =>
            this.m_pendingChunksToWrite.Count;

        public abstract IMyStorageDataProvider DataProvider { get; set; }

        public bool DeleteSupported =>
            (this.DataProvider != null);

        public bool Shared { get; internal set; }

        public uint StorageId { get; private set; }

        public MyVoxelGeometry Geometry =>
            this.m_geometry;

        public Vector3I Size { get; protected set; }

        public bool AreCompressedDataCached
        {
            get
            {
                object compressedDataLock = this.m_compressedDataLock;
                lock (compressedDataLock)
                {
                    return (this.m_compressedData != null);
                }
            }
        }

        public bool Closed =>
            (Interlocked.CompareExchange(ref this.m_closed, 0, 0) != 0);

        public bool MarkedForClose =>
            ((Interlocked.CompareExchange(ref this.m_pinAndCloseMark, 0, 0) & -2147483648) != 0);

        Vector3I VRage.ModAPI.IMyStorage.Size =>
            this.Size;

        public enum MyAccessType
        {
            Read,
            Write,
            Delete
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyVoxelObjectDefinition
        {
            public readonly string FilePath;
            public readonly Dictionary<byte, byte> Changes;
            public MyVoxelObjectDefinition(string filePath, Dictionary<byte, byte> changes)
            {
                this.FilePath = filePath;
                this.Changes = changes;
            }

            public override int GetHashCode()
            {
                int num = (0x11 * 0x1cfaa2db) + this.FilePath.GetHashCode();
                if (this.Changes != null)
                {
                    foreach (KeyValuePair<byte, byte> pair in this.Changes)
                    {
                        byte num2 = pair.Value;
                        num = (((num * 0x1cfaa2db) + pair.Key.GetHashCode()) * 0x1cfaa2db) + num2.GetHashCode();
                    }
                }
                return num;
            }
        }

        public class VoxelChunk
        {
            public const int SizeBits = 3;
            public const int Size = 8;
            public const int Volume = 0x200;
            public readonly Vector3I Coords;
            public byte MaxLod;
            public static readonly int TotalVolume = 0x349;
            public static readonly Vector3I SizeVector = new Vector3I(8);
            public static readonly Vector3I MaxVector = new Vector3I(7);
            public byte[] Material;
            public byte[] Content;
            public MyStorageDataTypeFlags Dirty;
            public MyStorageDataTypeFlags Cached;
            public int HitCount;
            internal int TreeProxy;
            public FastResourceLock Lock = new FastResourceLock();

            public VoxelChunk(Vector3I coords)
            {
                this.Coords = coords;
                this.Material = new byte[TotalVolume];
                this.Content = new byte[TotalVolume];
            }

            public void ExecuteOperator<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeFlags dataTypes, ref Vector3I targetOffset, ref Vector3I min, ref Vector3I max) where TVoxelOperator: IVoxelOperator
            {
                if (dataTypes.Requests(MyStorageDataTypeEnum.Content))
                {
                    this.ExecuteOperator<TVoxelOperator>(ref voxelOperator, MyStorageDataTypeEnum.Content, this.Content, targetOffset, min, max);
                }
                if (dataTypes.Requests(MyStorageDataTypeEnum.Material))
                {
                    this.ExecuteOperator<TVoxelOperator>(ref voxelOperator, MyStorageDataTypeEnum.Material, this.Material, targetOffset, min, max);
                }
                this.Cached |= dataTypes;
                this.Dirty |= dataTypes;
                this.MaxLod = 0;
            }

            private unsafe void ExecuteOperator<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeEnum dataType, byte[] dataArray, Vector3I tofft, Vector3I min, Vector3I max) where TVoxelOperator: IVoxelOperator
            {
                Vector3I vectori;
                int num = 8;
                int num2 = num * num;
                int* numPtr1 = (int*) ref min.Y;
                numPtr1[0] *= num;
                int* numPtr2 = (int*) ref min.Z;
                numPtr2[0] *= num2;
                int* numPtr3 = (int*) ref max.Y;
                numPtr3[0] *= num;
                int* numPtr4 = (int*) ref max.Z;
                numPtr4[0] *= num2;
                int z = min.Z;
                vectori.Z = tofft.Z;
                while (z <= max.Z)
                {
                    int y = min.Y;
                    vectori.Y = tofft.Y;
                    while (true)
                    {
                        if (y > max.Y)
                        {
                            z += num2;
                            int* numPtr7 = (int*) ref vectori.Z;
                            numPtr7[0]++;
                            break;
                        }
                        int x = min.X;
                        vectori.X = tofft.X;
                        while (true)
                        {
                            if (x > max.X)
                            {
                                y += num;
                                int* numPtr6 = (int*) ref vectori.Y;
                                numPtr6[0]++;
                                break;
                            }
                            voxelOperator.Op(ref vectori, dataType, ref dataArray[(z + y) + x]);
                            x++;
                            int* numPtr5 = (int*) ref vectori.X;
                            numPtr5[0]++;
                        }
                    }
                }
            }

            public MyStorageData MakeData() => 
                new MyStorageData(SizeVector, this.Content, this.Material);

            public void ReadLod(MyStorageData target, MyStorageDataTypeFlags dataTypes, ref Vector3I targetOffset, int lodIndex, ref Vector3I min, ref Vector3I max)
            {
                if (lodIndex > this.MaxLod)
                {
                    this.UpdateLodData(lodIndex);
                }
                if (dataTypes.Requests(MyStorageDataTypeEnum.Content))
                {
                    this.ReadLod(target, MyStorageDataTypeEnum.Content, this.Content, targetOffset, lodIndex, min, max);
                }
                if (dataTypes.Requests(MyStorageDataTypeEnum.Material))
                {
                    this.ReadLod(target, MyStorageDataTypeEnum.Material, this.Material, targetOffset, lodIndex, min, max);
                }
                this.HitCount++;
            }

            private unsafe void ReadLod(MyStorageData target, MyStorageDataTypeEnum dataType, byte[] dataArray, Vector3I tofft, int lod, Vector3I min, Vector3I max)
            {
                byte* numPtr;
                byte[] pinned buffer;
                byte* numPtr2;
                byte[] pinned buffer2;
                int num = 0;
                for (int i = 0; i < lod; i++)
                {
                    num += 0x200 >> (((i + i) + i) & 0x1f);
                }
                int num2 = 8 >> (lod & 0x1f);
                int num3 = num2 * num2;
                int* numPtr1 = (int*) ref min.Y;
                numPtr1[0] *= num2;
                int* numPtr4 = (int*) ref min.Z;
                numPtr4[0] *= num3;
                int* numPtr5 = (int*) ref max.Y;
                numPtr5[0] *= num2;
                int* numPtr6 = (int*) ref max.Z;
                numPtr6[0] *= num3;
                int stepX = target.StepX;
                int stepY = target.StepY;
                int stepZ = target.StepZ;
                int* numPtr7 = (int*) ref tofft.Y;
                numPtr7[0] *= stepY;
                int* numPtr8 = (int*) ref tofft.Z;
                numPtr8[0] *= stepZ;
                if (((buffer = dataArray) == null) || (buffer.Length == 0))
                {
                    numPtr = null;
                }
                else
                {
                    numPtr = buffer;
                }
                if (((buffer2 = target[dataType]) == null) || (buffer2.Length == 0))
                {
                    numPtr2 = null;
                }
                else
                {
                    numPtr2 = buffer2;
                }
                byte* numPtr3 = numPtr + num;
                int z = min.Z;
                int num13 = tofft.Z;
                while (z <= max.Z)
                {
                    int y = min.Y;
                    int num12 = tofft.Y;
                    while (true)
                    {
                        if (y > max.Y)
                        {
                            z += num3;
                            num13 += stepZ;
                            break;
                        }
                        int x = min.X;
                        int num11 = tofft.X;
                        while (true)
                        {
                            if (x > max.X)
                            {
                                y += num2;
                                num12 += stepY;
                                break;
                            }
                            numPtr2[(num13 + num12) + num11] = numPtr3[(z + y) + x];
                            x++;
                            num11 += stepX;
                        }
                    }
                }
                buffer2 = null;
                buffer = null;
            }

            public void UpdateLodData(int lod)
            {
                for (int i = this.MaxLod + 1; i <= lod; i++)
                {
                    UpdateLodDataInternal(i, this.Content, MyOctreeNode.ContentFilter);
                    UpdateLodDataInternal(i, this.Material, MyOctreeNode.MaterialFilter);
                }
                this.MaxLod = (byte) lod;
            }

            private static unsafe void UpdateLodDataInternal(int lod, byte[] dataArray, MyOctreeNode.FilterFunction filter)
            {
                ulong num8;
                byte* numPtr2;
                byte[] pinned buffer;
                int num = 0;
                for (int i = 0; i < (lod - 1); i++)
                {
                    num += 0x200 >> (((i + i) + i) & 0x1f);
                }
                int num2 = 8 >> (lod & 0x1f);
                int num3 = num2 * num2;
                int num4 = num3 * num2;
                int num5 = 8 >> ((lod - 1) & 0x1f);
                int num6 = num5 * num5;
                int num7 = num6 * num5;
                byte* pData = (byte*) &num8;
                if (((buffer = dataArray) == null) || (buffer.Length == 0))
                {
                    numPtr2 = null;
                }
                else
                {
                    numPtr2 = buffer;
                }
                byte* numPtr3 = numPtr2 + num;
                byte* numPtr4 = numPtr3 + num7;
                int num10 = 0;
                while (num10 < num4)
                {
                    int num11 = num10 << 3;
                    int num12 = (num10 << 3) + num6;
                    int num13 = 0;
                    while (true)
                    {
                        if (num13 >= num3)
                        {
                            num10 += num3;
                            break;
                        }
                        int num14 = num13 << 2;
                        int num15 = (num13 << 2) + num5;
                        int num16 = 0;
                        while (true)
                        {
                            if (num16 >= num2)
                            {
                                num13 += num2;
                                break;
                            }
                            int num17 = num16 << 1;
                            int num18 = (num16 << 1) + 1;
                            pData[0] = numPtr3[(num11 + num14) + num17];
                            pData[1] = numPtr3[(num11 + num14) + num18];
                            pData[2] = numPtr3[(num11 + num15) + num17];
                            pData[3] = numPtr3[(num11 + num15) + num18];
                            pData[4] = numPtr3[(num12 + num14) + num17];
                            pData[5] = numPtr3[(num12 + num14) + num18];
                            pData[6] = numPtr3[(num12 + num15) + num17];
                            pData[7] = numPtr3[(num12 + num15) + num18];
                            numPtr4[(num16 + num13) + num10] = filter(pData, lod);
                            num16++;
                        }
                    }
                }
                buffer = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WriteCacheStats
        {
            public int QueuedWrites;
            public int CachedChunks;
            public IEnumerable<KeyValuePair<Vector3I, MyStorageBase.VoxelChunk>> Chunks;
        }
    }
}

