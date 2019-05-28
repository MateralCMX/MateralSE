namespace SpaceEngineers.Game.EntityComponents.GameLogic
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;

    public class MyLifeSupportingComponent : MyEntityComponentBase
    {
        private int m_lastTimeUsed;
        private readonly MySoundPair m_progressSound;
        private readonly MyEntity3DSoundEmitter m_progressSoundEmitter;
        private string m_actionName;
        private float m_rechargeMultiplier = 1f;

        public MyLifeSupportingComponent(MyEntity owner, MySoundPair progressSound, string actionName = "GenericHeal", float rechargeMultiplier = 1f)
        {
            this.RechargeSocket = new MyRechargeSocket();
            this.m_actionName = actionName;
            this.m_rechargeMultiplier = rechargeMultiplier;
            this.m_progressSound = progressSound;
            this.m_progressSoundEmitter = new MyEntity3DSoundEmitter(owner, true, 1f);
            this.m_progressSoundEmitter.EmitterMethods[1].Add(() => (MySession.Static.ControlledEntity != null) && ReferenceEquals(this.User, MySession.Static.ControlledEntity.Entity));
            if (((MySession.Static != null) && MyFakes.ENABLE_NEW_SOUNDS) && MySession.Static.Settings.RealisticSound)
            {
                this.m_progressSoundEmitter.EmitterMethods[0].Add(() => (MySession.Static.ControlledEntity != null) && ReferenceEquals(this.User, MySession.Static.ControlledEntity.Entity));
            }
        }

        public override void OnRemovedFromScene()
        {
            this.Unplug();
            base.OnRemovedFromScene();
        }

        public void OnSupportRequested(MyCharacter user)
        {
            if ((this.User == null) || ReferenceEquals(this.User, user))
            {
                this.Entity.BroadcastSupportRequest(user);
            }
        }

        private void PlayProgressLoopSound()
        {
            if (!this.m_progressSoundEmitter.IsPlaying)
            {
                bool? nullable = null;
                this.m_progressSoundEmitter.PlaySound(this.m_progressSound, true, false, false, false, false, nullable);
            }
        }

        public void ProvideSupport(MyCharacter user)
        {
            if (this.Entity.IsWorking)
            {
                bool flag = false;
                if (this.User == null)
                {
                    this.User = user;
                    if (this.Entity.RefuelAllowed)
                    {
                        user.SuitBattery.ResourceSink.TemporaryConnectedEntity = this.Entity;
                        user.SuitBattery.RechargeMultiplier = this.m_rechargeMultiplier;
                        this.RechargeSocket.PlugIn(user.SuitBattery.ResourceSink);
                        flag = true;
                        PlayerSuitRechargeEvent playerSuitRecharging = MyVisualScriptLogicProvider.PlayerSuitRecharging;
                        if (playerSuitRecharging != null)
                        {
                            playerSuitRecharging(this.User.GetPlayerIdentityId(), this.Entity.BlockType);
                        }
                    }
                }
                this.m_lastTimeUsed = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                if ((this.User.StatComp != null) && this.Entity.HealingAllowed)
                {
                    this.User.StatComp.DoAction(this.m_actionName);
                    flag = true;
                    PlayerHealthRechargeEvent playerHealthRecharging = MyVisualScriptLogicProvider.PlayerHealthRecharging;
                    if (playerHealthRecharging != null)
                    {
                        float num = (this.User.StatComp.Health != null) ? this.User.StatComp.Health.Value : 0f;
                        playerHealthRecharging(this.User.GetPlayerIdentityId(), this.Entity.BlockType, num);
                    }
                }
                if (flag)
                {
                    this.PlayProgressLoopSound();
                }
            }
        }

        private void StopProgressLoopSound()
        {
            this.m_progressSoundEmitter.StopSound(false, true);
        }

        private void Unplug()
        {
            if (this.User != null)
            {
                this.RechargeSocket.Unplug();
                this.User.SuitBattery.ResourceSink.TemporaryConnectedEntity = null;
                this.User.SuitBattery.RechargeMultiplier = 1f;
                this.User = null;
                this.StopProgressLoopSound();
            }
        }

        public void Update10()
        {
            if ((this.User != null) && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeUsed) >= 100))
            {
                this.Unplug();
            }
        }

        public void UpdateSoundEmitters()
        {
            this.m_progressSoundEmitter.Update();
        }

        public IMyLifeSupportingBlock Entity =>
            ((IMyLifeSupportingBlock) base.Entity);

        public MyCharacter User { get; private set; }

        public MyRechargeSocket RechargeSocket { get; private set; }

        public override string ComponentTypeDebugString =>
            base.GetType().Name;
    }
}

