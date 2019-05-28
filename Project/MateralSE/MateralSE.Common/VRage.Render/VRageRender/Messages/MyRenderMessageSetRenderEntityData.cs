namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetRenderEntityData : MyRenderMessageBase
    {
        public uint ID;
        public MyModelData ModelData = new MyModelData();

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetRenderEntityData;
    }
}

