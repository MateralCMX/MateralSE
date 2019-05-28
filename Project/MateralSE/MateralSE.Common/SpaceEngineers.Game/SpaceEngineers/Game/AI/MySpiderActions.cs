namespace SpaceEngineers.Game.AI
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    [MyBehaviorDescriptor("Spider"), BehaviorActionImpl(typeof(MySpiderLogic))]
    public class MySpiderActions : MyAgentActions
    {
        public MySpiderActions(MyAnimalBot bot) : base(bot)
        {
        }

        [MyBehaviorTreeAction("Attack")]
        protected MyBehaviorTreeState Attack() => 
            (this.SpiderTarget.IsAttacking ? MyBehaviorTreeState.RUNNING : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("Burrow")]
        protected MyBehaviorTreeState Burrow() => 
            (!this.SpiderLogic.IsBurrowing ? MyBehaviorTreeState.NOT_TICKED : MyBehaviorTreeState.RUNNING);

        [MyBehaviorTreeAction("Deburrow")]
        protected MyBehaviorTreeState Deburrow() => 
            (!this.SpiderLogic.IsDeburrowing ? MyBehaviorTreeState.NOT_TICKED : MyBehaviorTreeState.RUNNING);

        [MyBehaviorTreeAction("GetTargetWithPriority")]
        protected MyBehaviorTreeState GetTargetWithPriority([BTParam] float radius, [BTInOut] ref MyBBMemoryTarget outTarget, [BTInOut] ref MyBBMemoryInt priority)
        {
            int num;
            MatrixD positionAndOrientation = base.Bot.Navigation.PositionAndOrientation;
            BoundingSphereD boundingSphere = new BoundingSphereD(positionAndOrientation.Translation, (double) radius);
            if (priority == null)
            {
                priority = new MyBBMemoryInt();
            }
            if (priority.IntValue <= 0)
            {
                num = 0x7fffffff;
            }
            MyBehaviorTreeState sUCCESS = base.IsTargetValid(ref outTarget);
            if (sUCCESS == MyBehaviorTreeState.FAILURE)
            {
                num = 7;
                MyBBMemoryTarget.UnsetTarget(ref outTarget);
            }
            Vector3D? memoryTargetPosition = this.SpiderTarget.GetMemoryTargetPosition(outTarget);
            if ((memoryTargetPosition == null) || (Vector3D.Distance(memoryTargetPosition.Value, base.Bot.AgentEntity.PositionComp.GetPosition()) > 400.0))
            {
                num = 7;
                MyBBMemoryTarget.UnsetTarget(ref outTarget);
            }
            MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(base.Bot.AgentEntity.ControllerInfo.ControllingIdentityId);
            List<MyEntity> topMostEntitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref boundingSphere);
            int? count = null;
            topMostEntitiesInSphere.ShuffleList<MyEntity>(0, count);
            foreach (MyEntity entity in topMostEntitiesInSphere)
            {
                if (ReferenceEquals(entity, base.Bot.AgentEntity))
                {
                    continue;
                }
                if (this.SpiderTarget.IsEntityReachable(entity))
                {
                    int num2 = 6;
                    MyCharacter objB = entity as MyCharacter;
                    if ((objB != null) && (objB.ControllerInfo != null))
                    {
                        MyFaction objA = MySession.Static.Factions.GetPlayerFaction(objB.ControllerInfo.ControllingIdentityId);
                        if (((playerFaction == null) || !ReferenceEquals(objA, playerFaction)) && !objB.IsDead)
                        {
                            MyPhysics.HitInfo? nullable3 = MyPhysics.CastRay(objB.WorldMatrix.Translation - (3.0 * objB.WorldMatrix.Up), objB.WorldMatrix.Translation + (3.0 * objB.WorldMatrix.Up), 15);
                            if ((nullable3 != null) && !ReferenceEquals(nullable3.HitEntity, objB))
                            {
                                num2 = 1;
                                if (num2 < num)
                                {
                                    sUCCESS = MyBehaviorTreeState.SUCCESS;
                                    num = num2;
                                    Vector3D? position = null;
                                    MyBBMemoryTarget.SetTargetEntity(ref outTarget, MyAiTargetEnum.CHARACTER, objB.EntityId, position);
                                }
                            }
                        }
                    }
                }
            }
            topMostEntitiesInSphere.Clear();
            priority.IntValue = num;
            return sUCCESS;
        }

        protected override MyBehaviorTreeState Idle() => 
            MyBehaviorTreeState.RUNNING;

        [MyBehaviorTreeAction("Attack", MyBehaviorTreeActionType.INIT)]
        protected void Init_Attack()
        {
            this.SpiderTarget.AimAtTarget();
            (this.SpiderTarget.TargetPosition - base.Bot.AgentEntity.PositionComp.GetPosition()).Normalize();
            this.SpiderTarget.Attack();
        }

        [MyBehaviorTreeAction("Burrow", MyBehaviorTreeActionType.INIT)]
        protected void Init_Burrow()
        {
            this.SpiderLogic.StartBurrowing();
        }

        [MyBehaviorTreeAction("Deburrow", MyBehaviorTreeActionType.INIT)]
        protected void Init_Deburrow()
        {
            this.SpiderLogic.StartDeburrowing();
        }

        [MyBehaviorTreeAction("IsAttacking", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsAttacking() => 
            (this.SpiderTarget.IsAttacking ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("Attack", MyBehaviorTreeActionType.POST)]
        protected void Post_Attack()
        {
        }

        [MyBehaviorTreeAction("Teleport", ReturnsRunning=false)]
        protected MyBehaviorTreeState Teleport()
        {
            MatrixD xd;
            if (base.Bot.Player.Character.HasAnimation("Deburrow"))
            {
                base.Bot.Player.Character.PlayCharacterAnimation("Deburrow", MyBlendOption.Immediate, MyFrameOption.JustFirstFrame, 0f, 1f, true, null, false);
                base.Bot.AgentEntity.DisableAnimationCommands();
            }
            if (!MySpaceBotFactory.GetSpiderSpawnPosition(out xd, new Vector3D?(base.Bot.Player.GetPosition())))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            Vector3D translation = xd.Translation;
            if (MyPhysics.CastRay(translation + (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), translation - (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), 9) != null)
            {
                return MyBehaviorTreeState.NOT_TICKED;
            }
            MyPhysics.HitInfo? nullable2 = MyPhysics.CastRay(base.Bot.AgentEntity.WorldMatrix.Translation - (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), base.Bot.AgentEntity.WorldMatrix.Translation + (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), 9);
            if ((nullable2 != null) && !ReferenceEquals(nullable2.HitEntity, base.Bot.AgentEntity))
            {
                nullable2 = MyPhysics.CastRay(base.Bot.AgentEntity.WorldMatrix.Translation - (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), base.Bot.AgentEntity.WorldMatrix.Translation + (3.0 * base.Bot.AgentEntity.WorldMatrix.Up), 9);
                return MyBehaviorTreeState.NOT_TICKED;
            }
            float radius = (float) base.Bot.AgentEntity.PositionComp.WorldVolume.Radius;
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(translation);
            if (closestPlanet != null)
            {
                closestPlanet.CorrectSpawnLocation(ref translation, (double) radius);
                xd.Translation = translation;
            }
            else
            {
                Vector3D? nullable3 = MyEntities.FindFreePlace(xd.Translation, radius, 20, 5, 0.2f);
                if (nullable3 != null)
                {
                    xd.Translation = nullable3.Value;
                }
            }
            base.Bot.AgentEntity.SetPhysicsEnabled(false);
            base.Bot.AgentEntity.WorldMatrix = xd;
            base.Bot.AgentEntity.Physics.CharacterProxy.SetForwardAndUp((Vector3) xd.Forward, (Vector3) xd.Up);
            base.Bot.AgentEntity.SetPhysicsEnabled(true);
            return MyBehaviorTreeState.SUCCESS;
        }

        private MySpiderTarget SpiderTarget =>
            (base.AiTargetBase as MySpiderTarget);

        protected MySpiderLogic SpiderLogic =>
            (base.Bot.AgentLogic as MySpiderLogic);
    }
}

