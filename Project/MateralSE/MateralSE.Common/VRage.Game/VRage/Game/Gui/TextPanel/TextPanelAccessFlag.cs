namespace VRage.Game.GUI.TextPanel
{
    using System;

    [Flags]
    public enum TextPanelAccessFlag : byte
    {
        NONE = 0,
        READ_FACTION = 2,
        WRITE_FACTION = 4,
        READ_AND_WRITE_FACTION = 6,
        READ_ENEMY = 8,
        WRITE_ENEMY = 0x10,
        READ_ALL = 10,
        WRITE_ALL = 20,
        READ_AND_WRITE_ALL = 30
    }
}

