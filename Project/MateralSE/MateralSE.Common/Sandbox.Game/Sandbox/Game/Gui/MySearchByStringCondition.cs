namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    public class MySearchByStringCondition : IMySearchCondition
    {
        private string[] m_searchItems;
        private HashSet<MyCubeBlockDefinitionGroup> m_sortedBlocks = new HashSet<MyCubeBlockDefinitionGroup>();

        public void AddDefinitionGroup(MyCubeBlockDefinitionGroup definitionGruop)
        {
            this.m_sortedBlocks.Add(definitionGruop);
        }

        public void Clean()
        {
            this.m_searchItems = null;
            this.CleanDefinitionGroups();
        }

        public void CleanDefinitionGroups()
        {
            this.m_sortedBlocks.Clear();
        }

        public HashSet<MyCubeBlockDefinitionGroup> GetSortedBlocks() => 
            this.m_sortedBlocks;

        public bool MatchesCondition(string itemId)
        {
            foreach (string str in this.m_searchItems)
            {
                if (!itemId.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        public bool MatchesCondition(MyDefinitionBase itemId) => 
            ((itemId != null) && this.MatchesCondition(itemId.DisplayNameText.ToString()));

        public string SearchName
        {
            set
            {
                char[] separator = new char[] { ' ' };
                this.m_searchItems = value.Split(separator);
            }
        }

        public bool IsValid =>
            (this.m_searchItems != null);
    }
}

