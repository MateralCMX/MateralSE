namespace Sandbox.Game.AI.Actions
{
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyAgentActions : MyBotActionsBase
    {
        private string m_animationName;
        private MyRandomLocationSphere m_locationSphere;

        protected MyAgentActions(MyAgentBot bot)
        {
            this.Bot = bot;
            this.m_locationSphere = new MyRandomLocationSphere(Vector3D.Zero, 30f, Vector3D.UnitX);
        }

        [MyBehaviorTreeAction("AimAtTarget")]
        protected MyBehaviorTreeState AimAtTarget() => 
            this.AimAtTargetCustom(2f);

        [MyBehaviorTreeAction("AimAtTargetCustom")]
        protected MyBehaviorTreeState AimAtTargetCustom([BTParam] float tolerance) => 
            (this.AiTargetBase.HasTarget() ? (!this.Bot.Navigation.HasRotation(MathHelper.ToRadians(tolerance)) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.RUNNING) : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("AimWithMovement", ReturnsRunning=false)]
        protected MyBehaviorTreeState AimWithMovement()
        {
            this.Bot.Navigation.AimWithMovement();
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("CallMoveAndRotate")]
        protected MyBehaviorTreeState CallMoveAndRotate()
        {
            if (this.Bot.AgentEntity == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            this.Bot.AgentEntity.MoveAndRotate(Vector3.Zero, Vector2.One, 0f);
            return MyBehaviorTreeState.RUNNING;
        }

        private void CheckReplanningOfPath(Vector3D targetPos, Vector3D navigationTarget)
        {
            Vector3D vectord = targetPos - this.Bot.Navigation.PositionAndOrientation.Translation;
            Vector3D v = navigationTarget - this.Bot.Navigation.PositionAndOrientation.Translation;
            double num = vectord.Length();
            double num2 = v.Length();
            if ((num != 0.0) && (num2 != 0.0))
            {
                double num3 = num / num2;
                if (((Math.Acos(vectord.Dot(v) / (num * num2)) > 0.62831853071795862) || (num3 < 0.8)) || ((num3 > 1.0) && (num2 < 2.0)))
                {
                    this.AiTargetBase.GotoTarget();
                    this.AiTargetBase.AimAtTarget();
                }
            }
        }

        [MyBehaviorTreeAction("ClearTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState ClearTarget([BTInOut] ref MyBBMemoryTarget inTarget)
        {
            if (inTarget != null)
            {
                inTarget.TargetType = MyAiTargetEnum.NO_TARGET;
                inTarget.Position = null;
                inTarget.EntityId = null;
                inTarget.TreeId = null;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("ClearUnreachableEntities")]
        protected MyBehaviorTreeState ClearUnreachableEntities()
        {
            this.AiTargetBase.ClearUnreachableEntities();
            return MyBehaviorTreeState.SUCCESS;
        }

        protected MyCharacter FindCharacterInRadius(int radius, bool ignoreReachability = false)
        {
            Vector3D translation = this.Bot.Navigation.PositionAndOrientation.Translation;
            MyCharacter character = null;
            double num = 3.4028234663852886E+38;
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SerialId != 0)
                {
                    MyHumanoidBot bot = MyAIComponent.Static.Bots.TryGetBot<MyHumanoidBot>(player.Id.SerialId);
                    if (bot == null)
                    {
                        continue;
                    }
                    if (bot.BotDefinition.BehaviorType == "Barbarian")
                    {
                        continue;
                    }
                }
                if (((player.Character != null) && (ignoreReachability || this.AiTargetBase.IsEntityReachable(player.Character))) && !player.Character.IsDead)
                {
                    double num2 = Vector3D.DistanceSquared(player.Character.PositionComp.GetPosition(), translation);
                    if ((num2 < (radius * radius)) && (num2 < num))
                    {
                        character = player.Character;
                        num = num2;
                    }
                }
            }
            return character;
        }

        [MyBehaviorTreeAction("FindCharacterInRadius", ReturnsRunning=false)]
        protected MyBehaviorTreeState FindCharacterInRadius([BTParam] int radius, [BTOut] ref MyBBMemoryTarget outCharacter)
        {
            MyCharacter character = this.FindCharacterInRadius(radius, false);
            if (character == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            Vector3D? position = null;
            MyBBMemoryTarget.SetTargetEntity(ref outCharacter, MyAiTargetEnum.CHARACTER, character.EntityId, position);
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("FindClosestBlock", ReturnsRunning=false)]
        protected MyBehaviorTreeState FindClosestBlock([BTOut] ref MyBBMemoryTarget outBlock)
        {
            if (!this.AiTargetBase.IsTargetGridOrBlock(this.AiTargetBase.TargetType))
            {
                outBlock = null;
                return MyBehaviorTreeState.FAILURE;
            }
            MyCubeGrid targetGrid = this.AiTargetBase.TargetGrid;
            Vector3 vector = (Vector3) Vector3D.Transform(this.Bot.BotEntity.PositionComp.GetPosition(), targetGrid.PositionComp.WorldMatrixNormalizedInv);
            float maxValue = float.MaxValue;
            MySlimBlock block = null;
            foreach (MySlimBlock block2 in targetGrid.GetBlocks())
            {
                float num2 = Vector3.DistanceSquared((Vector3) (block2.Position * targetGrid.GridSize), vector);
                if (num2 < maxValue)
                {
                    block = block2;
                    maxValue = num2;
                }
            }
            if (block == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            MyBBMemoryTarget.SetTargetCube(ref outBlock, block.Position, block.CubeGrid.EntityId);
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("FindClosestPlaceAreaInRadius", ReturnsRunning=false)]
        protected MyBehaviorTreeState FindClosestPlaceAreaInRadius([BTParam] float radius, [BTParam] string typeName, [BTOut] ref MyBBMemoryTarget outTarget) => 
            (!MyItemsCollector.FindClosestPlaceAreaInSphere(new BoundingSphereD(this.Bot.AgentEntity.PositionComp.GetPosition(), (double) radius), typeName, ref outTarget) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        private static Vector3D GetRandomPerpendicularVector(ref Vector3D axis)
        {
            Vector3D vectord2;
            Vector3D vectord = Vector3D.CalculatePerpendicularVector((Vector3D) axis);
            Vector3D.Cross(ref axis, ref vectord, out vectord2);
            double randomDouble = MyUtils.GetRandomDouble(0.0, 6.2831859588623047);
            return (Vector3D) ((Math.Cos(randomDouble) * vectord) + (Math.Sin(randomDouble) * vectord2));
        }

        [MyBehaviorTreeAction("GotoAndAimTarget")]
        protected MyBehaviorTreeState GotoAndAimTarget()
        {
            if (!this.AiTargetBase.HasTarget())
            {
                return MyBehaviorTreeState.FAILURE;
            }
            if (!this.Bot.Navigation.Navigating)
            {
                if (this.Bot.Navigation.HasRotation(MathHelper.ToRadians((float) 2f)))
                {
                    return MyBehaviorTreeState.RUNNING;
                }
                if (this.AiTargetBase.PositionIsNearTarget(this.Bot.Navigation.PositionAndOrientation.Translation, 2f))
                {
                    return MyBehaviorTreeState.SUCCESS;
                }
                this.AiTargetBase.GotoFailed();
                return MyBehaviorTreeState.FAILURE;
            }
            if (this.Bot.Navigation.Stuck)
            {
                this.AiTargetBase.GotoFailed();
                return MyBehaviorTreeState.FAILURE;
            }
            Vector3D targetPosition = this.AiTargetBase.GetTargetPosition(this.Bot.Navigation.PositionAndOrientation.Translation);
            Vector3D targetPoint = this.Bot.Navigation.TargetPoint;
            if ((targetPoint - targetPosition).Length() > 0.10000000149011612)
            {
                this.CheckReplanningOfPath(targetPosition, targetPoint);
            }
            return MyBehaviorTreeState.RUNNING;
        }

        [MyBehaviorTreeAction("GotoFailed", ReturnsRunning=false)]
        protected MyBehaviorTreeState GotoFailed() => 
            (!this.AiTargetBase.HasGotoFailed ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("GotoRandomLocation")]
        protected MyBehaviorTreeState GotoRandomLocation() => 
            this.GotoTarget();

        [MyBehaviorTreeAction("GotoTarget")]
        protected MyBehaviorTreeState GotoTarget()
        {
            if (this.AiTargetBase.HasTarget())
            {
                if (!this.Bot.Navigation.Navigating)
                {
                    return MyBehaviorTreeState.SUCCESS;
                }
                if (!this.Bot.Navigation.Stuck)
                {
                    return MyBehaviorTreeState.RUNNING;
                }
                this.AiTargetBase.GotoFailed();
            }
            return MyBehaviorTreeState.FAILURE;
        }

        [MyBehaviorTreeAction("GotoTargetNoPathfinding")]
        protected MyBehaviorTreeState GotoTargetNoPathfinding([BTParam] float radius, [BTParam] bool resetStuckDetection)
        {
            if (!this.AiTargetBase.HasTarget())
            {
                return MyBehaviorTreeState.FAILURE;
            }
            if (!this.Bot.Navigation.Navigating)
            {
                return MyBehaviorTreeState.SUCCESS;
            }
            if (this.Bot.Navigation.Stuck)
            {
                this.AiTargetBase.GotoFailed();
                return MyBehaviorTreeState.FAILURE;
            }
            this.AiTargetBase.GotoTargetNoPath(radius, resetStuckDetection);
            return MyBehaviorTreeState.RUNNING;
        }

        [MyBehaviorTreeAction("HasCharacter", ReturnsRunning=false)]
        protected MyBehaviorTreeState HasCharacter() => 
            ((this.Bot.AgentEntity != null) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("HasNoTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState HasNoTarget() => 
            ((this.HasTarget() == MyBehaviorTreeState.SUCCESS) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("HasTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState HasTarget() => 
            ((this.AiTargetBase.TargetType == MyAiTargetEnum.NO_TARGET) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("HasPlaceArea", ReturnsRunning=false)]
        protected MyBehaviorTreeState HasTargetArea([BTIn] ref MyBBMemoryTarget inTarget)
        {
            if ((inTarget != null) && (inTarget.EntityId != null))
            {
                VRage.Game.Entity.MyEntity entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(inTarget.EntityId.Value, out entity, false))
                {
                    MyPlaceArea component = null;
                    if (entity.Components.TryGet<MyPlaceArea>(out component))
                    {
                        return MyBehaviorTreeState.SUCCESS;
                    }
                }
            }
            return MyBehaviorTreeState.FAILURE;
        }

        [MyBehaviorTreeAction("AimAtTarget", MyBehaviorTreeActionType.INIT)]
        protected void Init_AimAtTarget()
        {
            this.Init_AimAtTargetCustom();
        }

        [MyBehaviorTreeAction("AimAtTargetCustom", MyBehaviorTreeActionType.INIT)]
        protected void Init_AimAtTargetCustom()
        {
            if (this.AiTargetBase.HasTarget())
            {
                this.AiTargetBase.AimAtTarget();
            }
        }

        [MyBehaviorTreeAction("GotoAndAimTarget", MyBehaviorTreeActionType.INIT)]
        protected void Init_GotoAndAimTarget()
        {
            if (this.AiTargetBase.HasTarget())
            {
                this.AiTargetBase.GotoTarget();
                this.AiTargetBase.AimAtTarget();
            }
        }

        [MyBehaviorTreeAction("GotoRandomLocation", MyBehaviorTreeActionType.INIT)]
        protected void Init_GotoRandomLocation()
        {
            Vector3D position = this.Bot.AgentEntity.PositionComp.GetPosition();
            Vector3D axis = MyPerGameSettings.NavmeshPresumesDownwardGravity ? Vector3D.UnitY : MyGravityProviderSystem.CalculateTotalGravityInPoint(position);
            axis.Normalize();
            Vector3D randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref axis);
            Vector3D worldCenter = position - (randomPerpendicularVector * 15.0);
            this.AiTargetBase.SetTargetPosition(position + (randomPerpendicularVector * 30.0));
            this.Bot.Navigation.AimAt(null, new Vector3D?(this.AiTargetBase.TargetPosition));
            this.m_locationSphere.Init(ref worldCenter, 30f, randomPerpendicularVector);
            this.Bot.Navigation.Goto(this.m_locationSphere, null);
        }

        [MyBehaviorTreeAction("GotoTarget", MyBehaviorTreeActionType.INIT)]
        protected virtual void Init_GotoTarget()
        {
            if (this.AiTargetBase.HasTarget())
            {
                this.AiTargetBase.GotoTarget();
            }
        }

        [MyBehaviorTreeAction("GotoTargetNoPathfinding", MyBehaviorTreeActionType.INIT)]
        protected virtual void Init_GotoTargetNoPathfinding()
        {
            if (this.AiTargetBase.HasTarget())
            {
                this.AiTargetBase.GotoTargetNoPath(1f, true);
            }
        }

        [MyBehaviorTreeAction("IsAtTargetPosition", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsAtTargetPosition([BTParam] float radius) => 
            (this.AiTargetBase.HasTarget() ? (!this.AiTargetBase.PositionIsNearTarget(this.Bot.Player.Character.PositionComp.GetPosition(), radius) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS) : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("IsAtTargetPositionCylinder", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsAtTargetPositionCylinder([BTParam] float radius, [BTParam] float height)
        {
            Vector3D vectord2;
            float num;
            if (!this.AiTargetBase.HasTarget())
            {
                return MyBehaviorTreeState.FAILURE;
            }
            Vector3D position = this.Bot.Player.Character.PositionComp.GetPosition();
            this.AiTargetBase.GetTargetPosition(position, out vectord2, out num);
            Vector2 vector = new Vector2((float) position.X, (float) position.Z);
            Vector2 vector2 = new Vector2((float) vectord2.X, (float) vectord2.Z);
            if (((Vector2.Distance(vector, vector2) > radius) || (vector.Y >= vector2.Y)) || ((vector.Y + height) <= vector2.Y))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("IsCharacterInRadius", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsCharacterInRadius([BTParam] int radius)
        {
            MyCharacter character = this.FindCharacterInRadius(radius, false);
            if ((character == null) || character.IsDead)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("IsLookingAtTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsLookingAtTarget() => 
            (!this.Bot.Navigation.HasRotation(MathHelper.ToRadians((float) 2f)) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("IsMoving", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsMoving() => 
            (this.Bot.Navigation.Navigating ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("IsNoCharacterInRadius", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsNoCharacterInRadius([BTParam] int radius)
        {
            MyCharacter character = this.FindCharacterInRadius(radius, false);
            if ((character == null) || character.IsDead)
            {
                return MyBehaviorTreeState.SUCCESS;
            }
            return MyBehaviorTreeState.FAILURE;
        }

        [MyBehaviorTreeAction("IsNotAtTargetPosition", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsNotAtTargetPosition([BTParam] float radius) => 
            (this.AiTargetBase.HasTarget() ? (!this.AiTargetBase.PositionIsNearTarget(this.Bot.Player.Character.PositionComp.GetPosition(), radius) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE) : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("IsTargetBlock", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsTargetBlock([BTIn] ref MyBBMemoryTarget inTarget)
        {
            if ((inTarget.TargetType == MyAiTargetEnum.COMPOUND_BLOCK) || (inTarget.TargetType == MyAiTargetEnum.CUBE))
            {
                return MyBehaviorTreeState.SUCCESS;
            }
            return MyBehaviorTreeState.FAILURE;
        }

        [MyBehaviorTreeAction("IsTargetNonBlock", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsTargetNonBlock([BTIn] ref MyBBMemoryTarget inTarget)
        {
            if ((inTarget.TargetType == MyAiTargetEnum.COMPOUND_BLOCK) || (inTarget.TargetType == MyAiTargetEnum.CUBE))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("IsTargetValid", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsTargetValid([BTIn] ref MyBBMemoryTarget inTarget) => 
            ((inTarget != null) ? (this.AiTargetBase.IsMemoryTargetValid(inTarget) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE) : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("PlayAnimation", ReturnsRunning=false)]
        protected MyBehaviorTreeState PlayAnimation([BTParam] string animationName, [BTParam] bool immediate)
        {
            if (!this.Bot.Player.Character.HasAnimation(animationName))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            this.m_animationName = animationName;
            this.Bot.Player.Character.PlayCharacterAnimation(animationName, immediate ? MyBlendOption.Immediate : MyBlendOption.WaitForPreviousEnd, MyFrameOption.PlayOnce, 0f, 1f, false, null, false);
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("AimAtTarget", MyBehaviorTreeActionType.POST)]
        protected void Post_AimAtTarget()
        {
            this.Post_AimAtTargetCustom();
        }

        [MyBehaviorTreeAction("AimAtTargetCustom", MyBehaviorTreeActionType.POST)]
        protected void Post_AimAtTargetCustom()
        {
            this.Bot.Navigation.StopAiming();
        }

        [MyBehaviorTreeAction("GotoAndAimTarget", MyBehaviorTreeActionType.POST)]
        protected void Post_GotoAndAimTarget()
        {
            this.Bot.Navigation.StopImmediate(true);
            this.Bot.Navigation.StopAiming();
        }

        [MyBehaviorTreeAction("GotoRandomLocation", MyBehaviorTreeActionType.POST)]
        protected void Post_GotoRandomLocation()
        {
            this.Post_GotoTarget();
        }

        [MyBehaviorTreeAction("GotoTarget", MyBehaviorTreeActionType.POST)]
        protected void Post_GotoTarget()
        {
            this.Bot.Navigation.StopImmediate(true);
        }

        [MyBehaviorTreeAction("ResetGotoFailed", ReturnsRunning=false)]
        protected MyBehaviorTreeState ResetGotoFailed()
        {
            this.AiTargetBase.HasGotoFailed = false;
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("SetAndAimTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState SetAndAimTarget([BTIn] ref MyBBMemoryTarget inTarget) => 
            this.SetTarget(true, ref inTarget);

        [MyBehaviorTreeAction("SetTarget", ReturnsRunning=false)]
        protected MyBehaviorTreeState SetTarget([BTIn] ref MyBBMemoryTarget inTarget)
        {
            if (inTarget != null)
            {
                return (!this.AiTargetBase.SetTargetFromMemory(inTarget) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);
            }
            this.AiTargetBase.UnsetTarget();
            return MyBehaviorTreeState.SUCCESS;
        }

        protected MyBehaviorTreeState SetTarget(bool aim, ref MyBBMemoryTarget inTarget)
        {
            if (inTarget == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            if (!this.AiTargetBase.SetTargetFromMemory(inTarget))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            if (aim)
            {
                this.AiTargetBase.AimAtTarget();
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("Stand", ReturnsRunning=false)]
        protected MyBehaviorTreeState Stand()
        {
            this.Bot.AgentEntity.Stand();
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("StopAiming", ReturnsRunning=false)]
        protected MyBehaviorTreeState StopAiming()
        {
            this.Bot.Navigation.StopAiming();
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("SwitchToRun", ReturnsRunning=false)]
        protected MyBehaviorTreeState SwitchToRun()
        {
            if (this.Bot.AgentEntity.WantsWalk)
            {
                this.Bot.AgentEntity.SwitchWalk();
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("SwitchToWalk", ReturnsRunning=false)]
        protected MyBehaviorTreeState SwitchToWalk()
        {
            if (!this.Bot.AgentEntity.WantsWalk)
            {
                this.Bot.AgentEntity.SwitchWalk();
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        protected MyAgentBot Bot { get; private set; }

        public MyAiTargetBase AiTargetBase =>
            this.Bot.AgentLogic.AiTarget;
    }
}

