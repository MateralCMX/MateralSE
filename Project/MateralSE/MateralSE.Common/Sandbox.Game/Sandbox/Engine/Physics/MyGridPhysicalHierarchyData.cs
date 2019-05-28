namespace Sandbox.Engine.Physics
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Groups;

    public class MyGridPhysicalHierarchyData : IGroupData<MyCubeGrid>
    {
        public MyCubeGrid m_root;
        private MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group m_group;

        public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
            this.m_group = group as MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group;
        }

        public void OnNodeAdded(MyCubeGrid entity)
        {
        }

        public void OnNodeRemoved(MyCubeGrid entity)
        {
        }

        public void OnRelease()
        {
            this.m_root = null;
            this.m_group = null;
        }
    }
}

