namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageAddRuntimeModel : MyRenderMessageBase
    {
        public string Name;
        public string ReplacedModel;
        public MyModelData ModelData = new MyModelData();

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.AddRuntimeModel;
    }
}

