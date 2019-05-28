namespace VRage.Filesystem
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.FileSystem;

    internal class MyFileChecksumWatcher : IDisposable
    {
        public MyFileChecksumWatcher()
        {
            this.ChecksumFound = true;
            this.ChecksumValid = true;
            MyFileSystem.FileVerifier.ChecksumFailed += new Action<string, string>(this.FileVerifier_ChecksumFailed);
            MyFileSystem.FileVerifier.ChecksumNotFound += new Action<IFileVerifier, string>(this.FileVerifier_ChecksumNotFound);
        }

        private void FileVerifier_ChecksumFailed(string arg1, string arg2)
        {
            this.ChecksumFound = true;
            this.ChecksumValid = false;
        }

        private void FileVerifier_ChecksumNotFound(IFileVerifier arg1, string arg2)
        {
            this.ChecksumFound = false;
            this.ChecksumValid = false;
        }

        public void Reset()
        {
            this.ChecksumValid = true;
            this.ChecksumFound = true;
        }

        void IDisposable.Dispose()
        {
            MyFileSystem.FileVerifier.ChecksumFailed -= new Action<string, string>(this.FileVerifier_ChecksumFailed);
            MyFileSystem.FileVerifier.ChecksumNotFound -= new Action<IFileVerifier, string>(this.FileVerifier_ChecksumNotFound);
        }

        public bool ChecksumFound { get; private set; }

        public bool ChecksumValid { get; private set; }
    }
}

