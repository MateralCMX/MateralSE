namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Voxels;
    using VRageMath;

    internal class MyProviderLeaf : IMyOctreeLeafNode
    {
        [ThreadStatic]
        private static MyStorageData m_filteredValueBuffer;
        private IMyStorageDataProvider m_provider;
        private MyStorageDataTypeEnum m_dataType;
        private MyCellCoord m_cell;

        public MyProviderLeaf(IMyStorageDataProvider provider, MyStorageDataTypeEnum dataType, ref MyCellCoord cell)
        {
            this.m_provider = provider;
            this.m_dataType = dataType;
            this.m_cell = cell;
        }

        [Conditional("DEBUG")]
        private void AssertRangeIsInside(int lodIndex, ref Vector3I globalMin, ref Vector3I globalMax)
        {
            int num = this.m_cell.Lod - lodIndex;
            int num2 = 1 << (num & 0x1f);
            Vector3 vector1 = (this.m_cell.CoordInLod << num) + (num2 - 1);
        }

        public ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true) => 
            this.m_provider.Intersect(box, lod);

        public bool Intersect(ref LineD line, out double startOffset, out double endOffset) => 
            this.m_provider.Intersect(ref line, out startOffset, out endOffset);

        void IMyOctreeLeafNode.ExecuteOperation<TOperator>(ref TOperator source, ref Vector3I readOffset, ref Vector3I min, ref Vector3I max) where TOperator: struct, IVoxelOperator
        {
            throw new NotSupportedException();
        }

        byte IMyOctreeLeafNode.GetFilteredValue()
        {
            MyStorageData filteredValueBuffer = FilteredValueBuffer;
            this.m_provider.ReadRange(filteredValueBuffer, this.m_dataType.ToFlags(), ref Vector3I.Zero, this.m_cell.Lod, ref this.m_cell.CoordInLod, ref this.m_cell.CoordInLod);
            return ((this.m_dataType == MyStorageDataTypeEnum.Material) ? filteredValueBuffer.Material(0) : filteredValueBuffer.Content(0));
        }

        void IMyOctreeLeafNode.OnDataProviderChanged(IMyStorageDataProvider newProvider)
        {
            this.m_provider = newProvider;
        }

        void IMyOctreeLeafNode.ReadRange(MyStorageData target, MyStorageDataTypeFlags types, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, ref MyVoxelRequestFlags flags)
        {
            int num = this.m_cell.Lod - lodIndex;
            Vector3I vectori = this.m_cell.CoordInLod << num;
            MyVoxelDataRequest request = new MyVoxelDataRequest {
                Target = target,
                Offset = writeOffset,
                Lod = lodIndex,
                MinInLod = (Vector3I) (minInLod + vectori),
                MaxInLod = (Vector3I) (maxInLod + vectori),
                RequestFlags = flags,
                RequestedData = types
            };
            this.m_provider.ReadRange(ref request, false);
            flags = request.Flags;
        }

        void IMyOctreeLeafNode.ReplaceValues(Dictionary<byte, byte> oldToNewValueMap)
        {
        }

        public bool TryGetUniformValue(out byte uniformValue)
        {
            MyStorageData filteredValueBuffer = FilteredValueBuffer;
            MyVoxelDataRequest request = new MyVoxelDataRequest {
                Target = null,
                Offset = Vector3I.Zero,
                Lod = this.m_cell.Lod,
                MinInLod = this.m_cell.CoordInLod,
                MaxInLod = this.m_cell.CoordInLod,
                RequestedData = this.m_dataType.ToFlags()
            };
            this.m_provider.ReadRange(ref request, true);
            if ((request.Flags & MyVoxelRequestFlags.EmptyData) > 0)
            {
                uniformValue = (this.m_dataType == MyStorageDataTypeEnum.Material) ? ((byte) 0xff) : ((byte) 0);
                return true;
            }
            if ((this.m_dataType != MyStorageDataTypeEnum.Content) || ((request.Flags & MyVoxelRequestFlags.FullContent) <= 0))
            {
                uniformValue = 0;
                return false;
            }
            uniformValue = 0xff;
            return true;
        }

        private static MyStorageData FilteredValueBuffer
        {
            get
            {
                if (m_filteredValueBuffer == null)
                {
                    m_filteredValueBuffer = new MyStorageData(MyStorageDataTypeFlags.All);
                    m_filteredValueBuffer.Resize(Vector3I.One);
                }
                return m_filteredValueBuffer;
            }
        }

        MyOctreeStorage.ChunkTypeEnum IMyOctreeLeafNode.SerializedChunkType =>
            ((this.m_dataType == MyStorageDataTypeEnum.Content) ? MyOctreeStorage.ChunkTypeEnum.ContentLeafProvider : MyOctreeStorage.ChunkTypeEnum.MaterialLeafProvider);

        int IMyOctreeLeafNode.SerializedChunkSize =>
            0;

        Vector3I IMyOctreeLeafNode.VoxelRangeMin =>
            (this.m_cell.CoordInLod << this.m_cell.Lod);

        bool IMyOctreeLeafNode.ReadOnly =>
            true;
    }
}

