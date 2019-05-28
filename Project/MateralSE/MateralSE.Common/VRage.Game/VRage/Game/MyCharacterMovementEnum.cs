namespace VRage.Game
{
    using System;

    public enum MyCharacterMovementEnum : ushort
    {
        Standing = 0,
        Sitting = 1,
        Crouching = 2,
        Flying = 3,
        Falling = 4,
        Jump = 5,
        Died = 6,
        Ladder = 7,
        LadderOut = 0x4000,
        RotatingLeft = 0x1000,
        RotatingRight = 0x2000,
        Walking = 0x10,
        BackWalking = 0x20,
        WalkStrafingLeft = 0x40,
        WalkStrafingRight = 0x80,
        WalkingRightFront = 0x90,
        WalkingRightBack = 160,
        WalkingLeftFront = 80,
        WalkingLeftBack = 0x60,
        Running = 0x410,
        Backrunning = 0x420,
        RunStrafingLeft = 0x440,
        RunStrafingRight = 0x480,
        RunningRightFront = 0x490,
        RunningRightBack = 0x4a0,
        RunningLeftFront = 0x450,
        RunningLeftBack = 0x460,
        CrouchWalking = 0x12,
        CrouchBackWalking = 0x22,
        CrouchStrafingLeft = 0x42,
        CrouchStrafingRight = 130,
        CrouchWalkingRightFront = 0x92,
        CrouchWalkingRightBack = 0xa2,
        CrouchWalkingLeftFront = 0x52,
        CrouchWalkingLeftBack = 0x62,
        CrouchRotatingLeft = 0x1002,
        CrouchRotatingRight = 0x2002,
        Sprinting = 0x810,
        LadderUp = 0x107,
        LadderDown = 0x207
    }
}

