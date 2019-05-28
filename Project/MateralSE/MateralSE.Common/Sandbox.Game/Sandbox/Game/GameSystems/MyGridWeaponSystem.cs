namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Utils;

    public class MyGridWeaponSystem
    {
        private Dictionary<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>> m_gunsByDefId = new Dictionary<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>>(5, MyDefinitionId.Comparer);
        [CompilerGenerated]
        private Action<MyGridWeaponSystem, EventArgs> WeaponRegistered;
        [CompilerGenerated]
        private Action<MyGridWeaponSystem, EventArgs> WeaponUnregistered;

        public event Action<MyGridWeaponSystem, EventArgs> WeaponRegistered
        {
            [CompilerGenerated] add
            {
                Action<MyGridWeaponSystem, EventArgs> weaponRegistered = this.WeaponRegistered;
                while (true)
                {
                    Action<MyGridWeaponSystem, EventArgs> a = weaponRegistered;
                    Action<MyGridWeaponSystem, EventArgs> action3 = (Action<MyGridWeaponSystem, EventArgs>) Delegate.Combine(a, value);
                    weaponRegistered = Interlocked.CompareExchange<Action<MyGridWeaponSystem, EventArgs>>(ref this.WeaponRegistered, action3, a);
                    if (ReferenceEquals(weaponRegistered, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGridWeaponSystem, EventArgs> weaponRegistered = this.WeaponRegistered;
                while (true)
                {
                    Action<MyGridWeaponSystem, EventArgs> source = weaponRegistered;
                    Action<MyGridWeaponSystem, EventArgs> action3 = (Action<MyGridWeaponSystem, EventArgs>) Delegate.Remove(source, value);
                    weaponRegistered = Interlocked.CompareExchange<Action<MyGridWeaponSystem, EventArgs>>(ref this.WeaponRegistered, action3, source);
                    if (ReferenceEquals(weaponRegistered, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGridWeaponSystem, EventArgs> WeaponUnregistered
        {
            [CompilerGenerated] add
            {
                Action<MyGridWeaponSystem, EventArgs> weaponUnregistered = this.WeaponUnregistered;
                while (true)
                {
                    Action<MyGridWeaponSystem, EventArgs> a = weaponUnregistered;
                    Action<MyGridWeaponSystem, EventArgs> action3 = (Action<MyGridWeaponSystem, EventArgs>) Delegate.Combine(a, value);
                    weaponUnregistered = Interlocked.CompareExchange<Action<MyGridWeaponSystem, EventArgs>>(ref this.WeaponUnregistered, action3, a);
                    if (ReferenceEquals(weaponUnregistered, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGridWeaponSystem, EventArgs> weaponUnregistered = this.WeaponUnregistered;
                while (true)
                {
                    Action<MyGridWeaponSystem, EventArgs> source = weaponUnregistered;
                    Action<MyGridWeaponSystem, EventArgs> action3 = (Action<MyGridWeaponSystem, EventArgs>) Delegate.Remove(source, value);
                    weaponUnregistered = Interlocked.CompareExchange<Action<MyGridWeaponSystem, EventArgs>>(ref this.WeaponUnregistered, action3, source);
                    if (ReferenceEquals(weaponUnregistered, source))
                    {
                        return;
                    }
                }
            }
        }

        public IMyGunObject<MyDeviceBase> GetGun(MyDefinitionId defId) => 
            (!this.m_gunsByDefId.ContainsKey(defId) ? null : this.m_gunsByDefId[defId].FirstOrDefault<IMyGunObject<MyDeviceBase>>());

        public HashSet<IMyGunObject<MyDeviceBase>> GetGunsById(MyDefinitionId gunId) => 
            (!this.m_gunsByDefId.ContainsKey(gunId) ? null : this.m_gunsByDefId[gunId]);

        public Dictionary<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>> GetGunSets() => 
            this.m_gunsByDefId;

        internal IMyGunObject<MyDeviceBase> GetGunWithAmmo(MyDefinitionId gunId, long shooter)
        {
            if (!this.m_gunsByDefId.ContainsKey(gunId))
            {
                return null;
            }
            IMyGunObject<MyDeviceBase> obj2 = this.m_gunsByDefId[gunId].FirstOrDefault<IMyGunObject<MyDeviceBase>>();
            foreach (IMyGunObject<MyDeviceBase> obj3 in this.m_gunsByDefId[gunId])
            {
                MyGunStatusEnum enum2;
                if (obj3.CanShoot(MyShootActionEnum.PrimaryAction, shooter, out enum2))
                {
                    obj2 = obj3;
                    break;
                }
            }
            return obj2;
        }

        public bool HasGunsOfId(MyDefinitionId defId) => 
            (this.m_gunsByDefId.ContainsKey(defId) && (this.m_gunsByDefId[defId].Count > 0));

        internal void Register(IMyGunObject<MyDeviceBase> gun)
        {
            if (!this.m_gunsByDefId.ContainsKey(gun.DefinitionId))
            {
                this.m_gunsByDefId.Add(gun.DefinitionId, new HashSet<IMyGunObject<MyDeviceBase>>());
            }
            this.m_gunsByDefId[gun.DefinitionId].Add(gun);
            if (this.WeaponRegistered != null)
            {
                EventArgs args = new EventArgs {
                    Weapon = gun
                };
                this.WeaponRegistered(this, args);
            }
        }

        internal void Unregister(IMyGunObject<MyDeviceBase> gun)
        {
            if (!this.m_gunsByDefId.ContainsKey(gun.DefinitionId))
            {
                MyDebug.FailRelease("deinition ID " + gun.DefinitionId + " not in m_gunsByDefId");
            }
            else
            {
                this.m_gunsByDefId[gun.DefinitionId].Remove(gun);
                if (this.WeaponUnregistered != null)
                {
                    EventArgs args = new EventArgs {
                        Weapon = gun
                    };
                    this.WeaponUnregistered(this, args);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EventArgs
        {
            public IMyGunObject<MyDeviceBase> Weapon;
        }
    }
}

