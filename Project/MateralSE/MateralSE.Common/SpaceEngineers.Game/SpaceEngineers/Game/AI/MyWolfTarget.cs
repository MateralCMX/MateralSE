namespace SpaceEngineers.Game.AI
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [TargetType("Wolf"), StaticEventOwner]
    public class MyWolfTarget : MyAiTargetBase
    {
        private int m_attackStart;
        private bool m_attackPerformed;
        private BoundingSphereD m_attackBoundingSphere;
        private static readonly int ATTACK_LENGTH = 0x3e8;
        private static readonly int ATTACK_DAMAGE_TO_CHARACTER = 12;
        private static readonly int ATTACK_DAMAGE_TO_GRID = 8;
        private static HashSet<MySlimBlock> m_tmpBlocks = new HashSet<MySlimBlock>();
        private static MyStringId m_stringIdAttackAction = MyStringId.GetOrCompute("attack");

        public MyWolfTarget(IMyEntityBot bot) : base(bot)
        {
        }

        public void Attack(bool playSound)
        {
            MyCharacter agentEntity = base.m_bot.AgentEntity;
            if (agentEntity != null)
            {
                this.IsAttacking = true;
                this.m_attackPerformed = false;
                this.m_attackStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                string animationName = "WolfAttack";
                string cueName = "ArcBotWolfAttack";
                if (!agentEntity.UseNewAnimationSystem)
                {
                    agentEntity.PlayCharacterAnimation(animationName, MyBlendOption.Immediate, MyFrameOption.PlayOnce, 0f, 1f, true, null, false);
                    agentEntity.DisableAnimationCommands();
                }
                agentEntity.SoundComp.StartSecondarySound(cueName, true);
            }
        }

        public override bool IsMemoryTargetValid(MyBBMemoryTarget targetMemory) => 
            ((targetMemory != null) ? ((targetMemory.TargetType != MyAiTargetEnum.GRID) ? ((targetMemory.TargetType != MyAiTargetEnum.CUBE) ? base.IsMemoryTargetValid(targetMemory) : false) : false) : false);

        [Event(null, 0x97), Broadcast, Reliable]
        private static void PlayAttackAnimation(long entityId)
        {
            if (MyEntities.EntityExists(entityId))
            {
                MyCharacter entityById = MyEntities.GetEntityById(entityId, false) as MyCharacter;
                if (entityById != null)
                {
                    entityById.AnimationController.TriggerAction(m_stringIdAttackAction);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.IsAttacking)
            {
                int num = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_attackStart;
                if (num > ATTACK_LENGTH)
                {
                    this.IsAttacking = false;
                    MyCharacter agentEntity = base.m_bot.AgentEntity;
                    if (agentEntity != null)
                    {
                        agentEntity.EnableAnimationCommands();
                    }
                }
                else if ((num > 500) && base.m_bot.AgentEntity.UseNewAnimationSystem)
                {
                    base.m_bot.AgentEntity.AnimationController.TriggerAction(m_stringIdAttackAction);
                    if (Sync.IsServer)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, string>(base.m_bot.AgentEntity, x => new Action<string>(x.TriggerAnimationEvent), m_stringIdAttackAction.String, targetEndpoint);
                    }
                }
                if ((num > 500) && !this.m_attackPerformed)
                {
                    MyCharacter agentEntity = base.m_bot.AgentEntity;
                    if (agentEntity != null)
                    {
                        Vector3D center = (agentEntity.WorldMatrix.Translation + (agentEntity.PositionComp.WorldMatrix.Forward * 1.1000000238418579)) + (agentEntity.PositionComp.WorldMatrix.Up * 0.44999998807907104);
                        this.m_attackBoundingSphere = new BoundingSphereD(center, 0.5);
                        this.m_attackPerformed = true;
                        List<MyEntity> topMostEntitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref this.m_attackBoundingSphere);
                        foreach (MyEntity entity in topMostEntitiesInSphere)
                        {
                            if (!(entity is MyCharacter))
                            {
                                continue;
                            }
                            if (!ReferenceEquals(entity, agentEntity))
                            {
                                MyCharacter character3 = entity as MyCharacter;
                                if (!character3.IsSitting)
                                {
                                    BoundingSphereD worldVolume = character3.PositionComp.WorldVolume;
                                    double num2 = this.m_attackBoundingSphere.Radius + worldVolume.Radius;
                                    num2 *= num2;
                                    if (Vector3D.DistanceSquared(this.m_attackBoundingSphere.Center, worldVolume.Center) <= num2)
                                    {
                                        character3.DoDamage((float) ATTACK_DAMAGE_TO_CHARACTER, MyDamageType.Wolf, true, agentEntity.EntityId);
                                    }
                                }
                            }
                        }
                        topMostEntitiesInSphere.Clear();
                    }
                }
            }
        }

        public bool IsAttacking { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyWolfTarget.<>c <>9 = new MyWolfTarget.<>c();
            public static Func<MyCharacter, Action<string>> <>9__14_0;

            internal Action<string> <Update>b__14_0(MyCharacter x) => 
                new Action<string>(x.TriggerAnimationEvent);
        }
    }
}

