namespace DShowNET
{
    using DirectShowLib;
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISampleGrabber
    {
        [PreserveSig]
        int SetOneShot([In, MarshalAs(UnmanagedType.Bool)] bool OneShot);
        [PreserveSig]
        int SetMediaType([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
        [PreserveSig]
        int GetConnectedMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
        [PreserveSig]
        int SetBufferSamples([In, MarshalAs(UnmanagedType.Bool)] bool BufferThem);
        [PreserveSig]
        int GetCurrentBuffer(ref int pBufferSize, IntPtr pBuffer);
        [PreserveSig]
        int GetCurrentSample(IntPtr ppSample);
        [PreserveSig]
        int SetCallback(DShowNET.ISampleGrabberCB pCallback, int WhichMethodToCallback);
    }
}

