namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("56a868c0-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaEventEx
    {
        [PreserveSig]
        int GetEventHandle(out IntPtr hEvent);
        [PreserveSig]
        int GetEvent(out DsEvCode lEventCode, out int lParam1, out int lParam2, int msTimeout);
        [PreserveSig]
        int WaitForCompletion(int msTimeout, out int pEvCode);
        [PreserveSig]
        int CancelDefaultHandling(int lEvCode);
        [PreserveSig]
        int RestoreDefaultHandling(int lEvCode);
        [PreserveSig]
        int FreeEventParams(DsEvCode lEvCode, int lParam1, int lParam2);
        [PreserveSig]
        int SetNotifyWindow(IntPtr hwnd, int lMsg, IntPtr lInstanceData);
        [PreserveSig]
        int SetNotifyFlags(int lNoNotifyFlags);
        [PreserveSig]
        int GetNotifyFlags(out int lplNoNotifyFlags);
    }
}

