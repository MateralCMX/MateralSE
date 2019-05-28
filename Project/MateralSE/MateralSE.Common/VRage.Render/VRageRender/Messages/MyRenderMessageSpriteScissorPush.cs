namespace VRageRender.Messages
{
    using VRageMath;

    public class MyRenderMessageSpriteScissorPush : MySpriteDrawRenderMessage
    {
        public Rectangle ScreenRectangle;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SpriteScissorPush;
    }
}

