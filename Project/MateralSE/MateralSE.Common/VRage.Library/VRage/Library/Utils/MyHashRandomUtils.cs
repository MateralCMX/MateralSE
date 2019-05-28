namespace VRage.Library.Utils
{
    using System;
    using System.IO;
    using System.Text;

    public static class MyHashRandomUtils
    {
        public static unsafe float CreateFloatFromMantissa(uint m)
        {
            m &= 0x7fffff;
            m |= 0x3f800000;
            return (*(((float*) &m)) - 1f);
        }

        public static uint JenkinsHash(uint x)
        {
            x += x << 10;
            x ^= x >> 6;
            x += x << 3;
            x ^= x >> 11;
            x += x << 15;
            return x;
        }

        public static void TestHashSample()
        {
            float[] numArray = new float[0x5f5e100];
            using (new MySimpleTestTimer("Int to sample fast"))
            {
                for (int k = 0; k < 0x5f5e100; k++)
                {
                    numArray[k] = UniformFloatFromSeed(k);
                }
            }
            float num = 0f;
            float maxValue = float.MaxValue;
            float minValue = float.MinValue;
            for (int i = 0; i < 0x5f5e100; i++)
            {
                num += numArray[i];
                if (maxValue > numArray[i])
                {
                    maxValue = numArray[i];
                }
                if (minValue < numArray[i])
                {
                    minValue = numArray[i];
                }
            }
            num /= 1E+08f;
            float num4 = 0f;
            for (int j = 0; j < 0x5f5e100; j++)
            {
                float num8 = numArray[j] - num;
                num4 += num8 * num8;
            }
            num4 = ((float) Math.Sqrt((double) num4)) / 1E+08f;
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Min/Max/Avg: {0}/{1}/{2}\n", maxValue, minValue, num);
            builder.AppendFormat("Std dev: {0}\n", num4);
            File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "perf.log"), builder.ToString());
        }

        public static float UniformFloatFromSeed(int seed) => 
            CreateFloatFromMantissa(JenkinsHash((uint) seed));
    }
}

