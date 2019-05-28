namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Utils;

    public class MyStateMachineNode
    {
        private readonly string m_name;
        protected internal List<MyStateMachineTransition> OutTransitions = new List<MyStateMachineTransition>();
        protected internal List<MyStateMachineTransition> InTransitions = new List<MyStateMachineTransition>();
        protected internal HashSet<MyStateMachineCursor> Cursors = new HashSet<MyStateMachineCursor>();
        public bool PassThrough;

        public MyStateMachineNode(string name)
        {
            this.m_name = name;
        }

        internal void Expand(MyStateMachineCursor cursor, MyConcurrentHashSet<MyStringId> enquedActions)
        {
            this.ExpandInternal(cursor, enquedActions, 100);
        }

        protected virtual void ExpandInternal(MyStateMachineCursor cursor, MyConcurrentHashSet<MyStringId> enquedActions, int passThrough)
        {
            do
            {
                MyStateMachineTransition transition = null;
                List<MyStateMachineTransition> outTransitions = cursor.Node.OutTransitions;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (enquedActions.Count > 0)
                {
                    int num = 0x7fffffff;
                    for (int i = 0; i < outTransitions.Count; i++)
                    {
                        int? priority = outTransitions[i].Priority;
                        int num3 = (priority != null) ? priority.GetValueOrDefault() : 0x7fffffff;
                        enquedActions.Contains(outTransitions[i].Name);
                        bool flag = false;
                        foreach (MyStringId id2 in enquedActions)
                        {
                            if (id2.String.ToLower() == outTransitions[i].Name.ToString().ToLower())
                            {
                                flag = true;
                            }
                        }
                        if ((flag && (num3 <= num)) && ((outTransitions[i].Conditions.Count == 0) || outTransitions[i].Evaluate()))
                        {
                            transition = outTransitions[i];
                            num = num3;
                            nullOrEmpty = outTransitions[i].Name;
                        }
                    }
                }
                if (transition == null)
                {
                    transition = cursor.Node.QueryNextTransition();
                    using (ConcurrentEnumerator<SpinLockRef.Token, MyStringId, HashSet<MyStringId>.Enumerator> enumerator = enquedActions.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                        }
                    }
                }
                if (transition != null)
                {
                    cursor.FollowTransition(transition, nullOrEmpty);
                }
                if (transition == null)
                {
                    break;
                }
                if (!cursor.Node.PassThrough)
                {
                    break;
                }
                passThrough--;
            }
            while (passThrough > 0);
        }

        public virtual void OnUpdate(MyStateMachine stateMachine)
        {
        }

        protected virtual MyStateMachineTransition QueryNextTransition()
        {
            for (int i = 0; i < this.OutTransitions.Count; i++)
            {
                if (this.OutTransitions[i].Evaluate())
                {
                    return this.OutTransitions[i];
                }
            }
            return null;
        }

        internal void TransitionAdded(MyStateMachineTransition transition)
        {
            this.TransitionAddedInternal(transition);
        }

        protected virtual void TransitionAddedInternal(MyStateMachineTransition transition)
        {
        }

        internal void TransitionRemoved(MyStateMachineTransition transition)
        {
            this.TransitionRemovedInternal(transition);
        }

        protected virtual void TransitionRemovedInternal(MyStateMachineTransition transition)
        {
        }

        public string Name =>
            this.m_name;
    }
}

