namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.SessionComponents;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    public class MyAnimationControllerComponent : MyEntityComponentBase
    {
        private readonly MyAnimationController m_controller = new MyAnimationController();
        private MyCharacterBone[] m_characterBones;
        private Matrix[] m_boneRelativeTransforms;
        private Matrix[] m_boneAbsoluteTransforms;
        private List<MyAnimationClip.BoneState> m_lastBoneResult;
        private bool m_componentValid;
        public readonly Action ReloadBonesNeeded;
        [CompilerGenerated]
        private Action<MyStringId> ActionTriggered;
        public float CameraDistance;
        private MyCharacterBone[] m_characterBonesSorted;
        private MyEntity m_entity;

        public event Action<MyStringId> ActionTriggered
        {
            [CompilerGenerated] add
            {
                Action<MyStringId> actionTriggered = this.ActionTriggered;
                while (true)
                {
                    Action<MyStringId> a = actionTriggered;
                    Action<MyStringId> action3 = (Action<MyStringId>) Delegate.Combine(a, value);
                    actionTriggered = Interlocked.CompareExchange<Action<MyStringId>>(ref this.ActionTriggered, action3, a);
                    if (ReferenceEquals(actionTriggered, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyStringId> actionTriggered = this.ActionTriggered;
                while (true)
                {
                    Action<MyStringId> source = actionTriggered;
                    Action<MyStringId> action3 = (Action<MyStringId>) Delegate.Remove(source, value);
                    actionTriggered = Interlocked.CompareExchange<Action<MyStringId>>(ref this.ActionTriggered, action3, source);
                    if (ReferenceEquals(actionTriggered, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyAnimationControllerComponent(MyEntity entity, Action obtainBones, IMyTerrainHeightProvider heightProvider)
        {
            this.m_entity = entity;
            this.ReloadBonesNeeded = obtainBones;
            this.m_controller.InverseKinematics.TerrainHeightProvider = heightProvider;
        }

        public void Clear()
        {
            bool inScene = this.m_entity.InScene;
            this.MarkAsInvalid();
            this.m_controller.InverseKinematics.Clear();
            this.m_controller.DeleteAllLayers();
            this.m_controller.Variables.Clear();
            this.m_controller.ResultBonesPool.Reset(null);
            this.m_characterBones = null;
            this.m_boneRelativeTransforms = null;
            this.m_boneAbsoluteTransforms = null;
            this.m_characterBonesSorted = null;
        }

        public MyCharacterBone FindBone(string name, out int index)
        {
            bool inScene = this.m_entity.InScene;
            if (name != null)
            {
                for (int i = 0; i < this.m_characterBones.Length; i++)
                {
                    if (this.m_characterBones[i].Name == name)
                    {
                        index = i;
                        return this.m_characterBones[i];
                    }
                }
            }
            index = -1;
            return null;
        }

        private void MarkAsInvalid()
        {
            this.m_componentValid = false;
        }

        public void MarkAsValid()
        {
            bool inScene = this.m_entity.InScene;
            this.m_componentValid = true;
        }

        public override void OnAddedToContainer()
        {
            bool inScene = this.m_entity.InScene;
            MySessionComponentAnimationSystem.Static.RegisterEntityComponent(this);
        }

        public override void OnBeforeRemovedFromContainer()
        {
            bool inScene = this.m_entity.InScene;
            MySessionComponentAnimationSystem.Static.UnregisterEntityComponent(this);
        }

        public void SetCharacterBones(MyCharacterBone[] characterBones, Matrix[] relativeTransforms, Matrix[] absoluteTransforms)
        {
            bool inScene = this.m_entity.InScene;
            this.m_characterBones = characterBones;
            this.m_characterBonesSorted = new MyCharacterBone[this.m_characterBones.Length];
            Array.Copy(this.m_characterBones, this.m_characterBonesSorted, this.m_characterBones.Length);
            Array.Sort<MyCharacterBone>(this.m_characterBonesSorted, (x, y) => x.Depth.CompareTo(y.Depth));
            this.m_boneRelativeTransforms = relativeTransforms;
            this.m_boneAbsoluteTransforms = absoluteTransforms;
            this.m_controller.ResultBonesPool.Reset(this.m_characterBones);
        }

        public void TriggerAction(MyStringId actionName)
        {
            bool inScene = this.m_entity.InScene;
            if (this.m_componentValid)
            {
                this.m_controller.TriggerAction(actionName);
                if (this.ActionTriggered != null)
                {
                    this.ActionTriggered(actionName);
                }
            }
        }

        public void Update()
        {
            if (this.CameraDistance <= 200f)
            {
                bool inScene = this.m_entity.InScene;
                if ((this.m_componentValid && (base.Entity is IMySkinnedEntity)) && base.Entity.InScene)
                {
                    (base.Entity as IMySkinnedEntity).UpdateAnimation(this.CameraDistance);
                    MyAnimationUpdateData animationUpdateData = new MyAnimationUpdateData {
                        DeltaTimeInSeconds = 0.01666666753590107,
                        CharacterBones = this.m_characterBones,
                        Controller = null,
                        BonesResult = null
                    };
                    this.m_controller.Update(ref animationUpdateData);
                    if (animationUpdateData.BonesResult != null)
                    {
                        for (int i = 0; i < animationUpdateData.BonesResult.Count; i++)
                        {
                            this.CharacterBones[i].SetCompleteTransform(ref animationUpdateData.BonesResult[i].Translation, ref animationUpdateData.BonesResult[i].Rotation);
                        }
                    }
                    this.m_lastBoneResult = animationUpdateData.BonesResult;
                }
            }
        }

        public void UpdateInverseKinematics()
        {
            bool inScene = this.m_entity.InScene;
            this.m_controller.UpdateInverseKinematics(ref this.m_characterBones);
        }

        public void UpdateTransformations()
        {
            if (this.m_characterBones != null)
            {
                bool inScene = this.m_entity.InScene;
                MyCharacterBone.ComputeAbsoluteTransforms(this.m_characterBonesSorted);
            }
        }

        public override string ComponentTypeDebugString =>
            "AnimationControllerComp";

        public MyAnimationController Controller
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_controller;
            }
        }

        public MyAnimationVariableStorage Variables
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_controller.Variables;
            }
        }

        public MyCharacterBone[] CharacterBones
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_characterBones;
            }
        }

        public MyAnimationInverseKinematics InverseKinematics
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_controller.InverseKinematics;
            }
        }

        public MyCharacterBone[] CharacterBonesSorted
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_characterBonesSorted;
            }
        }

        public Matrix[] BoneRelativeTransforms
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_boneRelativeTransforms;
            }
        }

        public Matrix[] BoneAbsoluteTransforms
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_boneAbsoluteTransforms;
            }
        }

        public List<MyAnimationClip.BoneState> LastRawBoneResult
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return this.m_lastBoneResult;
            }
        }

        public MyDefinitionId SourceId { get; set; }

        public List<MyStringId> LastFrameActions
        {
            get
            {
                bool inScene = this.m_entity.InScene;
                return null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAnimationControllerComponent.<>c <>9 = new MyAnimationControllerComponent.<>c();
            public static Comparison<MyCharacterBone> <>9__42_0;

            internal int <SetCharacterBones>b__42_0(MyCharacterBone x, MyCharacterBone y) => 
                x.Depth.CompareTo(y.Depth);
        }
    }
}

