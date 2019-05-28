namespace Sandbox.Game.AI
{
    using Sandbox.Game.AI.BehaviorTree;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Utils;

    public class ActionCollection
    {
        private Dictionary<MyStringId, BotActionDesc> m_actions = new Dictionary<MyStringId, BotActionDesc>(MyStringId.Comparer);

        private ActionCollection()
        {
        }

        public void AddAction(string actionName, MethodInfo methodInfo, bool returnsRunning, Func<IMyBot, object[], MyBehaviorTreeState> action)
        {
            this.AddAction(MyStringId.GetOrCompute(actionName), methodInfo, returnsRunning, action);
        }

        public void AddAction(MyStringId actionId, MethodInfo methodInfo, bool returnsRunning, Func<IMyBot, object[], MyBehaviorTreeState> action)
        {
            if (!this.m_actions.ContainsKey(actionId))
            {
                this.AddBotActionDesc(actionId);
            }
            BotActionDesc desc = this.m_actions[actionId];
            ParameterInfo[] parameters = methodInfo.GetParameters();
            desc._Action = action;
            desc.ActionParams = new object[parameters.Length];
            desc.ParametersDesc = new Dictionary<int, MyTuple<Type, MyMemoryParameterType>>();
            desc.ReturnsRunning = returnsRunning;
            for (int i = 0; i < parameters.Length; i++)
            {
                BTMemParamAttribute customAttribute = parameters[i].GetCustomAttribute<BTMemParamAttribute>(true);
                if (customAttribute != null)
                {
                    desc.ParametersDesc.Add(i, new MyTuple<Type, MyMemoryParameterType>(parameters[i].ParameterType.GetElementType(), customAttribute.MemoryType));
                }
            }
        }

        private void AddBotActionDesc(MyStringId actionId)
        {
            this.m_actions.Add(actionId, new BotActionDesc());
        }

        public void AddInitAction(string actionName, Action<IMyBot> action)
        {
            this.AddInitAction(MyStringId.GetOrCompute(actionName), action);
        }

        public void AddInitAction(MyStringId actionName, Action<IMyBot> action)
        {
            if (!this.m_actions.ContainsKey(actionName))
            {
                this.AddBotActionDesc(actionName);
            }
            this.m_actions[actionName].InitAction = action;
        }

        public void AddPostAction(string actionName, Action<IMyBot> action)
        {
            this.AddPostAction(MyStringId.GetOrCompute(actionName), action);
        }

        public void AddPostAction(MyStringId actionId, Action<IMyBot> action)
        {
            if (!this.m_actions.ContainsKey(actionId))
            {
                this.AddBotActionDesc(actionId);
            }
            this.m_actions[actionId].PostAction = action;
        }

        public bool ContainsAction(MyStringId actionId) => 
            (this.m_actions[actionId]._Action != null);

        public bool ContainsActionDesc(MyStringId actionId) => 
            this.m_actions.ContainsKey(actionId);

        public bool ContainsInitAction(MyStringId actionId) => 
            (this.m_actions[actionId].InitAction != null);

        public bool ContainsPostAction(MyStringId actionId) => 
            (this.m_actions[actionId].PostAction != null);

        public static ActionCollection CreateActionCollection(IMyBot bot)
        {
            ActionCollection actions = new ActionCollection();
            foreach (MethodInfo info in bot.BotActions.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ExtractAction(actions, info);
            }
            return actions;
        }

        private static void ExtractAction(ActionCollection actions, MethodInfo methodInfo)
        {
            MyBehaviorTreeActionAttribute customAttribute = methodInfo.GetCustomAttribute<MyBehaviorTreeActionAttribute>();
            if (customAttribute != null)
            {
                switch (customAttribute.ActionType)
                {
                    case MyBehaviorTreeActionType.INIT:
                        actions.AddInitAction(customAttribute.ActionName, x => methodInfo.Invoke(x.BotActions, null));
                        return;

                    case MyBehaviorTreeActionType.BODY:
                        actions.AddAction(customAttribute.ActionName, methodInfo, customAttribute.ReturnsRunning, (x, y) => (MyBehaviorTreeState) methodInfo.Invoke(x.BotActions, y));
                        return;

                    case MyBehaviorTreeActionType.POST:
                        actions.AddPostAction(customAttribute.ActionName, x => methodInfo.Invoke(x.BotActions, null));
                        return;
                }
            }
        }

        private void LoadActionParams(BotActionDesc action, object[] args, MyPerTreeBotMemory botMemory)
        {
            MyBBMemoryValue value2;
            int index = 0;
            goto TR_000F;
        TR_0001:
            index++;
        TR_000F:
            while (true)
            {
                if (index >= args.Length)
                {
                    return;
                }
                object obj2 = args[index];
                if ((obj2 is VRage.Boxed<MyStringId>) && action.ParametersDesc.ContainsKey(index))
                {
                    MyTuple<Type, MyMemoryParameterType> tuple = action.ParametersDesc[index];
                    VRage.Boxed<MyStringId> boxed = obj2 as VRage.Boxed<MyStringId>;
                    value2 = null;
                    if (!botMemory.TryGetFromBlackboard<MyBBMemoryValue>((MyStringId) boxed, out value2))
                    {
                        action.ActionParams[index] = null;
                        goto TR_0001;
                    }
                    else if ((value2 != null) && ((value2.GetType() != tuple.Item1) || (((MyMemoryParameterType) tuple.Item2) == MyMemoryParameterType.OUT)))
                    {
                        bool flag1 = value2.GetType() != tuple.Item1;
                        action.ActionParams[index] = null;
                        goto TR_0001;
                    }
                    break;
                }
                else
                {
                    action.ActionParams[index] = obj2;
                }
                goto TR_0001;
            }
            action.ActionParams[index] = value2;
            goto TR_0001;
        }

        public MyBehaviorTreeState PerformAction(IMyBot bot, MyStringId actionId, object[] args)
        {
            BotActionDesc action = this.m_actions[actionId];
            if (action == null)
            {
                return MyBehaviorTreeState.ERROR;
            }
            MyPerTreeBotMemory currentTreeBotMemory = bot.BotMemory.CurrentTreeBotMemory;
            if (action.ParametersDesc.Count == 0)
            {
                return action._Action(bot, args);
            }
            if (args == null)
            {
                return MyBehaviorTreeState.FAILURE;
            }
            this.LoadActionParams(action, args, currentTreeBotMemory);
            this.SaveActionParams(action, args, currentTreeBotMemory);
            return action._Action(bot, action.ActionParams);
        }

        public void PerformInitAction(IMyBot bot, MyStringId actionId)
        {
            BotActionDesc desc = this.m_actions[actionId];
            if (desc != null)
            {
                desc.InitAction(bot);
            }
        }

        public void PerformPostAction(IMyBot bot, MyStringId actionId)
        {
            BotActionDesc desc = this.m_actions[actionId];
            if (desc != null)
            {
                desc.PostAction(bot);
            }
        }

        public bool ReturnsRunning(MyStringId actionId) => 
            this.m_actions[actionId].ReturnsRunning;

        private void SaveActionParams(BotActionDesc action, object[] args, MyPerTreeBotMemory botMemory)
        {
            foreach (int num in action.ParametersDesc.Keys)
            {
                MyStringId id = (MyStringId) (args[num] as VRage.Boxed<MyStringId>);
                if (((MyMemoryParameterType) action.ParametersDesc[num].Item2) != MyMemoryParameterType.IN)
                {
                    botMemory.SaveToBlackboard(id, action.ActionParams[num] as MyBBMemoryValue);
                }
            }
        }

        public class BotActionDesc
        {
            public Action<IMyBot> InitAction;
            public object[] ActionParams;
            public Dictionary<int, MyTuple<Type, MyMemoryParameterType>> ParametersDesc;
            public Func<IMyBot, object[], MyBehaviorTreeState> _Action;
            public Action<IMyBot> PostAction;
            public bool ReturnsRunning;
        }
    }
}

