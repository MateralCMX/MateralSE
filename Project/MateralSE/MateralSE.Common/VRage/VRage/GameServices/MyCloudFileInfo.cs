namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCloudFileInfo
    {
        public string Name;
        public int Size;
        public long Timestamp;
        public string LocalPath;
        public MyCloudFileInfo(string name, string localPath, int size, long timestamp)
        {
            this.Name = name;
            this.Size = size;
            this.Timestamp = timestamp;
            this.LocalPath = localPath;
        }
    }
}

