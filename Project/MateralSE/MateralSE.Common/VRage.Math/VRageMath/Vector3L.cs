﻿namespace VRageMath
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct Vector3L : IEquatable<Vector3L>, IComparable<Vector3L>
    {
        public static readonly EqualityComparer Comparer;
        public static Vector3L UnitX;
        public static Vector3L UnitY;
        public static Vector3L UnitZ;
        public static Vector3L Zero;
        public static Vector3L MaxValue;
        public static Vector3L MinValue;
        public static Vector3L Up;
        public static Vector3L Down;
        public static Vector3L Right;
        public static Vector3L Left;
        public static Vector3L Forward;
        public static Vector3L Backward;
        public static Vector3L One;
        [ProtoMember(0x67)]
        public long X;
        [ProtoMember(0x69)]
        public long Y;
        [ProtoMember(0x6b)]
        public long Z;
        public long this[long index]
        {
            get
            {
                if (index > 2L)
                {
                    long local1 = index;
                }
                else
                {
                    switch (((uint) index))
                    {
                        case 0:
                            return this.X;

                        case 1:
                            return this.Y;

                        case 2:
                            return this.Z;

                        default:
                            break;
                    }
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index > 2L)
                {
                    long local1 = index;
                }
                else
                {
                    switch (((uint) index))
                    {
                        case 0:
                            this.X = value;
                            return;

                        case 1:
                            this.Y = value;
                            return;

                        case 2:
                            this.Z = value;
                            return;

                        default:
                            break;
                    }
                }
                throw new IndexOutOfRangeException();
            }
        }
        public Vector3L(long xyz)
        {
            this.X = xyz;
            this.Y = xyz;
            this.Z = xyz;
        }

        public Vector3L(long x, long y, long z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3L(Vector3 xyz)
        {
            this.X = (long) xyz.X;
            this.Y = (long) xyz.Y;
            this.Z = (long) xyz.Z;
        }

        public Vector3L(Vector3D xyz)
        {
            this.X = (long) xyz.X;
            this.Y = (long) xyz.Y;
            this.Z = (long) xyz.Z;
        }

        public Vector3L(Vector3S xyz)
        {
            this.X = xyz.X;
            this.Y = xyz.Y;
            this.Z = xyz.Z;
        }

        public Vector3L(float x, float y, float z)
        {
            this.X = (long) x;
            this.Y = (long) y;
            this.Z = (long) z;
        }

        public override string ToString() => 
            $"[X:{this.X}, Y:{this.Y}, Z:{this.Z}]";

        public bool Equals(Vector3L other) => 
            ((other.X == this.X) && ((other.Y == this.Y) && (other.Z == this.Z)));

        public override bool Equals(object obj) => 
            ((obj != null) ? (!(obj.GetType() != typeof(Vector3L)) ? this.Equals((Vector3L) obj) : false) : false);

        public override int GetHashCode() => 
            ((int) ((((this.X * 0x18dL) ^ this.Y) * 0x18dL) ^ this.Z));

        public bool IsInsideInclusiveEnd(ref Vector3L min, ref Vector3L max) => 
            ((min.X <= this.X) && ((this.X <= max.X) && ((min.Y <= this.Y) && ((this.Y <= max.Y) && ((min.Z <= this.Z) && (this.Z <= max.Z))))));

        public bool IsInsideInclusiveEnd(Vector3L min, Vector3L max) => 
            this.IsInsideInclusiveEnd(ref min, ref max);

        public bool IsInside(ref Vector3L inclusiveMin, ref Vector3L exclusiveMax) => 
            ((inclusiveMin.X <= this.X) && ((this.X < exclusiveMax.X) && ((inclusiveMin.Y <= this.Y) && ((this.Y < exclusiveMax.Y) && ((inclusiveMin.Z <= this.Z) && (this.Z < exclusiveMax.Z))))));

        public bool IsInside(Vector3L inclusiveMin, Vector3L exclusiveMax) => 
            this.IsInside(ref inclusiveMin, ref exclusiveMax);

        public long RectangularDistance(Vector3L otherVector) => 
            ((Math.Abs((long) (this.X - otherVector.X)) + Math.Abs((long) (this.Y - otherVector.Y))) + Math.Abs((long) (this.Z - otherVector.Z)));

        public long RectangularLength() => 
            ((Math.Abs(this.X) + Math.Abs(this.Y)) + Math.Abs(this.Z));

        public long Length() => 
            ((long) Math.Sqrt((double) Dot(this, this)));

        public static bool Boxlongersects(Vector3L minA, Vector3L maxA, Vector3L minB, Vector3L maxB) => 
            Boxlongersects(ref minA, ref maxA, ref minB, ref maxB);

        public static bool Boxlongersects(ref Vector3L minA, ref Vector3L maxA, ref Vector3L minB, ref Vector3L maxB) => 
            ((maxA.X >= minB.X) && ((minA.X <= maxB.X) && ((maxA.Y >= minB.Y) && ((minA.Y <= maxB.Y) && ((maxA.Z >= minB.Z) && (minA.Z <= maxB.Z))))));

        public static bool BoxContains(Vector3L boxMin, Vector3L boxMax, Vector3L pt) => 
            ((boxMax.X >= pt.X) && ((boxMin.X <= pt.X) && ((boxMax.Y >= pt.Y) && ((boxMin.Y <= pt.Y) && ((boxMax.Z >= pt.Z) && (boxMin.Z <= pt.Z))))));

        public static bool BoxContains(ref Vector3L boxMin, ref Vector3L boxMax, ref Vector3L pt) => 
            ((boxMax.X >= pt.X) && ((boxMin.X <= pt.X) && ((boxMax.Y >= pt.Y) && ((boxMin.Y <= pt.Y) && ((boxMax.Z >= pt.Z) && (boxMin.Z <= pt.Z))))));

        public static Vector3L operator *(Vector3L a, Vector3L b) => 
            new Vector3L(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        public static bool operator ==(Vector3L a, Vector3L b) => 
            ((a.X == b.X) && ((a.Y == b.Y) && (a.Z == b.Z)));

        public static bool operator !=(Vector3L a, Vector3L b) => 
            !(a == b);

        public static Vector3 operator +(Vector3L a, float b) => 
            new Vector3(a.X + b, a.Y + b, a.Z + b);

        public static Vector3 operator *(Vector3L a, Vector3 b) => 
            new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        public static Vector3 operator *(Vector3 a, Vector3L b) => 
            new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        public static Vector3 operator *(float num, Vector3L b) => 
            new Vector3(num * b.X, num * b.Y, num * b.Z);

        public static Vector3 operator *(Vector3L a, float num) => 
            new Vector3(num * a.X, num * a.Y, num * a.Z);

        public static Vector3D operator *(double num, Vector3L b) => 
            new Vector3D(num * b.X, num * b.Y, num * b.Z);

        public static Vector3D operator *(Vector3L a, double num) => 
            new Vector3D(num * a.X, num * a.Y, num * a.Z);

        public static Vector3 operator /(Vector3L a, float num) => 
            new Vector3(((float) a.X) / num, ((float) a.Y) / num, ((float) a.Z) / num);

        public static Vector3 operator /(float num, Vector3L a) => 
            new Vector3(num / ((float) a.X), num / ((float) a.Y), num / ((float) a.Z));

        public static Vector3L operator /(Vector3L a, long num) => 
            new Vector3L(a.X / num, a.Y / num, a.Z / num);

        public static Vector3L operator /(Vector3L a, Vector3L b) => 
            new Vector3L(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public static Vector3L operator %(Vector3L a, long num) => 
            new Vector3L(a.X % num, a.Y % num, a.Z % num);

        public static Vector3L operator >>(Vector3L v, int shift) => 
            new Vector3L(v.X >> (shift & 0x3f), v.Y >> (shift & 0x3f), v.Z >> (shift & 0x3f));

        public static Vector3L operator <<(Vector3L v, int shift) => 
            new Vector3L(v.X << (shift & 0x3f), v.Y << (shift & 0x3f), v.Z << (shift & 0x3f));

        public static Vector3L operator &(Vector3L v, long mask) => 
            new Vector3L(v.X & mask, v.Y & mask, v.Z & mask);

        public static Vector3L operator |(Vector3L v, long mask) => 
            new Vector3L(v.X | mask, v.Y | mask, v.Z | mask);

        public static Vector3L operator ^(Vector3L v, long mask) => 
            new Vector3L(v.X ^ mask, v.Y ^ mask, v.Z ^ mask);

        public static Vector3L operator ~(Vector3L v) => 
            new Vector3L(~v.X, ~v.Y, ~v.Z);

        public static Vector3L operator *(long num, Vector3L b) => 
            new Vector3L(num * b.X, num * b.Y, num * b.Z);

        public static Vector3L operator *(Vector3L a, long num) => 
            new Vector3L(num * a.X, num * a.Y, num * a.Z);

        public static Vector3L operator +(Vector3L a, Vector3L b) => 
            new Vector3L(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3L operator +(Vector3L a, long b) => 
            new Vector3L(a.X + b, a.Y + b, a.Z + b);

        public static Vector3L operator -(Vector3L a, Vector3L b) => 
            new Vector3L(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3L operator -(Vector3L a, long b) => 
            new Vector3L(a.X - b, a.Y - b, a.Z - b);

        public static Vector3L operator -(Vector3L a) => 
            new Vector3L(-a.X, -a.Y, -a.Z);

        public static unsafe Vector3L Min(Vector3L value1, Vector3L value2)
        {
            Vector3L vectorl;
            Vector3L* vectorlPtr1;
            Vector3L* vectorlPtr2;
            Vector3L* vectorlPtr3;
            vectorlPtr1->X = (value1.X < value2.X) ? value1.X : value2.X;
            vectorlPtr1 = (Vector3L*) ref vectorl;
            vectorlPtr2->Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            vectorlPtr2 = (Vector3L*) ref vectorl;
            vectorlPtr3->Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
            vectorlPtr3 = (Vector3L*) ref vectorl;
            return vectorl;
        }

        public static void Min(ref Vector3L value1, ref Vector3L value2, out Vector3L result)
        {
            result.X = (value1.X < value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
        }

        public long AbsMin() => 
            ((Math.Abs(this.X) >= Math.Abs(this.Y)) ? ((Math.Abs(this.Y) >= Math.Abs(this.Z)) ? Math.Abs(this.Z) : Math.Abs(this.Y)) : ((Math.Abs(this.X) >= Math.Abs(this.Z)) ? Math.Abs(this.Z) : Math.Abs(this.X)));

        public static unsafe Vector3L Max(Vector3L value1, Vector3L value2)
        {
            Vector3L vectorl;
            Vector3L* vectorlPtr1;
            Vector3L* vectorlPtr2;
            Vector3L* vectorlPtr3;
            vectorlPtr1->X = (value1.X > value2.X) ? value1.X : value2.X;
            vectorlPtr1 = (Vector3L*) ref vectorl;
            vectorlPtr2->Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            vectorlPtr2 = (Vector3L*) ref vectorl;
            vectorlPtr3->Z = (value1.Z > value2.Z) ? value1.Z : value2.Z;
            vectorlPtr3 = (Vector3L*) ref vectorl;
            return vectorl;
        }

        public static void Max(ref Vector3L value1, ref Vector3L value2, out Vector3L result)
        {
            result.X = (value1.X > value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z > value2.Z) ? value1.Z : value2.Z;
        }

        public long AbsMax() => 
            ((Math.Abs(this.X) <= Math.Abs(this.Y)) ? ((Math.Abs(this.Y) <= Math.Abs(this.Z)) ? Math.Abs(this.Z) : Math.Abs(this.Y)) : ((Math.Abs(this.X) <= Math.Abs(this.Z)) ? Math.Abs(this.Z) : Math.Abs(this.X)));

        public long AxisValue(Base6Directions.Axis axis) => 
            ((axis != Base6Directions.Axis.ForwardBackward) ? ((axis != Base6Directions.Axis.LeftRight) ? this.Y : this.X) : this.Z);

        public static CubeFace GetDominantDirection(Vector3L val) => 
            ((Math.Abs(val.X) <= Math.Abs(val.Y)) ? ((Math.Abs(val.Y) <= Math.Abs(val.Z)) ? ((val.Z <= 0L) ? CubeFace.Forward : CubeFace.Backward) : ((val.Y <= 0L) ? CubeFace.Down : CubeFace.Up)) : ((Math.Abs(val.X) <= Math.Abs(val.Z)) ? ((val.Z <= 0L) ? CubeFace.Forward : CubeFace.Backward) : ((val.X <= 0L) ? CubeFace.Left : CubeFace.Right)));

        public static Vector3L GetDominantDirectionVector(Vector3L val)
        {
            if (Math.Abs(val.X) > Math.Abs(val.Y))
            {
                val.Y = 0L;
                if (Math.Abs(val.X) > Math.Abs(val.Z))
                {
                    val.Z = 0L;
                    val.X = (val.X <= 0L) ? -1L : 1L;
                }
                else
                {
                    val.X = 0L;
                    val.Z = (val.Z <= 0L) ? -1L : 1L;
                }
            }
            else
            {
                val.X = 0L;
                if (Math.Abs(val.Y) > Math.Abs(val.Z))
                {
                    val.Z = 0L;
                    val.Y = (val.Y <= 0L) ? -1L : 1L;
                }
                else
                {
                    val.Y = 0L;
                    val.Z = (val.Z <= 0L) ? -1L : 1L;
                }
            }
            return val;
        }

        public static Vector3L DominantAxisProjection(Vector3L value1)
        {
            if (Math.Abs(value1.X) > Math.Abs(value1.Y))
            {
                value1.Y = 0L;
                if (Math.Abs(value1.X) > Math.Abs(value1.Z))
                {
                    value1.Z = 0L;
                }
                else
                {
                    value1.X = 0L;
                }
            }
            else
            {
                value1.X = 0L;
                if (Math.Abs(value1.Y) > Math.Abs(value1.Z))
                {
                    value1.Z = 0L;
                }
                else
                {
                    value1.Y = 0L;
                }
            }
            return value1;
        }

        public static void DominantAxisProjection(ref Vector3L value1, out Vector3L result)
        {
            if (Math.Abs(value1.X) > Math.Abs(value1.Y))
            {
                if (Math.Abs(value1.X) > Math.Abs(value1.Z))
                {
                    result = new Vector3L(value1.X, 0L, 0L);
                }
                else
                {
                    result = new Vector3L(0L, 0L, value1.Z);
                }
            }
            else if (Math.Abs(value1.Y) > Math.Abs(value1.Z))
            {
                result = new Vector3L(0L, value1.Y, 0L);
            }
            else
            {
                result = new Vector3L(0L, 0L, value1.Z);
            }
        }

        public static Vector3L Sign(Vector3 value) => 
            new Vector3L((long) Math.Sign(value.X), (long) Math.Sign(value.Y), (long) Math.Sign(value.Z));

        public static Vector3L Sign(Vector3L value) => 
            new Vector3L((long) Math.Sign(value.X), (long) Math.Sign(value.Y), (long) Math.Sign(value.Z));

        public static Vector3L Round(Vector3 value)
        {
            Vector3L vectorl;
            Round(ref value, out vectorl);
            return vectorl;
        }

        public static Vector3L Round(Vector3D value)
        {
            Vector3L vectorl;
            Round(ref value, out vectorl);
            return vectorl;
        }

        public static void Round(ref Vector3 v, out Vector3L r)
        {
            r.X = (long) Math.Round((double) v.X, MidpointRounding.AwayFromZero);
            r.Y = (long) Math.Round((double) v.Y, MidpointRounding.AwayFromZero);
            r.Z = (long) Math.Round((double) v.Z, MidpointRounding.AwayFromZero);
        }

        public static void Round(ref Vector3D v, out Vector3L r)
        {
            r.X = (long) Math.Round(v.X, MidpointRounding.AwayFromZero);
            r.Y = (long) Math.Round(v.Y, MidpointRounding.AwayFromZero);
            r.Z = (long) Math.Round(v.Z, MidpointRounding.AwayFromZero);
        }

        public static Vector3L Floor(Vector3 value) => 
            new Vector3L((long) Math.Floor((double) value.X), (long) Math.Floor((double) value.Y), (long) Math.Floor((double) value.Z));

        public static Vector3L Floor(Vector3D value) => 
            new Vector3L((long) Math.Floor(value.X), (long) Math.Floor(value.Y), (long) Math.Floor(value.Z));

        public static void Floor(ref Vector3 v, out Vector3L r)
        {
            r.X = (long) Math.Floor((double) v.X);
            r.Y = (long) Math.Floor((double) v.Y);
            r.Z = (long) Math.Floor((double) v.Z);
        }

        public static void Floor(ref Vector3D v, out Vector3L r)
        {
            r.X = (long) Math.Floor(v.X);
            r.Y = (long) Math.Floor(v.Y);
            r.Z = (long) Math.Floor(v.Z);
        }

        public static Vector3L Ceiling(Vector3 value) => 
            new Vector3L((long) Math.Ceiling((double) value.X), (long) Math.Ceiling((double) value.Y), (long) Math.Ceiling((double) value.Z));

        public static Vector3L Trunc(Vector3 value) => 
            new Vector3L((long) value.X, (long) value.Y, (long) value.Z);

        public static Vector3L Shift(Vector3L value) => 
            new Vector3L(value.Z, value.X, value.Y);

        public static implicit operator Vector3(Vector3L value) => 
            new Vector3((float) value.X, (float) value.Y, (float) value.Z);

        public static implicit operator Vector3D(Vector3L value) => 
            new Vector3D((double) value.X, (double) value.Y, (double) value.Z);

        public static explicit operator Vector3I(Vector3L value) => 
            new Vector3I((int) value.X, (int) value.Y, (int) value.Z);

        public static void Transform(ref Vector3L position, ref Matrix matrix, out Vector3L result)
        {
            long num = (((position.X * ((long) Math.Round((double) matrix.M11))) + (position.Y * ((long) Math.Round((double) matrix.M21)))) + (position.Z * ((long) Math.Round((double) matrix.M31)))) + ((long) Math.Round((double) matrix.M41));
            long num2 = (((position.X * ((long) Math.Round((double) matrix.M12))) + (position.Y * ((long) Math.Round((double) matrix.M22)))) + (position.Z * ((long) Math.Round((double) matrix.M32)))) + ((long) Math.Round((double) matrix.M42));
            long num3 = (((position.X * ((long) Math.Round((double) matrix.M13))) + (position.Y * ((long) Math.Round((double) matrix.M23)))) + (position.Z * ((long) Math.Round((double) matrix.M33)))) + ((long) Math.Round((double) matrix.M43));
            result.X = num;
            result.Y = num2;
            result.Z = num3;
        }

        public static void Transform(ref Vector3L value, ref Quaternion rotation, out Vector3L result)
        {
            float num = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num3 = rotation.Z + rotation.Z;
            float num4 = rotation.W * num;
            float num5 = rotation.W * num2;
            float num6 = rotation.W * num3;
            float num7 = rotation.X * num;
            float num8 = rotation.X * num2;
            float num9 = rotation.X * num3;
            float num10 = rotation.Y * num2;
            float num11 = rotation.Y * num3;
            float num12 = rotation.Z * num3;
            float num13 = (((float) (value.X * ((1.0 - num10) - num12))) + (value.Y * (num8 - num6))) + (value.Z * (num9 + num5));
            float num14 = ((value.X * (num8 + num6)) + ((float) (value.Y * ((1.0 - num7) - num12)))) + (value.Z * (num11 - num4));
            float num15 = ((value.X * (num9 - num5)) + (value.Y * (num11 + num4))) + ((float) (value.Z * ((1.0 - num7) - num10)));
            result.X = (long) Math.Round((double) num13);
            result.Y = (long) Math.Round((double) num14);
            result.Z = (long) Math.Round((double) num15);
        }

        public static Vector3L Transform(Vector3L value, Quaternion rotation)
        {
            Vector3L vectorl;
            Transform(ref value, ref rotation, out vectorl);
            return vectorl;
        }

        public static void TransformNormal(ref Vector3L normal, ref Matrix matrix, out Vector3L result)
        {
            long num = ((normal.X * ((long) Math.Round((double) matrix.M11))) + (normal.Y * ((long) Math.Round((double) matrix.M21)))) + (normal.Z * ((long) Math.Round((double) matrix.M31)));
            long num2 = ((normal.X * ((long) Math.Round((double) matrix.M12))) + (normal.Y * ((long) Math.Round((double) matrix.M22)))) + (normal.Z * ((long) Math.Round((double) matrix.M32)));
            long num3 = ((normal.X * ((long) Math.Round((double) matrix.M13))) + (normal.Y * ((long) Math.Round((double) matrix.M23)))) + (normal.Z * ((long) Math.Round((double) matrix.M33)));
            result.X = num;
            result.Y = num2;
            result.Z = num3;
        }

        public static void Cross(ref Vector3L vector1, ref Vector3L vector2, out Vector3L result)
        {
            long num = (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y);
            long num2 = (vector1.Z * vector2.X) - (vector1.X * vector2.Z);
            long num3 = (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
            result.X = num;
            result.Y = num2;
            result.Z = num3;
        }

        public long Size =>
            Math.Abs((long) ((this.X * this.Y) * this.Z));
        public long SizeLong =>
            Math.Abs((long) ((this.X * this.Y) * this.Z));
        public int CompareTo(Vector3L other)
        {
            long num = this.X - other.X;
            long num2 = this.Y - other.Y;
            long num3 = this.Z - other.Z;
            return ((num != 0) ? Math.Sign(num) : ((num2 != 0) ? Math.Sign(num2) : Math.Sign(num3)));
        }

        public static Vector3L Abs(Vector3L value) => 
            new Vector3L(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));

        public static void Abs(ref Vector3L value, out Vector3L result)
        {
            result.X = Math.Abs(value.X);
            result.Y = Math.Abs(value.Y);
            result.Z = Math.Abs(value.Z);
        }

        public static Vector3L Clamp(Vector3L value1, Vector3L min, Vector3L max)
        {
            Vector3L vectorl;
            Clamp(ref value1, ref min, ref max, out vectorl);
            return vectorl;
        }

        public static void Clamp(ref Vector3L value1, ref Vector3L min, ref Vector3L max, out Vector3L result)
        {
            long x = value1.X;
            long num2 = (x > max.X) ? max.X : x;
            result.X = (num2 < min.X) ? min.X : num2;
            long y = value1.Y;
            long num4 = (y > max.Y) ? max.Y : y;
            result.Y = (num4 < min.Y) ? min.Y : num4;
            long z = value1.Z;
            long num6 = (z > max.Z) ? max.Z : z;
            result.Z = (num6 < min.Z) ? min.Z : num6;
        }

        public static long DistanceManhattan(Vector3L first, Vector3L second)
        {
            Vector3L vectorl = Abs(first - second);
            return ((vectorl.X + vectorl.Y) + vectorl.Z);
        }

        public long Dot(ref Vector3L v) => 
            (((this.X * v.X) + (this.Y * v.Y)) + (this.Z * v.Z));

        public static long Dot(Vector3L vector1, Vector3L vector2) => 
            Dot(ref vector1, ref vector2);

        public static long Dot(ref Vector3L vector1, ref Vector3L vector2) => 
            (((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z));

        public static void Dot(ref Vector3L vector1, ref Vector3L vector2, out long dot)
        {
            dot = ((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z);
        }

        public static bool TryParseFromString(string p, out Vector3L vec)
        {
            char[] separator = new char[] { ';' };
            string[] strArray = p.Split(separator);
            if (strArray.Length != 3)
            {
                vec = Zero;
                return false;
            }
            try
            {
                vec.X = long.Parse(strArray[0]);
                vec.Y = long.Parse(strArray[1]);
                vec.Z = long.Parse(strArray[2]);
            }
            catch (FormatException)
            {
                vec = Zero;
                return false;
            }
            return true;
        }

        public long Volume() => 
            ((this.X * this.Y) * this.Z);

        [IteratorStateMachine(typeof(<EnumerateRange>d__118))]
        public static IEnumerable<Vector3L> EnumerateRange(Vector3L minInclusive, Vector3L maxExclusive)
        {
            <EnumerateRange>d__118 d__1 = new <EnumerateRange>d__118(-2);
            d__1.<>3__minInclusive = minInclusive;
            d__1.<>3__maxExclusive = maxExclusive;
            return d__1;
        }

        public void ToBytes(List<byte> result)
        {
            result.AddRange(BitConverter.GetBytes(this.X));
            result.AddRange(BitConverter.GetBytes(this.Y));
            result.AddRange(BitConverter.GetBytes(this.Z));
        }

        static Vector3L()
        {
            Comparer = new EqualityComparer();
            UnitX = new Vector3L(1L, 0L, 0L);
            UnitY = new Vector3L(0L, 1L, 0L);
            UnitZ = new Vector3L(0L, 0L, 1L);
            Zero = new Vector3L(0L, 0L, 0L);
            MaxValue = new Vector3L(0x7fffffffffffffffL, 0x7fffffffffffffffL, 0x7fffffffffffffffL);
            MinValue = new Vector3L(-9223372036854775808L, -9223372036854775808L, -9223372036854775808L);
            Up = new Vector3L(0L, 1L, 0L);
            Down = new Vector3L(0L, -1L, 0L);
            Right = new Vector3L(1L, 0L, 0L);
            Left = new Vector3L(-1L, 0L, 0L);
            Forward = new Vector3L(0L, 0L, -1L);
            Backward = new Vector3L(0L, 0L, 1L);
            One = new Vector3L(1L, 1L, 1L);
        }
        [CompilerGenerated]
        private sealed class <EnumerateRange>d__118 : IEnumerable<Vector3L>, IEnumerable, IEnumerator<Vector3L>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private Vector3L <>2__current;
            private int <>l__initialThreadId;
            private Vector3L minInclusive;
            public Vector3L <>3__minInclusive;
            private Vector3L maxExclusive;
            public Vector3L <>3__maxExclusive;
            private Vector3L <vec>5__2;

            [DebuggerHidden]
            public <EnumerateRange>d__118(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private unsafe bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<vec>5__2.Z = this.minInclusive.Z;
                    goto TR_0003;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    long* numPtr1 = (long*) ref this.<vec>5__2.X;
                    numPtr1[0] += 1L;
                }
                goto TR_000B;
            TR_0003:
                if (this.<vec>5__2.Z >= this.maxExclusive.Z)
                {
                    return false;
                }
                this.<vec>5__2.Y = this.minInclusive.Y;
            TR_0007:
                while (true)
                {
                    if (this.<vec>5__2.Y < this.maxExclusive.Y)
                    {
                        this.<vec>5__2.X = this.minInclusive.X;
                    }
                    else
                    {
                        long* numPtr3 = (long*) ref this.<vec>5__2.Z;
                        numPtr3[0] += 1L;
                        goto TR_0003;
                    }
                    break;
                }
            TR_000B:
                while (true)
                {
                    if (this.<vec>5__2.X >= this.maxExclusive.X)
                    {
                        long* numPtr2 = (long*) ref this.<vec>5__2.Y;
                        numPtr2[0] += 1L;
                        break;
                    }
                    this.<>2__current = this.<vec>5__2;
                    this.<>1__state = 1;
                    return true;
                }
                goto TR_0007;
            }

            [DebuggerHidden]
            IEnumerator<Vector3L> IEnumerable<Vector3L>.GetEnumerator()
            {
                Vector3L.<EnumerateRange>d__118 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new Vector3L.<EnumerateRange>d__118(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.minInclusive = this.<>3__minInclusive;
                d__.maxExclusive = this.<>3__maxExclusive;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<VRageMath.Vector3L>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            Vector3L IEnumerator<Vector3L>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        public class EqualityComparer : IEqualityComparer<Vector3L>, IComparer<Vector3L>
        {
            public int Compare(Vector3L x, Vector3L y) => 
                x.CompareTo(y);

            public bool Equals(Vector3L x, Vector3L y) => 
                (((x.X == y.X) & (x.Y == y.Y)) & (x.Z == y.Z));

            public int GetHashCode(Vector3L obj) => 
                ((int) ((((obj.X * 0x18dL) ^ obj.Y) * 0x18dL) ^ obj.Z));
        }
    }
}

