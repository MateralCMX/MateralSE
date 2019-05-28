namespace ProtoBuf
{
    using System;
    using System.Threading;

    internal class BufferPool
    {
        private const int PoolSize = 20;
        internal const int BufferLength = 0x400;
        private static readonly object[] pool = new object[20];

        private BufferPool()
        {
        }

        internal static void Flush()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                Interlocked.Exchange(ref pool[i], null);
            }
        }

        internal static byte[] GetBuffer()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                object obj2 = Interlocked.Exchange(ref pool[i], null);
                if (obj2 != null)
                {
                    return (byte[]) obj2;
                }
            }
            return new byte[0x400];
        }

        internal static void ReleaseBufferToPool(ref byte[] buffer)
        {
            if (buffer != null)
            {
                if (buffer.Length == 0x400)
                {
                    for (int i = 0; (i < pool.Length) && (Interlocked.CompareExchange(ref pool[i], buffer, null) != null); i++)
                    {
                    }
                }
                buffer = null;
            }
        }

        internal static void ResizeAndFlushLeft(ref byte[] buffer, int toFitAtLeastBytes, int copyFromIndex, int copyBytes)
        {
            int num = buffer.Length * 2;
            if (num < toFitAtLeastBytes)
            {
                num = toFitAtLeastBytes;
            }
            byte[] to = new byte[num];
            if (copyBytes > 0)
            {
                Helpers.BlockCopy(buffer, copyFromIndex, to, 0, copyBytes);
            }
            if (buffer.Length == 0x400)
            {
                ReleaseBufferToPool(ref buffer);
            }
            buffer = to;
        }
    }
}

