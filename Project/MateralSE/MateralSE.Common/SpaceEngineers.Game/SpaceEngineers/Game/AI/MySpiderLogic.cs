namespace SpaceEngineers.Game.AI
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner]
    public class MySpiderLogic : MyAgentLogic
    {
        private bool m_burrowing;
        private bool m_deburrowing;
        private bool m_deburrowAnimationStarted;
        private bool m_deburrowSoundStarted;
        private int m_burrowStart;
        private int m_deburrowStart;
        private Vector3D? m_effectOnPosition;
        private static readonly int BURROWING_TIME = 750;
        private static readonly int BURROWING_FX_START = 300;
        private static readonly int DEBURROWING_TIME = 0x708;
        private static readonly int DEBURROWING_ANIMATION_START = 0;
        private static readonly int DEBURROWING_SOUND_START = 0;

        public MySpiderLogic(MyAnimalBot bot) : base(bot)
        {
        }

        public override void Cleanup()
        {
            base.Cleanup();
            this.DeleteBurrowingParticleFX();
        }

        private void CreateBurrowingParticleFX()
        {
            Vector3D vectord = base.AgentBot.BotEntity.PositionComp.WorldMatrix.Translation + (base.AgentBot.BotEntity.PositionComp.WorldMatrix.Forward * 0.2);
            this.m_effectOnPosition = new Vector3D?(vectord);
            if (!Game.IsDedicated)
            {
                base.AgentBot.AgentEntity.CreateBurrowingParticleFX_Client(vectord);
            }
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, Vector3D>(base.AgentBot.AgentEntity, x => new Action<Vector3D>(x.CreateBurrowingParticleFX_Client), vectord, targetEndpoint);
        }

        private void DeleteBurrowingParticleFX()
        {
            if ((this.m_effectOnPosition != null) && !Game.IsDedicated)
            {
                MyCharacter agentEntity = base.AgentBot.AgentEntity;
                if (agentEntity != null)
                {
                    agentEntity.DeleteBurrowingParticleFX_Client(this.m_effectOnPosition.Value);
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCharacter, Vector3D>(base.AgentBot.AgentEntity, x => new Action<Vector3D>(x.DeleteBurrowingParticleFX_Client), this.m_effectOnPosition.Value, targetEndpoint);
                }
            }
            this.m_effectOnPosition = null;
        }

        public void StartBurrowing()
        {
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(base.AgentBot.BotEntity.WorldMatrix.Translation - (3.0 * base.AgentBot.BotEntity.WorldMatrix.Up), base.AgentBot.BotEntity.WorldMatrix.Translation + (3.0 * base.AgentBot.BotEntity.WorldMatrix.Up), 9);
            if ((nullable == null) || ReferenceEquals(nullable.HitEntity, base.AgentBot.BotEntity))
            {
                if (!base.AgentBot.AgentEntity.UseNewAnimationSystem)
                {
                    if (base.AgentBot.AgentEntity.HasAnimation("Burrow"))
                    {
                        base.AgentBot.AgentEntity.PlayCharacterAnimation("Burrow", MyBlendOption.Immediate, MyFrameOption.Default, 0f, 1f, true, null, false);
                        base.AgentBot.AgentEntity.DisableAnimationCommands();
                    }
                }
                else
                {
                    base.AgentBot.AgentEntity.TriggerAnimationEvent("burrow");
                    if (Sync.IsServer)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, string>(base.AgentBot.AgentEntity, x => new Action<string>(x.TriggerAnimationEvent), "burrow", targetEndpoint);
                    }
                }
                base.AgentBot.AgentEntity.SoundComp.StartSecondarySound("ArcBotSpiderBurrowIn", true);
                this.m_burrowing = true;
                this.m_burrowStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
        }

        public void StartDeburrowing()
        {
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(base.AgentBot.BotEntity.WorldMatrix.Translation - (3.0 * base.AgentBot.BotEntity.WorldMatrix.Up), base.AgentBot.BotEntity.WorldMatrix.Translation + (3.0 * base.AgentBot.BotEntity.WorldMatrix.Up), 9);
            if ((nullable == null) || ReferenceEquals(nullable.HitEntity, base.AgentBot.BotEntity))
            {
                if (base.AgentBot.AgentEntity.UseNewAnimationSystem)
                {
                    base.AgentBot.AgentEntity.TriggerAnimationEvent("deburrow");
                    if (Sync.IsServer)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, string>(base.AgentBot.AgentEntity, x => new Action<string>(x.TriggerAnimationEvent), "deburrow", targetEndpoint);
                    }
                }
                this.m_deburrowing = true;
                this.m_deburrowStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.CreateBurrowingParticleFX();
                this.m_deburrowAnimationStarted = false;
                this.m_deburrowSoundStarted = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.m_burrowing || this.m_deburrowing)
            {
                this.UpdateBurrowing();
            }
        }

        private void UpdateBurrowing()
        {
            if (this.m_burrowing)
            {
                int num1 = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_burrowStart;
                if ((num1 > BURROWING_FX_START) && (this.m_effectOnPosition == null))
                {
                    this.CreateBurrowingParticleFX();
                }
                if (num1 >= BURROWING_TIME)
                {
                    this.m_burrowing = false;
                    this.DeleteBurrowingParticleFX();
                    base.AgentBot.AgentEntity.EnableAnimationCommands();
                }
            }
            if (this.m_deburrowing)
            {
                int num = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_deburrowStart;
                if (!this.m_deburrowSoundStarted && (num >= DEBURROWING_SOUND_START))
                {
                    base.AgentBot.AgentEntity.SoundComp.StartSecondarySound("ArcBotSpiderBurrowOut", true);
                    this.m_deburrowSoundStarted = true;
                }
                if (!this.m_deburrowAnimationStarted && (num >= DEBURROWING_ANIMATION_START))
                {
                    if (base.AgentBot.AgentEntity.HasAnimation("Deburrow"))
                    {
                        base.AgentBot.AgentEntity.EnableAnimationCommands();
                        base.AgentBot.AgentEntity.PlayCharacterAnimation("Deburrow", MyBlendOption.Immediate, MyFrameOption.Default, 0f, 1f, true, null, false);
                        base.AgentBot.AgentEntity.DisableAnimationCommands();
                    }
                    this.m_deburrowAnimationStarted = true;
                }
                if (num >= DEBURROWING_TIME)
                {
                    this.m_deburrowing = false;
                    this.DeleteBurrowingParticleFX();
                    base.AgentBot.AgentEntity.EnableAnimationCommands();
                }
            }
        }

        public bool IsBurrowing =>
            this.m_burrowing;

        public bool IsDeburrowing =>
            this.m_deburrowing;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySpiderLogic.<>c <>9 = new MySpiderLogic.<>c();
            public static Func<MyCharacter, Action<string>> <>9__19_0;
            public static Func<MyCharacter, Action<string>> <>9__20_0;
            public static Func<MyCharacter, Action<Vector3D>> <>9__22_0;
            public static Func<MyCharacter, Action<Vector3D>> <>9__23_0;

            internal Action<Vector3D> <CreateBurrowingParticleFX>b__22_0(MyCharacter x) => 
                new Action<Vector3D>(x.CreateBurrowingParticleFX_Client);

            internal Action<Vector3D> <DeleteBurrowingParticleFX>b__23_0(MyCharacter x) => 
                new Action<Vector3D>(x.DeleteBurrowingParticleFX_Client);

            internal Action<string> <StartBurrowing>b__19_0(MyCharacter x) => 
                new Action<string>(x.TriggerAnimationEvent);

            internal Action<string> <StartDeburrowing>b__20_0(MyCharacter x) => 
                new Action<string>(x.TriggerAnimationEvent);
        }
    }
}

