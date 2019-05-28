namespace VRage.Game.Definitions.Animation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders;

    internal class MyAnimationControllerDefinitionPostprocess : MyDefinitionPostprocessor
    {
        public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
        {
            foreach (KeyValuePair<MyStringHash, MyDefinitionBase> pair in definitions.Definitions)
            {
                MyAnimationControllerDefinition definition = pair.Value as MyAnimationControllerDefinition;
                if ((definition != null) && ((definition.StateMachines != null) && (!pair.Value.Context.IsBaseGame && ((pair.Value.Context != null) && (pair.Value.Context.ModPath != null)))))
                {
                    using (List<MyObjectBuilder_AnimationSM>.Enumerator enumerator2 = definition.StateMachines.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            foreach (MyObjectBuilder_AnimationSMNode node in enumerator2.Current.Nodes)
                            {
                                if ((node.AnimationTree != null) && (node.AnimationTree.Child != null))
                                {
                                    this.ResolveMwmPaths(pair.Value.Context, node.AnimationTree.Child);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
        {
        }

        public override void OverrideBy(ref MyDefinitionPostprocessor.Bundle currentDefinitions, ref MyDefinitionPostprocessor.Bundle overrideBySet)
        {
            foreach (KeyValuePair<MyStringHash, MyDefinitionBase> pair in overrideBySet.Definitions)
            {
                MyAnimationControllerDefinition definition = pair.Value as MyAnimationControllerDefinition;
                if (pair.Value.Enabled && (definition != null))
                {
                    bool flag = true;
                    if (currentDefinitions.Definitions.ContainsKey(pair.Key))
                    {
                        MyAnimationControllerDefinition definition2 = currentDefinitions.Definitions[pair.Key] as MyAnimationControllerDefinition;
                        if (definition2 != null)
                        {
                            foreach (MyObjectBuilder_AnimationSM nsm in definition.StateMachines)
                            {
                                bool flag2 = false;
                                foreach (MyObjectBuilder_AnimationSM nsm2 in definition2.StateMachines)
                                {
                                    if (nsm.Name == nsm2.Name)
                                    {
                                        nsm2.Nodes = nsm.Nodes;
                                        nsm2.Transitions = nsm.Transitions;
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (!flag2)
                                {
                                    definition2.StateMachines.Add(nsm);
                                }
                            }
                            foreach (MyObjectBuilder_AnimationLayer layer in definition.Layers)
                            {
                                bool flag3 = false;
                                foreach (MyObjectBuilder_AnimationLayer layer2 in definition2.Layers)
                                {
                                    if (layer.Name == layer2.Name)
                                    {
                                        layer2.Name = layer.Name;
                                        layer2.BoneMask = layer.BoneMask;
                                        layer2.InitialSMNode = layer.InitialSMNode;
                                        layer2.StateMachine = layer.StateMachine;
                                        layer2.Mode = layer.Mode;
                                        flag3 = true;
                                    }
                                }
                                if (!flag3)
                                {
                                    definition2.Layers.Add(layer);
                                }
                            }
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        currentDefinitions.Definitions[pair.Key] = pair.Value;
                    }
                }
            }
        }

        private void ResolveMwmPaths(MyModContext modContext, MyObjectBuilder_AnimationTreeNode objBuilderNode)
        {
            MyObjectBuilder_AnimationTreeNodeTrack track = objBuilderNode as MyObjectBuilder_AnimationTreeNodeTrack;
            if ((track != null) && (track.PathToModel != null))
            {
                string path = Path.Combine(modContext.ModPath, track.PathToModel);
                if (MyFileSystem.FileExists(path))
                {
                    track.PathToModel = path;
                }
            }
            MyObjectBuilder_AnimationTreeNodeMix1D mixd = objBuilderNode as MyObjectBuilder_AnimationTreeNodeMix1D;
            if ((mixd != null) && (mixd.Children != null))
            {
                foreach (MyParameterAnimTreeNodeMapping mapping in mixd.Children)
                {
                    if (mapping.Node != null)
                    {
                        this.ResolveMwmPaths(modContext, mapping.Node);
                    }
                }
            }
            MyObjectBuilder_AnimationTreeNodeAdd add = objBuilderNode as MyObjectBuilder_AnimationTreeNodeAdd;
            if (add != null)
            {
                if (add.BaseNode.Node != null)
                {
                    this.ResolveMwmPaths(modContext, add.BaseNode.Node);
                }
                if (add.AddNode.Node != null)
                {
                    this.ResolveMwmPaths(modContext, add.AddNode.Node);
                }
            }
        }
    }
}

