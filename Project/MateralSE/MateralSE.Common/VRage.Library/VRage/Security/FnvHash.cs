namespace VRage.Security
{
    using System;

    public static class FnvHash
    {
        private const uint InitialFNV = 0x811c9dc5;
        private const uint FNVMultiple = 0x1000193;

        public static uint Compute(string s)
        {
            uint num = 0x811c9dc5;
            for (int i = 0; i < s.Length; i++)
            {
                num = (num ^ s[i]) * 0x1000193;
            }
            return num;
        }

        public static uint ComputeAscii(string s)
        {
            uint num = 0x811c9dc5;
            for (int i = 0; i < s.Length; i++)
            {
                num = (num ^ ((byte) s[i])) * 0x1000193;
            }
            return num;
        }
    }
}

