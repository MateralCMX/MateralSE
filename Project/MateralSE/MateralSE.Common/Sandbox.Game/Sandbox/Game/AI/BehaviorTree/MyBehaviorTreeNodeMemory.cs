namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    [MyBehaviorTreeNodeMemoryType(typeof(MyObjectBuilder_BehaviorTreeNodeMemory))]
    public class MyBehaviorTreeNodeMemory
    {
        public MyBehaviorTreeNodeMemory()
        {
            this.InitCalled = false;
            this.ClearNodeState();
        }

        public virtual void ClearMemory()
        {
            this.NodeState = MyBehaviorTreeState.NOT_TICKED;
            this.InitCalled = false;
        }

        public void ClearNodeState()
        {
            this.NodeState = MyBehaviorTreeState.NOT_TICKED;
        }

        private static Color GetColorByState(MyBehaviorTreeState state)
        {
            switch (state)
            {
                case MyBehaviorTreeState.ERROR:
                    return Color.Bisque;

                case MyBehaviorTreeState.NOT_TICKED:
                    return Color.White;

                case MyBehaviorTreeState.SUCCESS:
                    return Color.Green;

                case MyBehaviorTreeState.FAILURE:
                    return Color.Red;

                case MyBehaviorTreeState.RUNNING:
                    return Color.Yellow;
            }
            return Color.Black;
        }

        public virtual MyObjectBuilder_BehaviorTreeNodeMemory GetObjectBuilder()
        {
            MyObjectBuilder_BehaviorTreeNodeMemory memory1 = MyBehaviorTreeNodeMemoryFactory.CreateObjectBuilder(this);
            memory1.InitCalled = this.InitCalled;
            return memory1;
        }

        public virtual void Init(MyObjectBuilder_BehaviorTreeNodeMemory builder)
        {
            this.InitCalled = builder.InitCalled;
        }

        public virtual void PostTickMemory()
        {
        }

        public MyBehaviorTreeState NodeState { get; set; }

        public Color NodeStateColor =>
            GetColorByState(this.NodeState);

        public bool InitCalled { get; set; }

        public bool IsTicked =>
            (this.NodeState != MyBehaviorTreeState.NOT_TICKED);
    }
}

