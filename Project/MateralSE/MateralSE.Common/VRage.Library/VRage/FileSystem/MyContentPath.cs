namespace VRage.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyContentPath
    {
        private const string DEFAULT = "";
        private string m_absolutePath;
        private string m_rootFolder;
        private string m_alternatePath;
        public string Path;
        public string ModFolder;
        public string Absolute =>
            this.m_absolutePath;
        public string RootFolder =>
            this.m_rootFolder;
        public string AlternatePath =>
            this.m_alternatePath;
        public bool AbsoluteFileExists =>
            ((this.m_absolutePath != null) && MyFileSystem.FileExists(this.Absolute));
        public bool AbsoluteDirExists =>
            ((this.m_absolutePath != null) && MyFileSystem.DirectoryExists(this.Absolute));
        public bool AlternateFileExists =>
            ((this.m_alternatePath != null) && MyFileSystem.FileExists(this.AlternatePath));
        public bool AlternateDirExists =>
            ((this.m_alternatePath != null) && MyFileSystem.DirectoryExists(this.AlternatePath));
        public MyContentPath(string path = null, string possibleModPath = null)
        {
            this.Path = "";
            this.ModFolder = "";
            this.m_absolutePath = "";
            this.m_rootFolder = "";
            this.m_alternatePath = "";
            this.SetPath(path, possibleModPath);
        }

        public string GetExitingFilePath() => 
            (!this.AbsoluteFileExists ? (!this.AlternateFileExists ? "" : this.AlternatePath) : this.Absolute);

        public void SetPath(string path, string possibleModPath = null)
        {
            this.Path = path;
            this.ModFolder = "";
            this.m_absolutePath = "";
            this.m_rootFolder = "";
            this.m_alternatePath = "";
            if (!string.IsNullOrEmpty(path) && !System.IO.Path.IsPathRooted(path))
            {
                string str = "";
                string str2 = System.IO.Path.Combine(MyFileSystem.ContentPath, path);
                str = (possibleModPath == null) ? System.IO.Path.Combine(MyFileSystem.ModsPath, path) : System.IO.Path.Combine(MyFileSystem.ModsPath, possibleModPath, path);
                if (!MyFileSystem.FileExists(str))
                {
                    this.Path = !MyFileSystem.FileExists(str2) ? (!MyFileSystem.DirectoryExists(str) ? (!MyFileSystem.DirectoryExists(str2) ? "" : str2) : str) : str2;
                }
                else
                {
                    this.Path = str;
                    path = this.Path;
                }
            }
            if (!string.IsNullOrEmpty(this.Path))
            {
                if (this.Path.StartsWith(MyFileSystem.ContentPath))
                {
                    this.Path = (MyFileSystem.ContentPath.Length == this.Path.Length) ? "" : this.Path.Remove(0, MyFileSystem.ContentPath.Length + 1);
                }
                else if (!this.Path.StartsWith(MyFileSystem.ModsPath))
                {
                    this.Path = path;
                }
                else
                {
                    this.Path = this.Path.Remove(0, MyFileSystem.ModsPath.Length + 1);
                    int index = this.Path.IndexOf('\\');
                    if (index == -1)
                    {
                        this.ModFolder = this.Path;
                        this.Path = "";
                        this.SetupHelperPaths();
                        return;
                    }
                    this.ModFolder = this.Path.Substring(0, index);
                    this.Path = this.Path.Remove(0, index + 1);
                }
                this.SetupHelperPaths();
            }
        }

        private void SetupHelperPaths()
        {
            this.m_absolutePath = string.IsNullOrEmpty(this.ModFolder) ? System.IO.Path.Combine(MyFileSystem.ContentPath, this.Path) : System.IO.Path.Combine(MyFileSystem.ModsPath, this.ModFolder, this.Path);
            this.m_rootFolder = string.IsNullOrEmpty(this.ModFolder) ? MyFileSystem.ContentPath : System.IO.Path.Combine(MyFileSystem.ModsPath, this.ModFolder);
            this.m_alternatePath = string.IsNullOrEmpty(this.ModFolder) ? "" : System.IO.Path.Combine(MyFileSystem.ContentPath, this.Path);
        }

        public static implicit operator MyContentPath(string path) => 
            new MyContentPath(path, null);
    }
}

