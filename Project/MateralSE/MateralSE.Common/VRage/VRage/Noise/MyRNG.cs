namespace VRage.Noise
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRNG
    {
        private const uint MAX_MASK = 0x7fffffff;
        private const float MAX_MASK_FLOAT = 2.147484E+09f;
        public uint Seed;
        public MyRNG(int seed = 1)
        {
            this.Seed = (uint) seed;
        }

        public uint NextInt() => 
            this.Gen();

        public float NextFloat() => 
            (((float) this.Gen()) / 2.147484E+09f);

        public int NextIntRange(float min, float max) => 
            ((int) ((min + ((max - min) * this.NextFloat())) + 0.5f));

        public float NextFloatRange(float min, float max) => 
            (min + ((max - min) * this.NextFloat()));

        private uint Gen()
        {
            uint num;
            this.Seed = num = (this.Seed * 0x41a7) & 0x7fffffff;
            return num;
        }
    }
}

