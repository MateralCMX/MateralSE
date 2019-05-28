namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IMyIntergridCommunicationSystem
    {
        void DisableBroadcastListener(IMyBroadcastListener broadcastListener);
        void GetBroadcastListeners(List<IMyBroadcastListener> broadcastListeners, Func<IMyBroadcastListener, bool> collect = null);
        bool IsEndpointReachable(long address, TransmissionDistance transmissionDistance = 2);
        IMyBroadcastListener RegisterBroadcastListener(string tag);
        void SendBroadcastMessage<TData>(string tag, TData data, TransmissionDistance transmissionDistance = 2);
        bool SendUnicastMessage<TData>(long addressee, string tag, TData data);

        long Me { get; }

        IMyUnicastListener UnicastListener { get; }
    }
}

