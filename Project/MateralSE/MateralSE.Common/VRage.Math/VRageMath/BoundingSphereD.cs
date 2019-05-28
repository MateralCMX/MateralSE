namespace VRageMath
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct BoundingSphereD : IEquatable<BoundingSphereD>
    {
        public Vector3D Center;
        public double Radius;
        public BoundingSphereD(Vector3D center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public static bool operator ==(BoundingSphereD a, BoundingSphereD b) => 
            a.Equals(b);

        public static bool operator !=(BoundingSphereD a, BoundingSphereD b) => 
            ((a.Center != b.Center) || !(a.Radius == b.Radius));

        public bool Equals(BoundingSphereD other) => 
            ((this.Center == other.Center) && (this.Radius == other.Radius));

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is BoundingSphereD)
            {
                flag = this.Equals((BoundingSphereD) obj);
            }
            return flag;
        }

        public override int GetHashCode() => 
            (this.Center.GetHashCode() + this.Radius.GetHashCode());

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            object[] args = new object[] { this.Center.ToString(), this.Radius.ToString(currentCulture) };
            return string.Format(currentCulture, "{{Center:{0} Radius:{1}}}", args);
        }

        public static BoundingSphereD CreateMerged(BoundingSphereD original, BoundingSphereD additional)
        {
            Vector3D vectord;
            BoundingSphereD ed;
            Vector3D.Subtract(ref additional.Center, ref original.Center, out vectord);
            double num = vectord.Length();
            double radius = original.Radius;
            double num3 = additional.Radius;
            if ((radius + num3) >= num)
            {
                if ((radius - num3) >= num)
                {
                    return original;
                }
                if ((num3 - radius) >= num)
                {
                    return additional;
                }
            }
            Vector3D vectord2 = vectord * (1.0 / num);
            double num4 = MathHelper.Min(-radius, num - num3);
            double num5 = (MathHelper.Max(radius, num + num3) - num4) * 0.5;
            ed.Center = original.Center + (vectord2 * (num5 + num4));
            ed.Radius = num5;
            return ed;
        }

        public static void CreateMerged(ref BoundingSphereD original, ref BoundingSphereD additional, out BoundingSphereD result)
        {
            Vector3D vectord;
            Vector3D.Subtract(ref additional.Center, ref original.Center, out vectord);
            double num = vectord.Length();
            double radius = original.Radius;
            double num3 = additional.Radius;
            if ((radius + num3) >= num)
            {
                if ((radius - num3) >= num)
                {
                    result = original;
                    return;
                }
                if ((num3 - radius) >= num)
                {
                    result = additional;
                    return;
                }
            }
            Vector3D vectord2 = vectord * (1.0 / num);
            double num4 = MathHelper.Min(-radius, num - num3);
            double num5 = (MathHelper.Max(radius, num + num3) - num4) * 0.5;
            result.Center = original.Center + (vectord2 * (num5 + num4));
            result.Radius = num5;
        }

        public static BoundingSphereD CreateFromBoundingBox(BoundingBoxD box)
        {
            BoundingSphereD ed;
            double num;
            Vector3D.Lerp(ref box.Min, ref box.Max, 0.5, out ed.Center);
            Vector3D.Distance(ref box.Min, ref box.Max, out num);
            ed.Radius = num * 0.5;
            return ed;
        }

        public static void CreateFromBoundingBox(ref BoundingBoxD box, out BoundingSphereD result)
        {
            double num;
            Vector3D.Lerp(ref box.Min, ref box.Max, 0.5, out result.Center);
            Vector3D.Distance(ref box.Min, ref box.Max, out num);
            result.Radius = num * 0.5;
        }

        public static BoundingSphereD CreateFromPoints(Vector3D[] points)
        {
            Vector3D vectord;
            Vector3D vectord2;
            Vector3D vectord3;
            Vector3D vectord4;
            Vector3D vectord5;
            double num;
            double num2;
            double num3;
            Vector3D vectord7;
            double num4;
            BoundingSphereD ed;
            Vector3D vectord6 = vectord5 = vectord4 = vectord3 = vectord2 = vectord = points[0];
            foreach (Vector3D vectord8 in points)
            {
                if (vectord8.X < vectord6.X)
                {
                    vectord6 = vectord8;
                }
                if (vectord8.X > vectord5.X)
                {
                    vectord5 = vectord8;
                }
                if (vectord8.Y < vectord4.Y)
                {
                    vectord4 = vectord8;
                }
                if (vectord8.Y > vectord3.Y)
                {
                    vectord3 = vectord8;
                }
                if (vectord8.Z < vectord2.Z)
                {
                    vectord2 = vectord8;
                }
                if (vectord8.Z > vectord.Z)
                {
                    vectord = vectord8;
                }
            }
            Vector3D.Distance(ref vectord5, ref vectord6, out num);
            Vector3D.Distance(ref vectord3, ref vectord4, out num2);
            Vector3D.Distance(ref vectord, ref vectord2, out num3);
            if (num > num2)
            {
                if (num > num3)
                {
                    Vector3D.Lerp(ref vectord5, ref vectord6, 0.5, out vectord7);
                    num4 = num * 0.5;
                }
                else
                {
                    Vector3D.Lerp(ref vectord, ref vectord2, 0.5, out vectord7);
                    num4 = num3 * 0.5;
                }
            }
            else if (num2 > num3)
            {
                Vector3D.Lerp(ref vectord3, ref vectord4, 0.5, out vectord7);
                num4 = num2 * 0.5;
            }
            else
            {
                Vector3D.Lerp(ref vectord, ref vectord2, 0.5, out vectord7);
                num4 = num3 * 0.5;
            }
            foreach (Vector3D vectord9 in points)
            {
                Vector3D vectord10;
                vectord10.X = vectord9.X - vectord7.X;
                vectord10.Y = vectord9.Y - vectord7.Y;
                vectord10.Z = vectord9.Z - vectord7.Z;
                double num6 = vectord10.Length();
                if (num6 > num4)
                {
                    num4 = (num4 + num6) * 0.5;
                    vectord7 += (1.0 - (num4 / num6)) * vectord10;
                }
            }
            ed.Center = vectord7;
            ed.Radius = num4;
            return ed;
        }

        public static BoundingSphereD CreateFromFrustum(BoundingFrustumD frustum)
        {
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            return CreateFromPoints(frustum.CornerArray);
        }

        public bool Intersects(BoundingBoxD box)
        {
            Vector3D vectord;
            double num;
            Vector3D.Clamp(ref this.Center, ref box.Min, ref box.Max, out vectord);
            Vector3D.DistanceSquared(ref this.Center, ref vectord, out num);
            return (num <= (this.Radius * this.Radius));
        }

        public void Intersects(ref BoundingBoxD box, out bool result)
        {
            Vector3D vectord;
            double num;
            Vector3D.Clamp(ref this.Center, ref box.Min, ref box.Max, out vectord);
            Vector3D.DistanceSquared(ref this.Center, ref vectord, out num);
            result = num <= (this.Radius * this.Radius);
        }

        public double? Intersects(RayD ray) => 
            ray.Intersects(this);

        public bool Intersects(BoundingFrustumD frustum)
        {
            bool flag;
            frustum.Intersects(ref this, out flag);
            return flag;
        }

        public bool Intersects(BoundingSphereD sphere)
        {
            double num;
            Vector3D.DistanceSquared(ref this.Center, ref sphere.Center, out num);
            double radius = this.Radius;
            double num3 = sphere.Radius;
            return ((((radius * radius) + ((2.0 * radius) * num3)) + (num3 * num3)) > num);
        }

        public void Intersects(ref BoundingSphereD sphere, out bool result)
        {
            double num;
            Vector3D.DistanceSquared(ref this.Center, ref sphere.Center, out num);
            double radius = this.Radius;
            double num3 = sphere.Radius;
            result = (((radius * radius) + ((2.0 * radius) * num3)) + (num3 * num3)) > num;
        }

        public ContainmentType Contains(BoundingBoxD box)
        {
            Vector3D vectord;
            if (!box.Intersects(this))
            {
                return ContainmentType.Disjoint;
            }
            double num = this.Radius * this.Radius;
            vectord.X = this.Center.X - box.Min.X;
            vectord.Y = this.Center.Y - box.Max.Y;
            vectord.Z = this.Center.Z - box.Max.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Max.X;
            vectord.Y = this.Center.Y - box.Max.Y;
            vectord.Z = this.Center.Z - box.Max.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Max.X;
            vectord.Y = this.Center.Y - box.Min.Y;
            vectord.Z = this.Center.Z - box.Max.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Min.X;
            vectord.Y = this.Center.Y - box.Min.Y;
            vectord.Z = this.Center.Z - box.Max.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Min.X;
            vectord.Y = this.Center.Y - box.Max.Y;
            vectord.Z = this.Center.Z - box.Min.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Max.X;
            vectord.Y = this.Center.Y - box.Max.Y;
            vectord.Z = this.Center.Z - box.Min.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Max.X;
            vectord.Y = this.Center.Y - box.Min.Y;
            vectord.Z = this.Center.Z - box.Min.Z;
            if (vectord.LengthSquared() > num)
            {
                return ContainmentType.Intersects;
            }
            vectord.X = this.Center.X - box.Min.X;
            vectord.Y = this.Center.Y - box.Min.Y;
            vectord.Z = this.Center.Z - box.Min.Z;
            return ((vectord.LengthSquared() > num) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        public void Contains(ref BoundingBoxD box, out ContainmentType result)
        {
            bool flag;
            box.Intersects(ref this, out flag);
            if (!flag)
            {
                result = ContainmentType.Disjoint;
            }
            else
            {
                Vector3D vectord;
                double num = this.Radius * this.Radius;
                result = ContainmentType.Intersects;
                vectord.X = this.Center.X - box.Min.X;
                vectord.Y = this.Center.Y - box.Max.Y;
                vectord.Z = this.Center.Z - box.Max.Z;
                if (vectord.LengthSquared() <= num)
                {
                    vectord.X = this.Center.X - box.Max.X;
                    vectord.Y = this.Center.Y - box.Max.Y;
                    vectord.Z = this.Center.Z - box.Max.Z;
                    if (vectord.LengthSquared() <= num)
                    {
                        vectord.X = this.Center.X - box.Max.X;
                        vectord.Y = this.Center.Y - box.Min.Y;
                        vectord.Z = this.Center.Z - box.Max.Z;
                        if (vectord.LengthSquared() <= num)
                        {
                            vectord.X = this.Center.X - box.Min.X;
                            vectord.Y = this.Center.Y - box.Min.Y;
                            vectord.Z = this.Center.Z - box.Max.Z;
                            if (vectord.LengthSquared() <= num)
                            {
                                vectord.X = this.Center.X - box.Min.X;
                                vectord.Y = this.Center.Y - box.Max.Y;
                                vectord.Z = this.Center.Z - box.Min.Z;
                                if (vectord.LengthSquared() <= num)
                                {
                                    vectord.X = this.Center.X - box.Max.X;
                                    vectord.Y = this.Center.Y - box.Max.Y;
                                    vectord.Z = this.Center.Z - box.Min.Z;
                                    if (vectord.LengthSquared() <= num)
                                    {
                                        vectord.X = this.Center.X - box.Max.X;
                                        vectord.Y = this.Center.Y - box.Min.Y;
                                        vectord.Z = this.Center.Z - box.Min.Z;
                                        if (vectord.LengthSquared() <= num)
                                        {
                                            vectord.X = this.Center.X - box.Min.X;
                                            vectord.Y = this.Center.Y - box.Min.Y;
                                            vectord.Z = this.Center.Z - box.Min.Z;
                                            if (vectord.LengthSquared() <= num)
                                            {
                                                result = ContainmentType.Contains;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public ContainmentType Contains(BoundingFrustumD frustum)
        {
            if (!frustum.Intersects(this))
            {
                return ContainmentType.Disjoint;
            }
            double num = this.Radius * this.Radius;
            foreach (Vector3D vectord in frustum.CornerArray)
            {
                Vector3D vectord2;
                vectord2.X = vectord.X - this.Center.X;
                vectord2.Y = vectord.Y - this.Center.Y;
                vectord2.Z = vectord.Z - this.Center.Z;
                if (vectord2.LengthSquared() > num)
                {
                    return ContainmentType.Intersects;
                }
            }
            return ContainmentType.Contains;
        }

        public ContainmentType Contains(Vector3D point) => 
            ((Vector3D.DistanceSquared(point, this.Center) >= (this.Radius * this.Radius)) ? ContainmentType.Disjoint : ContainmentType.Contains);

        public void Contains(ref Vector3D point, out ContainmentType result)
        {
            double num;
            Vector3D.DistanceSquared(ref point, ref this.Center, out num);
            result = (num < (this.Radius * this.Radius)) ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        public ContainmentType Contains(BoundingSphereD sphere)
        {
            double num;
            Vector3D.Distance(ref this.Center, ref sphere.Center, out num);
            double radius = this.Radius;
            double num3 = sphere.Radius;
            return (((radius + num3) >= num) ? (((radius - num3) < num) ? ContainmentType.Intersects : ContainmentType.Contains) : ContainmentType.Disjoint);
        }

        public void Contains(ref BoundingSphereD sphere, out ContainmentType result)
        {
            double num;
            Vector3D.Distance(ref this.Center, ref sphere.Center, out num);
            double radius = this.Radius;
            double num3 = sphere.Radius;
            result = ((radius + num3) >= num) ? (((radius - num3) >= num) ? ContainmentType.Contains : ContainmentType.Intersects) : ContainmentType.Disjoint;
        }

        internal void SupportMapping(ref Vector3D v, out Vector3D result)
        {
            double num = this.Radius / v.Length();
            result.X = this.Center.X + (v.X * num);
            result.Y = this.Center.Y + (v.Y * num);
            result.Z = this.Center.Z + (v.Z * num);
        }

        public BoundingSphereD Transform(MatrixD matrix)
        {
            BoundingSphereD ed = new BoundingSphereD {
                Center = Vector3D.Transform(this.Center, matrix)
            };
            double d = Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max((double) (((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23)), (double) (((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33))));
            ed.Radius = this.Radius * Math.Sqrt(d);
            return ed;
        }

        public void Transform(ref MatrixD matrix, out BoundingSphereD result)
        {
            result.Center = Vector3D.Transform(this.Center, (MatrixD) matrix);
            double d = Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max((double) (((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23)), (double) (((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33))));
            result.Radius = this.Radius * Math.Sqrt(d);
        }

        public unsafe bool IntersectRaySphere(RayD ray, out double tmin, out double tmax)
        {
            tmin = 0.0;
            tmax = 0.0;
            Vector3D v = ray.Position - this.Center;
            double num = ray.Direction.Dot(ray.Direction);
            double num2 = 2.0 * v.Dot(ray.Direction);
            Vector3D* vectordPtr1 = (Vector3D*) ref v;
            double num3 = vectordPtr1.Dot(v) - (this.Radius * this.Radius);
            double d = (num2 * num2) - ((4.0 * num) * num3);
            if (d < 0.0)
            {
                return false;
            }
            tmin = (-num2 - Math.Sqrt(d)) / (2.0 * num);
            tmax = (-num2 + Math.Sqrt(d)) / (2.0 * num);
            if (tmin > tmax)
            {
                double num5 = tmin;
                tmin = tmax;
                tmax = num5;
            }
            return true;
        }

        public BoundingSphereD Include(BoundingSphereD sphere)
        {
            Include(ref this, ref sphere);
            return this;
        }

        public static void Include(ref BoundingSphereD sphere, ref BoundingSphereD otherSphere)
        {
            if (sphere.Radius == double.MinValue)
            {
                sphere.Center = otherSphere.Center;
                sphere.Radius = otherSphere.Radius;
            }
            else
            {
                double num = Vector3D.Distance(sphere.Center, otherSphere.Center);
                if ((num + otherSphere.Radius) > sphere.Radius)
                {
                    if ((num + sphere.Radius) <= otherSphere.Radius)
                    {
                        sphere = otherSphere;
                    }
                    else
                    {
                        double amount = ((num + otherSphere.Radius) - sphere.Radius) / (2.0 * num);
                        Vector3D vectord = Vector3D.Lerp(sphere.Center, otherSphere.Center, amount);
                        double num3 = ((num + sphere.Radius) + otherSphere.Radius) / 2.0;
                        sphere.Center = vectord;
                        sphere.Radius = num3;
                    }
                }
            }
        }

        public static BoundingSphereD CreateInvalid() => 
            new BoundingSphereD(Vector3D.Zero, double.MinValue);

        public static implicit operator BoundingSphereD(BoundingSphere b) => 
            new BoundingSphereD(b.Center, (double) b.Radius);

        public static implicit operator BoundingSphere(BoundingSphereD b) => 
            new BoundingSphere((Vector3) b.Center, (float) b.Radius);
    }
}

