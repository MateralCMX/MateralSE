namespace VRage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IFileProvider
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption);
        Stream Open(string path, FileMode mode, FileAccess access, FileShare share);
    }
}

