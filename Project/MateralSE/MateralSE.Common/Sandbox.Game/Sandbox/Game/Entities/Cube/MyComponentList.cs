namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;

    public class MyComponentList
    {
        private List<MyTuple<MyDefinitionId, int>> m_displayList = new List<MyTuple<MyDefinitionId, int>>();
        private Dictionary<MyDefinitionId, int> m_totalMaterials = new Dictionary<MyDefinitionId, int>();
        private Dictionary<MyDefinitionId, int> m_requiredMaterials = new Dictionary<MyDefinitionId, int>();

        public void AddMaterial(MyDefinitionId myDefinitionId, int amount, int requiredAmount = 0, bool addToDisplayList = true)
        {
            if (requiredAmount > amount)
            {
                requiredAmount = amount;
            }
            if (addToDisplayList)
            {
                this.m_displayList.Add(new MyTuple<MyDefinitionId, int>(myDefinitionId, amount));
            }
            this.AddToDictionary(this.m_totalMaterials, myDefinitionId, amount);
            if (requiredAmount > 0)
            {
                this.AddToDictionary(this.m_requiredMaterials, myDefinitionId, requiredAmount);
            }
        }

        private void AddToDictionary(Dictionary<MyDefinitionId, int> dict, MyDefinitionId myDefinitionId, int amount)
        {
            int num = 0;
            dict.TryGetValue(myDefinitionId, out num);
            num += amount;
            dict[myDefinitionId] = num;
        }

        public void Clear()
        {
            this.m_displayList.Clear();
            this.m_totalMaterials.Clear();
            this.m_requiredMaterials.Clear();
        }

        public DictionaryReader<MyDefinitionId, int> TotalMaterials =>
            new DictionaryReader<MyDefinitionId, int>(this.m_totalMaterials);

        public DictionaryReader<MyDefinitionId, int> RequiredMaterials =>
            new DictionaryReader<MyDefinitionId, int>(this.m_requiredMaterials);
    }
}

