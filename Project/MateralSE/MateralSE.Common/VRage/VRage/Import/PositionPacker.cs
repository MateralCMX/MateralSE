namespace VRage.Import
{
    using System;
    using VRageMath;
    using VRageMath.PackedVector;

    public static class PositionPacker
    {
        public static HalfVector4 PackPosition(ref Vector3 position)
        {
            float w = Math.Min((float) Math.Floor((double) Math.Max(Math.Max(Math.Abs(position.X), Math.Abs(position.Y)), Math.Abs(position.Z))), 2048f);
            float num2 = 0f;
            if (w > 0f)
            {
                num2 = 1f / w;
            }
            else
            {
                w = num2 = 1f;
            }
            return new HalfVector4(num2 * position.X, num2 * position.Y, num2 * position.Z, w);
        }

        public static Vector3 UnpackPosition(ref HalfVector4 position)
        {
            Vector4 vector = position.ToVector4();
            return (Vector3) (vector.W * new Vector3(vector.X, vector.Y, vector.Z));
        }
    }
}

