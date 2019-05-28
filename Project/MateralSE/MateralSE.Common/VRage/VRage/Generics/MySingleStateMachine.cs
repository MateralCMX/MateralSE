namespace VRage.Generics
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Utils;

    public class MySingleStateMachine : MyStateMachine
    {
        [CompilerGenerated]
        private StateChangedHandler OnStateChanged;

        public event StateChangedHandler OnStateChanged
        {
            [CompilerGenerated] add
            {
                StateChangedHandler onStateChanged = this.OnStateChanged;
                while (true)
                {
                    StateChangedHandler a = onStateChanged;
                    StateChangedHandler handler3 = (StateChangedHandler) Delegate.Combine(a, value);
                    onStateChanged = Interlocked.CompareExchange<StateChangedHandler>(ref this.OnStateChanged, handler3, a);
                    if (ReferenceEquals(onStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                StateChangedHandler onStateChanged = this.OnStateChanged;
                while (true)
                {
                    StateChangedHandler source = onStateChanged;
                    StateChangedHandler handler3 = (StateChangedHandler) Delegate.Remove(source, value);
                    onStateChanged = Interlocked.CompareExchange<StateChangedHandler>(ref this.OnStateChanged, handler3, source);
                    if (ReferenceEquals(onStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public override MyStateMachineCursor CreateCursor(string nodeName) => 
            null;

        private void CursorStateChanged(int transitionId, MyStringId action, MyStateMachineNode node, MyStateMachine stateMachine)
        {
            this.NotifyStateChanged(base.m_transitions[transitionId], action);
        }

        public override bool DeleteCursor(int id) => 
            false;

        protected void NotifyStateChanged(MyStateMachineTransitionWithStart transitionWithStart, MyStringId action)
        {
            if (this.OnStateChanged != null)
            {
                this.OnStateChanged(transitionWithStart, action);
            }
        }

        public bool SetState(string nameOfNewState)
        {
            if (base.m_activeCursors.Count != 0)
            {
                MyStateMachineNode node = base.FindNode(nameOfNewState);
                base.m_activeCursors[0].Node = node;
            }
            else
            {
                if (base.CreateCursor(nameOfNewState) == null)
                {
                    return false;
                }
                base.m_activeCursors.ApplyChanges();
                base.m_activeCursors[0].OnCursorStateChanged += new VRage.Generics.MyStateMachineCursor.CursorStateChanged(this.CursorStateChanged);
            }
            return true;
        }

        public MyStateMachineNode CurrentNode =>
            ((base.m_activeCursors.Count != 0) ? base.m_activeCursors[0].Node : null);

        public delegate void StateChangedHandler(MyStateMachineTransitionWithStart transition, MyStringId action);
    }
}

