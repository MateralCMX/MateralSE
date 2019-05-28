namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateGameplayFrame : MyRenderMessageBase
    {
        public int GameplayFrame;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateGameplayFrame;
    }
}

