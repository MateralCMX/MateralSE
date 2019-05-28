namespace Sandbox.Engine.Multiplayer
{
    using System;
    using VRage;

    public interface ITransportCallback
    {
        void Receive(ByteStream source, ulong sender);
    }
}

