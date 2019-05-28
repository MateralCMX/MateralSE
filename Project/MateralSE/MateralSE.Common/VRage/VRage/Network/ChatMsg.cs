namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ChatMsg
    {
        public string Text;
        public ulong Author;
        public byte Channel;
        public long TargetId;
        public string CustomAuthorName;
    }
}

