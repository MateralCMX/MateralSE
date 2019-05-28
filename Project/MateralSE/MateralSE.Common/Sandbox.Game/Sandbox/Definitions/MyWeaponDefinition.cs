namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_WeaponDefinition), (Type) null)]
    public class MyWeaponDefinition : MyDefinitionBase
    {
        private static readonly string ErrorMessageTemplate = "No weapon ammo data specified for {0} ammo (<{1}AmmoData> tag is missing in weapon definition)";
        public MySoundPair NoAmmoSound;
        public MySoundPair ReloadSound;
        public MySoundPair SecondarySound;
        public float DeviateShotAngle;
        public float ReleaseTimeAfterFire;
        public int MuzzleFlashLifeSpan;
        public MyDefinitionId[] AmmoMagazinesId;
        public MyWeaponAmmoData[] WeaponAmmoDatas;
        public MyWeaponEffect[] WeaponEffects;
        public MyStringHash PhysicalMaterial;
        public bool UseDefaultMuzzleFlash;
        public int ReloadTime = 0x7d0;
        public float DamageMultiplier = 1f;
        public float RangeMultiplier = 1f;
        public bool UseRandomizedRange = true;

        public int GetAmmoMagazineIdArrayIndex(MyDefinitionId ammoMagazineId)
        {
            for (int i = 0; i < this.AmmoMagazinesId.Length; i++)
            {
                if (ammoMagazineId.SubtypeId == this.AmmoMagazinesId[i].SubtypeId)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasAmmoMagazines() => 
            ((this.AmmoMagazinesId != null) && (this.AmmoMagazinesId.Length != 0));

        public bool HasSpecificAmmoData(MyAmmoDefinition ammoDefinition) => 
            (this.WeaponAmmoDatas[(int) ammoDefinition.AmmoType] != null);

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WeaponDefinition definition = builder as MyObjectBuilder_WeaponDefinition;
            this.WeaponAmmoDatas = new MyWeaponAmmoData[Enum.GetValues(typeof(MyAmmoType)).Length];
            this.WeaponEffects = new MyWeaponEffect[(definition.Effects == null) ? 0 : definition.Effects.Length];
            if (definition.Effects != null)
            {
                for (int j = 0; j < definition.Effects.Length; j++)
                {
                    this.WeaponEffects[j] = new MyWeaponEffect(definition.Effects[j].Action, definition.Effects[j].Dummy, definition.Effects[j].Particle, definition.Effects[j].Loop, definition.Effects[j].InstantStop);
                }
            }
            this.PhysicalMaterial = MyStringHash.GetOrCompute(definition.PhysicalMaterial);
            this.UseDefaultMuzzleFlash = definition.UseDefaultMuzzleFlash;
            this.NoAmmoSound = new MySoundPair(definition.NoAmmoSoundName, true);
            this.ReloadSound = new MySoundPair(definition.ReloadSoundName, true);
            this.SecondarySound = new MySoundPair(definition.SecondarySoundName, true);
            this.DeviateShotAngle = MathHelper.ToRadians(definition.DeviateShotAngle);
            this.ReleaseTimeAfterFire = definition.ReleaseTimeAfterFire;
            this.MuzzleFlashLifeSpan = definition.MuzzleFlashLifeSpan;
            this.ReloadTime = definition.ReloadTime;
            this.DamageMultiplier = definition.DamageMultiplier;
            this.RangeMultiplier = definition.RangeMultiplier;
            this.UseRandomizedRange = definition.UseRandomizedRange;
            this.AmmoMagazinesId = new MyDefinitionId[definition.AmmoMagazines.Length];
            for (int i = 0; i < this.AmmoMagazinesId.Length; i++)
            {
                MyObjectBuilder_WeaponDefinition.WeaponAmmoMagazine magazine = definition.AmmoMagazines[i];
                this.AmmoMagazinesId[i] = new MyDefinitionId(magazine.Type, magazine.Subtype);
                MyAmmoMagazineDefinition ammoMagazineDefinition = MyDefinitionManager.Static.GetAmmoMagazineDefinition(this.AmmoMagazinesId[i]);
                MyAmmoType ammoType = MyDefinitionManager.Static.GetAmmoDefinition(ammoMagazineDefinition.AmmoDefinitionId).AmmoType;
                string str = null;
                if (ammoType == MyAmmoType.HighSpeed)
                {
                    if (definition.ProjectileAmmoData != null)
                    {
                        this.WeaponAmmoDatas[0] = new MyWeaponAmmoData(definition.ProjectileAmmoData);
                    }
                    else
                    {
                        str = string.Format(ErrorMessageTemplate, "projectile", "Projectile");
                    }
                }
                else
                {
                    if (ammoType != MyAmmoType.Missile)
                    {
                        throw new NotImplementedException();
                    }
                    if (definition.MissileAmmoData != null)
                    {
                        this.WeaponAmmoDatas[1] = new MyWeaponAmmoData(definition.MissileAmmoData);
                    }
                    else
                    {
                        str = string.Format(ErrorMessageTemplate, "missile", "Missile");
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    MyDefinitionErrors.Add(base.Context, str, TErrorSeverity.Critical, true);
                }
            }
        }

        public bool IsAmmoMagazineCompatible(MyDefinitionId ammoMagazineDefinitionId)
        {
            for (int i = 0; i < this.AmmoMagazinesId.Length; i++)
            {
                if (ammoMagazineDefinitionId.SubtypeId == this.AmmoMagazinesId[i].SubtypeId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasProjectileAmmoDefined =>
            (this.WeaponAmmoDatas[0] != null);

        public bool HasMissileAmmoDefined =>
            (this.WeaponAmmoDatas[1] != null);

        public class MyWeaponAmmoData
        {
            public int RateOfFire;
            public int ShotsInBurst;
            public MySoundPair ShootSound;
            public int ShootIntervalInMiliseconds;

            public MyWeaponAmmoData(MyObjectBuilder_WeaponDefinition.WeaponAmmoData data) : this(data.RateOfFire, data.ShootSoundName, data.ShotsInBurst)
            {
            }

            public MyWeaponAmmoData(int rateOfFire, string soundName, int shotsInBurst)
            {
                this.RateOfFire = rateOfFire;
                this.ShotsInBurst = shotsInBurst;
                this.ShootSound = new MySoundPair(soundName, true);
                this.ShootIntervalInMiliseconds = (int) (1000f / (((float) this.RateOfFire) / 60f));
            }
        }

        public class MyWeaponEffect
        {
            public MyWeaponDefinition.WeaponEffectAction Action;
            public string Dummy = "";
            public string Particle = "";
            public bool Loop;
            public bool InstantStop;

            public MyWeaponEffect(string action, string dummy, string particle, bool loop, bool instantStop)
            {
                this.Dummy = dummy;
                this.Particle = particle;
                this.Loop = loop;
                this.InstantStop = instantStop;
                foreach (MyWeaponDefinition.WeaponEffectAction action2 in Enum.GetValues(typeof(MyWeaponDefinition.WeaponEffectAction)))
                {
                    if (action2.ToString().Equals(action))
                    {
                        this.Action = action2;
                        break;
                    }
                }
            }
        }

        public enum WeaponEffectAction
        {
            Unknown,
            Shoot
        }
    }
}

