namespace Sandbox.Game.Entities
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using VRage.Groups;

    public class MyGridLogicalGroupData : IGroupData<MyCubeGrid>
    {
        internal readonly GridTerminalSystem TerminalSystem;
        internal readonly MyGridWeaponSystem WeaponSystem;
        internal readonly MyResourceDistributorComponent ResourceDistributor;

        public MyGridLogicalGroupData() : this(null)
        {
        }

        public MyGridLogicalGroupData(string debugName)
        {
            this.TerminalSystem = new GridTerminalSystem();
            this.WeaponSystem = new MyGridWeaponSystem();
            this.ResourceDistributor = new MyResourceDistributorComponent(debugName);
        }

        public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
        }

        public void OnNodeAdded(MyCubeGrid entity)
        {
            entity.OnAddedToGroup(this);
        }

        public void OnNodeRemoved(MyCubeGrid entity)
        {
            entity.OnRemovedFromGroup(this);
        }

        public void OnRelease()
        {
        }

        internal void UpdateGridOwnership(List<MyCubeGrid> grids, long ownerID)
        {
            foreach (MyCubeGrid local1 in grids)
            {
                local1.IsAccessibleForProgrammableBlock = local1.BigOwners.Contains(ownerID);
            }
        }
    }
}

