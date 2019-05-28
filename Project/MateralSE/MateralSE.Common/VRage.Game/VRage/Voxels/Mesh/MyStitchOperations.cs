namespace VRage.Voxels.Mesh
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Voxels.Sewing;

    public static class MyStitchOperations
    {
        public static bool Contains(this VrSewOperation self, VrSewOperation flags) => 
            ((self & flags) == flags);

        public static VrSewOperation GetInstruction(bool x = false, bool y = false, bool z = false, bool xy = false, bool xz = false, bool yz = false, bool xyz = false) => 
            ((VrSewOperation) ((byte) (((((((0 | (!x ? 0 : 2)) | (!y ? 0 : 4)) | (!z ? 0 : 6)) | (!xy ? 0 : 8)) | (!xz ? 0 : 10)) | (!yz ? 0 : 12)) | (!xyz ? 0 : 14))));

        public static VrSewOperation With(this VrSewOperation self, VrSewOperation flags) => 
            (self | flags);

        public static VrSewOperation Without(this VrSewOperation self, VrSewOperation flags) => 
            (self & ((byte) ~flags));
    }
}

