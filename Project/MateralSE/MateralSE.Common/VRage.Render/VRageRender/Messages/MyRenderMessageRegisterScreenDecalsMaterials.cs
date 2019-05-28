namespace VRageRender.Messages
{
    using System.Collections.Generic;

    public class MyRenderMessageRegisterScreenDecalsMaterials : MyRenderMessageBase
    {
        public Dictionary<string, List<MyDecalMaterialDesc>> MaterialDescriptions;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RegisterDecalsMaterials;
    }
}

