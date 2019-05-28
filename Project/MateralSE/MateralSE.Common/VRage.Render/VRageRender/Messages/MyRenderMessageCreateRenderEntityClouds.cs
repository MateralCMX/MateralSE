namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCreateRenderEntityClouds : MyRenderMessageBase
    {
        public MyCloudLayerSettingsRender Settings;

        public override string ToString() => 
            (this.Settings.DebugName ?? (string.Empty + ", " + this.Settings.Model));

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderEntityClouds;
    }
}

