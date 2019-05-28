namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    internal class MyMicroOctreeLeaf : IMyOctreeLeafNode
    {
        private const bool DEBUG_WRITES = false;
        private MySparseOctree m_octree;
        private MyStorageDataTypeEnum m_dataType;
        private Vector3I m_voxelRangeMin;

        public MyMicroOctreeLeaf(MyStorageDataTypeEnum dataType, int height, Vector3I voxelRangeMin)
        {
            this.m_octree = (dataType != MyStorageDataTypeEnum.Content) ? new MySparseOctree(height, MyOctreeNode.MaterialFilter, 0xff) : new MySparseOctree(height, MyOctreeNode.ContentFilter, 0);
            this.m_dataType = dataType;
            this.m_voxelRangeMin = voxelRangeMin;
        }

        internal void BuildFrom(byte singleValue)
        {
            this.m_octree.Build(singleValue);
        }

        internal void BuildFrom(MyStorageData source)
        {
            MyStorageData.MortonEnumerator data = new MyStorageData.MortonEnumerator(source, this.m_dataType);
            this.m_octree.Build<MyStorageData.MortonEnumerator>(data);
        }

        internal void DebugDraw(IMyDebugDrawBatchAabb batch, Vector3 worldPos, MyVoxelDebugDrawMode mode)
        {
            this.m_octree.DebugDraw(batch, worldPos, mode);
        }

        public ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true)
        {
            BoundingBoxI xi = box;
            xi.Translate(-this.m_voxelRangeMin);
            return this.m_octree.Intersect(ref xi, lod, exhaustiveContainmentCheck);
        }

        public unsafe bool Intersect(ref LineD line, out double startOffset, out double endOffset)
        {
            Vector3D* vectordPtr1 = (Vector3D*) ref line.From;
            vectordPtr1[0] -= this.m_voxelRangeMin;
            Vector3D* vectordPtr2 = (Vector3D*) ref line.To;
            vectordPtr2[0] -= this.m_voxelRangeMin;
            if (!this.m_octree.Intersect(ref line, out startOffset, out endOffset))
            {
                return false;
            }
            Vector3D* vectordPtr3 = (Vector3D*) ref line.From;
            vectordPtr3[0] += this.m_voxelRangeMin;
            Vector3D* vectordPtr4 = (Vector3D*) ref line.To;
            vectordPtr4[0] += this.m_voxelRangeMin;
            return true;
        }

        internal void ReadFrom(MyOctreeStorage.ChunkHeader header, Stream stream)
        {
            if (this.m_octree == null)
            {
                this.m_octree = new MySparseOctree(0, (header.ChunkType == MyOctreeStorage.ChunkTypeEnum.ContentLeafOctree) ? MyOctreeNode.ContentFilter : MyOctreeNode.MaterialFilter, 0);
            }
            this.m_octree.ReadFrom(header, stream);
        }

        void IMyOctreeLeafNode.ExecuteOperation<TOperator>(ref TOperator source, ref Vector3I readOffset, ref Vector3I min, ref Vector3I max) where TOperator: struct, IVoxelOperator
        {
            this.m_octree.ExecuteOperation<TOperator>(ref source, this.m_dataType, ref readOffset, ref min, ref max);
        }

        byte IMyOctreeLeafNode.GetFilteredValue() => 
            this.m_octree.GetFilteredValue();

        void IMyOctreeLeafNode.OnDataProviderChanged(IMyStorageDataProvider newProvider)
        {
        }

        void IMyOctreeLeafNode.ReadRange(MyStorageData target, MyStorageDataTypeFlags types, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, ref MyVoxelRequestFlags flags)
        {
            this.m_octree.ReadRange(target, this.m_dataType, ref writeOffset, lodIndex, ref minInLod, ref maxInLod);
            flags = 0;
        }

        void IMyOctreeLeafNode.ReplaceValues(Dictionary<byte, byte> oldToNewValueMap)
        {
            this.m_octree.ReplaceValues(oldToNewValueMap);
        }

        public bool TryGetUniformValue(out byte uniformValue)
        {
            if (this.m_octree.IsAllSame)
            {
                uniformValue = this.m_octree.GetFilteredValue();
                return true;
            }
            uniformValue = 0;
            return false;
        }

        internal void WriteTo(Stream stream)
        {
            this.m_octree.WriteTo(stream);
        }

        Vector3I IMyOctreeLeafNode.VoxelRangeMin =>
            this.m_voxelRangeMin;

        bool IMyOctreeLeafNode.ReadOnly =>
            false;

        MyOctreeStorage.ChunkTypeEnum IMyOctreeLeafNode.SerializedChunkType =>
            ((this.m_dataType == MyStorageDataTypeEnum.Content) ? MyOctreeStorage.ChunkTypeEnum.ContentLeafOctree : MyOctreeStorage.ChunkTypeEnum.MaterialLeafOctree);

        int IMyOctreeLeafNode.SerializedChunkSize =>
            this.m_octree.SerializedSize;
    }
}

