namespace Sandbox.Game.AI.Actions
{
    using Sandbox;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;

    public abstract class MyHumanoidBotActions : MyAgentActions
    {
        private MyTimeSpan m_reservationTimeOut;
        private const int RESERVATION_WAIT_TIMEOUT_SECONDS = 3;

        public MyHumanoidBotActions(MyHumanoidBot humanoidBot) : base(humanoidBot)
        {
        }

        private void AreaReservationHandler(ref MyAiTargetManager.ReservedAreaData reservedArea, bool success)
        {
            if (((this.Bot != null) && ((this.Bot.HumanoidLogic != null) && (this.Bot.Player != null))) && (this.Bot.Player.Id.SerialId == reservedArea.ReserverId.SerialId))
            {
                MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
                humanoidLogic.ReservationStatus = MyReservationStatus.FAILURE;
                if (success && ((reservedArea.WorldPosition == humanoidLogic.ReservationAreaData.WorldPosition) && (reservedArea.Radius == humanoidLogic.ReservationAreaData.Radius)))
                {
                    humanoidLogic.ReservationStatus = MyReservationStatus.SUCCESS;
                }
            }
        }

        [MyBehaviorTreeAction("EquipItem", ReturnsRunning=false)]
        protected MyBehaviorTreeState EquipItem([BTParam] string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                return MyBehaviorTreeState.FAILURE;
            }
            MyCharacter humanoidEntity = this.Bot.HumanoidEntity;
            if ((humanoidEntity.CurrentWeapon == null) || (humanoidEntity.CurrentWeapon.DefinitionId.SubtypeName != itemName))
            {
                MyObjectBuilder_PhysicalGunObject self = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalGunObject>(itemName);
                MyDefinitionId id = self.GetId();
                if (!humanoidEntity.GetInventory(0).ContainItems(1, self) && humanoidEntity.WeaponTakesBuilderFromInventory(new MyDefinitionId?(id)))
                {
                    return MyBehaviorTreeState.FAILURE;
                }
                humanoidEntity.SwitchToWeapon(id);
            }
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("IsInReservedArea", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsInReservedArea([BTParam] string areaName) => 
            (!MyAiTargetManager.Static.IsInReservedArea(areaName, this.Bot.HumanoidEntity.WorldMatrix.Translation) ? MyBehaviorTreeState.FAILURE : MyBehaviorTreeState.SUCCESS);

        [MyBehaviorTreeAction("IsNotInReservedArea", ReturnsRunning=false)]
        protected MyBehaviorTreeState IsNotInReservedArea([BTParam] string areaName) => 
            (!MyAiTargetManager.Static.IsInReservedArea(areaName, this.Bot.HumanoidEntity.WorldMatrix.Translation) ? MyBehaviorTreeState.SUCCESS : MyBehaviorTreeState.FAILURE);

        [MyBehaviorTreeAction("PlaySound", ReturnsRunning=false)]
        protected MyBehaviorTreeState PlaySound([BTParam] string soundName)
        {
            this.Bot.HumanoidEntity.SoundComp.StartSecondarySound(soundName, true);
            return MyBehaviorTreeState.SUCCESS;
        }

        [MyBehaviorTreeAction("TryReserveArea", MyBehaviorTreeActionType.POST)]
        protected void Post_TryReserveArea()
        {
            if (this.Bot.HumanoidLogic != null)
            {
                MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
                if (humanoidLogic.ReservationStatus != MyReservationStatus.NONE)
                {
                    MyAiTargetManager.OnAreaReservationResult -= new Sandbox.Game.AI.MyAiTargetManager.AreaReservationHandler(this.AreaReservationHandler);
                }
                humanoidLogic.ReservationStatus = MyReservationStatus.NONE;
            }
        }

        [MyBehaviorTreeAction("TryReserveEntity", MyBehaviorTreeActionType.POST)]
        protected void Post_TryReserveEntity()
        {
            if ((this.Bot != null) && (this.Bot.HumanoidLogic != null))
            {
                MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
                if (humanoidLogic.ReservationStatus != MyReservationStatus.NONE)
                {
                    MyAiTargetManager.OnReservationResult -= new Sandbox.Game.AI.MyAiTargetManager.ReservationHandler(this.ReservationHandler);
                }
                humanoidLogic.ReservationStatus = MyReservationStatus.NONE;
            }
        }

        private void ReservationHandler(ref MyAiTargetManager.ReservedEntityData reservedEntity, bool success)
        {
            if (((this.Bot != null) && ((this.Bot.HumanoidLogic != null) && (this.Bot.Player != null))) && (this.Bot.Player.Id.SerialId == reservedEntity.ReserverId.SerialId))
            {
                MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
                humanoidLogic.ReservationStatus = MyReservationStatus.FAILURE;
                if (((success && (reservedEntity.EntityId == humanoidLogic.ReservationEntityData.EntityId)) && ((reservedEntity.Type != MyReservedEntityType.ENVIRONMENT_ITEM) || (reservedEntity.LocalId == humanoidLogic.ReservationEntityData.LocalId))) && ((reservedEntity.Type != MyReservedEntityType.VOXEL) || (reservedEntity.GridPos == humanoidLogic.ReservationEntityData.GridPos)))
                {
                    humanoidLogic.ReservationStatus = MyReservationStatus.SUCCESS;
                }
            }
        }

        [MyBehaviorTreeAction("TryReserveArea")]
        protected MyBehaviorTreeState TryReserveAreaAroundEntity([BTParam] string areaName, [BTParam] float radius, [BTParam] int timeMs)
        {
            MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
            MyBehaviorTreeState fAILURE = MyBehaviorTreeState.FAILURE;
            if (humanoidLogic != null)
            {
                switch (humanoidLogic.ReservationStatus)
                {
                    case MyReservationStatus.NONE:
                    {
                        humanoidLogic.ReservationStatus = MyReservationStatus.WAITING;
                        MyAiTargetManager.ReservedAreaData data = new MyAiTargetManager.ReservedAreaData {
                            WorldPosition = this.Bot.HumanoidEntity.WorldMatrix.Translation,
                            Radius = radius,
                            ReservationTimer = MyTimeSpan.FromMilliseconds((double) timeMs),
                            ReserverId = new MyPlayer.PlayerId(this.Bot.Player.Id.SteamId, this.Bot.Player.Id.SerialId)
                        };
                        humanoidLogic.ReservationAreaData = data;
                        MyAiTargetManager.OnAreaReservationResult += new Sandbox.Game.AI.MyAiTargetManager.AreaReservationHandler(this.AreaReservationHandler);
                        MyAiTargetManager.Static.RequestAreaReservation(areaName, this.Bot.HumanoidEntity.WorldMatrix.Translation, radius, (long) timeMs, this.Bot.Player.Id.SerialId);
                        this.m_reservationTimeOut = MySandboxGame.Static.TotalTime + MyTimeSpan.FromSeconds(3.0);
                        humanoidLogic.ReservationStatus = MyReservationStatus.WAITING;
                        fAILURE = MyBehaviorTreeState.RUNNING;
                        break;
                    }
                    case MyReservationStatus.WAITING:
                        fAILURE = (this.m_reservationTimeOut >= MySandboxGame.Static.TotalTime) ? MyBehaviorTreeState.RUNNING : MyBehaviorTreeState.FAILURE;
                        break;

                    case MyReservationStatus.SUCCESS:
                        fAILURE = MyBehaviorTreeState.SUCCESS;
                        break;

                    case MyReservationStatus.FAILURE:
                        fAILURE = MyBehaviorTreeState.FAILURE;
                        break;

                    default:
                        break;
                }
            }
            return fAILURE;
        }

        [MyBehaviorTreeAction("TryReserveEntity")]
        protected MyBehaviorTreeState TryReserveEntity([BTIn] ref MyBBMemoryTarget inTarget, [BTParam] int timeMs)
        {
            if ((this.Bot != null) && (this.Bot.Player != null))
            {
                MyHumanoidBotLogic humanoidLogic = this.Bot.HumanoidLogic;
                if (((inTarget != null) && ((inTarget.EntityId != null) && (inTarget.TargetType != MyAiTargetEnum.POSITION))) && (inTarget.TargetType != MyAiTargetEnum.NO_TARGET))
                {
                    switch (humanoidLogic.ReservationStatus)
                    {
                        case MyReservationStatus.NONE:
                            MyAiTargetManager.ReservedEntityData data;
                            switch (inTarget.TargetType)
                            {
                                case MyAiTargetEnum.GRID:
                                case MyAiTargetEnum.CUBE:
                                case MyAiTargetEnum.CHARACTER:
                                case MyAiTargetEnum.ENTITY:
                                    humanoidLogic.ReservationStatus = MyReservationStatus.WAITING;
                                    data = new MyAiTargetManager.ReservedEntityData {
                                        Type = MyReservedEntityType.ENTITY,
                                        EntityId = inTarget.EntityId.Value,
                                        ReservationTimer = timeMs,
                                        ReserverId = new MyPlayer.PlayerId(this.Bot.Player.Id.SteamId, this.Bot.Player.Id.SerialId)
                                    };
                                    humanoidLogic.ReservationEntityData = data;
                                    MyAiTargetManager.OnReservationResult += new Sandbox.Game.AI.MyAiTargetManager.ReservationHandler(this.ReservationHandler);
                                    MyAiTargetManager.Static.RequestEntityReservation(humanoidLogic.ReservationEntityData.EntityId, humanoidLogic.ReservationEntityData.ReservationTimer, this.Bot.Player.Id.SerialId);
                                    break;

                                case MyAiTargetEnum.ENVIRONMENT_ITEM:
                                    humanoidLogic.ReservationStatus = MyReservationStatus.WAITING;
                                    data = new MyAiTargetManager.ReservedEntityData {
                                        Type = MyReservedEntityType.ENVIRONMENT_ITEM,
                                        EntityId = inTarget.EntityId.Value,
                                        LocalId = inTarget.TreeId.Value,
                                        ReservationTimer = timeMs,
                                        ReserverId = new MyPlayer.PlayerId(this.Bot.Player.Id.SteamId, this.Bot.Player.Id.SerialId)
                                    };
                                    humanoidLogic.ReservationEntityData = data;
                                    MyAiTargetManager.OnReservationResult += new Sandbox.Game.AI.MyAiTargetManager.ReservationHandler(this.ReservationHandler);
                                    MyAiTargetManager.Static.RequestEnvironmentItemReservation(humanoidLogic.ReservationEntityData.EntityId, humanoidLogic.ReservationEntityData.LocalId, humanoidLogic.ReservationEntityData.ReservationTimer, this.Bot.Player.Id.SerialId);
                                    break;

                                case MyAiTargetEnum.VOXEL:
                                    humanoidLogic.ReservationStatus = MyReservationStatus.WAITING;
                                    data = new MyAiTargetManager.ReservedEntityData {
                                        Type = MyReservedEntityType.VOXEL,
                                        EntityId = inTarget.EntityId.Value,
                                        GridPos = inTarget.VoxelPosition,
                                        ReservationTimer = timeMs,
                                        ReserverId = new MyPlayer.PlayerId(this.Bot.Player.Id.SteamId, this.Bot.Player.Id.SerialId)
                                    };
                                    humanoidLogic.ReservationEntityData = data;
                                    MyAiTargetManager.OnReservationResult += new Sandbox.Game.AI.MyAiTargetManager.ReservationHandler(this.ReservationHandler);
                                    MyAiTargetManager.Static.RequestVoxelPositionReservation(humanoidLogic.ReservationEntityData.EntityId, humanoidLogic.ReservationEntityData.GridPos, humanoidLogic.ReservationEntityData.ReservationTimer, this.Bot.Player.Id.SerialId);
                                    break;

                                default:
                                    humanoidLogic.ReservationStatus = MyReservationStatus.FAILURE;
                                    break;
                            }
                            this.m_reservationTimeOut = MySandboxGame.Static.TotalTime + MyTimeSpan.FromSeconds(3.0);
                            break;

                        case MyReservationStatus.WAITING:
                            if (this.m_reservationTimeOut < MySandboxGame.Static.TotalTime)
                            {
                                humanoidLogic.ReservationStatus = MyReservationStatus.FAILURE;
                            }
                            break;

                        default:
                            break;
                    }
                }
                switch (humanoidLogic.ReservationStatus)
                {
                    case MyReservationStatus.WAITING:
                        return MyBehaviorTreeState.RUNNING;

                    case MyReservationStatus.SUCCESS:
                        return MyBehaviorTreeState.SUCCESS;
                }
            }
            return MyBehaviorTreeState.FAILURE;
        }

        protected MyHumanoidBot Bot =>
            (base.Bot as MyHumanoidBot);
    }
}

