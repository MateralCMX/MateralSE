namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    public class MySearchByCategoryCondition : IMySearchCondition
    {
        public List<MyGuiBlockCategoryDefinition> SelectedCategories;
        private MyGuiBlockCategoryDefinition m_lastCategory;
        private HashSet<MyCubeBlockDefinitionGroup> m_sortedBlocks = new HashSet<MyCubeBlockDefinitionGroup>();
        private Dictionary<string, List<MyCubeBlockDefinitionGroup>> m_blocksByCategories = new Dictionary<string, List<MyCubeBlockDefinitionGroup>>();

        public void AddDefinitionGroup(MyCubeBlockDefinitionGroup definitionGruop)
        {
            if (this.m_lastCategory != null)
            {
                List<MyCubeBlockDefinitionGroup> list = null;
                if (!this.m_blocksByCategories.TryGetValue(this.m_lastCategory.Name, out list))
                {
                    list = new List<MyCubeBlockDefinitionGroup>();
                    this.m_blocksByCategories.Add(this.m_lastCategory.Name, list);
                }
                list.Add(definitionGruop);
            }
        }

        public void CleanDefinitionGroups()
        {
            this.m_sortedBlocks.Clear();
            this.m_blocksByCategories.Clear();
        }

        public HashSet<MyCubeBlockDefinitionGroup> GetSortedBlocks()
        {
            foreach (KeyValuePair<string, List<MyCubeBlockDefinitionGroup>> pair in this.m_blocksByCategories)
            {
                foreach (MyCubeBlockDefinitionGroup group in pair.Value)
                {
                    this.m_sortedBlocks.Add(group);
                }
            }
            return this.m_sortedBlocks;
        }

        private bool IsItemInAnySelectedCategory(string itemId)
        {
            this.m_lastCategory = null;
            if (this.SelectedCategories == null)
            {
                return true;
            }
            using (List<MyGuiBlockCategoryDefinition>.Enumerator enumerator = this.SelectedCategories.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiBlockCategoryDefinition current = enumerator.Current;
                    if (!current.HasItem(itemId))
                    {
                        if (!current.ShowAnimations)
                        {
                            continue;
                        }
                        if (!itemId.Contains("AnimationDefinition"))
                        {
                            continue;
                        }
                    }
                    this.m_lastCategory = current;
                    return true;
                }
            }
            return false;
        }

        public bool MatchesCondition(string itemId) => 
            this.IsItemInAnySelectedCategory(itemId);

        public bool MatchesCondition(MyDefinitionBase itemId) => 
            this.IsItemInAnySelectedCategory(itemId.Id.ToString());
    }
}

