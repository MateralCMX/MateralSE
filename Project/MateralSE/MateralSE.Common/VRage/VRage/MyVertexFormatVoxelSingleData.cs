namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Import;
    using VRageMath;
    using VRageMath.PackedVector;

    [StructLayout(LayoutKind.Explicit)]
    public struct MyVertexFormatVoxelSingleData
    {
        [FieldOffset(0)]
        public Vector3 Position;
        [FieldOffset(12)]
        public Byte4 Material;
        [FieldOffset(0x10)]
        public Byte4 PackedNormal;
        [FieldOffset(20)]
        public uint PackedColorShift;

        public Vector3 Normal
        {
            get => 
                VF_Packer.UnpackNormal(ref this.PackedNormal);
            set => 
                (this.PackedNormal.PackedValue = VF_Packer.PackNormal(ref value));
        }
    }
}

