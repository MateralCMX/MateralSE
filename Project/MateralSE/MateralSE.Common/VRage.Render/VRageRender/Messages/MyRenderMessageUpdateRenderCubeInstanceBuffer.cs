namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageUpdateRenderCubeInstanceBuffer : MyRenderMessageBase
    {
        public uint ID;
        public List<MyCubeInstanceData> InstanceData = new List<MyCubeInstanceData>();
        public List<MyCubeInstanceDecalData> DecalsData = new List<MyCubeInstanceDecalData>();
        public int Capacity;

        public override void Close()
        {
            this.InstanceData.Clear();
            base.Close();
        }

        public override void Init()
        {
            this.DecalsData.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderCubeInstanceBuffer;
    }
}

