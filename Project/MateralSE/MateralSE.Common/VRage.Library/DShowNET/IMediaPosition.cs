namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaPosition
    {
        [PreserveSig]
        int get_Duration(out double pLength);
        [PreserveSig]
        int put_CurrentPosition(double llTime);
        [PreserveSig]
        int get_CurrentPosition(out double pllTime);
        [PreserveSig]
        int get_StopTime(out double pllTime);
        [PreserveSig]
        int put_StopTime(double llTime);
        [PreserveSig]
        int get_PrerollTime(out double pllTime);
        [PreserveSig]
        int put_PrerollTime(double llTime);
        [PreserveSig]
        int put_Rate(double dRate);
        [PreserveSig]
        int get_Rate(out double pdRate);
        [PreserveSig]
        int CanSeekForward(out int pCanSeekForward);
        [PreserveSig]
        int CanSeekBackward(out int pCanSeekBackward);
    }
}

