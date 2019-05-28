namespace Sandbox.Engine.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle
    {
        private Vector3 origin;
        private Vector3 edge0;
        private Vector3 edge1;
        public MyTriangle(Vector3 pt0, Vector3 pt1, Vector3 pt2)
        {
            this.origin = pt0;
            this.edge0 = pt1 - pt0;
            this.edge1 = pt2 - pt0;
        }

        public MyTriangle(ref Vector3 pt0, ref Vector3 pt1, ref Vector3 pt2)
        {
            this.origin = pt0;
            this.edge0 = pt1 - pt0;
            this.edge1 = pt2 - pt0;
        }

        public Vector3 GetPoint(int i) => 
            ((i != 1) ? ((i != 2) ? this.origin : (this.origin + this.edge1)) : (this.origin + this.edge0));

        public void GetPoint(int i, out Vector3 point)
        {
            if (i == 1)
            {
                point = this.origin + this.edge0;
            }
            else if (i == 2)
            {
                point = this.origin + this.edge1;
            }
            else
            {
                point = this.origin;
            }
        }

        public void GetPoint(ref Vector3 point, int i)
        {
            if (i == 1)
            {
                point.X = this.origin.X + this.edge0.X;
                point.Y = this.origin.Y + this.edge0.Y;
                point.Z = this.origin.Z + this.edge0.Z;
            }
            else if (i == 2)
            {
                point.X = this.origin.X + this.edge1.X;
                point.Y = this.origin.Y + this.edge1.Y;
                point.Z = this.origin.Z + this.edge1.Z;
            }
            else
            {
                point.X = this.origin.X;
                point.Y = this.origin.Y;
                point.Z = this.origin.Z;
            }
        }

        public Vector3 GetPoint(float t0, float t1) => 
            ((this.origin + (t0 * this.edge0)) + (t1 * this.edge1));

        public void GetSpan(out float min, out float max, Vector3 axis)
        {
            float a = Vector3.Dot(this.GetPoint(0), axis);
            float b = Vector3.Dot(this.GetPoint(1), axis);
            float c = Vector3.Dot(this.GetPoint(2), axis);
            min = MathHelper.Min(a, b, c);
            max = MathHelper.Max(a, b, c);
        }

        public void GetSpan(out float min, out float max, ref Vector3 axis)
        {
            Vector3 point = new Vector3();
            this.GetPoint(ref point, 0);
            float a = ((point.X * axis.X) + (point.Y * axis.Y)) + (point.Z * axis.Z);
            this.GetPoint(ref point, 1);
            float b = ((point.X * axis.X) + (point.Y * axis.Y)) + (point.Z * axis.Z);
            this.GetPoint(ref point, 2);
            float c = ((point.X * axis.X) + (point.Y * axis.Y)) + (point.Z * axis.Z);
            min = MathHelper.Min(a, b, c);
            max = MathHelper.Max(a, b, c);
        }

        public Vector3 Centre =>
            (this.origin + (0.3333333f * (this.edge0 + this.edge1)));
        public Vector3 Origin
        {
            get => 
                this.origin;
            set => 
                (this.origin = value);
        }
        public Vector3 Edge0
        {
            get => 
                this.edge0;
            set => 
                (this.edge0 = value);
        }
        public Vector3 Edge1
        {
            get => 
                this.edge1;
            set => 
                (this.edge1 = value);
        }
        public Vector3 Edge2 =>
            (this.edge1 - this.edge0);
        public VRageMath.Plane Plane =>
            new VRageMath.Plane(this.GetPoint(0), this.GetPoint(1), this.GetPoint(2));
        public Vector3 Normal =>
            Vector3.Normalize(Vector3.Cross(MyUtils.Normalize(this.edge0), MyUtils.Normalize(this.edge1)));
    }
}

