namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMyMessageProvider
    {
        MyIGCMessage AcceptMessage();
        void DisableMessageCallback();
        void SetMessageCallback(string argument = "");

        bool HasPendingMessage { get; }

        int MaxWaitingMessages { get; }
    }
}

