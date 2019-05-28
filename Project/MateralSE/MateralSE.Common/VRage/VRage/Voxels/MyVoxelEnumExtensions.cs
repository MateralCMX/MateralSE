namespace VRage.Voxels
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyVoxelEnumExtensions
    {
        public static bool Requests(this MyStorageDataTypeFlags self, MyStorageDataTypeEnum value) => 
            ((((int) self) & (1 << (value & 0x1f))) != ((int) MyStorageDataTypeFlags.None));

        public static MyStorageDataTypeFlags ToFlags(this MyStorageDataTypeEnum self) => 
            ((MyStorageDataTypeFlags) ((byte) (1 << (self & 0x1f))));

        public static MyStorageDataTypeFlags Without(this MyStorageDataTypeFlags self, MyStorageDataTypeEnum value) => 
            ((self & ~((byte) (1 << (value & 0x1f)))) & MyStorageDataTypeFlags.All);
    }
}

