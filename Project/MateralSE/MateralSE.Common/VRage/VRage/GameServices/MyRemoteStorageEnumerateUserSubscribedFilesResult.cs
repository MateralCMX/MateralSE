namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRemoteStorageEnumerateUserSubscribedFilesResult
    {
        public MyGameServiceCallResult Result;
        public int ResultsReturned;
        public int TotalResultCount;
        public List<ulong> FileIds;
        public ulong this[int i] =>
            this.FileIds[i];
    }
}

