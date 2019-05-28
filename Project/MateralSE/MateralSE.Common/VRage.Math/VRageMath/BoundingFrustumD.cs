namespace VRageMath
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Unsharper;

    [Serializable]
    public class BoundingFrustumD : IEquatable<BoundingFrustumD>
    {
        private readonly PlaneD[] m_planes;
        internal readonly Vector3D[] CornerArray;
        public const int CornerCount = 8;
        private const int NearPlaneIndex = 0;
        private const int FarPlaneIndex = 1;
        private const int LeftPlaneIndex = 2;
        private const int RightPlaneIndex = 3;
        private const int TopPlaneIndex = 4;
        private const int BottomPlaneIndex = 5;
        private const int NumPlanes = 6;
        private MatrixD matrix;
        private GjkD gjk;

        public BoundingFrustumD()
        {
            this.m_planes = new PlaneD[6];
            this.CornerArray = new Vector3D[8];
        }

        public BoundingFrustumD(MatrixD value)
        {
            this.m_planes = new PlaneD[6];
            this.CornerArray = new Vector3D[8];
            this.SetMatrix(ref value);
        }

        private static Vector3D ComputeIntersection(ref PlaneD plane, ref RayD ray)
        {
            double num = (-plane.D - Vector3D.Dot(plane.Normal, ray.Position)) / Vector3D.Dot(plane.Normal, ray.Direction);
            return (ray.Position + (ray.Direction * num));
        }

        private static unsafe RayD ComputeIntersectionLine(ref PlaneD p1, ref PlaneD p2)
        {
            RayD yd = new RayD {
                Direction = Vector3D.Cross(p1.Normal, p2.Normal)
            };
            double num = yd.Direction.LengthSquared();
            RayD* ydPtr1 = (RayD*) ref yd;
            ydPtr1->Position = Vector3D.Cross((Vector3D) ((-p1.D * p2.Normal) + (p2.D * p1.Normal)), yd.Direction) / num;
            return yd;
        }

        public ContainmentType Contains(BoundingBoxD box)
        {
            bool flag = false;
            foreach (PlaneD ed in this.m_planes)
            {
                PlaneIntersectionType type = box.Intersects(ed);
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

        public ContainmentType Contains(BoundingFrustumD frustum)
        {
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            ContainmentType disjoint = ContainmentType.Disjoint;
            if (this.Intersects(frustum))
            {
                disjoint = ContainmentType.Contains;
                for (int i = 0; i < this.CornerArray.Length; i++)
                {
                    if (this.Contains(frustum.CornerArray[i]) == ContainmentType.Disjoint)
                    {
                        disjoint = ContainmentType.Intersects;
                        break;
                    }
                }
            }
            return disjoint;
        }

        public ContainmentType Contains(BoundingSphereD sphere)
        {
            Vector3D center = sphere.Center;
            double radius = sphere.Radius;
            int num2 = 0;
            foreach (PlaneD ed in this.m_planes)
            {
                double num4 = (((ed.Normal.X * center.X) + (ed.Normal.Y * center.Y)) + (ed.Normal.Z * center.Z)) + ed.D;
                if (num4 > radius)
                {
                    return ContainmentType.Disjoint;
                }
                if (num4 < -radius)
                {
                    num2++;
                }
            }
            return ((num2 != 6) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        public ContainmentType Contains(Vector3D point)
        {
            foreach (PlaneD ed in this.m_planes)
            {
                if (((((ed.Normal.X * point.X) + (ed.Normal.Y * point.Y)) + (ed.Normal.Z * point.Z)) + ed.D) > 9.99999974737875E-06)
                {
                    return ContainmentType.Disjoint;
                }
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref BoundingBoxD box, out ContainmentType result)
        {
            bool flag = false;
            foreach (PlaneD ed in this.m_planes)
            {
                PlaneIntersectionType type = box.Intersects(ed);
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

        public void Contains(ref BoundingSphereD sphere, out ContainmentType result)
        {
            Vector3D center = sphere.Center;
            double radius = sphere.Radius;
            int num2 = 0;
            foreach (PlaneD ed in this.m_planes)
            {
                double num4 = (((ed.Normal.X * center.X) + (ed.Normal.Y * center.Y)) + (ed.Normal.Z * center.Z)) + ed.D;
                if (num4 > radius)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
                if (num4 < -radius)
                {
                    num2++;
                }
            }
            result = (num2 == 6) ? ContainmentType.Contains : ContainmentType.Intersects;
        }

        public void Contains(ref Vector3D point, out ContainmentType result)
        {
            foreach (PlaneD ed in this.m_planes)
            {
                if (((((ed.Normal.X * point.X) + (ed.Normal.Y * point.Y)) + (ed.Normal.Z * point.Z)) + ed.D) > 9.99999974737875E-06)
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
            BoundingFrustumD md = obj as BoundingFrustumD;
            if (md != null)
            {
                flag = this.matrix == md.matrix;
            }
            return flag;
        }

        public bool Equals(BoundingFrustumD other) => 
            ((other != null) ? (this.matrix == other.matrix) : false);

        public Vector3D[] GetCorners() => 
            ((Vector3D[]) this.CornerArray.Clone());

        public void GetCorners(Vector3D[] corners)
        {
            this.CornerArray.CopyTo(corners, 0);
        }

        [UnsharperDisableReflection]
        public unsafe void GetCornersUnsafe(Vector3D* corners)
        {
            corners[0] = this.CornerArray[0];
            corners[1] = this.CornerArray[1];
            corners[2] = this.CornerArray[2];
            corners[3] = this.CornerArray[3];
            corners[4] = this.CornerArray[4];
            corners[5] = this.CornerArray[5];
            corners[6] = this.CornerArray[6];
            corners[7] = this.CornerArray[7];
        }

        public override int GetHashCode() => 
            this.matrix.GetHashCode();

        public bool Intersects(BoundingBoxD box)
        {
            bool flag;
            this.Intersects(ref box, out flag);
            return flag;
        }

        public bool Intersects(BoundingFrustumD frustum)
        {
            Vector3D closestPoint;
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            if (this.gjk == null)
            {
                this.gjk = new GjkD();
            }
            this.gjk.Reset();
            Vector3D.Subtract(ref this.CornerArray[0], ref frustum.CornerArray[0], out closestPoint);
            if (closestPoint.LengthSquared() < 9.99999974737875E-06)
            {
                Vector3D.Subtract(ref this.CornerArray[0], ref frustum.CornerArray[1], out closestPoint);
            }
            double maxValue = double.MaxValue;
            while (true)
            {
                Vector3D vectord2;
                Vector3D vectord3;
                Vector3D vectord4;
                Vector3D vectord5;
                vectord2.X = -closestPoint.X;
                vectord2.Y = -closestPoint.Y;
                vectord2.Z = -closestPoint.Z;
                this.SupportMapping(ref vectord2, out vectord3);
                frustum.SupportMapping(ref closestPoint, out vectord4);
                Vector3D.Subtract(ref vectord3, ref vectord4, out vectord5);
                if ((((closestPoint.X * vectord5.X) + (closestPoint.Y * vectord5.Y)) + (closestPoint.Z * vectord5.Z)) > 0.0)
                {
                    return false;
                }
                this.gjk.AddSupportPoint(ref vectord5);
                closestPoint = this.gjk.ClosestPoint;
                double num3 = maxValue;
                maxValue = closestPoint.LengthSquared();
                double num2 = 3.9999998989515007E-05 * this.gjk.MaxLengthSquared;
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

        public bool Intersects(BoundingSphereD sphere)
        {
            bool flag;
            this.Intersects(ref sphere, out flag);
            return flag;
        }

        public PlaneIntersectionType Intersects(PlaneD plane)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                double num3;
                Vector3D.Dot(ref this.CornerArray[i], ref plane.Normal, out num3);
                num = ((num3 + plane.D) <= 0.0) ? (num | 2) : (num | 1);
                if (num == 3)
                {
                    return PlaneIntersectionType.Intersecting;
                }
            }
            return ((num != 1) ? PlaneIntersectionType.Back : PlaneIntersectionType.Front);
        }

        public double? Intersects(RayD ray)
        {
            double? nullable;
            this.Intersects(ref ray, out nullable);
            return nullable;
        }

        public void Intersects(ref BoundingBoxD box, out bool result)
        {
            Vector3D closestPoint;
            if (this.gjk == null)
            {
                this.gjk = new GjkD();
            }
            this.gjk.Reset();
            Vector3D.Subtract(ref this.CornerArray[0], ref box.Min, out closestPoint);
            if (closestPoint.LengthSquared() < 9.99999974737875E-06)
            {
                Vector3D.Subtract(ref this.CornerArray[0], ref box.Max, out closestPoint);
            }
            double maxValue = double.MaxValue;
            result = false;
            while (true)
            {
                Vector3D vectord2;
                Vector3D vectord3;
                Vector3D vectord4;
                Vector3D vectord5;
                vectord2.X = -closestPoint.X;
                vectord2.Y = -closestPoint.Y;
                vectord2.Z = -closestPoint.Z;
                this.SupportMapping(ref vectord2, out vectord3);
                box.SupportMapping(ref closestPoint, out vectord4);
                Vector3D.Subtract(ref vectord3, ref vectord4, out vectord5);
                if ((((closestPoint.X * vectord5.X) + (closestPoint.Y * vectord5.Y)) + (closestPoint.Z * vectord5.Z)) > 0.0)
                {
                    return;
                }
                this.gjk.AddSupportPoint(ref vectord5);
                closestPoint = this.gjk.ClosestPoint;
                double num3 = maxValue;
                maxValue = closestPoint.LengthSquared();
                if ((num3 - maxValue) <= (9.99999974737875E-06 * num3))
                {
                    return;
                }
                double num2 = 3.9999998989515007E-05 * this.gjk.MaxLengthSquared;
                if (this.gjk.FullSimplex || (maxValue < num2))
                {
                    result = true;
                    return;
                }
            }
        }

        public void Intersects(ref BoundingSphereD sphere, out bool result)
        {
            Vector3D unitX;
            if (this.gjk == null)
            {
                this.gjk = new GjkD();
            }
            this.gjk.Reset();
            Vector3D.Subtract(ref this.CornerArray[0], ref sphere.Center, out unitX);
            if (unitX.LengthSquared() < 9.99999974737875E-06)
            {
                unitX = Vector3D.UnitX;
            }
            double maxValue = double.MaxValue;
            result = false;
            while (true)
            {
                Vector3D vectord2;
                Vector3D vectord3;
                Vector3D vectord4;
                Vector3D vectord5;
                vectord2.X = -unitX.X;
                vectord2.Y = -unitX.Y;
                vectord2.Z = -unitX.Z;
                this.SupportMapping(ref vectord2, out vectord3);
                sphere.SupportMapping(ref unitX, out vectord4);
                Vector3D.Subtract(ref vectord3, ref vectord4, out vectord5);
                if ((((unitX.X * vectord5.X) + (unitX.Y * vectord5.Y)) + (unitX.Z * vectord5.Z)) > 0.0)
                {
                    return;
                }
                this.gjk.AddSupportPoint(ref vectord5);
                unitX = this.gjk.ClosestPoint;
                double num3 = maxValue;
                maxValue = unitX.LengthSquared();
                if ((num3 - maxValue) <= (9.99999974737875E-06 * num3))
                {
                    return;
                }
                double num2 = 3.9999998989515007E-05 * this.gjk.MaxLengthSquared;
                if (this.gjk.FullSimplex || (maxValue < num2))
                {
                    result = true;
                    return;
                }
            }
        }

        public void Intersects(ref PlaneD plane, out PlaneIntersectionType result)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                double num3;
                Vector3D.Dot(ref this.CornerArray[i], ref plane.Normal, out num3);
                num = ((num3 + plane.D) <= 0.0) ? (num | 2) : (num | 1);
                if (num == 3)
                {
                    result = PlaneIntersectionType.Intersecting;
                    return;
                }
            }
            result = (num == 1) ? PlaneIntersectionType.Front : PlaneIntersectionType.Back;
        }

        public void Intersects(ref RayD ray, out double? result)
        {
            ContainmentType type;
            this.Contains(ref ray.Position, out type);
            if (type == ContainmentType.Contains)
            {
                result = 0.0;
            }
            else
            {
                double minValue = double.MinValue;
                double maxValue = double.MaxValue;
                result = 0;
                foreach (PlaneD ed in this.m_planes)
                {
                    double num5;
                    double num6;
                    Vector3D normal = ed.Normal;
                    Vector3D.Dot(ref ray.Direction, ref normal, out num5);
                    Vector3D.Dot(ref ray.Position, ref normal, out num6);
                    num6 += ed.D;
                    if (Math.Abs(num5) < 9.99999974737875E-06)
                    {
                        if (num6 > 0.0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        double num7 = -num6 / num5;
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
                double num3 = (minValue >= 0.0) ? minValue : maxValue;
                if (num3 >= 0.0)
                {
                    result = new double?(num3);
                }
            }
        }

        public static bool operator ==(BoundingFrustumD a, BoundingFrustumD b) => 
            Equals(a, b);

        public static bool operator !=(BoundingFrustumD a, BoundingFrustumD b) => 
            !Equals(a, b);

        private void SetMatrix(ref MatrixD value)
        {
            this.matrix = value;
            this.m_planes[2].Normal.X = -value.M14 - value.M11;
            this.m_planes[2].Normal.Y = -value.M24 - value.M21;
            this.m_planes[2].Normal.Z = -value.M34 - value.M31;
            this.m_planes[2].D = -value.M44 - value.M41;
            this.m_planes[3].Normal.X = -value.M14 + value.M11;
            this.m_planes[3].Normal.Y = -value.M24 + value.M21;
            this.m_planes[3].Normal.Z = -value.M34 + value.M31;
            this.m_planes[3].D = -value.M44 + value.M41;
            this.m_planes[4].Normal.X = -value.M14 + value.M12;
            this.m_planes[4].Normal.Y = -value.M24 + value.M22;
            this.m_planes[4].Normal.Z = -value.M34 + value.M32;
            this.m_planes[4].D = -value.M44 + value.M42;
            this.m_planes[5].Normal.X = -value.M14 - value.M12;
            this.m_planes[5].Normal.Y = -value.M24 - value.M22;
            this.m_planes[5].Normal.Z = -value.M34 - value.M32;
            this.m_planes[5].D = -value.M44 - value.M42;
            this.m_planes[0].Normal.X = -value.M13;
            this.m_planes[0].Normal.Y = -value.M23;
            this.m_planes[0].Normal.Z = -value.M33;
            this.m_planes[0].D = -value.M43;
            this.m_planes[1].Normal.X = -value.M14 + value.M13;
            this.m_planes[1].Normal.Y = -value.M24 + value.M23;
            this.m_planes[1].Normal.Z = -value.M34 + value.M33;
            this.m_planes[1].D = -value.M44 + value.M43;
            for (int i = 0; i < 6; i++)
            {
                double num2 = this.m_planes[i].Normal.Length();
                this.m_planes[i].Normal /= num2;
                this.m_planes[i].D /= num2;
            }
            RayD ray = ComputeIntersectionLine(ref this.m_planes[0], ref this.m_planes[2]);
            this.CornerArray[0] = ComputeIntersection(ref this.m_planes[4], ref ray);
            this.CornerArray[3] = ComputeIntersection(ref this.m_planes[5], ref ray);
            RayD yd2 = ComputeIntersectionLine(ref this.m_planes[3], ref this.m_planes[0]);
            this.CornerArray[1] = ComputeIntersection(ref this.m_planes[4], ref yd2);
            this.CornerArray[2] = ComputeIntersection(ref this.m_planes[5], ref yd2);
            yd2 = ComputeIntersectionLine(ref this.m_planes[2], ref this.m_planes[1]);
            this.CornerArray[4] = ComputeIntersection(ref this.m_planes[4], ref yd2);
            this.CornerArray[7] = ComputeIntersection(ref this.m_planes[5], ref yd2);
            yd2 = ComputeIntersectionLine(ref this.m_planes[1], ref this.m_planes[3]);
            this.CornerArray[5] = ComputeIntersection(ref this.m_planes[4], ref yd2);
            this.CornerArray[6] = ComputeIntersection(ref this.m_planes[5], ref yd2);
        }

        internal void SupportMapping(ref Vector3D v, out Vector3D result)
        {
            double num2;
            int index = 0;
            Vector3D.Dot(ref this.CornerArray[0], ref v, out num2);
            for (int i = 1; i < this.CornerArray.Length; i++)
            {
                double num4;
                Vector3D.Dot(ref this.CornerArray[i], ref v, out num4);
                if (num4 > num2)
                {
                    index = i;
                    num2 = num4;
                }
            }
            result = this.CornerArray[index];
        }

        public override string ToString()
        {
            object[] args = new object[] { this.Near.ToString(), this.Far.ToString(), this.Left.ToString(), this.Right.ToString(), this.Top.ToString(), this.Bottom.ToString() };
            return string.Format(CultureInfo.CurrentCulture, "{{Near:{0} Far:{1} Left:{2} Right:{3} Top:{4} Bottom:{5}}}", args);
        }

        public PlaneD this[int index] =>
            this.m_planes[index];

        public PlaneD Near =>
            this.m_planes[0];

        public PlaneD Far =>
            this.m_planes[1];

        public PlaneD Left =>
            this.m_planes[2];

        public PlaneD Right =>
            this.m_planes[3];

        public PlaneD Top =>
            this.m_planes[4];

        public PlaneD Bottom =>
            this.m_planes[5];

        public MatrixD Matrix
        {
            get => 
                this.matrix;
            set => 
                this.SetMatrix(ref value);
        }
    }
}

