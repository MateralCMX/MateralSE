namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyRenderMessagePreloadTextures : MyRenderMessageBase
    {
        public List<string> Files;

        public override void Close()
        {
            base.Close();
            this.Files.Clear();
        }

        public VRageRender.Messages.TextureType TextureType { get; set; }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.PreloadTextures;
    }
}

