namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyAnimationClip
    {
        private List<Bone> bones = new List<Bone>();
        public string Name;
        public double Duration;

        public List<Bone> Bones =>
            this.bones;

        public class Bone
        {
            private string m_name = "";
            private readonly List<MyAnimationClip.Keyframe> m_keyframes = new List<MyAnimationClip.Keyframe>();

            public override string ToString()
            {
                object[] objArray1 = new object[] { this.m_name, " (", this.Keyframes.Count, " keys)" };
                return string.Concat(objArray1);
            }

            public string Name
            {
                get => 
                    this.m_name;
                set => 
                    (this.m_name = value);
            }

            public List<MyAnimationClip.Keyframe> Keyframes =>
                this.m_keyframes;
        }

        public class BoneState
        {
            public Quaternion Rotation = Quaternion.Identity;
            public Vector3 Translation;
        }

        public class Keyframe : MyAnimationClip.BoneState
        {
            public double Time;
            public double InvTimeDiff;
        }
    }
}

