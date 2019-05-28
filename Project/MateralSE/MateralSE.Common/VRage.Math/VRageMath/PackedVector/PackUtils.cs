namespace VRageMath.PackedVector
{
    using System;

    internal static class PackUtils
    {
        private static double ClampAndRound(float value, float min, float max) => 
            (!float.IsNaN(value) ? (!float.IsInfinity(value) ? ((value >= min) ? ((value <= max) ? Math.Round((double) value) : ((double) max)) : ((double) min)) : (float.IsNegativeInfinity(value) ? ((double) min) : ((double) max))) : 0.0);

        public static uint PackSigned(uint bitmask, float value)
        {
            float max = bitmask >> 1;
            float min = (float) (-((double) max) - 1.0);
            return (((uint) ((int) ClampAndRound(value, min, max))) & bitmask);
        }

        public static uint PackSNorm(uint bitmask, float value)
        {
            float max = bitmask >> 1;
            value *= max;
            return (((uint) ((int) ClampAndRound(value, -max, max))) & bitmask);
        }

        public static uint PackUNorm(float bitmask, float value)
        {
            value *= bitmask;
            return (uint) ClampAndRound(value, 0f, bitmask);
        }

        public static uint PackUnsigned(float bitmask, float value) => 
            ((uint) ClampAndRound(value, 0f, bitmask));

        public static float UnpackSNorm(uint bitmask, uint value)
        {
            uint num = (uint) ((bitmask + 1) >> 1);
            if ((value & num) == 0)
            {
                value &= bitmask;
            }
            else
            {
                if ((value & bitmask) == num)
                {
                    return -1f;
                }
                value |= ~bitmask;
            }
            float num2 = bitmask >> 1;
            return (((float) value) / num2);
        }

        public static float UnpackUNorm(uint bitmask, uint value)
        {
            value &= bitmask;
            return (((float) value) / ((float) bitmask));
        }
    }
}

