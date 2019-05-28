namespace SpaceEngineers.Game.Weapons.Guns.Barrels
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.Weapons.Guns.Barrels;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    internal class MyLargeMissileBarrel : MyLargeBarrelBase
    {
        private int m_reloadCompletionTime;
        private int m_nextShootTime;
        private int m_shotsLeftInBurst;
        private int m_nextNotificationTime;
        private MyHudNotification m_reloadNotification;
        private MyEntity3DSoundEmitter m_soundEmitter;

        public MyLargeMissileBarrel()
        {
            this.m_soundEmitter = new MyEntity3DSoundEmitter(base.m_entity, false, 1f);
        }

        public override void Close()
        {
            base.Close();
            this.m_soundEmitter.StopSound(true, true);
        }

        public override unsafe void Init(MyEntity entity, MyLargeTurretBase turretBase)
        {
            base.Init(entity, turretBase);
            if (!base.m_gunBase.HasDummies)
            {
                Matrix identity = Matrix.Identity;
                Matrix* matrixPtr1 = (Matrix*) ref identity;
                matrixPtr1.Translation += entity.PositionComp.WorldMatrix.Forward * 3.0;
                base.m_gunBase.AddMuzzleMatrix(MyAmmoType.Missile, identity);
            }
            this.m_shotsLeftInBurst = this.ShotsInBurst;
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Entity = turretBase;
            }
        }

        public void Init(Matrix localMatrix, MyLargeTurretBase parentObject)
        {
            this.m_shotsLeftInBurst = this.ShotsInBurst;
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
            if (((base.m_turretBase == null) || (base.m_turretBase.Parent == null)) || (base.m_turretBase.Parent.Physics == null))
            {
                return false;
            }
            bool dontTimeOffsetNextShot = base.m_dontTimeOffsetNextShot;
            if (Sync.IsServer)
            {
                if (((base.m_lateStartRandom > base.m_currentLateStart) && !base.m_dontTimeOffsetNextShot) && !base.m_turretBase.IsControlled)
                {
                    base.m_currentLateStart++;
                    return false;
                }
                base.m_dontTimeOffsetNextShot = false;
            }
            if (this.m_reloadCompletionTime > MySandboxGame.TotalGamePlayTimeInMilliseconds)
            {
                return false;
            }
            if (this.m_nextShootTime > MySandboxGame.TotalGamePlayTimeInMilliseconds)
            {
                return false;
            }
            if (((this.m_shotsLeftInBurst > 0) || (this.ShotsInBurst == 0)) && (((base.m_turretBase.Target != null) || base.m_turretBase.IsControlled) | dontTimeOffsetNextShot))
            {
                if (Sync.IsServer)
                {
                    base.GetWeaponBase().RemoveAmmoPerShot();
                }
                base.m_gunBase.Shoot(base.m_turretBase.Parent.Physics.LinearVelocity, null);
                base.m_lastTimeShoot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_nextShootTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + base.m_gunBase.ShootIntervalInMiliseconds;
                if (this.ShotsInBurst > 0)
                {
                    this.m_shotsLeftInBurst--;
                    if (this.m_shotsLeftInBurst <= 0)
                    {
                        this.m_reloadCompletionTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + base.m_gunBase.ReloadTime;
                        base.m_turretBase.OnReloadStarted(base.m_gunBase.ReloadTime);
                        this.m_shotsLeftInBurst = this.ShotsInBurst;
                    }
                }
            }
            return true;
        }

        private void StartSound()
        {
            base.m_gunBase.StartShootSound(this.m_soundEmitter, false);
        }

        public override void StopShooting()
        {
            base.StopShooting();
            base.m_currentLateStart = 0;
            this.m_soundEmitter.StopSound(true, true);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
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

