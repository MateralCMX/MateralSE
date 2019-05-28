namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyBox
    {
        public Vector3 Center;
        public Vector3 Size;
        public MyBox(Vector3 center, Vector3 size)
        {
            this.Center = center;
            this.Size = size;
        }
    }
}

