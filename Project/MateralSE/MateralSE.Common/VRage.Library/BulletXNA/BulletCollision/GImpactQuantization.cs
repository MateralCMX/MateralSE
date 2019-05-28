namespace BulletXNA.BulletCollision
{
    using BulletXNA;
    using BulletXNA.LinearMath;
    using System;
    using System.Runtime.InteropServices;

    public class GImpactQuantization
    {
        public static void CalcQuantizationParameters(out IndexedVector3 outMinBound, out IndexedVector3 outMaxBound, out IndexedVector3 bvhQuantization, ref IndexedVector3 srcMinBound, ref IndexedVector3 srcMaxBound, float quantizationMargin)
        {
            IndexedVector3 vector = new IndexedVector3(quantizationMargin);
            outMinBound = srcMinBound - vector;
            outMaxBound = srcMaxBound + vector;
            IndexedVector3 vector2 = outMaxBound - outMinBound;
            bvhQuantization = new IndexedVector3(65535f) / vector2;
        }

        public static void QuantizeClamp(out UShortVector3 output, ref IndexedVector3 point, ref IndexedVector3 min_bound, ref IndexedVector3 max_bound, ref IndexedVector3 bvhQuantization)
        {
            IndexedVector3 vector = point;
            MathUtil.VectorMax(ref min_bound, ref vector);
            MathUtil.VectorMin(ref max_bound, ref vector);
            IndexedVector3 vector2 = (vector - min_bound) * bvhQuantization;
            output = new UShortVector3();
            output[0] = (ushort) (vector2.X + 0.5f);
            output[1] = (ushort) (vector2.Y + 0.5f);
            output[2] = (ushort) (vector2.Z + 0.5f);
        }

        public static IndexedVector3 Unquantize(ref UShortVector3 vecIn, ref IndexedVector3 offset, ref IndexedVector3 bvhQuantization)
        {
            IndexedVector3 vector = new IndexedVector3(((float) vecIn[0]) / bvhQuantization.X, ((float) vecIn[1]) / bvhQuantization.Y, ((float) vecIn[2]) / bvhQuantization.Z);
            return (vector + offset);
        }
    }
}

