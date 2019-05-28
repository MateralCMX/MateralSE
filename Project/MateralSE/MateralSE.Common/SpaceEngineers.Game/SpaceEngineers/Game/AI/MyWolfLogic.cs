namespace SpaceEngineers.Game.AI
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using VRageMath;

    public class MyWolfLogic : MyAgentLogic
    {
        private static readonly int SELF_DESTRUCT_TIME_MS = 0xfa0;
        private static readonly float EXPLOSION_RADIUS = 4f;
        private static readonly int EXPLOSION_DAMAGE = 0x1d4c;
        private static readonly int EXPLOSION_PLAYER_DAMAGE = 0;
        private bool m_selfDestruct;
        private int m_selfDestructStartedInTime;
        private bool m_lastWasAttacking;

        public MyWolfLogic(MyAnimalBot bot) : base(bot)
        {
        }

        public void ActivateSelfDestruct()
        {
            if (!this.m_selfDestruct)
            {
                this.m_selfDestructStartedInTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_selfDestruct = true;
                string cueName = "ArcBotCyberSelfActDestr";
                base.AgentBot.AgentEntity.SoundComp.StartSecondarySound(cueName, true);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public void Remove()
        {
            MyAIComponent.Static.RemoveBot(base.AgentBot.Player.Id.SerialId, true);
        }

        public override void Update()
        {
            base.Update();
            if (this.m_selfDestruct && (MySandboxGame.TotalGamePlayTimeInMilliseconds >= (this.m_selfDestructStartedInTime + SELF_DESTRUCT_TIME_MS)))
            {
                MyAIComponent.Static.RemoveBot(base.AgentBot.Player.Id.SerialId, true);
                BoundingSphere sphere = new BoundingSphere((Vector3) base.AgentBot.Player.GetPosition(), EXPLOSION_RADIUS);
                MyExplosionInfo explosionInfo = new MyExplosionInfo {
                    PlayerDamage = EXPLOSION_PLAYER_DAMAGE,
                    Damage = EXPLOSION_DAMAGE,
                    ExplosionType = MyExplosionTypeEnum.BOMB_EXPLOSION,
                    ExplosionSphere = sphere,
                    LifespanMiliseconds = 700,
                    HitEntity = base.AgentBot.Player.Character,
                    ParticleScale = 0.5f,
                    OwnerEntity = base.AgentBot.Player.Character,
                    Direction = new Vector3?(Vector3.Zero),
                    VoxelExplosionCenter = base.AgentBot.Player.Character.PositionComp.GetPosition(),
                    ExplosionFlags = MyExplosionFlags.APPLY_DEFORMATION | MyExplosionFlags.CREATE_SHRAPNELS | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.CREATE_DEBRIS,
                    VoxelCutoutScale = 0.6f,
                    PlaySound = true,
                    ApplyForceAndDamage = true,
                    ObjectsRemoveDelayInMiliseconds = 40
                };
                if (base.AgentBot.Player.Character.Physics != null)
                {
                    explosionInfo.Velocity = base.AgentBot.Player.Character.Physics.LinearVelocity;
                }
                MyExplosions.AddExplosion(ref explosionInfo, true);
            }
            MyWolfTarget aiTarget = base.AiTarget as MyWolfTarget;
            if (((base.AgentBot.Player.Character != null) && (!base.AgentBot.Player.Character.UseNewAnimationSystem && (!aiTarget.IsAttacking && (!this.m_lastWasAttacking && aiTarget.HasTarget())))) && !aiTarget.PositionIsNearTarget(base.AgentBot.Player.Character.PositionComp.GetPosition(), 1.5f))
            {
                if (!base.AgentBot.Navigation.Stuck)
                {
                    base.AgentBot.Player.Character.EnableAnimationCommands();
                }
                else
                {
                    Vector3D position = base.AgentBot.Player.Character.PositionComp.GetPosition();
                    Vector3D vectord2 = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                    Vector3D vectord3 = base.AgentBot.Player.Character.AimedPoint - position;
                    Vector3D vectord4 = vectord3 - ((vectord2 * Vector3D.Dot(vectord3, vectord2)) / vectord2.LengthSquared());
                    vectord4.Normalize();
                    base.AgentBot.Navigation.AimAt(null, new Vector3D?(position + (100.0 * vectord4)));
                    base.AgentBot.Player.Character.PlayCharacterAnimation("WolfIdle1", MyBlendOption.Immediate, MyFrameOption.Loop, 0f, 1f, false, null, false);
                    base.AgentBot.Player.Character.DisableAnimationCommands();
                }
            }
            this.m_lastWasAttacking = aiTarget.IsAttacking;
        }

        public bool SelfDestructionActivated =>
            this.m_selfDestruct;
    }
}

