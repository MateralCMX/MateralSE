namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class MyWeaponBehavior
    {
        public string Name = "No name";
        public int Priority = 10;
        public float TimeMin = 2f;
        public float TimeMax = 4f;
        public bool IgnoresVoxels;
        public bool IgnoresGrids;
        [XmlArrayItem("WeaponRule")]
        public List<MyWeaponRule> WeaponRules;
        [XmlArrayItem("Weapon")]
        public List<string> Requirements;
        public bool RequirementsIsWhitelist;
    }
}

