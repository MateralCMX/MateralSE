namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageSubscribePublishedFileResult
    {
        public ulong PublishedFileId;
        public MyGameServiceCallResult Result;
    }
}

