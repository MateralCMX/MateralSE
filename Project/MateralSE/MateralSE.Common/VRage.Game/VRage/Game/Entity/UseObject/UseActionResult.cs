namespace VRage.Game.Entity.UseObject
{
    using System;

    public enum UseActionResult
    {
        OK,
        UsedBySomeoneElse,
        AccessDenied,
        Closed,
        Unpowered,
        CockpitDamaged,
        MissingDLC
    }
}

