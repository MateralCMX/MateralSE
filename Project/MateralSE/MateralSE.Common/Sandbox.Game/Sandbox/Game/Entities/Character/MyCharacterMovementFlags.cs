namespace Sandbox.Game.Entities.Character
{
    using System;

    [Flags]
    public enum MyCharacterMovementFlags : byte
    {
        Jump = 1,
        Sprint = 2,
        FlyUp = 4,
        FlyDown = 8,
        Crouch = 0x10,
        Walk = 0x20
    }
}

