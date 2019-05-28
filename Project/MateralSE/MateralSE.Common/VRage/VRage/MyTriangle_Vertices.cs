namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Vertices
    {
        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;
        public void Transform(ref Matrix transform)
        {
            this.Vertex0 = Vector3.Transform(this.Vertex0, ref transform);
            this.Vertex1 = Vector3.Transform(this.Vertex1, ref transform);
            this.Vertex2 = Vector3.Transform(this.Vertex2, ref transform);
        }
    }
}

