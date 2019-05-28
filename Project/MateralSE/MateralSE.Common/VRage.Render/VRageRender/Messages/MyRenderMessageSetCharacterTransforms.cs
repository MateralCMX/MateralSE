namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyRenderMessageSetCharacterTransforms : MyRenderMessageBase
    {
        public uint CharacterID;
        public Matrix[] BoneAbsoluteTransforms;
        public List<MyBoneDecalUpdate> BoneDecalUpdates = new List<MyBoneDecalUpdate>();

        public override void Close()
        {
            base.Close();
            this.CharacterID = uint.MaxValue;
        }

        public override void Init()
        {
            this.BoneDecalUpdates.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeEvery;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetCharacterTransforms;
    }
}

