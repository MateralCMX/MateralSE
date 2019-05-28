namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessagePreloadVoxelMaterials : MyRenderMessageBase
    {
        public byte[] Materials;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.PreloadVoxelMaterials;
    }
}

