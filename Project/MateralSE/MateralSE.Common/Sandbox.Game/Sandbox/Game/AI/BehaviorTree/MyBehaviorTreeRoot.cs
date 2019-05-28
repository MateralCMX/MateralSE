namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyBehaviorTreeRoot : MyBehaviorTreeNode
    {
        private MyBehaviorTreeNode m_child;

        public override void Construct(MyObjectBuilder_BehaviorTreeNode nodeDefinition, MyBehaviorTree.MyBehaviorTreeDesc treeDesc)
        {
            base.Construct(nodeDefinition, treeDesc);
            this.m_child = MyBehaviorTreeNodeFactory.CreateBTNode(nodeDefinition);
            this.m_child.Construct(nodeDefinition, treeDesc);
        }

        public override unsafe void DebugDraw(Vector2 pos, Vector2 size, List<MyBehaviorTreeNodeMemory> nodesMemory)
        {
            MyRenderProxy.DebugDrawText2D(pos, "ROOT", nodesMemory[base.MemoryIndex].NodeStateColor, MyBehaviorTreeNode.DEBUG_TEXT_SCALE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
            float* singlePtr1 = (float*) ref pos.Y;
            singlePtr1[0] += MyBehaviorTreeNode.DEBUG_ROOT_OFFSET;
            this.m_child.DebugDraw(pos, size, nodesMemory);
        }

        public override int GetHashCode() => 
            this.m_child.GetHashCode();

        public override MyBehaviorTreeNodeMemory GetNewMemoryObject() => 
            new MyBehaviorTreeNodeMemory();

        [Conditional("DEBUG")]
        private void RecordRunningNodeName(IMyBot bot, MyBehaviorTreeState state)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS && (bot is MyAgentBot))
            {
                switch (state)
                {
                    case MyBehaviorTreeState.ERROR:
                        (bot as MyAgentBot).LastActions.AddLastAction("error");
                        return;

                    case MyBehaviorTreeState.NOT_TICKED:
                        (bot as MyAgentBot).LastActions.AddLastAction("not ticked");
                        return;

                    case MyBehaviorTreeState.SUCCESS:
                        (bot as MyAgentBot).LastActions.AddLastAction("failure");
                        return;

                    case MyBehaviorTreeState.FAILURE:
                        (bot as MyAgentBot).LastActions.AddLastAction("failure");
                        return;

                    case MyBehaviorTreeState.RUNNING:
                        (bot as MyAgentBot).LastActions.AddLastAction(this.m_child.m_runningActionName);
                        return;
                }
            }
        }

        public override MyBehaviorTreeState Tick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            bot.BotMemory.RememberNode(this.m_child.MemoryIndex);
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                bot.LastBotMemory = bot.BotMemory.Clone();
            }
            MyBehaviorTreeState state = this.m_child.Tick(bot, botTreeMemory);
            botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex).NodeState = state;
            if (state != MyBehaviorTreeState.RUNNING)
            {
                bot.BotMemory.ForgetNode();
            }
            return state;
        }

        public override bool IsRunningStateSource =>
            false;
    }
}

