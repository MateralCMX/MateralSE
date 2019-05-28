namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBillboardViewProjection
    {
        public Matrix ViewAtZero;
        public Matrix Projection;
        public MyViewport Viewport;
        public Vector3D CameraPosition;
    }
}

