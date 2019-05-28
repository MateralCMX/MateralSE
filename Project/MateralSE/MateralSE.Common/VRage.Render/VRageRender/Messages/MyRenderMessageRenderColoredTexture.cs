namespace VRageRender.Messages
{
    using System.Collections.Generic;

    public class MyRenderMessageRenderColoredTexture : MyRenderMessageBase
    {
        public List<renderColoredTextureProperties> texturesToRender;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RenderColoredTexture;
    }
}

