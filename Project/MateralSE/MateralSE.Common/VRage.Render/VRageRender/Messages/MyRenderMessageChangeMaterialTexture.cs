namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageChangeMaterialTexture : MyRenderMessageBase
    {
        public uint RenderObjectID;
        public Dictionary<string, MyTextureChange> Changes;

        public override void Close()
        {
            this.Changes = null;
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ChangeMaterialTexture;
    }
}

