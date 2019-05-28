namespace VRageRender.Messages
{
    using System;

    [Flags]
    public enum GPUEmitterFlags : uint
    {
        Streaks = 1,
        Collide = 2,
        SleepState = 4,
        Dead = 8,
        Light = 0x10,
        VolumetricLight = 0x20,
        FreezeSimulate = 0x80,
        FreezeEmit = 0x100,
        RandomRotationEnabled = 0x200,
        LocalRotation = 0x400,
        LocalAndCameraRotation = 0x800,
        UseEmissivityChannel = 0x1000,
        UseAlphaAnisotropy = 0x2000
    }
}

