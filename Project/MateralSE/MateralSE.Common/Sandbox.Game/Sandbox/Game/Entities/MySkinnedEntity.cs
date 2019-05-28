namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;
    using VRageRender.Import;
    using VRageRender.Messages;

    public class MySkinnedEntity : MyEntity, IMySkinnedEntity
    {
        public bool UseNewAnimationSystem;
        private const int MAX_BONE_DECALS_COUNT = 10;
        private MyAnimationControllerComponent m_compAnimationController;
        private Dictionary<int, List<uint>> m_boneDecals = new Dictionary<int, List<uint>>();
        protected ulong m_actualUpdateFrame;
        protected ulong m_actualDrawFrame;
        protected Dictionary<string, Quaternion> m_additionalRotations = new Dictionary<string, Quaternion>();
        private Dictionary<string, MyAnimationPlayerBlendPair> m_animationPlayers = new Dictionary<string, MyAnimationPlayerBlendPair>();
        private Queue<MyAnimationCommand> m_commandQueue = new Queue<MyAnimationCommand>();
        private BoundingBoxD m_actualWorldAABB;
        private BoundingBoxD m_aabb;
        private List<MyAnimationSetData> m_continuingAnimSets = new List<MyAnimationSetData>();

        public MySkinnedEntity()
        {
            base.Render = new MyRenderComponentSkinnedEntity();
            base.Render.EnableColorMaskHsv = true;
            base.Render.NeedsDraw = true;
            base.Render.CastShadows = true;
            base.Render.NeedsResolveCastShadow = false;
            base.Render.SkipIfTooSmall = false;
            MyEntityTerrainHeightProviderComponent component = new MyEntityTerrainHeightProviderComponent();
            base.Components.Add<MyEntityTerrainHeightProviderComponent>(component);
            this.m_compAnimationController = new MyAnimationControllerComponent(this, new Action(this.ObtainBones), component);
            base.Components.Add<MyAnimationControllerComponent>(this.m_compAnimationController);
            this.DecalBoneUpdates = new List<MyBoneDecalUpdate>();
        }

        internal void AddAnimationPlayer(string name, string[] bones)
        {
            this.m_animationPlayers.Add(name, new MyAnimationPlayerBlendPair(this, bones, null, name));
        }

        protected void AddBoneDecal(uint decalId, int boneIndex)
        {
            List<uint> list;
            if (!this.m_boneDecals.TryGetValue(boneIndex, out list))
            {
                list = new List<uint>(10);
                this.m_boneDecals.Add(boneIndex, list);
            }
            if (list.Count == list.Capacity)
            {
                MyDecals.RemoveDecal(list[0]);
                list.RemoveAt(0);
            }
            list.Add(decalId);
        }

        public virtual void AddCommand(MyAnimationCommand command, bool sync = false)
        {
            this.m_commandQueue.Enqueue(command);
        }

        private bool AdvanceAnimation()
        {
            bool flag = false;
            foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair in this.m_animationPlayers)
            {
                flag = pair.Value.Advance() | flag;
            }
            return flag;
        }

        protected virtual void CalculateTransforms(float distance)
        {
            if (!this.UseNewAnimationSystem)
            {
                this.UpdateBones(distance);
            }
            this.AnimationController.UpdateTransformations();
        }

        protected void FlushAnimationQueue()
        {
            while (this.m_commandQueue.Count > 0)
            {
                this.ProcessCommands();
            }
        }

        public Quaternion GetAdditionalRotation(string bone)
        {
            Quaternion identity = Quaternion.Identity;
            return (!string.IsNullOrEmpty(bone) ? (!this.m_additionalRotations.TryGetValue(bone, out identity) ? Quaternion.Identity : identity) : identity);
        }

        internal DictionaryReader<string, MyAnimationPlayerBlendPair> GetAllAnimationPlayers() => 
            this.m_animationPlayers;

        public override void Init(StringBuilder displayName, string model, MyEntity parentObject, float? scale, string modelCollision = null)
        {
            base.Init(displayName, model, parentObject, scale, modelCollision);
            this.InitBones();
        }

        protected void InitBones()
        {
            this.ObtainBones();
            this.m_animationPlayers.Clear();
            this.AddAnimationPlayer("", null);
        }

        public virtual void ObtainBones()
        {
            MyCharacterBone[] characterBones = new MyCharacterBone[base.Model.Bones.Length];
            Matrix[] relativeStorage = new Matrix[base.Model.Bones.Length];
            Matrix[] absoluteStorage = new Matrix[base.Model.Bones.Length];
            for (int i = 0; i < base.Model.Bones.Length; i++)
            {
                MyModelBone bone = base.Model.Bones[i];
                Matrix transform = bone.Transform;
                MyCharacterBone parent = (bone.Parent != -1) ? characterBones[bone.Parent] : null;
                characterBones[i] = new MyCharacterBone(bone.Name, parent, transform, i, relativeStorage, absoluteStorage);
            }
            this.m_compAnimationController.SetCharacterBones(characterBones, relativeStorage, absoluteStorage);
        }

        protected virtual void OnAnimationPlay(MyAnimationDefinition animDefinition, MyAnimationCommand command, ref string bonesArea, ref MyFrameOption frameOption, ref bool useFirstPersonVersion)
        {
        }

        private void PlayAnimationSet(MyAnimationSetData animationSetData)
        {
            if (MyRandom.Instance.NextFloat(0f, 1f) < animationSetData.AnimationSet.Probability)
            {
                float num = animationSetData.AnimationSet.AnimationItems.Sum<AnimationItem>(x => x.Ratio);
                if (num > 0f)
                {
                    float num2 = MyRandom.Instance.NextFloat(0f, 1f);
                    float num3 = 0f;
                    foreach (AnimationItem item in animationSetData.AnimationSet.AnimationItems)
                    {
                        num3 += item.Ratio / num;
                        if (num2 < num3)
                        {
                            MyAnimationCommand command = new MyAnimationCommand {
                                AnimationSubtypeName = item.Animation,
                                PlaybackCommand = MyPlaybackCommand.Play,
                                Area = animationSetData.Area,
                                BlendTime = animationSetData.BlendTime,
                                TimeScale = 1f,
                                KeepContinuingAnimations = true
                            };
                            this.ProcessCommand(ref command);
                            return;
                        }
                    }
                }
            }
        }

        internal void PlayerPlay(string playerName, MyAnimationDefinition animDefinition, bool firstPerson, MyFrameOption frameOption, float blendTime, float timeScale)
        {
            MyAnimationPlayerBlendPair pair;
            if (this.TryGetAnimationPlayer(playerName, out pair))
            {
                pair.Play(animDefinition, firstPerson, frameOption, blendTime, timeScale);
            }
        }

        internal void PlayersPlay(string bonesArea, MyAnimationDefinition animDefinition, bool firstPerson, MyFrameOption frameOption, float blendTime, float timeScale)
        {
            char[] separator = new char[] { ' ' };
            string[] strArray = bonesArea.Split(separator);
            if (animDefinition.AnimationSets == null)
            {
                foreach (string str in strArray)
                {
                    this.PlayerPlay(str, animDefinition, firstPerson, frameOption, blendTime, timeScale);
                }
            }
            else
            {
                foreach (AnimationSet set in animDefinition.AnimationSets)
                {
                    MyAnimationSetData item = new MyAnimationSetData {
                        BlendTime = blendTime,
                        Area = bonesArea,
                        AnimationSet = set
                    };
                    if (set.Continuous)
                    {
                        this.m_continuingAnimSets.Add(item);
                    }
                    else
                    {
                        this.PlayAnimationSet(item);
                    }
                }
            }
        }

        internal void PlayerStop(string playerName, float blendTime)
        {
            MyAnimationPlayerBlendPair pair;
            if (this.TryGetAnimationPlayer(playerName, out pair))
            {
                pair.Stop(blendTime);
            }
        }

        private void ProcessCommand(ref MyAnimationCommand command)
        {
            if (command.PlaybackCommand != MyPlaybackCommand.Play)
            {
                if (command.PlaybackCommand == MyPlaybackCommand.Stop)
                {
                    char[] separator = new char[] { ' ' };
                    string[] strArray = ((command.Area == null) ? "" : command.Area).Split(separator);
                    if (!this.UseNewAnimationSystem)
                    {
                        foreach (string str2 in strArray)
                        {
                            this.PlayerStop(str2, command.BlendTime);
                        }
                    }
                }
            }
            else
            {
                MyAnimationDefinition definition;
                if (this.TryGetAnimationDefinition(command.AnimationSubtypeName, out definition))
                {
                    string influenceArea = definition.InfluenceArea;
                    MyFrameOption frameOption = command.FrameOption;
                    if (frameOption == MyFrameOption.Default)
                    {
                        frameOption = definition.Loop ? MyFrameOption.Loop : MyFrameOption.PlayOnce;
                    }
                    bool useFirstPersonVersion = false;
                    this.OnAnimationPlay(definition, command, ref influenceArea, ref frameOption, ref useFirstPersonVersion);
                    if (!string.IsNullOrEmpty(command.Area))
                    {
                        influenceArea = command.Area;
                    }
                    if (influenceArea == null)
                    {
                        influenceArea = "";
                    }
                    if (!command.KeepContinuingAnimations)
                    {
                        this.m_continuingAnimSets.Clear();
                    }
                    if (!this.UseNewAnimationSystem)
                    {
                        this.PlayersPlay(influenceArea, definition, useFirstPersonVersion, frameOption, command.BlendTime, command.TimeScale);
                    }
                }
            }
        }

        protected bool ProcessCommands()
        {
            if (this.m_commandQueue.Count <= 0)
            {
                return false;
            }
            MyAnimationCommand command = this.m_commandQueue.Dequeue();
            this.ProcessCommand(ref command);
            return true;
        }

        public void SetBoneLODs(Dictionary<float, string[]> boneLODs)
        {
            foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair in this.m_animationPlayers)
            {
                pair.Value.SetBoneLODs(boneLODs);
            }
        }

        [Obsolete]
        protected bool TryGetAnimationDefinition(string animationSubtypeName, out MyAnimationDefinition animDefinition)
        {
            if (animationSubtypeName == null)
            {
                animDefinition = null;
                return false;
            }
            animDefinition = MyDefinitionManager.Static.TryGetAnimationDefinition(animationSubtypeName);
            if (animDefinition == null)
            {
                string path = Path.Combine(MyFileSystem.ContentPath, animationSubtypeName);
                if (!MyFileSystem.FileExists(path))
                {
                    animDefinition = null;
                    return false;
                }
                MyAnimationDefinition definition1 = new MyAnimationDefinition();
                definition1.AnimationModel = path;
                definition1.ClipIndex = 0;
                animDefinition = definition1;
            }
            return true;
        }

        internal bool TryGetAnimationPlayer(string name, out MyAnimationPlayerBlendPair player)
        {
            if (name == null)
            {
                name = "";
            }
            if (name == "Body")
            {
                name = "";
            }
            return this.m_animationPlayers.TryGetValue(name, out player);
        }

        public virtual void UpdateAnimation(float distance)
        {
            this.m_compAnimationController.CameraDistance = distance;
            if (((!MyPerGameSettings.AnimateOnlyVisibleCharacters || Sandbox.Engine.Platform.Game.IsDedicated) || (((base.Render != null) && ((base.Render.RenderObjectIDs.Length != 0) && (MyRenderProxy.VisibleObjectsRead != null))) && MyRenderProxy.VisibleObjectsRead.Contains(base.Render.RenderObjectIDs[0]))) && (distance < MyFakes.ANIMATION_UPDATE_DISTANCE))
            {
                this.UpdateContinuingSets();
                bool flag = this.ProcessCommands();
                this.UpdateAnimationState();
                if ((this.AdvanceAnimation() | flag) || this.UseNewAnimationSystem)
                {
                    this.CalculateTransforms(distance);
                    this.UpdateRenderObject();
                }
            }
            this.UpdateBoneDecals();
        }

        private void UpdateAnimationState()
        {
            foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair in this.m_animationPlayers)
            {
                pair.Value.UpdateAnimationState();
            }
        }

        private void UpdateBoneDecals()
        {
            this.DecalBoneUpdates.Clear();
            foreach (KeyValuePair<int, List<uint>> pair in this.m_boneDecals)
            {
                foreach (uint num in pair.Value)
                {
                    MyBoneDecalUpdate item = new MyBoneDecalUpdate {
                        BoneID = pair.Key,
                        DecalID = num
                    };
                    this.DecalBoneUpdates.Add(item);
                }
            }
        }

        private void UpdateBones(float distance)
        {
            foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair in this.m_animationPlayers)
            {
                pair.Value.UpdateBones(distance);
            }
        }

        private void UpdateContinuingSets()
        {
            foreach (MyAnimationSetData data in this.m_continuingAnimSets)
            {
                this.PlayAnimationSet(data);
            }
        }

        protected void UpdateRenderObject()
        {
        }

        public MyAnimationControllerComponent AnimationController =>
            this.m_compAnimationController;

        public Matrix[] BoneAbsoluteTransforms =>
            this.m_compAnimationController.BoneAbsoluteTransforms;

        public Matrix[] BoneRelativeTransforms =>
            this.m_compAnimationController.BoneRelativeTransforms;

        public List<MyBoneDecalUpdate> DecalBoneUpdates { get; private set; }

        internal ulong ActualUpdateFrame =>
            this.m_actualUpdateFrame;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySkinnedEntity.<>c <>9 = new MySkinnedEntity.<>c();
            public static Func<AnimationItem, float> <>9__38_0;

            internal float <PlayAnimationSet>b__38_0(AnimationItem x) => 
                x.Ratio;
        }
    }
}

