namespace VRage
{
    using System;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;

    public abstract class MyPacket
    {
        public VRage.Library.Collections.BitStream BitStream;
        public VRage.ByteStream ByteStream;
        public Endpoint Sender;
        public MyTimeSpan ReceivedTime;

        protected MyPacket()
        {
        }

        public abstract void Return();
    }
}

