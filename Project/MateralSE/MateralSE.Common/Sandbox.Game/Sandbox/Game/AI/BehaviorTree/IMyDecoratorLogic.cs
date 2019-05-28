namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using VRage.Game;

    public interface IMyDecoratorLogic
    {
        bool CanRun(MyBehaviorTreeDecoratorNodeMemory.LogicMemory memory);
        void Construct(MyObjectBuilder_BehaviorTreeDecoratorNode.Logic logicData);
        MyBehaviorTreeDecoratorNodeMemory.LogicMemory GetNewMemoryObject();
        void Update(MyBehaviorTreeDecoratorNodeMemory.LogicMemory memory);
    }
}

