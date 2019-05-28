namespace Sandbox.Graphics.GUI
{
    using System;

    [Flags]
    public enum UrlOpenMode
    {
        SteamOverlay = 1,
        ExternalBrowser = 2,
        ConfirmExternal = 4,
        SteamOrExternal = 3,
        SteamOrExternalWithConfirm = 7
    }
}

