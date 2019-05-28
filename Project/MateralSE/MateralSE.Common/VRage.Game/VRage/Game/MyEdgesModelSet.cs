namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using VRage.Data;

    [ProtoContract]
    public class MyEdgesModelSet
    {
        [ProtoMember(10), ModdableContentFile("mwm")]
        public string Vertical;
        [ProtoMember(14), ModdableContentFile("mwm")]
        public string VerticalDiagonal;
        [ProtoMember(0x12), ModdableContentFile("mwm")]
        public string Horisontal;
        [ProtoMember(0x16), ModdableContentFile("mwm")]
        public string HorisontalDiagonal;
    }
}

