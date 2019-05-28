namespace VRage.Library.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class PathUtils
    {
        public static void GetfGetFilesRecursively(string path, string searchPath, List<string> paths)
        {
            paths.AddRange(Directory.GetFiles(path, searchPath));
            foreach (string str in Directory.GetDirectories(path))
            {
                GetfGetFilesRecursively(str, searchPath, paths);
            }
        }

        public static string[] GetFilesRecursively(string path, string searchPath)
        {
            List<string> paths = new List<string>();
            GetfGetFilesRecursively(path, searchPath, paths);
            return paths.ToArray();
        }
    }
}

