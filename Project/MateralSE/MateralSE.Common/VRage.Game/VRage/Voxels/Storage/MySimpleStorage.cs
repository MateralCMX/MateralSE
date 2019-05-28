namespace VRage.Voxels.Storage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;

    public class MySimpleStorage : VRage.Game.Voxels.IMyStorage, VRage.ModAPI.IMyStorage
    {
        private static int m_storegeIds;
        private MyStorageData m_data;
        [CompilerGenerated]
        private Action<Vector3I, Vector3I, MyStorageDataTypeFlags> RangeChanged;

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

        public MySimpleStorage(int size)
        {
            this.Size = new Vector3I(size);
            this.m_data = new MyStorageData(MyStorageDataTypeFlags.All);
            this.m_data.Resize(this.Size);
            this.StorageId = (uint) Interlocked.Increment(ref m_storegeIds);
        }

        public void Close()
        {
            this.Closed = true;
        }

        public VRage.Game.Voxels.IMyStorage Copy()
        {
            throw new NotImplementedException();
        }

        public void DebugDraw(ref MatrixD worldMatrix, MyVoxelDebugDrawMode mode)
        {
        }

        public void DeleteRange(MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify)
        {
        }

        public void ExecuteOperationFast<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, bool notifyRangeChanged) where TVoxelOperator: struct, IVoxelOperator
        {
            if ((dataToWrite & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                this.ExecuteOperationInternal<TVoxelOperator>(ref voxelOperator, MyStorageDataTypeEnum.Content, ref voxelRangeMin, ref voxelRangeMax);
            }
            if ((dataToWrite & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                this.ExecuteOperationInternal<TVoxelOperator>(ref voxelOperator, MyStorageDataTypeEnum.Material, ref voxelRangeMin, ref voxelRangeMax);
            }
        }

        private unsafe void ExecuteOperationInternal<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeEnum dataType, ref Vector3I min, ref Vector3I max) where TVoxelOperator: struct, IVoxelOperator
        {
            Vector3I vectori;
            Vector3I step = this.m_data.Step;
            byte[] buffer = this.m_data[dataType];
            vectori.Z = min.Z;
            int num3 = 0;
            while (vectori.Z <= max.Z)
            {
                vectori.Y = min.Y;
                int num2 = 0;
                while (true)
                {
                    if (vectori.Y > max.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.Z;
                        numPtr3[0]++;
                        num3 += step.Z;
                        break;
                    }
                    int num4 = num2 + num3;
                    vectori.X = min.X;
                    int num = 0;
                    while (true)
                    {
                        if (vectori.X > max.X)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            num2 += step.Y;
                            break;
                        }
                        voxelOperator.Op(ref vectori, dataType, ref buffer[num + num4]);
                        int* numPtr1 = (int*) ref vectori.X;
                        numPtr1[0]++;
                        num += step.X;
                    }
                }
            }
        }

        public byte[] GetVoxelData()
        {
            byte[] buffer;
            this.Save(out buffer);
            return buffer;
        }

        public bool Intersect(ref LineD line) => 
            true;

        public ContainmentType Intersect(ref BoundingBox box, bool lazy) => 
            ContainmentType.Disjoint;

        public ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true) => 
            ContainmentType.Intersects;

        public void NotifyChanged(Vector3I voxelRangeMin, Vector3I voxelRangeMax, MyStorageDataTypeFlags changedData)
        {
        }

        public void NotifyRangeChanged(ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, MyStorageDataTypeFlags dataChanged)
        {
        }

        public void OverwriteAllMaterials(byte materialIndex)
        {
        }

        public StoragePin Pin() => 
            new StoragePin(this);

        public void PinAndExecute(Action action)
        {
        }

        public void PinAndExecute(Action<VRage.ModAPI.IMyStorage> action)
        {
        }

        public void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax)
        {
            MyVoxelRequestFlags requestFlags = 0;
            this.ReadRange(target, dataToRead, lodIndex, lodVoxelRangeMin, lodVoxelRangeMax, ref requestFlags);
        }

        public void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, ref MyVoxelRequestFlags requestFlags)
        {
            Vector3I vectori1 = Vector3I.Min(lodVoxelRangeMax, (this.Size >> lodIndex) - 1);
            lodVoxelRangeMax = vectori1;
            if ((dataToRead & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                target.CopyRange(this.m_data, lodVoxelRangeMin, lodVoxelRangeMax, Vector3I.Zero, MyStorageDataTypeEnum.Content);
            }
            if ((dataToRead & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                target.CopyRange(this.m_data, lodVoxelRangeMin, lodVoxelRangeMax, Vector3I.Zero, MyStorageDataTypeEnum.Material);
            }
        }

        public void Reset(MyStorageDataTypeFlags dataToReset)
        {
        }

        public unsafe void Save(out byte[] outCompressedData)
        {
            byte* numPtr;
            byte[] pinned buffer;
            outCompressedData = new byte[(this.m_data.SizeLinear * 2) + 4];
            int index = 0;
            if (((buffer = outCompressedData) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            *((int*) numPtr) = this.m_data.Size3D.X;
            index += 4;
            for (int i = 0; i < this.m_data.SizeLinear; i++)
            {
                numPtr[index] = this.m_data.Content(i);
                index = (index + 1) + 1;
                numPtr[index] = this.m_data.Material(i);
            }
            buffer = null;
        }

        public void SetCompressedDataCache(byte[] data)
        {
        }

        public void Unpin()
        {
        }

        public void WriteRange(MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify, bool skipCache)
        {
            if ((dataToWrite & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                this.m_data.CopyRange(source, voxelRangeMin, voxelRangeMax, Vector3I.Zero, MyStorageDataTypeEnum.Content);
            }
            if ((dataToWrite & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                this.m_data.CopyRange(source, voxelRangeMin, voxelRangeMax, Vector3I.Zero, MyStorageDataTypeEnum.Material);
            }
        }

        public MyStorageData Data =>
            this.m_data;

        public bool AreCompressedDataCached =>
            true;

        public bool MarkedForClose =>
            false;

        public Vector3I Size { get; private set; }

        public uint StorageId { get; private set; }

        public bool Shared { get; private set; }

        public bool Closed { get; private set; }

        public bool DeleteSupported =>
            false;

        public IMyStorageDataProvider DataProvider =>
            null;
    }
}

