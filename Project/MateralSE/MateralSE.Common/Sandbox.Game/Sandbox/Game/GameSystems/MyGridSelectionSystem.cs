namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Network;
    using VRageMath;

    public class MyGridSelectionSystem
    {
        private HashSet<IMyGunObject<MyDeviceBase>> m_currentGuns = new HashSet<IMyGunObject<MyDeviceBase>>();
        private MyDefinitionId? m_gunId;
        private bool m_useSingleGun;
        private MyShipController m_shipController;
        private MyGridWeaponSystem m_weaponSystem;
        private int m_gunTimer;
        private int m_gunTimer_Max;
        private static int PerFrameMax = 10;
        private int m_curentDrawHudIndex;

        public MyGridSelectionSystem(MyShipController shipController)
        {
            this.m_shipController = shipController;
        }

        internal void BeginShoot(MyShootActionEnum action)
        {
            foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
            {
                if (obj2.EnabledInWorldRules)
                {
                    obj2.BeginShoot(action);
                    continue;
                }
                if (MyEventContext.Current.IsLocallyInvoked || (MyMultiplayer.Static == null))
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.WeaponDisabledInWorldSettings);
                }
            }
        }

        internal bool CanShoot(MyShootActionEnum action, out MyGunStatusEnum status, out IMyGunObject<MyDeviceBase> FailedGun)
        {
            FailedGun = null;
            if (this.m_currentGuns == null)
            {
                status = MyGunStatusEnum.NotSelected;
                return false;
            }
            bool flag = false;
            status = MyGunStatusEnum.OK;
            foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
            {
                MyGunStatusEnum enum2;
                flag |= obj2.CanShoot(action, (this.m_shipController.ControllerInfo.Controller != null) ? this.m_shipController.ControllerInfo.Controller.Player.Identity.IdentityId : this.m_shipController.OwnerId, out enum2);
                if (enum2 != MyGunStatusEnum.OK)
                {
                    FailedGun = obj2;
                    status = enum2;
                }
            }
            return flag;
        }

        public bool CanSwitchAmmoMagazine()
        {
            bool flag = true;
            if (this.m_currentGuns != null)
            {
                using (HashSet<IMyGunObject<MyDeviceBase>>.Enumerator enumerator = this.m_currentGuns.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        IMyGunObject<MyDeviceBase> current = enumerator.Current;
                        if (current.GunBase != null)
                        {
                            flag &= current.GunBase.CanSwitchAmmoMagazine();
                            continue;
                        }
                        return false;
                    }
                }
            }
            return flag;
        }

        internal void DrawHud(IMyCameraController camera, long playerId)
        {
            if (this.m_currentGuns != null)
            {
                if (this.m_gunTimer <= 0)
                {
                    this.m_gunTimer_Max = (this.m_currentGuns.Count / PerFrameMax) + 1;
                    this.m_gunTimer = this.m_gunTimer_Max;
                }
                this.m_gunTimer--;
                foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
                {
                    obj2.DrawHud(camera, playerId, ((obj2.GetHashCode() + ((int) MySandboxGame.Static.SimulationFrameCounter)) % this.m_gunTimer_Max) == 0);
                }
            }
        }

        internal void EndShoot(MyShootActionEnum action)
        {
            foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
            {
                if (obj2.EnabledInWorldRules)
                {
                    obj2.EndShoot(action);
                    continue;
                }
                if (MyEventContext.Current.IsLocallyInvoked || (MyMultiplayer.Static == null))
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.WeaponDisabledInWorldSettings);
                }
            }
        }

        public MyDefinitionId? GetGunId() => 
            this.m_gunId;

        internal void OnControlAcquired()
        {
            if (this.m_currentGuns != null)
            {
                this.SwitchTo(this.m_gunId, this.m_useSingleGun);
                using (HashSet<IMyGunObject<MyDeviceBase>>.Enumerator enumerator = this.m_currentGuns.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnControlAcquired(this.m_shipController.Pilot);
                    }
                }
            }
        }

        internal void OnControlReleased()
        {
            if (this.m_currentGuns != null)
            {
                using (HashSet<IMyGunObject<MyDeviceBase>>.Enumerator enumerator = this.m_currentGuns.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnControlReleased();
                    }
                }
            }
        }

        internal void Shoot(MyShootActionEnum action)
        {
            foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
            {
                MyGunStatusEnum enum2;
                if (!obj2.EnabledInWorldRules)
                {
                    if (!MyEventContext.Current.IsLocallyInvoked && (MyMultiplayer.Static != null))
                    {
                        continue;
                    }
                    MyHud.Notifications.Add(MyNotificationSingletons.WeaponDisabledInWorldSettings);
                    continue;
                }
                if (obj2.CanShoot(action, (this.m_shipController.ControllerInfo.Controller != null) ? this.m_shipController.ControllerInfo.ControllingIdentityId : this.m_shipController.OwnerId, out enum2))
                {
                    Vector3D? overrideWeaponPos = null;
                    obj2.Shoot(action, (Vector3) ((MyEntity) obj2).WorldMatrix.Forward, overrideWeaponPos, null);
                }
            }
        }

        internal void SwitchAmmoMagazine()
        {
            foreach (IMyGunObject<MyDeviceBase> obj2 in this.m_currentGuns)
            {
                if (obj2.EnabledInWorldRules)
                {
                    obj2.GunBase.SwitchToNextAmmoMagazine();
                    continue;
                }
                if (MyEventContext.Current.IsLocallyInvoked || (MyMultiplayer.Static == null))
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.WeaponDisabledInWorldSettings);
                }
            }
        }

        internal void SwitchTo(MyDefinitionId? gunId, bool useSingle = false)
        {
            HashSet<IMyGunObject<MyDeviceBase>>.Enumerator enumerator;
            this.m_gunId = gunId;
            this.m_useSingleGun = useSingle;
            if (this.m_currentGuns != null)
            {
                using (enumerator = this.m_currentGuns.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnControlReleased();
                    }
                }
            }
            if (gunId == null)
            {
                this.m_currentGuns.Clear();
            }
            else
            {
                this.m_currentGuns.Clear();
                if (useSingle)
                {
                    IMyGunObject<MyDeviceBase> gunWithAmmo = this.WeaponSystem.GetGunWithAmmo(gunId.Value, this.m_shipController.OwnerId);
                    if (gunWithAmmo != null)
                    {
                        this.m_currentGuns.Add(gunWithAmmo);
                    }
                }
                else
                {
                    HashSet<IMyGunObject<MyDeviceBase>> gunsById = this.WeaponSystem.GetGunsById(gunId.Value);
                    if (gunsById != null)
                    {
                        foreach (IMyGunObject<MyDeviceBase> obj3 in gunsById)
                        {
                            if (obj3 != null)
                            {
                                this.m_currentGuns.Add(obj3);
                            }
                        }
                    }
                }
                using (enumerator = this.m_currentGuns.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnControlAcquired(this.m_shipController.Pilot);
                    }
                }
            }
        }

        private void WeaponSystem_WeaponRegistered(MyGridWeaponSystem sender, MyGridWeaponSystem.EventArgs args)
        {
            if (this.m_shipController.Pilot != null)
            {
                MyDefinitionId definitionId = args.Weapon.DefinitionId;
                MyDefinitionId? gunId = this.m_gunId;
                if ((gunId != null) ? (definitionId == gunId.GetValueOrDefault()) : false)
                {
                    if (!this.m_useSingleGun)
                    {
                        args.Weapon.OnControlAcquired(this.m_shipController.Pilot);
                        this.m_currentGuns.Add(args.Weapon);
                    }
                    else if (this.m_currentGuns.Count < 1)
                    {
                        args.Weapon.OnControlAcquired(this.m_shipController.Pilot);
                        this.m_currentGuns.Add(args.Weapon);
                    }
                }
            }
        }

        private void WeaponSystem_WeaponUnregistered(MyGridWeaponSystem sender, MyGridWeaponSystem.EventArgs args)
        {
            if (this.m_shipController.Pilot != null)
            {
                MyDefinitionId definitionId = args.Weapon.DefinitionId;
                MyDefinitionId? gunId = this.m_gunId;
                if (((gunId != null) ? (definitionId == gunId.GetValueOrDefault()) : false) && this.m_currentGuns.Contains(args.Weapon))
                {
                    args.Weapon.OnControlReleased();
                    this.m_currentGuns.Remove(args.Weapon);
                }
            }
        }

        public MyGridWeaponSystem WeaponSystem
        {
            get => 
                this.m_weaponSystem;
            set
            {
                if (!ReferenceEquals(this.m_weaponSystem, value))
                {
                    if (this.m_weaponSystem != null)
                    {
                        this.m_weaponSystem.WeaponRegistered -= new Action<MyGridWeaponSystem, MyGridWeaponSystem.EventArgs>(this.WeaponSystem_WeaponRegistered);
                        this.m_weaponSystem.WeaponUnregistered -= new Action<MyGridWeaponSystem, MyGridWeaponSystem.EventArgs>(this.WeaponSystem_WeaponUnregistered);
                    }
                    this.m_weaponSystem = value;
                    if (this.m_weaponSystem != null)
                    {
                        this.m_weaponSystem.WeaponRegistered += new Action<MyGridWeaponSystem, MyGridWeaponSystem.EventArgs>(this.WeaponSystem_WeaponRegistered);
                        this.m_weaponSystem.WeaponUnregistered += new Action<MyGridWeaponSystem, MyGridWeaponSystem.EventArgs>(this.WeaponSystem_WeaponUnregistered);
                    }
                }
            }
        }
    }
}

