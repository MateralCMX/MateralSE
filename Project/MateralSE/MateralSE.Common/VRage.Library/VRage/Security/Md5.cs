namespace VRage.Security
{
    using System;

    public static class Md5
    {
        public static readonly uint[] T = new uint[] { 
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x2441453, 0xd8a1e681, 0xe7d3fbc8, 0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70, 0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x4881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1, 0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
        };

        public static Hash ComputeHash(byte[] input)
        {
            Hash dg = new Hash();
            ComputeHash(input, dg);
            return dg;
        }

        public static unsafe void ComputeHash(byte[] input, Hash dg)
        {
            uint* x = (uint*) stackalloc byte[0x40];
            dg.A = 0x67452301;
            dg.B = 0xefcdab89;
            dg.C = 0x98badcfe;
            dg.D = 0x10325476;
            uint num = (uint) ((input.Length * 8) / 0x20);
            for (uint i = 0; i < (num / 0x10); i++)
            {
                CopyBlock(input, i, x);
                PerformTransformation(ref dg.A, ref dg.B, ref dg.C, ref dg.D, x);
            }
            if ((input.Length % 0x40) < 0x38)
            {
                CopyLastBlock(input, x);
                *((long*) (x + (7 * 8))) = input.Length * 8L;
                PerformTransformation(ref dg.A, ref dg.B, ref dg.C, ref dg.D, x);
            }
            else
            {
                CopyLastBlock(input, x);
                PerformTransformation(ref dg.A, ref dg.B, ref dg.C, ref dg.D, x);
                for (int j = 0; j < 0x10; j++)
                {
                    x[j] = 0;
                }
                *((long*) (x + (7 * 8))) = input.Length * 8L;
                PerformTransformation(ref dg.A, ref dg.B, ref dg.C, ref dg.D, x);
            }
        }

        private static unsafe void CopyBlock(byte[] bMsg, uint block, uint* X)
        {
            block = block << 6;
            for (uint i = 0; i < 0x3d; i += 4)
            {
                X[(int) ((i >> 2) * 4L)] = (uint) ((((bMsg[(int) (block + (i + 3))] << 0x18) | (bMsg[(int) (block + (i + 2))] << 0x10)) | (bMsg[(int) (block + (i + 1))] << 8)) | bMsg[block + i]);
            }
        }

        private static unsafe void CopyLastBlock(byte[] bMsg, uint* X)
        {
            long num2 = (((long) bMsg.Length) / ((long) 0x40)) * 0x40;
            byte* numPtr = (byte*) X;
            int index = 0;
            while (index < (bMsg.Length - num2))
            {
                numPtr[index] = bMsg[(int) ((IntPtr) (num2 + index))];
                index++;
            }
            numPtr[index] = 0x80;
            index++;
            while (index < 0x40)
            {
                numPtr[index] = 0;
                index++;
            }
        }

        private static unsafe void PerformTransformation(ref uint A, ref uint B, ref uint C, ref uint D, uint* X)
        {
            uint num = A;
            uint num2 = B;
            uint num3 = C;
            uint num4 = D;
            TransF(ref A, B, C, D, 0, 7, 1, X);
            TransF(ref D, A, B, C, 1, 12, 2, X);
            TransF(ref C, D, A, B, 2, 0x11, 3, X);
            TransF(ref B, C, D, A, 3, 0x16, 4, X);
            TransF(ref A, B, C, D, 4, 7, 5, X);
            TransF(ref D, A, B, C, 5, 12, 6, X);
            TransF(ref C, D, A, B, 6, 0x11, 7, X);
            TransF(ref B, C, D, A, 7, 0x16, 8, X);
            TransF(ref A, B, C, D, 8, 7, 9, X);
            TransF(ref D, A, B, C, 9, 12, 10, X);
            TransF(ref C, D, A, B, 10, 0x11, 11, X);
            TransF(ref B, C, D, A, 11, 0x16, 12, X);
            TransF(ref A, B, C, D, 12, 7, 13, X);
            TransF(ref D, A, B, C, 13, 12, 14, X);
            TransF(ref C, D, A, B, 14, 0x11, 15, X);
            TransF(ref B, C, D, A, 15, 0x16, 0x10, X);
            TransG(ref A, B, C, D, 1, 5, 0x11, X);
            TransG(ref D, A, B, C, 6, 9, 0x12, X);
            TransG(ref C, D, A, B, 11, 14, 0x13, X);
            TransG(ref B, C, D, A, 0, 20, 20, X);
            TransG(ref A, B, C, D, 5, 5, 0x15, X);
            TransG(ref D, A, B, C, 10, 9, 0x16, X);
            TransG(ref C, D, A, B, 15, 14, 0x17, X);
            TransG(ref B, C, D, A, 4, 20, 0x18, X);
            TransG(ref A, B, C, D, 9, 5, 0x19, X);
            TransG(ref D, A, B, C, 14, 9, 0x1a, X);
            TransG(ref C, D, A, B, 3, 14, 0x1b, X);
            TransG(ref B, C, D, A, 8, 20, 0x1c, X);
            TransG(ref A, B, C, D, 13, 5, 0x1d, X);
            TransG(ref D, A, B, C, 2, 9, 30, X);
            TransG(ref C, D, A, B, 7, 14, 0x1f, X);
            TransG(ref B, C, D, A, 12, 20, 0x20, X);
            TransH(ref A, B, C, D, 5, 4, 0x21, X);
            TransH(ref D, A, B, C, 8, 11, 0x22, X);
            TransH(ref C, D, A, B, 11, 0x10, 0x23, X);
            TransH(ref B, C, D, A, 14, 0x17, 0x24, X);
            TransH(ref A, B, C, D, 1, 4, 0x25, X);
            TransH(ref D, A, B, C, 4, 11, 0x26, X);
            TransH(ref C, D, A, B, 7, 0x10, 0x27, X);
            TransH(ref B, C, D, A, 10, 0x17, 40, X);
            TransH(ref A, B, C, D, 13, 4, 0x29, X);
            TransH(ref D, A, B, C, 0, 11, 0x2a, X);
            TransH(ref C, D, A, B, 3, 0x10, 0x2b, X);
            TransH(ref B, C, D, A, 6, 0x17, 0x2c, X);
            TransH(ref A, B, C, D, 9, 4, 0x2d, X);
            TransH(ref D, A, B, C, 12, 11, 0x2e, X);
            TransH(ref C, D, A, B, 15, 0x10, 0x2f, X);
            TransH(ref B, C, D, A, 2, 0x17, 0x30, X);
            TransI(ref A, B, C, D, 0, 6, 0x31, X);
            TransI(ref D, A, B, C, 7, 10, 50, X);
            TransI(ref C, D, A, B, 14, 15, 0x33, X);
            TransI(ref B, C, D, A, 5, 0x15, 0x34, X);
            TransI(ref A, B, C, D, 12, 6, 0x35, X);
            TransI(ref D, A, B, C, 3, 10, 0x36, X);
            TransI(ref C, D, A, B, 10, 15, 0x37, X);
            TransI(ref B, C, D, A, 1, 0x15, 0x38, X);
            TransI(ref A, B, C, D, 8, 6, 0x39, X);
            TransI(ref D, A, B, C, 15, 10, 0x3a, X);
            TransI(ref C, D, A, B, 6, 15, 0x3b, X);
            TransI(ref B, C, D, A, 13, 0x15, 60, X);
            TransI(ref A, B, C, D, 4, 6, 0x3d, X);
            TransI(ref D, A, B, C, 11, 10, 0x3e, X);
            TransI(ref C, D, A, B, 2, 15, 0x3f, X);
            TransI(ref B, C, D, A, 9, 0x15, 0x40, X);
            A += num;
            B += num2;
            C += num3;
            D += num4;
        }

        private static uint RotateLeft(uint uiNumber, ushort shift) => 
            ((uiNumber >> ((0x20 - shift) & 0x1f)) | (uiNumber << (shift & 0x1f)));

        private static unsafe void TransF(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint* X)
        {
            a = b + RotateLeft(((a + ((b & c) | (~b & d))) + X[(int) (k * 4L)]) + T[((int) i) - 1], s);
        }

        private static unsafe void TransG(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint* X)
        {
            a = b + RotateLeft(((a + ((b & d) | (c & ~d))) + X[(int) (k * 4L)]) + T[((int) i) - 1], s);
        }

        private static unsafe void TransH(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint* X)
        {
            a = b + RotateLeft(((a + ((b ^ c) ^ d)) + X[(int) (k * 4L)]) + T[((int) i) - 1], s);
        }

        private static unsafe void TransI(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i, uint* X)
        {
            a = b + RotateLeft(((a + (c ^ (b | ~d))) + X[(int) (k * 4L)]) + T[((int) i) - 1], s);
        }

        public class Hash
        {
            public uint A;
            public uint B;
            public uint C;
            public uint D;

            public static uint ReverseByte(uint uiNumber) => 
                ((uint) (((((uiNumber & 0xff) << 0x18) | (uiNumber >> 0x18)) | ((uiNumber & 0xff0000) >> 8)) | ((uiNumber & 0xff00) << 8)));

            public string ToLowerString() => 
                (ReverseByte(this.A).ToString("x8") + ReverseByte(this.B).ToString("x8") + ReverseByte(this.C).ToString("x8") + ReverseByte(this.D).ToString("x8"));

            public override string ToString() => 
                (ReverseByte(this.A).ToString("X8") + ReverseByte(this.B).ToString("X8") + ReverseByte(this.C).ToString("X8") + ReverseByte(this.D).ToString("X8"));
        }
    }
}

