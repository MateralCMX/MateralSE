namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    public class MyGunBase : MyDeviceBase
    {
        public const int AMMO_PER_SHOOT = 1;
        protected MyWeaponPropertiesWrapper m_weaponProperties;
        protected Dictionary<MyDefinitionId, int> m_remainingAmmos = new Dictionary<MyDefinitionId, int>();
        protected VRage.Sync.Sync<int, SyncDirection.FromServer> m_cachedAmmunitionAmount;
        protected Dictionary<int, DummyContainer> m_dummiesByAmmoType = new Dictionary<int, DummyContainer>();
        protected MatrixD m_worldMatrix;
        protected IMyGunBaseUser m_user;
        private List<WeaponEffect> m_activeEffects = new List<WeaponEffect>();
        public Matrix m_holdingDummyMatrix;
        private int m_shotProjectiles;
        private Dictionary<string, MyModelDummy> m_dummies;

        public MyGunBase()
        {
            this.ShootIntervalModifier = 1f;
        }

        private void AddMissile(MyWeaponPropertiesWrapper weaponProperties, Vector3D initialPosition, Vector3 initialVelocity, Vector3 direction)
        {
            if (Sync.IsServer)
            {
                MyMissileAmmoDefinition currentAmmoDefinitionAs = weaponProperties.GetCurrentAmmoDefinitionAs<MyMissileAmmoDefinition>();
                Vector3 deviatedVector = direction;
                if (weaponProperties.IsDeviated)
                {
                    deviatedVector = this.GetDeviatedVector(weaponProperties.WeaponDefinition.DeviateShotAngle, direction);
                    deviatedVector.Normalize();
                }
                if (deviatedVector.IsValid())
                {
                    initialVelocity += deviatedVector * currentAmmoDefinitionAs.MissileInitialSpeed;
                    MyObjectBuilder_Missile builder = MyMissile.PrepareBuilder(weaponProperties, initialPosition, initialVelocity, deviatedVector, this.m_user.OwnerId, this.m_user.Owner.EntityId, (this.m_user.Launcher as MyEntity).EntityId);
                    this.m_user.Launcher.ShootMissile(builder);
                }
            }
        }

        public void AddMuzzleMatrix(MyAmmoType ammoType, Matrix localMatrix)
        {
            int key = (int) ammoType;
            if (!this.m_dummiesByAmmoType.ContainsKey(key))
            {
                this.m_dummiesByAmmoType[key] = new DummyContainer();
            }
            this.m_dummiesByAmmoType[key].Dummies.Add(MatrixD.Normalize(localMatrix));
        }

        private void AddProjectile(MyWeaponPropertiesWrapper weaponProperties, Vector3D initialPosition, Vector3D initialVelocity, Vector3D direction, MyEntity owner)
        {
            Vector3 directionNormalized = (Vector3) direction;
            if (weaponProperties.IsDeviated)
            {
                directionNormalized = this.GetDeviatedVector(weaponProperties.WeaponDefinition.DeviateShotAngle, (Vector3) direction);
                directionNormalized.Normalize();
            }
            if (directionNormalized.IsValid())
            {
                this.m_shotProjectiles++;
                MyProjectiles.Add(weaponProperties, initialPosition, (Vector3) initialVelocity, directionNormalized, this.m_user, owner);
            }
        }

        public override bool CanSwitchAmmoMagazine() => 
            ((this.m_weaponProperties != null) && this.m_weaponProperties.WeaponDefinition.HasAmmoMagazines());

        public void ConsumeAmmo()
        {
            if (Sync.IsServer)
            {
                this.CurrentAmmo--;
                if ((this.CurrentAmmo < 0) && this.HasEnoughAmmunition())
                {
                    this.CurrentAmmo = this.WeaponProperties.AmmoMagazineDefinition.Capacity - 1;
                    if (!MySession.Static.InfiniteAmmo)
                    {
                        this.m_user.AmmoInventory.RemoveItemsOfType(1, this.CurrentAmmoMagazineId, MyItemFlags.None, false);
                    }
                }
                this.RefreshAmmunitionAmount();
            }
            MyInventory ammoInventory = this.m_user.AmmoInventory;
            if (ammoInventory != null)
            {
                MyPhysicalInventoryItem? itemByID = null;
                if (base.InventoryItemId != null)
                {
                    itemByID = ammoInventory.GetItemByID(base.InventoryItemId.Value);
                }
                else
                {
                    itemByID = ammoInventory.FindUsableItem(this.m_user.PhysicalItemId);
                    if (itemByID != null)
                    {
                        base.InventoryItemId = new uint?(itemByID.Value.ItemId);
                    }
                }
                if (itemByID != null)
                {
                    MyObjectBuilder_PhysicalGunObject content = itemByID.Value.Content as MyObjectBuilder_PhysicalGunObject;
                    if (content != null)
                    {
                        IMyObjectBuilder_GunObject<MyObjectBuilder_GunBase> gunEntity = content.GunEntity as IMyObjectBuilder_GunObject<MyObjectBuilder_GunBase>;
                        if (gunEntity != null)
                        {
                            if (gunEntity.DeviceBase == null)
                            {
                                gunEntity.InitializeDeviceBase<MyObjectBuilder_GunBase>(this.GetObjectBuilder());
                            }
                            else
                            {
                                gunEntity.GetDevice<MyObjectBuilder_GunBase>().RemainingAmmo = this.CurrentAmmo;
                            }
                        }
                    }
                }
            }
        }

        public MyInventoryConstraint CreateAmmoInventoryConstraints(string displayName)
        {
            StringBuilder builder1 = new StringBuilder();
            builder1.AppendFormat(MyTexts.GetString(MySpaceTexts.ToolTipItemFilter_AmmoMagazineInput), displayName);
            MyInventoryConstraint constraint = new MyInventoryConstraint(builder1.ToString(), null, true);
            foreach (MyDefinitionId id in this.m_weaponProperties.WeaponDefinition.AmmoMagazinesId)
            {
                constraint.Add(id);
            }
            return constraint;
        }

        public void CreateEffects(MyWeaponDefinition.WeaponEffectAction action)
        {
            if (((this.m_dummies != null) && (this.m_dummies.Count > 0)) && (this.WeaponProperties.WeaponDefinition.WeaponEffects.Length != 0))
            {
                for (int i = 0; i < this.WeaponProperties.WeaponDefinition.WeaponEffects.Length; i++)
                {
                    MyModelDummy dummy;
                    if ((this.WeaponProperties.WeaponDefinition.WeaponEffects[i].Action == action) && this.m_dummies.TryGetValue(this.WeaponProperties.WeaponDefinition.WeaponEffects[i].Dummy, out dummy))
                    {
                        bool flag = true;
                        string effectName = string.Empty;
                        effectName = this.WeaponProperties.WeaponDefinition.WeaponEffects[i].Particle;
                        if (this.WeaponProperties.WeaponDefinition.WeaponEffects[i].Loop)
                        {
                            for (int j = 0; j < this.m_activeEffects.Count; j++)
                            {
                                if ((this.m_activeEffects[j].DummyName == dummy.Name) && (this.m_activeEffects[j].EffectName == effectName))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            MyParticleEffect effect;
                            MatrixD xd = MatrixD.Normalize(dummy.Matrix);
                            if (MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.Multiply(xd, this.WorldMatrix), out effect) && this.WeaponProperties.WeaponDefinition.WeaponEffects[i].Loop)
                            {
                                this.m_activeEffects.Add(new WeaponEffect(effectName, dummy.Name, (Matrix) xd, action, effect, this.WeaponProperties.WeaponDefinition.WeaponEffects[i].InstantStop));
                            }
                        }
                    }
                }
            }
        }

        public int DummiesPerType(MyAmmoType ammoType) => 
            (!this.m_dummiesByAmmoType.ContainsKey((int) ammoType) ? 0 : this.m_dummiesByAmmoType[(int) ammoType].Dummies.Count);

        private MyDefinitionId GetBackwardCompatibleDefinitionId(MyObjectBuilderType typeId)
        {
            if (typeId == typeof(MyObjectBuilder_LargeGatlingTurret))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "LargeGatlingTurret");
            }
            if (typeId == typeof(MyObjectBuilder_LargeMissileTurret))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "LargeMissileTurret");
            }
            if (typeId == typeof(MyObjectBuilder_InteriorTurret))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "LargeInteriorTurret");
            }
            if ((typeId == typeof(MyObjectBuilder_SmallMissileLauncher)) || (typeId == typeof(MyObjectBuilder_SmallMissileLauncherReload)))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "SmallMissileLauncher");
            }
            if (typeId == typeof(MyObjectBuilder_SmallGatlingGun))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "GatlingGun");
            }
            return new MyDefinitionId();
        }

        public Vector3 GetDeviatedVector(float deviateAngle, Vector3 direction) => 
            MyUtilRandomVector3ByDeviatingVector.GetRandom(direction, deviateAngle);

        public int GetInventoryAmmoMagazinesCount() => 
            ((int) this.m_user.AmmoInventory.GetItemAmount(this.CurrentAmmoMagazineId, MyItemFlags.None, false));

        public MatrixD GetMuzzleLocalMatrix()
        {
            DummyContainer container;
            return ((this.m_weaponProperties.AmmoDefinition != null) ? (!this.m_dummiesByAmmoType.TryGetValue((int) this.m_weaponProperties.AmmoDefinition.AmmoType, out container) ? MatrixD.Identity : container.DummyToUse) : MatrixD.Identity);
        }

        public override Vector3D GetMuzzleLocalPosition()
        {
            DummyContainer container;
            if ((this.m_weaponProperties.AmmoDefinition != null) && this.m_dummiesByAmmoType.TryGetValue((int) this.m_weaponProperties.AmmoDefinition.AmmoType, out container))
            {
                return container.DummyToUse.Translation;
            }
            return Vector3D.Zero;
        }

        public MatrixD GetMuzzleWorldMatrix()
        {
            DummyContainer container;
            if (this.m_weaponProperties.AmmoDefinition == null)
            {
                return this.m_worldMatrix;
            }
            if (!this.m_dummiesByAmmoType.TryGetValue((int) this.m_weaponProperties.AmmoDefinition.AmmoType, out container))
            {
                return this.m_worldMatrix;
            }
            if (container.Dirty)
            {
                container.DummyInWorld = container.DummyToUse * this.m_worldMatrix;
                container.Dirty = false;
            }
            return container.DummyInWorld;
        }

        public override Vector3D GetMuzzleWorldPosition()
        {
            DummyContainer container;
            if (this.m_weaponProperties.AmmoDefinition == null)
            {
                return this.m_worldMatrix.Translation;
            }
            if (!this.m_dummiesByAmmoType.TryGetValue((int) this.m_weaponProperties.AmmoDefinition.AmmoType, out container))
            {
                return this.m_worldMatrix.Translation;
            }
            if (container.Dirty)
            {
                container.DummyInWorld = container.DummyToUse * this.m_worldMatrix;
                container.Dirty = false;
            }
            return container.DummyInWorld.Translation;
        }

        public MyObjectBuilder_GunBase GetObjectBuilder()
        {
            MyObjectBuilder_GunBase base2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GunBase>();
            MyDefinitionId currentAmmoMagazineId = this.CurrentAmmoMagazineId;
            base2.CurrentAmmoMagazineName = currentAmmoMagazineId.SubtypeName;
            base2.RemainingAmmo = this.CurrentAmmo;
            base2.LastShootTime = this.LastShootTime.Ticks;
            base2.RemainingAmmosList = new List<MyObjectBuilder_GunBase.RemainingAmmoIns>();
            foreach (KeyValuePair<MyDefinitionId, int> pair in this.m_remainingAmmos)
            {
                MyObjectBuilder_GunBase.RemainingAmmoIns item = new MyObjectBuilder_GunBase.RemainingAmmoIns();
                currentAmmoMagazineId = pair.Key;
                item.SubtypeName = currentAmmoMagazineId.SubtypeName;
                item.Amount = pair.Value;
                base2.RemainingAmmosList.Add(item);
            }
            base2.InventoryItemId = base.InventoryItemId;
            return base2;
        }

        public int GetTotalAmmunitionAmount() => 
            ((int) this.m_cachedAmmunitionAmount);

        public bool HasEnoughAmmunition()
        {
            if (MySession.Static.InfiniteAmmo)
            {
                return true;
            }
            if (!Sync.IsServer)
            {
                return (this.m_cachedAmmunitionAmount > 0);
            }
            if (this.CurrentAmmo >= 1)
            {
                return true;
            }
            if ((this.m_user != null) && (this.m_user.AmmoInventory != null))
            {
                return (this.m_user.AmmoInventory.GetItemAmount(this.CurrentAmmoMagazineId, MyItemFlags.None, false) > 0);
            }
            string msg = $"Error: {(this.m_user == null) ? "User" : "AmmoInventory"} should not be null!";
            MyLog.Default.WriteLine(msg);
            return false;
        }

        public void Init(MyObjectBuilder_GunBase objectBuilder, MyCubeBlockDefinition cubeBlockDefinition, IMyGunBaseUser gunBaseUser)
        {
            if (cubeBlockDefinition is MyWeaponBlockDefinition)
            {
                MyWeaponBlockDefinition definition = cubeBlockDefinition as MyWeaponBlockDefinition;
                this.Init(objectBuilder, definition.WeaponDefinitionId, gunBaseUser);
            }
            else
            {
                MyDefinitionId backwardCompatibleDefinitionId = this.GetBackwardCompatibleDefinitionId(cubeBlockDefinition.Id.TypeId);
                this.Init(objectBuilder, backwardCompatibleDefinitionId, gunBaseUser);
            }
        }

        public void Init(MyObjectBuilder_GunBase objectBuilder, MyDefinitionId weaponDefinitionId, IMyGunBaseUser gunBaseUser)
        {
            if (objectBuilder != null)
            {
                base.Init(objectBuilder);
            }
            this.m_user = gunBaseUser;
            this.m_weaponProperties = new MyWeaponPropertiesWrapper(weaponDefinitionId);
            this.m_remainingAmmos = new Dictionary<MyDefinitionId, int>(this.WeaponProperties.AmmoMagazinesCount);
            if (objectBuilder == null)
            {
                if (this.WeaponProperties.WeaponDefinition.HasAmmoMagazines())
                {
                    this.m_weaponProperties.ChangeAmmoMagazine(this.m_weaponProperties.WeaponDefinition.AmmoMagazinesId[0]);
                }
                this.LastShootTime = new DateTime(0L);
            }
            else
            {
                MyDefinitionId newAmmoMagazineId = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), objectBuilder.CurrentAmmoMagazineName);
                if (this.m_weaponProperties.CanChangeAmmoMagazine(newAmmoMagazineId))
                {
                    this.CurrentAmmo = objectBuilder.RemainingAmmo;
                    this.m_weaponProperties.ChangeAmmoMagazine(newAmmoMagazineId);
                }
                else if (this.WeaponProperties.WeaponDefinition.HasAmmoMagazines())
                {
                    this.m_weaponProperties.ChangeAmmoMagazine(this.m_weaponProperties.WeaponDefinition.AmmoMagazinesId[0]);
                }
                foreach (MyObjectBuilder_GunBase.RemainingAmmoIns ins in objectBuilder.RemainingAmmosList)
                {
                    this.m_remainingAmmos.Add(new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), ins.SubtypeName), ins.Amount);
                }
                this.LastShootTime = new DateTime(objectBuilder.LastShootTime);
            }
            if (this.m_user.AmmoInventory != null)
            {
                if (this.m_user.PutConstraint())
                {
                    this.m_user.AmmoInventory.Constraint = this.CreateAmmoInventoryConstraints(this.m_user.ConstraintDisplayName);
                }
                this.RefreshAmmunitionAmount();
            }
            if (this.m_user.Weapon != null)
            {
                this.m_user.Weapon.OnClosing += new Action<MyEntity>(this.Weapon_OnClosing);
            }
        }

        public bool IsAmmoMagazineCompatible(MyDefinitionId ammoMagazineId) => 
            this.WeaponProperties.CanChangeAmmoMagazine(ammoMagazineId);

        public void LoadDummies(Dictionary<string, MyModelDummy> dummies)
        {
            this.m_dummies = dummies;
            this.m_dummiesByAmmoType.Clear();
            foreach (KeyValuePair<string, MyModelDummy> pair in dummies)
            {
                if (pair.Key.ToLower().Contains("muzzle_projectile"))
                {
                    this.AddMuzzleMatrix(MyAmmoType.HighSpeed, pair.Value.Matrix);
                    this.m_holdingDummyMatrix = pair.Value.Matrix;
                    this.m_holdingDummyMatrix = Matrix.CreateScale((Vector3) (1f / pair.Value.Matrix.Scale)) * this.m_holdingDummyMatrix;
                    this.m_holdingDummyMatrix = Matrix.Invert(this.m_holdingDummyMatrix);
                    continue;
                }
                if (pair.Key.ToLower().Contains("muzzle_missile"))
                {
                    this.AddMuzzleMatrix(MyAmmoType.Missile, pair.Value.Matrix);
                    continue;
                }
                if (pair.Key.ToLower().Contains("holding_dummy") || pair.Key.ToLower().Contains("holdingdummy"))
                {
                    this.m_holdingDummyMatrix = pair.Value.Matrix;
                    this.m_holdingDummyMatrix = Matrix.Normalize(this.m_holdingDummyMatrix);
                }
            }
        }

        private void MoveToNextMuzzle(MyAmmoType ammoType)
        {
            DummyContainer container;
            int key = (int) ammoType;
            if (this.m_dummiesByAmmoType.TryGetValue(key, out container) && (container.Dummies.Count > 1))
            {
                container.DummyIndex++;
                if (container.DummyIndex == container.Dummies.Count)
                {
                    container.DummyIndex = 0;
                }
                container.Dirty = true;
            }
        }

        private void RecalculateMuzzles()
        {
            using (Dictionary<int, DummyContainer>.ValueCollection.Enumerator enumerator = this.m_dummiesByAmmoType.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Dirty = true;
                }
            }
        }

        public void RefreshAmmunitionAmount()
        {
            if (Sync.IsServer)
            {
                if (MySession.Static.InfiniteAmmo)
                {
                    this.m_cachedAmmunitionAmount.Value = this.CurrentAmmo;
                }
                else if (((this.m_user == null) || (this.m_user.AmmoInventory == null)) || !this.m_weaponProperties.WeaponDefinition.HasAmmoMagazines())
                {
                    this.m_cachedAmmunitionAmount.Value = 0;
                }
                else
                {
                    if (!this.HasEnoughAmmunition())
                    {
                        this.SwitchAmmoMagazineToFirstAvailable();
                    }
                    this.m_cachedAmmunitionAmount.Value = this.CurrentAmmo + (((int) this.m_user.AmmoInventory.GetItemAmount(this.CurrentAmmoMagazineId, MyItemFlags.None, false)) * this.m_weaponProperties.AmmoMagazineDefinition.Capacity);
                }
            }
        }

        public void RemoveOldEffects(MyWeaponDefinition.WeaponEffectAction action = 1)
        {
            for (int i = 0; i < this.m_activeEffects.Count; i++)
            {
                if (this.m_activeEffects[i].Action == action)
                {
                    this.m_activeEffects[i].Effect.Stop(this.m_activeEffects[i].InstantStop);
                    this.m_activeEffects[i].Effect.Close(true, this.m_activeEffects[i].InstantStop);
                    this.m_activeEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Shoot(Vector3 initialVelocity, MyEntity owner = null)
        {
            MatrixD muzzleWorldMatrix = this.GetMuzzleWorldMatrix();
            this.Shoot(muzzleWorldMatrix.Translation, initialVelocity, (Vector3) muzzleWorldMatrix.Forward, owner);
        }

        public void Shoot(Vector3 initialVelocity, Vector3 direction, MyEntity owner = null)
        {
            MatrixD muzzleWorldMatrix = this.GetMuzzleWorldMatrix();
            this.Shoot(muzzleWorldMatrix.Translation, initialVelocity, direction, owner);
        }

        public void Shoot(Vector3D initialPosition, Vector3 initialVelocity, Vector3 direction, MyEntity owner = null)
        {
            MyAmmoDefinition ammoDefinition = this.m_weaponProperties.AmmoDefinition;
            MyAmmoType ammoType = ammoDefinition.AmmoType;
            if (ammoType != MyAmmoType.HighSpeed)
            {
                if ((ammoType == MyAmmoType.Missile) && Sync.IsServer)
                {
                    this.AddMissile(this.m_weaponProperties, initialPosition, initialVelocity, direction);
                }
            }
            else
            {
                int projectileCount = (ammoDefinition as MyProjectileAmmoDefinition).ProjectileCount;
                for (int i = 0; i < projectileCount; i++)
                {
                    this.AddProjectile(this.m_weaponProperties, initialPosition, initialVelocity, direction, owner);
                }
            }
            this.MoveToNextMuzzle(ammoDefinition.AmmoType);
            this.CreateEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
            this.LastShootTime = DateTime.UtcNow;
        }

        public void ShootWithOffset(Vector3 initialVelocity, Vector3 direction, float offset, MyEntity owner = null)
        {
            MatrixD muzzleWorldMatrix = this.GetMuzzleWorldMatrix();
            this.Shoot(muzzleWorldMatrix.Translation + (direction * offset), initialVelocity, direction, owner);
        }

        internal void StartNoAmmoSound(MyEntity3DSoundEmitter soundEmitter)
        {
            if ((this.NoAmmoSound != null) && (soundEmitter != null))
            {
                soundEmitter.StopSound(true, true);
                bool? nullable = null;
                soundEmitter.PlaySingleSound(this.NoAmmoSound, true, false, false, nullable);
            }
        }

        public void StartShootSound(MyEntity3DSoundEmitter soundEmitter, bool force2D = false)
        {
            if ((this.ShootSound != null) && (soundEmitter != null))
            {
                bool? nullable;
                if (!soundEmitter.IsPlaying)
                {
                    nullable = null;
                    soundEmitter.PlaySound(this.ShootSound, true, false, force2D, false, false, nullable);
                }
                else if (!soundEmitter.Loop)
                {
                    nullable = null;
                    soundEmitter.PlaySound(this.ShootSound, false, false, force2D, false, false, nullable);
                }
            }
        }

        public void StopShoot()
        {
            this.m_shotProjectiles = 0;
        }

        public bool SwitchAmmoMagazine(MyDefinitionId ammoMagazineId)
        {
            this.m_remainingAmmos[this.CurrentAmmoMagazineId] = this.CurrentAmmo;
            this.WeaponProperties.ChangeAmmoMagazine(ammoMagazineId);
            int num = 0;
            this.m_remainingAmmos.TryGetValue(ammoMagazineId, out num);
            this.CurrentAmmo = num;
            this.RefreshAmmunitionAmount();
            return (ammoMagazineId == this.WeaponProperties.AmmoMagazineId);
        }

        public bool SwitchAmmoMagazineToFirstAvailable()
        {
            MyWeaponDefinition weaponDefinition = this.WeaponProperties.WeaponDefinition;
            for (int i = 0; i < this.WeaponProperties.AmmoMagazinesCount; i++)
            {
                int num2 = 0;
                if (this.m_remainingAmmos.TryGetValue(weaponDefinition.AmmoMagazinesId[i], out num2) && (num2 > 0))
                {
                    return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[i]);
                }
                if (this.m_user.AmmoInventory.GetItemAmount(weaponDefinition.AmmoMagazinesId[i], MyItemFlags.None, false) > 0)
                {
                    return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[i]);
                }
            }
            return false;
        }

        public override bool SwitchAmmoMagazineToNextAvailable()
        {
            MyWeaponDefinition weaponDefinition = this.WeaponProperties.WeaponDefinition;
            if (weaponDefinition.HasAmmoMagazines())
            {
                int length = weaponDefinition.AmmoMagazinesId.Length;
                int index = weaponDefinition.GetAmmoMagazineIdArrayIndex(this.CurrentAmmoMagazineId) + 1;
                for (int i = 0; i != length; i++)
                {
                    if (index == length)
                    {
                        index = 0;
                    }
                    if (weaponDefinition.AmmoMagazinesId[index].SubtypeId != this.CurrentAmmoMagazineId.SubtypeId)
                    {
                        if (MySession.Static.InfiniteAmmo)
                        {
                            return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[index]);
                        }
                        int num4 = 0;
                        if (this.m_remainingAmmos.TryGetValue(weaponDefinition.AmmoMagazinesId[index], out num4) && (num4 > 0))
                        {
                            return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[index]);
                        }
                        if (this.m_user.AmmoInventory.GetItemAmount(weaponDefinition.AmmoMagazinesId[index], MyItemFlags.None, false) > 0)
                        {
                            return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[index]);
                        }
                    }
                    index++;
                }
            }
            return false;
        }

        public override bool SwitchToNextAmmoMagazine()
        {
            MyWeaponDefinition weaponDefinition = this.WeaponProperties.WeaponDefinition;
            int index = weaponDefinition.GetAmmoMagazineIdArrayIndex(this.CurrentAmmoMagazineId) + 1;
            if (index == weaponDefinition.AmmoMagazinesId.Length)
            {
                index = 0;
            }
            return this.SwitchAmmoMagazine(weaponDefinition.AmmoMagazinesId[index]);
        }

        public void UpdateEffects()
        {
            for (int i = 0; i < this.m_activeEffects.Count; i++)
            {
                if (!this.m_activeEffects[i].Effect.IsStopped || (this.m_activeEffects[i].Effect.GetParticlesCount() > 0))
                {
                    this.m_activeEffects[i].Effect.WorldMatrix = MatrixD.Multiply(this.m_activeEffects[i].LocalMatrix, this.WorldMatrix);
                }
                else
                {
                    this.m_activeEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        private void Weapon_OnClosing(MyEntity obj)
        {
            if (this.m_user.Weapon != null)
            {
                this.m_user.Weapon.OnClosing -= new Action<MyEntity>(this.Weapon_OnClosing);
            }
        }

        public int CurrentAmmo { get; set; }

        public MyWeaponPropertiesWrapper WeaponProperties =>
            this.m_weaponProperties;

        public MyAmmoMagazineDefinition CurrentAmmoMagazineDefinition =>
            this.WeaponProperties.AmmoMagazineDefinition;

        public MyDefinitionId CurrentAmmoMagazineId =>
            this.WeaponProperties.AmmoMagazineId;

        public MyAmmoDefinition CurrentAmmoDefinition =>
            this.WeaponProperties.AmmoDefinition;

        public float BackkickForcePerSecond
        {
            get
            {
                if ((this.WeaponProperties == null) || (this.WeaponProperties.AmmoDefinition == null))
                {
                    return 0f;
                }
                return this.WeaponProperties.AmmoDefinition.BackkickForce;
            }
        }

        public bool HasMissileAmmoDefined =>
            this.m_weaponProperties.WeaponDefinition.HasMissileAmmoDefined;

        public bool HasProjectileAmmoDefined =>
            this.m_weaponProperties.WeaponDefinition.HasProjectileAmmoDefined;

        public int MuzzleFlashLifeSpan =>
            this.m_weaponProperties.WeaponDefinition.MuzzleFlashLifeSpan;

        public int ShootIntervalInMiliseconds =>
            ((this.ShootIntervalModifier == 1f) ? this.m_weaponProperties.CurrentWeaponShootIntervalInMiliseconds : ((int) (this.ShootIntervalModifier * this.m_weaponProperties.CurrentWeaponShootIntervalInMiliseconds)));

        public float ShootIntervalModifier { get; set; }

        public float ReleaseTimeAfterFire =>
            this.m_weaponProperties.WeaponDefinition.ReleaseTimeAfterFire;

        public MySoundPair ShootSound =>
            this.m_weaponProperties.CurrentWeaponShootSound;

        public MySoundPair NoAmmoSound =>
            this.m_weaponProperties.WeaponDefinition.NoAmmoSound;

        public MySoundPair ReloadSound =>
            this.m_weaponProperties.WeaponDefinition.ReloadSound;

        public MySoundPair SecondarySound =>
            this.m_weaponProperties.WeaponDefinition.SecondarySound;

        public bool UseDefaultMuzzleFlash =>
            this.m_weaponProperties.WeaponDefinition.UseDefaultMuzzleFlash;

        public float MechanicalDamage =>
            ((this.WeaponProperties.AmmoDefinition == null) ? 0f : this.m_weaponProperties.AmmoDefinition.GetDamageForMechanicalObjects());

        public float DeviateAngle =>
            this.m_weaponProperties.WeaponDefinition.DeviateShotAngle;

        public bool HasAmmoMagazines =>
            this.m_weaponProperties.WeaponDefinition.HasAmmoMagazines();

        public bool IsAmmoProjectile =>
            this.m_weaponProperties.IsAmmoProjectile;

        public bool IsAmmoMissile =>
            this.m_weaponProperties.IsAmmoMissile;

        public int ShotsInBurst =>
            this.WeaponProperties.ShotsInBurst;

        public int ReloadTime =>
            this.WeaponProperties.ReloadTime;

        public bool HasDummies =>
            (this.m_dummiesByAmmoType.Count > 0);

        public MatrixD WorldMatrix
        {
            get => 
                this.m_worldMatrix;
            set
            {
                this.m_worldMatrix = value;
                this.RecalculateMuzzles();
            }
        }

        public DateTime LastShootTime { get; private set; }

        public int RemainingAmmo
        {
            set => 
                this.m_cachedAmmunitionAmount.SetLocalValue(value);
        }

        public class DummyContainer
        {
            public List<MatrixD> Dummies = new List<MatrixD>();
            public int DummyIndex;
            public MatrixD DummyInWorld = Matrix.Identity;
            public bool Dirty = true;

            public MatrixD DummyToUse =>
                this.Dummies[this.DummyIndex];
        }

        private class WeaponEffect
        {
            public string EffectName;
            public string DummyName;
            public Matrix LocalMatrix;
            public MyWeaponDefinition.WeaponEffectAction Action;
            public MyParticleEffect Effect;
            public bool InstantStop;

            public WeaponEffect(string effectName, string dummyName, Matrix localMatrix, MyWeaponDefinition.WeaponEffectAction action, MyParticleEffect effect, bool instantStop)
            {
                this.EffectName = effectName;
                this.DummyName = dummyName;
                this.Effect = effect;
                this.Action = action;
                this.LocalMatrix = localMatrix;
                this.InstantStop = instantStop;
            }
        }
    }
}

