namespace DShowNET
{
    using DirectShowLib;
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaSample
    {
        [PreserveSig]
        int GetPointer(out IntPtr ppBuffer);
        [PreserveSig]
        int GetSize();
        [PreserveSig]
        int GetTime(out long pTimeStart, out long pTimeEnd);
        [PreserveSig]
        int SetTime([In, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pTimeStart, [In, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pTimeEnd);
        [PreserveSig]
        int IsSyncPoint();
        [PreserveSig]
        int SetSyncPoint([In, MarshalAs(UnmanagedType.Bool)] bool bIsSyncPoint);
        [PreserveSig]
        int IsPreroll();
        [PreserveSig]
        int SetPreroll([In, MarshalAs(UnmanagedType.Bool)] bool bIsPreroll);
        [PreserveSig]
        int GetActualDataLength();
        [PreserveSig]
        int SetActualDataLength(int len);
        [PreserveSig]
        int GetMediaType([MarshalAs(UnmanagedType.LPStruct)] out AMMediaType ppMediaType);
        [PreserveSig]
        int SetMediaType([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pMediaType);
        [PreserveSig]
        int IsDiscontinuity();
        [PreserveSig]
        int SetDiscontinuity([In, MarshalAs(UnmanagedType.Bool)] bool bDiscontinuity);
        [PreserveSig]
        int GetMediaTime(out long pTimeStart, out long pTimeEnd);
        [PreserveSig]
        int SetMediaTime([In, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pTimeStart, [In, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pTimeEnd);
    }
}

