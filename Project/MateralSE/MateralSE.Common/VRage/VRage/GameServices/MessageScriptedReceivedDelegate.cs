namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void MessageScriptedReceivedDelegate(ulong memberId, string message, byte channel, long targetId, string author);
}

