namespace VRage.Utils
{
    using System;
    using VRageMath;

    public class MyAtlasTextureCoordinate
    {
        public Vector2 Offset;
        public Vector2 Size;

        public MyAtlasTextureCoordinate(Vector2 offset, Vector2 size)
        {
            this.Offset = offset;
            this.Size = size;
        }
    }
}

