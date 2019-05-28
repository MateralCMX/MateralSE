namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Generics;
    using VRage.Utils;

    public class MyAnimationStateMachineNode : MyStateMachineNode
    {
        private MyAnimationTreeNode m_rootAnimationNode;
        public List<VarAssignmentData> VariableAssignments;

        public MyAnimationStateMachineNode(string name) : base(name)
        {
        }

        public MyAnimationStateMachineNode(string name, MyAnimationClip animationClip) : base(name)
        {
            if (animationClip != null)
            {
                MyAnimationTreeNodeTrack track = new MyAnimationTreeNodeTrack();
                track.SetClip(animationClip);
                this.m_rootAnimationNode = track;
            }
        }

        public override void OnUpdate(MyStateMachine stateMachine)
        {
            MyAnimationStateMachine machine = stateMachine as MyAnimationStateMachine;
            if (machine != null)
            {
                if (this.m_rootAnimationNode == null)
                {
                    machine.CurrentUpdateData.BonesResult = machine.CurrentUpdateData.Controller.ResultBonesPool.Alloc();
                }
                else
                {
                    machine.CurrentUpdateData.AddVisitedTreeNodesPathPoint(1);
                    this.m_rootAnimationNode.Update(ref machine.CurrentUpdateData);
                }
                machine.CurrentUpdateData.AddVisitedTreeNodesPathPoint(0);
            }
        }

        protected override MyStateMachineTransition QueryNextTransition()
        {
            for (int i = 0; i < base.OutTransitions.Count; i++)
            {
                if ((base.OutTransitions[i].Name == MyStringId.NullOrEmpty) && base.OutTransitions[i].Evaluate())
                {
                    return base.OutTransitions[i];
                }
            }
            return null;
        }

        public MyAnimationTreeNode RootAnimationNode
        {
            get => 
                this.m_rootAnimationNode;
            set => 
                (this.m_rootAnimationNode = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VarAssignmentData
        {
            public MyStringId VariableId;
            public float Value;
        }
    }
}

