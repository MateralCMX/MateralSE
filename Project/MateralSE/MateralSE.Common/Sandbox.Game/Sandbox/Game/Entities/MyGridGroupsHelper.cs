namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game.ModAPI;

    public class MyGridGroupsHelper : IMyGridGroups
    {
        public List<IMyCubeGrid> GetGroup(IMyCubeGrid node, GridLinkTypeEnum type) => 
            MyCubeGridGroups.Static.GetGroups(type).GetGroupNodes((MyCubeGrid) node).Cast<IMyCubeGrid>().ToList<IMyCubeGrid>();

        public bool HasConnection(IMyCubeGrid grid1, IMyCubeGrid grid2, GridLinkTypeEnum type) => 
            MyCubeGridGroups.Static.GetGroups(type).HasSameGroup((MyCubeGrid) grid1, (MyCubeGrid) grid2);
    }
}

