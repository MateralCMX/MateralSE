namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    [MyBehaviorTreeNodeMemoryType(typeof(MyObjectBuilder_BehaviorTreeControlNodeMemory))]
    public class MyBehaviorTreeControlNodeMemory : MyBehaviorTreeNodeMemory
    {
        public MyBehaviorTreeControlNodeMemory()
        {
            this.InitialIndex = 0;
        }

        public override void ClearMemory()
        {
            base.ClearMemory();
            this.InitialIndex = 0;
        }

        public override MyObjectBuilder_BehaviorTreeNodeMemory GetObjectBuilder()
        {
            MyObjectBuilder_BehaviorTreeControlNodeMemory objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_BehaviorTreeControlNodeMemory;
            objectBuilder.InitialIndex = this.InitialIndex;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_BehaviorTreeNodeMemory builder)
        {
            base.Init(builder);
            MyObjectBuilder_BehaviorTreeControlNodeMemory memory = builder as MyObjectBuilder_BehaviorTreeControlNodeMemory;
            this.InitialIndex = memory.InitialIndex;
        }

        public override void PostTickMemory()
        {
            base.PostTickMemory();
            this.InitialIndex = 0;
        }

        public int InitialIndex { get; set; }
    }
}

