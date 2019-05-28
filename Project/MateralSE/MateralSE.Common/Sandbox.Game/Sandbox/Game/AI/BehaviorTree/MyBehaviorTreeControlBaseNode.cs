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

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorControlBaseNode), typeof(MyBehaviorTreeControlNodeMemory))]
    internal abstract class MyBehaviorTreeControlBaseNode : MyBehaviorTreeNode
    {
        protected List<MyBehaviorTreeNode> m_children;
        protected bool m_isMemorable;
        protected string m_name;

        protected MyBehaviorTreeControlBaseNode()
        {
        }

        public override void Construct(MyObjectBuilder_BehaviorTreeNode nodeDefinition, MyBehaviorTree.MyBehaviorTreeDesc treeDesc)
        {
            base.Construct(nodeDefinition, treeDesc);
            MyObjectBuilder_BehaviorControlBaseNode node = (MyObjectBuilder_BehaviorControlBaseNode) nodeDefinition;
            this.m_children = new List<MyBehaviorTreeNode>(node.BTNodes.Length);
            this.m_isMemorable = node.IsMemorable;
            this.m_name = node.Name;
            foreach (MyObjectBuilder_BehaviorTreeNode node2 in node.BTNodes)
            {
                MyBehaviorTreeNode item = MyBehaviorTreeNodeFactory.CreateBTNode(node2);
                item.Construct(node2, treeDesc);
                this.m_children.Add(item);
            }
        }

        public override unsafe void DebugDraw(Vector2 pos, Vector2 size, List<MyBehaviorTreeNodeMemory> nodesMemory)
        {
            MyRenderProxy.DebugDrawText2D(pos, this.DebugSign, nodesMemory[base.MemoryIndex].NodeStateColor, MyBehaviorTreeNode.DEBUG_TEXT_SCALE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
            float* singlePtr1 = (float*) ref size.X;
            singlePtr1[0] *= MyBehaviorTreeNode.DEBUG_SCALE;
            Vector2 position = (this.m_children.Count > 1) ? (pos - (size * 0.5f)) : pos;
            float* singlePtr2 = (float*) ref position.Y;
            singlePtr2[0] += MyBehaviorTreeNode.DEBUG_TEXT_Y_OFFSET;
            float* singlePtr3 = (float*) ref size.X;
            singlePtr3[0] /= (float) Math.Max(this.m_children.Count - 1, 1);
            foreach (MyBehaviorTreeNode node in this.m_children)
            {
                Vector2 vector2 = position - pos;
                vector2.Normalize();
                Vector2 pointTo = position - (vector2 * MyBehaviorTreeNode.DEBUG_LINE_OFFSET_MULT);
                Matrix? projection = null;
                MyRenderProxy.DebugDrawLine2D(pos + (vector2 * MyBehaviorTreeNode.DEBUG_LINE_OFFSET_MULT), pointTo, nodesMemory[node.MemoryIndex].NodeStateColor, nodesMemory[node.MemoryIndex].NodeStateColor, projection, false);
                node.DebugDraw(position, size, nodesMemory);
                float* singlePtr4 = (float*) ref position.X;
                singlePtr4[0] += size.X;
            }
        }

        public override int GetHashCode()
        {
            int num = (((((base.GetHashCode() * 0x18d) ^ this.m_isMemorable.GetHashCode()) * 0x18d) ^ this.SearchedValue.GetHashCode()) * 0x18d) ^ this.FinalValue.GetHashCode();
            for (int i = 0; i < this.m_children.Count; i++)
            {
                num = (num * 0x18d) ^ this.m_children[i].GetHashCode();
            }
            return num;
        }

        public override void PostTick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex).PostTickMemory();
            using (List<MyBehaviorTreeNode>.Enumerator enumerator = this.m_children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.PostTick(bot, botTreeMemory);
                }
            }
        }

        [Conditional("DEBUG")]
        private void RecordRunningNodeName(MyBehaviorTreeState state, MyBehaviorTreeNode node)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                base.m_runningActionName = "";
                if (state == MyBehaviorTreeState.RUNNING)
                {
                    if (node is MyBehaviorTreeActionNode)
                    {
                        base.m_runningActionName = ((MyBehaviorTreeActionNode) node).GetActionName();
                    }
                    else
                    {
                        string runningActionName = node.m_runningActionName;
                        if (runningActionName.Contains("Par_N"))
                        {
                            runningActionName = runningActionName.Replace("Par_N", this.m_name + "-");
                        }
                        base.m_runningActionName = runningActionName;
                    }
                }
            }
        }

        public override MyBehaviorTreeState Tick(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            MyBehaviorTreeState state;
            MyBehaviorTreeControlNodeMemory nodeMemoryByIndex = botTreeMemory.GetNodeMemoryByIndex(base.MemoryIndex) as MyBehaviorTreeControlNodeMemory;
            int initialIndex = nodeMemoryByIndex.InitialIndex;
            while (true)
            {
                if (initialIndex >= this.m_children.Count)
                {
                    nodeMemoryByIndex.NodeState = this.FinalValue;
                    nodeMemoryByIndex.InitialIndex = 0;
                    return this.FinalValue;
                }
                bot.BotMemory.RememberNode(this.m_children[initialIndex].MemoryIndex);
                if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
                {
                    if (this.m_children[initialIndex] is MyBehaviorTreeControlBaseNode)
                    {
                        string name = (this.m_children[initialIndex] as MyBehaviorTreeControlBaseNode).m_name;
                    }
                    else if (this.m_children[initialIndex] is MyBehaviorTreeActionNode)
                    {
                        (this.m_children[initialIndex] as MyBehaviorTreeActionNode).GetActionName();
                    }
                    else if (this.m_children[initialIndex] is MyBehaviorTreeDecoratorNode)
                    {
                        (this.m_children[initialIndex] as MyBehaviorTreeDecoratorNode).GetName();
                    }
                    base.m_runningActionName = "";
                }
                state = this.m_children[initialIndex].Tick(bot, botTreeMemory);
                if ((state == this.SearchedValue) || (state == this.FinalValue))
                {
                    this.m_children[initialIndex].PostTick(bot, botTreeMemory);
                }
                if (state == MyBehaviorTreeState.RUNNING)
                {
                    break;
                }
                if (state == this.SearchedValue)
                {
                    break;
                }
                bot.BotMemory.ForgetNode();
                initialIndex++;
            }
            nodeMemoryByIndex.NodeState = state;
            if (state != MyBehaviorTreeState.RUNNING)
            {
                bot.BotMemory.ForgetNode();
            }
            else if (this.m_isMemorable)
            {
                nodeMemoryByIndex.InitialIndex = initialIndex;
            }
            return state;
        }

        public override string ToString() => 
            this.m_name;

        public abstract MyBehaviorTreeState SearchedValue { get; }

        public abstract MyBehaviorTreeState FinalValue { get; }

        public abstract string DebugSign { get; }

        public override bool IsRunningStateSource =>
            false;
    }
}

