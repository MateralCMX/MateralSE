namespace VRageRender.Animations
{
    using System;
    using VRageMath;

    public class MyAnimationIkChainExt : MyAnimationIkChain
    {
        public float LastTerrainHeight;
        public Vector3 LastTerrainNormal;
        public Vector3 LastPoleVector;
        public Matrix LastAligningRotationMatrix;
        public float AligningSmoothness;

        public MyAnimationIkChainExt()
        {
            this.LastTerrainNormal = Vector3.Up;
            this.LastPoleVector = Vector3.Left;
            this.LastAligningRotationMatrix = Matrix.Identity;
            this.AligningSmoothness = 0.2f;
        }

        public MyAnimationIkChainExt(MyAnimationIkChain initFromChain)
        {
            this.LastTerrainNormal = Vector3.Up;
            this.LastPoleVector = Vector3.Left;
            this.LastAligningRotationMatrix = Matrix.Identity;
            this.AligningSmoothness = 0.2f;
            base.BoneIndex = initFromChain.BoneIndex;
            base.BoneName = initFromChain.BoneName;
            base.ChainLength = initFromChain.ChainLength;
            base.AlignBoneWithTerrain = initFromChain.AlignBoneWithTerrain;
            base.EndBoneTransform = initFromChain.EndBoneTransform;
            base.MinEndPointRotation = initFromChain.MinEndPointRotation;
            base.MaxEndPointRotation = initFromChain.MaxEndPointRotation;
        }
    }
}

