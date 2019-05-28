namespace VRage.Game
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyRelationsBetweenPlayerAndBlockExtensions
    {
        public static bool IsFriendly(this MyRelationsBetweenPlayerAndBlock relations) => 
            ((relations == MyRelationsBetweenPlayerAndBlock.NoOwnership) || ((relations == MyRelationsBetweenPlayerAndBlock.Owner) || (relations == MyRelationsBetweenPlayerAndBlock.FactionShare)));
    }
}

