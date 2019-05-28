namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public class MyBehaviorTreeDecoratorCounterLogic : IMyDecoratorLogic
    {
        public MyBehaviorTreeDecoratorCounterLogic()
        {
            this.CounterLimit = 0;
        }

        public bool CanRun(MyBehaviorTreeDecoratorNodeMemory.LogicMemory logicMemory) => 
            ((logicMemory as MyBehaviorTreeDecoratorNodeMemory.CounterLogicMemory).CurrentCount == this.CounterLimit);

        public void Construct(MyObjectBuilder_BehaviorTreeDecoratorNode.Logic logicData)
        {
            MyObjectBuilder_BehaviorTreeDecoratorNode.CounterLogic logic = logicData as MyObjectBuilder_BehaviorTreeDecoratorNode.CounterLogic;
            this.CounterLimit = logic.Count;
        }

        public override int GetHashCode() => 
            this.CounterLimit.GetHashCode();

        public MyBehaviorTreeDecoratorNodeMemory.LogicMemory GetNewMemoryObject() => 
            new MyBehaviorTreeDecoratorNodeMemory.CounterLogicMemory();

        public override string ToString() => 
            "Counter";

        public void Update(MyBehaviorTreeDecoratorNodeMemory.LogicMemory logicMemory)
        {
            MyBehaviorTreeDecoratorNodeMemory.CounterLogicMemory memory = logicMemory as MyBehaviorTreeDecoratorNodeMemory.CounterLogicMemory;
            if (memory.CurrentCount == this.CounterLimit)
            {
                memory.CurrentCount = 0;
            }
            else
            {
                memory.CurrentCount++;
            }
        }

        public int CounterLimit { get; private set; }
    }
}

