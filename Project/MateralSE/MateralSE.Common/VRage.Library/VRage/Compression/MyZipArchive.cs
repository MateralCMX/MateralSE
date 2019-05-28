namespace VRage.Compression
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyZipArchive : IDisposable
    {
        private object m_zip;
        private Dictionary<string, string> m_mixedCaseHelper = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private MyZipArchive(object zipObject, string path = null)
        {
            this.m_zip = zipObject;
            this.ZipPath = path;
            foreach (MyZipFileInfo info in this.Files)
            {
                this.m_mixedCaseHelper[info.Name.Replace('/', '\\')] = info.Name;
            }
        }

        public MyZipFileInfo AddFile(string path, CompressionMethodEnum compressionMethod = 8, DeflateOptionEnum deflateOption = 0) => 
            new MyZipFileInfo(MyZipArchiveReflection.AddFile(this.m_zip, path, (ushort) compressionMethod, (byte) deflateOption));

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, DeflateOptionEnum compressionLevel, bool includeBaseDirectory, string[] ignoredExtensions = null, bool includeHidden = true)
        {
            if (File.Exists(destinationArchiveFileName))
            {
                File.Delete(destinationArchiveFileName);
            }
            int startIndex = sourceDirectoryName.Length + 1;
            using (MyZipArchive archive = OpenOnFile(destinationArchiveFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, false))
            {
                DirectoryInfo relativeTo = new DirectoryInfo(sourceDirectoryName);
                foreach (FileInfo info2 in relativeTo.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (includeHidden || !IsHidden(info2, relativeTo))
                    {
                        string fullName = info2.FullName;
                        if ((ignoredExtensions == null) || !ignoredExtensions.Contains<string>(Path.GetExtension(fullName), StringComparer.InvariantCultureIgnoreCase))
                        {
                            using (FileStream stream = File.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                using (Stream stream2 = archive.AddFile(fullName.Substring(startIndex), CompressionMethodEnum.Deflated, compressionLevel).GetStream(FileMode.Open, FileAccess.Write))
                                {
                                    stream.CopyTo(stream2, 0x1000);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DeleteFile(string name)
        {
            FixName(ref name);
            MyZipArchiveReflection.DeleteFile(this.m_zip, name);
        }

        public bool DirectoryExists(string name)
        {
            FixName(ref name);
            using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = this.m_mixedCaseHelper.Keys.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    string current = enumerator.Current;
                    if (current.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            ((IDisposable) this.m_zip).Dispose();
        }

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            if (!Directory.Exists(destinationDirectoryName))
            {
                Directory.CreateDirectory(destinationDirectoryName);
            }
            using (MyZipArchive archive = OpenOnFile(sourceArchiveFileName, FileMode.Open, FileAccess.Read, FileShare.Read, false))
            {
                foreach (string str in archive.FileNames)
                {
                    Stream stream = archive.GetFile(str).GetStream(FileMode.Open, FileAccess.Read);
                    try
                    {
                        string path = Path.Combine(destinationDirectoryName, str);
                        string directoryName = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                        using (FileStream stream2 = File.Open(path, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(stream2, 0x1000);
                        }
                    }
                    finally
                    {
                        if (stream == null)
                        {
                            continue;
                        }
                        stream.Dispose();
                    }
                }
            }
        }

        public bool FileExists(string name)
        {
            FixName(ref name);
            return this.m_mixedCaseHelper.ContainsKey(name);
        }

        private static void FixName(ref string name)
        {
            name = name.Replace('/', '\\');
        }

        public MyZipFileInfo GetFile(string name)
        {
            FixName(ref name);
            return new MyZipFileInfo(MyZipArchiveReflection.GetFile(this.m_zip, this.m_mixedCaseHelper[name]));
        }

        public static bool IsHidden(FileInfo f, DirectoryInfo relativeTo)
        {
            if ((f.Attributes & FileAttributes.Hidden) != 0)
            {
                return true;
            }
            for (DirectoryInfo info = f.Directory; !info.FullName.Equals(relativeTo.FullName, StringComparison.InvariantCultureIgnoreCase); info = info.Parent)
            {
                if ((info.Attributes & FileAttributes.Hidden) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static MyZipArchive OpenOnFile(string path, FileMode mode = 3, FileAccess access = 1, FileShare share = 1, bool streaming = false) => 
            new MyZipArchive(MyZipArchiveReflection.OpenOnFile(path, mode, access, share, streaming), path);

        public static MyZipArchive OpenOnStream(Stream stream, FileMode mode = 4, FileAccess access = 3, bool streaming = false) => 
            new MyZipArchive(MyZipArchiveReflection.OpenOnStream(stream, mode, access, streaming), null);

        public string ZipPath { get; private set; }

        public IEnumerable<string> FileNames =>
            (from p in this.Files
                select p.Name into p
                orderby p
                select p);

        public Enumerator Files =>
            new Enumerator(((IEnumerable) MyZipArchiveReflection.GetFiles(this.m_zip)).GetEnumerator());

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyZipArchive.<>c <>9 = new MyZipArchive.<>c();
            public static Func<MyZipFileInfo, string> <>9__9_0;
            public static Func<string, string> <>9__9_1;

            internal string <get_FileNames>b__9_0(MyZipFileInfo p) => 
                p.Name;

            internal string <get_FileNames>b__9_1(string p) => 
                p;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<MyZipFileInfo>, IDisposable, IEnumerator, IEnumerable<MyZipFileInfo>, IEnumerable
        {
            public IEnumerator m_enumerator;
            public Enumerator(IEnumerator enumerator)
            {
                this.m_enumerator = enumerator;
            }

            public MyZipFileInfo Current =>
                new MyZipFileInfo(this.m_enumerator.Current);
            public bool MoveNext() => 
                this.m_enumerator.MoveNext();

            public void Reset()
            {
                this.m_enumerator.Reset();
            }

            object IEnumerator.Current =>
                this.Current;
            void IDisposable.Dispose()
            {
            }

            public IEnumerator<MyZipFileInfo> GetEnumerator() => 
                this;

            IEnumerator IEnumerable.GetEnumerator() => 
                this.GetEnumerator();
        }
    }
}

