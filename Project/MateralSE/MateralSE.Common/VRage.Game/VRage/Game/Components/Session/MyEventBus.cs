namespace VRage.Game.Components.Session
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage.ModAPI;

    public class MyEventBus
    {
        private Dictionary<string, HashSet<IRegisteredInstance>> m_registeredInstances = new Dictionary<string, HashSet<IRegisteredInstance>>();
        private Stopwatch m_stopwatch = new Stopwatch();
        private int m_executionCounter;

        public void OnEntityCreated(IMyEntity entity)
        {
            this.m_stopwatch.Start();
            Type type = entity.GetType();
            foreach (EventInfo info in type.GetEvents())
            {
                HashSet<IRegisteredInstance> set;
                string key = type.Name + "." + info.Name;
                if (!this.m_registeredInstances.TryGetValue(key, out set))
                {
                    set = new HashSet<IRegisteredInstance>();
                    this.m_registeredInstances.Add(key, set);
                }
                ParameterExpression[] expressionArray = (from parameter in info.EventHandlerType.GetMethod("Invoke").GetParameters() select Expression.Parameter(parameter.ParameterType)).ToArray<ParameterExpression>();
                if (expressionArray.Length == 1)
                {
                    Type[] typeArguments = new Type[] { expressionArray[0].Type };
                    Type type2 = typeof(RegisteredInstance).MakeGenericType(typeArguments);
                    object[] args = new object[] { key, entity };
                    object firstArgument = Activator.CreateInstance(type2, args);
                    Delegate handler = Delegate.CreateDelegate(info.EventHandlerType, firstArgument, type2.GetMethod("OnTriggered"));
                    info.AddEventHandler(entity, handler);
                    set.Add((IRegisteredInstance) firstArgument);
                }
            }
            this.m_executionCounter++;
            this.m_stopwatch.Stop();
        }

        public void OnEntityRemove(IMyEntity entity)
        {
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEventBus.<>c <>9 = new MyEventBus.<>c();
            public static Func<ParameterInfo, ParameterExpression> <>9__5_0;

            internal ParameterExpression <OnEntityCreated>b__5_0(ParameterInfo parameter) => 
                Expression.Parameter(parameter.ParameterType);
        }

        protected interface IRegisteredInstance
        {
        }

        protected class RegisteredInstance<T> : MyEventBus.IRegisteredInstance
        {
            public readonly string Name;
            public readonly object Instance;
            public readonly List<T> Data;

            public RegisteredInstance(string name, object instance)
            {
                this.Name = name;
                this.Instance = instance;
                this.Data = new List<T>();
            }

            public override bool Equals(object obj) => 
                this.Instance.Equals(obj);

            public override int GetHashCode() => 
                this.Instance.GetHashCode();

            public void OnTriggered(T data)
            {
                this.Data.Add(data);
            }
        }
    }
}

