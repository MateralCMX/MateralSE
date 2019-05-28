namespace VRage.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public interface IFileVerifier
    {
        event Action<string, string> ChecksumFailed;

        event Action<IFileVerifier, string> ChecksumNotFound;

        Stream Verify(string filename, Stream stream);
    }
}

