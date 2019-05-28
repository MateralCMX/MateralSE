namespace BulletXNA.BulletCollision
{
    using System;

    internal static class BoxCollision
    {
        public static bool BT_GREATER(float x, float y) => 
            (Math.Abs(x) > y);

        public static float BT_MAX(float a, float b) => 
            Math.Max(a, b);

        public static float BT_MIN(float a, float b) => 
            Math.Min(a, b);
    }
}

