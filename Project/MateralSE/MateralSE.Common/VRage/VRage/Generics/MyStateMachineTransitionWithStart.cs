namespace VRage.Generics
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyStateMachineTransitionWithStart
    {
        public MyStateMachineNode StartNode;
        public MyStateMachineTransition Transition;
        public MyStateMachineTransitionWithStart(MyStateMachineNode startNode, MyStateMachineTransition transition)
        {
            this.StartNode = startNode;
            this.Transition = transition;
        }
    }
}

