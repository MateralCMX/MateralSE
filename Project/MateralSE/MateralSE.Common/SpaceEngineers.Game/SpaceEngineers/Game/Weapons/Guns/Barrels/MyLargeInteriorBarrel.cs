namespace SpaceEngineers.Game.Weapons.Guns.Barrels
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.Weapons.Guns.Barrels;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyLargeInteriorBarrel : MyLargeBarrelBase
    {
        public override void Close()
        {
            if (base.m_shotSmoke != null)
            {
                base.m_shotSmoke.Stop(true);
                base.m_shotSmoke = null;
            }
            if (base.m_muzzleFlash != null)
            {
                base.m_muzzleFlash.Stop(true);
                base.m_muzzleFlash = null;
            }
        }

        public override void Draw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                MyRenderProxy.DebugDrawLine3D(base.m_entity.PositionComp.GetPosition(), base.m_entity.PositionComp.GetPosition() + base.m_entity.WorldMatrix.Forward, Color.Green, Color.GreenYellow, false, false);
                if (base.GetWeaponBase().Target != null)
                {
                    MyRenderProxy.DebugDrawSphere(base.GetWeaponBase().Target.PositionComp.GetPosition(), 0.4f, Color.Green, 1f, false, false, true, false);
                }
            }
        }

        public override void Init(MyEntity entity, MyLargeTurretBase turretBase)
        {
            base.Init(entity, turretBase);
            if (!base.m_gunBase.HasDummies)
            {
                Vector3 position = (Vector3) (-base.Entity.PositionComp.WorldMatrix.Forward * 0.800000011920929);
                base.m_gunBase.AddMuzzleMatrix(MyAmmoType.HighSpeed, Matrix.CreateTranslation(position));
            }
        }

        public override bool StartShooting()
        {
            if ((base.m_lateStartRandom > base.m_currentLateStart) && !base.m_dontTimeOffsetNextShot)
            {
                base.m_currentLateStart++;
                return false;
            }
            base.m_dontTimeOffsetNextShot = false;
            if (!base.StartShooting())
            {
                return false;
            }
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - base.m_lastTimeShoot) < base.m_gunBase.ShootIntervalInMiliseconds)
            {
                return false;
            }
            if ((base.m_turretBase != null) && !base.m_turretBase.IsTargetVisible())
            {
                return false;
            }
            base.m_muzzleFlashLength = MyUtils.GetRandomFloat(1f, 2f);
            base.m_muzzleFlashRadius = MyUtils.GetRandomFloat(0.3f, 0.5f);
            if (base.m_turretBase.IsControlledByLocalPlayer)
            {
                base.m_muzzleFlashLength *= 0.33f;
                base.m_muzzleFlashRadius *= 0.33f;
            }
            base.IncreaseSmoke();
            if (base.m_shotSmoke == null)
            {
                if (base.m_smokeToGenerate > 0)
                {
                    MyParticlesManager.TryCreateParticleEffect("Smoke_LargeGunShot", base.m_gunBase.GetMuzzleWorldMatrix(), out base.m_shotSmoke);
                }
            }
            else if (base.m_shotSmoke.IsEmittingStopped)
            {
                base.m_shotSmoke.Play();
            }
            if (base.m_muzzleFlash == null)
            {
                MyParticlesManager.TryCreateParticleEffect("Muzzle_Flash_Large", base.m_gunBase.GetMuzzleWorldMatrix(), out base.m_muzzleFlash);
            }
            if (base.m_shotSmoke != null)
            {
                base.m_shotSmoke.WorldMatrix = base.m_gunBase.GetMuzzleWorldMatrix();
            }
            if (base.m_muzzleFlash != null)
            {
                base.m_muzzleFlash.WorldMatrix = base.m_gunBase.GetMuzzleWorldMatrix();
            }
            base.GetWeaponBase().PlayShootingSound();
            base.Shoot((Vector3) base.Entity.PositionComp.GetPosition());
            base.m_lastTimeShoot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            return true;
        }

        public override void StopShooting()
        {
            base.StopShooting();
            base.m_currentLateStart = 0;
            if (base.m_muzzleFlash != null)
            {
                base.m_muzzleFlash.Stop(true);
                base.m_muzzleFlash = null;
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (base.m_shotSmoke != null)
            {
                base.m_shotSmoke.WorldMatrix = base.m_gunBase.GetMuzzleWorldMatrix();
                if (base.m_smokeToGenerate > 0)
                {
                    base.m_shotSmoke.UserBirthMultiplier = base.m_smokeToGenerate;
                }
                else
                {
                    base.m_shotSmoke.Stop(false);
                    base.m_shotSmoke = null;
                }
            }
            if (base.m_muzzleFlash != null)
            {
                if (base.m_smokeToGenerate == 0)
                {
                    base.m_muzzleFlash.Stop(true);
                    base.m_muzzleFlash = null;
                }
                else
                {
                    base.m_muzzleFlash.WorldMatrix = base.m_gunBase.GetMuzzleWorldMatrix();
                }
            }
        }
    }
}

