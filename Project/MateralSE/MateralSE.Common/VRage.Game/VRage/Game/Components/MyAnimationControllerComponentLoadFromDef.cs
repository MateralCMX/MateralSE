namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.Animation;
    using VRage.Generics.StateMachine;
    using VRage.Utils;
    using VRageRender.Animations;

    public static class MyAnimationControllerComponentLoadFromDef
    {
        private static readonly char[] m_boneListSeparators = new char[] { ' ' };

        private static MyCondition<float>.MyOperation ConvertOperation(MyObjectBuilder_AnimationSMCondition.MyOperationType operation)
        {
            switch (operation)
            {
                case MyObjectBuilder_AnimationSMCondition.MyOperationType.AlwaysFalse:
                    return MyCondition<float>.MyOperation.AlwaysFalse;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.AlwaysTrue:
                    return MyCondition<float>.MyOperation.AlwaysTrue;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.NotEqual:
                    return MyCondition<float>.MyOperation.NotEqual;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.Less:
                    return MyCondition<float>.MyOperation.Less;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.LessOrEqual:
                    return MyCondition<float>.MyOperation.LessOrEqual;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.Equal:
                    return MyCondition<float>.MyOperation.Equal;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.GreaterOrEqual:
                    return MyCondition<float>.MyOperation.GreaterOrEqual;

                case MyObjectBuilder_AnimationSMCondition.MyOperationType.Greater:
                    return MyCondition<float>.MyOperation.Greater;
            }
            return MyCondition<float>.MyOperation.AlwaysFalse;
        }

        private static void CreateTransition(MyAnimationStateMachine layer, MyAnimationController animationController, string absoluteNameNodeFrom, string absoluteNameNodeTo, MyObjectBuilder_AnimationSMTransition objBuilderTransition)
        {
            int index = 0;
            while (true)
            {
                MyAnimationStateMachineTransition transition = layer.AddTransition(absoluteNameNodeFrom, absoluteNameNodeTo, new MyAnimationStateMachineTransition(), null) as MyAnimationStateMachineTransition;
                if (transition != null)
                {
                    transition.Name = MyStringId.GetOrCompute(objBuilderTransition.Name?.ToLower());
                    transition.TransitionTimeInSec = objBuilderTransition.TimeInSec;
                    transition.Sync = objBuilderTransition.Sync;
                    transition.Curve = objBuilderTransition.Curve;
                    transition.Priority = objBuilderTransition.Priority;
                    if ((objBuilderTransition.Conditions != null) && (objBuilderTransition.Conditions[index] != null))
                    {
                        foreach (MyObjectBuilder_AnimationSMCondition condition in objBuilderTransition.Conditions[index].Conditions)
                        {
                            MyCondition<float> item = ParseOneCondition(animationController, condition);
                            if (item != null)
                            {
                                transition.Conditions.Add(item);
                            }
                        }
                    }
                }
                index++;
                if ((objBuilderTransition.Conditions == null) || (index >= objBuilderTransition.Conditions.Length))
                {
                    return;
                }
            }
        }

        public static bool InitFromDefinition(this MyAnimationControllerComponent thisController, MyAnimationControllerDefinition animControllerDefinition, bool forceReloadMwm = false)
        {
            bool flag = true;
            thisController.Clear();
            thisController.SourceId = animControllerDefinition.Id;
            foreach (MyObjectBuilder_AnimationLayer layer in animControllerDefinition.Layers)
            {
                MyAnimationStateMachine machine = thisController.Controller.CreateLayer(layer.Name, -1);
                if (machine != null)
                {
                    MyObjectBuilder_AnimationLayer.MyLayerMode mode = layer.Mode;
                    machine.Mode = (mode == MyObjectBuilder_AnimationLayer.MyLayerMode.Replace) ? MyAnimationStateMachine.MyBlendingMode.Replace : ((mode != MyObjectBuilder_AnimationLayer.MyLayerMode.Add) ? MyAnimationStateMachine.MyBlendingMode.Replace : MyAnimationStateMachine.MyBlendingMode.Add);
                    if (layer.BoneMask == null)
                    {
                        machine.BoneMaskStrIds.Clear();
                    }
                    else
                    {
                        foreach (string str in layer.BoneMask.Split(m_boneListSeparators))
                        {
                            machine.BoneMaskStrIds.Add(MyStringId.GetOrCompute(str));
                        }
                    }
                    machine.BoneMask = null;
                    MyAnimationVirtualNodes virtualNodes = new MyAnimationVirtualNodes();
                    flag = InitLayerNodes(machine, layer.StateMachine, animControllerDefinition, thisController.Controller, machine.Name + "/", virtualNodes, forceReloadMwm) & flag;
                    machine.SetState(machine.Name + "/" + layer.InitialSMNode);
                    if ((machine.ActiveCursors.Count > 0) && (machine.ActiveCursors[0].Node != null))
                    {
                        MyAnimationStateMachineNode node = machine.ActiveCursors[0].Node as MyAnimationStateMachineNode;
                        if (node != null)
                        {
                            foreach (MyAnimationStateMachineNode.VarAssignmentData data in node.VariableAssignments)
                            {
                                thisController.Controller.Variables.SetValue(data.VariableId, data.Value);
                            }
                        }
                    }
                    machine.SortTransitions();
                }
            }
            foreach (MyObjectBuilder_AnimationFootIkChain chain in animControllerDefinition.FootIkChains)
            {
                thisController.InverseKinematics.RegisterFootBone(chain.FootBone, chain.ChainLength, chain.AlignBoneWithTerrain);
            }
            foreach (string str2 in animControllerDefinition.IkIgnoredBones)
            {
                thisController.InverseKinematics.RegisterIgnoredBone(str2);
            }
            if (flag)
            {
                thisController.MarkAsValid();
            }
            return flag;
        }

        private static bool InitLayerNodes(MyAnimationStateMachine layer, string stateMachineName, MyAnimationControllerDefinition animControllerDefinition, MyAnimationController animationController, string currentNodeNamePrefix, MyAnimationVirtualNodes virtualNodes, bool forceReloadMwm)
        {
            MyObjectBuilder_AnimationSM nsm = animControllerDefinition.StateMachines.FirstOrDefault<MyObjectBuilder_AnimationSM>(x => x.Name == stateMachineName);
            if (nsm == null)
            {
                return false;
            }
            bool flag = true;
            if (nsm.Nodes != null)
            {
                foreach (MyObjectBuilder_AnimationSMNode node in nsm.Nodes)
                {
                    string name = currentNodeNamePrefix + node.Name;
                    if (node.StateMachineName != null)
                    {
                        if (!InitLayerNodes(layer, node.StateMachineName, animControllerDefinition, animationController, name + "/", virtualNodes, forceReloadMwm))
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        MyAnimationStateMachineNode newNode = new MyAnimationStateMachineNode(name);
                        if (((node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.PassThrough) || (node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.Any)) || (node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.AnyExceptTarget))
                        {
                            newNode.PassThrough = true;
                        }
                        else
                        {
                            newNode.PassThrough = false;
                        }
                        if ((node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.Any) || (node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.AnyExceptTarget))
                        {
                            MyAnimationVirtualNodeData data = new MyAnimationVirtualNodeData {
                                AnyNodePrefix = currentNodeNamePrefix,
                                ExceptTarget = node.Type == MyObjectBuilder_AnimationSMNode.MySMNodeType.AnyExceptTarget
                            };
                            virtualNodes.NodesAny.Add(name, data);
                        }
                        layer.AddNode(newNode);
                        if (node.AnimationTree != null)
                        {
                            newNode.RootAnimationNode = InitNodeAnimationTree(node.AnimationTree.Child, forceReloadMwm);
                        }
                        else
                        {
                            newNode.RootAnimationNode = new MyAnimationTreeNodeDummy();
                        }
                        if (node.Variables != null)
                        {
                            newNode.VariableAssignments = new List<MyAnimationStateMachineNode.VarAssignmentData>(from builder in node.Variables select new MyAnimationStateMachineNode.VarAssignmentData { 
                                VariableId = MyStringId.GetOrCompute(builder.Name),
                                Value = builder.Value
                            });
                        }
                    }
                }
            }
            if (nsm.Transitions != null)
            {
                foreach (MyObjectBuilder_AnimationSMTransition transition in nsm.Transitions)
                {
                    MyAnimationVirtualNodeData data2;
                    string key = currentNodeNamePrefix + transition.From;
                    string absoluteNameNodeTo = currentNodeNamePrefix + transition.To;
                    if (virtualNodes.NodesAny.TryGetValue(key, out data2))
                    {
                        foreach (KeyValuePair<string, MyStateMachineNode> pair in layer.AllNodes)
                        {
                            if (!pair.Key.StartsWith(data2.AnyNodePrefix))
                            {
                                continue;
                            }
                            if ((pair.Key != key) && (!data2.ExceptTarget || (absoluteNameNodeTo != pair.Key)))
                            {
                                CreateTransition(layer, animationController, pair.Key, absoluteNameNodeTo, transition);
                            }
                        }
                    }
                    CreateTransition(layer, animationController, key, absoluteNameNodeTo, transition);
                }
            }
            return flag;
        }

        private static MyAnimationTreeNode InitNodeAnimationTree(MyObjectBuilder_AnimationTreeNode objBuilderNode, bool forceReloadMwm)
        {
            MyObjectBuilder_AnimationTreeNodeDynamicTrack track = objBuilderNode as MyObjectBuilder_AnimationTreeNodeDynamicTrack;
            if (track != null)
            {
                MyAnimationTreeNodeDynamicTrack track1 = new MyAnimationTreeNodeDynamicTrack();
                track1.Loop = track.Loop;
                track1.Speed = track.Speed;
                track1.DefaultAnimation = MyStringId.GetOrCompute(track.DefaultAnimation);
                track1.Interpolate = track.Interpolate;
                track1.SynchronizeWithLayer = track.SynchronizeWithLayer;
                return track1;
            }
            MyObjectBuilder_AnimationTreeNodeTrack objBuilderNodeTrack = objBuilderNode as MyObjectBuilder_AnimationTreeNodeTrack;
            if (objBuilderNodeTrack != null)
            {
                MyAnimationTreeNodeTrack track2 = new MyAnimationTreeNodeTrack();
                MyModel model = (objBuilderNodeTrack.PathToModel != null) ? MyModels.GetModelOnlyAnimationData(objBuilderNodeTrack.PathToModel, forceReloadMwm) : null;
                if (((model == null) || ((model.Animations == null) || (model.Animations.Clips == null))) || (model.Animations.Clips.Count <= 0))
                {
                    if (objBuilderNodeTrack.PathToModel != null)
                    {
                        object[] args = new object[] { objBuilderNodeTrack.PathToModel };
                        MyLog.Default.Log(MyLogSeverity.Error, "Cannot load MWM track {0}.", args);
                    }
                }
                else
                {
                    MyAnimationClip animationClip = model.Animations.Clips.FirstOrDefault<MyAnimationClip>(clipItem => (clipItem.Name == objBuilderNodeTrack.AnimationName)) ?? model.Animations.Clips[0];
                    MyAnimationClip clip1 = animationClip;
                    track2.SetClip(animationClip);
                    track2.Loop = objBuilderNodeTrack.Loop;
                    track2.Speed = objBuilderNodeTrack.Speed;
                    track2.Interpolate = objBuilderNodeTrack.Interpolate;
                    track2.SynchronizeWithLayer = objBuilderNodeTrack.SynchronizeWithLayer;
                }
                return track2;
            }
            MyObjectBuilder_AnimationTreeNodeMix1D mixd = objBuilderNode as MyObjectBuilder_AnimationTreeNodeMix1D;
            if (mixd == null)
            {
                MyObjectBuilder_AnimationTreeNodeAdd add1 = objBuilderNode as MyObjectBuilder_AnimationTreeNodeAdd;
                return null;
            }
            MyAnimationTreeNodeMix1D mixd2 = new MyAnimationTreeNodeMix1D();
            if (mixd.Children != null)
            {
                MyParameterAnimTreeNodeMapping[] children = mixd.Children;
                int index = 0;
                while (true)
                {
                    if (index >= children.Length)
                    {
                        mixd2.ChildMappings.Sort((x, y) => x.ParamValueBinding.CompareTo(y.ParamValueBinding));
                        break;
                    }
                    MyParameterAnimTreeNodeMapping mapping = children[index];
                    MyAnimationTreeNodeMix1D.MyParameterNodeMapping item = new MyAnimationTreeNodeMix1D.MyParameterNodeMapping {
                        ParamValueBinding = mapping.Param,
                        Child = InitNodeAnimationTree(mapping.Node, forceReloadMwm)
                    };
                    mixd2.ChildMappings.Add(item);
                    index++;
                }
            }
            mixd2.ParameterName = MyStringId.GetOrCompute(mixd.ParameterName);
            mixd2.Circular = mixd.Circular;
            mixd2.Sensitivity = mixd.Sensitivity;
            float? maxChange = mixd.MaxChange;
            mixd2.MaxChange = (maxChange != null) ? maxChange.GetValueOrDefault() : float.PositiveInfinity;
            if (mixd2.MaxChange <= 0f)
            {
                mixd2.MaxChange = float.PositiveInfinity;
            }
            return mixd2;
        }

        private static MyCondition<float> ParseOneCondition(MyAnimationController animationController, MyObjectBuilder_AnimationSMCondition objBuilderCondition)
        {
            MyCondition<float> condition;
            double num;
            double num2;
            objBuilderCondition.ValueLeft = (objBuilderCondition.ValueLeft != null) ? objBuilderCondition.ValueLeft.ToLower() : "0";
            objBuilderCondition.ValueRight = (objBuilderCondition.ValueRight != null) ? objBuilderCondition.ValueRight.ToLower() : "0";
            if (double.TryParse(objBuilderCondition.ValueLeft, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
            {
                if (double.TryParse(objBuilderCondition.ValueRight, NumberStyles.Float, CultureInfo.InvariantCulture, out num2))
                {
                    condition = new MyCondition<float>(animationController.Variables, ConvertOperation(objBuilderCondition.Operation), (float) num, (float) num2);
                }
                else
                {
                    condition = new MyCondition<float>(animationController.Variables, ConvertOperation(objBuilderCondition.Operation), (float) num, objBuilderCondition.ValueRight);
                    MyStringId orCompute = MyStringId.GetOrCompute(objBuilderCondition.ValueRight);
                    if (!animationController.Variables.AllVariables.ContainsKey(orCompute))
                    {
                        animationController.Variables.SetValue(orCompute, 0f);
                    }
                }
            }
            else if (double.TryParse(objBuilderCondition.ValueRight, NumberStyles.Float, CultureInfo.InvariantCulture, out num2))
            {
                condition = new MyCondition<float>(animationController.Variables, ConvertOperation(objBuilderCondition.Operation), objBuilderCondition.ValueLeft, (float) num2);
                MyStringId orCompute = MyStringId.GetOrCompute(objBuilderCondition.ValueLeft);
                if (!animationController.Variables.AllVariables.ContainsKey(orCompute))
                {
                    animationController.Variables.SetValue(orCompute, 0f);
                }
            }
            else
            {
                condition = new MyCondition<float>(animationController.Variables, ConvertOperation(objBuilderCondition.Operation), objBuilderCondition.ValueLeft, objBuilderCondition.ValueRight);
                MyStringId orCompute = MyStringId.GetOrCompute(objBuilderCondition.ValueLeft);
                MyStringId key = MyStringId.GetOrCompute(objBuilderCondition.ValueRight);
                if (!animationController.Variables.AllVariables.ContainsKey(orCompute))
                {
                    animationController.Variables.SetValue(orCompute, 0f);
                }
                if (!animationController.Variables.AllVariables.ContainsKey(key))
                {
                    animationController.Variables.SetValue(key, 0f);
                }
            }
            return condition;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAnimationControllerComponentLoadFromDef.<>c <>9 = new MyAnimationControllerComponentLoadFromDef.<>c();
            public static Func<MyObjectBuilder_AnimationSMVariable, MyAnimationStateMachineNode.VarAssignmentData> <>9__4_1;
            public static Comparison<MyAnimationTreeNodeMix1D.MyParameterNodeMapping> <>9__7_1;

            internal MyAnimationStateMachineNode.VarAssignmentData <InitLayerNodes>b__4_1(MyObjectBuilder_AnimationSMVariable builder) => 
                new MyAnimationStateMachineNode.VarAssignmentData { 
                    VariableId = MyStringId.GetOrCompute(builder.Name),
                    Value = builder.Value
                };

            internal int <InitNodeAnimationTree>b__7_1(MyAnimationTreeNodeMix1D.MyParameterNodeMapping x, MyAnimationTreeNodeMix1D.MyParameterNodeMapping y) => 
                x.ParamValueBinding.CompareTo(y.ParamValueBinding);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyAnimationVirtualNodeData
        {
            public bool ExceptTarget;
            public string AnyNodePrefix;
        }

        private class MyAnimationVirtualNodes
        {
            public readonly Dictionary<string, MyAnimationControllerComponentLoadFromDef.MyAnimationVirtualNodeData> NodesAny = new Dictionary<string, MyAnimationControllerComponentLoadFromDef.MyAnimationVirtualNodeData>();
        }
    }
}

