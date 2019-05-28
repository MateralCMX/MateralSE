namespace Sandbox.Game.AI.Actions
{
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.AI;

    public abstract class MyBotActionsBase
    {
        protected MyBotActionsBase()
        {
        }

        [MyBehaviorTreeAction("DummyFailingNode", ReturnsRunning=false)]
        protected MyBehaviorTreeState DummyFailingNode() => 
            MyBehaviorTreeState.FAILURE;

        [MyBehaviorTreeAction("DummyRunningNode")]
        protected MyBehaviorTreeState DummyRunningNode() => 
            MyBehaviorTreeState.RUNNING;

        [MyBehaviorTreeAction("DummySucceedingNode", ReturnsRunning=false)]
        protected MyBehaviorTreeState DummySucceedingNode() => 
            MyBehaviorTreeState.SUCCESS;

        [MyBehaviorTreeAction("Idle")]
        protected virtual MyBehaviorTreeState Idle() => 
            MyBehaviorTreeState.RUNNING;

        [MyBehaviorTreeAction("Increment", ReturnsRunning=false)]
        protected MyBehaviorTreeState Increment([BTInOut] ref MyBBMemoryInt variable)
        {
            if (variable == null)
            {
                variable = new MyBBMemoryInt();
            }
            variable.IntValue++;
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("Idle", MyBehaviorTreeActionType.INIT)]
        protected virtual void Init_Idle()
        {
        }

        [MyBehaviorTreeAction("IsCreativeGame", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsCreativeGame() => 
            (!MySession.Static.CreativeMode ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("IsFalse", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsFalse([BTIn] ref MyBBMemoryBool variable)
        {
            if ((variable == null) || variable.BoolValue)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("IsIntLargerThan", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsIntLargerThan([BTIn] ref MyBBMemoryInt variable, [BTParam] int value)
        {
            if (variable == null)
            {
                variable = new MyBBMemoryInt();
            }
            return ((variable.IntValue > value) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);
        }

        [MyBehaviorTreeAction("IsSurvivalGame", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsSurvivalGame() => 
            (!MySession.Static.SurvivalMode ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("IsTrue", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsTrue([BTIn] ref MyBBMemoryBool variable)
        {
            if ((variable == null) || !variable.BoolValue)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("SetBoolean", ReturnsRunning=false)]
        protected MyBehaviorTreeState SetBoolean([BTOut] ref MyBBMemoryBool variable, [BTParam] bool value)
        {
            if (variable == null)
            {
                variable = new MyBBMemoryBool();
            }
            variable.BoolValue = value;
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("SetInt", ReturnsRunning=false)]
        protected MyBehaviorTreeState SetInt([BTOut] ref MyBBMemoryInt variable, [BTParam] int value)
        {
            if (variable == null)
            {
                variable = new MyBBMemoryInt();
            }
            variable.IntValue = value;
            return MyBehaviorTreeState.SUCCESS;
        }
    }
}

