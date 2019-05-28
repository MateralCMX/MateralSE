namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void MyLobbyChatUpdated(IMyLobby lobby, ulong changedUserId, ulong makingChangeUserId, MyChatMemberStateChangeEnum stateChange);
}

