namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPlane
    {
        public Vector3 Point;
        public Vector3 Normal;
        public MyPlane(Vector3 point, Vector3 normal)
        {
            this.Point = point;
            this.Normal = normal;
        }

        public MyPlane(ref Vector3 point, ref Vector3 normal)
        {
            this.Point = point;
            this.Normal = normal;
        }

        public MyPlane(ref MyTriangle_Vertices triangle)
        {
            this.Point = triangle.Vertex0;
            this.Normal = MyUtils.Normalize(Vector3.Cross(triangle.Vertex1 - triangle.Vertex0, triangle.Vertex2 - triangle.Vertex0));
        }

        public float GetPlaneDistance() => 
            -(((this.Normal.X * this.Point.X) + (this.Normal.Y * this.Point.Y)) + (this.Normal.Z * this.Point.Z));
    }
}

