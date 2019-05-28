namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using VRage.Game;

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorTreeSequenceNode), typeof(MyBehaviorTreeControlNodeMemory))]
    internal class MyBehaviorTreeSequenceNode : MyBehaviorTreeControlBaseNode
    {
        public override string ToString() => 
            ("SEQ: " + base.ToString());

        public override MyBehaviorTreeState SearchedValue =>
            MyBehaviorTreeState.FAILURE;

        public override MyBehaviorTreeState FinalValue =>
            MyBehaviorTreeState.SUCCESS;

        public override string DebugSign =>
            "->";
    }
}

