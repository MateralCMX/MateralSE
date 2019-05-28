namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPolyLine
    {
        public Vector3 LineDirectionNormalized;
        public Vector3 Point0;
        public Vector3 Point1;
        public float Thickness;
    }
}

