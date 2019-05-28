namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;

    public class MyIDModule
    {
        public MyIDModule() : this(0L, MyOwnershipShareModeEnum.None)
        {
        }

        public MyIDModule(long owner, MyOwnershipShareModeEnum shareMode)
        {
            this.Owner = owner;
            this.ShareMode = shareMode;
        }

        public static MyRelationsBetweenPlayerAndBlock GetRelation(long owner, long user, MyOwnershipShareModeEnum share = 0, MyRelationsBetweenPlayerAndBlock noFactionResult = 4, MyRelationsBetweenFactions defaultFactionRelations = 1, MyRelationsBetweenPlayerAndBlock defaultShareWithAllRelations = 2)
        {
            if (!MyFakes.SHOW_FACTIONS_GUI)
            {
                return MyRelationsBetweenPlayerAndBlock.NoOwnership;
            }
            if (owner == 0)
            {
                return MyRelationsBetweenPlayerAndBlock.NoOwnership;
            }
            if (owner == user)
            {
                return MyRelationsBetweenPlayerAndBlock.Owner;
            }
            IMyFaction objA = MySession.Static.Factions.TryGetPlayerFaction(user);
            IMyFaction objB = MySession.Static.Factions.TryGetPlayerFaction(owner);
            if (((objA == null) || !ReferenceEquals(objA, objB)) || (share != MyOwnershipShareModeEnum.Faction))
            {
                return ((share != MyOwnershipShareModeEnum.All) ? ((objA != null) ? ((objB != null) ? ((MySession.Static.Factions.GetRelationBetweenFactions(objB.FactionId, objA.FactionId, defaultFactionRelations) != MyRelationsBetweenFactions.Neutral) ? MyRelationsBetweenPlayerAndBlock.Enemies : MyRelationsBetweenPlayerAndBlock.Neutral) : noFactionResult) : noFactionResult) : defaultShareWithAllRelations);
            }
            return MyRelationsBetweenPlayerAndBlock.FactionShare;
        }

        public MyRelationsBetweenPlayerAndBlock GetUserRelationToOwner(long identityId) => 
            GetRelation(this.Owner, identityId, this.ShareMode, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare);

        public long Owner { get; set; }

        public MyOwnershipShareModeEnum ShareMode { get; set; }
    }
}

