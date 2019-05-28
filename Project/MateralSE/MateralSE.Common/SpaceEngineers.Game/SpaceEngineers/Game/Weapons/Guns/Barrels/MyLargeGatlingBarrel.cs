namespace SpaceEngineers.Game.Weapons.Guns.Barrels
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.Weapons.Guns.Barrels;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyLargeGatlingBarrel : MyLargeBarrelBase
    {
        private Vector3D m_muzzleFlashPosition;
        private int m_nextNotificationTime;
        private MyHudNotification m_reloadNotification;
        private float m_rotationTimeout = (2000f + MyUtils.GetRandomFloat(-500f, 500f));
        private int m_shotsLeftInBurst;
        private int m_reloadCompletionTime;

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
            this.m_shotsLeftInBurst = this.ShotsInBurst;
            if (!base.m_gunBase.HasDummies)
            {
                Vector3 position = (Vector3) (2.0 * entity.PositionComp.WorldMatrix.Forward);
                base.m_gunBase.AddMuzzleMatrix(MyAmmoType.HighSpeed, Matrix.CreateTranslation(position));
            }
        }

        private void ShowReloadNotification(int duration)
        {
            int num = MySandboxGame.TotalGamePlayTimeInMilliseconds + duration;
            if (this.m_reloadNotification != null)
            {
                int timeStep = num - this.m_nextNotificationTime;
                this.m_reloadNotification.AddAliveTime(timeStep);
                this.m_nextNotificationTime = num;
            }
            else
            {
                duration = Math.Max(0, duration - 250);
                if (duration != 0)
                {
                    this.m_reloadNotification = new MyHudNotification(MySpaceTexts.LargeMissileTurretReloadingNotification, duration, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                    MyHud.Notifications.Add(this.m_reloadNotification);
                    this.m_nextNotificationTime = num;
                }
            }
        }

        public override bool StartShooting()
        {
            if (this.m_reloadCompletionTime > MySandboxGame.TotalGamePlayTimeInMilliseconds)
            {
                return false;
            }
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
            if ((base.m_lateStartRandom > base.m_currentLateStart) && !base.m_dontTimeOffsetNextShot)
            {
                base.m_currentLateStart++;
                return false;
            }
            base.m_dontTimeOffsetNextShot = false;
            if ((this.m_shotsLeftInBurst <= 0) && (this.ShotsInBurst != 0))
            {
                return false;
            }
            base.m_muzzleFlashLength = MyUtils.GetRandomFloat(4f, 6f);
            base.m_muzzleFlashRadius = MyUtils.GetRandomFloat(1.2f, 2f);
            if (base.m_turretBase.IsControlledByLocalPlayer)
            {
                base.m_muzzleFlashRadius *= 0.33f;
            }
            base.IncreaseSmoke();
            this.m_muzzleFlashPosition = base.m_gunBase.GetMuzzleWorldPosition();
            if (base.m_shotSmoke == null)
            {
                if (base.m_smokeToGenerate > 0)
                {
                    MyParticlesManager.TryCreateParticleEffect("Smoke_LargeGunShot", MatrixD.CreateTranslation(this.m_muzzleFlashPosition), out base.m_shotSmoke);
                }
            }
            else if (base.m_shotSmoke.IsEmittingStopped)
            {
                base.m_shotSmoke.Play();
            }
            if (base.m_muzzleFlash == null)
            {
                MyParticlesManager.TryCreateParticleEffect("Muzzle_Flash_Large", MatrixD.CreateTranslation(this.m_muzzleFlashPosition), out base.m_muzzleFlash);
            }
            if (base.m_shotSmoke != null)
            {
                base.m_shotSmoke.WorldMatrix = MatrixD.CreateTranslation(this.m_muzzleFlashPosition);
            }
            if (base.m_muzzleFlash != null)
            {
                base.m_muzzleFlash.WorldMatrix = MatrixD.CreateTranslation(this.m_muzzleFlashPosition);
            }
            base.GetWeaponBase().PlayShootingSound();
            base.Shoot((Vector3) base.Entity.PositionComp.GetPosition());
            if (this.ShotsInBurst > 0)
            {
                this.m_shotsLeftInBurst--;
                if (this.m_shotsLeftInBurst <= 0)
                {
                    this.m_reloadCompletionTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + base.m_gunBase.ReloadTime;
                    base.m_turretBase.OnReloadStarted(base.m_gunBase.ReloadTime);
                    this.m_shotsLeftInBurst = this.ShotsInBurst;
                    if (base.m_muzzleFlash != null)
                    {
                        base.m_muzzleFlash.Stop(true);
                        base.m_muzzleFlash = null;
                    }
                    base.m_currentLateStart = 0;
                }
            }
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
            float radians = (MathHelper.SmoothStep((float) 0f, (float) 1f, (float) (1f - MathHelper.Clamp((float) (((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - base.m_lastTimeShoot)) / this.m_rotationTimeout), (float) 0f, (float) 1f))) * 12.56637f) * 0.01666667f;
            if (radians != 0f)
            {
                base.Entity.PositionComp.LocalMatrix = Matrix.CreateRotationZ(radians) * base.Entity.PositionComp.LocalMatrix;
            }
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
                if (base.m_smokeToGenerate != 0)
                {
                    base.m_muzzleFlash.WorldMatrix = base.m_gunBase.GetMuzzleWorldMatrix();
                }
                else
                {
                    base.m_muzzleFlash.Stop(true);
                    base.m_muzzleFlash = null;
                }
            }
            this.UpdateReloadNotification();
        }

        private void UpdateReloadNotification()
        {
            if (MySandboxGame.TotalGamePlayTimeInMilliseconds > this.m_nextNotificationTime)
            {
                this.m_reloadNotification = null;
            }
            if (!base.m_gunBase.HasEnoughAmmunition() && MySession.Static.SurvivalMode)
            {
                MyHud.Notifications.Remove(this.m_reloadNotification);
                this.m_reloadNotification = null;
            }
            else if (!base.m_turretBase.IsControlledByLocalPlayer)
            {
                if (this.m_reloadNotification != null)
                {
                    MyHud.Notifications.Remove(this.m_reloadNotification);
                    this.m_reloadNotification = null;
                }
            }
            else if (this.m_reloadCompletionTime > MySandboxGame.TotalGamePlayTimeInMilliseconds)
            {
                this.ShowReloadNotification(this.m_reloadCompletionTime - MySandboxGame.TotalGamePlayTimeInMilliseconds);
            }
        }

        public int ShotsInBurst =>
            base.m_gunBase.ShotsInBurst;
    }
}

