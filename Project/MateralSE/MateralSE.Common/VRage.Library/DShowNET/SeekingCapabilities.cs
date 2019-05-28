namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [Flags, ComVisible(false)]
    public enum SeekingCapabilities
    {
        CanSeekAbsolute = 1,
        CanSeekForwards = 2,
        CanSeekBackwards = 4,
        CanGetCurrentPos = 8,
        CanGetStopPos = 0x10,
        CanGetDuration = 0x20,
        CanPlayBackwards = 0x40,
        CanDoSegments = 0x80,
        Source = 0x100
    }
}

