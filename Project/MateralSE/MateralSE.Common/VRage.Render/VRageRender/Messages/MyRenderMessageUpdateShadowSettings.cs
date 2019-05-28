namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    public class MyRenderMessageUpdateShadowSettings : MyRenderMessageBase
    {
        public MyRenderMessageUpdateShadowSettings()
        {
            this.Settings = new MyShadowsSettings();
        }

        public MyShadowsSettings Settings { get; private set; }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateShadowSettings;
    }
}

