namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using VRage.Game;

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorTreeSelectorNode), typeof(MyBehaviorTreeControlNodeMemory))]
    internal class MyBehaviorTreeSelectorNode : MyBehaviorTreeControlBaseNode
    {
        public override string ToString() => 
            ("SEL: " + base.ToString());

        public override MyBehaviorTreeState SearchedValue =>
            MyBehaviorTreeState.SUCCESS;

        public override MyBehaviorTreeState FinalValue =>
            MyBehaviorTreeState.FAILURE;

        public override string DebugSign =>
            "?";
    }
}

