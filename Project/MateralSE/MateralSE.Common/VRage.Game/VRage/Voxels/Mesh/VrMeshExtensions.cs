namespace VRage.Voxels.Mesh
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Voxels;

    public static class VrMeshExtensions
    {
        public static bool IsEmpty(this VrVoxelMesh self) => 
            ((self == null) || (self.TriangleCount == 0));
    }
}

