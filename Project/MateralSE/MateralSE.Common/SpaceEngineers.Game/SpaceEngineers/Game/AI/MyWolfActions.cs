namespace SpaceEngineers.Game.AI
{
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MyBehaviorDescriptor("Wolf"), BehaviorActionImpl(typeof(MyWolfLogic))]
    public class MyWolfActions : MyAgentActions
    {
        private Vector3D? m_runAwayPos;
        private Vector3D? m_lastTargetedEntityPosition;
        private Vector3D? m_debugTarget;

        public MyWolfActions(MyAnimalBot bot) : base(bot)
        {
        }

        [MyBehaviorTreeAction("Attack")]
        protected MyBehaviorTreeState Attack() => 
            (this.WolfTarget.IsAttacking ? MyBehaviorTreeState.RUNNING : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("Explode")]
        protected MyBehaviorTreeState Explode()
        {
            this.WolfLogic.ActivateSelfDestruct();
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("GetTargetWithPriority")]
        protected MyBehaviorTreeState GetTargetWithPriority([BTParam] float radius, [BTInOut] ref MyBBMemoryTarget outTarget, [BTInOut] ref MyBBMemoryInt priority)
        {
            int num;
            if (this.WolfLogic.SelfDestructionActivated)
            {
                return MyBehaviorTreeState.SUCCESS;
            }
            if (base.Bot == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            if (base.Bot.AgentEntity == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            BoundingSphereD boundingSphere = new BoundingSphereD(base.Bot.Navigation.PositionAndOrientation.Translation, (double) radius);
            if (priority == null)
            {
                priority = new MyBBMemoryInt();
            }
            if ((priority.IntValue <= 0) || base.Bot.Navigation.Stuck)
            {
                num = 0x7fffffff;
            }
            MyBehaviorTreeState sUCCESS = base.IsTargetValid(ref outTarget);
            if (sUCCESS == MyBehaviorTreeState.FAILURE)
            {
                num = 7;
                MyBBMemoryTarget.UnsetTarget(ref outTarget);
            }
            if (this.WolfTarget == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            Vector3D? memoryTargetPosition = this.WolfTarget.GetMemoryTargetPosition(outTarget);
            if ((memoryTargetPosition == null) || (Vector3D.DistanceSquared(memoryTargetPosition.Value, base.Bot.AgentEntity.PositionComp.GetPosition()) > 160000.0))
            {
                num = 7;
                MyBBMemoryTarget.UnsetTarget(ref outTarget);
            }
            if (memoryTargetPosition != null)
            {
                Vector3D position = memoryTargetPosition.Value;
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                if (closestPlanet != null)
                {
                    Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref position);
                    if ((Vector3D.DistanceSquared(closestSurfacePointGlobal, position) > 2.25) && (Vector3D.DistanceSquared(closestSurfacePointGlobal, base.Bot.AgentEntity.PositionComp.GetPosition()) < 25.0))
                    {
                        num = 7;
                        MyBBMemoryTarget.UnsetTarget(ref outTarget);
                    }
                }
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
                if (!(entity is MyVoxelBase) && this.WolfTarget.IsEntityReachable(entity))
                {
                    Vector3D position = entity.PositionComp.GetPosition();
                    MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                    if (closestPlanet != null)
                    {
                        Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref position);
                        if (Vector3D.DistanceSquared(closestSurfacePointGlobal, position) > 1.0)
                        {
                            continue;
                        }
                    }
                    int num2 = 6;
                    MyCharacter character = entity as MyCharacter;
                    if (character != null)
                    {
                        MyFaction objA = MySession.Static.Factions.GetPlayerFaction(character.ControllerInfo.ControllingIdentityId);
                        if (((playerFaction == null) || !ReferenceEquals(objA, playerFaction)) && !character.IsDead)
                        {
                            num2 = 1;
                            if (num2 < num)
                            {
                                sUCCESS = MyBehaviorTreeState.SUCCESS;
                                num = num2;
                                Vector3D? nullable3 = null;
                                MyBBMemoryTarget.SetTargetEntity(ref outTarget, MyAiTargetEnum.CHARACTER, character.EntityId, nullable3);
                                this.m_lastTargetedEntityPosition = new Vector3D?(character.PositionComp.GetPosition());
                            }
                        }
                    }
                }
            }
            topMostEntitiesInSphere.Clear();
            priority.IntValue = num;
            if (outTarget.TargetType == MyAiTargetEnum.NO_TARGET)
            {
                sUCCESS = MyBehaviorTreeState.FAILURE;
            }
            return sUCCESS;
        }

        [MyBehaviorTreeAction("GoToPlayerDefinedTarget", ReturnsRunning=true)]
        protected MyBehaviorTreeState GoToPlayerDefinedTarget()
        {
            Vector3D? debugTarget = this.m_debugTarget;
            Vector3D? nullable2 = MyAIComponent.Static.DebugTarget;
            if (((debugTarget != null) == (nullable2 != null)) ? ((debugTarget != null) ? (debugTarget.GetValueOrDefault() != nullable2.GetValueOrDefault()) : false) : true)
            {
                this.m_debugTarget = MyAIComponent.Static.DebugTarget;
                if (MyAIComponent.Static.DebugTarget == null)
                {
                    return MyBehaviorTreeState.FAILURE;
                }
            }
            Vector3D position = base.Bot.Player.Character.PositionComp.GetPosition();
            if (this.m_debugTarget != null)
            {
                Vector3D vectord3;
                float num;
                IMyEntity entity;
                if (Vector3D.Distance(position, this.m_debugTarget.Value) <= 1.0)
                {
                    return MyBehaviorTreeState.SUCCESS;
                }
                if (!MyAIComponent.Static.Pathfinding.FindPathGlobal(position, new MyDestinationSphere(ref this.m_debugTarget.Value, 1f), null).GetNextTarget(position, out vectord3, out num, out entity))
                {
                    return MyBehaviorTreeState.FAILURE;
                }
                if (this.WolfTarget.TargetPosition != vectord3)
                {
                    this.WolfTarget.SetTargetPosition(vectord3);
                }
                this.WolfTarget.AimAtTarget();
                this.WolfTarget.GotoTargetNoPath(0f, false);
            }
            return MyBehaviorTreeState.RUNNING;
        }

        protected override MyBehaviorTreeState Idle() => 
            MyBehaviorTreeState.RUNNING;

        [MyBehaviorTreeAction("Attack", MyBehaviorTreeActionType.INIT)]
        protected void Init_Attack()
        {
            this.WolfTarget.AimAtTarget();
            (this.WolfTarget.TargetPosition - base.Bot.AgentEntity.PositionComp.GetPosition()).Normalize();
            this.WolfTarget.Attack(!this.WolfLogic.SelfDestructionActivated);
        }

        [MyBehaviorTreeAction("IsAttacking", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsAttacking() => 
            (this.WolfTarget.IsAttacking ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("IsRunningAway", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsRunningAway() => 
            ((this.m_runAwayPos != null) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("Attack", MyBehaviorTreeActionType.POST)]
        protected void Post_Attack()
        {
        }

        [MyBehaviorTreeAction("RunAway")]
        protected MyBehaviorTreeState RunAway([BTParam] float distance)
        {
            if (this.m_runAwayPos != null)
            {
                if (base.Bot.Navigation.Stuck)
                {
                    return MyBehaviorTreeState.FAILURE;
                }
            }
            else
            {
                Vector3D position = base.Bot.Player.Character.PositionComp.GetPosition();
                Vector3D v = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                if (closestPlanet == null)
                {
                    return MyBehaviorTreeState.FAILURE;
                }
                if (this.m_lastTargetedEntityPosition != null)
                {
                    Vector3D globalPos = this.m_lastTargetedEntityPosition.Value;
                    Vector3D vectord5 = position + (Vector3D.Normalize(position - closestPlanet.GetClosestSurfacePointGlobal(ref globalPos)) * distance);
                    this.m_runAwayPos = new Vector3D?(closestPlanet.GetClosestSurfacePointGlobal(ref vectord5));
                }
                else
                {
                    v.Normalize();
                    Vector3D vectord6 = Vector3D.CalculatePerpendicularVector(v);
                    Vector3D bitangent = Vector3D.Cross(v, vectord6);
                    vectord6.Normalize();
                    bitangent.Normalize();
                    Vector3D vectord8 = MyUtils.GetRandomDiscPosition(ref position, (double) distance, (double) distance, ref vectord6, ref bitangent);
                    this.m_runAwayPos = (closestPlanet == null) ? new Vector3D?(vectord8) : new Vector3D?(closestPlanet.GetClosestSurfacePointGlobal(ref vectord8));
                }
                base.AiTargetBase.SetTargetPosition(this.m_runAwayPos.Value);
                base.AimWithMovement();
            }
            base.AiTargetBase.GotoTargetNoPath(1f, false);
            if (Vector3D.DistanceSquared(this.m_runAwayPos.Value, base.Bot.Player.Character.PositionComp.GetPosition()) >= 100.0)
            {
                return MyBehaviorTreeState.RUNNING;
            }
            this.WolfLogic.Remove();
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("RunAway", MyBehaviorTreeActionType.INIT)]
        protected MyBehaviorTreeState RunAway_Init() => 
            MyBehaviorTreeState.RUNNING;

        private MyWolfTarget WolfTarget =>
            (base.AiTargetBase as MyWolfTarget);

        protected MyWolfLogic WolfLogic =>
            (base.Bot.AgentLogic as MyWolfLogic);
    }
}

