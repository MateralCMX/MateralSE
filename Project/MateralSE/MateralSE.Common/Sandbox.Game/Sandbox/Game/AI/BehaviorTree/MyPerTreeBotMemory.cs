namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Utils;

    public class MyPerTreeBotMemory
    {
        private List<MyBehaviorTreeNodeMemory> m_nodesMemory = new List<MyBehaviorTreeNodeMemory>(20);
        private Dictionary<MyStringId, MyBBMemoryValue> m_blackboardMemory = new Dictionary<MyStringId, MyBBMemoryValue>(20, MyStringId.Comparer);

        public void AddBlackboardMemoryInstance(string name, MyBBMemoryValue obj)
        {
            MyStringId orCompute = MyStringId.GetOrCompute(name);
            this.m_blackboardMemory.Add(orCompute, obj);
        }

        public void AddNodeMemory(MyBehaviorTreeNodeMemory nodeMemory)
        {
            this.m_nodesMemory.Add(nodeMemory);
        }

        public void Clear()
        {
            this.m_nodesMemory.Clear();
            this.m_blackboardMemory.Clear();
        }

        public void ClearNodesData()
        {
            using (List<MyBehaviorTreeNodeMemory>.Enumerator enumerator = this.m_nodesMemory.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ClearNodeState();
                }
            }
        }

        public MyBehaviorTreeNodeMemory GetNodeMemoryByIndex(int index) => 
            this.m_nodesMemory[index];

        public void RemoveBlackboardMemoryInstance(MyStringId name)
        {
            this.m_blackboardMemory.Remove(name);
        }

        public void SaveToBlackboard(MyStringId id, MyBBMemoryValue value)
        {
            if (id != MyStringId.NullOrEmpty)
            {
                this.m_blackboardMemory[id] = value;
            }
        }

        public bool TryGetFromBlackboard<T>(MyStringId id, out T value) where T: MyBBMemoryValue
        {
            MyBBMemoryValue value2 = null;
            bool flag1 = this.m_blackboardMemory.TryGetValue(id, out value2);
            value = value2 as T;
            return flag1;
        }

        public MyBBMemoryValue TrySaveToBlackboard(MyStringId id, Type type)
        {
            if (!type.IsSubclassOf(typeof(MyBBMemoryValue)) && (type != typeof(MyBBMemoryValue)))
            {
                return null;
            }
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                return null;
            }
            MyBBMemoryValue value2 = Activator.CreateInstance(type) as MyBBMemoryValue;
            this.m_blackboardMemory[id] = value2;
            return value2;
        }

        public int NodesMemoryCount =>
            this.m_nodesMemory.Count;

        public ListReader<MyBehaviorTreeNodeMemory> NodesMemory =>
            new ListReader<MyBehaviorTreeNodeMemory>(this.m_nodesMemory);

        public IEnumerable<KeyValuePair<MyStringId, MyBBMemoryValue>> BBMemory =>
            this.m_blackboardMemory;
    }
}

