namespace VRage
{
    using System;

    public interface IPacketData
    {
        void Return();

        byte[] Data { get; }

        IntPtr Ptr { get; }

        int Size { get; }

        int Offset { get; }
    }
}

