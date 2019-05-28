namespace VRage.Library.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct BitReaderWriter
    {
        private IBitSerializable m_writeData;
        private BitStream m_readStream;
        private int m_readStreamPosition;
        public readonly bool IsReading;
        public BitReaderWriter(IBitSerializable writeData)
        {
            this.m_writeData = writeData;
            this.m_readStream = null;
            this.m_readStreamPosition = 0;
            this.IsReading = false;
        }

        private BitReaderWriter(BitStream readStream, int readPos)
        {
            this.m_writeData = null;
            this.m_readStream = readStream;
            this.m_readStreamPosition = readPos;
            this.IsReading = true;
        }

        public static BitReaderWriter ReadFrom(BitStream stream)
        {
            uint num = stream.ReadUInt32Variant();
            BitReaderWriter writer = new BitReaderWriter(stream, stream.BitPosition);
            stream.SetBitPositionRead(stream.BitPosition + ((int) num));
            return writer;
        }

        public void Write(BitStream stream)
        {
            if ((stream == null) || (this.m_writeData == null))
            {
                BitStream stream1 = stream;
                IBitSerializable writeData = this.m_writeData;
            }
            else
            {
                int bitPosition = stream.BitPosition;
                this.m_writeData.Serialize(stream, false, true);
                int num2 = stream.BitPosition - bitPosition;
                stream.SetBitPositionWrite(bitPosition);
                stream.WriteVariant((uint) num2);
                this.m_writeData.Serialize(stream, false, true);
            }
        }

        public bool ReadData(IBitSerializable readDataInto, bool validate, bool acceptAndSetValue = true)
        {
            bool flag;
            int bitPosition = this.m_readStream.BitPosition;
            this.m_readStream.SetBitPositionRead(this.m_readStreamPosition);
            try
            {
                flag = readDataInto.Serialize(this.m_readStream, validate, acceptAndSetValue);
            }
            finally
            {
                this.m_readStream.SetBitPositionRead(bitPosition);
            }
            return flag;
        }
    }
}

