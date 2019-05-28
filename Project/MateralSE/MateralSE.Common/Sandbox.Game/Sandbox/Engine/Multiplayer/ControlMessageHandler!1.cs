namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void ControlMessageHandler<T>(ref T message, ulong sender) where T: struct;
}

