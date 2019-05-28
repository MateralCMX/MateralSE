namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    public interface IMySearchCondition
    {
        void AddDefinitionGroup(MyCubeBlockDefinitionGroup definitionGruop);
        void CleanDefinitionGroups();
        HashSet<MyCubeBlockDefinitionGroup> GetSortedBlocks();
        bool MatchesCondition(string itemId);
        bool MatchesCondition(MyDefinitionBase itemId);
    }
}

