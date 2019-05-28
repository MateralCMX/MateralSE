namespace Sandbox.Game.AI
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.AI.Logic;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRageMath;

    public abstract class MyBotFactoryBase
    {
        protected Dictionary<string, Type> m_TargetTypeByName = new Dictionary<string, Type>();
        protected Dictionary<string, BehaviorData> m_botDataByBehaviorType = new Dictionary<string, BehaviorData>();
        protected Dictionary<string, LogicData> m_logicDataByBehaviorSubtype = new Dictionary<string, LogicData>();
        protected Dictionary<Type, BehaviorTypeData> m_botTypeByDefinitionTypeRemoveThis;
        private Type[] m_tmpTypeArray = new Type[1];
        private object[] m_tmpConstructorParamArray = new object[1];
        private static MyObjectFactory<MyBotTypeAttribute, IMyBot> m_objectFactory = new MyObjectFactory<MyBotTypeAttribute, IMyBot>();

        static MyBotFactoryBase()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(MyAgentBot));
            m_objectFactory.RegisterFromAssembly(assembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            foreach (IPlugin plugin in MyPlugins.Plugins)
            {
                m_objectFactory.RegisterFromAssembly(plugin.GetType().Assembly);
            }
        }

        public MyBotFactoryBase()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(MyAgentBot));
            this.LoadBotData(assembly);
            this.LoadBotData(MyPlugins.GameAssembly);
            foreach (IPlugin plugin in MyPlugins.Plugins)
            {
                this.LoadBotData(plugin.GetType().Assembly);
            }
        }

        public abstract bool CanCreateBotOfType(string behaviorType, bool load);
        private void CreateActions(IMyBot bot, Type actionImplType)
        {
            this.m_tmpTypeArray[0] = bot.GetType();
            if (actionImplType.GetConstructor(this.m_tmpTypeArray) == null)
            {
                bot.BotActions = Activator.CreateInstance(actionImplType) as MyBotActionsBase;
            }
            else
            {
                object[] args = new object[] { bot };
                bot.BotActions = Activator.CreateInstance(actionImplType, args) as MyBotActionsBase;
            }
            this.m_tmpTypeArray[0] = null;
        }

        public IMyBot CreateBot(MyPlayer player, MyObjectBuilder_Bot botBuilder, MyBotDefinition botDefinition)
        {
            MyObjectBuilderType invalid = MyObjectBuilderType.Invalid;
            if (botBuilder != null)
            {
                invalid = botBuilder.TypeId;
            }
            else
            {
                invalid = botDefinition.Id.TypeId;
                botBuilder = m_objectFactory.CreateObjectBuilder<MyObjectBuilder_Bot>(m_objectFactory.GetProducedType(invalid));
            }
            if (!this.m_botDataByBehaviorType.ContainsKey(botDefinition.BehaviorType))
            {
                return null;
            }
            BehaviorData data = this.m_botDataByBehaviorType[botDefinition.BehaviorType];
            IMyBot bot = this.CreateBot(m_objectFactory.GetProducedType(invalid), player, botDefinition);
            this.CreateActions(bot, data.BotActionsType);
            this.CreateLogic(bot, data.LogicType, botDefinition.BehaviorSubtype);
            bot.Init(botBuilder);
            return bot;
        }

        private IMyBot CreateBot(Type botType, MyPlayer player, MyBotDefinition botDefinition)
        {
            object[] args = new object[] { player, botDefinition };
            return (Activator.CreateInstance(botType, args) as IMyBot);
        }

        private void CreateLogic(IMyBot output, Type defaultLogicType, string definitionLogicType)
        {
            Type logicType = null;
            if (!this.m_logicDataByBehaviorSubtype.ContainsKey(definitionLogicType))
            {
                logicType = defaultLogicType;
            }
            else
            {
                logicType = this.m_logicDataByBehaviorSubtype[definitionLogicType].LogicType;
                if (!logicType.IsSubclassOf(defaultLogicType) && (logicType != defaultLogicType))
                {
                    logicType = defaultLogicType;
                }
            }
            object[] args = new object[] { output };
            MyBotLogic logic = Activator.CreateInstance(logicType, args) as MyBotLogic;
            output.InitLogic(logic);
        }

        public MyAiTargetBase CreateTargetForBot(MyAgentBot bot)
        {
            MyAiTargetBase base2 = null;
            this.m_tmpConstructorParamArray[0] = bot;
            Type type = null;
            this.m_TargetTypeByName.TryGetValue(bot.AgentDefinition.TargetType, out type);
            if (type != null)
            {
                base2 = Activator.CreateInstance(type, this.m_tmpConstructorParamArray) as MyAiTargetBase;
            }
            this.m_tmpConstructorParamArray[0] = null;
            return base2;
        }

        public abstract bool GetBotGroupSpawnPositions(string behaviorType, int count, List<Vector3D> spawnPositions);
        public MyObjectBuilder_Bot GetBotObjectBuilder(IMyBot myAgentBot) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_Bot>(myAgentBot);

        public abstract bool GetBotSpawnPosition(string behaviorType, out Vector3D spawnPosition);
        protected void LoadBotData(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                object[] customAttributes;
                int num2;
                if (type.IsAbstract || !type.IsSubclassOf(typeof(MyBotActionsBase)))
                {
                    if (!type.IsAbstract && type.IsSubclassOf(typeof(MyBotLogic)))
                    {
                        customAttributes = type.GetCustomAttributes(typeof(BehaviorLogicAttribute), true);
                        num2 = 0;
                        while (num2 < customAttributes.Length)
                        {
                            BehaviorLogicAttribute attribute2 = customAttributes[num2] as BehaviorLogicAttribute;
                            this.m_logicDataByBehaviorSubtype[attribute2.BehaviorSubtype] = new LogicData(type);
                            num2++;
                        }
                    }
                    else if (!type.IsAbstract && typeof(MyAiTargetBase).IsAssignableFrom(type))
                    {
                        customAttributes = type.GetCustomAttributes(typeof(TargetTypeAttribute), true);
                        num2 = 0;
                        while (num2 < customAttributes.Length)
                        {
                            TargetTypeAttribute attribute3 = customAttributes[num2] as TargetTypeAttribute;
                            this.m_TargetTypeByName[attribute3.TargetType] = type;
                            num2++;
                        }
                    }
                }
                else
                {
                    string descriptorCategory = "";
                    BehaviorData data = new BehaviorData(type);
                    customAttributes = type.GetCustomAttributes(true);
                    num2 = 0;
                    while (true)
                    {
                        if (num2 >= customAttributes.Length)
                        {
                            if (!string.IsNullOrEmpty(descriptorCategory) && (data.LogicType != null))
                            {
                                this.m_botDataByBehaviorType[descriptorCategory] = data;
                            }
                            break;
                        }
                        object obj2 = customAttributes[num2];
                        if (obj2 is MyBehaviorDescriptorAttribute)
                        {
                            descriptorCategory = (obj2 as MyBehaviorDescriptorAttribute).DescriptorCategory;
                        }
                        else if (obj2 is BehaviorActionImplAttribute)
                        {
                            data.LogicType = (obj2 as BehaviorActionImplAttribute).LogicType;
                        }
                        num2++;
                    }
                }
            }
        }

        public abstract int MaximumUncontrolledBotCount { get; }

        public abstract int MaximumBotPerPlayer { get; }

        protected class BehaviorData
        {
            public readonly Type BotActionsType;
            public Type LogicType;

            public BehaviorData(Type t)
            {
                this.BotActionsType = t;
            }
        }

        protected class BehaviorTypeData
        {
            public Type BotType;

            public BehaviorTypeData(Type botType)
            {
                this.BotType = botType;
            }
        }

        protected class LogicData
        {
            public readonly Type LogicType;

            public LogicData(Type t)
            {
                this.LogicType = t;
            }
        }
    }
}

