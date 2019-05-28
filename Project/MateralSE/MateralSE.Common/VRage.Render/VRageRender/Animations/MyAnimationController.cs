namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimationController
    {
        private readonly List<MyAnimationStateMachine> m_layers = new List<MyAnimationStateMachine>(1);
        private readonly Dictionary<string, int> m_tableLayerNameToIndex = new Dictionary<string, int>(1);
        public readonly MyResultBonesPool ResultBonesPool;
        public readonly MyAnimationInverseKinematics InverseKinematics;

        public MyAnimationController()
        {
            this.Variables = new MyAnimationVariableStorage();
            this.ResultBonesPool = new MyResultBonesPool();
            this.InverseKinematics = new MyAnimationInverseKinematics();
            this.FrameCounter = 0;
            this.IkUpdateEnabled = true;
        }

        public MyAnimationStateMachine CreateLayer(string name, int insertionIndex = -1)
        {
            if (this.GetLayerByName(name) != null)
            {
                return null;
            }
            MyAnimationStateMachine item = new MyAnimationStateMachine {
                Name = name
            };
            if (insertionIndex != -1)
            {
                this.m_tableLayerNameToIndex.Add(name, insertionIndex);
                this.m_layers.Insert(insertionIndex, item);
            }
            else
            {
                this.m_tableLayerNameToIndex.Add(name, this.m_layers.Count);
                this.m_layers.Add(item);
            }
            return item;
        }

        public void DeleteAllLayers()
        {
            this.m_tableLayerNameToIndex.Clear();
            this.m_layers.Clear();
        }

        public MyAnimationStateMachine GetLayerByIndex(int index)
        {
            if ((index < 0) || (index >= this.m_layers.Count))
            {
                return null;
            }
            return this.m_layers[index];
        }

        public MyAnimationStateMachine GetLayerByName(string layerName)
        {
            int num;
            return (!this.m_tableLayerNameToIndex.TryGetValue(layerName, out num) ? null : this.m_layers[num]);
        }

        public int GetLayerCount() => 
            this.m_layers.Count;

        public void TriggerAction(MyStringId actionName)
        {
            using (List<MyAnimationStateMachine>.Enumerator enumerator = this.m_layers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.TriggerAction(actionName);
                }
            }
        }

        public void Update(ref MyAnimationUpdateData animationUpdateData)
        {
            int frameCounter = this.FrameCounter;
            this.FrameCounter = frameCounter + 1;
            if ((animationUpdateData.CharacterBones != null) && this.ResultBonesPool.IsValid())
            {
                if (animationUpdateData.Controller == null)
                {
                    animationUpdateData.Controller = this;
                }
                this.ResultBonesPool.FreeAll();
                this.Variables.SetValue(MyAnimationVariableStorageHints.StrIdRandomStable, MyRandom.Instance.NextFloat());
                if (this.m_layers.Count > 0)
                {
                    this.ResultBonesPool.SetDefaultPose(null);
                    this.m_layers[0].Update(ref animationUpdateData);
                }
                for (int i = 1; i < this.m_layers.Count; i++)
                {
                    MyAnimationStateMachine machine = this.m_layers[i];
                    MyAnimationUpdateData data = animationUpdateData;
                    animationUpdateData.LayerBoneMask = null;
                    animationUpdateData.BonesResult = null;
                    this.ResultBonesPool.SetDefaultPose((machine.Mode == MyAnimationStateMachine.MyBlendingMode.Replace) ? data.BonesResult : null);
                    this.m_layers[i].Update(ref animationUpdateData);
                    if (((animationUpdateData.BonesResult == null) || ((this.m_layers[i].CurrentNode == null) || (((MyAnimationStateMachineNode) this.m_layers[i].CurrentNode).RootAnimationNode == null))) || (((MyAnimationStateMachineNode) this.m_layers[i].CurrentNode).RootAnimationNode is MyAnimationTreeNodeDummy))
                    {
                        animationUpdateData = data;
                    }
                    else
                    {
                        int count = animationUpdateData.BonesResult.Count;
                        List<MyAnimationClip.BoneState> bonesResult = data.BonesResult;
                        List<MyAnimationClip.BoneState> list2 = animationUpdateData.BonesResult;
                        MyCharacterBone[] characterBones = data.CharacterBones;
                        if (machine.Mode == MyAnimationStateMachine.MyBlendingMode.Replace)
                        {
                            for (int j = 0; j < count; j++)
                            {
                                if (!animationUpdateData.LayerBoneMask[j])
                                {
                                    list2[j].Translation = bonesResult[j].Translation;
                                    list2[j].Rotation = bonesResult[j].Rotation;
                                }
                            }
                        }
                        else if (machine.Mode == MyAnimationStateMachine.MyBlendingMode.Add)
                        {
                            for (int j = 0; j < count; j++)
                            {
                                if (!animationUpdateData.LayerBoneMask[j])
                                {
                                    list2[j].Translation = bonesResult[j].Translation;
                                    list2[j].Rotation = bonesResult[j].Rotation;
                                }
                                else
                                {
                                    Vector3 vector;
                                    Quaternion quaternion;
                                    characterBones[j].GetCompleteTransform(ref list2[j].Translation, ref list2[j].Rotation, out vector, out quaternion);
                                    list2[j].Translation = bonesResult[j].Translation + Vector3.Transform(vector, bonesResult[j].Rotation);
                                    list2[j].Rotation = bonesResult[j].Rotation * quaternion;
                                }
                            }
                        }
                        this.ResultBonesPool.Free(data.BonesResult);
                    }
                }
            }
        }

        public void UpdateInverseKinematics(ref MyCharacterBone[] characterBonesStorage)
        {
            if ((this.Variables != null) && this.IkUpdateEnabled)
            {
                float num;
                float num2;
                float num3;
                float num4;
                float num5;
                float num6;
                float num7;
                float num8;
                float num9;
                int num1;
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdFlying, out num);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdFalling, out num2);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdDead, out num3);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdSitting, out num4);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdJumping, out num5);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdSpeed, out num6);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdFirstPerson, out num7);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdForcedFirstPerson, out num8);
                this.Variables.GetValue(MyAnimationVariableStorageHints.StrIdLadder, out num9);
                if (num6 < 0.25f)
                {
                    this.InverseKinematics.ClearCharacterOffsetFilteringSamples();
                }
                if (num4 > 0f)
                {
                    this.InverseKinematics.ResetIkInfluence();
                }
                if (((num > 0f) || ((num2 > 0f) || ((num3 > 0f) || (num5 > 0f)))) || (num4 > 0f))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !(num9 != 0f);
                }
                this.InverseKinematics.SolveFeet((bool) num1, characterBonesStorage, (num7 <= 0f) || (num8 > 0f));
            }
        }

        public int FrameCounter { get; private set; }

        private bool IkUpdateEnabled { get; set; }

        public MyAnimationVariableStorage Variables { get; private set; }

        public class MyResultBonesPool
        {
            private int m_boneCount;
            private readonly List<List<MyAnimationClip.BoneState>> m_freeToUse = new List<List<MyAnimationClip.BoneState>>(8);
            private readonly List<List<MyAnimationClip.BoneState>> m_taken = new List<List<MyAnimationClip.BoneState>>(8);
            private List<MyAnimationClip.BoneState> m_restPose;
            private List<MyAnimationClip.BoneState> m_currentDefaultPose;

            public List<MyAnimationClip.BoneState> Alloc()
            {
                if (this.m_freeToUse.Count != 0)
                {
                    List<MyAnimationClip.BoneState> item = this.m_freeToUse[this.m_freeToUse.Count - 1];
                    this.m_freeToUse.RemoveAt(this.m_freeToUse.Count - 1);
                    this.m_taken.Add(item);
                    for (int j = 0; j < this.m_boneCount; j++)
                    {
                        item[j].Translation = this.m_currentDefaultPose[j].Translation;
                        item[j].Rotation = this.m_currentDefaultPose[j].Rotation;
                    }
                    return item;
                }
                List<MyAnimationClip.BoneState> list = new List<MyAnimationClip.BoneState>(this.m_boneCount);
                list.SetSize<MyAnimationClip.BoneState>(this.m_boneCount);
                for (int i = 0; i < this.m_boneCount; i++)
                {
                    MyAnimationClip.BoneState state1 = new MyAnimationClip.BoneState();
                    state1.Translation = this.m_currentDefaultPose[i].Translation;
                    state1.Rotation = this.m_currentDefaultPose[i].Rotation;
                    list[i] = state1;
                }
                this.m_taken.Add(list);
                return list;
            }

            public void Free(List<MyAnimationClip.BoneState> toBeFreed)
            {
                int index = -1;
                int num2 = this.m_taken.Count - 1;
                while (true)
                {
                    if (num2 >= 0)
                    {
                        if (this.m_taken[num2] != toBeFreed)
                        {
                            num2--;
                            continue;
                        }
                        index = num2;
                    }
                    if (index != -1)
                    {
                        this.m_freeToUse.Add(this.m_taken[index]);
                        this.m_taken.RemoveAtFast<List<MyAnimationClip.BoneState>>(index);
                    }
                    return;
                }
            }

            public void FreeAll()
            {
                foreach (List<MyAnimationClip.BoneState> list in this.m_taken)
                {
                    this.m_freeToUse.Add(list);
                }
                this.m_taken.Clear();
            }

            public bool IsValid() => 
                (this.m_currentDefaultPose != null);

            public void Reset(MyCharacterBone[] restPoseBones)
            {
                this.m_freeToUse.Clear();
                this.m_taken.Clear();
                if (restPoseBones == null)
                {
                    this.m_boneCount = 0;
                    this.m_restPose = null;
                }
                else
                {
                    int length = restPoseBones.Length;
                    this.m_boneCount = length;
                    this.m_restPose = new List<MyAnimationClip.BoneState>(length);
                    for (int i = 0; i < length; i++)
                    {
                        Matrix bindTransform = restPoseBones[i].BindTransform;
                        MyAnimationClip.BoneState item = new MyAnimationClip.BoneState();
                        item.Translation = bindTransform.Translation;
                        item.Rotation = Quaternion.CreateFromRotationMatrix(restPoseBones[i].BindTransform);
                        this.m_restPose.Add(item);
                    }
                    this.m_currentDefaultPose = this.m_restPose;
                }
            }

            public void SetDefaultPose(List<MyAnimationClip.BoneState> linkToDefaultPose)
            {
                this.m_currentDefaultPose = linkToDefaultPose ?? this.m_restPose;
            }
        }
    }
}

