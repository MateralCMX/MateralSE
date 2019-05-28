namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyViewport
    {
        public float OffsetX;
        public float OffsetY;
        public float Width;
        public float Height;
        public MyViewport(float width, float height)
        {
            this.OffsetX = 0f;
            this.OffsetY = 0f;
            this.Width = width;
            this.Height = height;
        }

        public MyViewport(Vector2I resolution)
        {
            this.OffsetX = 0f;
            this.OffsetY = 0f;
            this.Width = resolution.X;
            this.Height = resolution.Y;
        }

        public MyViewport(float x, float y, float width, float height)
        {
            this.OffsetX = x;
            this.OffsetY = y;
            this.Width = width;
            this.Height = height;
        }
    }
}

