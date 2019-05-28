namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageResetGeneratedTexture : MyRenderMessageBase
    {
        public string TextureName;
        public byte[] Data;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ResetGeneratedTexture;
    }
}

