namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageFileShareResult
    {
        public ulong FileHandle;
        public MyGameServiceCallResult Result;
    }
}

