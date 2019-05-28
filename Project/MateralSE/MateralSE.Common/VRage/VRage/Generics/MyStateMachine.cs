namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;

    public class MyStateMachine
    {
        private int m_transitionIdCounter;
        protected Dictionary<string, MyStateMachineNode> m_nodes = new Dictionary<string, MyStateMachineNode>();
        protected Dictionary<int, MyStateMachineTransitionWithStart> m_transitions = new Dictionary<int, MyStateMachineTransitionWithStart>();
        protected Dictionary<int, MyStateMachineCursor> m_activeCursorsById = new Dictionary<int, MyStateMachineCursor>();
        protected CachingList<MyStateMachineCursor> m_activeCursors = new CachingList<MyStateMachineCursor>();
        protected MyConcurrentHashSet<MyStringId> m_enqueuedActions = new MyConcurrentHashSet<MyStringId>();

        public virtual bool AddNode(MyStateMachineNode newNode)
        {
            if (this.FindNode(newNode.Name) != null)
            {
                return false;
            }
            this.m_nodes.Add(newNode.Name, newNode);
            return true;
        }

        public virtual MyStateMachineTransition AddTransition(string startNodeName, string endNodeName, MyStateMachineTransition existingInstance = null, string name = null)
        {
            MyStateMachineTransition transition;
            MyStateMachineNode startNode = this.FindNode(startNodeName);
            MyStateMachineNode node2 = this.FindNode(endNodeName);
            if ((startNode == null) || (node2 == null))
            {
                return null;
            }
            if (existingInstance != null)
            {
                transition = existingInstance;
            }
            else
            {
                transition = new MyStateMachineTransition();
                if (name != null)
                {
                    transition.Name = MyStringId.GetOrCompute(name);
                }
            }
            this.m_transitionIdCounter++;
            transition._SetId(this.m_transitionIdCounter);
            transition.TargetNode = node2;
            startNode.OutTransitions.Add(transition);
            node2.InTransitions.Add(transition);
            this.m_transitions.Add(this.m_transitionIdCounter, new MyStateMachineTransitionWithStart(startNode, transition));
            startNode.TransitionAdded(transition);
            node2.TransitionAdded(transition);
            return transition;
        }

        public virtual MyStateMachineCursor CreateCursor(string nodeName)
        {
            MyStateMachineNode node = this.FindNode(nodeName);
            if (node == null)
            {
                return null;
            }
            MyStateMachineCursor cursor = new MyStateMachineCursor(node, this);
            this.m_activeCursorsById.Add(cursor.Id, cursor);
            this.m_activeCursors.Add(cursor);
            return cursor;
        }

        public virtual bool DeleteCursor(int id)
        {
            if (!this.m_activeCursorsById.ContainsKey(id))
            {
                return false;
            }
            MyStateMachineCursor entity = this.m_activeCursorsById[id];
            this.m_activeCursorsById.Remove(id);
            this.m_activeCursors.Remove(entity, false);
            return true;
        }

        public virtual bool DeleteNode(string nodeName)
        {
            MyStateMachineNode rtnNode;
            this.m_nodes.TryGetValue(nodeName, out rtnNode);
            if (rtnNode == null)
            {
                return false;
            }
            foreach (KeyValuePair<string, MyStateMachineNode> pair in this.m_nodes)
            {
                Predicate<MyStateMachineTransition> <>9__0;
                Predicate<MyStateMachineTransition> match = <>9__0;
                if (<>9__0 == null)
                {
                    Predicate<MyStateMachineTransition> local1 = <>9__0;
                    match = <>9__0 = x => ReferenceEquals(x.TargetNode, rtnNode);
                }
                pair.Value.OutTransitions.RemoveAll(match);
            }
            this.m_nodes.Remove(nodeName);
            int num = 0;
            while (num < this.m_activeCursors.Count)
            {
                if (this.m_activeCursors[num].Node.Name != nodeName)
                {
                    continue;
                }
                this.m_activeCursors[num].Node = null;
                this.m_activeCursorsById.Remove(this.m_activeCursors[num].Id);
                this.m_activeCursors.Remove(this.m_activeCursors[num], false);
            }
            return true;
        }

        public virtual bool DeleteTransition(int transitionId)
        {
            MyStateMachineTransitionWithStart start;
            if (!this.m_transitions.TryGetValue(transitionId, out start))
            {
                return false;
            }
            start.StartNode.TransitionRemoved(start.Transition);
            start.Transition.TargetNode.TransitionRemoved(start.Transition);
            this.m_transitions.Remove(transitionId);
            start.StartNode.OutTransitions.Remove(start.Transition);
            start.Transition.TargetNode.InTransitions.Remove(start.Transition);
            return true;
        }

        public MyStateMachineCursor FindCursor(int cursorId)
        {
            MyStateMachineCursor cursor;
            this.m_activeCursorsById.TryGetValue(cursorId, out cursor);
            return cursor;
        }

        public MyStateMachineNode FindNode(string nodeName)
        {
            MyStateMachineNode node;
            this.m_nodes.TryGetValue(nodeName, out node);
            return node;
        }

        public MyStateMachineTransition FindTransition(int transitionId) => 
            this.FindTransitionWithStart(transitionId).Transition;

        public MyStateMachineTransitionWithStart FindTransitionWithStart(int transitionId)
        {
            MyStateMachineTransitionWithStart start;
            this.m_transitions.TryGetValue(transitionId, out start);
            return start;
        }

        public virtual bool SetState(int cursorId, string nameOfNewState)
        {
            MyStateMachineNode node = this.FindNode(nameOfNewState);
            MyStateMachineCursor cursor = this.FindCursor(cursorId);
            if (node == null)
            {
                return false;
            }
            cursor.Node = node;
            return true;
        }

        public void SortTransitions()
        {
            using (Dictionary<string, MyStateMachineNode>.ValueCollection.Enumerator enumerator = this.m_nodes.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OutTransitions.Sort(delegate (MyStateMachineTransition transition1, MyStateMachineTransition transition2) {
                        int? priority = transition1.Priority;
                        int num = (priority != null) ? priority.GetValueOrDefault() : 0x7fffffff;
                        priority = transition2.Priority;
                        return num.CompareTo((priority != null) ? priority.GetValueOrDefault() : 0x7fffffff);
                    });
                }
            }
        }

        public void TriggerAction(MyStringId actionName)
        {
            this.m_enqueuedActions.Add(actionName);
        }

        public virtual void Update()
        {
            this.m_activeCursors.ApplyChanges();
            if (this.m_activeCursorsById.Count == 0)
            {
                this.m_enqueuedActions.Clear();
            }
            else
            {
                foreach (MyStateMachineCursor cursor in this.m_activeCursors)
                {
                    cursor.Node.Expand(cursor, this.m_enqueuedActions);
                    cursor.Node.OnUpdate(this);
                }
                this.m_enqueuedActions.Clear();
            }
        }

        public DictionaryReader<string, MyStateMachineNode> AllNodes =>
            this.m_nodes;

        public List<MyStateMachineCursor> ActiveCursors =>
            new List<MyStateMachineCursor>(this.m_activeCursors);

        public string Name { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyStateMachine.<>c <>9 = new MyStateMachine.<>c();
            public static Comparison<MyStateMachineTransition> <>9__27_0;

            internal int <SortTransitions>b__27_0(MyStateMachineTransition transition1, MyStateMachineTransition transition2)
            {
                int? priority = transition1.Priority;
                int num = (priority != null) ? priority.GetValueOrDefault() : 0x7fffffff;
                priority = transition2.Priority;
                return num.CompareTo((priority != null) ? priority.GetValueOrDefault() : 0x7fffffff);
            }
        }
    }
}

