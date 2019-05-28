namespace Medieval.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;

    [ProtoContract]
    public class TilingSetup
    {
        [ProtoMember(0xaf)]
        public float InitialScale = 2f;
        [ProtoMember(0xb2)]
        public float ScaleMultiplier = 4f;
        [ProtoMember(0xb5)]
        public float InitialDistance = 5f;
        [ProtoMember(0xb8)]
        public float DistanceMultiplier = 4f;
        [ProtoMember(0xbb)]
        public float TilingScale = 32f;
        [ProtoMember(190)]
        public float Far1Distance;
        [ProtoMember(0xc1)]
        public float Far2Distance;
        [ProtoMember(0xc4)]
        public float Far3Distance;
        [ProtoMember(0xc7)]
        public float Far1Scale = 400f;
        [ProtoMember(0xca)]
        public float Far2Scale = 2000f;
        [ProtoMember(0xcd)]
        public float Far3Scale = 7000f;
        [ProtoMember(0xd0)]
        public float ExtDetailScale;
    }
}

