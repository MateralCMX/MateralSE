namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPolyLineD
    {
        public Vector3 LineDirectionNormalized;
        public Vector3D Point0;
        public Vector3D Point1;
        public float Thickness;
    }
}

