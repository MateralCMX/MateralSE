namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageCreateManualCullObject : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public MatrixD WorldMatrix;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateManualCullObject;
    }
}

