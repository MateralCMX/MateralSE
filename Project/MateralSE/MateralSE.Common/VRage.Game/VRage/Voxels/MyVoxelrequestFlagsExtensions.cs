namespace VRage.Voxels
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyVoxelrequestFlagsExtensions
    {
        public static bool HasFlags(this MyVoxelRequestFlags self, MyVoxelRequestFlags other) => 
            ((self & other) == other);
    }
}

