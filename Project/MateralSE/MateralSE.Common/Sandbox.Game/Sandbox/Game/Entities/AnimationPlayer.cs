namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    internal class AnimationPlayer
    {
        public static bool ENABLE_ANIMATION_CACHE = false;
        public static bool ENABLE_ANIMATION_LODS = true;
        private static Dictionary<int, AnimationPlayer> CachedAnimationPlayers = new Dictionary<int, AnimationPlayer>();
        private float m_position;
        private float m_duration;
        private BoneInfo[] m_boneInfos;
        private Dictionary<float, List<BoneInfo>> m_boneLODs = new Dictionary<float, List<BoneInfo>>();
        private int m_boneCount;
        private MySkinnedEntity m_skinnedEntity;
        private MyFrameOption m_frameOption = MyFrameOption.PlayOnce;
        private float m_weight = 1f;
        private float m_timeScale = 1f;
        private bool m_initialized;
        private int m_currentLODIndex;
        private List<BoneInfo> m_currentLOD;
        private int m_hash;
        public string AnimationMwmPathDebug;
        public string AnimationNameDebug;
        private bool t = true;

        public void Advance(float value)
        {
            if (this.m_frameOption == MyFrameOption.JustFirstFrame)
            {
                this.Position = 0f;
            }
            else
            {
                this.Position += value * this.m_timeScale;
                if ((this.m_frameOption == MyFrameOption.StayOnLastFrame) && (this.Position > this.m_duration))
                {
                    this.Position = this.m_duration;
                }
            }
        }

        public void Done()
        {
            this.m_initialized = false;
            CachedAnimationPlayers.Remove(this.m_hash);
        }

        private MyAnimationClip.Bone FindBone(List<MyAnimationClip.Bone> bones, string name)
        {
            using (List<MyAnimationClip.Bone>.Enumerator enumerator = bones.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyAnimationClip.Bone current = enumerator.Current;
                    if (current.Name == name)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        private static int GetAnimationPlayerHash(AnimationPlayer player)
        {
            float num = 10f;
            return ((((player.Name.GetHashCode() * 0x18d) ^ (MyUtils.GetHash((double) player.m_skinnedEntity.Model.UniqueId, -2128831035) * 0x18d)) ^ (((int) (player.m_position.GetHashCode() * num)) * 0x18d)) ^ player.m_currentLODIndex.GetHashCode());
        }

        public void Initialize(AnimationPlayer player)
        {
            if (this.m_hash != 0)
            {
                CachedAnimationPlayers.Remove(this.m_hash);
                this.m_hash = 0;
            }
            this.Name = player.Name;
            this.m_duration = player.m_duration;
            this.m_skinnedEntity = player.m_skinnedEntity;
            this.m_weight = player.Weight;
            this.m_timeScale = player.m_timeScale;
            this.m_frameOption = player.m_frameOption;
            using (Dictionary<float, List<BoneInfo>>.ValueCollection.Enumerator enumerator = this.m_boneLODs.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Clear();
                }
            }
            this.m_boneCount = player.m_boneCount;
            if ((this.m_boneInfos == null) || (this.m_boneInfos.Length < this.m_boneCount))
            {
                this.m_boneInfos = new BoneInfo[this.m_boneCount];
            }
            this.Position = player.Position;
            for (int i = 0; i < this.m_boneCount; i++)
            {
                BoneInfo item = this.m_boneInfos[i];
                if (item == null)
                {
                    item = new BoneInfo();
                    this.m_boneInfos[i] = item;
                }
                item.ClipBone = player.m_boneInfos[i].ClipBone;
                item.Player = this;
                item.SetModel(this.m_skinnedEntity);
                item.CurrentKeyframe = player.m_boneInfos[i].CurrentKeyframe;
                item.SetPosition(this.Position);
                if (((player.m_boneLODs != null) && (item.ModelBone != null)) && ENABLE_ANIMATION_LODS)
                {
                    foreach (KeyValuePair<float, List<BoneInfo>> pair in player.m_boneLODs)
                    {
                        List<BoneInfo> list;
                        if (!this.m_boneLODs.TryGetValue(pair.Key, out list))
                        {
                            list = new List<BoneInfo>();
                            this.m_boneLODs.Add(pair.Key, list);
                        }
                        foreach (BoneInfo info2 in pair.Value)
                        {
                            if ((info2.ModelBone != null) && (item.ModelBone.Name == info2.ModelBone.Name))
                            {
                                list.Add(item);
                                break;
                            }
                        }
                    }
                }
                bool flag1 = MyFakes.ENABLE_BONES_AND_ANIMATIONS_DEBUG;
            }
            this.m_initialized = true;
        }

        public void Initialize(MyModel animationModel, string playerName, int clipIndex, MySkinnedEntity skinnedEntity, float weight, float timeScale, MyFrameOption frameOption, string[] explicitBones = null, Dictionary<float, string[]> boneLODs = null)
        {
            List<BoneInfo> list;
            if (this.m_hash != 0)
            {
                CachedAnimationPlayers.Remove(this.m_hash);
                this.m_hash = 0;
            }
            MyAnimationClip clip = animationModel.Animations.Clips[clipIndex];
            this.Name = MyStringId.GetOrCompute(animationModel.AssetName + " : " + playerName);
            this.m_duration = (float) clip.Duration;
            this.m_skinnedEntity = skinnedEntity;
            this.m_weight = weight;
            this.m_timeScale = timeScale;
            this.m_frameOption = frameOption;
            using (Dictionary<float, List<BoneInfo>>.ValueCollection.Enumerator enumerator = this.m_boneLODs.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Clear();
                }
            }
            if (!this.m_boneLODs.TryGetValue(0f, out list))
            {
                list = new List<BoneInfo>();
                this.m_boneLODs.Add(0f, list);
            }
            int num = (explicitBones == null) ? clip.Bones.Count : explicitBones.Length;
            if ((this.m_boneInfos == null) || (this.m_boneInfos.Length < num))
            {
                this.m_boneInfos = new BoneInfo[num];
            }
            int index = 0;
            for (int i = 0; i < num; i++)
            {
                MyAnimationClip.Bone bone = (explicitBones == null) ? clip.Bones[i] : this.FindBone(clip.Bones, explicitBones[i]);
                if ((bone != null) && (bone.Keyframes.Count != 0))
                {
                    BoneInfo item = this.m_boneInfos[index];
                    if (this.m_boneInfos[index] == null)
                    {
                        item = new BoneInfo(bone, this);
                    }
                    else
                    {
                        item.Clear();
                        item.Init(bone, this);
                    }
                    this.m_boneInfos[index] = item;
                    this.m_boneInfos[index].SetModel(skinnedEntity);
                    if (item.ModelBone != null)
                    {
                        list.Add(item);
                        if (boneLODs != null)
                        {
                            foreach (KeyValuePair<float, string[]> pair in boneLODs)
                            {
                                List<BoneInfo> list2;
                                if (!this.m_boneLODs.TryGetValue(pair.Key, out list2))
                                {
                                    list2 = new List<BoneInfo>();
                                    this.m_boneLODs.Add(pair.Key, list2);
                                }
                                foreach (string str in pair.Value)
                                {
                                    if (item.ModelBone.Name == str)
                                    {
                                        list2.Add(item);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    index++;
                }
            }
            this.m_boneCount = index;
            this.Position = 0f;
            this.m_initialized = true;
        }

        public void UpdateBones(float distance)
        {
            if (!ENABLE_ANIMATION_LODS)
            {
                for (int i = 0; i < this.m_boneCount; i++)
                {
                    this.m_boneInfos[i].SetPosition(this.m_position);
                }
            }
            else
            {
                this.m_currentLODIndex = -1;
                this.m_currentLOD = null;
                int num = 0;
                List<BoneInfo> list = null;
                foreach (KeyValuePair<float, List<BoneInfo>> pair in this.m_boneLODs)
                {
                    if (distance <= pair.Key)
                    {
                        break;
                    }
                    list = pair.Value;
                    this.m_currentLODIndex = num;
                    this.m_currentLOD = list;
                    num++;
                }
                if (list != null)
                {
                    AnimationPlayer player;
                    if (CachedAnimationPlayers.TryGetValue(this.m_hash, out player) && ReferenceEquals(player, this))
                    {
                        CachedAnimationPlayers.Remove(this.m_hash);
                    }
                    this.m_hash = GetAnimationPlayerHash(this);
                    if (CachedAnimationPlayers.TryGetValue(this.m_hash, out player))
                    {
                        int animationPlayerHash = GetAnimationPlayerHash(player);
                        if (this.m_hash != animationPlayerHash)
                        {
                            CachedAnimationPlayers.Remove(this.m_hash);
                            player = null;
                        }
                    }
                    if (player != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Translation = player.m_currentLOD[i].Translation;
                            list[i].Rotation = player.m_currentLOD[i].Rotation;
                            list[i].AssignToCharacterBone();
                        }
                    }
                    else if (list.Count > 0)
                    {
                        if (ENABLE_ANIMATION_CACHE)
                        {
                            CachedAnimationPlayers[this.m_hash] = this;
                        }
                        using (List<BoneInfo>.Enumerator enumerator2 = list.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                enumerator2.Current.SetPosition(this.m_position);
                            }
                        }
                    }
                }
            }
        }

        public MyStringId Name { get; private set; }

        [Browsable(false)]
        public float Position
        {
            get => 
                this.m_position;
            set
            {
                float num = value;
                if (num > this.m_duration)
                {
                    if (this.Looping)
                    {
                        num -= this.m_duration;
                    }
                    else
                    {
                        value = this.m_duration;
                    }
                }
                this.m_position = num;
            }
        }

        public float Weight
        {
            get => 
                this.m_weight;
            set => 
                (this.m_weight = value);
        }

        public float TimeScale
        {
            get => 
                this.m_timeScale;
            set => 
                (this.m_timeScale = value);
        }

        public bool Looping =>
            (this.m_frameOption == MyFrameOption.Loop);

        public bool AtEnd =>
            ((this.Position >= this.m_duration) && (this.m_frameOption != MyFrameOption.StayOnLastFrame));

        public bool IsInitialized =>
            this.m_initialized;

        private class BoneInfo
        {
            private int m_currentKeyframe;
            private bool m_isConst;
            private MyCharacterBone m_assignedBone;
            public Quaternion Rotation;
            public Vector3 Translation;
            public AnimationPlayer Player;
            public MyAnimationClip.Keyframe Keyframe1;
            public MyAnimationClip.Keyframe Keyframe2;
            private MyAnimationClip.Bone m_clipBone;

            public BoneInfo()
            {
            }

            public BoneInfo(MyAnimationClip.Bone bone, AnimationPlayer player)
            {
                this.Init(bone, player);
            }

            public void AssignToCharacterBone()
            {
                if (this.m_assignedBone != null)
                {
                    Quaternion rotation = this.Rotation;
                    Quaternion additionalRotation = this.Player.m_skinnedEntity.GetAdditionalRotation(this.m_assignedBone.Name);
                    rotation = this.Rotation * additionalRotation;
                    this.m_assignedBone.SetCompleteTransform(ref this.Translation, ref rotation, this.Player.Weight);
                }
            }

            public void Clear()
            {
                this.m_currentKeyframe = 0;
                this.m_isConst = false;
                this.m_assignedBone = null;
                this.Rotation = new Quaternion();
                this.Translation = Vector3.Zero;
                this.Player = null;
                this.Keyframe1 = null;
                this.Keyframe2 = null;
                this.m_clipBone = null;
            }

            public void Init(MyAnimationClip.Bone bone, AnimationPlayer player)
            {
                this.ClipBone = bone;
                this.Player = player;
                this.SetKeyframes();
                this.SetPosition(0f);
                this.m_isConst = this.ClipBone.Keyframes.Count == 1;
            }

            private void SetKeyframes()
            {
                if (this.ClipBone != null)
                {
                    if (this.ClipBone.Keyframes.Count <= 0)
                    {
                        this.Keyframe1 = null;
                        this.Keyframe2 = null;
                    }
                    else
                    {
                        this.Keyframe1 = this.ClipBone.Keyframes[this.m_currentKeyframe];
                        if (this.m_currentKeyframe == (this.ClipBone.Keyframes.Count - 1))
                        {
                            this.Keyframe2 = this.Keyframe1;
                        }
                        else
                        {
                            this.Keyframe2 = this.ClipBone.Keyframes[this.m_currentKeyframe + 1];
                        }
                    }
                }
            }

            public void SetModel(MySkinnedEntity skinnedEntity)
            {
                if (this.ClipBone != null)
                {
                    int num;
                    this.m_assignedBone = skinnedEntity.AnimationController.FindBone(this.ClipBone.Name, out num);
                }
            }

            public void SetPosition(float position)
            {
                if (this.ClipBone != null)
                {
                    List<MyAnimationClip.Keyframe> keyframes = this.ClipBone.Keyframes;
                    if (((keyframes != null) && ((this.Keyframe1 != null) && (this.Keyframe2 != null))) && (keyframes.Count != 0))
                    {
                        if (!this.m_isConst)
                        {
                            while (true)
                            {
                                if ((position >= this.Keyframe1.Time) || (this.m_currentKeyframe <= 0))
                                {
                                    while (true)
                                    {
                                        if ((position < this.Keyframe2.Time) || (this.m_currentKeyframe >= (this.ClipBone.Keyframes.Count - 2)))
                                        {
                                            if (ReferenceEquals(this.Keyframe1, this.Keyframe2))
                                            {
                                                this.Rotation = this.Keyframe1.Rotation;
                                                this.Translation = this.Keyframe1.Translation;
                                            }
                                            else
                                            {
                                                float amount = MathHelper.Clamp((float) ((position - this.Keyframe1.Time) * this.Keyframe2.InvTimeDiff), 0f, 1f);
                                                Quaternion.Slerp(ref this.Keyframe1.Rotation, ref this.Keyframe2.Rotation, amount, out this.Rotation);
                                                Vector3.Lerp(ref this.Keyframe1.Translation, ref this.Keyframe2.Translation, amount, out this.Translation);
                                            }
                                            break;
                                        }
                                        this.m_currentKeyframe++;
                                        this.SetKeyframes();
                                    }
                                    break;
                                }
                                this.m_currentKeyframe--;
                                this.SetKeyframes();
                            }
                        }
                        this.AssignToCharacterBone();
                    }
                }
            }

            public int CurrentKeyframe
            {
                get => 
                    this.m_currentKeyframe;
                set
                {
                    this.m_currentKeyframe = value;
                    this.SetKeyframes();
                }
            }

            public MyAnimationClip.Bone ClipBone
            {
                get => 
                    this.m_clipBone;
                set => 
                    (this.m_clipBone = value);
            }

            public MyCharacterBone ModelBone =>
                this.m_assignedBone;
        }
    }
}

