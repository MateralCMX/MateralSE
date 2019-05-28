namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WeaponDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x25)]
        public WeaponAmmoData ProjectileAmmoData;
        [ProtoMember(40)]
        public WeaponAmmoData MissileAmmoData;
        [ProtoMember(0x2b)]
        public string NoAmmoSoundName;
        [ProtoMember(0x2e)]
        public string ReloadSoundName;
        [ProtoMember(0x31)]
        public string SecondarySoundName;
        [ProtoMember(0x34)]
        public string PhysicalMaterial = "Metal";
        [ProtoMember(0x37)]
        public float DeviateShotAngle;
        [ProtoMember(0x3a)]
        public float ReleaseTimeAfterFire;
        [ProtoMember(0x3d)]
        public int MuzzleFlashLifeSpan;
        [ProtoMember(0x40)]
        public int ReloadTime = 0x7d0;
        [XmlArrayItem("AmmoMagazine"), ProtoMember(0x44)]
        public WeaponAmmoMagazine[] AmmoMagazines;
        [XmlArrayItem("Effect"), ProtoMember(0x48)]
        public WeaponEffect[] Effects;
        [ProtoMember(0x4b)]
        public bool UseDefaultMuzzleFlash = true;
        [ProtoMember(0x61), DefaultValue(1)]
        public float DamageMultiplier = 1f;
        [ProtoMember(100, IsRequired=false), DefaultValue(1)]
        public float RangeMultiplier = 1f;
        [ProtoMember(0x67, IsRequired=false), DefaultValue(true)]
        public bool UseRandomizedRange = true;

        [ProtoContract]
        public class WeaponAmmoData
        {
            [XmlAttribute]
            public int RateOfFire;
            [XmlAttribute]
            public string ShootSoundName;
            [XmlAttribute]
            public int ShotsInBurst;
        }

        [ProtoContract]
        public class WeaponAmmoMagazine
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_AmmoMagazine);
            [XmlAttribute, ProtoMember(0x21)]
            public string Subtype;
        }

        [ProtoContract]
        public class WeaponEffect
        {
            [XmlAttribute, ProtoMember(0x51)]
            public string Action = "";
            [XmlAttribute, ProtoMember(0x54)]
            public string Dummy = "";
            [XmlAttribute, ProtoMember(0x57)]
            public string Particle = "";
            [XmlAttribute, ProtoMember(90)]
            public bool Loop;
            [XmlAttribute, ProtoMember(0x5d, IsRequired=false)]
            public bool InstantStop = true;
        }
    }
}

