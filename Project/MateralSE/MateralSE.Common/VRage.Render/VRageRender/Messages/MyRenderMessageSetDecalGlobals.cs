namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageSetDecalGlobals : MyRenderMessageBase
    {
        public MyDecalGlobals Globals;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetDecalGlobals;
    }
}

