namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetCharacterSkeleton : MyRenderMessageBase
    {
        public uint CharacterID;
        public MySkeletonBoneDescription[] SkeletonBones;
        public int[] SkeletonIndices;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetCharacterSkeleton;
    }
}

