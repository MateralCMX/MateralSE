namespace Sandbox.Engine.Multiplayer
{
    using System;

    public enum MyControlMessageEnum : byte
    {
        Kick = 0,
        Disconnected = 1,
        Ban = 2,
        SendPasswordHash = 3
    }
}

