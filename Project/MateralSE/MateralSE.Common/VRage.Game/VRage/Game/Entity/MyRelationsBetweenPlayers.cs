namespace VRage.Game.Entity
{
    using System;

    public enum MyRelationsBetweenPlayers
    {
        Self = 0,
        [Obsolete("Vanilla game is not using this value.")]
        Allies = 1,
        Neutral = 2,
        Enemies = 3
    }
}

