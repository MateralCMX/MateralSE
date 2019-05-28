namespace Sandbox.Game.Weapons.Guns.Barrels
{
    using Sandbox.Definitions;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    public abstract class MyLargeBarrelBase
    {
        protected MyGunBase m_gunBase;
        protected int m_lastTimeShoot = 0;
        private int m_lastTimeSmooke = 0;
        protected int m_lateStartRandom;
        protected int m_currentLateStart;
        private float m_barrelElevationMin;
        private float m_barrelSinElevationMin;
        protected MyParticleEffect m_shotSmoke;
        protected MyParticleEffect m_muzzleFlash;
        protected bool m_dontTimeOffsetNextShot;
        protected int m_smokeLastTime;
        protected int m_smokeToGenerate;
        protected float m_muzzleFlashLength;
        protected float m_muzzleFlashRadius;
        protected MyEntity m_entity;
        protected MyLargeTurretBase m_turretBase;

        public MyLargeBarrelBase()
        {
            this.BarrelElevationMin = -0.6f;
        }

        public virtual void Close()
        {
        }

        protected void DecreaseSmoke()
        {
            this.m_smokeToGenerate--;
            this.m_smokeToGenerate = MyUtils.GetClampInt(this.m_smokeToGenerate, 0, 50);
        }

        public void DontTimeOffsetNextShot()
        {
            this.m_dontTimeOffsetNextShot = true;
        }

        public virtual void Draw()
        {
        }

        private void DrawCrossHair()
        {
        }

        protected MyLargeTurretBase GetWeaponBase() => 
            this.m_turretBase;

        protected void IncreaseSmoke()
        {
            this.m_smokeToGenerate += 0x13;
            this.m_smokeToGenerate = MyUtils.GetClampInt(this.m_smokeToGenerate, 0, 50);
        }

        public virtual void Init(MyEntity entity, MyLargeTurretBase turretBase)
        {
            this.m_entity = entity;
            this.m_turretBase = turretBase;
            this.m_gunBase = turretBase.GunBase;
            this.m_lateStartRandom = turretBase.LateStartRandom;
            if (this.m_entity.Model != null)
            {
                if (this.m_entity.Model.Dummies.ContainsKey("camera"))
                {
                    this.CameraDummy = this.m_entity.Model.Dummies["camera"];
                }
                this.m_gunBase.LoadDummies(this.m_entity.Model.Dummies);
            }
            this.m_entity.OnClose += new Action<MyEntity>(this.m_entity_OnClose);
        }

        public bool IsControlledByPlayer() => 
            ReferenceEquals(MySession.Static.ControlledEntity, this);

        private void m_entity_OnClose(MyEntity obj)
        {
            if (this.m_shotSmoke != null)
            {
                MyParticlesManager.RemoveParticleEffect(this.m_shotSmoke, false);
                this.m_shotSmoke = null;
            }
            if (this.m_muzzleFlash != null)
            {
                MyParticlesManager.RemoveParticleEffect(this.m_muzzleFlash, false);
                this.m_muzzleFlash = null;
            }
        }

        public void RemoveSmoke()
        {
            this.m_smokeToGenerate = 0;
        }

        public void ResetCurrentLateStart()
        {
            this.m_currentLateStart = 0;
        }

        protected void Shoot(Vector3 muzzlePosition)
        {
            if (this.m_turretBase.Parent.Physics != null)
            {
                this.m_entity.WorldMatrix.Forward;
                Vector3 linearVelocity = this.m_turretBase.Parent.Physics.LinearVelocity;
                this.GetWeaponBase().RemoveAmmoPerShot();
                this.m_gunBase.Shoot(linearVelocity, null);
            }
        }

        public void ShootEffect()
        {
            this.m_gunBase.CreateEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
        }

        public virtual bool StartShooting()
        {
            this.m_turretBase.Render.NeedsDrawFromParent = true;
            return true;
        }

        public virtual void StopShooting()
        {
            this.m_turretBase.Render.NeedsDrawFromParent = false;
            this.GetWeaponBase().StopShootingSound();
        }

        public virtual void UpdateAfterSimulation()
        {
            this.DecreaseSmoke();
        }

        public void WorldPositionChanged()
        {
            this.m_gunBase.WorldMatrix = this.Entity.PositionComp.WorldMatrix;
        }

        public MyGunBase GunBase =>
            this.m_gunBase;

        public MyModelDummy CameraDummy { get; private set; }

        public int LateTimeRandom
        {
            get => 
                this.m_lateStartRandom;
            set => 
                (this.m_lateStartRandom = value);
        }

        public float BarrelElevationMin
        {
            get => 
                this.m_barrelElevationMin;
            protected set
            {
                this.m_barrelElevationMin = value;
                this.m_barrelSinElevationMin = (float) Math.Sin((double) this.m_barrelSinElevationMin);
            }
        }

        public float BarrelSinElevationMin =>
            this.m_barrelSinElevationMin;

        public MyEntity Entity =>
            this.m_entity;
    }
}

