namespace VRageRender.Messages
{
    using VRage;

    public class MyRenderMessageUpdateRenderVoxelMaterials : MyRenderMessageBase
    {
        public MyRenderVoxelMaterialData[] Materials;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderVoxelMaterials;
    }
}

