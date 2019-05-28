namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageDrawStringAligned : MyRenderMessageDrawString
    {
        public int TextureWidthInPx;
        public MyRenderTextAlignmentEnum Alignment;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawStringAligned;
    }
}

