namespace VRage.Game.Gui
{
    using System;

    [Flags]
    internal enum MyHudDrawElementEnum
    {
        NONE = 0,
        DIRECTION_INDICATORS = 1,
        CROSSHAIR = 2,
        DAMAGE_INDICATORS = 4,
        AMMO = 8,
        HARVEST_MATERIAL = 0x10,
        BARGRAPHS_PLAYER_SHIP = 0x40,
        BARGRAPHS_LARGE_WEAPON = 0x80,
        DIALOGUES = 0x100,
        MISSION_OBJECTIVES = 0x200,
        BACK_CAMERA = 0x400,
        WHEEL_CONTROL = 0x800,
        CROSSHAIR_DYNAMIC = 0x1000
    }
}

