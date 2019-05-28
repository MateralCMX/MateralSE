namespace Sandbox.Game.Multiplayer
{
    using System;

    [Flags]
    public enum MyMessagePermissions
    {
        FromServer = 1,
        ToServer = 2
    }
}

