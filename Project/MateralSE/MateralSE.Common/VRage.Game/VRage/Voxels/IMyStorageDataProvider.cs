namespace VRage.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRageMath;

    public interface IMyStorageDataProvider
    {
        void Close();
        void DebugDraw(ref MatrixD worldMatrix);
        ContainmentType Intersect(BoundingBoxI box, int lod);
        bool Intersect(ref LineD line, out double startOffset, out double endOffset);
        void PostProcess(VrVoxelMesh mesh, MyStorageDataTypeFlags dataTypes);
        void ReadFrom(int storageVersion, Stream stream, int size, ref bool isOldFormat);
        void ReadRange(ref MyVoxelDataRequest request, bool detectOnly = false);
        void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataType, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod);
        void ReindexMaterials(Dictionary<byte, byte> oldToNewIndexMap);
        void WriteTo(Stream stream);

        int SerializedSize { get; }
    }
}

