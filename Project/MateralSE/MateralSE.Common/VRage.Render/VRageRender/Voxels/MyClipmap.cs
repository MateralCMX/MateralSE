namespace VRageRender.Voxels
{
    using System;
    using VRageMath;

    public static class MyClipmap
    {
        public static bool EnableUpdate = true;
        public static bool EnableDebugDraw;
        public const int LodCount = 0x10;
        public static readonly Vector4[] LodColors;

        static MyClipmap()
        {
            Vector4[] vectorArray1 = new Vector4[0x10];
            vectorArray1[0] = new Vector4(1f, 0f, 0f, 1f);
            vectorArray1[1] = new Vector4(0f, 1f, 0f, 1f);
            vectorArray1[2] = new Vector4(0f, 0f, 1f, 1f);
            vectorArray1[3] = new Vector4(1f, 1f, 0f, 1f);
            vectorArray1[4] = new Vector4(0f, 1f, 1f, 1f);
            vectorArray1[5] = new Vector4(1f, 0f, 1f, 1f);
            vectorArray1[6] = new Vector4(0.5f, 0f, 1f, 1f);
            vectorArray1[7] = new Vector4(0.5f, 1f, 0f, 1f);
            vectorArray1[8] = new Vector4(1f, 0f, 0.5f, 1f);
            vectorArray1[9] = new Vector4(0f, 1f, 0.5f, 1f);
            vectorArray1[10] = new Vector4(1f, 0.5f, 0f, 1f);
            vectorArray1[11] = new Vector4(0f, 0.5f, 1f, 1f);
            vectorArray1[12] = new Vector4(0.5f, 1f, 1f, 1f);
            vectorArray1[13] = new Vector4(1f, 0.5f, 1f, 1f);
            vectorArray1[14] = new Vector4(1f, 1f, 0.5f, 1f);
            vectorArray1[15] = new Vector4(0.5f, 0.5f, 1f, 1f);
            LodColors = vectorArray1;
        }
    }
}

