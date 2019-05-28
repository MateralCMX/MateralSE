namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Generics;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimationStateMachine : MySingleStateMachine
    {
        public MyAnimationUpdateData CurrentUpdateData;
        public MyBlendingMode Mode;
        public readonly HashSet<MyStringId> BoneMaskStrIds = new HashSet<MyStringId>();
        public bool[] BoneMask;
        private readonly List<MyStateTransitionBlending> m_stateTransitionBlending;
        private int[] m_lastVisitedTreeNodesPath;

        public MyAnimationStateMachine()
        {
            this.VisitedTreeNodesPath = new int[0x40];
            this.m_lastVisitedTreeNodesPath = new int[0x40];
            this.m_stateTransitionBlending = new List<MyStateTransitionBlending>();
            base.OnStateChanged += new MySingleStateMachine.StateChangedHandler(this.AnimationStateChanged);
        }

        private unsafe void AnimationStateChanged(MyStateMachineTransitionWithStart transitionWithStart, MyStringId action)
        {
            MyAnimationStateMachineTransition transition = transitionWithStart.Transition as MyAnimationStateMachineTransition;
            if (transition != null)
            {
                MyAnimationStateMachineNode startNode = transitionWithStart.StartNode as MyAnimationStateMachineNode;
                MyAnimationStateMachineNode targetNode = transition.TargetNode as MyAnimationStateMachineNode;
                if (startNode != null)
                {
                    if (targetNode != null)
                    {
                        this.AssignVariableValues(targetNode);
                        bool flag = false;
                        using (List<MyStateTransitionBlending>.Enumerator enumerator = this.m_stateTransitionBlending.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (ReferenceEquals(enumerator.Current.SourceState, targetNode))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (targetNode.RootAnimationNode != null)
                        {
                            targetNode.RootAnimationNode.SetAction(action);
                        }
                        switch (transition.Sync)
                        {
                            case MyAnimationTransitionSyncType.Restart:
                                if (targetNode.RootAnimationNode != null)
                                {
                                    targetNode.RootAnimationNode.SetLocalTimeNormalized(0f);
                                }
                                break;

                            case MyAnimationTransitionSyncType.Synchronize:
                                if ((!flag && (startNode.RootAnimationNode != null)) && (targetNode.RootAnimationNode != null))
                                {
                                    float localTimeNormalized = startNode.RootAnimationNode.GetLocalTimeNormalized();
                                    targetNode.RootAnimationNode.SetLocalTimeNormalized(localTimeNormalized);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    if (((transition.TransitionTimeInSec > 0.0) || transitionWithStart.Transition.TargetNode.PassThrough) && !transitionWithStart.StartNode.PassThrough)
                    {
                        MyStateTransitionBlending item = new MyStateTransitionBlending {
                            SourceState = startNode,
                            TimeLeftInSeconds = transition.TransitionTimeInSec,
                            InvTotalTime = 1.0 / transition.TransitionTimeInSec,
                            Curve = transition.Curve
                        };
                        if (!item.InvTotalTime.IsValid())
                        {
                            item.InvTotalTime = 1.0;
                        }
                        this.m_stateTransitionBlending.Insert(0, item);
                    }
                    else if (transitionWithStart.StartNode.PassThrough && (this.m_stateTransitionBlending.Count > 0))
                    {
                        MyStateTransitionBlending blending3 = this.m_stateTransitionBlending[0];
                        blending3.TimeLeftInSeconds = Math.Max(transition.TransitionTimeInSec, this.m_stateTransitionBlending[0].TimeLeftInSeconds);
                        MyStateTransitionBlending* blendingPtr1 = (MyStateTransitionBlending*) ref blending3;
                        blendingPtr1->InvTotalTime = 1.0 / blending3.TimeLeftInSeconds;
                        blending3.Curve = transition.Curve;
                        if (!blending3.InvTotalTime.IsValid())
                        {
                            blending3.InvTotalTime = 1.0;
                        }
                        this.m_stateTransitionBlending[0] = blending3;
                    }
                    else if (transition.TransitionTimeInSec <= 9.9999997473787516E-06)
                    {
                        this.m_stateTransitionBlending.Clear();
                    }
                }
            }
        }

        private void AssignVariableValues(MyAnimationStateMachineNode freshNode)
        {
            freshNode.VariableAssignments.ForEach(data => this.CurrentUpdateData.Controller.Variables.SetValue(data.VariableId, data.Value));
        }

        private static float ComputeEaseInEaseOut(float t, MyAnimationTransitionCurve curve) => 
            ((curve == MyAnimationTransitionCurve.Smooth) ? ((t * t) * (3f - (2f * t))) : ((curve == MyAnimationTransitionCurve.EaseIn) ? ((t * t) * t) : t));

        private void RebuildBoneMask()
        {
            if (this.CurrentUpdateData.CharacterBones != null)
            {
                this.BoneMask = new bool[this.CurrentUpdateData.CharacterBones.Length];
                if (this.BoneMaskStrIds.Count == 0)
                {
                    for (int i = 0; i < this.CurrentUpdateData.CharacterBones.Length; i++)
                    {
                        this.BoneMask[i] = true;
                    }
                }
                else
                {
                    for (int i = 0; i < this.CurrentUpdateData.CharacterBones.Length; i++)
                    {
                        MyStringId item = MyStringId.TryGet(this.CurrentUpdateData.CharacterBones[i].Name);
                        if ((item != MyStringId.NullOrEmpty) && this.BoneMaskStrIds.Contains(item))
                        {
                            this.BoneMask[i] = true;
                        }
                    }
                }
            }
        }

        public override string ToString() => 
            $"MyAnimationStateMachine, Name='{base.Name}', Mode='{this.Mode}'";

        public unsafe void Update(ref MyAnimationUpdateData data)
        {
            if (data.CharacterBones == null)
            {
                return;
            }
            this.CurrentUpdateData = data;
            this.CurrentUpdateData.VisitedTreeNodesCounter = 0;
            this.CurrentUpdateData.VisitedTreeNodesPath = this.m_lastVisitedTreeNodesPath;
            this.CurrentUpdateData.VisitedTreeNodesPath[0] = 0;
            if (this.BoneMask == null)
            {
                this.RebuildBoneMask();
            }
            data.LayerBoneMask = this.CurrentUpdateData.LayerBoneMask = this.BoneMask;
            MyAnimationStateMachineNode currentNode = base.CurrentNode as MyAnimationStateMachineNode;
            if ((currentNode == null) || (currentNode.RootAnimationNode == null))
            {
                data.Controller.Variables.SetValue(MyAnimationVariableStorageHints.StrIdAnimationFinished, 0f);
            }
            else
            {
                float localTimeNormalized = currentNode.RootAnimationNode.GetLocalTimeNormalized();
                data.Controller.Variables.SetValue(MyAnimationVariableStorageHints.StrIdAnimationFinished, localTimeNormalized);
            }
            base.Update();
            int[] visitedTreeNodesPath = this.VisitedTreeNodesPath;
            this.VisitedTreeNodesPath = this.m_lastVisitedTreeNodesPath;
            this.m_lastVisitedTreeNodesPath = visitedTreeNodesPath;
            this.CurrentUpdateData.VisitedTreeNodesPath = null;
            float num = 1f;
            int num3 = 0;
            while (true)
            {
                if (num3 < this.m_stateTransitionBlending.Count)
                {
                    MyStateTransitionBlending blending = this.m_stateTransitionBlending[num3];
                    float num4 = (float) (blending.TimeLeftInSeconds * blending.InvTotalTime);
                    num *= num4;
                    if (num > 0f)
                    {
                        List<MyAnimationClip.BoneState> bonesResult = this.CurrentUpdateData.BonesResult;
                        this.CurrentUpdateData.BonesResult = null;
                        blending.SourceState.OnUpdate(this);
                        if ((bonesResult != null) && (this.CurrentUpdateData.BonesResult != null))
                        {
                            int index = 0;
                            while (true)
                            {
                                if (index >= bonesResult.Count)
                                {
                                    data.Controller.ResultBonesPool.Free(bonesResult);
                                    break;
                                }
                                if (data.LayerBoneMask[index])
                                {
                                    float amount = ComputeEaseInEaseOut(MathHelper.Clamp(num, 0f, 1f), blending.Curve);
                                    this.CurrentUpdateData.BonesResult[index].Rotation = Quaternion.Slerp(bonesResult[index].Rotation, this.CurrentUpdateData.BonesResult[index].Rotation, amount);
                                    this.CurrentUpdateData.BonesResult[index].Translation = Vector3.Lerp(bonesResult[index].Translation, this.CurrentUpdateData.BonesResult[index].Translation, amount);
                                }
                                index++;
                            }
                        }
                    }
                    double* numPtr1 = (double*) ref blending.TimeLeftInSeconds;
                    numPtr1[0] -= data.DeltaTimeInSeconds;
                    this.m_stateTransitionBlending[num3] = blending;
                    if (blending.TimeLeftInSeconds <= 0.0)
                    {
                        break;
                    }
                    if (num <= 0f)
                    {
                        break;
                    }
                    num3++;
                    continue;
                }
                goto TR_0001;
            }
            for (int i = num3 + 1; i < this.m_stateTransitionBlending.Count; i++)
            {
                MyStateTransitionBlending blending2 = this.m_stateTransitionBlending[i];
                blending2.TimeLeftInSeconds = 0.0;
                this.m_stateTransitionBlending[i] = blending2;
            }
        TR_0001:
            this.m_stateTransitionBlending.RemoveAll(s => !(s.TimeLeftInSeconds != 0.0));
            data.BonesResult = this.CurrentUpdateData.BonesResult;
        }

        public int[] VisitedTreeNodesPath { get; private set; }

        public ListReader<MyStateTransitionBlending> StateTransitionBlending =>
            this.m_stateTransitionBlending;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAnimationStateMachine.<>c <>9 = new MyAnimationStateMachine.<>c();
            public static Predicate<MyAnimationStateMachine.MyStateTransitionBlending> <>9__15_0;

            internal bool <Update>b__15_0(MyAnimationStateMachine.MyStateTransitionBlending s) => 
                !(s.TimeLeftInSeconds != 0.0);
        }

        public enum MyBlendingMode
        {
            Replace,
            Add
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyStateTransitionBlending
        {
            public double TimeLeftInSeconds;
            public double InvTotalTime;
            public MyAnimationStateMachineNode SourceState;
            public MyAnimationTransitionCurve Curve;
        }
    }
}

