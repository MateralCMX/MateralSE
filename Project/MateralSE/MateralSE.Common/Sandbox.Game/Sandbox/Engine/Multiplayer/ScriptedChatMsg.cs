namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ScriptedChatMsg
    {
        public string Text;
        public string Author;
        public long Target;
        public string Font;
    }
}

