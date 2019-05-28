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
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRageMath;

    [TargetType("Spider")]
    public class MySpiderTarget : MyAiTargetBase
    {
        private int m_attackStart;
        private int m_attackCtr;
        private bool m_attackPerformed;
        private BoundingSphereD m_attackBoundingSphere;
        private static readonly int ATTACK_LENGTH = 0x3e8;
        private static readonly int ATTACK_ACTIVATION = 700;
        private static readonly int ATTACK_DAMAGE_TO_CHARACTER = 0x23;
        private static readonly int ATTACK_DAMAGE_TO_GRID = 50;
        private static HashSet<MySlimBlock> m_tmpBlocks = new HashSet<MySlimBlock>();

        public MySpiderTarget(IMyEntityBot bot) : base(bot)
        {
        }

        public void Attack()
        {
            MyCharacter agentEntity = base.m_bot.AgentEntity;
            if (agentEntity != null)
            {
                string str;
                string str2;
                this.IsAttacking = true;
                this.m_attackPerformed = false;
                this.m_attackStart = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.ChooseAttackAnimationAndSound(out str, out str2);
                agentEntity.PlayCharacterAnimation(str, MyBlendOption.Immediate, MyFrameOption.PlayOnce, 0f, 1f, true, null, false);
                agentEntity.DisableAnimationCommands();
                agentEntity.SoundComp.StartSecondarySound(str2, true);
            }
        }

        private void ChooseAttackAnimationAndSound(out string animation, out string sound)
        {
            this.m_attackCtr++;
            MyAiTargetEnum targetType = base.TargetType;
            if (((targetType - 2) <= MyAiTargetEnum.GRID) || (targetType != MyAiTargetEnum.CHARACTER))
            {
                animation = "AttackFrontLegs";
                sound = "ArcBotSpiderAttackClaw";
            }
            else
            {
                MyCharacter targetEntity = base.TargetEntity as MyCharacter;
                if ((targetEntity == null) || !targetEntity.IsDead)
                {
                    if ((this.m_attackCtr % 2) == 0)
                    {
                        animation = "AttackStinger";
                        sound = "ArcBotSpiderAttackSting";
                    }
                    else
                    {
                        animation = "AttackBite";
                        sound = "ArcBotSpiderAttackBite";
                    }
                }
                else if ((this.m_attackCtr % 3) == 0)
                {
                    animation = "AttackFrontLegs";
                    sound = "ArcBotSpiderAttackClaw";
                }
                else
                {
                    animation = "AttackBite";
                    sound = "ArcBotSpiderAttackBite";
                }
            }
        }

        public override bool IsMemoryTargetValid(MyBBMemoryTarget targetMemory) => 
            ((targetMemory != null) ? ((targetMemory.TargetType != MyAiTargetEnum.GRID) ? ((targetMemory.TargetType != MyAiTargetEnum.CUBE) ? base.IsMemoryTargetValid(targetMemory) : false) : false) : false);

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
                else if (((num > 500) && base.m_bot.AgentEntity.UseNewAnimationSystem) && !this.m_attackPerformed)
                {
                    base.m_bot.AgentEntity.TriggerAnimationEvent("attack");
                    if (Sync.IsServer)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, string>(base.m_bot.AgentEntity, x => new Action<string>(x.TriggerAnimationEvent), "attack", targetEndpoint);
                    }
                }
                if ((num > 750) && !this.m_attackPerformed)
                {
                    MyCharacter agentEntity = base.m_bot.AgentEntity;
                    if (agentEntity != null)
                    {
                        Vector3D center = (agentEntity.WorldMatrix.Translation + (agentEntity.PositionComp.WorldMatrix.Forward * 2.5)) + (agentEntity.PositionComp.WorldMatrix.Up * 1.0);
                        this.m_attackBoundingSphere = new BoundingSphereD(center, 0.9);
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
                                        character3.DoDamage((float) ATTACK_DAMAGE_TO_CHARACTER, MyDamageType.Spider, true, agentEntity.EntityId);
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
            public static readonly MySpiderTarget.<>c <>9 = new MySpiderTarget.<>c();
            public static Func<MyCharacter, Action<string>> <>9__15_0;

            internal Action<string> <Update>b__15_0(MyCharacter x) => 
                new Action<string>(x.TriggerAnimationEvent);
        }
    }
}

