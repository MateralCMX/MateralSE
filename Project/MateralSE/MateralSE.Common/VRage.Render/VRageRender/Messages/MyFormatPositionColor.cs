namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyFormatPositionColor
    {
        public Vector3 Position;
        public VRageMath.Color Color;
        public MyFormatPositionColor(Vector3 position, VRageMath.Color color)
        {
            this.Position = position;
            this.Color = color;
        }
    }
}

