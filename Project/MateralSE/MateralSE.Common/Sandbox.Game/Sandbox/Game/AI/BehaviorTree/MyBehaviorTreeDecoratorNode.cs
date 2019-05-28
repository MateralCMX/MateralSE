namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using VRage.Game;
    using VRageMath;

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorTreeDecoratorNode), typeof(MyBehaviorTreeDecoratorNodeMemory))]
    public class MyBehaviorTreeDecoratorNode : MyBehaviorTreeNode
    {
        private MyBehaviorTreeNode m_child = null;
        private IMyDecoratorLogic m_decoratorLogic = null;
        private MyBehaviorTreeState m_defaultReturnValue;
        private string m_decoratorLogicName;

        public override void Construct(MyObjectBuilder_BehaviorTreeNode nodeDefinition, MyBehaviorTree.MyBehaviorTreeDesc treeDesc)
        {
            base.Construct(nodeDefinition, treeDesc);
            MyObjectBuilder_BehaviorTreeDecoratorNode node = nodeDefinition as MyObjectBuilder_BehaviorTreeDecoratorNode;
            this.m_defaultReturnValue = (MyBehaviorTreeState) ((sbyte) node.DefaultReturnValue);
            this.m_decoratorLogicName = node.DecoratorLogic.GetType().Name;
            this.m_decoratorLogic = GetDecoratorLogic(node.DecoratorLogic);
            this.m_decoratorLogic.Construct(node.DecoratorLogic);
            if (node.BTNode != null)
            {
                this.m_child = MyBehaviorTreeNodeFactory.CreateBTNode(node.BTNode);
                this.m_child.Construct(node.BTNode, treeDesc);
            }
        }

        public override void DebugDraw(Vector2 position, Vector2 size, List<MyBehaviorTreeNodeMemory> nodesMemory)
        {
        }

        private static IMyDecoratorLogic GetDecoratorLogic(MyObjectBuilder_BehaviorTreeDecoratorNode.Logic logicData) => 
            (!(logicData is MyObjectBuilder_BehaviorTreeDecoratorNode.TimerLogic) ? (!(logicData is MyObjectBuilder_BehaviorTreeDecoratorNode.CounterLogic) ? null : ((IMyDecoratorLogic) new MyBehaviorTreeDecoratorCounterLogic())) : ((IMyDecoratorLogic) new MyBehaviorTreeDecoratorTimerLogic()));

        public override int GetHashCode() => 
            ((((((((base.GetHashCode() * 0x18d) ^ this.m_child.GetHashCode()) * 0x18d) ^ this.m_decoratorLogic.GetHashCode()) * 0x18d) ^ this.m_decoratorLogicName.GetHashCode()) * 0x18d) ^ this.DecoratorDefaultReturnValue.GetHashCode());

        public string GetName() => 
            this.m_decoratorLogicName;

        public override MyBehaviorTreeNodeMemory GetNewMemoryObject()
        {
            MyBehaviorTreeDecoratorNodeMemory newMemoryObject = base.GetNewMemoryObject() as MyBehaviorTreeDecoratorNodeMemory;
            newMemoryObject.DecoratorLogicMemory = this.m_decoratorLogic.GetNewMemoryObject();
            return newMemoryObject;
        }

        public override void PostTick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            base.PostTick(bot, botTreeMemory);
            MyBehaviorTreeDecoratorNodeMemory nodeMemoryByIndex = botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex) as MyBehaviorTreeDecoratorNodeMemory;
            if (nodeMemoryByIndex.ChildState == MyBehaviorTreeState.NOT_TICKED)
            {
                if (this.IsRunningStateSource)
                {
                    nodeMemoryByIndex.PostTickMemory();
                }
            }
            else
            {
                nodeMemoryByIndex.PostTickMemory();
                if (this.m_child != null)
                {
                    this.m_child.PostTick(bot, botTreeMemory);
                }
            }
        }

        [Conditional("DEBUG")]
        private void RecordRunningNodeName(MyBehaviorTreeState state)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS && (state == MyBehaviorTreeState.RUNNING))
            {
                base.m_runningActionName = this.m_child.m_runningActionName;
            }
        }

        public override MyBehaviorTreeState Tick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            MyBehaviorTreeDecoratorNodeMemory nodeMemoryByIndex = botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex) as MyBehaviorTreeDecoratorNodeMemory;
            if (this.m_child != null)
            {
                if (nodeMemoryByIndex.ChildState == MyBehaviorTreeState.RUNNING)
                {
                    return this.TickChild(bot, botTreeMemory, nodeMemoryByIndex);
                }
                this.m_decoratorLogic.Update(nodeMemoryByIndex.DecoratorLogicMemory);
                if (this.m_decoratorLogic.CanRun(nodeMemoryByIndex.DecoratorLogicMemory))
                {
                    return this.TickChild(bot, botTreeMemory, nodeMemoryByIndex);
                }
                if (this.IsRunningStateSource)
                {
                    bot.BotMemory.ProcessLastRunningNode(this);
                }
                botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex).NodeState = this.m_defaultReturnValue;
                if (MyDebugDrawSettings.DEBUG_DRAW_BOTS && (this.m_defaultReturnValue == MyBehaviorTreeState.RUNNING))
                {
                    base.m_runningActionName = "Par_N" + this.m_decoratorLogicName;
                }
            }
            return this.m_defaultReturnValue;
        }

        private MyBehaviorTreeState TickChild(IMyBot bot, MyPerTreeBotMemory botTreeMemory, MyBehaviorTreeDecoratorNodeMemory thisMemory)
        {
            bot.BotMemory.RememberNode(this.m_child.MemoryIndex);
            MyBehaviorTreeState state = this.m_child.Tick(bot, botTreeMemory);
            thisMemory.NodeState = state;
            thisMemory.ChildState = state;
            if (state != MyBehaviorTreeState.RUNNING)
            {
                bot.BotMemory.ForgetNode();
            }
            return state;
        }

        public override string ToString() => 
            ("DEC: " + this.m_decoratorLogic.ToString());

        private MyDecoratorDefaultReturnValues DecoratorDefaultReturnValue =>
            ((MyDecoratorDefaultReturnValues) ((byte) this.m_defaultReturnValue));

        public override bool IsRunningStateSource =>
            (this.m_defaultReturnValue == MyBehaviorTreeState.RUNNING);
    }
}

