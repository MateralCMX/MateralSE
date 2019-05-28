namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBasicAudio
    {
        [PreserveSig]
        int put_Volume(int lVolume);
        [PreserveSig]
        int get_Volume(out int plVolume);
        [PreserveSig]
        int put_Balance(int lBalance);
        [PreserveSig]
        int get_Balance(out int plBalance);
    }
}

