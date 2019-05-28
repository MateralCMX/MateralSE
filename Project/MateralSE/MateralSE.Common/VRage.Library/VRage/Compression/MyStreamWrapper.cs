namespace VRage.Compression
{
    using System;
    using System.IO;

    public class MyStreamWrapper : Stream
    {
        private readonly IDisposable m_obj;
        private readonly Stream m_innerStream;

        public MyStreamWrapper(Stream innerStream, IDisposable objectToClose)
        {
            this.m_innerStream = innerStream;
            this.m_obj = objectToClose;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.m_obj != null))
            {
                this.m_obj.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            this.m_innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) => 
            this.m_innerStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => 
            this.m_innerStream.Seek(offset, origin);

        public override void SetLength(long value)
        {
            this.m_innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead =>
            this.m_innerStream.CanRead;

        public override bool CanSeek =>
            this.m_innerStream.CanSeek;

        public override bool CanWrite =>
            this.m_innerStream.CanWrite;

        public override long Length =>
            this.m_innerStream.Length;

        public override long Position
        {
            get => 
                this.m_innerStream.Position;
            set => 
                (this.m_innerStream.Position = value);
        }
    }
}

