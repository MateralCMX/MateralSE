namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;

    [ProtoContract]
    public class MyContainerGPS
    {
        [ProtoMember(14)]
        public int TimeLeft;
        [ProtoMember(0x11)]
        public string GPSName;

        public MyContainerGPS()
        {
        }

        public MyContainerGPS(int time, string name)
        {
            this.TimeLeft = time;
            this.GPSName = name;
        }
    }
}

