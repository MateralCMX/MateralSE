namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Library.Utils;
    using VRage.ModAPI;

    internal class MyEntityIdRemapHelper : IMyRemapHelper
    {
        private static int DEFAULT_REMAPPER_SIZE = 0x200;
        private Dictionary<long, long> m_oldToNewMap = new Dictionary<long, long>(DEFAULT_REMAPPER_SIZE);
        private Dictionary<string, Dictionary<int, int>> m_groupMap = new Dictionary<string, Dictionary<int, int>>();

        public void Clear()
        {
            this.m_oldToNewMap.Clear();
            this.m_groupMap.Clear();
        }

        public long RemapEntityId(long oldEntityId)
        {
            long num;
            if (!this.m_oldToNewMap.TryGetValue(oldEntityId, out num))
            {
                num = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                this.m_oldToNewMap.Add(oldEntityId, num);
            }
            return num;
        }

        public int RemapGroupId(string group, int oldValue)
        {
            Dictionary<int, int> dictionary;
            int num;
            if (!this.m_groupMap.TryGetValue(group, out dictionary))
            {
                dictionary = new Dictionary<int, int>();
                this.m_groupMap.Add(group, dictionary);
            }
            if (!dictionary.TryGetValue(oldValue, out num))
            {
                num = MyRandom.Instance.Next();
                dictionary.Add(oldValue, num);
            }
            return num;
        }
    }
}

