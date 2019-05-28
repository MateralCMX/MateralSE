namespace VRage.Game.SessionComponents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageRender.Animations;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 500)]
    public class MySessionComponentAnimationSystem : MySessionComponentBase
    {
        public static MySessionComponentAnimationSystem Static;
        private readonly HashSet<MyAnimationControllerComponent> m_skinnedEntityComponents = new HashSet<MyAnimationControllerComponent>();
        private readonly List<MyAnimationControllerComponent> m_skinnedEntityComponentsToAdd = new List<MyAnimationControllerComponent>(0x20);
        private readonly List<MyAnimationControllerComponent> m_skinnedEntityComponentsToRemove = new List<MyAnimationControllerComponent>(0x20);
        private readonly FastResourceLock m_lock = new FastResourceLock();
        private int m_debuggingSendNameCounter;
        private const int m_debuggingSendNameCounterMax = 60;
        private string m_debuggingLastNameSent;
        private readonly List<MyStateMachineNode> m_debuggingAnimControllerCurrentNodes = new List<MyStateMachineNode>();
        private readonly List<int[]> m_debuggingAnimControllerTreePath = new List<int[]>();
        public MyEntity EntitySelectedForDebug;

        private void LiveDebugging()
        {
            if ((base.Session != null) && (MySessionComponentExtDebug.Static != null))
            {
                MyEntity entity = this.EntitySelectedForDebug ?? ((base.Session.ControlledObject != null) ? (base.Session.ControlledObject.Entity as MyEntity) : null);
                if (entity != null)
                {
                    MyAnimationControllerComponent component = entity.Components.Get<MyAnimationControllerComponent>();
                    if ((component != null) && !component.SourceId.TypeId.IsNull)
                    {
                        this.m_debuggingSendNameCounter--;
                        if (component.SourceId.SubtypeName != this.m_debuggingLastNameSent)
                        {
                            this.m_debuggingSendNameCounter = 0;
                        }
                        if (this.m_debuggingSendNameCounter <= 0)
                        {
                            this.LiveDebugging_SendControllerNameToEditor(component.SourceId.SubtypeName);
                            this.m_debuggingSendNameCounter = 60;
                            this.m_debuggingLastNameSent = component.SourceId.SubtypeName;
                        }
                        this.LiveDebugging_SendAnimationStateChangesToEditor(component.Controller);
                    }
                }
            }
        }

        private static bool LiveDebugging_CompareAnimTreePathSeqs(int[] seq1, int[] seq2)
        {
            if (((seq1 == null) || (seq2 == null)) || (seq1.Length != seq2.Length))
            {
                return false;
            }
            for (int i = 0; i < seq1.Length; i++)
            {
                if (seq1[i] != seq2[i])
                {
                    return false;
                }
                if ((seq1[i] == 0) && (seq2[i] == 0))
                {
                    return true;
                }
            }
            return true;
        }

        private void LiveDebugging_ReceivedMessageHandler(MyExternalDebugStructures.CommonMsgHeader messageHeader, IntPtr messageData)
        {
            MyExternalDebugStructures.ACReloadInGameMsg msg;
            if (MyExternalDebugStructures.ReadMessageFromPtr<MyExternalDebugStructures.ACReloadInGameMsg>(ref messageHeader, messageData, out msg))
            {
                try
                {
                    MyObjectBuilder_Definitions definitions;
                    string aCContentAddress = msg.ACContentAddress;
                    string aCAddress = msg.ACAddress;
                    string aCName = msg.ACName;
                    if ((MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(aCAddress, out definitions) && (definitions.Definitions != null)) && (definitions.Definitions.Length != 0))
                    {
                        MyModContext modContext = new MyModContext();
                        modContext.Init("AnimationControllerDefinition", aCAddress, aCContentAddress);
                        MyAnimationControllerDefinition definition = new MyAnimationControllerDefinition();
                        definition.Init(definitions.Definitions[0], modContext);
                        MyStringHash orCompute = MyStringHash.GetOrCompute(aCName);
                        MyAnimationControllerDefinition definition2 = MyDefinitionManagerBase.Static.GetDefinition<MyAnimationControllerDefinition>(orCompute);
                        MyDefinitionPostprocessor postProcessor = MyDefinitionManagerBase.GetPostProcessor(typeof(MyObjectBuilder_AnimationControllerDefinition));
                        if (postProcessor != null)
                        {
                            MyDefinitionPostprocessor.Bundle bundle3 = new MyDefinitionPostprocessor.Bundle {
                                Context = MyModContext.BaseGame
                            };
                            Dictionary<MyStringHash, MyDefinitionBase> dictionary1 = new Dictionary<MyStringHash, MyDefinitionBase>();
                            dictionary1.Add(orCompute, definition2);
                            bundle3.Definitions = dictionary1;
                            bundle3.Set = new MyDefinitionSet();
                            MyDefinitionPostprocessor.Bundle currentDefinitions = bundle3;
                            currentDefinitions.Set.AddDefinition(definition2);
                            bundle3 = new MyDefinitionPostprocessor.Bundle {
                                Context = modContext
                            };
                            Dictionary<MyStringHash, MyDefinitionBase> dictionary2 = new Dictionary<MyStringHash, MyDefinitionBase>();
                            dictionary2.Add(orCompute, definition);
                            bundle3.Definitions = dictionary2;
                            bundle3.Set = new MyDefinitionSet();
                            MyDefinitionPostprocessor.Bundle bundle2 = bundle3;
                            bundle2.Set.AddDefinition(definition);
                            postProcessor.AfterLoaded(ref bundle2);
                            postProcessor.OverrideBy(ref currentDefinitions, ref bundle2);
                        }
                        foreach (MyAnimationControllerComponent component in this.m_skinnedEntityComponents)
                        {
                            if (component == null)
                            {
                                continue;
                            }
                            if (component.SourceId.SubtypeName == aCName)
                            {
                                component.Clear();
                                component.InitFromDefinition(definition2, true);
                                if (component.ReloadBonesNeeded != null)
                                {
                                    component.ReloadBonesNeeded();
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        private void LiveDebugging_SendAnimationStateChangesToEditor(MyAnimationController animController)
        {
            if (animController != null)
            {
                int layerCount = animController.GetLayerCount();
                if (layerCount != this.m_debuggingAnimControllerCurrentNodes.Count)
                {
                    this.m_debuggingAnimControllerCurrentNodes.Clear();
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= layerCount)
                        {
                            this.m_debuggingAnimControllerTreePath.Clear();
                            for (int j = 0; j < layerCount; j++)
                            {
                                this.m_debuggingAnimControllerTreePath.Add(new int[animController.GetLayerByIndex(j).VisitedTreeNodesPath.Length]);
                            }
                            break;
                        }
                        this.m_debuggingAnimControllerCurrentNodes.Add(null);
                        num2++;
                    }
                }
                for (int i = 0; i < layerCount; i++)
                {
                    int[] visitedTreeNodesPath = animController.GetLayerByIndex(i).VisitedTreeNodesPath;
                    if ((animController.GetLayerByIndex(i).CurrentNode != this.m_debuggingAnimControllerCurrentNodes[i]) || !LiveDebugging_CompareAnimTreePathSeqs(visitedTreeNodesPath, this.m_debuggingAnimControllerTreePath[i]))
                    {
                        Array.Copy(visitedTreeNodesPath, this.m_debuggingAnimControllerTreePath[i], visitedTreeNodesPath.Length);
                        this.m_debuggingAnimControllerCurrentNodes[i] = animController.GetLayerByIndex(i).CurrentNode;
                        if (this.m_debuggingAnimControllerCurrentNodes[i] != null)
                        {
                            MyExternalDebugStructures.ACSendStateToEditorMsg msg = MyExternalDebugStructures.ACSendStateToEditorMsg.Create(this.m_debuggingAnimControllerCurrentNodes[i].Name, this.m_debuggingAnimControllerTreePath[i]);
                            MySessionComponentExtDebug.Static.SendMessageToClients<MyExternalDebugStructures.ACSendStateToEditorMsg>(msg);
                        }
                    }
                }
            }
        }

        private void LiveDebugging_SendControllerNameToEditor(string subtypeName)
        {
            MyExternalDebugStructures.ACConnectToEditorMsg msg = new MyExternalDebugStructures.ACConnectToEditorMsg {
                ACName = subtypeName
            };
            MySessionComponentExtDebug.Static.SendMessageToClients<MyExternalDebugStructures.ACConnectToEditorMsg>(msg);
        }

        public override void LoadData()
        {
            this.EntitySelectedForDebug = null;
            this.m_skinnedEntityComponents.Clear();
            this.m_skinnedEntityComponentsToAdd.Clear();
            this.m_skinnedEntityComponentsToRemove.Clear();
            Static = this;
            if (!MySessionComponentExtDebug.Static.IsHandlerRegistered(new MySessionComponentExtDebug.ReceivedMsgHandler(this.LiveDebugging_ReceivedMessageHandler)))
            {
                MySessionComponentExtDebug.Static.ReceivedMsg += new MySessionComponentExtDebug.ReceivedMsgHandler(this.LiveDebugging_ReceivedMessageHandler);
            }
            MyAnimationTreeNodeDynamicTrack.OnAction = (Func<MyStringId, MyAnimationTreeNodeDynamicTrack.DynamicTrackData>) Delegate.Combine(MyAnimationTreeNodeDynamicTrack.OnAction, new Func<MyStringId, MyAnimationTreeNodeDynamicTrack.DynamicTrackData>(this.OnDynamicTrackAction));
        }

        private MyAnimationTreeNodeDynamicTrack.DynamicTrackData OnDynamicTrackAction(MyStringId action)
        {
            MyAnimationTreeNodeDynamicTrack.DynamicTrackData data = new MyAnimationTreeNodeDynamicTrack.DynamicTrackData();
            MyAnimationDefinition definition = MyDefinitionManagerBase.Static.GetDefinition<MyAnimationDefinition>(action.ToString());
            if (definition != null)
            {
                string animationModel = definition.AnimationModel;
                if (string.IsNullOrEmpty(definition.AnimationModel))
                {
                    return data;
                }
                if (!MyFileSystem.FileExists(Path.IsPathRooted(animationModel) ? animationModel : Path.Combine(MyFileSystem.ContentPath, animationModel)))
                {
                    definition.Status = MyAnimationDefinition.AnimationStatus.Failed;
                    return data;
                }
                MyModel modelOnlyAnimationData = MyModels.GetModelOnlyAnimationData(animationModel, false);
                if (((modelOnlyAnimationData != null) && (modelOnlyAnimationData.Animations == null)) || (modelOnlyAnimationData.Animations.Clips.Count == 0))
                {
                    return data;
                }
                data.Clip = modelOnlyAnimationData.Animations.Clips[definition.ClipIndex];
                data.Loop = definition.Loop;
            }
            return data;
        }

        internal void RegisterEntityComponent(MyAnimationControllerComponent entityComponent)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_skinnedEntityComponentsToAdd.Add(entityComponent);
            }
        }

        public void ReloadMwmTracks()
        {
            foreach (MyAnimationControllerComponent component in this.m_skinnedEntityComponents)
            {
                MyDefinitionId sourceId = component.SourceId;
                MyAnimationControllerDefinition animControllerDefinition = MyDefinitionManagerBase.Static.GetDefinition<MyAnimationControllerDefinition>(MyStringHash.GetOrCompute(sourceId.SubtypeName));
                if (animControllerDefinition != null)
                {
                    component.Clear();
                    component.InitFromDefinition(animControllerDefinition, true);
                    if (component.ReloadBonesNeeded != null)
                    {
                        component.ReloadBonesNeeded();
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            this.EntitySelectedForDebug = null;
            this.m_skinnedEntityComponents.Clear();
            this.m_skinnedEntityComponentsToAdd.Clear();
            this.m_skinnedEntityComponentsToRemove.Clear();
            MyAnimationTreeNodeDynamicTrack.OnAction = (Func<MyStringId, MyAnimationTreeNodeDynamicTrack.DynamicTrackData>) Delegate.Remove(MyAnimationTreeNodeDynamicTrack.OnAction, new Func<MyStringId, MyAnimationTreeNodeDynamicTrack.DynamicTrackData>(this.OnDynamicTrackAction));
        }

        internal void UnregisterEntityComponent(MyAnimationControllerComponent entityComponent)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_skinnedEntityComponentsToRemove.Add(entityComponent);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            using (this.m_lock.AcquireExclusiveUsing())
            {
                foreach (MyAnimationControllerComponent component in this.m_skinnedEntityComponentsToRemove)
                {
                    if (this.m_skinnedEntityComponents.Contains(component))
                    {
                        this.m_skinnedEntityComponents.Remove(component);
                        this.m_skinnedEntityComponentsToAdd.Remove(component);
                    }
                }
                this.m_skinnedEntityComponentsToRemove.Clear();
                foreach (MyAnimationControllerComponent component2 in this.m_skinnedEntityComponentsToAdd)
                {
                    this.m_skinnedEntityComponents.Add(component2);
                }
                this.m_skinnedEntityComponentsToAdd.Clear();
            }
            using (HashSet<MyAnimationControllerComponent>.Enumerator enumerator2 = this.m_skinnedEntityComponents.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.Update();
                }
            }
            this.LiveDebugging();
        }

        public override void UpdateBeforeSimulation()
        {
        }

        public IEnumerable<MyAnimationControllerComponent> RegisteredAnimationComponents =>
            this.m_skinnedEntityComponents;
    }
}

