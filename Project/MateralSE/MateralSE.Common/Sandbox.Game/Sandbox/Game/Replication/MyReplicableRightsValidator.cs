namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Network;
    using VRageMath;

    internal static class MyReplicableRightsValidator
    {
        private static float ALLOWED_PHYSICAL_DISTANCE_SQUARED = ((MyConstants.DEFAULT_INTERACTIVE_DISTANCE * 3f) * (MyConstants.DEFAULT_INTERACTIVE_DISTANCE * 3f));

        public static bool GetAccess(MyCharacterReplicable characterReplicable, Vector3D characterPosition, MyCubeGrid grid, MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group, bool physical)
        {
            if ((characterReplicable == null) || (grid == null))
            {
                return false;
            }
            if (group != null)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                {
                    bool flag;
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            return false;
                        }
                        MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node current = enumerator.Current;
                        if (physical && (current.NodeData.PositionComp.WorldAABB.DistanceSquared(characterPosition) <= ALLOWED_PHYSICAL_DISTANCE_SQUARED))
                        {
                            flag = true;
                        }
                        else
                        {
                            if (!characterReplicable.CachedParentDependencies.Contains(current.NodeData))
                            {
                                continue;
                            }
                            flag = true;
                        }
                        break;
                    }
                    return flag;
                }
            }
            if (!physical || (grid.PositionComp.WorldAABB.DistanceSquared(characterPosition) > ALLOWED_PHYSICAL_DISTANCE_SQUARED))
            {
                return characterReplicable.CachedParentDependencies.Contains(grid);
            }
            return true;
        }

        public static bool GetBigOwner(MyCubeGrid grid, EndpointId endpointId, long identityId, bool spaceMaster)
        {
            if (grid == null)
            {
                return false;
            }
            bool flag = (grid.BigOwners.Count == 0) || grid.BigOwners.Contains(identityId);
            if (spaceMaster)
            {
                flag |= MySession.Static.IsUserSpaceMaster(endpointId.Value);
            }
            return flag;
        }

        public static ValidationResult GetControlled(MyEntity controlledEntity, EndpointId endpointId)
        {
            if (controlledEntity != null)
            {
                MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(controlledEntity);
                if (((controllingPlayer != null) && (controllingPlayer.Client.SteamUserId == endpointId.Value)) || MySession.Static.IsUserAdmin(endpointId.Value))
                {
                    return ValidationResult.Passed;
                }
                controllingPlayer = MySession.Static.Players.GetPreviousControllingPlayer(controlledEntity);
                if (((controllingPlayer != null) && (controllingPlayer.Client.SteamUserId == endpointId.Value)) || MySession.Static.IsUserAdmin(endpointId.Value))
                {
                    return ValidationResult.Controlled;
                }
            }
            return (ValidationResult.Controlled | ValidationResult.Kick);
        }
    }
}

