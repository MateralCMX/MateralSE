namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCreateRenderInstanceBuffer : MyRenderMessageBase
    {
        public uint ID;
        public uint ParentID;
        public string DebugName;
        public MyRenderInstanceBufferType Type;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderInstanceBuffer;
    }
}

