namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageUpdatePublishedFileResult
    {
        public ulong PublishedFileId;
        public MyGameServiceCallResult Result;
    }
}

