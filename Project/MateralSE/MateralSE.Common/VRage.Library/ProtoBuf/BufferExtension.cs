namespace ProtoBuf
{
    using System;
    using System.IO;

    public sealed class BufferExtension : IExtension
    {
        private byte[] buffer;

        Stream IExtension.BeginAppend() => 
            new MemoryStream();

        Stream IExtension.BeginQuery() => 
            ((this.buffer == null) ? Stream.Null : new MemoryStream(this.buffer));

        void IExtension.EndAppend(Stream stream, bool commit)
        {
            using (stream)
            {
                int num;
                if (commit && ((num = (int) stream.Length) > 0))
                {
                    MemoryStream stream3 = (MemoryStream) stream;
                    if (this.buffer == null)
                    {
                        this.buffer = stream3.ToArray();
                    }
                    else
                    {
                        int length = this.buffer.Length;
                        byte[] to = new byte[length + num];
                        Helpers.BlockCopy(this.buffer, 0, to, 0, length);
                        Helpers.BlockCopy(stream3.GetBuffer(), 0, to, length, num);
                        this.buffer = to;
                    }
                }
            }
        }

        void IExtension.EndQuery(Stream stream)
        {
            using (stream)
            {
            }
        }

        int IExtension.GetLength() => 
            ((this.buffer == null) ? 0 : this.buffer.Length);
    }
}

