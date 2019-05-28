namespace VRageRender.Messages
{
    using System;
    using VRage;

    public class MyRenderMessageUpdateRenderInstanceBufferRange : MyRenderMessageBase
    {
        public uint ID;
        public MyInstanceData[] InstanceData;
        public int StartOffset;
        public bool Trim;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderInstanceBufferRange;
    }
}

