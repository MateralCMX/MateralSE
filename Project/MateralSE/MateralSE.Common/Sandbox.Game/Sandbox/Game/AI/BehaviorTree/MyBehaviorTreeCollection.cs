namespace Sandbox.Game.AI.BehaviorTree
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.SessionComponents;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Win32;

    public class MyBehaviorTreeCollection
    {
        private IntPtr m_toolWindowHandle = IntPtr.Zero;
        public const int UPDATE_COUNTER = 10;
        public const int INIT_UPDATE_COUNTER = 8;
        public static readonly string DEFAULT_EXTENSION = ".sbc";
        private Dictionary<MyStringHash, BTData> m_BTDataByName = new Dictionary<MyStringHash, BTData>(MyStringHash.Comparer);
        private Dictionary<IMyBot, MyStringHash> m_botBehaviorIds = new Dictionary<IMyBot, MyStringHash>();
        private IMyBot m_debugBot;

        public MyBehaviorTreeCollection()
        {
            this.DebugIsCurrentTreeVerified = false;
            foreach (MyBehaviorDefinition definition in MyDefinitionManager.Static.GetBehaviorDefinitions())
            {
                this.BuildBehaviorTree(definition);
            }
        }

        private void AssignBotBehaviorTreeInternal(MyBehaviorTree behaviorTree, IMyBot bot)
        {
            bot.BotMemory.AssignBehaviorTree(behaviorTree);
            this.m_BTDataByName[behaviorTree.BehaviorTreeId].BotsData.Add(new BotData(bot));
            this.m_botBehaviorIds[bot] = behaviorTree.BehaviorTreeId;
        }

        public bool AssignBotToBehaviorTree(MyBehaviorTree behaviorTree, IMyBot bot)
        {
            if (!behaviorTree.IsCompatibleWithBot(bot.ActionCollection))
            {
                return false;
            }
            this.AssignBotBehaviorTreeInternal(behaviorTree, bot);
            return true;
        }

        public bool AssignBotToBehaviorTree(string behaviorName, IMyBot bot)
        {
            MyStringHash key = MyStringHash.TryGet(behaviorName);
            return ((key != MyStringHash.NullOrEmpty) && (this.m_BTDataByName.ContainsKey(key) && this.AssignBotToBehaviorTree(this.m_BTDataByName[key].BehaviorTree, bot)));
        }

        private bool BuildBehaviorTree(MyBehaviorDefinition behaviorDefinition)
        {
            if (this.m_BTDataByName.ContainsKey(behaviorDefinition.Id.SubtypeId))
            {
                return false;
            }
            MyBehaviorTree behaviorTree = new MyBehaviorTree(behaviorDefinition);
            behaviorTree.Construct();
            BTData data = new BTData(behaviorTree);
            this.m_BTDataByName.Add(behaviorDefinition.Id.SubtypeId, data);
            return true;
        }

        public bool ChangeBehaviorTree(string behaviorTreeName, IMyBot bot)
        {
            bool flag = false;
            MyBehaviorTree behaviorTree = null;
            if (!this.TryGetBehaviorTreeByName(behaviorTreeName, out behaviorTree))
            {
                return false;
            }
            if (!behaviorTree.IsCompatibleWithBot(bot.ActionCollection))
            {
                return false;
            }
            MyBehaviorTree tree2 = this.TryGetBehaviorTreeForBot(bot);
            if (tree2 == null)
            {
                flag = true;
            }
            else if (tree2.BehaviorTreeId == behaviorTree.BehaviorTreeId)
            {
                flag = false;
            }
            else
            {
                this.UnassignBotBehaviorTree(bot);
                flag = true;
            }
            if (flag)
            {
                this.AssignBotBehaviorTreeInternal(behaviorTree, bot);
            }
            return flag;
        }

        public string GetBehaviorName(IMyBot bot)
        {
            MyStringHash hash;
            this.m_botBehaviorIds.TryGetValue(bot, out hash);
            return hash.String;
        }

        public bool HasBehavior(MyStringHash id) => 
            this.m_BTDataByName.ContainsKey(id);

        private static MyBehaviorDefinition LoadBehaviorTreeFromFile(string path)
        {
            MyObjectBuilder_Definitions objectBuilder = null;
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(path, out objectBuilder);
            if (((objectBuilder == null) || (objectBuilder.AIBehaviors == null)) || (objectBuilder.AIBehaviors.Length == 0))
            {
                return null;
            }
            MyObjectBuilder_BehaviorTreeDefinition builder = objectBuilder.AIBehaviors[0];
            MyModContext modContext = new MyModContext();
            modContext.Init("BehaviorDefinition", Path.GetFileName(path), null);
            MyBehaviorDefinition definition1 = new MyBehaviorDefinition();
            definition1.Init(builder, modContext);
            return definition1;
        }

        public static bool LoadUploadedBehaviorTree(out MyBehaviorDefinition definition)
        {
            MyBehaviorDefinition definition2 = LoadBehaviorTreeFromFile(Path.Combine(MyFileSystem.UserDataPath, "UploadTree" + DEFAULT_EXTENSION));
            definition = definition2;
            return (definition != null);
        }

        public bool RebuildBehaviorTree(MyBehaviorDefinition newDefinition, out MyBehaviorTree outBehaviorTree)
        {
            if (!this.m_BTDataByName.ContainsKey(newDefinition.Id.SubtypeId))
            {
                outBehaviorTree = null;
                return false;
            }
            outBehaviorTree = this.m_BTDataByName[newDefinition.Id.SubtypeId].BehaviorTree;
            outBehaviorTree.ReconstructTree(newDefinition);
            return true;
        }

        private void SendDataToTool(IMyBot bot, MyPerTreeBotMemory botTreeMemory)
        {
            if (!this.DebugIsCurrentTreeVerified || (this.DebugLastWindowHandle.ToInt32() != this.m_toolWindowHandle.ToInt32()))
            {
                IntPtr wParam = new IntPtr(this.m_BTDataByName[this.m_botBehaviorIds[bot]].BehaviorTree.GetHashCode());
                WinApi.PostMessage(this.m_toolWindowHandle, 0x403, wParam, IntPtr.Zero);
                this.DebugIsCurrentTreeVerified = true;
                this.DebugLastWindowHandle = new IntPtr(this.m_toolWindowHandle.ToInt32());
            }
            WinApi.PostMessage(this.m_toolWindowHandle, 0x401, IntPtr.Zero, IntPtr.Zero);
            for (int i = 0; i < botTreeMemory.NodesMemoryCount; i++)
            {
                MyBehaviorTreeState nodeState = botTreeMemory.GetNodeMemoryByIndex(i).NodeState;
                if (nodeState != MyBehaviorTreeState.NOT_TICKED)
                {
                    WinApi.PostMessage(this.m_toolWindowHandle, 0x400, new IntPtr((long) ((ulong) i)), new IntPtr((int) nodeState));
                }
            }
            WinApi.PostMessage(this.m_toolWindowHandle, 0x402, IntPtr.Zero, IntPtr.Zero);
        }

        private void SendSelectedTreeForDebug(MyBehaviorTree behaviorTree)
        {
            if (MySessionComponentExtDebug.Static != null)
            {
                this.DebugSelectedTreeHashSent = true;
                this.DebugCurrentBehaviorTree = behaviorTree.BehaviorTreeName;
                MyExternalDebugStructures.SelectedTreeMsg msg = new MyExternalDebugStructures.SelectedTreeMsg {
                    BehaviorTreeName = behaviorTree.BehaviorTreeName
                };
                MySessionComponentExtDebug.Static.SendMessageToClients<MyExternalDebugStructures.SelectedTreeMsg>(msg);
            }
        }

        public void SetBehaviorName(IMyBot bot, string behaviorName)
        {
            this.m_botBehaviorIds[bot] = MyStringHash.GetOrCompute(behaviorName);
        }

        public bool TryGetBehaviorTreeByName(string name, out MyBehaviorTree behaviorTree)
        {
            MyStringHash hash;
            MyStringHash.TryGet(name, out hash);
            if ((hash == MyStringHash.NullOrEmpty) || !this.m_BTDataByName.ContainsKey(hash))
            {
                behaviorTree = null;
                return false;
            }
            behaviorTree = this.m_BTDataByName[hash].BehaviorTree;
            return (behaviorTree != null);
        }

        public MyBehaviorTree TryGetBehaviorTreeForBot(IMyBot bot)
        {
            BTData data = null;
            this.m_BTDataByName.TryGetValue(this.m_botBehaviorIds[bot], out data);
            return ((data == null) ? null : data.BehaviorTree);
        }

        public bool TryGetValidToolWindow(out IntPtr windowHandle)
        {
            windowHandle = IntPtr.Zero;
            windowHandle = WinApi.FindWindowInParent("VRageEditor", "BehaviorTreeWindow");
            if (windowHandle == IntPtr.Zero)
            {
                windowHandle = WinApi.FindWindowInParent("Behavior tree tool", "BehaviorTreeWindow");
            }
            return (windowHandle != IntPtr.Zero);
        }

        public void UnassignBotBehaviorTree(IMyBot bot)
        {
            this.m_BTDataByName[this.m_botBehaviorIds[bot]].RemoveBot(bot);
            bot.BotMemory.UnassignCurrentBehaviorTree();
            this.m_botBehaviorIds[bot] = MyStringHash.NullOrEmpty;
        }

        public void Update()
        {
            foreach (BTData local1 in this.m_BTDataByName.Values)
            {
                MyBehaviorTree behaviorTree = local1.BehaviorTree;
                foreach (BotData data in local1.BotsData)
                {
                    IMyBot bot = data.Bot;
                    if (bot.IsValidForUpdate)
                    {
                        int num = data.UpdateCounter + 1;
                        data.UpdateCounter = num;
                        if (num > 10)
                        {
                            if (MyFakes.DEBUG_BEHAVIOR_TREE)
                            {
                                if (!MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP)
                                {
                                    continue;
                                }
                                MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP = false;
                            }
                            data.UpdateCounter = 0;
                            bot.BotMemory.PreTickClear();
                            behaviorTree.Tick(bot);
                            if ((MyFakes.ENABLE_BEHAVIOR_TREE_TOOL_COMMUNICATION && (ReferenceEquals(this.DebugBot, data.Bot) && (!this.DebugBreakDebugging && MyDebugDrawSettings.DEBUG_DRAW_BOTS))) && this.TryGetValidToolWindow(out this.m_toolWindowHandle))
                            {
                                if ((!this.DebugSelectedTreeHashSent || (this.m_toolWindowHandle != this.DebugLastWindowHandle)) || (this.DebugCurrentBehaviorTree != this.m_botBehaviorIds[this.DebugBot].String))
                                {
                                    this.SendSelectedTreeForDebug(behaviorTree);
                                }
                                this.SendDataToTool(data.Bot, data.Bot.BotMemory.CurrentTreeBotMemory);
                            }
                        }
                    }
                }
            }
        }

        public bool DebugSelectedTreeHashSent { get; private set; }

        public IntPtr DebugLastWindowHandle { get; private set; }

        public bool DebugIsCurrentTreeVerified { get; private set; }

        public IMyBot DebugBot
        {
            get => 
                this.m_debugBot;
            set
            {
                this.m_debugBot = value;
                this.DebugSelectedTreeHashSent = false;
            }
        }

        public bool DebugBreakDebugging { get; set; }

        public string DebugCurrentBehaviorTree { get; private set; }

        private class BotData
        {
            public IMyBot Bot;
            public int UpdateCounter = 8;

            public BotData(IMyBot bot)
            {
                this.Bot = bot;
            }
        }

        private class BTData : IEqualityComparer<MyBehaviorTreeCollection.BotData>
        {
            private static readonly MyBehaviorTreeCollection.BotData SearchData = new MyBehaviorTreeCollection.BotData(null);
            public MyBehaviorTree BehaviorTree;
            public HashSet<MyBehaviorTreeCollection.BotData> BotsData;

            public BTData(MyBehaviorTree behaviorTree)
            {
                this.BehaviorTree = behaviorTree;
                this.BotsData = new HashSet<MyBehaviorTreeCollection.BotData>(this);
            }

            public bool ContainsBot(IMyBot bot)
            {
                SearchData.Bot = bot;
                return this.BotsData.Contains(SearchData);
            }

            public bool RemoveBot(IMyBot bot)
            {
                SearchData.Bot = bot;
                return this.BotsData.Remove(SearchData);
            }

            bool IEqualityComparer<MyBehaviorTreeCollection.BotData>.Equals(MyBehaviorTreeCollection.BotData x, MyBehaviorTreeCollection.BotData y) => 
                ReferenceEquals(x.Bot, y.Bot);

            int IEqualityComparer<MyBehaviorTreeCollection.BotData>.GetHashCode(MyBehaviorTreeCollection.BotData obj) => 
                obj.Bot.GetHashCode();
        }
    }
}

