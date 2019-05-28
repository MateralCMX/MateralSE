namespace VRage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Common.Utils;

    public class MyChecksumVerifier : IFileVerifier
    {
        public readonly string BaseChecksumDir;
        public readonly byte[] PublicKey;
        private Dictionary<string, string> m_checksums;
        [CompilerGenerated]
        private Action<IFileVerifier, string> ChecksumNotFound;
        [CompilerGenerated]
        private Action<string, string> ChecksumFailed;

        public event Action<string, string> ChecksumFailed
        {
            [CompilerGenerated] add
            {
                Action<string, string> checksumFailed = this.ChecksumFailed;
                while (true)
                {
                    Action<string, string> a = checksumFailed;
                    Action<string, string> action3 = (Action<string, string>) Delegate.Combine(a, value);
                    checksumFailed = Interlocked.CompareExchange<Action<string, string>>(ref this.ChecksumFailed, action3, a);
                    if (ReferenceEquals(checksumFailed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<string, string> checksumFailed = this.ChecksumFailed;
                while (true)
                {
                    Action<string, string> source = checksumFailed;
                    Action<string, string> action3 = (Action<string, string>) Delegate.Remove(source, value);
                    checksumFailed = Interlocked.CompareExchange<Action<string, string>>(ref this.ChecksumFailed, action3, source);
                    if (ReferenceEquals(checksumFailed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<IFileVerifier, string> ChecksumNotFound
        {
            [CompilerGenerated] add
            {
                Action<IFileVerifier, string> checksumNotFound = this.ChecksumNotFound;
                while (true)
                {
                    Action<IFileVerifier, string> a = checksumNotFound;
                    Action<IFileVerifier, string> action3 = (Action<IFileVerifier, string>) Delegate.Combine(a, value);
                    checksumNotFound = Interlocked.CompareExchange<Action<IFileVerifier, string>>(ref this.ChecksumNotFound, action3, a);
                    if (ReferenceEquals(checksumNotFound, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IFileVerifier, string> checksumNotFound = this.ChecksumNotFound;
                while (true)
                {
                    Action<IFileVerifier, string> source = checksumNotFound;
                    Action<IFileVerifier, string> action3 = (Action<IFileVerifier, string>) Delegate.Remove(source, value);
                    checksumNotFound = Interlocked.CompareExchange<Action<IFileVerifier, string>>(ref this.ChecksumNotFound, action3, source);
                    if (ReferenceEquals(checksumNotFound, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyChecksumVerifier(MyChecksums checksums, string baseChecksumDir)
        {
            this.PublicKey = checksums.PublicKeyAsArray;
            this.BaseChecksumDir = baseChecksumDir;
            this.m_checksums = checksums.Items.Dictionary;
        }

        public Stream Verify(string filename, Stream stream)
        {
            Action<string, string> checksumFailed = this.ChecksumFailed;
            Action<IFileVerifier, string> checksumNotFound = this.ChecksumNotFound;
            if (((checksumFailed != null) || (checksumNotFound != null)) && filename.StartsWith(this.BaseChecksumDir, StringComparison.InvariantCultureIgnoreCase))
            {
                string str2;
                string key = filename.Substring(this.BaseChecksumDir.Length + 1);
                if (this.m_checksums.TryGetValue(key, out str2))
                {
                    if (checksumFailed != null)
                    {
                        return new MyCheckSumStream(stream, filename, Convert.FromBase64String(str2), this.PublicKey, checksumFailed);
                    }
                }
                else if (checksumNotFound != null)
                {
                    checksumNotFound(this, filename);
                }
            }
            return stream;
        }
    }
}

