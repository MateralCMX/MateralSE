namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUnloadTexture : MyRenderMessageBase
    {
        public string Texture;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UnloadTexture;
    }
}

