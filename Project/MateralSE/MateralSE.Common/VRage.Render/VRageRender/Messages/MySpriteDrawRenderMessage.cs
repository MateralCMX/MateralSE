namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class MySpriteDrawRenderMessage : MyRenderMessageBase
    {
        protected MySpriteDrawRenderMessage()
        {
        }

        public string TargetTexture { get; set; }

        public override MyRenderMessageType MessageClass =>
            ((this.TargetTexture == null) ? MyRenderMessageType.Draw : MyRenderMessageType.StateChangeOnce);
    }
}

