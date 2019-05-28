namespace VRageRender.Import
{
    using System;
    using System.Reflection;

    [Obfuscation(Feature="cw symbol renaming", ApplyToMembers=true, Exclude=true)]
    public enum MyMeshDrawTechniqueOld : byte
    {
        MESH = 0,
        VOXELS_DEBRIS = 1,
        VOXEL_MAP = 2,
        ALPHA_MASKED = 3,
        FOLIAGE = 4,
        DECAL = 5,
        DECAL_CUTOUT = 6,
        HOLO = 7,
        VOXEL_MAP_SINGLE = 8,
        VOXEL_MAP_MULTI = 9,
        SKINNED = 10,
        GLASS = 11
    }
}

