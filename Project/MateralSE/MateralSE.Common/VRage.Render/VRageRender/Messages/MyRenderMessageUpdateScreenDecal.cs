namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageUpdateScreenDecal : MyRenderMessageBase
    {
        public List<MyDecalPositionUpdate> Decals = new List<MyDecalPositionUpdate>();

        public override void Init()
        {
            this.Decals.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeEvery;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateScreenDecal;
    }
}

