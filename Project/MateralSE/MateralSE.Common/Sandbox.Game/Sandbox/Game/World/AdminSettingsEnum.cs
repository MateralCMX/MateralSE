namespace Sandbox.Game.World
{
    using System;

    [Flags]
    public enum AdminSettingsEnum
    {
        None = 0,
        Invulnerable = 1,
        ShowPlayers = 2,
        UseTerminals = 4,
        Untargetable = 8,
        KeepOriginalOwnershipOnPaste = 0x10,
        AdminOnly = 13
    }
}

