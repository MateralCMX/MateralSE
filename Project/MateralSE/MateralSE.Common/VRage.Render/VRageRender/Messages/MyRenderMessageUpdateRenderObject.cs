namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateRenderObject : MyRenderMessageBase
    {
        public uint ID;
        public int LastMomentUpdateIndex = -1;
        public MyRenderObjectUpdateData Data = new MyRenderObjectUpdateData();

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderObject;
    }
}

