namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Utils;

    public class MyBehaviorTree
    {
        private static List<MyStringId> m_tmpHelper = new List<MyStringId>();
        private MyBehaviorTreeNode m_root;
        private MyBehaviorTreeDesc m_treeDesc;
        private MyBehaviorDefinition m_behaviorDefinition;

        public MyBehaviorTree(MyBehaviorDefinition def)
        {
            this.m_behaviorDefinition = def;
            this.m_treeDesc = new MyBehaviorTreeDesc();
        }

        public void CallPostTickOnPath(IMyBot bot, MyPerTreeBotMemory botTreeMemory, IEnumerable<int> postTickNodes)
        {
            foreach (int num in postTickNodes)
            {
                this.m_treeDesc.Nodes[num].PostTick(bot, botTreeMemory);
            }
        }

        public void ClearData()
        {
            this.m_treeDesc.MemorableNodesCounter = 0;
            this.m_treeDesc.ActionIds.Clear();
            this.m_treeDesc.Nodes.Clear();
        }

        public void Construct()
        {
            this.ClearData();
            this.m_root = new MyBehaviorTreeRoot();
            this.m_root.Construct(this.m_behaviorDefinition.FirstNode, this.m_treeDesc);
        }

        public override int GetHashCode() => 
            this.m_root.GetHashCode();

        public MyBehaviorTreeNode GetNodeByIndex(int index) => 
            ((index < this.m_treeDesc.Nodes.Count) ? this.m_treeDesc.Nodes[index] : null);

        public bool IsCompatibleWithBot(ActionCollection botActions)
        {
            foreach (MyStringId id in this.m_treeDesc.ActionIds)
            {
                if (!botActions.ContainsActionDesc(id))
                {
                    m_tmpHelper.Add(id);
                }
            }
            if (m_tmpHelper.Count <= 0)
            {
                return true;
            }
            StringBuilder builder = new StringBuilder("Error! The behavior tree is not compatible with the bot. Missing bot actions: ");
            foreach (MyStringId id2 in m_tmpHelper)
            {
                builder.Append(id2.ToString());
                builder.Append(", ");
            }
            m_tmpHelper.Clear();
            return false;
        }

        public void ReconstructTree(MyBehaviorDefinition def)
        {
            this.m_behaviorDefinition = def;
            this.Construct();
        }

        public void Tick(IMyBot bot)
        {
            this.m_root.Tick(bot, bot.BotMemory.CurrentTreeBotMemory);
        }

        public int TotalNodeCount =>
            this.m_treeDesc.Nodes.Count;

        public MyBehaviorDefinition BehaviorDefinition =>
            this.m_behaviorDefinition;

        public string BehaviorTreeName =>
            this.m_behaviorDefinition.Id.SubtypeName;

        public MyStringHash BehaviorTreeId =>
            this.m_behaviorDefinition.Id.SubtypeId;

        public class MyBehaviorTreeDesc
        {
            public MyBehaviorTreeDesc()
            {
                this.Nodes = new List<MyBehaviorTreeNode>(20);
                this.ActionIds = new HashSet<MyStringId>(MyStringId.Comparer);
                this.MemorableNodesCounter = 0;
            }

            public List<MyBehaviorTreeNode> Nodes { get; private set; }

            public HashSet<MyStringId> ActionIds { get; private set; }

            public int MemorableNodesCounter { get; set; }
        }
    }
}

