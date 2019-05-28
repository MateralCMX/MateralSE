namespace VRage.Game.VisualScripting.Missions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.Game.VisualScripting.ScriptBuilder;
    using VRage.Generics;
    using VRage.Utils;

    public class MyVSStateMachine : MyStateMachine
    {
        private MyObjectBuilder_ScriptSM m_objectBuilder;
        private readonly MyConcurrentHashSet<MyStringId> m_cachedActions = new MyConcurrentHashSet<MyStringId>();
        private readonly List<MyStateMachineCursor> m_cursorsToInit = new List<MyStateMachineCursor>();
        private readonly List<MyStateMachineCursor> m_cursorsToDeserialize = new List<MyStateMachineCursor>();
        [CompilerGenerated]
        private Action<MyVSStateMachineNode, MyVSStateMachineNode> CursorStateChanged;
        private long m_ownerId;

        public event Action<MyVSStateMachineNode, MyVSStateMachineNode> CursorStateChanged
        {
            [CompilerGenerated] add
            {
                Action<MyVSStateMachineNode, MyVSStateMachineNode> cursorStateChanged = this.CursorStateChanged;
                while (true)
                {
                    Action<MyVSStateMachineNode, MyVSStateMachineNode> a = cursorStateChanged;
                    Action<MyVSStateMachineNode, MyVSStateMachineNode> action3 = (Action<MyVSStateMachineNode, MyVSStateMachineNode>) Delegate.Combine(a, value);
                    cursorStateChanged = Interlocked.CompareExchange<Action<MyVSStateMachineNode, MyVSStateMachineNode>>(ref this.CursorStateChanged, action3, a);
                    if (ReferenceEquals(cursorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyVSStateMachineNode, MyVSStateMachineNode> cursorStateChanged = this.CursorStateChanged;
                while (true)
                {
                    Action<MyVSStateMachineNode, MyVSStateMachineNode> source = cursorStateChanged;
                    Action<MyVSStateMachineNode, MyVSStateMachineNode> action3 = (Action<MyVSStateMachineNode, MyVSStateMachineNode>) Delegate.Remove(source, value);
                    cursorStateChanged = Interlocked.CompareExchange<Action<MyVSStateMachineNode, MyVSStateMachineNode>>(ref this.CursorStateChanged, action3, source);
                    if (ReferenceEquals(cursorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void ApplyChangesToCursors()
        {
            base.m_activeCursors.ApplyChanges();
        }

        public override MyStateMachineCursor CreateCursor(string nodeName)
        {
            using (Dictionary<int, MyStateMachineCursor>.ValueCollection.Enumerator enumerator = base.m_activeCursorsById.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.Node.Name == nodeName)
                    {
                        return null;
                    }
                }
            }
            MyStateMachineCursor item = base.CreateCursor(nodeName);
            if (item != null)
            {
                item.OnCursorStateChanged += new VRage.Generics.MyStateMachineCursor.CursorStateChanged(this.OnCursorStateChanged);
                if (item.Node is MyVSStateMachineNode)
                {
                    this.m_cursorsToInit.Add(item);
                }
            }
            return item;
        }

        public void Dispose()
        {
            base.m_activeCursors.ApplyChanges();
            for (int i = 0; i < base.m_activeCursors.Count; i++)
            {
                MyVSStateMachineNode node = base.m_activeCursors[i].Node as MyVSStateMachineNode;
                if (node != null)
                {
                    node.DisposeScript();
                }
                this.DeleteCursor(base.m_activeCursors[i].Id);
            }
            base.m_activeCursors.ApplyChanges();
            base.m_activeCursors.Clear();
        }

        public MyObjectBuilder_ScriptSM GetObjectBuilder()
        {
            this.m_objectBuilder.Cursors = new MyObjectBuilder_ScriptSMCursor[base.m_activeCursors.Count];
            this.m_objectBuilder.OwnerId = this.m_ownerId;
            for (int i = 0; i < base.m_activeCursors.Count; i++)
            {
                MyObjectBuilder_ScriptSMCursor cursor1 = new MyObjectBuilder_ScriptSMCursor();
                cursor1.NodeName = base.m_activeCursors[i].Node.Name;
                this.m_objectBuilder.Cursors[i] = cursor1;
            }
            return this.m_objectBuilder;
        }

        public void Init(MyObjectBuilder_ScriptSM ob, long? ownerId = new long?())
        {
            this.m_objectBuilder = ob;
            base.Name = ob.Name;
            if (ob.Nodes != null)
            {
                foreach (MyObjectBuilder_ScriptSMNode node in ob.Nodes)
                {
                    MyStateMachineNode node2;
                    if (node is MyObjectBuilder_ScriptSMFinalNode)
                    {
                        node2 = new MyVSStateMachineFinalNode(node.Name);
                    }
                    else if (node is MyObjectBuilder_ScriptSMSpreadNode)
                    {
                        node2 = new MyVSStateMachineSpreadNode(node.Name);
                    }
                    else if (node is MyObjectBuilder_ScriptSMBarrierNode)
                    {
                        node2 = new MyVSStateMachineBarrierNode(node.Name);
                    }
                    else
                    {
                        Type script = MyVSAssemblyProvider.GetType("VisualScripting.CustomScripts." + node.ScriptClassName);
                        MyVSStateMachineNode node3 = new MyVSStateMachineNode(node.Name, script);
                        if (node3.ScriptInstance != null)
                        {
                            node3.ScriptInstance.OwnerId = (ownerId != null) ? ownerId.Value : ob.OwnerId;
                        }
                        node2 = node3;
                    }
                    this.AddNode(node2);
                }
            }
            if (ob.Transitions != null)
            {
                foreach (MyObjectBuilder_ScriptSMTransition transition in ob.Transitions)
                {
                    this.AddTransition(transition.From, transition.To, null, transition.Name);
                }
            }
            if (ob.Cursors != null)
            {
                foreach (MyObjectBuilder_ScriptSMCursor cursor in ob.Cursors)
                {
                    this.CreateCursor(cursor.NodeName);
                }
            }
        }

        private void NotifyStateChanged(MyVSStateMachineNode from, MyVSStateMachineNode to)
        {
            if (this.CursorStateChanged != null)
            {
                this.CursorStateChanged(from, to);
            }
        }

        private void OnCursorStateChanged(int transitionId, MyStringId action, MyStateMachineNode node, MyStateMachine stateMachine)
        {
            MyVSStateMachineNode startNode = base.FindTransitionWithStart(transitionId).StartNode as MyVSStateMachineNode;
            if (startNode != null)
            {
                startNode.DisposeScript();
            }
            MyVSStateMachineNode to = node as MyVSStateMachineNode;
            if (to != null)
            {
                to.ActivateScript(false);
            }
            this.NotifyStateChanged(startNode, to);
        }

        public MyStateMachineCursor RestoreCursor(string nodeName)
        {
            using (Dictionary<int, MyStateMachineCursor>.ValueCollection.Enumerator enumerator = base.m_activeCursorsById.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.Node.Name == nodeName)
                    {
                        return null;
                    }
                }
            }
            MyStateMachineCursor item = base.CreateCursor(nodeName);
            if (item != null)
            {
                item.OnCursorStateChanged += new VRage.Generics.MyStateMachineCursor.CursorStateChanged(this.OnCursorStateChanged);
                if (item.Node is MyVSStateMachineNode)
                {
                    this.m_cursorsToDeserialize.Add(item);
                }
            }
            return item;
        }

        public void TriggerCachedAction(MyStringId actionName)
        {
            this.m_cachedActions.Add(actionName);
        }

        public override void Update()
        {
            List<MyStateMachineCursor>.Enumerator enumerator;
            base.m_activeCursors.ApplyChanges();
            using (enumerator = this.m_cursorsToDeserialize.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyVSStateMachineNode node = enumerator.Current.Node as MyVSStateMachineNode;
                    if (node != null)
                    {
                        node.ActivateScript(true);
                    }
                }
            }
            this.m_cursorsToDeserialize.Clear();
            using (enumerator = this.m_cursorsToInit.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyVSStateMachineNode node = enumerator.Current.Node as MyVSStateMachineNode;
                    if (node != null)
                    {
                        node.ActivateScript(false);
                    }
                }
            }
            this.m_cursorsToInit.Clear();
            foreach (MyStringId id in this.m_cachedActions)
            {
                base.m_enqueuedActions.Add(id);
            }
            this.m_cachedActions.Clear();
            base.Update();
        }

        public int ActiveCursorCount =>
            base.m_activeCursors.Count;

        public long OwnerId
        {
            get => 
                this.m_ownerId;
            set
            {
                foreach (MyVSStateMachineNode node in base.m_nodes.Values)
                {
                    if (node == null)
                    {
                        continue;
                    }
                    if (node.ScriptInstance != null)
                    {
                        node.ScriptInstance.OwnerId = value;
                    }
                }
                this.m_ownerId = value;
            }
        }
    }
}

