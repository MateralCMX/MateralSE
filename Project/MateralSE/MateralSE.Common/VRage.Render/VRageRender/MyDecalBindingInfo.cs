namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDecalBindingInfo
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Matrix Transformation;
    }
}

