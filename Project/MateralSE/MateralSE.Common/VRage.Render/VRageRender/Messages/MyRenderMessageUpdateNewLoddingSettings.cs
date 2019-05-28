namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    public class MyRenderMessageUpdateNewLoddingSettings : MyRenderMessageBase
    {
        public MyRenderMessageUpdateNewLoddingSettings()
        {
            this.Settings = new MyNewLoddingSettings();
        }

        public MyNewLoddingSettings Settings { get; private set; }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateNewLoddingSettings;
    }
}

