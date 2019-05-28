namespace Sandbox.Game.Screens.Helpers
{
    using System;

    [Flags]
    public enum MySupportKeysEnum : byte
    {
        NONE = 0,
        CTRL = 1,
        ALT = 2,
        SHIFT = 4,
        CTRL_ALT = 3,
        CTRL_SHIFT = 5,
        ALT_SHIFT = 6,
        CTRL_ALT_SHIFT = 7
    }
}

