namespace VRage.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelDataRequest
    {
        public int Lod;
        public Vector3I MinInLod;
        public Vector3I MaxInLod;
        public Vector3I Offset;
        public MyStorageDataTypeFlags RequestedData;
        public MyVoxelRequestFlags RequestFlags;
        public MyVoxelRequestFlags Flags;
        public MyStorageData Target;
        public string ToStringShort() => 
            $"lod{this.Lod}: {this.SizeLinear}voxels";

        public int SizeLinear =>
            ((this.MaxInLod - this.MinInLod) + Vector3I.One).Size;
    }
}

