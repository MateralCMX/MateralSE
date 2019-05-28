namespace BulletXNA.BulletCollision
{
    using BulletXNA.LinearMath;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct AABB
    {
        public IndexedVector3 m_min;
        public IndexedVector3 m_max;
        public AABB(ref IndexedVector3 min, ref IndexedVector3 max)
        {
            this.m_min = min;
            this.m_max = max;
        }

        public AABB(IndexedVector3 min, IndexedVector3 max)
        {
            this.m_min = min;
            this.m_max = max;
        }

        public void Invalidate()
        {
            this.m_min.X = float.MaxValue;
            this.m_min.Y = float.MaxValue;
            this.m_min.Z = float.MaxValue;
            this.m_max.X = float.MinValue;
            this.m_max.Y = float.MinValue;
            this.m_max.Z = float.MinValue;
        }

        public void Merge(AABB box)
        {
            this.Merge(ref box);
        }

        public void Merge(ref AABB box)
        {
            this.m_min.X = BoxCollision.BT_MIN(this.m_min.X, box.m_min.X);
            this.m_min.Y = BoxCollision.BT_MIN(this.m_min.Y, box.m_min.Y);
            this.m_min.Z = BoxCollision.BT_MIN(this.m_min.Z, box.m_min.Z);
            this.m_max.X = BoxCollision.BT_MAX(this.m_max.X, box.m_max.X);
            this.m_max.Y = BoxCollision.BT_MAX(this.m_max.Y, box.m_max.Y);
            this.m_max.Z = BoxCollision.BT_MAX(this.m_max.Z, box.m_max.Z);
        }

        public void GetCenterExtend(out IndexedVector3 center, out IndexedVector3 extend)
        {
            center = new IndexedVector3((this.m_max + this.m_min) * 0.5f);
            extend = new IndexedVector3(this.m_max - center);
        }

        public bool CollideRay(ref IndexedVector3 vorigin, ref IndexedVector3 vdir)
        {
            IndexedVector3 vector;
            IndexedVector3 vector2;
            this.GetCenterExtend(out vector2, out vector);
            float x = vorigin.X - vector2.X;
            if (BoxCollision.BT_GREATER(x, vector.X) && ((x * vdir.X) >= 0f))
            {
                return false;
            }
            float num2 = vorigin.Y - vector2.Y;
            if (BoxCollision.BT_GREATER(num2, vector.Y) && ((num2 * vdir.Y) >= 0f))
            {
                return false;
            }
            float num3 = vorigin.Z - vector2.Z;
            if (BoxCollision.BT_GREATER(num3, vector.Z) && ((num3 * vdir.Z) >= 0f))
            {
                return false;
            }
            if (Math.Abs((float) ((vdir.Y * num3) - (vdir.Z * num2))) > ((vector.Y * Math.Abs(vdir.Z)) + (vector.Z * Math.Abs(vdir.Y))))
            {
                return false;
            }
            if (Math.Abs((float) ((vdir.Z * x) - (vdir.X * num3))) > ((vector.X * Math.Abs(vdir.Z)) + (vector.Z * Math.Abs(vdir.X))))
            {
                return false;
            }
            return (Math.Abs((float) ((vdir.X * num2) - (vdir.Y * x))) <= ((vector.X * Math.Abs(vdir.Y)) + (vector.Y * Math.Abs(vdir.X))));
        }

        public float? CollideRayDistance(ref IndexedVector3 origin, ref IndexedVector3 direction)
        {
            float num9;
            IndexedVector3 vector = new IndexedVector3(1f / direction.X, 1f / direction.Y, 1f / direction.Z);
            float num = (this.m_min.X - origin.X) * vector.X;
            float num2 = (this.m_max.X - origin.X) * vector.X;
            float num3 = (this.m_min.Y - origin.Y) * vector.Y;
            float num4 = (this.m_max.Y - origin.Y) * vector.Y;
            float num5 = (this.m_min.Z - origin.Z) * vector.Z;
            float num6 = (this.m_max.Z - origin.Z) * vector.Z;
            float num7 = Math.Max(Math.Max(Math.Min(num, num2), Math.Min(num3, num4)), Math.Min(num5, num6));
            float num8 = Math.Min(Math.Min(Math.Max(num, num2), Math.Max(num3, num4)), Math.Max(num5, num6));
            if (num8 < 0f)
            {
                num9 = num8;
                return null;
            }
            if (num7 <= num8)
            {
                return new float?(num7);
            }
            num9 = num8;
            return null;
        }
    }
}

