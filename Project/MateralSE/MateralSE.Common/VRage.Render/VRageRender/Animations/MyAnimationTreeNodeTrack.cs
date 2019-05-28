namespace VRageRender.Animations
{
    using System;
    using VRageMath;

    public class MyAnimationTreeNodeTrack : MyAnimationTreeNode
    {
        private MyAnimationClip m_animationClip;
        private double m_localTime;
        private double m_speed = 1.0;
        private int[] m_currentKeyframes;
        private int[] m_boneIndicesMapping;
        private bool m_loop = true;
        private bool m_interpolate = true;
        private int m_timeAdvancedOnFrameNum;
        private MyAnimationStateMachine m_synchronizeWithLayerRef;
        private string m_synchronizeWithLayerName;

        public override float GetLocalTimeNormalized()
        {
            if ((this.m_animationClip == null) || (this.m_animationClip.Duration <= 0.0))
            {
                return 0f;
            }
            return ((this.m_localTime < this.m_animationClip.Duration) ? ((float) (this.m_localTime / this.m_animationClip.Duration)) : (this.Loop ? 0.99999f : 1f));
        }

        private bool ProcessLayerTimeSync(ref MyAnimationUpdateData data)
        {
            if (this.m_synchronizeWithLayerRef == null)
            {
                if (this.m_synchronizeWithLayerName == null)
                {
                    return false;
                }
                this.m_synchronizeWithLayerRef = data.Controller.GetLayerByName(this.m_synchronizeWithLayerName);
                if (this.m_synchronizeWithLayerRef == null)
                {
                    return false;
                }
            }
            MyAnimationStateMachineNode currentNode = this.m_synchronizeWithLayerRef.CurrentNode as MyAnimationStateMachineNode;
            if ((currentNode == null) || (currentNode.RootAnimationNode == null))
            {
                return false;
            }
            this.SetLocalTimeNormalized(currentNode.RootAnimationNode.GetLocalTimeNormalized());
            return true;
        }

        private void RebuildBoneIndices(MyCharacterBone[] characterBones)
        {
            this.m_boneIndicesMapping = new int[this.m_animationClip.Bones.Count];
            int index = 0;
            while (index < this.m_animationClip.Bones.Count)
            {
                this.m_boneIndicesMapping[index] = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < characterBones.Length)
                    {
                        if (this.m_animationClip.Bones[index].Name != characterBones[num2].Name)
                        {
                            num2++;
                            continue;
                        }
                        this.m_boneIndicesMapping[index] = num2;
                    }
                    int num3 = this.m_boneIndicesMapping[index];
                    index++;
                    break;
                }
            }
        }

        public bool SetClip(MyAnimationClip animationClip)
        {
            this.m_animationClip = animationClip;
            this.m_currentKeyframes = (animationClip != null) ? new int[animationClip.Bones.Count] : null;
            this.m_boneIndicesMapping = null;
            return true;
        }

        public override void SetLocalTimeNormalized(float normalizedTime)
        {
            if (this.m_animationClip != null)
            {
                this.m_localTime = normalizedTime * this.m_animationClip.Duration;
            }
        }

        public override void Update(ref MyAnimationUpdateData data)
        {
            data.BonesResult = data.Controller.ResultBonesPool.Alloc();
            if ((this.m_animationClip != null) && (this.m_animationClip.Bones != null))
            {
                if (this.m_boneIndicesMapping == null)
                {
                    this.RebuildBoneIndices(data.CharacterBones);
                }
                if (!this.ProcessLayerTimeSync(ref data) && (this.m_timeAdvancedOnFrameNum != data.Controller.FrameCounter))
                {
                    this.m_timeAdvancedOnFrameNum = data.Controller.FrameCounter;
                    this.m_localTime += data.DeltaTimeInSeconds * this.Speed;
                    if (this.m_loop)
                    {
                        while (true)
                        {
                            if (this.m_localTime < this.m_animationClip.Duration)
                            {
                                while (this.m_localTime < 0.0)
                                {
                                    this.m_localTime += this.m_animationClip.Duration;
                                }
                                break;
                            }
                            this.m_localTime -= this.m_animationClip.Duration;
                        }
                    }
                    else if (this.m_localTime >= this.m_animationClip.Duration)
                    {
                        this.m_localTime = this.m_animationClip.Duration;
                    }
                    else if (this.m_localTime < 0.0)
                    {
                        this.m_localTime = 0.0;
                    }
                }
                this.UpdateKeyframeIndices();
                for (int i = 0; i < this.m_animationClip.Bones.Count; i++)
                {
                    int num3;
                    MyAnimationClip.Bone bone = this.m_animationClip.Bones[i];
                    int num2 = this.m_currentKeyframes[i];
                    if ((num2 + 1) >= bone.Keyframes.Count)
                    {
                        num3 = Math.Max(0, bone.Keyframes.Count - 1);
                    }
                    int index = this.m_boneIndicesMapping[i];
                    if (((index >= 0) && (index < data.BonesResult.Count)) && data.LayerBoneMask[index])
                    {
                        if ((num2 == num3) || !this.m_interpolate)
                        {
                            if (bone.Keyframes.Count != 0)
                            {
                                data.BonesResult[index].Rotation = bone.Keyframes[num2].Rotation;
                                data.BonesResult[index].Translation = bone.Keyframes[num2].Translation;
                            }
                        }
                        else
                        {
                            MyAnimationClip.Keyframe keyframe = bone.Keyframes[num2];
                            MyAnimationClip.Keyframe keyframe2 = bone.Keyframes[num3];
                            float amount = MathHelper.Clamp((float) ((this.m_localTime - keyframe.Time) * keyframe2.InvTimeDiff), 0f, 1f);
                            Quaternion.Slerp(ref keyframe.Rotation, ref keyframe2.Rotation, amount, out data.BonesResult[index].Rotation);
                            Vector3.Lerp(ref keyframe.Translation, ref keyframe2.Translation, amount, out data.BonesResult[index].Translation);
                        }
                    }
                }
            }
            data.AddVisitedTreeNodesPathPoint(-1);
        }

        private void UpdateKeyframeIndices()
        {
            if ((this.m_animationClip != null) && (this.m_animationClip.Bones != null))
            {
                int index = 0;
                while (index < this.m_animationClip.Bones.Count)
                {
                    MyAnimationClip.Bone bone = this.m_animationClip.Bones[index];
                    int num2 = this.m_currentKeyframes[index];
                    while (true)
                    {
                        if ((num2 >= (bone.Keyframes.Count - 2)) || (this.m_localTime <= bone.Keyframes[num2 + 1].Time))
                        {
                            while (true)
                            {
                                if ((num2 <= 0) || (this.m_localTime >= bone.Keyframes[num2].Time))
                                {
                                    this.m_currentKeyframes[index] = num2;
                                    index++;
                                    break;
                                }
                                num2--;
                            }
                            break;
                        }
                        num2++;
                    }
                }
            }
        }

        public bool Loop
        {
            get => 
                this.m_loop;
            set => 
                (this.m_loop = value);
        }

        public double Speed
        {
            get => 
                this.m_speed;
            set => 
                (this.m_speed = value);
        }

        public bool Interpolate
        {
            get => 
                this.m_interpolate;
            set => 
                (this.m_interpolate = value);
        }

        public string SynchronizeWithLayer
        {
            get => 
                this.m_synchronizeWithLayerName;
            set
            {
                this.m_synchronizeWithLayerName = value;
                this.m_synchronizeWithLayerRef = null;
            }
        }
    }
}

