namespace Sandbox.Game.Weapons
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public class MyWeaponPropertiesWrapper
    {
        private MyWeaponDefinition m_weaponDefinition;
        private MyAmmoDefinition m_ammoDefinition;
        private MyAmmoMagazineDefinition m_ammoMagazineDefinition;

        public MyWeaponPropertiesWrapper(MyDefinitionId weaponDefinitionId)
        {
            this.WeaponDefinitionId = weaponDefinitionId;
            this.m_weaponDefinition = MyDefinitionManager.Static.GetWeaponDefinition(this.WeaponDefinitionId);
        }

        public bool CanChangeAmmoMagazine(MyDefinitionId newAmmoMagazineId) => 
            this.WeaponDefinition.IsAmmoMagazineCompatible(newAmmoMagazineId);

        public void ChangeAmmoMagazine(MyDefinitionId newAmmoMagazineId)
        {
            this.AmmoMagazineId = newAmmoMagazineId;
            this.m_ammoMagazineDefinition = MyDefinitionManager.Static.GetAmmoMagazineDefinition(this.AmmoMagazineId);
            this.AmmoDefinitionId = this.AmmoMagazineDefinition.AmmoDefinitionId;
            this.m_ammoDefinition = MyDefinitionManager.Static.GetAmmoDefinition(this.AmmoDefinitionId);
        }

        public T GetCurrentAmmoDefinitionAs<T>() where T: MyAmmoDefinition => 
            (this.AmmoDefinition as T);

        public MyDefinitionId WeaponDefinitionId { get; private set; }

        public MyDefinitionId AmmoMagazineId { get; private set; }

        public MyDefinitionId AmmoDefinitionId { get; private set; }

        public MyAmmoDefinition AmmoDefinition =>
            this.m_ammoDefinition;

        public MyWeaponDefinition WeaponDefinition =>
            this.m_weaponDefinition;

        public MyAmmoMagazineDefinition AmmoMagazineDefinition =>
            this.m_ammoMagazineDefinition;

        public int AmmoMagazinesCount =>
            this.WeaponDefinition.AmmoMagazinesId.Length;

        public bool IsAmmoProjectile =>
            (this.AmmoDefinition.AmmoType == MyAmmoType.HighSpeed);

        public bool IsAmmoMissile =>
            (this.AmmoDefinition.AmmoType == MyAmmoType.Missile);

        public bool IsDeviated =>
            !(this.WeaponDefinition.DeviateShotAngle == 0f);

        public int CurrentWeaponRateOfFire =>
            this.m_weaponDefinition.WeaponAmmoDatas[(int) this.AmmoDefinition.AmmoType].RateOfFire;

        public int ShotsInBurst =>
            this.m_weaponDefinition.WeaponAmmoDatas[(int) this.AmmoDefinition.AmmoType].ShotsInBurst;

        public int ReloadTime =>
            this.m_weaponDefinition.ReloadTime;

        public int CurrentWeaponShootIntervalInMiliseconds =>
            this.m_weaponDefinition.WeaponAmmoDatas[(int) this.AmmoDefinition.AmmoType].ShootIntervalInMiliseconds;

        public MySoundPair CurrentWeaponShootSound =>
            this.m_weaponDefinition.WeaponAmmoDatas[(int) this.AmmoDefinition.AmmoType].ShootSound;
    }
}

