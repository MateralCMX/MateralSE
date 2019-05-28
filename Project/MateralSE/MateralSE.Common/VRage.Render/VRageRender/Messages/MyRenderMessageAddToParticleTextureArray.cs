namespace VRageRender.Messages
{
    using System.Collections.Generic;

    public class MyRenderMessageAddToParticleTextureArray : MyRenderMessageBase
    {
        public HashSet<string> Files;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.AddToParticleTextureArray;
    }
}

