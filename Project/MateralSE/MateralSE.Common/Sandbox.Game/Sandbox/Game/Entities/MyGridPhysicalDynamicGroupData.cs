namespace Sandbox.Game.Entities
{
    using System;
    using VRage.Groups;

    public class MyGridPhysicalDynamicGroupData : IGroupData<MyCubeGrid>
    {
        public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
        }

        public void OnNodeAdded(MyCubeGrid entity)
        {
        }

        public void OnNodeRemoved(MyCubeGrid entity)
        {
        }

        public void OnRelease()
        {
        }
    }
}

