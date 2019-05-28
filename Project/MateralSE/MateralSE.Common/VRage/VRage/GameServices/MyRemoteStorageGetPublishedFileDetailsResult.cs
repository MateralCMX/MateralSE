namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageGetPublishedFileDetailsResult
    {
        public bool AcceptedForUse;
        public bool Banned;
        public uint ConsumerAppID;
        public uint CreatorAppID;
        public string Description;
        public ulong FileHandle;
        public string FileName;
        public int FileSize;
        public ulong PreviewFileHandle;
        public int PreviewFileSize;
        public ulong PublishedFileId;
        public MyGameServiceCallResult Result;
        public ulong SteamIDOwner;
        public string Tags;
        public bool TagsTruncated;
        public uint TimeCreated;
        public uint TimeUpdated;
        public string Title;
        public string URL;
    }
}

