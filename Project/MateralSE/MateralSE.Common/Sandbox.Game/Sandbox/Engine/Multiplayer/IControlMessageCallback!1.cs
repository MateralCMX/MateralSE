namespace Sandbox.Engine.Multiplayer
{
    using System;
    using VRage;

    internal interface IControlMessageCallback<TMsg> : ITransportCallback
    {
        void Write(ByteStream destination, ref TMsg msg);
    }
}

