namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyControlDisconnectedMsg
    {
        public ulong Client;
    }
}

