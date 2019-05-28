namespace VRageRender.Messages
{
    public class MyRenderMessageDrawScene : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.Draw;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawScene;
    }
}

