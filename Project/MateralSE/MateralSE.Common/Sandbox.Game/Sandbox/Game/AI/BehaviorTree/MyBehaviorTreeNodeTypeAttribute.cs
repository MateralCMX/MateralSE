namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using VRage.Game.Common;

    public class MyBehaviorTreeNodeTypeAttribute : MyFactoryTagAttribute
    {
        public readonly Type MemoryType;

        public MyBehaviorTreeNodeTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
            this.MemoryType = typeof(MyBehaviorTreeNodeMemory);
        }

        public MyBehaviorTreeNodeTypeAttribute(Type objectBuilderType, Type memoryType) : base(objectBuilderType, true)
        {
            this.MemoryType = memoryType;
        }
    }
}

