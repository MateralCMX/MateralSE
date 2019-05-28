namespace VRageRender.Messages
{
    internal class MyRenderMessageDebugPrintAllFileTexturesIntoLog : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugPrintAllFileTexturesIntoLog;
    }
}

