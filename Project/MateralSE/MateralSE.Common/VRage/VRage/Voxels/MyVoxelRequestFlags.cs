namespace VRage.Voxels
{
    using System;

    [Flags]
    public enum MyVoxelRequestFlags
    {
        SurfaceMaterial = 1,
        ConsiderContent = 2,
        ForPhysics = 4,
        EmptyData = 8,
        FullContent = 0x10,
        OneMaterial = 0x20,
        AdviseCache = 0x40,
        ContentChecked = 0x80,
        ContentCheckedDeep = 0x100,
        UseNativeProvider = 0x200,
        Postprocess = 0x400,
        DoNotCheck = 0x10000,
        PreciseOrePositions = 0x20000,
        RequestFlags = 3
    }
}

