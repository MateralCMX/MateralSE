namespace VRageRender.Animations
{
    using System;
    using VRage.Utils;

    public abstract class MyAnimationTreeNode
    {
        protected MyAnimationTreeNode()
        {
        }

        public abstract float GetLocalTimeNormalized();
        public virtual void SetAction(MyStringId action)
        {
        }

        public abstract void SetLocalTimeNormalized(float normalizedTime);
        public abstract void Update(ref MyAnimationUpdateData data);
    }
}

