namespace VRage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class MyFileSystem
    {
        public static readonly Assembly MainAssembly;
        public static readonly string MainAssemblyName;
        public static string ExePath;
        private static string m_shadersBasePath;
        private static string m_contentPath;
        private static string m_modsPath;
        private static string m_userDataPath;
        private static string m_savesPath;
        public static IFileVerifier FileVerifier;
        private static MyFileProviderAggregator m_fileProvider;

        static MyFileSystem()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            MainAssembly = entryAssembly ?? Assembly.GetCallingAssembly();
            MainAssemblyName = MainAssembly.GetName().Name;
            ExePath = new FileInfo(MainAssembly.Location).DirectoryName;
            FileVerifier = new MyNullVerifier();
            IFileProvider[] providers = new IFileProvider[] { new MyClassicFileProvider(), new MyZipFileProvider() };
            m_fileProvider = new MyFileProviderAggregator(providers);
        }

        public static bool CheckFileWriteAccess(string path)
        {
            bool flag;
            try
            {
                using (OpenWrite(path, FileMode.Append))
                {
                    flag = true;
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        private static void CheckInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Paths are not initialized, call 'Init'");
            }
        }

        private static void CheckUserSpecificInitialized()
        {
            if (m_userDataPath == null)
            {
                throw new InvalidOperationException("User specific path not initialized, call 'InitUserSpecific'");
            }
        }

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

        public static void CopyAll(string source, string target, Predicate<string> condition)
        {
            EnsureDirectoryExists(target);
            foreach (FileInfo info in new DirectoryInfo(source).GetFiles())
            {
                if (condition(info.FullName))
                {
                    info.CopyTo(Path.Combine(target, info.Name), true);
                }
            }
            foreach (DirectoryInfo info2 in new DirectoryInfo(source).GetDirectories())
            {
                if (condition(info2.FullName))
                {
                    DirectoryInfo info3 = Directory.CreateDirectory(Path.Combine(target, info2.Name));
                    CopyAll(info2.FullName, info3.FullName, condition);
                }
            }
        }

        public static void CreateDirectoryRecursive(string path)
        {
            if (!string.IsNullOrEmpty(path) && !DirectoryExists(path))
            {
                CreateDirectoryRecursive(Path.GetDirectoryName(path));
                Directory.CreateDirectory(path);
            }
        }

        public static bool DirectoryExists(string path) => 
            m_fileProvider.DirectoryExists(path);

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

        public static bool FileExists(string path) => 
            m_fileProvider.FileExists(path);

        public static IEnumerable<string> GetFiles(string path) => 
            m_fileProvider.GetFiles(path, "*", MySearchOption.AllDirectories);

        public static IEnumerable<string> GetFiles(string path, string filter) => 
            m_fileProvider.GetFiles(path, filter, MySearchOption.AllDirectories);

        public static IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption) => 
            m_fileProvider.GetFiles(path, filter, searchOption);

        public static void Init(string contentPath, string userData, string modDirName = "Mods", string shadersBasePath = null)
        {
            if (m_contentPath != null)
            {
                throw new InvalidOperationException("Paths already initialized");
            }
            m_contentPath = Path.GetFullPath(contentPath);
            m_shadersBasePath = string.IsNullOrEmpty(shadersBasePath) ? m_contentPath : Path.GetFullPath(shadersBasePath);
            m_userDataPath = Path.GetFullPath(userData);
            m_modsPath = Path.Combine(m_userDataPath, modDirName);
            Directory.CreateDirectory(m_modsPath);
        }

        public static void InitUserSpecific(string userSpecificName, string saveDirName = "Saves")
        {
            CheckInitialized();
            if (m_savesPath != null)
            {
                throw new InvalidOperationException("User specific paths already initialized");
            }
            m_savesPath = Path.Combine(m_userDataPath, saveDirName, userSpecificName ?? string.Empty);
            Directory.CreateDirectory(m_savesPath);
        }

        public static bool IsDirectory(string path)
        {
            if (!DirectoryExists(path))
            {
                return false;
            }
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }
            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }
            Uri uri = new Uri(fromPath);
            Uri uri2 = new Uri(toPath);
            if (uri.Scheme != uri2.Scheme)
            {
                return toPath;
            }
            string str = Uri.UnescapeDataString(uri.MakeRelativeUri(uri2).ToString());
            if (uri2.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                str = str.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return str;
        }

        public static Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            Stream stream = m_fileProvider.Open(path, mode, access, share);
            if (((mode != FileMode.Open) || (access == FileAccess.Write)) || (stream == null))
            {
                return stream;
            }
            return FileVerifier.Verify(path, stream);
        }

        public static Stream OpenRead(string path) => 
            Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        public static Stream OpenRead(string path, string subpath) => 
            OpenRead(Path.Combine(path, subpath));

        public static Stream OpenWrite(string path, FileMode mode = 2)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return File.Open(path, mode, FileAccess.Write, FileShare.Read);
        }

        public static Stream OpenWrite(string path, string subpath, FileMode mode = 2) => 
            OpenWrite(Path.Combine(path, subpath), mode);

        public static void Reset()
        {
            m_contentPath = m_shadersBasePath = m_modsPath = m_userDataPath = (string) (m_savesPath = null);
        }

        public static string TerminatePath(string path)
        {
            if (string.IsNullOrEmpty(path) || (path[path.Length - 1] != Path.DirectorySeparatorChar))
            {
                return (path + Path.DirectorySeparatorChar.ToString());
            }
            return path;
        }

        public static string ShadersBasePath
        {
            get
            {
                CheckInitialized();
                return m_shadersBasePath;
            }
        }

        public static string ContentPath
        {
            get
            {
                CheckInitialized();
                return m_contentPath;
            }
        }

        public static string ModsPath
        {
            get
            {
                CheckInitialized();
                return m_modsPath;
            }
        }

        public static string UserDataPath
        {
            get
            {
                CheckInitialized();
                return m_userDataPath;
            }
        }

        public static string SavesPath
        {
            get
            {
                CheckUserSpecificInitialized();
                return m_savesPath;
            }
        }

        public static bool IsInitialized =>
            (m_contentPath != null);
    }
}

