namespace VRageRender.Messages
{
    using System;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    public class MyRenderMessageCreateRenderEntity : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public string Model;
        public MatrixD WorldMatrix;
        public MyMeshDrawTechnique Technique;
        public RenderFlags Flags;
        public int DepthBias;
        public VRageRender.CullingOptions CullingOptions;
        public float MaxViewDistance;
        public float Rescale = 1f;

        public override string ToString() => 
            (this.DebugName ?? (string.Empty + ", " + this.Model));

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderEntity;
    }
}

