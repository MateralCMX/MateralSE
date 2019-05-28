namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCreateGeneratedTexture : MyRenderMessageBase
    {
        public string TextureName;
        public int Width;
        public int Height;
        public MyGeneratedTextureType Type;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateGeneratedTexture;
    }
}

