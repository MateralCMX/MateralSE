namespace VRageRender.Messages
{
    using System;
    using VRageRender;

    public class MyRenderMessageCreateScreenDecal : MyRenderMessageBase
    {
        public uint ID;
        public uint[] ParentIDs;
        public MyDecalTopoData TopoData;
        public MyDecalFlags Flags;
        public string SourceTarget;
        public string Material;
        public int MaterialIndex;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateScreenDecal;
    }
}

