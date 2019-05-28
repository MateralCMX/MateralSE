namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyControlKickClientMsg
    {
        public ulong KickedClient;
        public BoolBlit Kicked;
        public BoolBlit Add;
    }
}

