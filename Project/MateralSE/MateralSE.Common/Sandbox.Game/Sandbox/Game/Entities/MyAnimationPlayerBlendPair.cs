namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.FileSystem;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Models;
    using VRageMath;

    internal class MyAnimationPlayerBlendPair
    {
        public AnimationPlayer BlendPlayer = new AnimationPlayer();
        public AnimationPlayer ActualPlayer = new AnimationPlayer();
        private AnimationBlendState m_state;
        public float m_currentBlendTime;
        public float m_totalBlendTime;
        private string[] m_bones;
        private MySkinnedEntity m_skinnedEntity;
        private string m_name;
        private Dictionary<float, string[]> m_boneLODs;

        public MyAnimationPlayerBlendPair(MySkinnedEntity skinnedEntity, string[] bones, Dictionary<float, string[]> boneLODs, string name)
        {
            this.m_bones = bones;
            this.m_skinnedEntity = skinnedEntity;
            this.m_boneLODs = boneLODs;
            this.m_name = name;
        }

        public bool Advance()
        {
            if (this.m_state == AnimationBlendState.Stopped)
            {
                return false;
            }
            float num = 0.01666667f;
            this.m_currentBlendTime += num * this.ActualPlayer.TimeScale;
            this.ActualPlayer.Advance(num);
            if ((!this.ActualPlayer.Looping && this.ActualPlayer.AtEnd) && (this.m_state == AnimationBlendState.Playing))
            {
                this.Stop(this.m_totalBlendTime);
            }
            return true;
        }

        public AnimationBlendState GetState() => 
            this.m_state;

        public void Play(MyAnimationDefinition animationDefinition, bool firstPerson, MyFrameOption frameOption, float blendTime, float timeScale)
        {
            string animationModel;
            if (!firstPerson || string.IsNullOrEmpty(animationDefinition.AnimationModelFPS))
            {
                animationModel = animationDefinition.AnimationModel;
            }
            else
            {
                animationModel = animationDefinition.AnimationModelFPS;
            }
            string path = animationModel;
            if (!string.IsNullOrEmpty(animationDefinition.AnimationModel))
            {
                if ((animationDefinition.Status == MyAnimationDefinition.AnimationStatus.Unchecked) && !MyFileSystem.FileExists(Path.IsPathRooted(path) ? path : Path.Combine(MyFileSystem.ContentPath, path)))
                {
                    animationDefinition.Status = MyAnimationDefinition.AnimationStatus.Failed;
                }
                else
                {
                    animationDefinition.Status = MyAnimationDefinition.AnimationStatus.OK;
                    MyModel modelOnlyAnimationData = MyModels.GetModelOnlyAnimationData(path, false);
                    if ((((modelOnlyAnimationData == null) || (modelOnlyAnimationData.Animations != null)) && (modelOnlyAnimationData.Animations.Clips.Count != 0)) && (modelOnlyAnimationData.Animations.Clips.Count > animationDefinition.ClipIndex))
                    {
                        if (this.ActualPlayer.IsInitialized)
                        {
                            this.BlendPlayer.Initialize(this.ActualPlayer);
                        }
                        this.ActualPlayer.Initialize(modelOnlyAnimationData, this.m_name, animationDefinition.ClipIndex, this.m_skinnedEntity, 1f, timeScale, frameOption, this.m_bones, this.m_boneLODs);
                        this.ActualPlayer.AnimationMwmPathDebug = path;
                        this.ActualPlayer.AnimationNameDebug = animationDefinition.Id.SubtypeName;
                        this.m_state = AnimationBlendState.BlendIn;
                        this.m_currentBlendTime = 0f;
                        this.m_totalBlendTime = blendTime;
                    }
                }
            }
        }

        public void SetBoneLODs(Dictionary<float, string[]> boneLODs)
        {
            this.m_boneLODs = boneLODs;
        }

        public void Stop(float blendTime)
        {
            if (this.m_state != AnimationBlendState.Stopped)
            {
                this.BlendPlayer.Done();
                this.m_state = AnimationBlendState.BlendOut;
                this.m_currentBlendTime = 0f;
                this.m_totalBlendTime = blendTime;
            }
        }

        public void UpdateAnimationState()
        {
            float num = 0f;
            if (this.ActualPlayer.IsInitialized && (this.m_currentBlendTime > 0f))
            {
                num = 1f;
                if (this.m_totalBlendTime > 0f)
                {
                    num = MathHelper.Clamp((float) (this.m_currentBlendTime / this.m_totalBlendTime), (float) 0f, (float) 1f);
                }
            }
            if (this.ActualPlayer.IsInitialized)
            {
                if (this.m_state == AnimationBlendState.BlendOut)
                {
                    this.ActualPlayer.Weight = 1f - num;
                    if (num == 1f)
                    {
                        this.ActualPlayer.Done();
                        this.m_state = AnimationBlendState.Stopped;
                    }
                }
                if (this.m_state == AnimationBlendState.BlendIn)
                {
                    if (this.m_totalBlendTime == 0f)
                    {
                        num = 1f;
                    }
                    this.ActualPlayer.Weight = num;
                    if (this.BlendPlayer.IsInitialized)
                    {
                        this.BlendPlayer.Weight = 1f;
                    }
                    if (num == 1f)
                    {
                        this.m_state = AnimationBlendState.Playing;
                        this.BlendPlayer.Done();
                    }
                }
            }
        }

        public void UpdateBones(float distance)
        {
            if (this.m_state != AnimationBlendState.Stopped)
            {
                if (this.BlendPlayer.IsInitialized)
                {
                    this.BlendPlayer.UpdateBones(distance);
                }
                if (this.ActualPlayer.IsInitialized)
                {
                    this.ActualPlayer.UpdateBones(distance);
                }
            }
        }

        public enum AnimationBlendState
        {
            Stopped,
            BlendIn,
            Playing,
            BlendOut
        }
    }
}

