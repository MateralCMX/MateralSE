namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;

    public class MyWeaponRule
    {
        public string Weapon = "";
        public float TimeMin = 2f;
        public float TimeMax = 3f;
        public bool FiringAfterLosingSight = true;
        public bool CanGoThroughVoxels;
    }
}

