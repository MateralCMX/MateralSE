namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MySessionComponentEquivalency : MySessionComponentBase
    {
        public static MySessionComponentEquivalency Static;
        private Dictionary<MyDefinitionId, HashSet<MyDefinitionId>> m_equivalencyGroups;
        private Dictionary<MyDefinitionId, MyDefinitionId> m_groupMain;
        private HashSet<MyDefinitionId> m_forcedMain;

        public MySessionComponentEquivalency()
        {
            Static = this;
            this.m_equivalencyGroups = new Dictionary<MyDefinitionId, HashSet<MyDefinitionId>>(MyDefinitionId.Comparer);
            this.m_groupMain = new Dictionary<MyDefinitionId, MyDefinitionId>(MyDefinitionId.Comparer);
            this.m_forcedMain = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
        }

        public MyDefinitionId Convert(MyDefinitionId id) => 
            (!this.m_forcedMain.Contains(id) ? id : this.m_groupMain[id]);

        public bool ForceMainElement(MyDefinitionId id) => 
            this.m_forcedMain.Contains(id);

        public HashSet<MyDefinitionId> GetEquivalents(MyDefinitionId id)
        {
            HashSet<MyDefinitionId> set;
            MyDefinitionId mainElement = this.GetMainElement(id);
            return (!this.m_equivalencyGroups.TryGetValue(mainElement, out set) ? null : set);
        }

        public MyDefinitionId GetMainElement(MyDefinitionId id)
        {
            MyDefinitionId id2;
            return (!this.m_groupMain.TryGetValue(id, out id2) ? id : id2);
        }

        public bool HasEquivalents(MyDefinitionId id) => 
            this.m_equivalencyGroups.ContainsKey(this.GetMainElement(id));

        public bool IsProvided(Dictionary<MyDefinitionId, MyFixedPoint> itemCounts, MyDefinitionId required, int amount = 1)
        {
            if (amount == 0)
            {
                return true;
            }
            HashSet<MyDefinitionId> equivalents = this.GetEquivalents(required);
            if (equivalents == null)
            {
                return false;
            }
            int num = 0;
            foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> pair in itemCounts)
            {
                if (equivalents.Contains(pair.Key))
                {
                    num += pair.Value.ToIntSafe();
                }
            }
            return (num >= amount);
        }

        public override void LoadData()
        {
            base.LoadData();
            this.m_equivalencyGroups = new Dictionary<MyDefinitionId, HashSet<MyDefinitionId>>(MyDefinitionId.Comparer);
            this.m_groupMain = new Dictionary<MyDefinitionId, MyDefinitionId>(MyDefinitionId.Comparer);
            this.m_forcedMain = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
            foreach (MyEquivalencyGroupDefinition definition in MyDefinitionManager.Static.GetDefinitions<MyEquivalencyGroupDefinition>())
            {
                HashSet<MyDefinitionId> set;
                MyDefinitionId mainElement = definition.MainElement;
                if (!this.m_equivalencyGroups.TryGetValue(mainElement, out set))
                {
                    set = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
                }
                foreach (MyDefinitionId id2 in definition.Equivalents)
                {
                    this.m_groupMain[id2] = mainElement;
                    if (definition.ForceMainElement)
                    {
                        this.m_forcedMain.Add(id2);
                    }
                    set.Add(id2);
                }
                set.Add(mainElement);
                this.m_equivalencyGroups[mainElement] = set;
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_equivalencyGroups = null;
            this.m_groupMain = null;
            this.m_forcedMain = null;
        }

        public override bool IsRequiredByGame =>
            (MyPerGameSettings.Game == GameEnum.ME_GAME);
    }
}

