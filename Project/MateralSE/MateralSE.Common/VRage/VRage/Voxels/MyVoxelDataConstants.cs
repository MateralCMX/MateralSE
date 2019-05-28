namespace VRage.Voxels
{
    using System;

    public static class MyVoxelDataConstants
    {
        public const string StorageV2Extension = ".vx2";
        public const byte IsoLevel = 0x7f;
        public const byte ContentEmpty = 0;
        public const byte ContentFull = 0xff;
        public const float HalfContent = 127.5f;
        public const float HalfContentReciprocal = 0.007843138f;
        public const float ContentReciprocal = 0.003921569f;
        public const byte NullMaterial = 0xff;
        private static readonly byte[] Defaults;
        public const int LodCount = 0x10;

        static MyVoxelDataConstants()
        {
            byte[] buffer1 = new byte[2];
            buffer1[1] = 0xff;
            Defaults = buffer1;
        }

        public static byte DefaultValue(MyStorageDataTypeEnum type) => 
            Defaults[(int) type];
    }
}

