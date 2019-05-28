namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("36b73880-c2c8-11cf-8b46-00805f6cef60"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaSeeking
    {
        [PreserveSig]
        int GetCapabilities(out SeekingCapabilities pCapabilities);
        [PreserveSig]
        int CheckCapabilities([In, Out] ref SeekingCapabilities pCapabilities);
        [PreserveSig]
        int IsFormatSupported([In] ref Guid pFormat);
        [PreserveSig]
        int QueryPreferredFormat(out Guid pFormat);
        [PreserveSig]
        int GetTimeFormat(out Guid pFormat);
        [PreserveSig]
        int IsUsingTimeFormat([In] ref Guid pFormat);
        [PreserveSig]
        int SetTimeFormat([In] ref Guid pFormat);
        [PreserveSig]
        int GetDuration(out long pDuration);
        [PreserveSig]
        int GetStopPosition(out long pStop);
        [PreserveSig]
        int GetCurrentPosition(out long pCurrent);
        [PreserveSig]
        int ConvertTimeFormat(out long pTarget, [In] ref Guid pTargetFormat, long Source, [In] ref Guid pSourceFormat);
        [PreserveSig]
        int SetPositions([In, Out, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pCurrent, SeekingFlags dwCurrentFlags, [In, Out, MarshalAs(UnmanagedType.LPStruct)] DsOptInt64 pStop, SeekingFlags dwStopFlags);
        [PreserveSig]
        int GetPositions(out long pCurrent, out long pStop);
        [PreserveSig]
        int GetAvailable(out long pEarliest, out long pLatest);
        [PreserveSig]
        int SetRate(double dRate);
        [PreserveSig]
        int GetRate(out double pdRate);
        [PreserveSig]
        int GetPreroll(out long pllPreroll);
    }
}

