namespace VRage.GameServices
{
    using System;

    public enum MyChatMemberStateChangeEnum
    {
        Entered = 1,
        Left = 2,
        Disconnected = 4,
        Kicked = 8,
        Banned = 0x10
    }
}

