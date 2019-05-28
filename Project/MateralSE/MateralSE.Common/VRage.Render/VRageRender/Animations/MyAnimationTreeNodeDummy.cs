namespace VRageRender.Animations
{
    using System;

    public class MyAnimationTreeNodeDummy : MyAnimationTreeNode
    {
        private float m_localNormalizedTime;

        public override float GetLocalTimeNormalized() => 
            this.m_localNormalizedTime;

        public override void SetLocalTimeNormalized(float normalizedTime)
        {
            this.m_localNormalizedTime = normalizedTime;
        }

        public override void Update(ref MyAnimationUpdateData data)
        {
            data.BonesResult = data.Controller.ResultBonesPool.Alloc();
            data.AddVisitedTreeNodesPathPoint(-1);
        }
    }
}

