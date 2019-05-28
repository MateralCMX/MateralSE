namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using VRage.Game.Common;

    [AttributeUsage(AttributeTargets.Class)]
    public class MyBehaviorTreeNodeMemoryTypeAttribute : MyFactoryTagAttribute
    {
        public MyBehaviorTreeNodeMemoryTypeAttribute(Type objectBuilderType) : base(objectBuilderType, true)
        {
        }
    }
}

