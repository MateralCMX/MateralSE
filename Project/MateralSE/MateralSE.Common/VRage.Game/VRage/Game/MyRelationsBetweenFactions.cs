namespace VRage.Game
{
    using System;

    public enum MyRelationsBetweenFactions
    {
        Neutral = 0,
        Enemies = 1,
        [Obsolete("Not used in our code, it's here for backwards compatibility")]
        Allies = 2
    }
}

