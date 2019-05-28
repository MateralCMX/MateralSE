namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;

    public class ModelAnimations
    {
        private List<int> skeleton = new List<int>();
        private List<MyAnimationClip> clips = new List<MyAnimationClip>();

        public List<int> Skeleton
        {
            get => 
                this.skeleton;
            set => 
                (this.skeleton = value);
        }

        public List<MyAnimationClip> Clips
        {
            get => 
                this.clips;
            set => 
                (this.clips = value);
        }
    }
}

