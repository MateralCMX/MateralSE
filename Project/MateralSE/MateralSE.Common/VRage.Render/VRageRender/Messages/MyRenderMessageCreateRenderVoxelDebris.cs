namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageCreateRenderVoxelDebris : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public string Model;
        public MatrixD WorldMatrix;
        public float TextureCoordOffset;
        public float TextureCoordScale;
        public float TextureColorMultiplier;
        public byte VoxelMaterialIndex;
        public bool FadeIn;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderVoxelDebris;
    }
}

