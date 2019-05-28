﻿namespace VRage.GameServices
{
    using System;

    [Flags]
    public enum MyWorkshopItemState
    {
        None = 0,
        Subscribed = 1,
        LegacyItem = 2,
        Installed = 4,
        NeedsUpdate = 8,
        Downloading = 0x10,
        DownloadPending = 0x20
    }
}

