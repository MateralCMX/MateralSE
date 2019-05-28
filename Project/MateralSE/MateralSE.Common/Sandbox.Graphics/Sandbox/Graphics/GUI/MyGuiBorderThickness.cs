namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGuiBorderThickness
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public MyGuiBorderThickness(float val = 0f)
        {
            float num;
            this.Bottom = num = val;
            this.Top = num = num;
            this.Left = this.Right = num;
        }

        public MyGuiBorderThickness(float horizontal, float vertical)
        {
            this.Left = this.Right = horizontal;
            this.Top = this.Bottom = vertical;
        }

        public MyGuiBorderThickness(float left, float right, float top, float bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public float HorizontalSum =>
            (this.Left + this.Right);
        public float VerticalSum =>
            (this.Top + this.Bottom);
        public Vector2 TopLeftOffset =>
            new Vector2(this.Left, this.Top);
        public Vector2 TopRightOffset =>
            new Vector2(-this.Right, this.Top);
        public Vector2 BottomLeftOffset =>
            new Vector2(this.Left, -this.Bottom);
        public Vector2 BottomRightOffset =>
            new Vector2(-this.Right, -this.Bottom);
        public Vector2 SizeChange =>
            new Vector2(this.HorizontalSum, this.VerticalSum);
        public Vector2 MarginStep =>
            new Vector2(Math.Max(this.Left, this.Right), Math.Max(this.Top, this.Bottom));
    }
}

