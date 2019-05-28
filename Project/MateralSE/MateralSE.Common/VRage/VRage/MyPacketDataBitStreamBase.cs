namespace VRage
{
    using System;
    using VRage.Library.Collections;

    public abstract class MyPacketDataBitStreamBase : IPacketData
    {
        private readonly BitStream m_stream = new BitStream(0x600);
        protected bool m_returned;

        protected MyPacketDataBitStreamBase()
        {
            this.m_stream.ResetWrite();
        }

        public abstract void Return();

        public BitStream Stream =>
            this.m_stream;

        public abstract byte[] Data { get; }

        public abstract IntPtr Ptr { get; }

        public abstract int Size { get; }

        public abstract int Offset { get; }
    }
}

