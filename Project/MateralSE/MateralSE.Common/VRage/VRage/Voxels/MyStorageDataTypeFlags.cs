namespace VRage.Voxels
{
    using System;

    [Flags]
    public enum MyStorageDataTypeFlags : byte
    {
        None = 0,
        Content = 1,
        Material = 2,
        ContentAndMaterial = 3,
        All = 3
    }
}

