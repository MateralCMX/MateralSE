namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetVisibilityUpdates : MyRenderMessageBase
    {
        public uint ID;
        public bool State;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetVisibilityUpdates;
    }
}

