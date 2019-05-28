namespace VRageMath
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Unsharper;

    [Serializable]
    public class BoundingFrustum : IEquatable<BoundingFrustum>
    {
        private Plane[] planes;
        internal Vector3[] cornerArray;
        public const int CornerCount = 8;
        private const int NearPlaneIndex = 0;
        private const int FarPlaneIndex = 1;
        private const int LeftPlaneIndex = 2;
        private const int RightPlaneIndex = 3;
        private const int TopPlaneIndex = 4;
        private const int BottomPlaneIndex = 5;
        private const int NumPlanes = 6;
        private VRageMath.Matrix matrix;
        private Gjk gjk;

        public BoundingFrustum()
        {
            this.planes = new Plane[6];
            this.cornerArray = new Vector3[8];
        }

        public BoundingFrustum(VRageMath.Matrix value)
        {
            this.planes = new Plane[6];
            this.cornerArray = new Vector3[8];
            this.SetMatrix(ref value);
        }

        private static Vector3 ComputeIntersection(ref Plane plane, ref Ray ray)
        {
            float num = (-plane.D - Vector3.Dot(plane.Normal, ray.Position)) / Vector3.Dot(plane.Normal, ray.Direction);
            return (ray.Position + (ray.Direction * num));
        }

        private static unsafe Ray ComputeIntersectionLine(ref Plane p1, ref Plane p2)
        {
            Ray ray = new Ray {
                Direction = Vector3.Cross(p1.Normal, p2.Normal)
            };
            float num = ray.Direction.LengthSquared();
            Ray* rayPtr1 = (Ray*) ref ray;
            rayPtr1->Position = Vector3.Cross((Vector3) ((-p1.D * p2.Normal) + (p2.D * p1.Normal)), ray.Direction) / num;
            return ray;
        }

        public ContainmentType Contains(ref BoundingBox box)
        {
            bool flag = false;
            foreach (Plane plane in this.planes)
            {
                PlaneIntersectionType type = box.Intersects(plane);
                if (type == PlaneIntersectionType.Front)
                {
                    return ContainmentType.Disjoint;
                }
                if (type == PlaneIntersectionType.Intersecting)
                {
                    flag = true;
                }
            }
            return (!flag ? ContainmentType.Contains : ContainmentType.Intersects);
        }

        public ContainmentType Contains(BoundingFrustum frustum)
        {
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            ContainmentType disjoint = ContainmentType.Disjoint;
            if (this.Intersects(frustum))
            {
                disjoint = ContainmentType.Contains;
                for (int i = 0; i < this.cornerArray.Length; i++)
                {
                    if (this.Contains(frustum.cornerArray[i]) == ContainmentType.Disjoint)
                    {
                        disjoint = ContainmentType.Intersects;
                        break;
                    }
                }
            }
            return disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            Vector3 center = sphere.Center;
            float radius = sphere.Radius;
            int num2 = 0;
            foreach (Plane plane in this.planes)
            {
                float num4 = (((plane.Normal.X * center.X) + (plane.Normal.Y * center.Y)) + (plane.Normal.Z * center.Z)) + plane.D;
                if (num4 > radius)
                {
                    return ContainmentType.Disjoint;
                }
                if (num4 < -((double) radius))
                {
                    num2++;
                }
            }
            return ((num2 != 6) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        public ContainmentType Contains(Vector3 point)
        {
            foreach (Plane plane in this.planes)
            {
                if (((((plane.Normal.X * point.X) + (plane.Normal.Y * point.Y)) + (plane.Normal.Z * point.Z)) + plane.D) > 9.99999974737875E-06)
                {
                    return ContainmentType.Disjoint;
                }
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            bool flag = false;
            foreach (Plane plane in this.planes)
            {
                PlaneIntersectionType type = box.Intersects(plane);
                if (type == PlaneIntersectionType.Front)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
                if (type == PlaneIntersectionType.Intersecting)
                {
                    flag = true;
                }
            }
            result = flag ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            Vector3 center = sphere.Center;
            float radius = sphere.Radius;
            int num2 = 0;
            foreach (Plane plane in this.planes)
            {
                float num4 = (((plane.Normal.X * center.X) + (plane.Normal.Y * center.Y)) + (plane.Normal.Z * center.Z)) + plane.D;
                if (num4 > radius)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
                if (num4 < -((double) radius))
                {
                    num2++;
                }
            }
            result = (num2 == 6) ? ContainmentType.Contains : ContainmentType.Intersects;
        }

        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            foreach (Plane plane in this.planes)
            {
                if (((((plane.Normal.X * point.X) + (plane.Normal.Y * point.Y)) + (plane.Normal.Z * point.Z)) + plane.D) > 9.99999974737875E-06)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
            }
            result = ContainmentType.Contains;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            BoundingFrustum frustum = obj as BoundingFrustum;
            if (frustum != null)
            {
                flag = this.matrix == frustum.matrix;
            }
            return flag;
        }

        public bool Equals(BoundingFrustum other) => 
            ((other != null) ? (this.matrix == other.matrix) : false);

        public Vector3[] GetCorners() => 
            ((Vector3[]) this.cornerArray.Clone());

        public void GetCorners(Vector3[] corners)
        {
            this.cornerArray.CopyTo(corners, 0);
        }

        [UnsharperDisableReflection]
        public unsafe void GetCornersUnsafe(Vector3* corners)
        {
            corners[0] = this.cornerArray[0];
            corners[1] = this.cornerArray[1];
            corners[2] = this.cornerArray[2];
            corners[3] = this.cornerArray[3];
            corners[4] = this.cornerArray[4];
            corners[5] = this.cornerArray[5];
            corners[6] = this.cornerArray[6];
            corners[7] = this.cornerArray[7];
        }

        public override int GetHashCode() => 
            this.matrix.GetHashCode();

        public bool Intersects(BoundingBox box)
        {
            bool flag;
            this.Intersects(ref box, out flag);
            return flag;
        }

        public bool Intersects(BoundingFrustum frustum)
        {
            Vector3 closestPoint;
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref frustum.cornerArray[0], out closestPoint);
            if (closestPoint.LengthSquared() < 9.99999974737875E-06)
            {
                Vector3.Subtract(ref this.cornerArray[0], ref frustum.cornerArray[1], out closestPoint);
            }
            float maxValue = float.MaxValue;
            while (true)
            {
                Vector3 vector2;
                Vector3 vector3;
                Vector3 vector4;
                Vector3 vector5;
                vector2.X = -closestPoint.X;
                vector2.Y = -closestPoint.Y;
                vector2.Z = -closestPoint.Z;
                this.SupportMapping(ref vector2, out vector3);
                frustum.SupportMapping(ref closestPoint, out vector4);
                Vector3.Subtract(ref vector3, ref vector4, out vector5);
                if ((((closestPoint.X * vector5.X) + (closestPoint.Y * vector5.Y)) + (closestPoint.Z * vector5.Z)) > 0.0)
                {
                    return false;
                }
                this.gjk.AddSupportPoint(ref vector5);
                closestPoint = this.gjk.ClosestPoint;
                float num3 = maxValue;
                maxValue = closestPoint.LengthSquared();
                float num2 = 4E-05f * this.gjk.MaxLengthSquared;
                if ((num3 - maxValue) <= (9.99999974737875E-06 * num3))
                {
                    return false;
                }
                if (this.gjk.FullSimplex || (maxValue < num2))
                {
                    return true;
                }
            }
        }

        public bool Intersects(BoundingSphere sphere)
        {
            bool flag;
            this.Intersects(ref sphere, out flag);
            return flag;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                float num3;
                Vector3.Dot(ref this.cornerArray[i], ref plane.Normal, out num3);
                num = ((num3 + plane.D) <= 0.0) ? (num | 2) : (num | 1);
                if (num == 3)
                {
                    return PlaneIntersectionType.Intersecting;
                }
            }
            return ((num != 1) ? PlaneIntersectionType.Back : PlaneIntersectionType.Front);
        }

        public float? Intersects(Ray ray)
        {
            float? nullable;
            this.Intersects(ref ray, out nullable);
            return nullable;
        }

        public void Intersects(ref BoundingBox box, out bool result)
        {
            Vector3 closestPoint;
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref box.Min, out closestPoint);
            if (closestPoint.LengthSquared() < 9.99999974737875E-06)
            {
                Vector3.Subtract(ref this.cornerArray[0], ref box.Max, out closestPoint);
            }
            float maxValue = float.MaxValue;
            result = false;
            while (true)
            {
                Vector3 vector2;
                Vector3 vector3;
                Vector3 vector4;
                Vector3 vector5;
                vector2.X = -closestPoint.X;
                vector2.Y = -closestPoint.Y;
                vector2.Z = -closestPoint.Z;
                this.SupportMapping(ref vector2, out vector3);
                box.SupportMapping(ref closestPoint, out vector4);
                Vector3.Subtract(ref vector3, ref vector4, out vector5);
                if ((((closestPoint.X * vector5.X) + (closestPoint.Y * vector5.Y)) + (closestPoint.Z * vector5.Z)) > 0.0)
                {
                    return;
                }
                this.gjk.AddSupportPoint(ref vector5);
                closestPoint = this.gjk.ClosestPoint;
                float num3 = maxValue;
                maxValue = closestPoint.LengthSquared();
                if ((num3 - maxValue) <= (9.99999974737875E-06 * num3))
                {
                    return;
                }
                float num2 = 4E-05f * this.gjk.MaxLengthSquared;
                if (this.gjk.FullSimplex || (maxValue < num2))
                {
                    result = true;
                    return;
                }
            }
        }

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            Vector3 unitX;
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref sphere.Center, out unitX);
            if (unitX.LengthSquared() < 9.99999974737875E-06)
            {
                unitX = Vector3.UnitX;
            }
            float maxValue = float.MaxValue;
            result = false;
            while (true)
            {
                Vector3 vector2;
                Vector3 vector3;
                Vector3 vector4;
                Vector3 vector5;
                vector2.X = -unitX.X;
                vector2.Y = -unitX.Y;
                vector2.Z = -unitX.Z;
                this.SupportMapping(ref vector2, out vector3);
                sphere.SupportMapping(ref unitX, out vector4);
                Vector3.Subtract(ref vector3, ref vector4, out vector5);
                if ((((unitX.X * vector5.X) + (unitX.Y * vector5.Y)) + (unitX.Z * vector5.Z)) > 0.0)
                {
                    return;
                }
                this.gjk.AddSupportPoint(ref vector5);
                unitX = this.gjk.ClosestPoint;
                float num3 = maxValue;
                maxValue = unitX.LengthSquared();
                if ((num3 - maxValue) <= (9.99999974737875E-06 * num3))
                {
                    return;
                }
                float num2 = 4E-05f * this.gjk.MaxLengthSquared;
                if (this.gjk.FullSimplex || (maxValue < num2))
                {
                    result = true;
                    return;
                }
            }
        }

        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                float num3;
                Vector3.Dot(ref this.cornerArray[i], ref plane.Normal, out num3);
                num = ((num3 + plane.D) <= 0.0) ? (num | 2) : (num | 1);
                if (num == 3)
                {
                    result = PlaneIntersectionType.Intersecting;
                    return;
                }
            }
            result = (num == 1) ? PlaneIntersectionType.Front : PlaneIntersectionType.Back;
        }

        public void Intersects(ref Ray ray, out float? result)
        {
            ContainmentType type;
            this.Contains(ref ray.Position, out type);
            if (type == ContainmentType.Contains)
            {
                result = 0f;
            }
            else
            {
                float minValue = float.MinValue;
                float maxValue = float.MaxValue;
                result = 0;
                foreach (Plane plane in this.planes)
                {
                    float num5;
                    float num6;
                    Vector3 normal = plane.Normal;
                    Vector3.Dot(ref ray.Direction, ref normal, out num5);
                    Vector3.Dot(ref ray.Position, ref normal, out num6);
                    num6 += plane.D;
                    if (Math.Abs(num5) < 9.99999974737875E-06)
                    {
                        if (num6 > 0.0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        float num7 = -num6 / num5;
                        if (num5 < 0.0)
                        {
                            if (num7 > maxValue)
                            {
                                return;
                            }
                            if (num7 > minValue)
                            {
                                minValue = num7;
                            }
                        }
                        else
                        {
                            if (num7 < minValue)
                            {
                                return;
                            }
                            if (num7 < maxValue)
                            {
                                maxValue = num7;
                            }
                        }
                    }
                }
                float num3 = (minValue >= 0.0) ? minValue : maxValue;
                if (num3 >= 0.0)
                {
                    result = new float?(num3);
                }
            }
        }

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b) => 
            Equals(a, b);

        public static bool operator !=(BoundingFrustum a, BoundingFrustum b) => 
            !Equals(a, b);

        private unsafe void SetMatrix(ref VRageMath.Matrix value)
        {
            this.matrix = value;
            this.planes[2].Normal.X = -value.M14 - value.M11;
            this.planes[2].Normal.Y = -value.M24 - value.M21;
            this.planes[2].Normal.Z = -value.M34 - value.M31;
            this.planes[2].D = -value.M44 - value.M41;
            this.planes[3].Normal.X = -value.M14 + value.M11;
            this.planes[3].Normal.Y = -value.M24 + value.M21;
            this.planes[3].Normal.Z = -value.M34 + value.M31;
            this.planes[3].D = -value.M44 + value.M41;
            this.planes[4].Normal.X = -value.M14 + value.M12;
            this.planes[4].Normal.Y = -value.M24 + value.M22;
            this.planes[4].Normal.Z = -value.M34 + value.M32;
            this.planes[4].D = -value.M44 + value.M42;
            this.planes[5].Normal.X = -value.M14 - value.M12;
            this.planes[5].Normal.Y = -value.M24 - value.M22;
            this.planes[5].Normal.Z = -value.M34 - value.M32;
            this.planes[5].D = -value.M44 - value.M42;
            this.planes[0].Normal.X = -value.M13;
            this.planes[0].Normal.Y = -value.M23;
            this.planes[0].Normal.Z = -value.M33;
            this.planes[0].D = -value.M43;
            this.planes[1].Normal.X = -value.M14 + value.M13;
            this.planes[1].Normal.Y = -value.M24 + value.M23;
            this.planes[1].Normal.Z = -value.M34 + value.M33;
            this.planes[1].D = -value.M44 + value.M43;
            for (int i = 0; i < 6; i++)
            {
                float num2 = this.planes[i].Normal.Length();
                Vector3* vectorPtr1 = (Vector3*) ref this.planes[i].Normal;
                vectorPtr1[0] /= num2;
                float* singlePtr1 = (float*) ref this.planes[i].D;
                singlePtr1[0] /= num2;
            }
            Ray ray = ComputeIntersectionLine(ref this.planes[0], ref this.planes[2]);
            this.cornerArray[0] = ComputeIntersection(ref this.planes[4], ref ray);
            this.cornerArray[3] = ComputeIntersection(ref this.planes[5], ref ray);
            Ray ray2 = ComputeIntersectionLine(ref this.planes[3], ref this.planes[0]);
            this.cornerArray[1] = ComputeIntersection(ref this.planes[4], ref ray2);
            this.cornerArray[2] = ComputeIntersection(ref this.planes[5], ref ray2);
            ray2 = ComputeIntersectionLine(ref this.planes[2], ref this.planes[1]);
            this.cornerArray[4] = ComputeIntersection(ref this.planes[4], ref ray2);
            this.cornerArray[7] = ComputeIntersection(ref this.planes[5], ref ray2);
            ray2 = ComputeIntersectionLine(ref this.planes[1], ref this.planes[3]);
            this.cornerArray[5] = ComputeIntersection(ref this.planes[4], ref ray2);
            this.cornerArray[6] = ComputeIntersection(ref this.planes[5], ref ray2);
        }

        internal void SupportMapping(ref Vector3 v, out Vector3 result)
        {
            float num2;
            int index = 0;
            Vector3.Dot(ref this.cornerArray[0], ref v, out num2);
            for (int i = 1; i < this.cornerArray.Length; i++)
            {
                float num4;
                Vector3.Dot(ref this.cornerArray[i], ref v, out num4);
                if (num4 > num2)
                {
                    index = i;
                    num2 = num4;
                }
            }
            result = this.cornerArray[index];
        }

        public override string ToString()
        {
            object[] args = new object[] { this.Near.ToString(), this.Far.ToString(), this.Left.ToString(), this.Right.ToString(), this.Top.ToString(), this.Bottom.ToString() };
            return string.Format(CultureInfo.CurrentCulture, "{{Near:{0} Far:{1} Left:{2} Right:{3} Top:{4} Bottom:{5}}}", args);
        }

        public Plane[] Planes =>
            this.planes;

        public Plane this[int index] =>
            this.planes[index];

        public Plane Near =>
            this.planes[0];

        public Plane Far =>
            this.planes[1];

        public Plane Left =>
            this.planes[2];

        public Plane Right =>
            this.planes[3];

        public Plane Top =>
            this.planes[4];

        public Plane Bottom =>
            this.planes[5];

        public VRageMath.Matrix Matrix
        {
            get => 
                this.matrix;
            set => 
                this.SetMatrix(ref value);
        }
    }
}

