namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void MessageReceivedDelegate(ulong memberId, string message, byte channel, long targetId);
}

