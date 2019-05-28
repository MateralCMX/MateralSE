namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    [MyBehaviorTreeNodeType(typeof(MyObjectBuilder_BehaviorTreeNode))]
    public abstract class MyBehaviorTreeNode
    {
        protected static float DEBUG_TEXT_SCALE = 0.5f;
        protected static float DEBUG_TEXT_Y_OFFSET = 60f;
        protected static float DEBUG_SCALE = 0.4f;
        protected static float DEBUG_ROOT_OFFSET = 20f;
        protected static float DEBUG_LINE_OFFSET_MULT = 25f;
        public const string ParentName = "Par_N";
        public string m_runningActionName = "";

        public MyBehaviorTreeNode()
        {
            foreach (object obj2 in base.GetType().GetCustomAttributes(false))
            {
                if (obj2.GetType() == typeof(MyBehaviorTreeNodeTypeAttribute))
                {
                    MyBehaviorTreeNodeTypeAttribute attribute = obj2 as MyBehaviorTreeNodeTypeAttribute;
                    this.MemoryType = attribute.MemoryType;
                }
            }
        }

        public virtual void Construct(MyObjectBuilder_BehaviorTreeNode nodeDefinition, MyBehaviorTree.MyBehaviorTreeDesc treeDesc)
        {
            int memorableNodesCounter = treeDesc.MemorableNodesCounter;
            treeDesc.MemorableNodesCounter = memorableNodesCounter + 1;
            this.MemoryIndex = memorableNodesCounter;
            treeDesc.Nodes.Add(this);
        }

        public abstract void DebugDraw(Vector2 position, Vector2 size, List<MyBehaviorTreeNodeMemory> nodesMemory);
        public override int GetHashCode() => 
            this.MemoryIndex;

        public virtual MyBehaviorTreeNodeMemory GetNewMemoryObject()
        {
            if ((this.MemoryType == null) || (!this.MemoryType.IsSubclassOf(typeof(MyBehaviorTreeNodeMemory)) && (this.MemoryType != typeof(MyBehaviorTreeNodeMemory))))
            {
                return new MyBehaviorTreeNodeMemory();
            }
            return (Activator.CreateInstance(this.MemoryType) as MyBehaviorTreeNodeMemory);
        }

        public virtual void PostTick(IMyBot bot, MyPerTreeBotMemory nodesMemory)
        {
        }

        public abstract MyBehaviorTreeState Tick(IMyBot bot, MyPerTreeBotMemory nodesMemory);

        public int MemoryIndex { get; private set; }

        public Type MemoryType { get; private set; }

        public abstract bool IsRunningStateSource { get; }
    }
}

