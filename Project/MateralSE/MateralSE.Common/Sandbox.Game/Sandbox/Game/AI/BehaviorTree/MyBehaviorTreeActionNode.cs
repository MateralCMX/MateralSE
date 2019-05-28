namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorTreeActionNode), typeof(MyBehaviorTreeNodeMemory))]
    internal class MyBehaviorTreeActionNode : MyBehaviorTreeNode
    {
        private MyStringId m_actionName = MyStringId.NullOrEmpty;
        private object[] m_parameters = null;

        public MyBehaviorTreeActionNode()
        {
            this.ReturnsRunning = true;
        }

        public override void Construct(MyObjectBuilder_BehaviorTreeNode nodeDefinition, MyBehaviorTree.MyBehaviorTreeDesc treeDesc)
        {
            base.Construct(nodeDefinition, treeDesc);
            MyObjectBuilder_BehaviorTreeActionNode node = (MyObjectBuilder_BehaviorTreeActionNode) nodeDefinition;
            if (!string.IsNullOrEmpty(node.ActionName))
            {
                this.m_actionName = MyStringId.GetOrCompute(node.ActionName);
                treeDesc.ActionIds.Add(this.m_actionName);
            }
            if (node.Parameters != null)
            {
                MyObjectBuilder_BehaviorTreeActionNode.TypeValue[] parameters = node.Parameters;
                this.m_parameters = new object[parameters.Length];
                for (int i = 0; i < this.m_parameters.Length; i++)
                {
                    MyObjectBuilder_BehaviorTreeActionNode.TypeValue value2 = parameters[i];
                    if (value2 is MyObjectBuilder_BehaviorTreeActionNode.MemType)
                    {
                        this.m_parameters[i] = (VRage.Boxed<MyStringId>) MyStringId.GetOrCompute((string) value2.GetValue());
                    }
                    else
                    {
                        this.m_parameters[i] = value2.GetValue();
                    }
                }
            }
        }

        public override void DebugDraw(Vector2 position, Vector2 size, List<MyBehaviorTreeNodeMemory> nodesMemory)
        {
            MyRenderProxy.DebugDrawText2D(position, "A:" + this.m_actionName.ToString(), nodesMemory[base.MemoryIndex].NodeStateColor, MyBehaviorTreeNode.DEBUG_TEXT_SCALE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
        }

        public string GetActionName() => 
            this.m_actionName.ToString();

        public override int GetHashCode()
        {
            int num = (base.GetHashCode() * 0x18d) ^ this.m_actionName.ToString().GetHashCode();
            if (this.m_parameters != null)
            {
                foreach (object obj2 in this.m_parameters)
                {
                    num = (num * 0x18d) ^ obj2.ToString().GetHashCode();
                }
            }
            return num;
        }

        public override void PostTick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            MyBehaviorTreeNodeMemory nodeMemoryByIndex = botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex);
            if (nodeMemoryByIndex.InitCalled)
            {
                if (bot.ActionCollection.ContainsPostAction(this.m_actionName))
                {
                    bot.ActionCollection.PerformPostAction(bot, this.m_actionName);
                }
                nodeMemoryByIndex.InitCalled = false;
            }
        }

        public override MyBehaviorTreeState Tick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            if (bot.ActionCollection.ReturnsRunning(this.m_actionName))
            {
                bot.BotMemory.ProcessLastRunningNode(this);
            }
            MyBehaviorTreeNodeMemory nodeMemoryByIndex = botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex);
            if (!nodeMemoryByIndex.InitCalled)
            {
                nodeMemoryByIndex.InitCalled = true;
                if (bot.ActionCollection.ContainsInitAction(this.m_actionName))
                {
                    bot.ActionCollection.PerformInitAction(bot, this.m_actionName);
                }
            }
            MyBehaviorTreeState state = bot.ActionCollection.PerformAction(bot, this.m_actionName, this.m_parameters);
            nodeMemoryByIndex.NodeState = state;
            return state;
        }

        public override string ToString() => 
            ("ACTION: " + this.m_actionName.ToString());

        public bool ReturnsRunning { get; private set; }

        public override bool IsRunningStateSource =>
            this.ReturnsRunning;
    }
}

