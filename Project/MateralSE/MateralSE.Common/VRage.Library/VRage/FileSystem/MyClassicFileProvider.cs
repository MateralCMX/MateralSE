namespace VRage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class MyClassicFileProvider : IFileProvider
    {
        public bool DirectoryExists(string path) => 
            Directory.Exists(path);

        public bool FileExists(string path) => 
            File.Exists(path);

        public IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption) => 
            (Directory.Exists(path) ? ((IEnumerable<string>) Directory.GetFiles(path, filter, (SearchOption) searchOption)) : null);

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                return File.Open(path, mode, access, share);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

