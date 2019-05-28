namespace VRage.Utils
{
    using System;

    internal class MyQuantizer
    {
        private int m_quantizationBits;
        private int m_throwawayBits;
        private int m_minValue;
        private byte[] m_smearBits;
        private uint[] m_bitmask;

        public MyQuantizer(int quantizationBits)
        {
            this.m_quantizationBits = quantizationBits;
            this.m_throwawayBits = 8 - this.m_quantizationBits;
            this.m_smearBits = new byte[1 << (this.m_quantizationBits & 0x1f)];
            for (uint i = 0; i < (1 << (this.m_quantizationBits & 0x1f)); i++)
            {
                uint num2 = i << (this.m_throwawayBits & 0x1f);
                num2 += num2 >> (this.m_quantizationBits & 0x1f);
                if (this.m_quantizationBits < 4)
                {
                    num2 += num2 >> ((this.m_quantizationBits * 2) & 0x1f);
                    if (this.m_quantizationBits < 2)
                    {
                        num2 += num2 >> ((this.m_quantizationBits * 4) & 0x1f);
                    }
                }
                this.m_smearBits[i] = (byte) num2;
            }
            this.m_bitmask = new uint[] { (uint) ~(0xff >> (this.m_throwawayBits & 0x1f)), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 1), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 2), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 3), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 4), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 5), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 6), (uint) ~((0xff >> (this.m_throwawayBits & 0x1f)) << 7) };
            this.m_minValue = 1 << (this.m_throwawayBits & 0x1f);
        }

        public int ComputeRequiredPackedSize(int unpackedSize) => 
            ((((unpackedSize * this.m_quantizationBits) + 7) / 8) + 1);

        public int GetMinimumQuantizableValue() => 
            this.m_minValue;

        public byte QuantizeValue(byte val) => 
            this.m_smearBits[val >> (this.m_throwawayBits & 0x1f)];

        public byte ReadVal(byte[] packed, int idx)
        {
            int num = idx * this.m_quantizationBits;
            int index = num >> 3;
            uint num3 = (uint) (packed[index] + (packed[index + 1] << 8));
            return this.m_smearBits[(int) ((IntPtr) ((num3 >> ((num & 7) & 0x1f)) & (0xff >> (this.m_throwawayBits & 0x1f))))];
        }

        public unsafe void SetAllFromUnpacked(byte[] dstPacked, int dstSize, byte[] srcUnpacked)
        {
            Array.Clear(dstPacked, 0, dstPacked.Length);
            int num = 0;
            for (int i = 0; num < (dstSize * this.m_quantizationBits); i++)
            {
                int index = num >> 3;
                uint num4 = (uint) ((srcUnpacked[i] >> (this.m_throwawayBits & 0x1f)) << ((num & 7) & 0x1f));
                byte* numPtr1 = (byte*) ref dstPacked[index];
                numPtr1[0] = (byte) (numPtr1[0] | ((byte) num4));
                byte* numPtr2 = (byte*) ref dstPacked[index + 1];
                numPtr2[0] = (byte) (numPtr2[0] | ((byte) (num4 >> 8)));
                num += this.m_quantizationBits;
            }
        }

        public void WriteVal(byte[] packed, int idx, byte val)
        {
            int num1 = idx * this.m_quantizationBits;
            int index = num1 & 7;
            int num2 = num1 >> 3;
            uint num3 = (uint) ((val >> (this.m_throwawayBits & 0x1f)) << (index & 0x1f));
            packed[num2] = (byte) ((packed[num2] & this.m_bitmask[index]) | num3);
            packed[num2 + 1] = (byte) ((packed[num2 + 1] & (this.m_bitmask[index] >> 8)) | (num3 >> 8));
        }
    }
}

