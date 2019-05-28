namespace VRageRender.Animations
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;

    [ProtoContract]
    public class Generation2DProperty
    {
        [ProtoMember(0x3f)]
        public List<AnimationKey> Keys;
    }
}

