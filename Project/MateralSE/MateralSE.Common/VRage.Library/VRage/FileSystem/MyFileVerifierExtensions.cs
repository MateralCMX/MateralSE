namespace VRage.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public static class MyFileVerifierExtensions
    {
        public static Stream Verify(this IFileVerifier verifier, string path, Stream stream) => 
            verifier.Verify(path, stream);
    }
}

