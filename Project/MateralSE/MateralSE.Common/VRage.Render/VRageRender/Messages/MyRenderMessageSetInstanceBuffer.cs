namespace VRageRender.Messages
{
    using System;
    using VRage;
    using VRageMath;

    public class MyRenderMessageSetInstanceBuffer : MyRenderMessageBase
    {
        public uint ID;
        public uint InstanceBufferId;
        public int InstanceStart;
        public int InstanceCount;
        public MyInstanceData[] InstanceData;
        public BoundingBox LocalAabb;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetInstanceBuffer;
    }
}

