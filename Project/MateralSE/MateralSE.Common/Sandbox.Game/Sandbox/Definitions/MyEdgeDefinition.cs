namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEdgeDefinition
    {
        public Vector3 Point0;
        public Vector3 Point1;
        public int Side0;
        public int Side1;
    }
}

