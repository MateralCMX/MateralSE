namespace Sandbox.Game.AI
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.AI.Navigation;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyBotType(typeof(MyObjectBuilder_AgentBot))]
    public class MyAgentBot : IMyEntityBot, IMyBot
    {
        protected MyPlayer m_player;
        protected MyBotNavigation m_navigation;
        protected Sandbox.Game.AI.ActionCollection m_actionCollection;
        protected MyBotMemory m_botMemory;
        private MyBotMemory m_LastBotMemory;
        protected MyAgentActions m_actions;
        protected MyAgentDefinition m_botDefinition;
        protected MyAgentLogic m_botLogic;
        private int m_deathCountdownMs;
        private int m_lastCountdownTime;
        private bool m_respawnRequestSent;
        private bool m_removeAfterDeath;
        private bool m_botRemoved;
        private bool m_joinRequestSent;
        public MyLastActions LastActions = new MyLastActions();

        public MyAgentBot(MyPlayer player, MyBotDefinition botDefinition)
        {
            this.m_player = player;
            this.m_navigation = new MyBotNavigation();
            this.m_actionCollection = null;
            this.m_botMemory = new MyBotMemory(this);
            this.m_botDefinition = botDefinition as MyAgentDefinition;
            this.m_removeAfterDeath = this.m_botDefinition.RemoveAfterDeath;
            this.m_respawnRequestSent = false;
            this.m_botRemoved = false;
            this.m_player.Controller.ControlledEntityChanged += new Action<IMyControllableEntity, IMyControllableEntity>(this.Controller_ControlledEntityChanged);
            this.m_navigation.ChangeEntity(this.m_player.Controller.ControlledEntity);
            MyCestmirDebugInputComponent.PlacedAction += new Action<Vector3D, MyEntity>(this.DebugGoto);
        }

        public virtual void Cleanup()
        {
            MyCestmirDebugInputComponent.PlacedAction -= new Action<Vector3D, MyEntity>(this.DebugGoto);
            this.m_navigation.Cleanup();
            if (this.HasLogic)
            {
                this.m_botLogic.Cleanup();
            }
            this.m_player.Controller.ControlledEntityChanged -= new Action<IMyControllableEntity, IMyControllableEntity>(this.Controller_ControlledEntityChanged);
            this.m_player = null;
        }

        protected virtual void Controller_ControlledEntityChanged(IMyControllableEntity oldEntity, IMyControllableEntity newEntity)
        {
            if ((oldEntity == null) && (newEntity is MyCharacter))
            {
                this.EraseRespawn();
            }
            this.m_navigation.ChangeEntity(newEntity);
            this.m_navigation.AimWithMovement();
            MyCharacter character = newEntity as MyCharacter;
            if (character != null)
            {
                IMyControllableEntity controlledEntity = this.m_player.Controller.ControlledEntity;
                MyCharacterJetpackComponent jetpackComp = character.JetpackComp;
                if (jetpackComp != null)
                {
                    jetpackComp.TurnOnJetpack(false, false, false);
                }
            }
            if (this.HasLogic)
            {
                this.m_botLogic.OnControlledEntityChanged(newEntity);
            }
        }

        public virtual unsafe void DebugDraw()
        {
            if (this.AgentEntity != null)
            {
                MyAiTargetBase aiTargetBase = this.m_actions.AiTargetBase;
                if ((aiTargetBase != null) && aiTargetBase.HasTarget())
                {
                    MyRenderProxy.DebugDrawPoint(aiTargetBase.TargetPosition, Color.Aquamarine, false, false);
                    if ((this.BotEntity != null) && (aiTargetBase.TargetEntity != null))
                    {
                        string text = null;
                        text = (aiTargetBase.TargetType != MyAiTargetEnum.CUBE) ? $"Target:{aiTargetBase.TargetEntity.ToString()}" : $"Target:{aiTargetBase.GetTargetBlock()}";
                        Vector3D center = this.BotEntity.PositionComp.WorldAABB.Center;
                        double* numPtr1 = (double*) ref center.Y;
                        numPtr1[0] += this.BotEntity.PositionComp.WorldAABB.HalfExtents.Y + 0.20000000298023224;
                        MyRenderProxy.DebugDrawText3D(center, text, Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, false);
                    }
                }
                this.m_botLogic.DebugDraw();
            }
        }

        public virtual void DebugGoto(Vector3D point, MyEntity entity = null)
        {
            if (this.m_player.Id.SerialId != 0)
            {
                this.m_navigation.AimWithMovement();
                this.m_navigation.GotoNoPath(point, 0f, entity, true);
            }
        }

        private void EraseRespawn()
        {
            this.m_deathCountdownMs = 0;
            this.m_respawnRequestSent = false;
        }

        public virtual MyObjectBuilder_Bot GetObjectBuilder()
        {
            MyObjectBuilder_AgentBot botObjectBuilder = MyAIComponent.BotFactory.GetBotObjectBuilder(this) as MyObjectBuilder_AgentBot;
            botObjectBuilder.BotDefId = (SerializableDefinitionId) this.BotDefinition.Id;
            botObjectBuilder.AiTarget = this.AgentActions.AiTargetBase.GetObjectBuilder();
            botObjectBuilder.BotMemory = this.m_botMemory.GetObjectBuilder();
            botObjectBuilder.LastBehaviorTree = this.BehaviorSubtypeName;
            botObjectBuilder.RemoveAfterDeath = this.m_removeAfterDeath;
            botObjectBuilder.RespawnCounter = this.m_deathCountdownMs;
            return botObjectBuilder;
        }

        private void HandleDeadBot()
        {
            if (this.m_deathCountdownMs > 0)
            {
                int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_deathCountdownMs -= totalGamePlayTimeInMilliseconds - this.m_lastCountdownTime;
                this.m_lastCountdownTime = totalGamePlayTimeInMilliseconds;
            }
            else
            {
                Vector3D zero = Vector3D.Zero;
                if (!this.m_removeAfterDeath && MyAIComponent.BotFactory.GetBotSpawnPosition(this.BotDefinition.BehaviorType, out zero))
                {
                    MyPlayerCollection.OnRespawnRequest(false, false, 0L, null, new Vector3D?(zero), new SerializableDefinitionId?((SerializableDefinitionId) this.BotDefinition.Id), false, this.Player.Id.SerialId, null, Color.Red);
                    this.m_respawnRequestSent = true;
                }
                else if (!this.m_botRemoved)
                {
                    this.m_botRemoved = true;
                    MyAIComponent.Static.RemoveBot(this.Player.Id.SerialId, false);
                }
            }
        }

        public virtual void Init(MyObjectBuilder_Bot botBuilder)
        {
            MyObjectBuilder_AgentBot bot = botBuilder as MyObjectBuilder_AgentBot;
            if (bot != null)
            {
                this.m_deathCountdownMs = bot.RespawnCounter;
                if (this.AgentDefinition.FactionTag != null)
                {
                    MyFaction faction = MySession.Static.Factions.TryGetOrCreateFactionByTag(this.AgentDefinition.FactionTag);
                    if (faction != null)
                    {
                        MyFactionCollection.SendJoinRequest(faction.FactionId, this.Player.Identity.IdentityId);
                        this.m_joinRequestSent = true;
                    }
                }
                if (bot.AiTarget != null)
                {
                    this.AgentActions.AiTargetBase.Init(bot.AiTarget);
                }
                if (botBuilder.BotMemory != null)
                {
                    this.m_botMemory.Init(botBuilder.BotMemory);
                }
                MyAIComponent.Static.BehaviorTrees.SetBehaviorName(this, bot.LastBehaviorTree);
            }
        }

        public virtual void InitActions(Sandbox.Game.AI.ActionCollection actionCollection)
        {
            this.m_actionCollection = actionCollection;
        }

        public virtual void InitLogic(MyBotLogic botLogic)
        {
            this.m_botLogic = botLogic as MyAgentLogic;
            if (this.HasLogic)
            {
                this.m_botLogic.Init();
                if (this.AgentEntity != null)
                {
                    this.AgentLogic.OnCharacterControlAcquired(this.AgentEntity);
                }
            }
        }

        public virtual void Reset()
        {
            this.BotMemory.ResetMemory(true);
            this.m_navigation.StopImmediate(true);
            this.AgentActions.AiTargetBase.UnsetTarget();
        }

        public void ReturnToLastMemory()
        {
            if (this.m_LastBotMemory != null)
            {
                this.m_botMemory = this.m_LastBotMemory;
            }
        }

        public virtual void Spawn(Vector3D? spawnPosition, bool spawnedByPlayer)
        {
            this.CreatedByPlayer = spawnedByPlayer;
            MyCharacter controlledEntity = this.m_player.Controller.ControlledEntity as MyCharacter;
            if ((((controlledEntity != null) && controlledEntity.IsDead) || this.m_player.Identity.IsDead) && !this.m_respawnRequestSent)
            {
                this.m_respawnRequestSent = true;
                MyPlayerCollection.OnRespawnRequest(false, false, 0L, null, spawnPosition, new SerializableDefinitionId?((SerializableDefinitionId) this.BotDefinition.Id), false, this.m_player.Id.SerialId, null, Color.Red);
            }
        }

        private void StartRespawn()
        {
            this.m_lastCountdownTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.m_removeAfterDeath)
            {
                this.m_deathCountdownMs = this.AgentDefinition.RemoveTimeMs;
            }
            else
            {
                this.m_deathCountdownMs = this.AgentDefinition.RemoveTimeMs;
            }
        }

        public void Update()
        {
            if (this.m_player.Controller.ControlledEntity == null)
            {
                if (!this.m_respawnRequestSent)
                {
                    this.HandleDeadBot();
                }
            }
            else if (((this.AgentEntity != null) && this.AgentEntity.IsDead) && !this.m_respawnRequestSent)
            {
                this.HandleDeadBot();
            }
            else
            {
                if (((this.AgentEntity != null) && !this.AgentEntity.IsDead) && this.m_respawnRequestSent)
                {
                    this.EraseRespawn();
                }
                this.UpdateInternal();
            }
        }

        protected virtual void UpdateInternal()
        {
            this.m_navigation.Update(this.m_botMemory.TickCounter);
            this.m_botLogic.Update();
            if ((!this.m_joinRequestSent && (this.m_botDefinition.FactionTag != null)) && (this.m_botDefinition.FactionTag.Length > 0))
            {
                string tag = this.m_botDefinition.FactionTag.ToUpperInvariant();
                MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(tag, null);
                if (faction != null)
                {
                    long controllingIdentityId = this.AgentEntity.ControllerInfo.ControllingIdentityId;
                    if ((MySession.Static.Factions.TryGetPlayerFaction(controllingIdentityId) == null) && !this.m_joinRequestSent)
                    {
                        MyFactionCollection.SendJoinRequest(faction.FactionId, controllingIdentityId);
                        this.m_joinRequestSent = true;
                    }
                }
            }
        }

        public MyPlayer Player =>
            this.m_player;

        public MyBotNavigation Navigation =>
            this.m_navigation;

        public MyCharacter AgentEntity =>
            ((this.m_player.Controller.ControlledEntity == null) ? null : (this.m_player.Controller.ControlledEntity as MyCharacter));

        public MyEntity BotEntity =>
            this.AgentEntity;

        public string BehaviorSubtypeName =>
            MyAIComponent.Static.BehaviorTrees.GetBehaviorName(this);

        public Sandbox.Game.AI.ActionCollection ActionCollection =>
            this.m_actionCollection;

        public MyBotMemory BotMemory =>
            this.m_botMemory;

        public MyBotMemory LastBotMemory
        {
            get => 
                this.m_LastBotMemory;
            set => 
                (this.m_LastBotMemory = value);
        }

        public MyAgentActions AgentActions =>
            this.m_actions;

        public MyBotActionsBase BotActions
        {
            get => 
                this.m_actions;
            set => 
                (this.m_actions = value as MyAgentActions);
        }

        public MyBotDefinition BotDefinition =>
            this.m_botDefinition;

        public MyAgentDefinition AgentDefinition =>
            this.m_botDefinition;

        public MyBotLogic BotLogic =>
            this.m_botLogic;

        public MyAgentLogic AgentLogic =>
            this.m_botLogic;

        public bool HasLogic =>
            (this.m_botLogic != null);

        public virtual bool ShouldFollowPlayer
        {
            get => 
                false;
            set
            {
            }
        }

        public virtual bool IsValidForUpdate =>
            ((this.m_player != null) && ((this.m_player.Controller.ControlledEntity != null) && ((this.m_player.Controller.ControlledEntity.Entity != null) && ((this.AgentEntity != null) && !this.AgentEntity.IsDead))));

        public bool CreatedByPlayer { get; set; }

        public class MyLastActions
        {
            private List<MyAgentBot.SLastRunningState> m_LastActions = new List<MyAgentBot.SLastRunningState>();
            private int MaxActionsCount = 5;

            public void AddLastAction(string lastAction)
            {
                if (this.m_LastActions.Count != 0)
                {
                    if (lastAction == this.GetLastAction())
                    {
                        MyAgentBot.SLastRunningState local1 = this.m_LastActions.Last<MyAgentBot.SLastRunningState>();
                        local1.counter++;
                        return;
                    }
                    if (this.m_LastActions.Count == this.MaxActionsCount)
                    {
                        this.m_LastActions.RemoveAt(0);
                    }
                }
                MyAgentBot.SLastRunningState item = new MyAgentBot.SLastRunningState();
                item.actionName = lastAction;
                item.counter = 1;
                this.m_LastActions.Add(item);
            }

            public void Clear()
            {
                this.m_LastActions.Clear();
            }

            private string GetLastAction() => 
                this.m_LastActions.Last<MyAgentBot.SLastRunningState>().actionName;

            public string GetLastActionsString()
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < this.m_LastActions.Count; i++)
                {
                    builder.AppendFormat("{0}-{1}", this.m_LastActions[i].counter, this.m_LastActions[i].actionName);
                    if (i != (this.m_LastActions.Count - 1))
                    {
                        builder.AppendFormat(", ", Array.Empty<object>());
                    }
                }
                return builder.ToString();
            }
        }

        public class SLastRunningState
        {
            public string actionName;
            public int counter;
        }
    }
}

