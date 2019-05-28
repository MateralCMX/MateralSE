namespace Sandbox.Game.AI
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI.BehaviorTree;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyBotCollection
    {
        private Dictionary<int, IMyBot> m_allBots;
        private Dictionary<Type, ActionCollection> m_botActions;
        private MyBehaviorTreeCollection m_behaviorTreeCollection;
        private Dictionary<string, int> m_botsCountPerBehavior;
        private List<int> m_botsQueue;
        private int m_botIndex = -1;

        public MyBotCollection(MyBehaviorTreeCollection behaviorTreeCollection)
        {
            this.m_behaviorTreeCollection = behaviorTreeCollection;
            this.m_allBots = new Dictionary<int, IMyBot>(8);
            this.m_botActions = new Dictionary<Type, ActionCollection>(8);
            this.m_botsQueue = new List<int>(8);
            this.m_botsCountPerBehavior = new Dictionary<string, int>();
        }

        public void AddBot(int botHandler, IMyBot newBot)
        {
            if (!this.m_allBots.ContainsKey(botHandler))
            {
                ActionCollection actionCollection = null;
                if (this.m_botActions.ContainsKey(newBot.BotActions.GetType()))
                {
                    actionCollection = this.m_botActions[newBot.GetType()];
                }
                else
                {
                    actionCollection = ActionCollection.CreateActionCollection(newBot);
                    this.m_botActions[newBot.GetType()] = actionCollection;
                }
                newBot.InitActions(actionCollection);
                if (string.IsNullOrEmpty(newBot.BehaviorSubtypeName))
                {
                    this.m_behaviorTreeCollection.AssignBotToBehaviorTree(newBot.BotDefinition.BotBehaviorTree.SubtypeName, newBot);
                }
                else
                {
                    this.m_behaviorTreeCollection.AssignBotToBehaviorTree(newBot.BehaviorSubtypeName, newBot);
                }
                this.m_allBots.Add(botHandler, newBot);
                this.m_botsQueue.Add(botHandler);
                if (!this.m_botsCountPerBehavior.ContainsKey(newBot.BotDefinition.BehaviorType))
                {
                    this.m_botsCountPerBehavior[newBot.BotDefinition.BehaviorType] = 1;
                }
                else
                {
                    string behaviorType = newBot.BotDefinition.BehaviorType;
                    this.m_botsCountPerBehavior[behaviorType] += 1;
                }
            }
        }

        public void CheckCompatibilityWithBots(MyBehaviorTree behaviorTree)
        {
            foreach (IMyBot bot in this.m_allBots.Values)
            {
                if (behaviorTree.BehaviorTreeName.CompareTo(bot.BehaviorSubtypeName) == 0)
                {
                    if (!behaviorTree.IsCompatibleWithBot(bot.ActionCollection))
                    {
                        this.m_behaviorTreeCollection.UnassignBotBehaviorTree(bot);
                        continue;
                    }
                    bot.BotMemory.ResetMemory(false);
                }
            }
        }

        internal void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                foreach (KeyValuePair<int, IMyBot> pair in this.m_allBots)
                {
                    pair.Value.DebugDraw();
                }
            }
        }

        internal unsafe void DebugDrawBots()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                Vector2 normalizedCoord = new Vector2(0.01f, 0.4f);
                for (int i = 0; i < this.m_botsQueue.Count; i++)
                {
                    IMyBot bot = this.m_allBots[this.m_botsQueue[i]];
                    if (bot is IMyEntityBot)
                    {
                        IMyEntityBot bot2 = bot as IMyEntityBot;
                        Color green = Color.Green;
                        if ((this.m_botIndex == -1) || (i != this.m_botIndex))
                        {
                            green = Color.Red;
                        }
                        string text = $"Bot[{i}]: {bot2.BehaviorSubtypeName}";
                        if (bot is MyAgentBot)
                        {
                            text = text + (bot as MyAgentBot).LastActions.GetLastActionsString();
                        }
                        MyRenderProxy.DebugDrawText2D(MyGuiManager.GetHudPixelCoordFromNormalizedCoord(normalizedCoord), text, green, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false);
                        MyCharacter botEntity = bot2.BotEntity as MyCharacter;
                        IMyFaction faction = null;
                        if (botEntity != null)
                        {
                            long identityId = botEntity.ControllerInfo.Controller.Player.Identity.IdentityId;
                            faction = MySession.Static.Factions.TryGetPlayerFaction(identityId);
                        }
                        if (bot2.BotEntity != null)
                        {
                            Vector3D center = bot2.BotEntity.PositionComp.WorldAABB.Center;
                            double* numPtr1 = (double*) ref center.Y;
                            numPtr1[0] += bot2.BotEntity.PositionComp.WorldAABB.HalfExtents.Y;
                            MyRenderProxy.DebugDrawText3D(center, $"Bot:{i}", green, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, false);
                            MyRenderProxy.DebugDrawText3D(center - new Vector3(0f, -0.5f, 0f), (faction == null) ? "NO_FACTION" : faction.Tag, green, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, false);
                        }
                        float* singlePtr1 = (float*) ref normalizedCoord.Y;
                        singlePtr1[0] += 0.02f;
                    }
                }
            }
        }

        internal void DebugSelectNextBot()
        {
            this.m_botIndex++;
            if (this.m_botIndex == this.m_botsQueue.Count)
            {
                this.m_botIndex = (this.m_botsQueue.Count != 0) ? 0 : -1;
            }
            this.SelectBotForDebugging(this.m_botIndex);
        }

        internal void DebugSelectPreviousBot()
        {
            this.m_botIndex--;
            if (this.m_botIndex < 0)
            {
                this.m_botIndex = (this.m_botsQueue.Count <= 0) ? -1 : (this.m_botsQueue.Count - 1);
            }
            this.SelectBotForDebugging(this.m_botIndex);
        }

        public DictionaryReader<int, IMyBot> GetAllBots() => 
            new DictionaryReader<int, IMyBot>(this.m_allBots);

        public void GetBotsData(List<MyObjectBuilder_AIComponent.BotData> botDataList)
        {
            foreach (KeyValuePair<int, IMyBot> pair in this.m_allBots)
            {
                MyObjectBuilder_AIComponent.BotData item = new MyObjectBuilder_AIComponent.BotData {
                    BotBrain = pair.Value.GetObjectBuilder(),
                    PlayerHandle = pair.Key
                };
                botDataList.Add(item);
            }
        }

        public BotType GetBotType(int botHandler)
        {
            if (this.m_allBots.ContainsKey(botHandler))
            {
                MyBotLogic botLogic = this.m_allBots[botHandler].BotLogic;
                if (botLogic != null)
                {
                    return botLogic.BotType;
                }
            }
            return BotType.UNKNOWN;
        }

        public int GetCreatedBotCount()
        {
            int num = 0;
            using (Dictionary<int, IMyBot>.ValueCollection.Enumerator enumerator = this.m_allBots.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.CreatedByPlayer)
                    {
                        continue;
                    }
                    num++;
                }
            }
            return num;
        }

        public int GetCurrentBotsCount(string behaviorType) => 
            (this.m_botsCountPerBehavior.ContainsKey(behaviorType) ? this.m_botsCountPerBehavior[behaviorType] : 0);

        public int GetGeneratedBotCount()
        {
            int num = 0;
            using (Dictionary<int, IMyBot>.ValueCollection.Enumerator enumerator = this.m_allBots.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.CreatedByPlayer)
                    {
                        continue;
                    }
                    num++;
                }
            }
            return num;
        }

        public int GetHandleToFirstBot() => 
            ((this.m_botsQueue.Count <= 0) ? -1 : this.m_botsQueue[0]);

        public int GetHandleToFirstBot(string behaviorType)
        {
            using (List<int>.Enumerator enumerator = this.m_botsQueue.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    int current = enumerator.Current;
                    if (this.m_allBots[current].BotDefinition.BehaviorType == behaviorType)
                    {
                        return current;
                    }
                }
            }
            return -1;
        }

        public bool IsBotSelectedForDegugging(IMyBot bot) => 
            ReferenceEquals(this.m_behaviorTreeCollection.DebugBot, bot);

        public void ResetBots(string treeName)
        {
            foreach (IMyBot bot in this.m_allBots.Values)
            {
                if (bot.BehaviorSubtypeName == treeName)
                {
                    bot.Reset();
                }
            }
        }

        internal void SelectBotForDebugging(IMyBot bot)
        {
            this.m_behaviorTreeCollection.DebugBot = bot;
            for (int i = 0; i < this.m_botsQueue.Count; i++)
            {
                if (this.m_allBots[this.m_botsQueue[i]] == bot)
                {
                    this.m_botIndex = i;
                    return;
                }
            }
        }

        internal void SelectBotForDebugging(int index)
        {
            if (this.m_botIndex != -1)
            {
                int num = this.m_botsQueue[index];
                this.m_behaviorTreeCollection.DebugBot = this.m_allBots[num];
            }
        }

        public BotType TryGetBot<BotType>(int botHandler) where BotType: class, IMyBot
        {
            IMyBot bot = null;
            this.m_allBots.TryGetValue(botHandler, out bot);
            if (bot != null)
            {
                return (bot as BotType);
            }
            return default(BotType);
        }

        public void TryRemoveBot(int botHandler)
        {
            IMyBot bot = null;
            this.m_allBots.TryGetValue(botHandler, out bot);
            if (bot != null)
            {
                string behaviorType = bot.BotDefinition.BehaviorType;
                bot.Cleanup();
                if (this.m_botIndex != -1)
                {
                    if (ReferenceEquals(this.m_behaviorTreeCollection.DebugBot, bot))
                    {
                        this.m_behaviorTreeCollection.DebugBot = null;
                    }
                    int index = this.m_botsQueue.IndexOf(botHandler);
                    if (index < this.m_botIndex)
                    {
                        this.m_botIndex--;
                    }
                    else if (index == this.m_botIndex)
                    {
                        this.m_botIndex = -1;
                    }
                }
                this.m_allBots.Remove(botHandler);
                this.m_botsQueue.Remove(botHandler);
                string str2 = behaviorType;
                this.m_botsCountPerBehavior[str2] -= 1;
            }
        }

        public void UnloadData()
        {
            foreach (KeyValuePair<int, IMyBot> pair in this.m_allBots)
            {
                pair.Value.Cleanup();
            }
        }

        public void Update()
        {
            foreach (KeyValuePair<int, IMyBot> pair in this.m_allBots)
            {
                pair.Value.Update();
            }
        }

        public bool HasBot =>
            (this.m_botsQueue.Count > 0);

        public int TotalBotCount { get; set; }

        public Dictionary<int, IMyBot> BotsDictionary =>
            this.m_allBots;
    }
}

