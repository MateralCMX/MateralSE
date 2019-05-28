namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library;

    [Serializable]
    public class MyRandom
    {
        [ThreadStatic]
        private static MyRandom m_instance;
        private int inext;
        private int inextp;
        private const int MBIG = 0x7fffffff;
        private const int MSEED = 0x9a4ec86;
        private const int MZ = 0;
        private int[] SeedArray;
        private byte[] m_tmpLongArray;
        internal static bool EnableDeterminism;

        public MyRandom() : this(MyEnvironment.TickCount + Thread.CurrentThread.ManagedThreadId)
        {
        }

        public MyRandom(int Seed)
        {
            this.m_tmpLongArray = new byte[8];
            this.SeedArray = new int[0x38];
            this.SetSeed(Seed);
        }

        public int CreateRandomSeed() => 
            (MyEnvironment.TickCount ^ this.Next());

        public float GetRandomFloat(float minValue, float maxValue) => 
            ((this.NextFloat() * (maxValue - minValue)) + minValue);

        public float GetRandomSign() => 
            ((float) Math.Sign((float) (((float) this.NextDouble()) - 0.5f)));

        private double GetSampleForLargeRange()
        {
            int num = this.InternalSample();
            if ((this.InternalSample() % 2) == 0)
            {
                num = -num;
            }
            return ((num + 2147483646.0) / 4294967293);
        }

        public unsafe void GetState(out State state)
        {
            state.Inext = this.inext;
            state.Inextp = this.inextp;
            int* numPtr = &state.Seed.FixedElementField;
            Marshal.Copy(this.SeedArray, 0, new IntPtr((void*) numPtr), 0x38);
            fixed (int* numRef = null)
            {
                return;
            }
        }

        private int InternalSample()
        {
            int inextp = this.inextp;
            int index = this.inext + 1;
            if (index >= 0x38)
            {
                index = 1;
            }
            if (++inextp >= 0x38)
            {
                inextp = 1;
            }
            int num3 = this.SeedArray[index] - this.SeedArray[inextp];
            if (num3 == 0x7fffffff)
            {
                num3--;
            }
            if (num3 < 0)
            {
                num3 += 0x7fffffff;
            }
            this.SeedArray[index] = num3;
            this.inext = index;
            this.inextp = inextp;
            return num3;
        }

        public int Next() => 
            this.InternalSample();

        public int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue");
            }
            return (int) (this.Sample() * maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue");
            }
            long num = maxValue - minValue;
            return ((num > 0x7fffffffL) ? (((int) ((long) (this.GetSampleForLargeRange() * num))) + minValue) : (((int) (this.Sample() * num)) + minValue));
        }

        public void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) (this.InternalSample() % 0x100);
            }
        }

        public double NextDouble() => 
            this.Sample();

        public float NextFloat() => 
            ((float) this.NextDouble());

        public long NextLong()
        {
            this.NextBytes(this.m_tmpLongArray);
            return BitConverter.ToInt64(this.m_tmpLongArray, 0);
        }

        public StateToken PushSeed(int newSeed) => 
            new StateToken(this, newSeed);

        protected double Sample() => 
            (this.InternalSample() * 4.6566128752457969E-10);

        public unsafe void SetSeed(int Seed)
        {
            int num1;
            int num2 = 0x9a4ec86 - ((Seed == -2147483648) ? 0x7fffffff : num1);
            this.SeedArray[0x37] = num2;
            int num3 = 1;
            for (int i = 1; i < 0x37; i++)
            {
                int index = (0x15 * i) % 0x37;
                this.SeedArray[index] = num3;
                num3 = num2 - num3;
                if (num3 < 0)
                {
                    num3 += 0x7fffffff;
                }
                num2 = this.SeedArray[index];
            }
            int num6 = 1;
            while (num6 < 5)
            {
                int index = 1;
                while (true)
                {
                    if (index >= 0x38)
                    {
                        num6++;
                        break;
                    }
                    int* numPtr1 = (int*) ref this.SeedArray[index];
                    numPtr1[0] -= this.SeedArray[1 + ((index + 30) % 0x37)];
                    if (this.SeedArray[index] < 0)
                    {
                        int* numPtr2 = (int*) ref this.SeedArray[index];
                        numPtr2[0] += 0x7fffffff;
                    }
                    index++;
                }
            }
            this.inext = 0;
            this.inextp = 0x15;
            num1 = Math.Abs(Seed);
            Seed = 1;
        }

        public unsafe void SetState(ref State state)
        {
            this.inext = state.Inext;
            this.inextp = state.Inextp;
            Marshal.Copy(new IntPtr((void*) &state.Seed.FixedElementField), this.SeedArray, 0, 0x38);
            fixed (int* numRef = null)
            {
                return;
            }
        }

        public static MyRandom Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyRandom();
                }
                return m_instance;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct State
        {
            public int Inext;
            public int Inextp;
            [FixedBuffer(typeof(int), 0x38)]
            public <Seed>e__FixedBuffer Seed;
            [StructLayout(LayoutKind.Sequential, Size=0xe0), CompilerGenerated, UnsafeValueType]
            public struct <Seed>e__FixedBuffer
            {
                public int FixedElementField;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StateToken : IDisposable
        {
            private MyRandom m_random;
            private MyRandom.State m_state;
            public StateToken(MyRandom random)
            {
                this.m_random = random;
                random.GetState(out this.m_state);
            }

            public StateToken(MyRandom random, int newSeed)
            {
                this.m_random = random;
                random.GetState(out this.m_state);
                random.SetSeed(newSeed);
            }

            public void Dispose()
            {
                if (this.m_random != null)
                {
                    this.m_random.SetState(ref this.m_state);
                }
            }
        }
    }
}

