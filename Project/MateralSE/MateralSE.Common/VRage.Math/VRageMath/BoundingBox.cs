﻿namespace VRageMath
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public const int CornerCount = 8;
        [ProtoMember(20)]
        public Vector3 Min;
        [ProtoMember(0x19)]
        public Vector3 Max;
        public static readonly ComparerType Comparer;
        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public BoundingBox(BoundingBoxD bbd)
        {
            this.Min = (Vector3) bbd.Min;
            this.Max = (Vector3) bbd.Max;
        }

        public BoundingBox(BoundingBoxI bbd)
        {
            this.Min = (Vector3) bbd.Min;
            this.Max = (Vector3) bbd.Max;
        }

        public BoxCornerEnumerator Corners
        {
            get => 
                new BoxCornerEnumerator(this.Min, this.Max);
            set
            {
            }
        }
        public static bool operator ==(BoundingBox a, BoundingBox b) => 
            a.Equals(b);

        public static bool operator !=(BoundingBox a, BoundingBox b) => 
            ((a.Min != b.Min) || (a.Max != b.Max));

        public Vector3[] GetCorners() => 
            new Vector3[] { new Vector3(this.Min.X, this.Max.Y, this.Max.Z), new Vector3(this.Max.X, this.Max.Y, this.Max.Z), new Vector3(this.Max.X, this.Min.Y, this.Max.Z), new Vector3(this.Min.X, this.Min.Y, this.Max.Z), new Vector3(this.Min.X, this.Max.Y, this.Min.Z), new Vector3(this.Max.X, this.Max.Y, this.Min.Z), new Vector3(this.Max.X, this.Min.Y, this.Min.Z), new Vector3(this.Min.X, this.Min.Y, this.Min.Z) };

        public void GetCorners(Vector3[] corners)
        {
            corners[0].X = this.Min.X;
            corners[0].Y = this.Max.Y;
            corners[0].Z = this.Max.Z;
            corners[1].X = this.Max.X;
            corners[1].Y = this.Max.Y;
            corners[1].Z = this.Max.Z;
            corners[2].X = this.Max.X;
            corners[2].Y = this.Min.Y;
            corners[2].Z = this.Max.Z;
            corners[3].X = this.Min.X;
            corners[3].Y = this.Min.Y;
            corners[3].Z = this.Max.Z;
            corners[4].X = this.Min.X;
            corners[4].Y = this.Max.Y;
            corners[4].Z = this.Min.Z;
            corners[5].X = this.Max.X;
            corners[5].Y = this.Max.Y;
            corners[5].Z = this.Min.Z;
            corners[6].X = this.Max.X;
            corners[6].Y = this.Min.Y;
            corners[6].Z = this.Min.Z;
            corners[7].X = this.Min.X;
            corners[7].Y = this.Min.Y;
            corners[7].Z = this.Min.Z;
        }

        public unsafe void GetCornersUnsafe(Vector3* corners)
        {
            corners.X = this.Min.X;
            corners.Y = this.Max.Y;
            corners.Z = this.Max.Z;
            corners[1].X = this.Max.X;
            corners[1].Y = this.Max.Y;
            corners[1].Z = this.Max.Z;
            corners[2].X = this.Max.X;
            corners[2].Y = this.Min.Y;
            corners[2].Z = this.Max.Z;
            corners[3].X = this.Min.X;
            corners[3].Y = this.Min.Y;
            corners[3].Z = this.Max.Z;
            corners[4].X = this.Min.X;
            corners[4].Y = this.Max.Y;
            corners[4].Z = this.Min.Z;
            corners[5].X = this.Max.X;
            corners[5].Y = this.Max.Y;
            corners[5].Z = this.Min.Z;
            corners[6].X = this.Max.X;
            corners[6].Y = this.Min.Y;
            corners[6].Z = this.Min.Z;
            corners[7].X = this.Min.X;
            corners[7].Y = this.Min.Y;
            corners[7].Z = this.Min.Z;
        }

        public bool Equals(BoundingBox other) => 
            ((this.Min == other.Min) && (this.Max == other.Max));

        public override bool Equals(object obj) => 
            ((obj is BoundingBox) && this.Equals((BoundingBox) obj));

        public bool Equals(BoundingBox other, float epsilon) => 
            (this.Min.Equals(other.Min, epsilon) && this.Max.Equals(other.Max, epsilon));

        public override int GetHashCode() => 
            (this.Min.GetHashCode() + this.Max.GetHashCode());

        public override string ToString()
        {
            object[] args = new object[] { this.Min.ToString(), this.Max.ToString() };
            return string.Format(CultureInfo.CurrentCulture, "{{Min:{0} Max:{1}}}", args);
        }

        public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
        {
            BoundingBox box;
            Vector3.Min(ref original.Min, ref additional.Min, out box.Min);
            Vector3.Max(ref original.Max, ref additional.Max, out box.Max);
            return box;
        }

        public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3.Min(ref original.Min, ref additional.Min, out vector);
            Vector3.Max(ref original.Max, ref additional.Max, out vector2);
            result.Min = vector;
            result.Max = vector2;
        }

        public static BoundingBox CreateFromSphere(BoundingSphere sphere)
        {
            BoundingBox box;
            box.Min.X = sphere.Center.X - sphere.Radius;
            box.Min.Y = sphere.Center.Y - sphere.Radius;
            box.Min.Z = sphere.Center.Z - sphere.Radius;
            box.Max.X = sphere.Center.X + sphere.Radius;
            box.Max.Y = sphere.Center.Y + sphere.Radius;
            box.Max.Z = sphere.Center.Z + sphere.Radius;
            return box;
        }

        public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
        {
            result.Min.X = sphere.Center.X - sphere.Radius;
            result.Min.Y = sphere.Center.Y - sphere.Radius;
            result.Min.Z = sphere.Center.Z - sphere.Radius;
            result.Max.X = sphere.Center.X + sphere.Radius;
            result.Max.Y = sphere.Center.Y + sphere.Radius;
            result.Max.Z = sphere.Center.Z + sphere.Radius;
        }

        public static unsafe BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException();
            }
            bool flag = false;
            Vector3 result = new Vector3(float.MaxValue);
            Vector3 vector2 = new Vector3(float.MinValue);
            foreach (Vector3 vector3 in points)
            {
                Vector3* vectorPtr1 = (Vector3*) ref result;
                Vector3.Min(ref (Vector3) ref vectorPtr1, ref vector3, out result);
                Vector3* vectorPtr2 = (Vector3*) ref vector2;
                Vector3.Max(ref (Vector3) ref vectorPtr2, ref vector3, out vector2);
                flag = true;
            }
            if (!flag)
            {
                throw new ArgumentException();
            }
            return new BoundingBox(result, vector2);
        }

        public static BoundingBox CreateFromHalfExtent(Vector3 center, float halfExtent) => 
            CreateFromHalfExtent(center, new Vector3(halfExtent));

        public static BoundingBox CreateFromHalfExtent(Vector3 center, Vector3 halfExtent) => 
            new BoundingBox(center - halfExtent, center + halfExtent);

        public BoundingBox Intersect(BoundingBox box)
        {
            BoundingBox box2;
            box2.Min.X = Math.Max(this.Min.X, box.Min.X);
            box2.Min.Y = Math.Max(this.Min.Y, box.Min.Y);
            box2.Min.Z = Math.Max(this.Min.Z, box.Min.Z);
            box2.Max.X = Math.Min(this.Max.X, box.Max.X);
            box2.Max.Y = Math.Min(this.Max.Y, box.Max.Y);
            box2.Max.Z = Math.Min(this.Max.Z, box.Max.Z);
            return box2;
        }

        public bool Intersects(BoundingBox box) => 
            this.Intersects(ref box);

        public bool Intersects(ref BoundingBox box) => 
            ((this.Max.X >= box.Min.X) && ((this.Min.X <= box.Max.X) && ((this.Max.Y >= box.Min.Y) && ((this.Min.Y <= box.Max.Y) && ((this.Max.Z >= box.Min.Z) && (this.Min.Z <= box.Max.Z))))));

        public void Intersects(ref BoundingBox box, out bool result)
        {
            result = false;
            if (((this.Max.X >= box.Min.X) && ((this.Min.X <= box.Max.X) && ((this.Max.Y >= box.Min.Y) && ((this.Min.Y <= box.Max.Y) && (this.Max.Z >= box.Min.Z))))) && (this.Min.Z <= box.Max.Z))
            {
                result = true;
            }
        }

        public bool IntersectsTriangle(Vector3 v0, Vector3 v1, Vector3 v2) => 
            this.IntersectsTriangle(ref v0, ref v1, ref v2);

        public unsafe bool IntersectsTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector5;
            float num;
            PlaneIntersectionType type;
            Vector3.Min(ref v0, ref v1, out vector);
            Vector3* vectorPtr1 = (Vector3*) ref vector;
            Vector3.Min(ref (Vector3) ref vectorPtr1, ref v2, out vector);
            Vector3.Max(ref v0, ref v1, out vector2);
            Vector3* vectorPtr2 = (Vector3*) ref vector2;
            Vector3.Max(ref (Vector3) ref vectorPtr2, ref v2, out vector2);
            if (vector.X > this.Max.X)
            {
                return false;
            }
            if (vector2.X < this.Min.X)
            {
                return false;
            }
            if (vector.Y > this.Max.Y)
            {
                return false;
            }
            if (vector2.Y < this.Min.Y)
            {
                return false;
            }
            if (vector.Z > this.Max.Z)
            {
                return false;
            }
            if (vector2.Z < this.Min.Z)
            {
                return false;
            }
            Vector3 vector3 = v1 - v0;
            Vector3 vector4 = v2 - v1;
            Vector3.Cross(ref vector3, ref vector4, out vector5);
            Vector3.Dot(ref v0, ref vector5, out num);
            Plane plane = new Plane(vector5, -num);
            this.Intersects(ref plane, out type);
            if (type == PlaneIntersectionType.Back)
            {
                return false;
            }
            if (type == PlaneIntersectionType.Front)
            {
                return false;
            }
            Vector3 center = this.Center;
            BoundingBox box = new BoundingBox(this.Min - center, this.Max - center);
            Vector3 halfExtents = box.HalfExtents;
            Vector3 vector8 = v0 - v2;
            Vector3 vector9 = v0 - center;
            Vector3 vector10 = v1 - center;
            Vector3 vector11 = v2 - center;
            float num2 = (halfExtents.Y * Math.Abs(vector3.Z)) + (halfExtents.Z * Math.Abs(vector3.Y));
            float num3 = (vector9.Z * vector10.Y) - (vector9.Y * vector10.Z);
            float num5 = (vector11.Z * vector3.Y) - (vector11.Y * vector3.Z);
            if ((Math.Min(num3, num5) > num2) || (Math.Max(num3, num5) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector3.Z)) + (halfExtents.Z * Math.Abs(vector3.X));
            num3 = (vector9.X * vector10.Z) - (vector9.Z * vector10.X);
            num5 = (vector11.X * vector3.Z) - (vector11.Z * vector3.X);
            if ((Math.Min(num3, num5) > num2) || (Math.Max(num3, num5) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector3.Y)) + (halfExtents.Y * Math.Abs(vector3.X));
            num3 = (vector9.Y * vector10.X) - (vector9.X * vector10.Y);
            num5 = (vector11.Y * vector3.X) - (vector11.X * vector3.Y);
            if ((Math.Min(num3, num5) > num2) || (Math.Max(num3, num5) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.Y * Math.Abs(vector4.Z)) + (halfExtents.Z * Math.Abs(vector4.Y));
            float num4 = (vector10.Z * vector11.Y) - (vector10.Y * vector11.Z);
            num3 = (vector9.Z * vector4.Y) - (vector9.Y * vector4.Z);
            if ((Math.Min(num4, num3) > num2) || (Math.Max(num4, num3) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector4.Z)) + (halfExtents.Z * Math.Abs(vector4.X));
            num4 = (vector10.X * vector11.Z) - (vector10.Z * vector11.X);
            num3 = (vector9.X * vector4.Z) - (vector9.Z * vector4.X);
            if ((Math.Min(num4, num3) > num2) || (Math.Max(num4, num3) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector4.Y)) + (halfExtents.Y * Math.Abs(vector4.X));
            num4 = (vector10.Y * vector11.X) - (vector10.X * vector11.Y);
            num3 = (vector9.Y * vector4.X) - (vector9.X * vector4.Y);
            if ((Math.Min(num4, num3) > num2) || (Math.Max(num4, num3) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.Y * Math.Abs(vector8.Z)) + (halfExtents.Z * Math.Abs(vector8.Y));
            num5 = (vector11.Z * vector9.Y) - (vector11.Y * vector9.Z);
            num4 = (vector10.Z * vector8.Y) - (vector10.Y * vector8.Z);
            if ((Math.Min(num5, num4) > num2) || (Math.Max(num5, num4) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector8.Z)) + (halfExtents.Z * Math.Abs(vector8.X));
            num5 = (vector11.X * vector9.Z) - (vector11.Z * vector9.X);
            num4 = (vector10.X * vector8.Z) - (vector10.Z * vector8.X);
            if ((Math.Min(num5, num4) > num2) || (Math.Max(num5, num4) < -num2))
            {
                return false;
            }
            num2 = (halfExtents.X * Math.Abs(vector8.Y)) + (halfExtents.Y * Math.Abs(vector8.X));
            num5 = (vector11.Y * vector9.X) - (vector11.X * vector9.Y);
            num4 = (vector10.Y * vector8.X) - (vector10.X * vector8.Y);
            return ((Math.Min(num5, num4) <= num2) && (Math.Max(num5, num4) >= -num2));
        }

        public Vector3 Center =>
            ((this.Min + this.Max) * 0.5f);
        public Vector3 HalfExtents =>
            ((this.Max - this.Min) * 0.5f);
        public Vector3 Extents =>
            (this.Max - this.Min);
        public float Width =>
            (this.Max.X - this.Min.X);
        public float Height =>
            (this.Max.Y - this.Min.Y);
        public float Depth =>
            (this.Max.Z - this.Min.Z);
        public bool Intersects(BoundingFrustum frustum)
        {
            if (null == frustum)
            {
                throw new ArgumentNullException("frustum");
            }
            return frustum.Intersects(this);
        }

        public unsafe PlaneIntersectionType Intersects(Plane plane)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3* vectorPtr1;
            Vector3* vectorPtr2;
            Vector3* vectorPtr3;
            Vector3* vectorPtr4;
            Vector3* vectorPtr5;
            Vector3* vectorPtr6;
            vectorPtr1->X = (plane.Normal.X >= 0.0) ? this.Min.X : this.Max.X;
            vectorPtr1 = (Vector3*) ref vector;
            vectorPtr2->Y = (plane.Normal.Y >= 0.0) ? this.Min.Y : this.Max.Y;
            vectorPtr2 = (Vector3*) ref vector;
            vectorPtr3->Z = (plane.Normal.Z >= 0.0) ? this.Min.Z : this.Max.Z;
            vectorPtr3 = (Vector3*) ref vector;
            if (((((plane.Normal.X * vector.X) + (plane.Normal.Y * vector.Y)) + (plane.Normal.Z * vector.Z)) + plane.D) > 0.0)
            {
                return PlaneIntersectionType.Front;
            }
            vectorPtr4->X = (plane.Normal.X >= 0.0) ? this.Max.X : this.Min.X;
            vectorPtr4 = (Vector3*) ref vector2;
            vectorPtr5->Y = (plane.Normal.Y >= 0.0) ? this.Max.Y : this.Min.Y;
            vectorPtr5 = (Vector3*) ref vector2;
            vectorPtr6->Z = (plane.Normal.Z >= 0.0) ? this.Max.Z : this.Min.Z;
            vectorPtr6 = (Vector3*) ref vector2;
            return ((((((plane.Normal.X * vector2.X) + (plane.Normal.Y * vector2.Y)) + (plane.Normal.Z * vector2.Z)) + plane.D) < 0.0) ? PlaneIntersectionType.Back : PlaneIntersectionType.Intersecting);
        }

        public unsafe void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3* vectorPtr1;
            Vector3* vectorPtr2;
            Vector3* vectorPtr3;
            Vector3* vectorPtr4;
            Vector3* vectorPtr5;
            Vector3* vectorPtr6;
            vectorPtr1->X = (plane.Normal.X >= 0.0) ? this.Min.X : this.Max.X;
            vectorPtr1 = (Vector3*) ref vector;
            vectorPtr2->Y = (plane.Normal.Y >= 0.0) ? this.Min.Y : this.Max.Y;
            vectorPtr2 = (Vector3*) ref vector;
            vectorPtr3->Z = (plane.Normal.Z >= 0.0) ? this.Min.Z : this.Max.Z;
            vectorPtr3 = (Vector3*) ref vector;
            if (((((plane.Normal.X * vector.X) + (plane.Normal.Y * vector.Y)) + (plane.Normal.Z * vector.Z)) + plane.D) > 0.0)
            {
                result = PlaneIntersectionType.Front;
            }
            vectorPtr4->X = (plane.Normal.X >= 0.0) ? this.Max.X : this.Min.X;
            vectorPtr4 = (Vector3*) ref vector2;
            vectorPtr5->Y = (plane.Normal.Y >= 0.0) ? this.Max.Y : this.Min.Y;
            vectorPtr5 = (Vector3*) ref vector2;
            vectorPtr6->Z = (plane.Normal.Z >= 0.0) ? this.Max.Z : this.Min.Z;
            vectorPtr6 = (Vector3*) ref vector2;
            result = (((((plane.Normal.X * vector2.X) + (plane.Normal.Y * vector2.Y)) + (plane.Normal.Z * vector2.Z)) + plane.D) < 0.0) ? PlaneIntersectionType.Back : PlaneIntersectionType.Intersecting;
        }

        public bool Intersects(Line line, out float distance)
        {
            distance = 0f;
            float? nullable = this.Intersects(new Ray(line.From, line.Direction));
            if (nullable == null)
            {
                return false;
            }
            if (nullable.Value < 0f)
            {
                return false;
            }
            if (nullable.Value > line.Length)
            {
                return false;
            }
            distance = nullable.Value;
            return true;
        }

        public float? Intersects(Ray ray)
        {
            float num = 0f;
            float maxValue = float.MaxValue;
            if (Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((ray.Position.X < this.Min.X) || (ray.Position.X > this.Max.X))
                {
                    return null;
                }
            }
            else
            {
                float num3 = 1f / ray.Direction.X;
                float num4 = (this.Min.X - ray.Position.X) * num3;
                float num5 = (this.Max.X - ray.Position.X) * num3;
                if (num4 > num5)
                {
                    num4 = num5;
                    num5 = num4;
                }
                maxValue = MathHelper.Min(num5, maxValue);
                if (MathHelper.Max(num4, num) > maxValue)
                {
                    return null;
                }
            }
            if (Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
            {
                if ((ray.Position.Y < this.Min.Y) || (ray.Position.Y > this.Max.Y))
                {
                    return null;
                }
            }
            else
            {
                float num6 = 1f / ray.Direction.Y;
                float num7 = (this.Min.Y - ray.Position.Y) * num6;
                float num8 = (this.Max.Y - ray.Position.Y) * num6;
                if (num7 > num8)
                {
                    num7 = num8;
                    num8 = num7;
                }
                maxValue = MathHelper.Min(num8, maxValue);
                if (MathHelper.Max(num7, num) > maxValue)
                {
                    return null;
                }
            }
            if (Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
            {
                if ((ray.Position.Z < this.Min.Z) || (ray.Position.Z > this.Max.Z))
                {
                    return null;
                }
            }
            else
            {
                float num9 = 1f / ray.Direction.Z;
                float num10 = (this.Min.Z - ray.Position.Z) * num9;
                float num11 = (this.Max.Z - ray.Position.Z) * num9;
                if (num10 > num11)
                {
                    num10 = num11;
                    num11 = num10;
                }
                num = MathHelper.Max(num10, num);
                float num12 = MathHelper.Min(num11, maxValue);
                if (num > num12)
                {
                    return null;
                }
            }
            return new float?(num);
        }

        public void Intersects(ref Ray ray, out float? result)
        {
            result = 0;
            float num = 0f;
            float maxValue = float.MaxValue;
            if (Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((ray.Position.X < this.Min.X) || (ray.Position.X > this.Max.X))
                {
                    return;
                }
            }
            else
            {
                float num3 = 1f / ray.Direction.X;
                float num4 = (this.Min.X - ray.Position.X) * num3;
                float num5 = (this.Max.X - ray.Position.X) * num3;
                if (num4 > num5)
                {
                    num4 = num5;
                    num5 = num4;
                }
                maxValue = MathHelper.Min(num5, maxValue);
                if (MathHelper.Max(num4, num) > maxValue)
                {
                    return;
                }
            }
            if (Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
            {
                if ((ray.Position.Y < this.Min.Y) || (ray.Position.Y > this.Max.Y))
                {
                    return;
                }
            }
            else
            {
                float num6 = 1f / ray.Direction.Y;
                float num7 = (this.Min.Y - ray.Position.Y) * num6;
                float num8 = (this.Max.Y - ray.Position.Y) * num6;
                if (num7 > num8)
                {
                    num7 = num8;
                    num8 = num7;
                }
                maxValue = MathHelper.Min(num8, maxValue);
                if (MathHelper.Max(num7, num) > maxValue)
                {
                    return;
                }
            }
            if (Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
            {
                if ((ray.Position.Z < this.Min.Z) || (ray.Position.Z > this.Max.Z))
                {
                    return;
                }
            }
            else
            {
                float num9 = 1f / ray.Direction.Z;
                float num10 = (this.Min.Z - ray.Position.Z) * num9;
                float num11 = (this.Max.Z - ray.Position.Z) * num9;
                if (num10 > num11)
                {
                    num10 = num11;
                    num11 = num10;
                }
                num = MathHelper.Max(num10, num);
                float num12 = MathHelper.Min(num11, maxValue);
                if (num > num12)
                {
                    return;
                }
            }
            result = new float?(num);
        }

        public bool Intersects(BoundingSphere sphere) => 
            this.Intersects(ref sphere);

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            Vector3 vector;
            float num;
            Vector3.Clamp(ref sphere.Center, ref this.Min, ref this.Max, out vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out num);
            result = num <= (sphere.Radius * sphere.Radius);
        }

        public bool Intersects(ref BoundingSphere sphere)
        {
            Vector3 vector;
            float num;
            Vector3.Clamp(ref sphere.Center, ref this.Min, ref this.Max, out vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out num);
            return (num <= (sphere.Radius * sphere.Radius));
        }

        public bool Intersects(ref BoundingSphereD sphere)
        {
            Vector3 vector;
            float num;
            Vector3 center = (Vector3) sphere.Center;
            Vector3.Clamp(ref center, ref this.Min, ref this.Max, out vector);
            Vector3.DistanceSquared(ref center, ref vector, out num);
            return (num <= (sphere.Radius * sphere.Radius));
        }

        public float Distance(Vector3 point) => 
            ((this.Contains(point) != ContainmentType.Contains) ? Vector3.Distance(Vector3.Clamp(point, this.Min, this.Max), point) : 0f);

        public float DistanceSquared(Vector3 point) => 
            ((this.Contains(point) != ContainmentType.Contains) ? Vector3.DistanceSquared(Vector3.Clamp(point, this.Min, this.Max), point) : 0f);

        public ContainmentType Contains(BoundingBox box)
        {
            if (((this.Max.X < box.Min.X) || ((this.Min.X > box.Max.X) || ((this.Max.Y < box.Min.Y) || ((this.Min.Y > box.Max.Y) || (this.Max.Z < box.Min.Z))))) || (this.Min.Z > box.Max.Z))
            {
                return ContainmentType.Disjoint;
            }
            if (((this.Min.X > box.Min.X) || ((box.Max.X > this.Max.X) || ((this.Min.Y > box.Min.Y) || ((box.Max.Y > this.Max.Y) || (this.Min.Z > box.Min.Z))))) || (box.Max.Z > this.Max.Z))
            {
                return ContainmentType.Intersects;
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = ContainmentType.Disjoint;
            if (((this.Max.X >= box.Min.X) && ((this.Min.X <= box.Max.X) && ((this.Max.Y >= box.Min.Y) && ((this.Min.Y <= box.Max.Y) && (this.Max.Z >= box.Min.Z))))) && (this.Min.Z <= box.Max.Z))
            {
                int num1;
                if (((this.Min.X > box.Min.X) || ((box.Max.X > this.Max.X) || ((this.Min.Y > box.Min.Y) || ((box.Max.Y > this.Max.Y) || (this.Min.Z > box.Min.Z))))) || (box.Max.Z > this.Max.Z))
                {
                    num1 = 2;
                }
                else
                {
                    num1 = 1;
                }
                result = (ContainmentType) num1;
            }
        }

        public ContainmentType Contains(BoundingFrustum frustum)
        {
            if (!frustum.Intersects(this))
            {
                return ContainmentType.Disjoint;
            }
            foreach (Vector3 vector in frustum.cornerArray)
            {
                if (this.Contains(vector) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }
            return ContainmentType.Contains;
        }

        public ContainmentType Contains(Vector3 point)
        {
            if (((this.Min.X > point.X) || ((point.X > this.Max.X) || ((this.Min.Y > point.Y) || ((point.Y > this.Max.Y) || (this.Min.Z > point.Z))))) || (point.Z > this.Max.Z))
            {
                return ContainmentType.Disjoint;
            }
            return ContainmentType.Contains;
        }

        public ContainmentType Contains(Vector3D point)
        {
            if (((this.Min.X > point.X) || ((point.X > this.Max.X) || ((this.Min.Y > point.Y) || ((point.Y > this.Max.Y) || (this.Min.Z > point.Z))))) || (point.Z > this.Max.Z))
            {
                return ContainmentType.Disjoint;
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            int num1;
            if (((this.Min.X > point.X) || ((point.X > this.Max.X) || ((this.Min.Y > point.Y) || ((point.Y > this.Max.Y) || (this.Min.Z > point.Z))))) || (point.Z > this.Max.Z))
            {
                num1 = 0;
            }
            else
            {
                num1 = 1;
            }
            result = (ContainmentType) num1;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            Vector3 vector;
            float num;
            Vector3.Clamp(ref sphere.Center, ref this.Min, ref this.Max, out vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out num);
            float radius = sphere.Radius;
            if (num > (radius * radius))
            {
                return ContainmentType.Disjoint;
            }
            if ((((this.Min.X + radius) > sphere.Center.X) || ((sphere.Center.X > (this.Max.X - radius)) || (((this.Max.X - this.Min.X) <= radius) || (((this.Min.Y + radius) > sphere.Center.Y) || ((sphere.Center.Y > (this.Max.Y - radius)) || (((this.Max.Y - this.Min.Y) <= radius) || (((this.Min.Z + radius) > sphere.Center.Z) || (sphere.Center.Z > (this.Max.Z - radius))))))))) || ((this.Max.X - this.Min.X) <= radius))
            {
                return ContainmentType.Intersects;
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            Vector3 vector;
            float num;
            Vector3.Clamp(ref sphere.Center, ref this.Min, ref this.Max, out vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out num);
            float radius = sphere.Radius;
            if (num > (radius * radius))
            {
                result = ContainmentType.Disjoint;
            }
            else
            {
                int num1;
                if ((((this.Min.X + radius) > sphere.Center.X) || ((sphere.Center.X > (this.Max.X - radius)) || (((this.Max.X - this.Min.X) <= radius) || (((this.Min.Y + radius) > sphere.Center.Y) || ((sphere.Center.Y > (this.Max.Y - radius)) || (((this.Max.Y - this.Min.Y) <= radius) || (((this.Min.Z + radius) > sphere.Center.Z) || (sphere.Center.Z > (this.Max.Z - radius))))))))) || ((this.Max.X - this.Min.X) <= radius))
                {
                    num1 = 2;
                }
                else
                {
                    num1 = 1;
                }
                result = (ContainmentType) num1;
            }
        }

        internal void SupportMapping(ref Vector3 v, out Vector3 result)
        {
            result.X = (v.X >= 0.0) ? this.Max.X : this.Min.X;
            result.Y = (v.Y >= 0.0) ? this.Max.Y : this.Min.Y;
            result.Z = (v.Z >= 0.0) ? this.Max.Z : this.Min.Z;
        }

        public BoundingBox Translate(VRageMath.Matrix worldMatrix)
        {
            this.Min += worldMatrix.Translation;
            this.Max += worldMatrix.Translation;
            return this;
        }

        public BoundingBox Translate(Vector3 vctTranlsation)
        {
            this.Min += vctTranlsation;
            this.Max += vctTranlsation;
            return this;
        }

        public Vector3 Size =>
            Vector3.Abs(this.Max - this.Min);
        public VRageMath.Matrix Matrix
        {
            get
            {
                VRageMath.Matrix matrix;
                Vector3 center = this.Center;
                Vector3 size = this.Size;
                VRageMath.Matrix.CreateTranslation(ref center, out matrix);
                VRageMath.Matrix.Rescale(ref matrix, ref size);
                return matrix;
            }
        }
        public BoundingBox Transform(VRageMath.Matrix worldMatrix) => 
            this.Transform(ref worldMatrix);

        public BoundingBoxD Transform(MatrixD worldMatrix) => 
            this.Transform(ref worldMatrix);

        public BoundingBox Transform(ref VRageMath.Matrix m)
        {
            BoundingBox bb = CreateInvalid();
            this.Transform(ref m, ref bb);
            return bb;
        }

        public unsafe void Transform(ref VRageMath.Matrix m, ref BoundingBox bb)
        {
            bb.Min = bb.Max = m.Translation;
            Vector3 min = m.Right * this.Min.X;
            Vector3 max = m.Right * this.Max.X;
            Vector3.MinMax(ref min, ref max);
            Vector3* vectorPtr1 = (Vector3*) ref bb.Min;
            vectorPtr1[0] += min;
            Vector3* vectorPtr2 = (Vector3*) ref bb.Max;
            vectorPtr2[0] += max;
            min = m.Up * this.Min.Y;
            max = m.Up * this.Max.Y;
            Vector3.MinMax(ref min, ref max);
            Vector3* vectorPtr3 = (Vector3*) ref bb.Min;
            vectorPtr3[0] += min;
            Vector3* vectorPtr4 = (Vector3*) ref bb.Max;
            vectorPtr4[0] += max;
            min = m.Backward * this.Min.Z;
            max = m.Backward * this.Max.Z;
            Vector3.MinMax(ref min, ref max);
            Vector3* vectorPtr5 = (Vector3*) ref bb.Min;
            vectorPtr5[0] += min;
            Vector3* vectorPtr6 = (Vector3*) ref bb.Max;
            vectorPtr6[0] += max;
        }

        public BoundingBoxD Transform(ref MatrixD m)
        {
            BoundingBoxD bb = BoundingBoxD.CreateInvalid();
            this.Transform(ref m, ref bb);
            return bb;
        }

        public unsafe void Transform(ref MatrixD m, ref BoundingBoxD bb)
        {
            bb.Min = bb.Max = m.Translation;
            Vector3D min = m.Right * this.Min.X;
            Vector3D max = m.Right * this.Max.X;
            Vector3D.MinMax(ref min, ref max);
            Vector3D* vectordPtr1 = (Vector3D*) ref bb.Min;
            vectordPtr1[0] += min;
            Vector3D* vectordPtr2 = (Vector3D*) ref bb.Max;
            vectordPtr2[0] += max;
            min = m.Up * this.Min.Y;
            max = m.Up * this.Max.Y;
            Vector3D.MinMax(ref min, ref max);
            Vector3D* vectordPtr3 = (Vector3D*) ref bb.Min;
            vectordPtr3[0] += min;
            Vector3D* vectordPtr4 = (Vector3D*) ref bb.Max;
            vectordPtr4[0] += max;
            min = m.Backward * this.Min.Z;
            max = m.Backward * this.Max.Z;
            Vector3D.MinMax(ref min, ref max);
            Vector3D* vectordPtr5 = (Vector3D*) ref bb.Min;
            vectordPtr5[0] += min;
            Vector3D* vectordPtr6 = (Vector3D*) ref bb.Max;
            vectordPtr6[0] += max;
        }

        public BoundingBox Include(ref Vector3 point)
        {
            this.Min.X = Math.Min(point.X, this.Min.X);
            this.Min.Y = Math.Min(point.Y, this.Min.Y);
            this.Min.Z = Math.Min(point.Z, this.Min.Z);
            this.Max.X = Math.Max(point.X, this.Max.X);
            this.Max.Y = Math.Max(point.Y, this.Max.Y);
            this.Max.Z = Math.Max(point.Z, this.Max.Z);
            return this;
        }

        public BoundingBox GetIncluded(Vector3 point)
        {
            BoundingBox box = this;
            box.Include(point);
            return box;
        }

        public BoundingBox Include(Vector3 point) => 
            this.Include(ref point);

        public BoundingBox Include(Vector3 p0, Vector3 p1, Vector3 p2) => 
            this.Include(ref p0, ref p1, ref p2);

        public BoundingBox Include(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2)
        {
            this.Include(ref p0);
            this.Include(ref p1);
            this.Include(ref p2);
            return this;
        }

        public BoundingBox Include(ref BoundingBox box)
        {
            this.Min = Vector3.Min(this.Min, box.Min);
            this.Max = Vector3.Max(this.Max, box.Max);
            return this;
        }

        public BoundingBox Include(BoundingBox box) => 
            this.Include(ref box);

        public void Include(ref Line line)
        {
            this.Include(ref line.From);
            this.Include(ref line.To);
        }

        public BoundingBox Include(BoundingSphere sphere) => 
            this.Include(ref sphere);

        public unsafe BoundingBox Include(ref BoundingSphere sphere)
        {
            Vector3 vector = new Vector3(sphere.Radius);
            Vector3 center = sphere.Center;
            Vector3 result = sphere.Center;
            Vector3* vectorPtr1 = (Vector3*) ref center;
            Vector3.Subtract(ref (Vector3) ref vectorPtr1, ref vector, out center);
            Vector3* vectorPtr2 = (Vector3*) ref result;
            Vector3.Add(ref (Vector3) ref vectorPtr2, ref vector, out result);
            this.Include(ref center);
            this.Include(ref result);
            return this;
        }

        public unsafe BoundingBox Include(ref BoundingFrustum frustum)
        {
            Vector3* corners = (Vector3*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3))];
            frustum.GetCornersUnsafe(corners);
            this.Include(ref (ref Vector3) ((Vector3) ref corners));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 1)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 2)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 3)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 4)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 5)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 6)));
            this.Include(ref (ref Vector3) ((Vector3) ref (corners + 7)));
            return this;
        }

        public static BoundingBox CreateInvalid()
        {
            BoundingBox box = new BoundingBox();
            Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            box.Min = vector;
            box.Max = vector2;
            return box;
        }

        public float SurfaceArea()
        {
            Vector3 vector = this.Max - this.Min;
            return (2f * (((vector.X * vector.Y) + (vector.X * vector.Z)) + (vector.Y * vector.Z)));
        }

        public float Volume()
        {
            Vector3 vector = this.Max - this.Min;
            return ((vector.X * vector.Y) * vector.Z);
        }

        public float ProjectedArea(Vector3 viewDir)
        {
            Vector3 vector = this.Max - this.Min;
            return Vector3.Abs(viewDir).Dot(new Vector3(vector.Y, vector.Z, vector.X) * new Vector3(vector.Z, vector.X, vector.Y));
        }

        public float Perimeter
        {
            get
            {
                float num3 = this.Max.Z - this.Min.Z;
                return (4f * (((this.Max.X - this.Min.X) + (this.Max.Y - this.Min.Y)) + num3));
            }
        }
        public void Inflate(float size)
        {
            this.Max += new Vector3(size);
            this.Min -= new Vector3(size);
        }

        public void Inflate(Vector3 size)
        {
            this.Max += size;
            this.Min -= size;
        }

        public void InflateToMinimum(Vector3 minimumSize)
        {
            Vector3 center = this.Center;
            if (this.Size.X < minimumSize.X)
            {
                this.Min.X = center.X - (minimumSize.X * 0.5f);
                this.Max.X = center.X + (minimumSize.X * 0.5f);
            }
            if (this.Size.Y < minimumSize.Y)
            {
                this.Min.Y = center.Y - (minimumSize.Y * 0.5f);
                this.Max.Y = center.Y + (minimumSize.Y * 0.5f);
            }
            if (this.Size.Z < minimumSize.Z)
            {
                this.Min.Z = center.Z - (minimumSize.Z * 0.5f);
                this.Max.Z = center.Z + (minimumSize.Z * 0.5f);
            }
        }

        public void Scale(Vector3 scale)
        {
            Vector3 center = this.Center;
            Vector3 vector2 = this.HalfExtents * scale;
            this.Min = center - vector2;
            this.Max = center + vector2;
        }

        public BoundingBox Round(int decimals) => 
            new BoundingBox(Vector3.Round(this.Min, decimals), Vector3.Round(this.Max, decimals));

        public BoundingBoxI Round() => 
            new BoundingBoxI(Vector3D.Round(this.Min), Vector3D.Round(this.Max));

        static BoundingBox()
        {
            Comparer = new ComparerType();
        }
        public class ComparerType : IEqualityComparer<BoundingBoxD>
        {
            public bool Equals(BoundingBoxD x, BoundingBoxD y) => 
                ((x.Min == y.Min) && (x.Max == y.Max));

            public int GetHashCode(BoundingBoxD obj) => 
                (obj.Min.GetHashCode() ^ obj.Max.GetHashCode());
        }
    }
}

