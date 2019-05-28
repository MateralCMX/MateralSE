namespace VRage.FileSystem
{
    using System;
    using System.IO;

    public class MyNullVerifier : IFileVerifier
    {
        public event Action<string, string> ChecksumFailed
        {
            add
            {
            }
            remove
            {
            }
        }

        public event Action<IFileVerifier, string> ChecksumNotFound
        {
            add
            {
            }
            remove
            {
            }
        }

        public Stream Verify(string filename, Stream stream) => 
            stream;
    }
}

