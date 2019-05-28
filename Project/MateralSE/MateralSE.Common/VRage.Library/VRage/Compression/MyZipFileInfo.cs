namespace VRage.Compression
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyZipFileInfo
    {
        internal object m_fileInfo;
        internal MyZipFileInfo(object fileInfo)
        {
            this.m_fileInfo = fileInfo;
        }

        public bool IsValid =>
            (this.m_fileInfo != null);
        public CompressionMethodEnum CompressionMethod =>
            MyZipFileInfoReflection.CompressionMethod(this.m_fileInfo);
        public DeflateOptionEnum DeflateOption =>
            MyZipFileInfoReflection.DeflateOption(this.m_fileInfo);
        public bool FolderFlag =>
            MyZipFileInfoReflection.FolderFlag(this.m_fileInfo);
        public DateTime LastModFileDateTime =>
            MyZipFileInfoReflection.LastModFileDateTime(this.m_fileInfo);
        public string Name =>
            MyZipFileInfoReflection.Name(this.m_fileInfo);
        public bool VolumeLabelFlag =>
            MyZipFileInfoReflection.VolumeLabelFlag(this.m_fileInfo);
        public Stream GetStream(FileMode mode = 3, FileAccess access = 1) => 
            MyZipFileInfoReflection.GetStream(this.m_fileInfo, mode, access);

        public override string ToString() => 
            this.Name;
    }
}

