namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRectangle2D
    {
        public Vector2 LeftTop;
        public Vector2 Size;
        public MyRectangle2D(Vector2 leftTop, Vector2 size)
        {
            this.LeftTop = leftTop;
            this.Size = size;
        }
    }
}

