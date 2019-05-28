namespace VRageMath.PackedVector
{
    using System;
    using System.Runtime.CompilerServices;

    public static class HalfUtils
    {
        private const int cFracBits = 10;
        private const int cExpBits = 5;
        private const int cSignBit = 15;
        private const uint cSignMask = 0x8000;
        private const uint cFracMask = 0x3ff;
        private const int cExpBias = 15;
        private const uint cRoundBit = 0x1000;
        private const uint eMax = 0x10;
        private const int eMin = -14;
        private const uint wMaxNormal = 0x47ffefff;
        private const uint wMinNormal = 0x38800000;
        private const uint BiasDiffo = 0xc8000000;
        private const int cFracBitsDiff = 13;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ushort Pack(float value)
        {
            ushort num3;
            uint num1 = *((uint*) &value);
            uint num = (uint) ((num1 & -2147483648) >> 0x10);
            uint num2 = num1 & 0x7fffffff;
            if (num2 > 0x47ffefff)
            {
                num3 = (ushort) (num | 0x7fff);
            }
            else if (num2 >= 0x38800000)
            {
                num3 = (ushort) (num | ((((num2 - 0x38000000) + 0xfff) + ((num2 >> 13) & 1)) >> 13));
            }
            else
            {
                uint num4 = (num2 & 0x7fffff) | 0x800000;
                int num5 = (int) (0x71 - (num2 >> 0x17));
                uint num6 = (num5 > 0x1f) ? 0 : (num4 >> (num5 & 0x1f));
                num3 = (ushort) (num | (((num6 + 0xfff) + ((num6 >> 13) & 1)) >> 13));
            }
            return num3;
        }

        public static unsafe float Unpack(ushort value)
        {
            uint num;
            if ((value & -33792) != 0)
            {
                num = (uint) ((((value & 0x8000) << 0x10) | (((((value >> 10) & 0x1f) - 15) + 0x7f) << 0x17)) | ((value & 0x3ff) << 13));
            }
            else if ((value & 0x3ff) == 0)
            {
                num = (uint) ((value & 0x8000) << 0x10);
            }
            else
            {
                uint num2 = 0xfffffff2;
                uint num3 = (uint) (value & 0x3ff);
                while (true)
                {
                    if ((num3 & 0x400) != 0)
                    {
                        uint num4 = num3 & 0xfffffbff;
                        num = ((uint) (((value & 0x8000) << 0x10) | ((num2 + 0x7f) << 0x17))) | (num4 << 13);
                        break;
                    }
                    num2--;
                    num3 = num3 << 1;
                }
            }
            return *(((float*) &num));
        }
    }
}

