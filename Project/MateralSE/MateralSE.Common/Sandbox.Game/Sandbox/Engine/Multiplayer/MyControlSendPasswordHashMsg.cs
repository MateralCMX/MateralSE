namespace Sandbox.Engine.Multiplayer
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyControlSendPasswordHashMsg
    {
        [ProtoMember(0x4c), Serialize(MyObjectFlags.DefaultZero)]
        public byte[] PasswordHash;
    }
}

