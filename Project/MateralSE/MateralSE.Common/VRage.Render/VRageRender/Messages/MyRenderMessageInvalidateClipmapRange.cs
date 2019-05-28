namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageInvalidateClipmapRange : MyRenderMessageBase
    {
        public uint ClipmapId;
        public Vector3I MinCellLod0;
        public Vector3I MaxCellLod0;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.InvalidateClipmapRange;
    }
}

