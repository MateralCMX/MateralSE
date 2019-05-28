namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Groups;

    public class MyGridNoDamageGroupData : IGroupData<MyCubeGrid>
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

