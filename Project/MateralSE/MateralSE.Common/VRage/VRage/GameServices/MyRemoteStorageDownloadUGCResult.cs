namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageDownloadUGCResult
    {
        public uint AppID;
        public ulong FileHandle;
        public string FileName;
        public MyGameServiceCallResult Result;
        public int SizeInBytes;
        public ulong SteamIDOwner;
    }
}

