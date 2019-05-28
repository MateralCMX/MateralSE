namespace VRageRender.Import
{
    using System;
    using System.Reflection;

    [Obfuscation(Feature="cw symbol renaming", ApplyToMembers=true, Exclude=true)]
    public enum MyMeshDrawTechnique : byte
    {
        MESH = 0,
        VOXELS_DEBRIS = 1,
        VOXEL_MAP = 2,
        ALPHA_MASKED = 3,
        ALPHA_MASKED_SINGLE_SIDED = 4,
        FOLIAGE = 5,
        DECAL = 6,
        DECAL_NOPREMULT = 7,
        DECAL_CUTOUT = 8,
        HOLO = 9,
        VOXEL_MAP_SINGLE = 10,
        VOXEL_MAP_MULTI = 11,
        SKINNED = 12,
        MESH_INSTANCED = 13,
        MESH_INSTANCED_SKINNED = 14,
        GLASS = 15,
        MESH_INSTANCED_GENERIC = 0x10,
        MESH_INSTANCED_GENERIC_MASKED = 0x11,
        ATMOSPHERE = 0x12,
        CLOUD_LAYER = 0x13,
        COUNT = 20
    }
}

