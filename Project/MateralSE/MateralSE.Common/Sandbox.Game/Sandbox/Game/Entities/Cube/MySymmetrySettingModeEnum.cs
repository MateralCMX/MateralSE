namespace Sandbox.Game.Entities.Cube
{
    using System;

    [Flags]
    public enum MySymmetrySettingModeEnum
    {
        Disabled = 0,
        NoPlane = 1,
        XPlane = 2,
        XPlaneOdd = 4,
        YPlane = 8,
        YPlaneOdd = 0x10,
        ZPlane = 0x20,
        ZPlaneOdd = 0x40
    }
}

