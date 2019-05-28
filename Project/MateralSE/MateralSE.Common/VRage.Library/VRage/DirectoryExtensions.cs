namespace VRage
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public static class DirectoryExtensions
    {
        public static void CopyAll(string source, string target)
        {
            EnsureDirectoryExists(target);
            foreach (FileInfo info in new DirectoryInfo(source).GetFiles())
            {
                info.CopyTo(Path.Combine(target, info.Name), true);
            }
            foreach (DirectoryInfo info2 in new DirectoryInfo(source).GetDirectories())
            {
                DirectoryInfo info3 = Directory.CreateDirectory(Path.Combine(target, info2.Name));
                CopyAll(info2.FullName, info3.FullName);
            }
        }

        public static void EnsureDirectoryExists(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Parent != null)
            {
                EnsureDirectoryExists(info.Parent.FullName);
            }
            if (!info.Exists)
            {
                info.Create();
            }
        }

        public static bool IsParentOf(this DirectoryInfo dir, string absPath)
        {
            char[] trimChars = new char[] { Path.DirectorySeparatorChar };
            string str = dir.FullName.TrimEnd(trimChars);
            for (DirectoryInfo info = new DirectoryInfo(absPath); info.Exists; info = info.Parent)
            {
                char[] chArray2 = new char[] { Path.DirectorySeparatorChar };
                if (info.FullName.TrimEnd(chArray2).Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                char[] chArray3 = new char[] { Path.DirectorySeparatorChar };
                if (!info.FullName.TrimEnd(chArray3).StartsWith(str))
                {
                    return false;
                }
                if (info.Parent == null)
                {
                    return false;
                }
            }
            return false;
        }
    }
}

