namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using VRage;
    using VRage.Security;

    public class MyKeyHasher
    {
        public List<MyKeys> Keys = new List<MyKeys>(10);
        public VRage.Security.Md5.Hash Hash = new VRage.Security.Md5.Hash();
        private SHA256 m_hasher = MySHA256.Create();
        private byte[] m_tmpHashData = new byte[0x100];

        public void ComputeHash(string salt)
        {
            this.Keys.Sort(EnumComparer<MyKeys>.Instance);
            int index = 0;
            foreach (MyKeys keys in this.Keys)
            {
                index++;
                this.m_tmpHashData[index] = (byte) keys;
            }
            foreach (char ch in salt)
            {
                index++;
                this.m_tmpHashData[index] = (byte) ch;
                index++;
                this.m_tmpHashData[index] = (byte) (ch >> 8);
            }
            Md5.ComputeHash(this.m_tmpHashData, this.Hash);
        }

        private static byte HexToByte(char c) => 
            ((c < 'a') ? ((c < 'A') ? ((byte) (c - '0')) : ((byte) (('\n' + c) - 0x41))) : ((byte) (('\n' + c) - 0x61)));

        private static byte HexToByte(char c1, char c2) => 
            ((byte) ((HexToByte(c1) * 0x10) + HexToByte(c2)));

        public unsafe bool TestHash(string hash, string salt)
        {
            uint* numPtr = (uint*) stackalloc byte[0x10];
            for (int i = 0; i < (Math.Min(hash.Length, 0x20) / 2); i++)
            {
                *((sbyte*) (numPtr + i)) = HexToByte(hash[i * 2], hash[(i * 2) + 1]);
            }
            return this.TestHash(numPtr[0], numPtr[1], numPtr[2], numPtr[3], salt);
        }

        public bool TestHash(uint h0, uint h1, uint h2, uint h3, string salt)
        {
            this.ComputeHash(salt);
            return ((this.Hash.A == h0) && ((this.Hash.B == h1) && ((this.Hash.C == h2) && (this.Hash.D == h3))));
        }
    }
}

