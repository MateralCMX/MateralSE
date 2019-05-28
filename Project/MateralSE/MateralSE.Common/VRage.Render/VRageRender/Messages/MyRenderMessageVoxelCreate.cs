namespace VRageRender.Messages
{
    using System;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    public class MyRenderMessageVoxelCreate : MyRenderMessageBase
    {
        public uint Id;
        public string DebugName;
        public MatrixD WorldMatrix;
        public Vector3I Size;
        public float? SpherizeRadius;
        public Vector3D SpherizePosition;
        public IMyLodController Clipmap;
        public VRageRender.RenderFlags RenderFlags;
        public float Dithering;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.VoxelCreate;
    }
}

