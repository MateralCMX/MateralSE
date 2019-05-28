namespace VRage.Generics
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Utils;

    public class MyStateMachineCursor
    {
        private static int m_idCounter;
        private readonly MyStateMachine m_stateMachine;
        private MyStateMachineNode m_node;
        public readonly int Id;
        [CompilerGenerated]
        private CursorStateChanged OnCursorStateChanged;

        public event CursorStateChanged OnCursorStateChanged
        {
            [CompilerGenerated] add
            {
                CursorStateChanged onCursorStateChanged = this.OnCursorStateChanged;
                while (true)
                {
                    CursorStateChanged a = onCursorStateChanged;
                    CursorStateChanged changed3 = (CursorStateChanged) Delegate.Combine(a, value);
                    onCursorStateChanged = Interlocked.CompareExchange<CursorStateChanged>(ref this.OnCursorStateChanged, changed3, a);
                    if (ReferenceEquals(onCursorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                CursorStateChanged onCursorStateChanged = this.OnCursorStateChanged;
                while (true)
                {
                    CursorStateChanged source = onCursorStateChanged;
                    CursorStateChanged changed3 = (CursorStateChanged) Delegate.Remove(source, value);
                    onCursorStateChanged = Interlocked.CompareExchange<CursorStateChanged>(ref this.OnCursorStateChanged, changed3, source);
                    if (ReferenceEquals(onCursorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyStateMachineCursor(MyStateMachineNode node, MyStateMachine stateMachine)
        {
            this.m_stateMachine = stateMachine;
            this.Id = Interlocked.Increment(ref m_idCounter);
            this.m_node = node;
            this.m_node.Cursors.Add(this);
            this.OnCursorStateChanged = null;
        }

        public void FollowTransition(MyStateMachineTransition transition, MyStringId action)
        {
            this.Node.Cursors.Remove(this);
            transition.TargetNode.Cursors.Add(this);
            this.Node = transition.TargetNode;
            this.LastTransitionTakenId = transition.Id;
            this.NotifyCursorChanged(transition, action);
        }

        private void NotifyCursorChanged(MyStateMachineTransition transition, MyStringId action)
        {
            if (this.OnCursorStateChanged != null)
            {
                this.OnCursorStateChanged(transition.Id, action, this.Node, this.StateMachine);
            }
        }

        public MyStateMachineNode Node
        {
            get => 
                this.m_node;
            internal set => 
                (this.m_node = value);
        }

        public int LastTransitionTakenId { get; private set; }

        public MyStateMachine StateMachine =>
            this.m_stateMachine;

        public delegate void CursorStateChanged(int transitionId, MyStringId action, MyStateMachineNode node, MyStateMachine stateMachine);
    }
}

