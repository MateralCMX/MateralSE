namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Normals
    {
        public Vector3 Normal0;
        public Vector3 Normal1;
        public Vector3 Normal2;
    }
}

