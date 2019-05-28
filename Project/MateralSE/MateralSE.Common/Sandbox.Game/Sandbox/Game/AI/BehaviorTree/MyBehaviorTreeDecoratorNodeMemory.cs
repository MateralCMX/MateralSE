namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    [MyBehaviorTreeNodeMemoryType(typeof(MyObjectBuilder_BehaviorTreeDecoratorNodeMemory))]
    public class MyBehaviorTreeDecoratorNodeMemory : MyBehaviorTreeNodeMemory
    {
        public override void ClearMemory()
        {
            base.ClearMemory();
            this.ChildState = MyBehaviorTreeState.NOT_TICKED;
            this.DecoratorLogicMemory.ClearMemory();
        }

        private static LogicMemory GetLogicMemoryByBuilder(MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder builder) => 
            (!(builder is MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.TimerLogicMemoryBuilder) ? (!(builder is MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.CounterLogicMemoryBuilder) ? null : ((LogicMemory) new CounterLogicMemory())) : ((LogicMemory) new TimerLogicMemory()));

        public override MyObjectBuilder_BehaviorTreeNodeMemory GetObjectBuilder()
        {
            MyObjectBuilder_BehaviorTreeDecoratorNodeMemory objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_BehaviorTreeDecoratorNodeMemory;
            objectBuilder.ChildState = this.ChildState;
            objectBuilder.Logic = this.DecoratorLogicMemory.GetObjectBuilder();
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_BehaviorTreeNodeMemory builder)
        {
            base.Init(builder);
            MyObjectBuilder_BehaviorTreeDecoratorNodeMemory memory = builder as MyObjectBuilder_BehaviorTreeDecoratorNodeMemory;
            this.ChildState = memory.ChildState;
            this.DecoratorLogicMemory = GetLogicMemoryByBuilder(memory.Logic);
        }

        public override void PostTickMemory()
        {
            base.PostTickMemory();
            this.ChildState = MyBehaviorTreeState.NOT_TICKED;
            this.DecoratorLogicMemory.PostTickMemory();
        }

        public MyBehaviorTreeState ChildState { get; set; }

        public LogicMemory DecoratorLogicMemory { get; set; }

        public class CounterLogicMemory : MyBehaviorTreeDecoratorNodeMemory.LogicMemory
        {
            public override void ClearMemory()
            {
                this.CurrentCount = 0;
            }

            public override MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder GetObjectBuilder()
            {
                MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.CounterLogicMemoryBuilder builder1 = new MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.CounterLogicMemoryBuilder();
                builder1.CurrentCount = this.CurrentCount;
                return builder1;
            }

            public override void Init(MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder logicMemoryBuilder)
            {
                MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.CounterLogicMemoryBuilder builder = logicMemoryBuilder as MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.CounterLogicMemoryBuilder;
                this.CurrentCount = builder.CurrentCount;
            }

            public override void PostTickMemory()
            {
                this.CurrentCount = 0;
            }

            public int CurrentCount { get; set; }
        }

        public abstract class LogicMemory
        {
            protected LogicMemory()
            {
            }

            public abstract void ClearMemory();
            public abstract MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder GetObjectBuilder();
            public abstract void Init(MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder logicMemoryBuilder);
            public abstract void PostTickMemory();
        }

        public class TimerLogicMemory : MyBehaviorTreeDecoratorNodeMemory.LogicMemory
        {
            public override void ClearMemory()
            {
                this.TimeLimitReached = true;
                this.CurrentTime = Stopwatch.GetTimestamp();
            }

            public override MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder GetObjectBuilder()
            {
                MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.TimerLogicMemoryBuilder builder1 = new MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.TimerLogicMemoryBuilder();
                builder1.CurrentTime = Stopwatch.GetTimestamp() - this.CurrentTime;
                builder1.TimeLimitReached = this.TimeLimitReached;
                return builder1;
            }

            public override void Init(MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.LogicMemoryBuilder logicMemoryBuilder)
            {
                MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.TimerLogicMemoryBuilder builder = logicMemoryBuilder as MyObjectBuilder_BehaviorTreeDecoratorNodeMemory.TimerLogicMemoryBuilder;
                this.CurrentTime = Stopwatch.GetTimestamp() - builder.CurrentTime;
                this.TimeLimitReached = builder.TimeLimitReached;
            }

            public override void PostTickMemory()
            {
                this.TimeLimitReached = false;
                this.CurrentTime = Stopwatch.GetTimestamp();
            }

            public bool TimeLimitReached { get; set; }

            public long CurrentTime { get; set; }
        }
    }
}

