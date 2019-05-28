namespace VRage.Voxels.Clipmap
{
    using System;

    public class MyVoxelClipmapSettingsPresets
    {
        public static MyVoxelClipmapSettings[] NormalSettings = new MyVoxelClipmapSettings[] { MyVoxelClipmapSettings.Create(4, 3, 2f, 4, 0x4000), MyVoxelClipmapSettings.Create(5, 3, 2f, 4, 0x4000), MyVoxelClipmapSettings.Create(5, 3, 3f, -1, -1), MyVoxelClipmapSettings.Create(5, 0xfa0, 9f, -1, -1) };
        public static MyVoxelClipmapSettings[] PlanetSettings = new MyVoxelClipmapSettings[] { MyVoxelClipmapSettings.Create(4, 2, 2f, -1, -1), MyVoxelClipmapSettings.Create(5, 2, 2f, -1, -1), MyVoxelClipmapSettings.Create(5, 3, 2f, -1, -1), MyVoxelClipmapSettings.Create(5, 3, 3f, -1, -1) };
    }
}

