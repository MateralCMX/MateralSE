namespace VRage.Common.Utils
{
    using System;
    using System.IO;

    internal class MyCheckSumStream : Stream
    {
        private MyRSA m_verifier;
        private Stream m_stream;
        private string m_filename;
        private byte[] m_signedHash;
        private byte[] m_publicKey;
        private Action<string, string> m_failHandler;
        private long m_lastPosition;
        private byte[] m_tmpArray = new byte[1];

        internal MyCheckSumStream(Stream stream, string filename, byte[] signedHash, byte[] publicKey, Action<string, string> failHandler)
        {
            this.m_stream = stream;
            this.m_verifier = new MyRSA();
            this.m_signedHash = signedHash;
            this.m_publicKey = publicKey;
            this.m_filename = filename;
            this.m_failHandler = failHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_verifier.HashObject.TransformFinalBlock(new byte[0], 0, 0);
                if (!this.m_verifier.VerifyHash(this.m_verifier.HashObject.Hash, this.m_signedHash, this.m_publicKey))
                {
                    this.m_failHandler(this.m_filename, Convert.ToBase64String(this.m_verifier.HashObject.Hash));
                }
                this.m_stream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            this.m_stream.Flush();
        }

        public override int Read(byte[] array, int offset, int count)
        {
            int num = (int) (this.m_lastPosition - this.m_stream.Position);
            int num2 = this.m_stream.Read(array, offset, count);
            int num3 = offset + num;
            if (((num2 - num) > 0) && (num3 > 0))
            {
                this.m_verifier.HashObject.TransformBlock(array, offset + num, num2 - num, null, 0);
            }
            this.m_lastPosition = this.m_stream.Position;
            return num2;
        }

        public override int ReadByte() => 
            ((this.Read(this.m_tmpArray, 0, 1) != 0) ? this.m_tmpArray[0] : -1);

        public override long Seek(long offset, SeekOrigin origin) => 
            this.m_stream.Seek(offset, origin);

        public override void SetLength(long value)
        {
            this.m_stream.SetLength(value);
        }

        public override void Write(byte[] array, int offset, int count)
        {
            this.m_stream.Write(array, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.m_stream.WriteByte(value);
        }

        public override bool CanRead =>
            this.m_stream.CanRead;

        public override bool CanSeek =>
            this.m_stream.CanSeek;

        public override bool CanWrite =>
            this.m_stream.CanWrite;

        public override long Length =>
            this.m_stream.Length;

        public override long Position
        {
            get => 
                this.m_stream.Position;
            set => 
                (this.m_stream.Position = value);
        }
    }
}

