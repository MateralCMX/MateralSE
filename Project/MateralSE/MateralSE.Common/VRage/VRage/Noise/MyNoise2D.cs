namespace VRage.Noise
{
    using System;
    using VRageMath;

    public static class MyNoise2D
    {
        private static MyRNG m_rnd = new MyRNG();
        private const int B = 0x100;
        private const int BM = 0xff;
        private static float[] rand = new float[0x100];
        private static int[] perm = new int[0x200];

        public static float Billow(float x, float y, int numLayers)
        {
            int num = 1;
            float num2 = 1f;
            float num3 = 0f;
            float num4 = 0f;
            for (int i = 0; i < numLayers; i++)
            {
                num3 += num2;
                num4 += Math.Abs((float) ((2f * Noise(x * num, y * num)) - 1f)) * num2;
                num2 *= 0.5f;
                num = num << 1;
            }
            return (num4 / num3);
        }

        public static float FBM(float x, float y, int numLayers, float lacunarity, float gain)
        {
            float num = 1f;
            float num2 = 1f;
            float num3 = 0f;
            float num4 = 0f;
            for (int i = 0; i < numLayers; i++)
            {
                num3 += num2;
                num4 += Noise(x * num, y * num) * num2;
                num2 *= gain;
                num *= lacunarity;
            }
            return (num4 / num3);
        }

        public static float Fractal(float x, float y, int numOctaves)
        {
            int num = 1;
            float num2 = 1f;
            float num3 = 0f;
            float num4 = 0f;
            for (int i = 0; i < numOctaves; i++)
            {
                num3 += num2;
                num4 += Noise(x * num, y * num) * num2;
                num2 *= 0.5f;
                num = num << 1;
            }
            return (num4 / num3);
        }

        public static void Init(int seed)
        {
            m_rnd.Seed = (uint) seed;
            for (int i = 0; i < 0x100; i++)
            {
                rand[i] = m_rnd.NextFloat();
                perm[i] = i;
            }
            for (int j = 0; j < 0x100; j++)
            {
                int index = ((int) m_rnd.NextInt()) & 0xff;
                int num4 = perm[index];
                perm[index] = perm[j];
                perm[j] = num4;
                perm[j + 0x100] = perm[j];
            }
        }

        public static float Marble(float x, float y, int numOctaves) => 
            ((((float) Math.Sin((double) (4f * (x + Fractal(x * 0.5f, y * 0.5f, numOctaves))))) + 1f) * 0.5f);

        public static float Noise(float x, float y)
        {
            int num = (int) x;
            int num2 = (int) y;
            float amount = x - num;
            float num4 = y - num2;
            int index = 0xff & num;
            int num6 = 0xff & (num + 1);
            int num7 = 0xff & num2;
            int num8 = 0xff & (num2 + 1);
            float num10 = rand[perm[perm[index] + num8]];
            return MathHelper.SmoothStep(MathHelper.SmoothStep(rand[perm[perm[index] + num7]], rand[perm[perm[num6] + num7]], amount), MathHelper.SmoothStep(num10, rand[perm[perm[num6] + num8]], amount), num4);
        }

        public static float Rotation(float x, float y, int numLayers)
        {
            float[] numArray = new float[numLayers];
            float[] numArray2 = new float[numLayers];
            for (int i = 0; i < numLayers; i++)
            {
                numArray[i] = (float) Math.Sin((double) (0.4363323f * i));
                numArray2[i] = (float) Math.Cos((double) (0.4363323f * i));
            }
            float num = 0f;
            int num2 = 0;
            for (int j = 0; j < numLayers; j++)
            {
                num += Noise((x * numArray2[j]) - (y * numArray[j]), (x * numArray[j]) + (y * numArray2[j]));
                num2++;
            }
            return (num / ((float) num2));
        }

        public static float Wood(float x, float y, float scale)
        {
            float single1 = Noise(x, y) * scale;
            return (single1 - ((int) single1));
        }
    }
}

