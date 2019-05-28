namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateRenderEntity : MyRenderMessageBase
    {
        public uint ID;
        public Color? DiffuseColor;
        public Vector3? ColorMaskHSV;
        public float? Dithering;
        public bool FadeIn;

        public override void Close()
        {
            this.DiffuseColor = null;
            this.ColorMaskHSV = null;
            base.Close();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderEntity;
    }
}

