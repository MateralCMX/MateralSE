namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCreateLineBasedObject : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public string ColorMetalTexture;
        public string NormalGlossTexture;
        public string ExtensionTexture;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateLineBasedObject;
    }
}

