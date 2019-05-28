namespace VRageRender.Messages
{
    using System;
    using VRageMath;
    using VRageRender;

    public class MyRenderMessageCreateRenderCharacter : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public string Model;
        public string LOD1;
        public MatrixD WorldMatrix;
        public RenderFlags Flags;
        public Color? DiffuseColor;
        public Vector3? ColorMaskHSV;
        public bool FadeIn;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderCharacter;
    }
}

