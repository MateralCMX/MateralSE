namespace VRage.Game.Entity.UseObject
{
    using System;

    [Flags]
    public enum UseActionEnum
    {
        None = 0,
        Manipulate = 1,
        OpenTerminal = 2,
        OpenInventory = 4,
        UseFinished = 8,
        Close = 0x10,
        PickUp = 0x20
    }
}

