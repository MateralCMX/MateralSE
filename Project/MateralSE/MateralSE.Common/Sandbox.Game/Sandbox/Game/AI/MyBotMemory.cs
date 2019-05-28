namespace Sandbox.Game.AI
{
    using Sandbox.Game.AI.BehaviorTree;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public class MyBotMemory
    {
        private IMyBot m_memoryUser;
        private MyBehaviorTree m_behaviorTree;
        private MyPerTreeBotMemory m_treeBotMemory;
        private Stack<int> m_newNodePath;
        private HashSet<int> m_oldNodePath;

        public MyBotMemory(IMyBot bot)
        {
            this.LastRunningNodeIndex = -1;
            this.m_memoryUser = bot;
            this.m_newNodePath = new Stack<int>(20);
            this.m_oldNodePath = new HashSet<int>();
        }

        public void AssignBehaviorTree(MyBehaviorTree behaviorTree)
        {
            if ((this.CurrentTreeBotMemory == null) && ((this.m_behaviorTree == null) || (behaviorTree.BehaviorTreeId == this.m_behaviorTree.BehaviorTreeId)))
            {
                this.CurrentTreeBotMemory = this.CreateBehaviorTreeMemory(behaviorTree);
            }
            else if (!this.ValidateMemoryForBehavior(behaviorTree))
            {
                this.CurrentTreeBotMemory.Clear();
                this.ClearPathMemory(false);
                this.ResetMemoryInternal(behaviorTree, this.CurrentTreeBotMemory);
            }
            this.m_behaviorTree = behaviorTree;
        }

        private void ClearOldPath()
        {
            this.m_oldNodePath.Clear();
            this.LastRunningNodeIndex = -1;
        }

        public void ClearPathMemory(bool postTick)
        {
            if (postTick)
            {
                this.PostTickPaths();
            }
            this.m_newNodePath.Clear();
            this.m_oldNodePath.Clear();
            this.LastRunningNodeIndex = -1;
        }

        public MyBotMemory Clone()
        {
            MyBotMemory memory1 = new MyBotMemory(this.m_memoryUser);
            memory1.m_behaviorTree = this.m_behaviorTree;
            MyObjectBuilder_BotMemory builder = new MyObjectBuilder_BotMemory();
            builder = this.GetObjectBuilder();
            memory1.Init(builder);
            return memory1;
        }

        private MyPerTreeBotMemory CreateBehaviorTreeMemory(MyBehaviorTree behaviorTree)
        {
            MyPerTreeBotMemory treeMemory = new MyPerTreeBotMemory();
            this.ResetMemoryInternal(behaviorTree, treeMemory);
            return treeMemory;
        }

        public void ForgetNode()
        {
            this.m_newNodePath.Pop();
        }

        public MyObjectBuilder_BotMemory GetObjectBuilder()
        {
            MyObjectBuilder_BotMemory memory = new MyObjectBuilder_BotMemory {
                LastRunningNodeIndex = this.LastRunningNodeIndex,
                NewPath = this.m_newNodePath.ToList<int>(),
                OldPath = this.m_oldNodePath.ToList<int>()
            };
            MyObjectBuilder_BotMemory.BehaviorTreeNodesMemory memory2 = new MyObjectBuilder_BotMemory.BehaviorTreeNodesMemory {
                BehaviorName = this.m_behaviorTree.BehaviorTreeName,
                Memory = new List<MyObjectBuilder_BehaviorTreeNodeMemory>(this.CurrentTreeBotMemory.NodesMemoryCount)
            };
            foreach (MyBehaviorTreeNodeMemory memory3 in this.CurrentTreeBotMemory.NodesMemory)
            {
                memory2.Memory.Add(memory3.GetObjectBuilder());
            }
            memory2.BlackboardMemory = new List<MyObjectBuilder_BotMemory.BehaviorTreeBlackboardMemory>();
            foreach (KeyValuePair<MyStringId, MyBBMemoryValue> pair in this.CurrentTreeBotMemory.BBMemory)
            {
                MyObjectBuilder_BotMemory.BehaviorTreeBlackboardMemory item = new MyObjectBuilder_BotMemory.BehaviorTreeBlackboardMemory();
                item.MemberName = pair.Key.ToString();
                item.Value = pair.Value;
                memory2.BlackboardMemory.Add(item);
            }
            memory.BehaviorTreeMemory = memory2;
            return memory;
        }

        public void Init(MyObjectBuilder_BotMemory builder)
        {
            if (builder.BehaviorTreeMemory != null)
            {
                MyPerTreeBotMemory memory = new MyPerTreeBotMemory();
                foreach (MyObjectBuilder_BehaviorTreeNodeMemory memory2 in builder.BehaviorTreeMemory.Memory)
                {
                    MyBehaviorTreeNodeMemory nodeMemory = MyBehaviorTreeNodeMemoryFactory.CreateNodeMemory(memory2);
                    nodeMemory.Init(memory2);
                    memory.AddNodeMemory(nodeMemory);
                }
                if (builder.BehaviorTreeMemory.BlackboardMemory != null)
                {
                    foreach (MyObjectBuilder_BotMemory.BehaviorTreeBlackboardMemory memory4 in builder.BehaviorTreeMemory.BlackboardMemory)
                    {
                        memory.AddBlackboardMemoryInstance(memory4.MemberName, memory4.Value);
                    }
                }
                this.CurrentTreeBotMemory = memory;
            }
            if (builder.OldPath != null)
            {
                for (int i = 0; i < builder.OldPath.Count; i++)
                {
                    this.m_oldNodePath.Add(i);
                }
            }
            if (builder.NewPath != null)
            {
                for (int i = 0; i < builder.NewPath.Count; i++)
                {
                    this.m_newNodePath.Push(builder.NewPath[i]);
                }
            }
            this.LastRunningNodeIndex = builder.LastRunningNodeIndex;
            this.TickCounter = 0;
        }

        private void PostTickOldPath()
        {
            if (this.HasOldPath)
            {
                this.m_oldNodePath.ExceptWith(this.m_newNodePath);
                this.m_behaviorTree.CallPostTickOnPath(this.m_memoryUser, this.CurrentTreeBotMemory, this.m_oldNodePath);
                this.ClearOldPath();
            }
        }

        private void PostTickPaths()
        {
            if (this.m_behaviorTree != null)
            {
                this.m_behaviorTree.CallPostTickOnPath(this.m_memoryUser, this.CurrentTreeBotMemory, this.m_oldNodePath);
                this.m_behaviorTree.CallPostTickOnPath(this.m_memoryUser, this.CurrentTreeBotMemory, this.m_newNodePath);
            }
        }

        public void PrepareForNewNodePath()
        {
            this.m_oldNodePath.Clear();
            this.m_oldNodePath.UnionWith(this.m_newNodePath);
            this.LastRunningNodeIndex = this.m_newNodePath.Peek();
            this.m_newNodePath.Clear();
        }

        public void PreTickClear()
        {
            if (this.HasPathToSave)
            {
                this.PrepareForNewNodePath();
            }
            this.CurrentTreeBotMemory.ClearNodesData();
            this.TickCounter++;
        }

        public void ProcessLastRunningNode(MyBehaviorTreeNode node)
        {
            if (this.LastRunningNodeIndex != -1)
            {
                if (this.LastRunningNodeIndex != node.MemoryIndex)
                {
                    this.PostTickOldPath();
                }
                else
                {
                    this.ClearOldPath();
                }
            }
        }

        public void RememberNode(int nodeIndex)
        {
            this.m_newNodePath.Push(nodeIndex);
        }

        public void ResetMemory(bool clearMemory = false)
        {
            if (this.m_behaviorTree != null)
            {
                if (clearMemory)
                {
                    this.ClearPathMemory(true);
                }
                this.CurrentTreeBotMemory.Clear();
                this.ResetMemoryInternal(this.m_behaviorTree, this.CurrentTreeBotMemory);
            }
        }

        private void ResetMemoryInternal(MyBehaviorTree behaviorTree, MyPerTreeBotMemory treeMemory)
        {
            for (int i = 0; i < behaviorTree.TotalNodeCount; i++)
            {
                treeMemory.AddNodeMemory(behaviorTree.GetNodeByIndex(i).GetNewMemoryObject());
            }
        }

        public void UnassignCurrentBehaviorTree()
        {
            this.ClearPathMemory(true);
            this.CurrentTreeBotMemory = null;
            this.m_behaviorTree = null;
        }

        public bool ValidateMemoryForBehavior(MyBehaviorTree behaviorTree)
        {
            bool flag = true;
            if (this.CurrentTreeBotMemory.NodesMemoryCount != behaviorTree.TotalNodeCount)
            {
                flag = false;
            }
            else
            {
                for (int i = 0; i < this.CurrentTreeBotMemory.NodesMemoryCount; i++)
                {
                    if (this.CurrentTreeBotMemory.GetNodeMemoryByIndex(i).GetType() != behaviorTree.GetNodeByIndex(i).MemoryType)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            return flag;
        }

        public MyPerTreeBotMemory CurrentTreeBotMemory
        {
            get => 
                this.m_treeBotMemory;
            private set => 
                (this.m_treeBotMemory = value);
        }

        public bool HasOldPath =>
            (this.m_oldNodePath.Count > 0);

        public int LastRunningNodeIndex { get; private set; }

        public bool HasPathToSave =>
            (this.m_newNodePath.Count > 0);

        public int TickCounter { get; private set; }
    }
}

