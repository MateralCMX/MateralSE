namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetParentCullObject : MyRenderMessageBase
    {
        public uint ID;
        public uint CullObjectID;
        public Matrix? ChildToParent;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetParentCullObject;
    }
}

