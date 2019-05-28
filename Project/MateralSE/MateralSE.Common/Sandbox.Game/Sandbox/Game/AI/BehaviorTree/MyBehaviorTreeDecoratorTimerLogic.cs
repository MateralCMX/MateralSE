namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public class MyBehaviorTreeDecoratorTimerLogic : IMyDecoratorLogic
    {
        public MyBehaviorTreeDecoratorTimerLogic()
        {
            this.TimeInMs = 0L;
        }

        public bool CanRun(MyBehaviorTreeDecoratorNodeMemory.LogicMemory logicMemory) => 
            (logicMemory as MyBehaviorTreeDecoratorNodeMemory.TimerLogicMemory).TimeLimitReached;

        public void Construct(MyObjectBuilder_BehaviorTreeDecoratorNode.Logic logicData)
        {
            MyObjectBuilder_BehaviorTreeDecoratorNode.TimerLogic logic = logicData as MyObjectBuilder_BehaviorTreeDecoratorNode.TimerLogic;
            this.TimeInMs = logic.TimeInMs;
        }

        public override int GetHashCode() => 
            ((int) this.TimeInMs).GetHashCode();

        public MyBehaviorTreeDecoratorNodeMemory.LogicMemory GetNewMemoryObject() => 
            new MyBehaviorTreeDecoratorNodeMemory.TimerLogicMemory();

        public override string ToString() => 
            "Timer";

        public void Update(MyBehaviorTreeDecoratorNodeMemory.LogicMemory logicMemory)
        {
            MyBehaviorTreeDecoratorNodeMemory.TimerLogicMemory memory = logicMemory as MyBehaviorTreeDecoratorNodeMemory.TimerLogicMemory;
            if ((((Stopwatch.GetTimestamp() - memory.CurrentTime) / Stopwatch.Frequency) * 0x3e8L) <= this.TimeInMs)
            {
                memory.TimeLimitReached = false;
            }
            else
            {
                memory.CurrentTime = Stopwatch.GetTimestamp();
                memory.TimeLimitReached = true;
            }
        }

        public long TimeInMs { get; private set; }
    }
}

