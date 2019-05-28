namespace Sandbox.Graphics
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTextureAtlasItem
    {
        public string AtlasTexture;
        public Vector4 UVOffsets;
        public MyTextureAtlasItem(string atlasTex, Vector4 uvOffsets)
        {
            this.AtlasTexture = atlasTex;
            this.UVOffsets = uvOffsets;
        }
    }
}

